# Improve AirFlash reliability, pairing and diagnostics

This branch carries the remaining fixes forward onto upstream Main after AirFlash v0.6.2. Delayed work can no longer displace a newer session, DNS retries have finite logical deadlines, and a terminal engine event closes an open pairing prompt. The changes also improve mute recovery, diagnostics and panel feedback.

Base `Ding-Kyoma:main` at `11b7d18`. Head `ThatMarko:codex/reliability-refresh-v062` at `dc78911`. The original SDK preflight, settings recovery, PIN/HAP credential changes, native EQ responses and receiver ownership controls already on Main are the foundation for these follow-ups. [Complete diff](https://github.com/Ding-Kyoma/AirFlash/compare/11b7d18ad89fe2bbab43dfbea1191609e9457177...ThatMarko:codex/reliability-refresh-v062)

## Review guide

### 1. Build and package boundaries

Reject cleanup through linked paths, preserve selected-host precedence, align preview naming, reject unbuildable package versions before side effects, and bound release diagnostic children. Fake-operation tests verify ordering, ownership and exit propagation. [Package build](https://github.com/ThatMarko/AirFlash/blob/dc78911eb78354332e5d8c118a0cc811bf0296e3/scripts/build-package.ps1) · [Release checks](https://github.com/ThatMarko/AirFlash/blob/dc78911eb78354332e5d8c118a0cc811bf0296e3/tests/test_verify_release.py)

### 2. Session and receiver intent

Capture the snapshot, owner and automatic intent together. Revalidate queued work under serialization, preserve exact manual endpoints and owned settings, and retain active controls through passive discovery gaps and Quit. Keep a visible Stop action if the worker cannot confirm process disposal, and wait for the worker to finish its mute restoration attempt before retrying disposal. Keep a changed default capture endpoint observable when delayed work belongs to a replaced session. Tests cover delayed identity persistence, explicit Stop during admission, alias selection, a manual endpoint changed during a queued Play, retained playback across an adapter change, and endpoint recovery. [Controller](https://github.com/ThatMarko/AirFlash/blob/dc78911eb78354332e5d8c118a0cc811bf0296e3/desktop/AirFlash.Core/SessionController.cs) · [Lifecycle fixtures](https://github.com/ThatMarko/AirFlash/blob/dc78911eb78354332e5d8c118a0cc811bf0296e3/desktop/AirFlash.Tests/SessionLifecycleTests.cs)

### 3. Settings recovery

Reject a null software-group ID through the existing original-byte recovery path. Controls cover repeated loads, BOM recovery and schema-4-to-5 migration. [Settings fixtures](https://github.com/ThatMarko/AirFlash/blob/dc78911eb78354332e5d8c118a0cc811bf0296e3/desktop/AirFlash.Tests/SoftwareGroupSettingsTests.cs)

### 4. DNS lifetime

Separate logical deadlines from native ownership. Bound retries and queued work, ignore obsolete callbacks and retain uncertain helper capacity until exact child exit. Coalesce ordinary queue waits separately from ownership warnings. [Discovery coordinator](https://github.com/ThatMarko/AirFlash/blob/dc78911eb78354332e5d8c118a0cc811bf0296e3/desktop/AirFlash.App/Services/WindowsDiscovery.cs) · [Resolve fixtures](https://github.com/ThatMarko/AirFlash/blob/dc78911eb78354332e5d8c118a0cc811bf0296e3/desktop/AirFlash.Tests/ResolveProcessTests.cs)

### 5. Pairing and stored passwords

Treat only initial-open absence as a missing stored password. Read the opened object once with a bound and preserve DPAPI/decoding errors. Validate the complete UTF-8 value and keep one reader while input is open. Join the prompt before reporting a terminal event. Exercise the real authentication selector with injected lookups and exchanges, covering both storage errors and failed exchanges without fallback. [Stored credentials](https://github.com/ThatMarko/AirFlash/blob/dc78911eb78354332e5d8c118a0cc811bf0296e3/native/airflash-engine/src/credentials.rs) · [Prompt lifecycle fixtures](https://github.com/ThatMarko/AirFlash/blob/dc78911eb78354332e5d8c118a0cc811bf0296e3/desktop/AirFlash.Tests/PairingPromptLifecycleTests.cs)

### 6. Process-owned IPC faults

Keep the defined JSONL v1 terminal error's detail and nonretryable classification in hello, receiver-capability and application-catalog queries. Preserve the session filters and upstream equalizer preparation behavior. [IPC fixtures](https://github.com/ThatMarko/AirFlash/blob/dc78911eb78354332e5d8c118a0cc811bf0296e3/desktop/AirFlash.Tests/IpcFailureTests.cs)

### 7. Local mute recovery

Journal the exact endpoint and previous bit before muting, under an exclusive owner lease. Keep unresolved or ambiguous records and restore the saved bit after normal cleanup. Fake endpoints exercise both prior bits, lease contention and persistence failures. [Mute recovery fixtures](https://github.com/ThatMarko/AirFlash/blob/dc78911eb78354332e5d8c118a0cc811bf0296e3/desktop/AirFlash.Tests/MuteRecoveryTests.cs)

### 8. Diagnostics, panel feedback and clock startup

Expose Monitor Copy alongside Export and report optional pre-EQ padded frames and wholly padded pulls once per shared capture pull. Keep retry faults visible until recovery, provide localized notice dismissal, and reserve scrollbar space only when needed while keeping the compact panel's volume tracks aligned. Validate PTP Grant flags, peer Signaling sequences and complete TLV framing while retaining upstream clock readiness and retry cadence. Record Grant acceptance and rejection categories and subscription send failures in the existing clock diagnostics. [Capture accounting](https://github.com/ThatMarko/AirFlash/blob/dc78911eb78354332e5d8c118a0cc811bf0296e3/native/airflash-engine/src/live.rs) · [PTP fixtures](https://github.com/ThatMarko/AirFlash/blob/dc78911eb78354332e5d8c118a0cc811bf0296e3/native/airflash-engine/src/clock_tests.rs) · [Scroll fixtures](https://github.com/ThatMarko/AirFlash/blob/dc78911eb78354332e5d8c118a0cc811bf0296e3/desktop/AirFlash.App/Verification/UiScrolling.cs)

## Verification

- The exact Release solution build passed with zero warnings and errors. The exact-source Windows packaging workflow passed 1,243 Core, 252 Python and 356 Rust tests, with two intentional device and soak ignores. Both new cleanup regressions failed under the prior controller. [Package qualification](https://github.com/ThatMarko/AirFlash/actions/runs/41940619983) [Headless run](https://github.com/ThatMarko/AirFlash/actions/runs/41934146311)
- The same-intent endpoint test reproduced the failure under the prior observer logic. The exact-head Windows mock matrix passed all 22 cells and 14,877 assertions. It includes the held-Apply endpoint case, both prior mute states for Quit and the visible Stop after engine cleanup failure, and four synthetic 125% DPI cells. Five startup guards and six synthetic DNS-helper settlement checks passed. Before and after source manifests matched all 305 tracked files. [Endpoint before control](https://github.com/ThatMarko/AirFlash/actions/runs/41934697011) · [Desktop matrix](https://github.com/ThatMarko/AirFlash/actions/runs/41940619999)

The unofficial `6.0.1-rc.1` artifact passed nine additional portable mock UI cells with 10,407 assertions. Portable, MSI and engine hashes match the run manifest. Fresh install, repair, upgrade and removal passed on a disposable Windows runner. The checks use isolated services, temporary settings and synthetic input. Receiver playback, physical sleep/resume and physical power loss remain field checks. [PTP report #38](https://github.com/Ding-Kyoma/AirFlash/issues/38) · [Discovery report #42](https://github.com/Ding-Kyoma/AirFlash/issues/42)
