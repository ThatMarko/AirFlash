# AirFlash Codebase Issue Tracker & Severity Matrix

This directory contains formal, tracked issues identified during the comprehensive deep recursive scan of the AirFlash codebase across Python, C#, Rust, PowerShell, XAML, and WiX installer pipelines.

To submit a new issue report formatted for submission to the repository maintainers, use the standard [**`ISSUE_TEMPLATE.md`**](ISSUE_TEMPLATE.md).

---

## 📊 Severity Matrix

| Issue ID | Severity | Title | Affected Subsystem | Primary Target Files |
| :--- | :--- | :--- | :--- | :--- |
| [**ISSUE-01**](ISSUE-01-CRITICAL-discovery-disconnects-active-playback.md) | **CRITICAL** | Active Playback Prematurely Terminated by Transient mDNS Discovery Snapshots | Desktop App (`AirFlash.App`) | [`AppViewModel.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L211) |
| [**ISSUE-02**](ISSUE-02-HIGH-test-suite-skips-under-windows-powershell.md) | **HIGH** | Automated Release Verification Test Suite Skips Under Standard Windows PowerShell | Testing / CI (`tests/`) | [`test_release_version.py`](file:///C:/Users/marko/AirFlash/tests/test_release_version.py#L11) |
| [**ISSUE-03**](ISSUE-03-HIGH-wasapi-underrun-resync-strategy.md) | **HIGH** | WASAPI Loopback Audio Capture Lacks Adaptive Resync and User Feedback on System Latency Spikes | Native Engine (`airflash-engine`) | [`live.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/live.rs#L125), [`transport.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/transport.rs#L398) |
| [**ISSUE-04**](ISSUE-04-MEDIUM-unbound-copy-diagnostics-command.md) | **MEDIUM** | Unbound `CopyDiagnosticsCommand` in Settings UI | Desktop UI (`AirFlash.App`) | [`SettingsViewModel.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/ViewModels/SettingsViewModel.cs#L62), [`SettingsWindow.xaml`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Ui/SettingsWindow.xaml) |
| [**ISSUE-05**](ISSUE-05-MEDIUM-build-pipeline-missing-dotnet-sdk-check.md) | **MEDIUM** | Missing Pre-Flight Check for .NET 10 SDK in Build Pipeline | Developer Experience (`scripts/`) | [`dotnet.ps1`](file:///C:/Users/marko/AirFlash/scripts/dotnet.ps1#L1) |
| [**ISSUE-06**](ISSUE-06-MEDIUM-dns-sd-ipv6-resolution-exclusion.md) | **MEDIUM** | DNS-SD Resolution and RTSP Socket Binding Excludes Pure IPv6 Endpoints | Network Discovery & Session | [`WindowsDiscovery.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L140), [`SessionController.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/SessionController.cs#L450) |
| [**ISSUE-07**](ISSUE-07-LOW-orphaned-translation-keys-cleanup.md) | **LOW** | Prune Superseded and Orphaned Translation Keys in `Strings.zh.json` | Localization (`AirFlash.Core`) | [`Strings.zh.json`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/Strings.zh.json) |
| [**ISSUE-08**](ISSUE-08-LOW-ptp-timing-port-collision-fallback.md) | **LOW** | Hardcoded RTSP Port 7000 and Timing UDP Socket Collision Handling | Native Engine (`airflash-engine`) | [`session.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/session.rs#L30), [`clock.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/clock.rs#L95) |
| [**ISSUE-09**](ISSUE-09-HIGH-abrupt-unmute-pc-speaker-blurt-on-session-stop.md) | **HIGH** | Abrupt Unmute and PC Speaker Blurt on Discovery Drop or Session Termination | Audio Session (`AirFlash.Core` & `AirFlash.App`) | [`SessionController.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/SessionController.cs#L209), [`AudioService.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/AudioService.cs#L73) |
| [**ISSUE-10**](ISSUE-10-HIGH-wlan-background-scan-latency-spikes-starve-realtime-buffer.md) | **HIGH** | WLAN Background Scan Latency Spikes Starve Realtime Audio Jitter Buffer | Wi-Fi Transport & Engine | [`session.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/session.rs#L520), [`Settings.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/Settings.cs#L45) |
| [**ISSUE-11**](ISSUE-11-HIGH-unpersisted-endpoint-mute-state-leaves-speakers-muted-on-crash.md) | **HIGH** | Unpersisted Endpoint Mute State Leaves Host Speakers Permanently Muted on Crash | Audio Endpoint (`AirFlash.App`) | [`AudioService.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/AudioService.cs#L60), [`App.xaml.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/App.xaml.cs) |
| [**ISSUE-12**](ISSUE-12-MEDIUM-rtsp-connection-mutex-contention-high-frequency-volume-polling.md) | **MEDIUM** | RTSP Connection Mutex Contention from High-Frequency Volume Polling | RTSP Transport (`airflash-engine`) | [`volume.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/volume.rs#L125), [`transport.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/transport.rs#L285) |
| [**ISSUE-13**](ISSUE-13-MEDIUM-volume-curve-quantization-mismatch-stereo-unconfirmed.md) | **MEDIUM** | HomePod Non-Linear Volume Curve Quantization Mismatch Causes False "Unconfirmed" Status | Volume Control Subsystem | [`volume.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/volume.rs#L65), [`DeviceVolumeState.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/DeviceVolumeState.cs#L15) |
| [**ISSUE-14**](ISSUE-14-HIGH-dns-service-resolve-hanging-operation-leak-and-discovery-stall.md) | **HIGH** | DnsServiceResolve Async Callback Loss Causes Unmanaged Memory Leak and Permanent Discovery Stall | Native Discovery (`AirFlash.App`) | [`WindowsDiscovery.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L85) |

---

## 🔗 Mapping to Upstream GitHub Issues #6 and #7

The table below details how our discovered issues directly explain, cause, or exacerbate the known upstream GitHub issues:

### 1. GitHub Issue #6: *"AirFlash stops streaming / disconnects unexpectedly when devices refresh, roams, or drops off discovery"*

| Related Issue | Role in Issue #6 | Mechanism |
| :--- | :--- | :--- |
| [**ISSUE-01**](ISSUE-01-CRITICAL-discovery-disconnects-active-playback.md) | **Direct Root Cause** | [`AppViewModel.cs:L215`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L215) intentionally calls `Session.StopAsync()` whenever the active receiver is omitted from an mDNS snapshot during standard 20s route refreshes or IP address changes. |
| [**ISSUE-14**](ISSUE-14-HIGH-dns-service-resolve-hanging-operation-leak-and-discovery-stall.md) | **Discovery Stall & Ghosting** | In [`WindowsDiscovery.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Services/WindowsDiscovery.cs), dropped mDNS resolve packets leave orphaned tokens in `_pending`. Subsequent resolutions are permanently rejected, preventing the receiver from re-appearing in discovery without an app restart. |
| [**ISSUE-09**](ISSUE-09-HIGH-abrupt-unmute-pc-speaker-blurt-on-session-stop.md) | **Auditory Symptom Cascade** | When `ISSUE-01` aborts streaming, `SessionController` immediately restores local audio before the session completes teardown, triggering an unpleasant, loud PC speaker blast. |
| [**ISSUE-11**](ISSUE-11-HIGH-unpersisted-endpoint-mute-state-leaves-speakers-muted-on-crash.md) | **Failure Recovery Hazard** | If a user terminates AirFlash or it crashes while in the middle of an Issue #6 drop, `AudioService._originalMute` is lost and PC speakers remain permanently muted. |
| [**ISSUE-03**](ISSUE-03-HIGH-wasapi-underrun-resync-strategy.md) | **Telemetry Distortion** | During silent playback preceding an Issue #6 disconnect, the engine injects synthetic silence frames that increment `underrun_packets`, misleading users to blame Wi-Fi bandwidth rather than the discovery kill switch. |

### 2. GitHub Issue #7: *"Realtime audio stutter / buffer starvation on 120ms latency mode with HomePod"*

| Related Issue | Role in Issue #7 | Mechanism |
| :--- | :--- | :--- |
| [**ISSUE-10**](ISSUE-10-HIGH-wlan-background-scan-latency-spikes-starve-realtime-buffer.md) | **Direct Physical Root Cause** | Windows WLAN AutoConfig executes periodic background scans (every 60–120s), freezing the Wi-Fi adapter for 50–120ms. In 120ms realtime mode, this starves the HomePod DAC jitter buffer and causes audible stuttering. |
| [**ISSUE-03**](ISSUE-03-HIGH-wasapi-underrun-resync-strategy.md) | **Missing Adaptive Buffer Control** | The audio capture pipeline lacks burst detection to dynamically expand the jitter buffer or warn the user to select 150ms/200ms latency when high DPC or Wi-Fi jitter is detected. |
| [**ISSUE-08**](ISSUE-08-LOW-ptp-timing-port-collision-fallback.md) | **Clock Drift & Port Collision** | Hardcoded PTP UDP ports 319/320 collisions or lack of graceful NTP fallback cause synchronization jitter between the PC host clock and the HomePod render clock. |
| [**ISSUE-12**](ISSUE-12-MEDIUM-rtsp-connection-mutex-contention-high-frequency-volume-polling.md) | **RTSP Mutex Lock Contention** | Polling `GET /info` every 1.0s over the shared `Arc<Mutex<Connection>>` delays RTCP `POST /feedback` round-trip measurements needed for HomePod packet pacing. |
| [**ISSUE-13**](ISSUE-13-MEDIUM-volume-curve-quantization-mismatch-stereo-unconfirmed.md) | **Volume Status Flapping** | Strict float equality checks on non-linear HomePod volume curves prevent confirmation of volume changes in stereo pairs, causing repeated sync queries during playback. |

---

## 🎯 Prioritization & Recommended Roadmap

1. **Immediate Focus (Critical & High)**:
   - Resolve **ISSUE-01**: Protect active streaming sessions from being prematurely aborted when discovery refreshes or drops an mDNS multicast packet.
   - Resolve **ISSUE-14**: Prune orphaned keys in `_pending` during discovery refresh and add timeouts to `DnsServiceResolve` operations to prevent unmanaged memory leaks and permanent device ghosting.
   - Resolve **ISSUE-09**: Introduce a grace period before restoring PC speaker un-mute during transient reconnection cycles to eliminate harsh audio blurts.
   - Resolve **ISSUE-10**: Suppress Windows Wi-Fi background scans via `WlanSetInterface(wlan_intf_opcode_media_streaming_mode)` and recommend 150 ms latency for HomePod OS 27.
   - Resolve **ISSUE-11**: Persist endpoint mute state to disk on startup/shutdown to avoid leaving host PC speakers permanently muted after abnormal exit.
   - Resolve **ISSUE-02**: Enable full automated pytest test coverage on standard Windows developer machines by recognizing Windows PowerShell.
   - Resolve **ISSUE-03**: Distinguish intentional idle silence padding from active capture queue starvation and add adaptive burst telemetry.

2. **Secondary Focus (Medium)**:
   - Resolve **ISSUE-12**: Throttle background `GET /info` volume polling to 10–15s to eliminate RTSP connection mutex contention with `POST /feedback`.
   - Resolve **ISSUE-13**: Allow $\pm 1\%$ tolerance when confirming HomePod volume changes to handle hardware DSP quantization differences in stereo pairs.
   - Expose the `"Copy diagnostics"` button in the Settings window (**ISSUE-04**).
   - Add friendly .NET 10 SDK pre-flight validation to `dotnet.ps1` (**ISSUE-05**).
   - Add IPv6 dual-stack support to DNS-SD discovery (**ISSUE-06**).

3. **Cleanup & Polish (Low)**:
   - Clean up orphaned strings in `Strings.zh.json` and add assertion testing (**ISSUE-07**).
   - Harden PTP UDP port 319/320 binding diagnostics and ephemeral timing fallbacks (**ISSUE-08**).
