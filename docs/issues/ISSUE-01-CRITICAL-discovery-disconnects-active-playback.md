# [CRITICAL] Active Playback Prematurely Terminated and UI Evicted by Transient mDNS Discovery Snapshots

- **Issue ID**: ISSUE-01
- **Severity**: **CRITICAL**
- **Subsystem**: Desktop Frontend Orchestration (`AirFlash.App`)
- **Status**: Open / Triaged
- **Target Files**:
  - [`desktop/AirFlash.App/ViewModels/AppViewModel.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L211-L245)
  - [`desktop/AirFlash.App/Services/WindowsDiscovery.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L45-L65)
  - [`docs/analysis/playback-coupling.md`](../analysis/playback-coupling.md)

---

## 1. Summary
When streaming audio to an AirPlay 2 receiver (e.g., Apple HomePod or stereo pair), any transient loss or omission of the device in a Windows DNS-SD (`WindowsDiscovery`) mDNS snapshot causes [`AppViewModel`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/ViewModels/AppViewModel.cs) to actively and intentionally terminate the running audio stream (`await Session.StopAsync()`) and immediately evict the receiver from the flyout UI.

Because mDNS discovery is inherently lossy over Wi-Fi and routinely resets during network route refreshes (every 20 seconds) or adapter address updates (such as Tailscale or virtual adapter DHCP renewals), users experience sudden, unexpected playback stoppages where audio halts cleanly, the device disappears from the UI for 10–30 seconds, and then reappears.

---

## 2. Technical Root Cause Analysis

### 2.1 The Premature Session Teardown
In [`desktop/AirFlash.App/ViewModels/AppViewModel.cs:L211-L221`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L211-L221):

```csharp
if (currentSnapshot.Receiver is { IsManual: false } current)
{
    var id = ReceiverMigrations.GetValueOrDefault(current.Id, current.Id);
    if (!discovered.TryGetValue(id, out var next) || !next.Complete)
    {
        if (currentSnapshot.IsActive) await Session.StopAsync(); // <-- ACTIVE TERMINATION
    }
    else if (!ReceiverEqual(current, next) || !SettingsMerge.Equal(originalSettings, _settings)) 
        await Session.UpdateReceiverAsync(next, _settings.Clone());
}
```

Notice that manual receivers (`{ IsManual: false }`) are explicitly exempted from this teardown logic, confirming that the author knew that receivers not present in mDNS should not necessarily be killed, but failed to extend the same protection to auto-discovered receivers during streaming.

### 2.2 Instant UI Eviction and Memory Cache Erasure
In [`desktop/AirFlash.App/ViewModels/AppViewModel.cs:L235-L245`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L235-L245):

```csharp
foreach (var row in _receivers.ToArray())
{
    if (row.IsManual || active.Contains(row.Id)) continue;
    _receivers.Remove(row);   // Evicted from UI immediately
    _known.Remove(row.Id);    // Evicted from memory cache
}
```

When discovery resets and publishes `[]`:
1. The active session is intentionally killed by calling `Session.StopAsync()`.
2. Even if playback weren't killed, the receiver row is instantly removed from the UI collection, causing the flyout to display "No receivers found" and flickering the tray status.

### 2.3 Why this is a critical flaw:
1. **Coupling Control-Plane Discovery to Data-Plane Streaming**: Once an RTSP connection is negotiated and RTP audio is flowing over UDP to the receiver's IP:Port, continuous mDNS visibility is **not** required for playback.
2. **Periodic Discovery Flush**: [`WindowsDiscovery.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L45-L65) responds to network address changes and timer expirations by purging `_records` and publishing an empty list before restarting the browse operations.
3. **No Grace Period or Transport Watchdog**: The decision to terminate playback is made instantly on a single discovery event rather than querying the live RTCP transport watchdog or allowing a grace period (e.g., 30–60 seconds of missed discovery without transport failure).

---

## 3. Reproduction Steps
1. Connect AirFlash to an Apple HomePod or HomePod mini.
2. Begin audio streaming from Windows.
3. Simulate a Wi-Fi roaming event, an IP address change on a secondary virtual adapter, or wait for the 20s discovery refresh under lossy Wi-Fi conditions.
4. **Observed Result**: Audio stops immediately. The HomePod chimes/pauses. The tray UI flips back to stopped state. Diagnostics show 0 reconnects because the disconnect was requested by the host application.
5. **Expected Result**: Audio streaming continues uninterrupted as long as RTP/RTCP packets are acknowledged by the HomePod.

---

## 4. Proposed Solution
1. **Decouple Playback from Discovery Snapshot**:
   - In `AppViewModel.cs`, remove `await Session.StopAsync()` triggered solely by discovery absence.
   - Let [`SessionController.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/SessionController.cs) and the native engine's RTCP feedback loop determine whether the receiver is unreachable.
2. **Implement Graceful Expiry**:
   - If a receiver is missing from discovery, mark `Online = false` in the catalog after a reasonable debounce period (e.g. 45 seconds), but keep active streaming alive.
   - Only terminate or reconnect if the native engine reports a transport timeout (`transport_metrics` watchdog failure or RTSP teardown).
3. **Protect UI State for the Active Streaming Receiver**:
   - In `RefreshReceivers()`, never remove the currently active receiver row (`row.Id == currentSnapshot.Receiver?.Id`), preserving user controls (volume, disconnect) during transient mDNS packet loss.
