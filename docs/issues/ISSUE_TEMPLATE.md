# [SEVERITY] <Short, Descriptive Title of the Issue>

<!-- 
INSTRUCTIONS FOR SUBMISSION:
Replace the bracketed placeholders with your issue's details.
Refer to docs/issues/README.md for severity classification guidelines.
Cross-reference related GitHub issues (e.g. Issue #6, Issue #7) or internal tracked issues (ISSUE-01 through ISSUE-14).
-->

- **Issue ID**: ISSUE-XX <!-- Sequential ID or leave as TBD -->
- **Severity**: **[CRITICAL | HIGH | MEDIUM | LOW]**
- **Subsystem**: `[Desktop App (AirFlash.App) | Core Orchestration (AirFlash.Core) | Audio Engine (airflash-engine) | Win32 Discovery (dnsapi/wlanapi) | Build & Tests]`
- **Status**: Open / Triaged
- **Target Files**:
  - [`desktop/AirFlash.App/...`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/...)
  - [`native/airflash-engine/src/...`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/...)
- **Upstream Branch Status**: Affects `main` (v0.3.0/v0.3.2) / Affects `dev` (v0.3.3-rc.1, PR #5) / Both

---

## 1. Summary
<!-- 
Provide a concise, 1-2 paragraph description of the problem. 
Describe what happens, why it matters, and who is impacted (e.g. HomePod stereo users, third-party AirPlay 2 receiver owners, multi-homed network users).
-->

[Describe the defect, regression, or missing safety mechanism here.]

---

## 2. Technical Root Cause Analysis
<!-- 
Provide recursive, code-level analysis. Trace the exact call stack, lifecycle state transitions, 
and concurrency hazards. Quote the relevant lines of code with absolute file/line markdown links.
-->

### 2.1 Code Trace & Call Site
In [`desktop/AirFlash.App/...`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/...#L1-L20):

```csharp
// Paste relevant code excerpt here
```

### 2.2 Mechanism of Failure
<!-- 
Detail why this code causes the problem:
- Concurrency / lock contention?
- Race condition or event-ordering mismatch?
- Win32 API interop memory leak or unhandled status code?
- Premature teardown due to control/data plane coupling?
-->

1. **Failure Step 1**: ...
2. **Failure Step 2**: ...
3. **Failure Step 3**: ...

---

## 3. Reproduction Steps & Diagnostic Evidence

### 3.1 Environment Details
- **Operating System**: Windows 10 / Windows 11 (Build: `______`)
- **AirFlash Version**: `main` (Commit: `______`) / `dev` (Commit: `67435a4`)
- **Audio Output Endpoint**: e.g., Realtek High Definition Audio (Default Render Device)
- **AirPlay Receiver(s)**:
  - Model: e.g., Apple HomePod (2nd gen) Stereo Pair / Sonos Era 100 / Denon AVR
  - Firmware Version: e.g., HomePod OS 17.5 (Build 21T567) / 18.0
  - Configured Latency: e.g., Realtime (120 ms) / Normal (200 ms) / Compatibility (3000 ms)
- **Network Interface Configuration**: e.g., Wi-Fi 6 (802.11ax), Dual-band 2.4/5GHz, virtual adapters active (Tailscale / Hyper-V vSwitch)

### 3.2 Step-by-Step Reproduction
1. Launch AirFlash and select receiver `______`.
2. Start streaming system audio.
3. Trigger event: [e.g. Wi-Fi background scan / idle audio pause / virtual interface IP renew / 90s discovery timeout].
4. **Observed Result**: [Audio stops abruptly / PC speakers blurt loudly / Receiver disappears from UI / Telemetry spikes].
5. **Expected Result**: [Audio continues uninterrupted / Clean, graceful silence / UI retains controls / Telemetry accurately reflects health].

### 3.3 Diagnostic Artifacts
<!-- 
Include relevant log lines from `%LOCALAPPDATA%\AirFlash\logs\` or the Settings -> Monitor telemetry snapshot.
-->

```text
// Example log output or monitor metrics
[2026-10-06 20:00:00.123] [Discovery] Resolved: {"Instance":"HomePod Living Room._airplay._tcp.local"}
[2026-10-06 20:00:20.456] [Session] Stopping active stream...
```

---

## 4. Relationship to Known Systemic Issues

<!-- 
Identify if and how this issue relates to existing upstream GitHub issues:
- Issue #6: "AirFlash stops streaming / disconnects unexpectedly when devices refresh, roams, or drops off discovery"
- Issue #7: "Realtime audio stutter / buffer starvation on 120ms latency mode with HomePod"
-->

- **Relationship to GitHub Issue #6 (Unexpected Disconnects / Discovery Drops)**:
  - [Explain whether this issue is a root cause, secondary consequence, or exacerbating factor of Issue #6.]
- **Relationship to GitHub Issue #7 (Audio Glitches / Buffer Starvation at 120ms)**:
  - [Explain whether this issue affects transport timing, WLAN latency, DPC scheduling, or jitter buffers.]
- **Related Tracked Internal Issues**:
  - `ISSUE-01`: Premature session abort on mDNS snapshot flush.
  - `ISSUE-03`: WASAPI silence padding vs true queue starvation telemetry.
  - `ISSUE-09`: PC speaker unmute blurt cascade on session teardown.
  - `ISSUE-10`: WLAN AutoConfig background scan starvation.
  - `ISSUE-14`: DnsServiceResolve unmanaged memory leak and permanent device ghosting.

---

## 5. Proposed Solution & Implementation Guidance

### 5.1 Proposed Code Diff
<!-- 
Provide concrete code diffs or pseudocode illustrating the minimal, robust fix.
-->

```diff
--- a/desktop/AirFlash.App/...
+++ b/desktop/AirFlash.App/...
@@ -100,6 +100,7 @@
+ // Protective guard or timeout logic
```

### 5.2 Verification Plan
<!-- 
Define how the fix should be verified:
- Automated unit/integration test (C# / Rust)
- Manual Wi-Fi roaming / network interface flap test
- Hardware qualification test (HomePod stereo / third-party receiver)
-->

1. Run automated test suite: `uv run pytest` and `powershell -File scripts/test.ps1`.
2. Run stress/soak test: `cargo test -p airflash-engine --test soak`.
3. Verify manual reproduction scenario: Verify that simulated packet drop does not trigger session termination.
