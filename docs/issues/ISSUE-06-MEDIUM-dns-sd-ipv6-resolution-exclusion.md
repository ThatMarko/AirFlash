# [MEDIUM] DNS-SD Resolution and RTSP Socket Binding Excludes Pure IPv6 Endpoints

- **Issue ID**: ISSUE-06
- **Severity**: **MEDIUM**
- **Subsystem**: Network Discovery & RTSP Transport
- **Status**: Open / Triaged
- **Target Files**:
  - [`desktop/AirFlash.App/Services/WindowsDiscovery.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L140-L150)
  - [`desktop/AirFlash.Core/SessionController.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/SessionController.cs#L450-L460)

---

## 1. Summary
The AirFlash discovery pipeline ([`WindowsDiscovery.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/WindowsDiscovery.cs)) and session resolution logic ([`SessionController.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/SessionController.cs)) strictly restrict IP address handling to IPv4 (`AddressFamily.InterNetwork`). 

On modern home networks configured for pure IPv6 or where HomePod devices advertise over link-local IPv6 addresses (`fe80::...`), the receiver cannot be resolved, throwing `IOException("Cannot resolve IPv4 address")` and discarding the DNS-SD resolution record.

---

## 2. Technical Root Cause Analysis
In [`desktop/AirFlash.App/Services/WindowsDiscovery.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L140-L144):

```csharp
var info = Marshal.PtrToStructure<ServiceInstance>(result);
if (info.Ipv4 == IntPtr.Zero || info.Port == 0 || info.Count > 256) return;
var address = new byte[4]; Marshal.Copy(info.Ipv4, address, 0, 4);
```
`info.Ipv6` is completely ignored; any service instance advertising only via IPv6 is discarded.

In [`desktop/AirFlash.Core/SessionController.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/SessionController.cs#L450-L456):

```csharp
private static async Task<string> ResolveAsync(string host, CancellationToken cancellation)
{
    if (IPAddress.TryParse(host, out var literal) && literal.AddressFamily == AddressFamily.InterNetwork) return host;
    var addresses = await Dns.GetHostAddressesAsync(host, cancellation).ConfigureAwait(false);
    return addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)?.ToString() 
        ?? throw new IOException(L.Format("Cannot resolve IPv4 address: {0}", host));
}
```
Only `AddressFamily.InterNetwork` is accepted.

---

## 3. Reproduction Steps
1. On an IPv6-only network segment or when a receiver is reachable solely via link-local IPv6:
2. Start AirFlash discovery.
3. **Observed Result**: The receiver is never discovered or fails with `"Cannot resolve IPv4 address: <host>"`.
4. **Expected Result**: AirFlash discovers dual-stack IPv4/IPv6 endpoints and allows streaming over IPv6 if IPv4 is unavailable.

---

## 4. Proposed Solution
1. In `WindowsDiscovery.cs`, read `info.Ipv6` if `info.Ipv4 == IntPtr.Zero`.
2. In `SessionController.cs`, support both `AddressFamily.InterNetwork` and `AddressFamily.InterNetworkV6`.
3. In `airflash-engine/src/session.rs`, ensure RTSP/RTP sockets bind with dual-stack support or handle `IpAddr::V6`.
