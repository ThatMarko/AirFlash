# Improve AirFlash reliability, pairing and diagnostics

AirFlash can receive a discovery result or finish saving settings after a user has already stopped playback or selected a different receiver. This change checks ownership before that work can affect the new session. It also bounds receiver resolution, preserves pairing errors, and makes interrupted mute operations recoverable. The base is upstream v0.7.0 at `60e3390`. The head is `ThatMarko:codex/pr-ready` at `2812b6f`. [Complete diff](https://github.com/Ding-Kyoma/AirFlash/compare/60e3390c8e171a49ba842cd8ccc957d648e3823b...ThatMarko:codex/pr-ready)

## Review guide

### 1. Build and packaging

Reject unbuildable numeric versions before reservation. Check the output tree before removing an existing build, preserve the selected SDK host, and use the same preview MSI name in the builder and verifier. Release diagnostics have bounded waits and clean up their own child processes. The package fixtures intercept deletion, native builds and publishing. [Package guards](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/scripts/build-package.ps1) · [Packaging tests](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/tests/test_build_package.py)

### 2. Session and receiver intent

Revalidate a queued automatic start or settings update under serialization. Stop, Pair, a changed manual endpoint and a replacement session each invalidate older work. An owned stream retains its transport through discovery gaps. A new group must be complete. Manual Reconnect follows current availability after ownership loss. Multi-room still uses per-member channels, offsets and remembered volume. The shell attempts every shutdown step even when an earlier one fails. [Session controller](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/desktop/AirFlash.Core/SessionController.cs) · [Lifecycle fixtures](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/desktop/AirFlash.Tests/SessionLifecycleTests.cs)

### 3. Settings recovery

A null software-group ID now reaches the existing invalid-settings recovery path. Tests cover migration, original backup bytes, repeat loads and manual receiver data. The upstream schema remains in use. [Group settings fixtures](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/desktop/AirFlash.Tests/SoftwareGroupSettingsTests.cs)

### 4. Discovery

Give each resolve a deadline and bounded retries. Ignore callbacks from previous requests and track helper ownership until the exact child exits. During playback, a retry for a positively identified different receiver can remain in diagnostics. Unknown, active-member, adapter and capacity failures remain visible. The original string failure event still works. Typed providers can identify a retry and its later resolution. The discovery check freezes its output and exit decision at the same observation. [Discovery service](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/desktop/AirFlash.App/Services/WindowsDiscovery.cs) · [Lifecycle fixtures](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/desktop/AirFlash.Tests/DiscoveryLifecycleTests.cs)

### 5. Pairing and stored receiver passwords

Preserve read, decryption and decoding errors when loading a receiver password. Submit the complete accepted UTF-8 value and join an outstanding prompt when the engine reports a terminal failure. Upstream PR #25 already provided PIN normalization and HAP credential lookup fixes. [Credential storage](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/native/airflash-engine/src/credentials.rs) · [Prompt fixtures](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/desktop/AirFlash.Tests/PairingPromptLifecycleTests.cs) · [Merged PR #25](https://github.com/Ding-Kyoma/AirFlash/pull/25)

### 6. Process errors and control replies

Retain the original detail and retry classification of process-owned terminal JSONL v1 errors in hello and capability queries. Preserve filtering of unrelated session events and upstream equalizer replies. Remote controls, Now Playing updates and diagnostic responses are checked against the transport that owns them. [IPC fixtures](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/desktop/AirFlash.Tests/IpcFailureTests.cs) · [Diagnostic correlation](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/desktop/AirFlash.Tests/DiagnosticCorrelationTests.cs)

### 7. Local mute recovery

Record the actual endpoint and its previous mute bit under an exclusive lease before mutating it. Normal cleanup attempts exact restoration, including when the endpoint was already muted. Ambiguous or unavailable endpoints retain their recovery record. Tests use fake endpoints and a temporary store. [Mute recovery fixtures](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/desktop/AirFlash.Tests/MuteRecoveryTests.cs)

### 8. Audio, diagnostics and panel feedback

Bound capture and transport counter history, expose Monitor Copy, and retain actionable faults across retries. Report pre-EQ padded frames once per shared capture pull. Capture telemetry reuses one sample per interval and excludes empty queues from the age histogram. Keep controls clear of scrollbars, including when the receiver list overflows. [Capture accounting](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/native/airflash-engine/src/live.rs) · [Scrolling fixtures](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/desktop/AirFlash.App/Verification/UiScrolling.cs)

Keep the existing HomePod timing-peer setup and upstream guarded recovery. Correlate PTP Grants with a current request of the same type, tolerate reserved trailer flags and preserve the valid clock mapping during holdover. During group shutdown, a TEARDOWN already parsed by a control worker suppresses both the final RTP packet and outgoing TEARDOWN. An advisory close header alone allows the normal fade and outgoing TEARDOWN. A separate loss reported by the feedback worker can still suppress outgoing TEARDOWN. [PTP fixtures](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/native/airflash-engine/src/clock_tests.rs) · [Playback and loopback fixtures](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/native/airflash-engine/src/session.rs)

## Verification

Windows headless qualification built the Release solution and passed 1,438 Core, 253 Python and 409 Rust tests. The native suite recorded 2 intentional ignores. Clippy and Ruff passed. Every changed Rust file passed rustfmt. Six byte-identical upstream files retain their existing formatting. [Exact-source headless run](https://github.com/ThatMarko/AirFlash/actions/runs/44371683131)

The hosted WPF suite passed all 21 isolated cases with 17,951 assertions, including lifecycle, pairing, discovery, scrolling, diagnostics and eight language or regional configurations. The source, apphost, managed assembly and individual reports were verified against the run receipts. [Exact-source desktop run](https://github.com/ThatMarko/AirFlash/actions/runs/44371694466)

A consumer compiled against upstream Core also ran against the candidate without recompilation. All 1,476 original public type and member entries were present in a comparison of the two Release builds. The native engine retains the public capture metrics shape, age accessor and ownership helpers. [Core contract fixtures](https://github.com/ThatMarko/AirFlash/blob/2812b6f13f97e101f59656b8edfdfbdef2bf7cde/desktop/AirFlash.Tests/CorePublicApiCompatibilityTests.cs)

The packet, audio and UI checks use synthetic input or loopback receivers. Audible crackling, physical sleep/resume, power-loss durability and HomePod Sound Glow output remain hardware observations. The open grouping, master-volume and iOS playback requests have separate acceptance criteria. [Upstream release notes](https://github.com/Ding-Kyoma/AirFlash/releases/tag/v0.7.0) · [Sound Glow field report](https://github.com/Ding-Kyoma/AirFlash/pull/41#issuecomment-6973389190)
