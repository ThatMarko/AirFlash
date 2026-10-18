# [LOW] Equalizer retained-preparation replies — resolved in fork main

**Status:** Implemented and integrated in own-fork main `8989b3609dd1294f4d1d6eb1502f345cc22bc382`, reviewed 2026-10-12. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`; this implementation is not yet integrated there. Severity is the original triage priority.

**Group:** F — JSONL control contracts. **Feature head:** `421e1ea8159803fbc339dff3782b1858e8a797d4`. [Plan](IMPLEMENTATION_PLAN.md#issue-14), [acceptance](GROUP-F-ACCEPTANCE.md), [EQ map](../analysis/equalizer.md).

## Publication status — 2026-10-18

Published with process-owned IPC routing as [issue #26](https://github.com/Ding-Kyoma/AirFlash/issues/26) and [PR #27](https://github.com/Ding-Kyoma/AirFlash/pull/27). Both remain open; the PR is unmerged at current standalone head `28cb83e55eda2e4b50847aafd783e75244697d23`. Earlier feature/main hashes, source links and verification counts below retain their recorded provenance; [F acceptance](GROUP-F-ACCEPTANCE.md) records later qualification separately.

## Current behavior and source trace

[Control::set](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/equalizer.rs#L151) returns Accepted/Idempotent/Stale/Conflict plus retained sequence/Prepared. Replay identity is equality of **full Prepared**, including coefficients, gain, bypass and both headroom fields.

[equalizer_event](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/main.rs#L59) serializes retained metadata for accepted/newer or identical equal-sequence replay. Older/conflicting requests return compatible nonfatal equalizer_error with request sequence and leave the mailbox intact. Positive sequence/matching stored start slot/valid settings remain required; probe/Pair/wrong session reject without destroying later valid control.

A matched stored slot is not a worker-liveness contract. A reply reports preparation adoption/idempotency, not audio pickup or crossfade completion. Packet try_lock/update, latest-value sequence guard and 20 ms fade remain separate.

## Acceptance and evidence

- Newer valid request adopts and reports retained preparation.
- Stale/same-sequence conflict cannot mutate or falsely report submitted headroom as adopted.
- Identical full-preparation replay succeeds idempotently.
- Invalid settings/shape/nonpositive sequence/wrong slot remain nonfatal; a later valid command recovers.

[production handler serialization fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/main.rs#L426) assert serialized response **together with retained mailbox** at both rates, using distinct curves/headroom. [mailbox outcomes](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/equalizer.rs#L406) verifies full preparation equality. The separately retained submitted-metadata mutation fails, as recorded in acceptance.

The separate inherited desktop EQ CI failure (expected 9/actual 4) was investigated rather than accepted after a green rerun: quiet-read-timeout mechanism reproduced under unchanged runtime, healthy fixture now gates actual send/cancellation, and a separate expired-connection fixture verifies preview is cached for next start. The exact original CI timing cause remains inferred; no production timeout change was used.

Exact-main CI passed 667 Core, 124 Python and 69 native tests (recording-device qualification and thirty-minute localhost soak ignored). Packaged integration at `4ae1db4` passed six 1,059-assertion UI runs, three actual 125% DPI runs with 40 each and disposable MSI lifecycle; current runtime/package inputs are identical, with main's last commit documentation only. This refresh reviewed sources/records; it did not rerun suites or use real user data/devices.

## Remaining work

The published upstream PR awaits maintainer review and integration. No acoustic mismatch, hardware cause, completed DSP/receiver response or arbitrary-input no-clipping claim is made.

## Historical finding

Original `41190e0` retained newer mailbox state for an ignored older/duplicate request but unconditionally returned submitted preparation success/headroom. The defect was the result contract, not failure of the sequence guard or headroom math. Original reproduction was source-derived before controlled execution. The [archival report at the prior documentation commit](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/issues/ISSUE-14-LOW-equalizer-ignored-update-success-metadata.md) preserves original `41190e0` source anchors, reproduction/proposal and dated updates. Its proposal-only/open/pending-reboot labels are historical, superseded by the current resolution above.