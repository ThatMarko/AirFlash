# [MEDIUM] Transport-wide IPC command errors are discarded and become unrelated session timeouts

- **Issue ID**: ISSUE-12
- **Implementation plan**: [Work package ISSUE-12](IMPLEMENTATION_PLAN.md#issue-12)
- **PR group**: F — JSONL control contracts
- **Severity**: MEDIUM; an explicit final IPC rejection can lose its cause and enter host-timeout retry handling
- **Kind**: Defect — IPC contract
- **Subsystem**: Core process orchestration / native JSONL v1
- **Status**: Open — source-confirmed error-routing gap; production trigger frequency unknown
- **Runtime baseline**: upstream `41190e0`; documentation baseline `c077a05`
- **Evidence status**: Native error envelope and desktop filtering verified; deterministic native/fake-controller tests specified, not executed
- **Implementation status**: Proposal only
- **Target files**: [main.rs:22–34,54–110](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/main.rs#L22), [SessionController.cs:441–454](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SessionController.cs#L441)
- **Analysis context**: [Credentials/IPC limits and correlation](../analysis/credentials-and-ipc.md), [Session read budgets](../analysis/session.md), [Failure classification](../analysis/failures.md)

## Current behavior and concrete effect

The native input reader rejects a command exceeding 65,536 bytes including newline before parsing it. It emits `IPC command exceeds 64KiB` using empty request/session ids at [main.rs:88–94](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/main.rs#L88). Envelope-deserialization/malformed-JSON failures use the same empty ids at [lines 96–100](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/main.rs#L96). [send:22–31](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/main.rs#L22) gives these errors version 1, code `command_error`, channel `ipc`, and `retryable: false`.

The desktop's [ReadEventAsync:441–454](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SessionController.cs#L441) returns only an event whose version is 1 and `session_id` matches the attempt. Empty-id IPC errors fail that filter and are discarded. If no matching event follows, the actual rejection is replaced by `The audio engine response timed out.` after the current read budget. During playback setup that is normally the remaining 40-second deadline; during pairing each event read has a 40-second budget.

The original final `command_error`/retryable-false detail never reaches the playback fault handler. A timeout instead becomes desktop `host_error`, with no structured retry flag, at [SessionController.cs:373–388](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SessionController.cs#L373), and can retry when force reconnect is enabled. On a process already streaming, subsequent matching metrics can instead conceal the rejected command without a timeout. Native command errors alone leave the command loop alive.

This proves conditional error routing, not that normal UI commands currently exceed the limit or routinely send malformed JSON. The built-in serializer normally produces valid envelopes; the gap matters when the process interface rejects an oversized/faulty client message. It is not evidence of a receiver, network, or audio fault.

## Minimal deterministic reproduction plan

Two independent tests isolate the boundary without receiver sockets:

1. Native process fixture: send an oversized `hello` envelope with a synthetic session id and large ignored parameter, then a small valid `hello`. Assert the first event is final `command_error` with empty ids and the second is a normal hello; neither command opens receiver sockets.
2. Controller fixture: use a fake engine connection that does not emit `streaming`, set a short injected connect timing, and emit the exact native empty-id IPC error while the controller awaits its start result. Current filtering predicts a later timeout/`host_error`, not the supplied `command_error`.
3. Repeat with force reconnect enabled and assert the incorrect host-timeout retry can occur; repeat in pairing to assert the original detail is lost there too.
4. Control cases: stale nonempty session events remain ignored, a current-session ordinary error is preserved, and lifetime cancellation still cleans up.

These are proposed tests, not executed protocol/controller results. They require only fake connections and a built engine for the native half; no capture, credentials, pairing, or physical receiver is needed.

## Proposed implementation and acceptance

Define empty-id `event:error`, `code:command_error`, `channel:ipc` from the connection's child as a transport-wide protocol failure. Handle that verified envelope before ordinary session filtering, preserving its message/classification/final retry flag. Each desktop pairing/playback attempt owns its child connection, so this can be attributed to that connection's attempt without inventing a receiver fault. Do not reinterpret arbitrary other-session events as current failures.

- Oversized/malformed-envelope rejection surfaces its native IPC detail promptly rather than waiting for a read timeout.
- Final `command_error` does not enter playback retry through a substituted timeout.
- Valid matching events, stale-session filtering, pairing cancellation, and normal child disposal retain their current behavior.
- Empty-id events are accepted as global failures only under the defined version/type/code/channel policy; malformed child output has an explicit controlled protocol-failure result.
- Tests assert both failure identity and elapsed/bounded behavior using injected timing, with no real network actions.
- Existing JSONL v1 fields and schema-2 settings remain compatible.

## Separate contract-hardening note and existing tests

[EngineConnection.cs:33–46](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/EngineConnection.cs#L33) does not size-check outgoing commands, and checks stdout length only after allocating a complete line, in UTF-16 characters. Native output has no symmetric byte cap. Defining consistent byte/newline budgets and bounded desktop reads is separate IPC hardening; it is not required to demonstrate or fix the error-routing defect above and is not assigned another issue ID here.

[SessionTests.cs:184–187](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/SessionTests.cs#L184) covers timeout and [lines 292–302](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/SessionTests.cs#L292) covers stale-session filtering, but neither asserts empty-id IPC rejection handling. [equalizer_ipc.rs:9](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/tests/equalizer_ipc.rs#L9) covers nonfatal equalizer/parameter errors with supplied ids, not malformed envelopes/oversized lines. No existing test was found for this crossing of the native rejection path and desktop correlation filter. No verified external audio-issue linkage is asserted.
