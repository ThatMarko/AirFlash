# [LOW] Ignored equalizer updates return success metadata for the submitted curve

- **Issue ID**: ISSUE-21
- **Severity**: LOW
- **Kind**: Defect — IPC result contract
- **Type / Status**: Open IPC result-contract defect — source-verified; no inferred desktop or acoustic failure
- **Subsystem**: JSONL equalizer command handling and native latest-value mailbox
- **Runtime baseline**: `41190e0d13a63a714c08dffe73ababca1804875c`, unchanged by documentation baseline `c077a0577de26ad7e44b8fd50f62456e12965fb0`
- **Evidence status**: Command handler, mailbox, desktop reader, and existing test sources inspected on 2026-10-07. The isolated command/control reproduction below has not been executed. No receiver, WASAPI capture, credentials, or hardware response was inspected.
- **Implementation status**: Proposal only; no runtime change applied
- **Target files**: [main.rs](../../native/airflash-engine/src/main.rs#L185-L202), [equalizer.rs](../../native/airflash-engine/src/equalizer.rs#L124-L145), [equalizer_ipc.rs](../../native/airflash-engine/tests/equalizer_ipc.rs#L8-L65)
- **Analysis context**: [Equalizer](../analysis/equalizer.md), [Credentials and IPC](../analysis/credentials-and-ipc.md), [Session](../analysis/session.md)

## 1. Current behavior and bounded impact

For a positive valid `set_equalizer` sequence on a matching stored start entry, the command handler prepares the submitted curve, calls `control.set`, and returns `equalizer_changed` containing the submitted sequence and its calculated headroom. `Control::set` updates the mailbox only when the sequence is strictly newer and returns no acceptance result. A stale sequence, or a duplicate sequence containing a different curve, is therefore ignored while still producing the same successful reply shape. ([main.rs:185–202](../../native/airflash-engine/src/main.rs#L185-L202), [equalizer.rs:133–137](../../native/airflash-engine/src/equalizer.rs#L133-L137).)

The response does establish that the payload prepared successfully; it does **not** establish mailbox adoption or DSP application. The contract limitation is that an IPC caller cannot distinguish an adopted preparation from an ignored one using this reply, and its headroom values may describe a curve absent from the current mailbox. This issue concerns result reporting, not failure of the sequence guard or clipping/headroom math.

The desktop increases edit sequences and ignores obsolete sequence replies. No existing source evidence establishes a user-visible desktop volume/EQ error, audible mismatch, or receiver problem caused by this case. Matching stored state can also exist during setup or after a worker finishes; worker-liveness reporting is outside this focused issue. ([SessionController.cs:38–65](../../desktop/AirFlash.Core/SessionController.cs#L38-L65), [320–324](../../desktop/AirFlash.Core/SessionController.cs#L320-L324), [main.rs:154–171](../../native/airflash-engine/src/main.rs#L154-L171).)

## 2. Bounded root cause

The mailbox's mutation condition and command success are disconnected. `Control::set(sequence, prepared)` silently returns for `sequence <= latest.0`, but `main.rs` unconditionally returns `Ok(prepared)` afterward. `equalizer_changed` fields are derived from that return value, not from the retained mailbox. No audio-application completion acknowledgment exists, and this report does not propose turning preparation success into one. ([equalizer.rs:124–145](../../native/airflash-engine/src/equalizer.rs#L124-L145), [main.rs:195–201](../../native/airflash-engine/src/main.rs#L195-L201), [live.rs:120–122](../../native/airflash-engine/src/live.rs#L120-L122).)

## 3. Proposed reproduction — not executed

Use an isolated command-handler/control fixture with a valid stored start control slot. Factor or inject that slot for tests rather than invoking a real `start` handshake.

1. Prepare curve A: enabled, flat bands, preamp 0 dB; store it with sequence 10.
2. Submit sequence 9 with curve B: enabled, flat bands, preamp −12 dB, through the equalizer handler.
3. Inspect the response and retained mailbox. Repeat with sequence 10 and curve B.
4. Submit sequence 10 with curve A to test an identical idempotent duplicate, then sequence 11 with curve B to test a genuinely newer update.

**Source-predicted current result:** steps 2 and 3 return `equalizer_changed` with B's effective preamp −12 dB while the mailbox retains sequence 10/A, whose effective preamp is 0 dB. Sequence 11 adopts B. These are predictions from control flow, not captured engine output. The fixture should test the handler's envelope/headroom result and mailbox state together; calling the mailbox alone cannot verify the reporting defect.

## 4. Proposed change — unimplemented

Have mailbox mutation return an explicit outcome and the relevant stored preparation. Distinguish a newly accepted sequence, an identical idempotent duplicate, a stale sequence, and a same-sequence conflicting preparation. Derive response metadata from the outcome rather than unconditionally from submitted values.

An identical duplicate may remain successful and idempotent so benign replay does not create a spurious desktop editor error. A stale/conflicting update should produce a distinguishable nonfatal result, for example the existing `equalizer_error` event with an explanatory message, without changing the current mailbox. Alternatively, an optional acceptance/outcome field can clarify semantics, but its JSONL v1 compatibility and older-reader behavior must be specified. A generic success event must not imply that an ignored different curve replaced the retained preparation.

Keep the distinction between preparation adoption and later audio pickup/crossfade; do not wait for the 20 ms fade on the stdin or media thread to issue a preparation reply. Preserve `try_lock` pickup, valid supported rates/gains, session correlation, positive-sequence checks, nonfatal rejection, and the latest-value guard. No schema 2 settings change, receiver command, authentication-mode change, credential change, or local mute behavior is required. Exact-bit restore and pair-verify/transient mutual exclusion remain invariants.

## 5. Testable acceptance criteria

- A newer valid sequence updates the mailbox and returns metadata for that accepted preparation.
- An older sequence cannot mutate the mailbox and receives a result distinguishable from acceptance of its submitted curve.
- An equal sequence with conflicting preparation cannot mutate the mailbox or report its different headroom as newly adopted.
- An identical equal-sequence replay is explicitly idempotent under the chosen contract; desktop sends during startup/preview coalescing do not gain misleading errors.
- Malformed payloads, invalid gains, nonpositive sequences, wrong sessions, and probe/pair control slots retain nonfatal equalizer rejection semantics.
- Rejected commands leave the current preparation and ongoing playback unchanged. A reply remains preparation-level information and never claims completed audio application.
- Request/session envelope handling and JSONL v1 compatibility remain covered; a subsequently valid command still succeeds after a rejected stale/conflicting command.

## 6. Existing coverage and verification plan

[equalizer.rs:353–372](../../native/airflash-engine/src/equalizer.rs#L353-L372) checks invalid settings and that an old mailbox update leaves the newer sequence intact. It submits the same prepared value for both sequences, and checks no command reply. [equalizer_ipc.rs:8–65](../../native/airflash-engine/tests/equalizer_ipc.rs#L8-L65) checks malformed/nonactive updates and enabled-probe rejection, but has no successful live control slot. [SessionTests.cs:77–88](../../desktop/AirFlash.Tests/SessionTests.cs#L77-L88) uses fake replies to verify obsolete-sequence filtering and nonfatal feedback. These test sources were inspected, not executed for this issue review.

Add the paired handler/mailbox cases above, including distinct headroom values and same-sequence conflict versus identical replay. No capture device or network handshake is required. If separately authorized hardware qualification is later used, finite probes remain at sender gain at most 0.1 and duration at most five seconds, with EQ disabled as required; a long receiver stream is unnecessary to demonstrate a preparation-result contract gap.
