# [MEDIUM] Process-owned IPC rejection routing — resolved in fork main

## Current-source boundary — 2026-10-21

Original process-owned terminal IPC routing is upstream through PR #27 and maintainer PR #33 on stable `439de626`. Combined `6c1e2ef` fixes the remaining hello, capabilities and application-catalog consumers and handles terminal events during pairing input. Original native routing is retained, not presented as a missing merge.

[Current source, acceptance and executed limits](PR-READINESS-2026-10-21.md) supersede older current/open/schema labels below. The remaining source links and test counts retain their recorded historical baselines; this documentation refresh claims no new test execution or upstream publication.

**Status:** Implemented and integrated in own-fork main `8989b3609dd1294f4d1d6eb1502f345cc22bc382`, reviewed 2026-10-12. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`; this implementation is not yet integrated there. Severity is the original triage priority, not an unqualified current hardware failure.

**Group:** F — JSONL control contracts. **Feature head:** `421e1ea8159803fbc339dff3782b1858e8a797d4`. [Plan](IMPLEMENTATION_PLAN.md#issue-12), [acceptance](GROUP-F-ACCEPTANCE.md), [IPC map](../analysis/credentials-and-ipc.md), [failures](../analysis/failures.md).

## Publication status — 2026-10-18

Published with EQ preparation replies as [issue #26](https://github.com/Ding-Kyoma/AirFlash/issues/26) and [PR #27](https://github.com/Ding-Kyoma/AirFlash/pull/27). Both remain open; the PR is unmerged at current standalone head `28cb83e55eda2e4b50847aafd783e75244697d23`. Earlier feature/main hashes, source links and verification counts below retain their recorded provenance; [F acceptance](GROUP-F-ACCEPTANCE.md) records later qualification separately.

## Current behavior and source trace

[native envelope](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/main.rs#L22) stamps rejected malformed/oversized command envelopes as v1 error/command_error/ipc/retryable false/null host/explicit empty id and session_id. [IsProcessRejection](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/EngineEvents.cs#L8) recognizes only that exact shape, before [ordinary session filtering](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs#L625) and in [startup hello](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/EngineHandshake.cs#L6).

The owning startup/playback/Pair process surfaces the original detail and final classification promptly; force reconnect cannot convert it into a timeout retry. Stale ordinary events and near matches stay filtered within the original budget. Malformed/non-object output becomes a controlled final protocol_error/ipc. Nonfatal EQ errors remain separate.

The asymmetric limits remain: native command guard counts 65,536 bytes including newline, native event output has no cap, desktop checks 65,536 UTF-16 characters after full line allocation and has no outgoing limit. General pipe allocation/size hardening is outside this fix.

## Acceptance and evidence

- Exact startup/connecting/streaming/Pair rejection preserves message/code/channel/false retryability.
- Force reconnect does not reopen a final IPC failure.
- Near-match/stale/version events, cancellation and cleanup retain their normal behavior.
- Malformed output reports controlled protocol failure; native rejection leaves its command loop responsive to a later valid hello.

[fake-process fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/IpcFailureTests.cs#L21) and [receiver-free child parser fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/tests/equalizer_ipc.rs#L9) assert outcomes/disposal and later recovery. No ordinary UI command is claimed routinely oversized; this is error-routing correctness, not evidence of a hardware root cause.

Exact-main CI passed 667 Core, 124 Python and 69 native tests (recording-device qualification and thirty-minute localhost soak ignored). Packaged integration at `4ae1db4` passed six 1,059-assertion UI runs, three actual 125% DPI runs with 40 each and disposable MSI lifecycle; current runtime/package inputs are identical, with main's last commit documentation only. This refresh reviewed sources/records; it did not rerun suites or use real user data/devices.

## Remaining work

The published upstream PR awaits maintainer review and integration. Worker liveness and generalized bounded desktop reads remain outside the implemented group.

## Historical finding

At original `41190e0`, empty-correlated rejection failed the current-session filter. It could be hidden by later metrics or become a substituted host timeout/retry. Native envelope plus desktop filtering established the conditional source defect; original reports did not measure production trigger frequency. The [archival report at the prior documentation commit](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/issues/ISSUE-12-MEDIUM-uncorrelated-ipc-errors-become-timeouts.md) retains the original `41190e0` source links, source-derived reproduction/proposal and separately dated implementation updates. Its open/proposal-only/pending-reboot labels are historical; current acceptance is above.