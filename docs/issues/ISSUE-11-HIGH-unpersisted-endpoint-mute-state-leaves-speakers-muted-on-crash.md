# [HIGH] Unpersisted Endpoint Mute State Leaves Host Speakers Permanently Muted on Crash

- **Issue ID**: ISSUE-11
- **Severity**: **HIGH**
- **Subsystem**: Audio Endpoint Management (`AirFlash.App`)
- **Status**: Open / Triaged
- **Target Files**:
  - [`desktop/AirFlash.App/Services/AudioService.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/AudioService.cs#L60-L85)
  - [`desktop/AirFlash.App/App.xaml.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/App.xaml.cs)

---

## 1. Summary
When `MuteWhileStreaming` is active, AirFlash mutes the host PC's primary audio output device via Windows CoreAudio MMDevice APIs and records the prior mute status in an in-memory dictionary (`_originalMute`).

If AirFlash terminates abnormally—due to an unhandled exception, process termination via Task Manager, an unexpected OS shutdown, or a blue screen/reboot—the in-memory dictionary is destroyed. Consequently, the host PC's physical audio output remains **permanently muted in Windows**, leaving the user with a completely silent PC until they manually open Windows Sound Settings to un-mute their audio hardware.

---

## 2. Technical Root Cause Analysis
In [`desktop/AirFlash.App/Services/AudioService.cs:L64-L85`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/AudioService.cs#L64-L85):

```csharp
private readonly Dictionary<string, bool> _originalMute = [];
...
public Task MuteAsync(string? endpointId) => RunAsync(() =>
{
    using var enumerator = new MMDeviceEnumerator();
    using var device = endpointId is null ? enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console) : enumerator.GetDevice(endpointId);
    if (_originalMute.ContainsKey(device.ID)) return true;
    var previous = device.AudioEndpointVolume.Mute;
    device.AudioEndpointVolume.Mute = true;
    _originalMute[device.ID] = previous; // <-- VOLATILE MEMORY ONLY
    return true;
});
```

### The Failure Scenarios:
1. **Volatile In-Memory Storage**: `_originalMute` is never persisted to disk (e.g. `%APPDATA%\AirFlash\muted_endpoints.json` or local settings).
2. **No Startup Recovery**: When AirFlash boots, `AudioService` does not check whether a previous instance terminated unexpectedly while leaving devices muted.
3. **No Process Exit Hook**: In `App.xaml.cs`, there is no `AppDomain.CurrentDomain.ProcessExit` or `AppDomain.CurrentDomain.UnhandledException` handler ensuring that `AudioService.RestoreAsync()` is called before process exit.

---

## 3. Reproduction Steps
1. Start streaming audio with AirFlash to an AirPlay receiver with `MuteWhileStreaming` enabled. Verify the PC speaker is muted.
2. Open Windows Task Manager and forcefully end `AirFlash.App.exe` (or simulate a system crash).
3. **Observed Result**: The PC audio device remains muted in Windows Volume Mixer. Other Windows applications produce no sound.
4. **Expected Result**: Either the mute is safely cleared on process exit, or AirFlash persists the mute state to disk and restores it on next launch.

---

## 4. Proposed Solution
1. **Persist Active Mutes to Disk**:
   Serialize `_originalMute` to an atomic recovery file (`%APPDATA%\AirFlash\active_mutes.json`) upon muting, and delete the file upon clean restoration.
2. **On-Startup Crash Recovery**:
   During `AudioService` initialization, check if `active_mutes.json` exists. If present, immediately un-mute the referenced endpoints and remove the recovery file.
3. **Attach ProcessExit / UnhandledException Cleanup**:
   In `App.xaml.cs`, register a synchronous fallback handler on `AppDomain.CurrentDomain.ProcessExit` that invokes synchronous un-muting on any currently muted endpoints.
