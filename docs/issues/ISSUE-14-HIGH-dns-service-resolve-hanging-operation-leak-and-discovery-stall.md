# [HIGH] DnsServiceResolve Async Callback Loss Causes Unmanaged Memory Leak and Permanent Receiver Discovery Stall

- **Issue ID**: ISSUE-14
- **Severity**: **HIGH**
- **Subsystem**: Native mDNS Discovery & Win32 Interop (`AirFlash.App/Services/WindowsDiscovery.cs`)
- **Status**: Open / Triaged
- **Target Files**:
  - [`desktop/AirFlash.App/Services/WindowsDiscovery.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L85-L135)
  - [`desktop/AirFlash.App/Services/WindowsDiscovery.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L205-L235)

---

## 1. Summary
In [`WindowsDiscovery.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/WindowsDiscovery.cs), each DNS-SD service resolution initiates a native Win32 `DnsServiceResolve` asynchronous operation through a managed `Operation` wrapper. Each operation allocates unmanaged native memory (`Marshal.StringToHGlobalUni` and `Marshal.AllocHGlobal`) and registers its token in `_pending[instance]` to prevent duplicate simultaneous queries.

However, if an mDNS resolution query is dropped by a Wi-Fi access point, IGMP snooping router, or times out inside Windows `dnsapi.dll` without triggering the completion thunk (`ResolveThunk`):
1. The unmanaged memory for the operation is **never freed**, resulting in a cumulative native memory leak in `Operation.All`.
2. The key remains in `_pending[instance]` permanently because the 20-second `Refresh()` cycle prunes `_records` and `_instances` upon TTL expiration, but **fails to remove keys from `_pending`**.
3. Any subsequent attempts to resolve that receiver (whether by timer `Refresh()` or new PTR announcements in `Browse()`) are permanently blocked by `if (_pending.ContainsKey(instance)) return;`. The device becomes a "ghost" that can never be discovered again until the user manually restarts AirFlash or changes network adapters.

---

## 2. Technical Root Cause Analysis

### 2.1 The Missing Prune in `Refresh()`
In [`WindowsDiscovery.cs:L85-L95`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L85-L95):
```csharp
KeyValuePair<string, string>[] instances;
lock (_sync)
{
    if (_disposed) return;
    foreach (var key in _records.Where(p => p.Value.Expires <= DateTime.UtcNow).Select(p => p.Key).ToArray())
    {
        _records.Remove(key);
        _instances.Remove(key);
        // CRITICAL BUG: _pending.Remove(key) is MISSING!
    }
    instances = _instances.ToArray();
}
Publish();
foreach (var item in instances) Resolve(item.Key, item.Value);
```

### 2.2 The Permanent Lockout in `Resolve()`
In [`WindowsDiscovery.cs:L115-L125`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L115-L125):
```csharp
private void Resolve(string instance, string type)
{
    lock (_sync)
    {
        if (_disposed || _pending.ContainsKey(instance)) return; // BLOCKED FOREVER
        var operation = new Operation(this, instance, type, _epoch, true, _interfaceIndex!.Value);
        _pending[instance] = operation.Token;
        ...
```
When `_pending[instance]` is left orphaned:
- Every periodic call to `Resolve(instance)` immediately returns.
- Even if the HomePod multicasts an unsolicited PTR response in `Browse()`:
  ```csharp
  _instances[instance] = operation.Type;
  Resolve(instance, operation.Type);
  ```
  `Resolve()` checks `_pending.ContainsKey(instance)` and aborts.

### 2.3 Unmanaged Handle Leak in `Operation`
In [`WindowsDiscovery.cs:L210-L235`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L210-L235):
```csharp
public Operation(...)
{
    ...
    _name = Marshal.StringToHGlobalUni(name);
    _cancel = Marshal.AllocHGlobal(IntPtr.Size);
    Marshal.WriteIntPtr(_cancel, IntPtr.Zero);
    All[Token] = this; // RETAINED IN STATIC DICTIONARY
}
public void Complete() { _pending = false; Dispose(); }
public void Dispose()
{
    if (!All.TryRemove(Token, out _)) return;
    if (_pending) { if (_resolve) DnsServiceResolveCancel(_cancel); ... }
    Marshal.FreeHGlobal(_name);
    Marshal.FreeHGlobal(_cancel);
    _name = _cancel = IntPtr.Zero;
}
```
`Complete()` is only called inside `Resolved()`:
```csharp
finally { if (result != IntPtr.Zero) DnsServiceFreeInstance(result); operation.Complete(); }
```
If `dnsapi.dll` silently fails to invoke the callback, `Complete()` and `Dispose()` are never called. The object remains anchored in `Operation.All`, leaking the unmanaged string and cancel pointers.

---

## 3. Reproduction Steps
1. Launch AirFlash on a Wi-Fi network with high packet loss or simulate packet drop on UDP port 5353.
2. Allow `WindowsDiscovery` to discover a HomePod and initiate `Resolve`.
3. Simulate an unresolved query by dropping the inbound mDNS SRV/TXT response.
4. Wait 90 seconds for the record TTL to expire.
5. **Observed Result**: The record is evicted from `_records` and `_instances`. However, `_pending` still holds the instance token. The receiver never re-appears in the UI, even when subsequent valid mDNS packets arrive.
6. **Expected Result**: Unresolved queries time out cleanly, native resources are freed, and future resolve attempts succeed.

---

## 4. Proposed Solution
1. **Prune `_pending` on Expiration**:
   In `Refresh()`, explicitly remove expired keys from `_pending`:
   ```csharp
   foreach (var key in _records.Where(p => p.Value.Expires <= DateTime.UtcNow).Select(p => p.Key).ToArray())
   {
       _records.Remove(key);
       _instances.Remove(key);
       _pending.Remove(key);
   }
   ```
2. **Add a 10-Second Watchdog Timer on `Operation`**:
   Attach a cancellation timeout to each `Operation`. If `ResolveThunk` does not fire within 10 seconds:
   - Call `operation.Dispose()`.
   - Remove `instance` from `_pending`.
   - Log an advisory diagnostic message.
