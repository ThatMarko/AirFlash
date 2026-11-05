# Improve AirFlash reliability, pairing and diagnostics

This branch carries the remaining fixes forward onto upstream Main after AirFlash v0.6.2. Delayed work can no longer displace a newer session, DNS retries have finite logical deadlines, and a terminal engine event closes an open pairing prompt. The changes also improve mute recovery, diagnostics and panel feedback.

Base `Ding-Kyoma:main` at `a52803d`. Head `ThatMarko:codex/reliability-refresh-v062` at `367dbb3`. The original SDK preflight, settings recovery, PIN/HAP credential changes, native EQ responses and receiver ownership controls already on Main are the foundation for these follow-ups. [Complete diff](https://github.com/Ding-Kyoma/AirFlash/compare/a52803dac1696a56b7067356e09bd7e7ebb9dab6...ThatMarko:codex/reliability-refresh-v062)

## Review guide

### 1. Build and package boundaries

Reject cleanup through linked paths, preserve selected-host precedence, align preview naming, reject unbuildable package versions before side effects, and bound release diagnostic children. Fake-operation tests verify ordering, ownership and exit propagation. [Package build](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/scripts/build-package.ps1) · [Release checks](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/tests/test_verify_release.py)

### 2. Session and receiver intent

Capture the snapshot, owner and automatic intent together. Revalidate queued work under serialization, preserve exact manual endpoints and owned settings, and retain active controls through passive discovery gaps and Quit. Keep a visible Stop action if the worker cannot confirm process disposal, and wait for the worker to finish its mute restoration attempt before retrying disposal. Keep a changed default capture endpoint observable when delayed work belongs to a replaced session. Require two distinct usable hosts for a native pair while retaining the original advertised endpoints for inspection. Tests cover delayed identity persistence, explicit Stop during admission, alias selection, a manual endpoint changed during a queued Play, retained playback across an adapter change, endpoint recovery, and saved gain whose first guarded application was rejected. Saved volume work is retried with current authority without another save; edits made during application remain pending. [Controller](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/desktop/AirFlash.Core/SessionController.cs) · [Lifecycle fixtures](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/desktop/AirFlash.Tests/SessionLifecycleTests.cs) · [Saved-gain UI fixtures](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/desktop/AirFlash.App/Verification/UiSessionLifecycle.cs)

### 3. Settings recovery

Reject a null software-group ID through the existing original-byte recovery path. Controls cover repeated loads, BOM recovery and schema-4-to-5 migration. [Settings fixtures](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/desktop/AirFlash.Tests/SoftwareGroupSettingsTests.cs)

### 4. DNS lifetime

Separate logical deadlines from native ownership. Bound retries and queued work, ignore obsolete callbacks and retain uncertain helper capacity until exact child exit. Coalesce ordinary queue waits separately from ownership warnings. [Discovery coordinator](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/desktop/AirFlash.App/Services/WindowsDiscovery.cs) · [Resolve fixtures](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/desktop/AirFlash.Tests/ResolveProcessTests.cs)

### 5. Pairing and stored passwords

Treat only initial-open absence as a missing stored password. Read the opened object once with a bound and preserve DPAPI/decoding errors. Validate the complete UTF-8 value and keep one reader while input is open. Join the prompt before reporting a terminal event. Exercise the real authentication selector with injected lookups and exchanges, covering both storage errors and failed exchanges without fallback. [Stored credentials](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/native/airflash-engine/src/credentials.rs) · [Prompt lifecycle fixtures](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/desktop/AirFlash.Tests/PairingPromptLifecycleTests.cs)

### 6. Process-owned IPC faults

Keep the defined JSONL v1 terminal error's detail and nonretryable classification in hello, receiver-capability and application-catalog queries. Preserve the session filters and upstream equalizer preparation behavior. [IPC fixtures](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/desktop/AirFlash.Tests/IpcFailureTests.cs)

### 7. Local mute recovery

Journal the exact endpoint and previous bit before muting, under an exclusive owner lease. Keep unresolved or ambiguous records and restore the saved bit after normal cleanup. Fake endpoints exercise both prior bits, lease contention and persistence failures. [Mute recovery fixtures](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/desktop/AirFlash.Tests/MuteRecoveryTests.cs)

### 8. Diagnostics, panel feedback and clock startup

Expose Monitor Copy alongside Export and report optional pre-EQ padded frames and wholly padded pulls once per shared capture pull. Keep retry faults visible until recovery, provide localized notice dismissal, and reserve scrollbar space only when needed and let receiver volume sliders fill the available card width. Validate PTP Grant flags, exact peer/target, complete TLV framing and current per-type pending requests while retaining upstream clock readiness and retry cadence. Accept independently reordered Grants and same-identity contract renewal without treating the sender counter as a local request token. Duplicates without pending work cannot renew a lease; these checks do not provide cryptographic replay protection. Record Grant acceptance and rejection categories and subscription send failures in the existing clock diagnostics. Keep using the last validated PTP mapping during the existing grace period if the same clock needs to reacquire. Real expiry still enters the ownership guard, and a pending check prevents final media writes. Keep empty capture polls from resetting the silence-probe cooldown. Give Settings navigation and dropdown scrollbars their own columns. [Capture accounting](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/native/airflash-engine/src/live.rs) · [PTP fixtures](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/native/airflash-engine/src/clock_tests.rs) · [Scroll fixtures](https://github.com/ThatMarko/AirFlash/blob/367dbb3dc6163b2bb9e3772a267d33314a05a4f3/desktop/AirFlash.App/Verification/UiScrolling.cs)

## Verification

Exact-source [Windows headless qualification](https://github.com/ThatMarko/AirFlash/actions/runs/42006160255) of `367dbb3` passes 1,247 Core, 252 Python and 362 Rust tests, with two intentional native ignores. The Release solution build has zero warnings/errors. Native lint and compatibility checks pass; formatting has no new violations, with eight unchanged baseline files retaining existing drift. The [full isolated desktop matrix](https://github.com/ThatMarko/AirFlash/actions/runs/42006699186) passes all 22 cells and 15,189 assertions, with observed regional/catalog combinations and four synthetic 125% DPI cells. Five startup guards and six synthetic DNS-helper settlement checks pass. Before/after source manifests match all 305 tracked files; binary and assembly receipts are unchanged across cells.

Two added controls fail at their intended assertions on test-only commits with unchanged candidate runtime code: reordered independent Grants adopt one lease instead of three, and a saved gain never reaches the still-owned stream after a competing Play is rejected. [Grant before control](https://github.com/ThatMarko/AirFlash/actions/runs/42000664181) · [Gain before control](https://github.com/ThatMarko/AirFlash/actions/runs/42000856056)

An earlier candidate passed broad suites and disposable packaging, but those results do not qualify this corrected head. The checks use isolated services, temporary settings and synthetic input. A fresh package/MSI lifecycle was not run on this corrected head. Receiver playback, physical sleep/resume, helper-inclusive discovery resource cost and physical power loss remain field checks. The packet controls do not establish the cause of reported acoustic or clock failures. [PTP report #38](https://github.com/Ding-Kyoma/AirFlash/issues/38) · [Discovery report #42](https://github.com/Ding-Kyoma/AirFlash/issues/42)