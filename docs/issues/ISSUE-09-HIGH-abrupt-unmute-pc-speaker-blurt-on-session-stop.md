# [HIGH] Abrupt Unmute and PC Speaker Blurt on Discovery Drop or Session Termination

- **Issue ID**: ISSUE-09
- **Severity**: **HIGH**
- **Subsystem**: Audio Session & Desktop Mute Coordination (`AirFlash.App` & `AirFlash.Core`)
- **Status**: Open / Triaged
- **Target Files**:
  - [`desktop/AirFlash.Core/SessionController.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/SessionController.cs#L209-L215)
  - [`desktop/AirFlash.Core/SessionController.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/SessionController.cs#L345-L350)
  - [`desktop/AirFlash.App/Services/AudioService.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/AudioService.cs#L73-L85)
  - [`docs/analysis/control-and-mute.md`](../analysis/control-and-mute.md)

---

## 1. Summary
When `MuteWhileStreaming` is enabled (the default configuration), AirFlash mutes the local Windows output endpoint so audio only emerges from the connected AirPlay 2 receiver (e.g., HomePod mini). 

However, whenever an active stream is stopped—most notably during transient mDNS discovery drops (see [ISSUE-01](ISSUE-01-CRITICAL-discovery-disconnects-active-playback.md))—[`SessionController.StopAsync()`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/SessionController.cs#L209) immediately runs `finally { await RestoreAudioAsync(); }`. This causes the physical PC speakers to **instantly unmute and blurt system audio** into the room at full volume for 10–30 seconds until the receiver is re-discovered and auto-reconnected, at which point the speakers abruptly cut off again.

---

## 2. Technical Root Cause Analysis

### 2.1 The Mute / Restore Execution Chain
In [`desktop/AirFlash.Core/SessionController.cs:L345`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/SessionController.cs#L345):
```csharp
if (kind == "streaming")
{
    startedAt = Stopwatch.GetTimestamp();
    if (_settings.MuteWhileStreaming) await audio.MuteAsync(_settings.EffectiveEndpoint).ConfigureAwait(false);
    Publish(epoch, PlaybackState.Streaming);
}
```
When discovery flushes or a network adapter event causes `AppViewModel` to call `Session.StopAsync()`, the teardown logic executes in [`SessionController.cs:L209`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/SessionController.cs#L209):
```csharp
finally
{
    await RestoreAudioAsync().ConfigureAwait(false);
}
```
Which calls [`AudioService.Restore()`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/AudioService.cs#L74-L83):
```csharp
private void Restore()
{
    if (_originalMute.Count == 0) return;
    using var enumerator = new MMDeviceEnumerator();
    foreach (var (id, previous) in _originalMute.ToArray())
        try { using var device = enumerator.GetDevice(id); device.AudioEndpointVolume.Mute = previous; _originalMute.Remove(id); }
        catch (System.Runtime.InteropServices.COMException) { }
}
```

### 2.2 Why this is a Critical UX Defect
1. **No Grace Period for Reconnection**: The restore logic assumes every stop is an intentional user action. It makes no distinction between an explicit user pause/disconnect and a transient disconnect where auto-connect or reconnection is actively armed.
2. **Audio Whiplash**: During the 10–30 second discovery blackout, media continues playing through the host PC's internal speakers. As soon as discovery recovers, auto-connect mutes the PC speakers again, resulting in an abrasive "speaker blast -> mute -> resume on HomePod" cycle.

---

## 3. Reproduction Steps
1. In AirFlash Settings, ensure `Mute local speakers while streaming` is enabled (`true`).
2. Connect to an Apple HomePod and begin playing music. The PC speakers are muted as expected.
3. Trigger an mDNS cache flush (e.g. by enabling/disabling a virtual adapter or running `ipconfig /registerdns`).
4. **Observed Result**: HomePod goes silent. PC speakers instantly unmute, blasting music aloud. 15 seconds later, HomePod auto-reconnects, and PC speakers are abruptly muted again.
5. **Expected Result**: If auto-connect or retry is armed, local audio muting should be held for a grace period (e.g. 15–20 seconds) to prevent jarring acoustic spikes.

---

## 4. Proposed Solution
1. **Introduce a Reconnect Mute Grace Period**:
   In `SessionController.cs`, before invoking `RestoreAudioAsync()`, inspect whether `_settings.ForceReconnect` or `AutoConnect` is active and whether the stop was caused by a transient failure rather than an explicit user `Stop`.
2. **Delayed Restoration**:
   If an automatic reconnect attempt is scheduled, defer `RestoreAudioAsync()` by 15 seconds using a cancellable CTS (`_restoreDelayCts`). If reconnect succeeds within the window, cancel the CTS and keep the mute active.
