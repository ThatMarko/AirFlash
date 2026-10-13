# Group F acceptance — truthful JSONL errors and EQ preparation

## Publication and current fork integration — 2026-10-13

Explicitly approved and published: [issue #26](https://github.com/Ding-Kyoma/AirFlash/issues/26) and [PR #27](https://github.com/Ding-Kyoma/AirFlash/pull/27). The PR is open and ready for review, not merged. It targets original `main` from `ThatMarko:codex/fix-jsonl-control`, exact head `421e1ea8159803fbc339dff3782b1858e8a797d4`, verified base `cf420d97700463c56adc31408510e8336e99e51c`. GitHub CLI verifies exact titles/bodies, three outgoing commits and the ten-file managed/native implementation/test diff. Every outgoing commit and the complete diff exclude fork-only analysis/issues/planning. Independent final native, desktop, regression/evidence and publication-text reviews are clear.

Fresh prepublication standalone Release Core qualification passed **239 tests**, zero failures/skips, using selected SDK **10.0.401** from `desktop` with locked restore. Recorded standalone Release build passed with zero warnings/errors; the three source-bound isolated WPF runs passed **568 assertions each**, English/en-US, Simplified Chinese/de-DE and Traditional Chinese/de-DE. They were not rerun during this publication-only step. [Exact-source Windows run 38359129170](https://github.com/ThatMarko/AirFlash/actions/runs/38359129170) checked out this feature head separately from workflow revision `f181e12057c02d18ce0a49455fbcd658e023bc82`: **53 native passes, two existing ignores, 239 Core passes**, Clippy, Release/static-CRT and executable compatibility audit all passed. The ignored tests are recording-endpoint qualification and the thirty-minute localhost soak. The compatibility audit remains static evidence, not physical Windows 10 qualification.

[Upstream PR CI 38455062214](https://github.com/Ding-Kyoma/AirFlash/actions/runs/38455062214) **failed managed locked restore before Core tests**: inherited floating `10.0.x` setup installed SDK **10.0.402**, whose implicit `Microsoft.NET.ILLink.Tasks` request is **10.0.13**, versus the unchanged lock's **10.0.12** (`NU1004`). Python **96** and native **53** passed (two existing ignores); discovery/package checks were skipped. CI workflow, `global.json`, application project and lock file are byte-identical to original base; the shared mismatch is source-confirmed, while unchanged-base execution with SDK 10.0.402 has not been performed. A shared build dependency correction and complete new CI run remain necessary; no unchanged rerun or unlocked restore is substituted for a repair. The inherited action Node.js deprecation annotation is a separate nonblocking warning.

The publication receipt, failed upstream log and raw fresh Core execution are retained in ignored `artifacts/group-f-publication-2026-10-13`. Both this package and the other fixes are integrated into qualified fork code `892a458078930b7b25d973d773e0a93e356ef36b`; later documentation-only Main commits do not change that tested code. Publication does not establish integration into original stable Main.

The exact process-owned rejection is event `error`, code `command_error`, channel `ipc`, numeric version 1, explicit empty request/session strings, null host and false retryability. Pair coverage injects it before a PIN prompt; the reader remains paused while an open prompt is awaited. Native EQ handler cases use a synthetic stored streaming slot with an EQ mailbox, not a receiver worker. Preparation acceptance does not establish DSP completion or worker health. The inherited quiet-stream regression was separated from healthy-owner/send-cancellation coverage without changing production timeout/debounce/authentication policy; its historical cause remains qualified as inferred below.

## Current Main integration — 2026-10-12

Reviewed fork runtime baseline `8989b3609dd1294f4d1d6eb1502f345cc22bc382` contains this feature; desktop/native/tooling trees match qualified code `4ae1db4f01dcf7857482dd136ea8d9dffa3aa8a1`. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`. The public branch is pushed to origin and its complete diff/every outgoing commit exclude private planning. [Combined acceptance](CONTINUATION-ACCEPTANCE.md) binds final 667 Core, 124 Python, 69 native, portable UI/DPI and MSI results. No upstream publication is authorized by this documentation update.

Feature `codex/fix-jsonl-control`, head `421e1ea8159803fbc339dff3782b1858e8a797d4`; worktree `fix-jsonl-control/AirFlash`. Reports 12/14 are complete. Independent code/fixture/receipt review is clear. Current owning Core suite passes 239 cases; 13 narrow Debug cases pass and Release solution build has zero warnings/errors. Runtime predecessor `9634007` passed 76 focused/238 owning cases; newer head corrects fixtures and a native replay comment, not runtime policy.

Current source: [controller](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs), [process connection](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/EngineConnection.cs), [command handler](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/main.rs), [equalizer mailbox](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/equalizer.rs) and [controller assertions](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/SessionTests.cs).

## Process-owned IPC failure

Only the defined v1 `error`/`command_error`/`ipc` envelope with explicit empty request/session strings, null host and retryable false is recognized from the owning process before ordinary filtering. Hello/Connecting/Streaming/Pairing retain original structured detail and nonretryable classification. Force reconnect cannot turn it into an unrelated timeout retry. Arbitrary stale nonempty ids and near matches remain filtered; nonfatal EQ feedback remains nonfatal. Obsolete generations cannot record/publish faults.

Malformed/non-object output creates a controlled nonretryable protocol failure without changing general reader size/allocation policy. Native command rejection leaves the command loop alive; the desktop attempt's cleanup/lifetime is distinct.

New 36-case Core fixture against unchanged stable runtime: 15 desired failures/21 controls. Fixed historical `0ce7139`: all 36/235 owning Core pass with warning-free build. The intermediate pairing fixture incorrectly assumed playback's two restoration calls; correcting its phase-specific expectation required no runtime change. Failed TRX files remain in `artifacts/group-f-2026-10-11/`.

Receiver-free native real-child fixtures prove the 65,536-byte input budget includes LF, reject oversized ASCII/UTF-8 by bytes, preserve empty-correlated rejection fields and accept a subsequent valid hello/EOF. They start no receiver or capture worker.

## Truthful EQ preparation

Mailbox mutation atomically returns Accepted/Idempotent/Stale/Conflict and retained sequence/preparation. Prepared-value equality includes **gain, bypass, coefficients and headroom**. Newer/identical updates use compatible `equalizer_changed`; stale/conflicting requests retain state and emit compatible nonfatal `equalizer_error`. Successful replies carry headroom metadata from retained preparation adoption, not DSP pickup/fade completion. Rejected EQ replies echo the submitted sequence and error message without headroom fields while preserving the mailbox.

Both rates cover serialized production replies and mailbox state together for 10/A, 9/B, 10/B, 10/A, 11/B. Invalid gains/structure/sequence, wrong/absent sessions, pair/probe slots and subsequent recovery remain covered. Preserved mutation `codex/verify-eq-reply-regression` `78649b3` restores old response selection; [run 38165179020](https://github.com/ThatMarko/AirFlash/actions/runs/38165179020) fails the decisive serialized EQ assertion after 27 library controls. It is source-equivalent, not literal old source, and must never merge.

## Additional inherited EQ fixture correction — 2026-10-12

Main `107174a` [normal CI 38292659589](https://github.com/ThatMarko/AirFlash/actions/runs/38292659589) failed expected gain 9/actual 4 while its Release integration passed 666 Core. That green was not a repair. Original and refreshed sources have the same fixture/controller methods; its quiet fake used a one-second read deadline.

Control `codex/verify-eq-read-deadline` at `4a444f0f9ed4423e8d846c9b3e79f3669acf7c56` waits for actual timeout/Error and disposed transport after rollback 4, then deterministically fails original gain-9 expectation with unchanged runtime. This proves the mechanism. Original CI captured no state, so its exact cause remains **inferred**, not conclusively reproduced.

Corrected healthy fixture uses the ordinary finite production read budget, gates gain-7 send entry and cancellation before delivery, releases the gate and clears BeforeSend in finally. The healthy path joins the pending send and asserts Streaming, no stale wire delivery, old-owner command-count invariance and exact 9→4 rollback. Separate deadline fixture verifies original diagnostic/disposed transport, no new old-process commands and preview 9 in a fresh start payload. No production timeout/debounce/authentication policy changed.

Initial new fixture compared decorated snapshot text rather than original fault detail; corrected field assertion and failed receipt remain separate. Final Main `4ae1db4` passed 667 Core Debug/Release, normal CI and complete package integration. Exact current Main `8989b36` also passes CI; source trees are identical.

## Hosted evidence and limits

Historical [run 38165067327](https://github.com/ThatMarko/AirFlash/actions/runs/38165067327) at `0ce7139` passed 51 native/235 Core, clippy/static-CRT/compatibility and warning-free build. Receipts under `ci-f-0ce7139/` explicitly identify tested feature SHA; mutation logs under `ci-f-mutation-78649b3/`. Final Main native checks include current upstream recording source separately.

All fixtures are synthetic and isolated; no real receiver/capture/credentials/endpoint or local MSI. Native qualification is hosted. F preserves JSONL v1 and asymmetric existing IPC limits. It is integrated into fork Main; issue #26/PR #27 are published and open, with original stable integration pending.
