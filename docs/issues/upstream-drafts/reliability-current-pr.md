# Improve AirFlash reliability, pairing and diagnostics

This branch carries the remaining fixes forward onto upstream Main after AirFlash v0.6.2. Delayed work can no longer displace a newer session, DNS retries have finite logical deadlines, and a terminal engine event closes an open pairing prompt. The changes also improve mute recovery, diagnostics and panel feedback.

Base `Ding-Kyoma:main` at `a52803d`. Head `ThatMarko:codex/reliability-refresh-v062` at `18eb7de`. The original SDK preflight, settings recovery, PIN/HAP credential changes, native EQ responses and receiver ownership controls already on Main are the foundation for these follow-ups. [Complete diff](https://github.com/ThatMarko/AirFlash/compare/a52803dac1696a56b7067356e09bd7e7ebb9dab6...18eb7de28da3c669670392c5556a636e46b594ca)

The later capture and engine audit changes drain readable WASAPI packets after a missed wake, retain yield-only receiver ownership, and align clock telemetry and capability prediction with playback. Queued remote controls carry the originating live transport's authority. Shutdown closes diagnostics admission without stranding queued writers. Now Playing writes stay ordered, project cached progress on replay, and can clear the native mailbox during standby resume. Retransmit gap telemetry excludes requests for zero packets. [Capture correction](https://github.com/ThatMarko/AirFlash/commit/07587e0fe30882ca7c3348ada0244185e024df54) · [Engine and desktop audit](https://github.com/ThatMarko/AirFlash/commit/a78b723fac05e15a526a797a7134f38978bd543f) · [Follow-up regressions](https://github.com/ThatMarko/AirFlash/commit/d6de3adade3306f72bb74a369e5eeb4352bf96dd)

## Review guide

### 1. Build and package boundaries

Reject cleanup through linked paths, preserve selected-host precedence, align preview naming, reject unbuildable package versions before side effects, and bound release diagnostic children. Fake-operation tests verify ordering, ownership and exit propagation. [Package build](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/scripts/build-package.ps1) · [Release checks](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/tests/test_verify_release.py)

### 2. Session and receiver intent

Capture the snapshot, owner and automatic intent together. Revalidate queued work under serialization, preserve exact manual endpoints and owned settings, and retain active controls through passive discovery gaps and Quit. Keep a visible Stop action if the worker cannot confirm process disposal, and wait for the worker to finish its mute restoration attempt before retrying disposal. During Quit, attempt every cleanup step even if an earlier one fails. Preserve the caller's dispatcher context and keep the owned Stop action available while rejecting new Settings and second-instance activation. Keep a changed default capture endpoint observable when delayed work belongs to a replaced session. Require two distinct usable hosts for a native pair while retaining the original advertised endpoints for inspection. Tests cover delayed identity persistence, explicit Stop during admission, alias selection, a manual endpoint changed during a queued Play, retained playback across an adapter change, endpoint recovery, and saved gain whose first guarded application was rejected. Saved volume work is retried with current authority without another save; edits made during application remain pending. [Controller](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/desktop/AirFlash.Core/SessionController.cs) · [Lifecycle fixtures](https://github.com/ThatMarko/AirFlash/blob/7c07561aac7851cb4394cfd687a7c5393f46b3ca/desktop/AirFlash.Tests/SessionLifecycleTests.cs) · [Saved-gain UI fixtures](https://github.com/ThatMarko/AirFlash/blob/7c07561aac7851cb4394cfd687a7c5393f46b3ca/desktop/AirFlash.App/Verification/UiSessionLifecycle.cs) · [Shutdown order and failure coverage](https://github.com/ThatMarko/AirFlash/blob/7c07561aac7851cb4394cfd687a7c5393f46b3ca/desktop/AirFlash.Tests/ShellShutdownTests.cs)

### 3. Settings recovery

Reject a null software-group ID through the existing original-byte recovery path. Controls cover repeated loads, BOM recovery and schema-4-to-5 migration. [Settings fixtures](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/desktop/AirFlash.Tests/SoftwareGroupSettingsTests.cs)

### 4. DNS lifetime

Separate logical deadlines from native ownership. Bound retries and queued work, ignore obsolete callbacks and retain uncertain helper capacity until exact child exit. Coalesce ordinary queue waits separately from ownership warnings. [Discovery coordinator](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/desktop/AirFlash.App/Services/WindowsDiscovery.cs) · [Resolve fixtures](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/desktop/AirFlash.Tests/ResolveProcessTests.cs)

### 5. Pairing and stored passwords

This follows merged PR #25, which normalized PIN input and corrected HAP credential lookup. Treat only initial-open absence as a missing stored password. Read the opened object once with a bound and preserve DPAPI/decoding errors. Validate the complete UTF-8 value and keep one reader while input is open. Join the prompt before reporting a terminal event. Exercise the real authentication selector with injected lookups and exchanges, covering both storage errors and failed exchanges without fallback. [Stored credentials](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/native/airflash-engine/src/credentials.rs) · [Prompt lifecycle fixtures](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/desktop/AirFlash.Tests/PairingPromptLifecycleTests.cs) · [Merged PR #25](https://github.com/Ding-Kyoma/AirFlash/pull/25)

### 6. Process-owned IPC faults

Keep the defined JSONL v1 terminal error's detail and nonretryable classification in hello, receiver-capability and application-catalog queries. Preserve the session filters and upstream equalizer preparation behavior. [IPC fixtures](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/desktop/AirFlash.Tests/IpcFailureTests.cs)

### 7. Local mute recovery

Journal the exact endpoint and previous bit before muting, under an exclusive owner lease. Keep unresolved or ambiguous records and restore the saved bit after normal cleanup. Fake endpoints exercise both prior bits, lease contention and persistence failures. [Mute recovery fixtures](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/desktop/AirFlash.Tests/MuteRecoveryTests.cs)

### 8. Diagnostics, panel feedback and clock startup

Expose Monitor Copy alongside Export and report optional pre-EQ padded frames and wholly padded pulls once per shared capture pull. Keep retry faults visible until recovery, provide localized notice dismissal, and reserve scrollbar space only when needed and let receiver volume sliders fill the available card width. Validate PTP Grant flags, exact peer/target, complete TLV framing and current per-type pending requests while retaining upstream clock readiness and retry cadence. Accept independently reordered Grants and same-identity contract renewal without treating the sender counter as a local request token. A delayed valid Grant can match a later compatible pending request. The serialized replay counter remains zero for compatibility. Record Grant acceptance and rejection categories and subscription send failures in the existing clock diagnostics. Keep using the last validated PTP mapping during the existing grace period if the same clock needs to reacquire. Real expiry still enters the ownership guard, and a pending check prevents final media writes. Keep empty capture polls from resetting the silence-probe cooldown. Give Settings navigation and dropdown scrollbars their own columns. [Capture accounting](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/native/airflash-engine/src/live.rs) · [PTP fixtures](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/native/airflash-engine/src/clock_tests.rs) · [Scroll fixtures](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/desktop/AirFlash.App/Verification/UiScrolling.cs)

Use the same merged receiver model for catalog display and automatic PTP role selection. Start the capture crossfade at the first sample from a reopened stream, including inside a packet. Keep percentile sorting outside the capture lock and share one capture snapshot per reporting interval. [Clock role tests](https://github.com/ThatMarko/AirFlash/blob/18eb7de28da3c669670392c5556a636e46b594ca/desktop/AirFlash.Tests/ReceiverClockRoleTests.cs) · [Capture reopen fix](https://github.com/ThatMarko/AirFlash/commit/181159bcaaa928842f2b0e5dc952ba6751b40700)

Keep bounded counter and discovery histories for diagnosis. During healthy playback, quiet routine resolve warnings when discovery identifies a different receiver. Match exported engine summaries to the request and current playback attempt, check the native epoch when available, and capture the summary together with its counter history. [Diagnostic correlation tests](https://github.com/ThatMarko/AirFlash/blob/18eb7de28da3c669670392c5556a636e46b594ca/desktop/AirFlash.Tests/DiagnosticCorrelationTests.cs)

## Verification

Windows verification on `18eb7de` passed the Release build, 1,379 Core, 252 Python and 375 Rust tests, with two Rust ignores. The `0.3.7-rc.1` preview passed six full portable UI cells, three fractional DPI cells, embedded-engine checks, repeat packaging and the disposable installer lifecycle. The full UI cells verified that the saved diagnostic reply carries the actual request ID and active epoch. [Preview validation](https://github.com/ThatMarko/AirFlash/actions/runs/42917344419)

Two added controls fail at their intended assertions on test-only commits with unchanged candidate runtime code: reordered independent Grants adopt one lease instead of three, and a saved gain never reaches the still-owned stream after a competing Play is rejected. [Grant before control](https://github.com/ThatMarko/AirFlash/actions/runs/42000664181) · [Gain before control](https://github.com/ThatMarko/AirFlash/actions/runs/42000856056)

The checks use isolated services, temporary settings and synthetic input. Receiver playback, physical sleep/resume, helper-inclusive discovery resource cost and physical power loss remain field checks. The packet controls cover Grant handling and holdover with synthetic inputs. The reported acoustic and clock failures remain field observations. [PTP report #38](https://github.com/Ding-Kyoma/AirFlash/issues/38) · [After-sleep report #42](https://github.com/Ding-Kyoma/AirFlash/issues/42)

## Related upstream work

Issues #6, #7, #14 and #24 are closed. This PR carries the remaining session, discovery and authentication follow-ups. The proposed wake-retry policy in #45 and multi-room streaming in #48 are separate changes. [Wake retry PR #45](https://github.com/Ding-Kyoma/AirFlash/pull/45) · [Multi-room PR #48](https://github.com/Ding-Kyoma/AirFlash/pull/48)
