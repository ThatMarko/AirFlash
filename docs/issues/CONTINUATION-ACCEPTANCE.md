# Continuation acceptance — 2026-10-11

This dated record supersedes the earlier pending-reboot and unpushed-branch statements. Historical source audits and receipts remain unchanged. Windows boot was verified at 06:59 on October 11; the deferred execution gate is satisfied. Both remotes were fetched: original stable remains `d4aa0686fc411de6655aac313243f0f6bf71e439`. A PR #11 and scrolling PR #13 are merged; B PR #15 remains closed without merge at frozen `11c6046`, with issue #14 open. No upstream publication, reopening, merge or release is authorized by this continuation.

## Discovery cancellation fixture

The failing run `38134996290` is preserved; documentation-only green run `38135036967` was never treated as its repair. The original fixture allowed the concurrent browse callback to drain deferred work and synchronously wait for its own return. A test-only outer publication recursion forced that schedule and reproduced the exact circular-wait assertion on unchanged production source.

`68cae6f` separates fake cancellation request from the native join and controls both drain winners. Additional assertions execute the real unmanaged-owner adapter with injected native calls and retained input buffers. Removing native callback deferral fails the concurrent-drainer case; removing publication deferral fails both coordinator cases. Both temporary mutations were restored byte-for-byte. No production deadlock was established and no production cancellation change was needed.

The branch was normally merged with current upstream and gained five Traditional Chinese diagnostics in `0b2079479457b7fdbf070dc712de08ee606c3465`. It is pushed to the user's fork and integrated locally. Standalone current-base checks: 237 Core cases, 84 focused discovery/localization cases after translation, Release build with zero warnings/errors. Fresh independent code/assertion review is clear. Complete feature diff and every outgoing commit exclude fork-only planning. Evidence: implementation worktree `artifacts/cancellation-fixture-2026-10-11/`, including failed original, mutation and corrected TRX/source receipts.

## Explicit user intent

An explicit Play delayed by saving, or a Start/Pair queued behind controller serialization/cleanup, could acquire a new intent stamp later and supersede a newer Stop/Pair. Twelve controlled Core cases failed before `fc5219c`; the real view-model save/Pair regression also failed. The fix carries one action token from entry through admission and cleanup. Rejected work does not update attempted/last-used state or clear newer Stop suppression.

Independent review found a second post-admission notice race. A gated WPF comparison failed before the guard: an older admitted Play continuation cleared a newer Pair notice. Notice clearing now requires the same current intent, while bookkeeping still records an admitted attempt. The final fixture checks newer Pair before PIN prompting and the actual bound row Stop, exact owner/state/process count, notice, selection and suppression. A fixture-only intermediate failure checked the view-model before its queued Idle publication; awaiting that publication corrected the assertion. Failed reports remain preserved.

Current feature head `87e1f3658f22addcd6bc7a4e85db0f946c5fb6a7` includes a normal current-upstream merge, is pushed to the fork and integrated locally. Standalone current-base Core: 267 passed; Release build zero warnings/errors. Before stable refresh, final crossed English/Simplified-Chinese WPF runs each passed 324 assertions. Fresh independent final review is clear; feature diff/outgoing commits exclude fork-only planning. Evidence: implementation worktree `artifacts/explicit-intent-2026-10-11/`, including both decisive failures and superseding receipts. The closed upstream PR has not been reopened or replaced.

## Integrated qualification

Runtime source `a4661025269bee01213035bf5ad9a55cc80bcc2e` combines these fixes with preserved A/B/C/D, scrolling, installer refresh and current upstream. Actual local checks:

| Check | Result |
| --- | --- |
| Selected desktop SDK | 10.0.401 resolves 10.0.100/latestFeature |
| Unchanged starting main Core/Python | 422 Core; 86 Python passed |
| Corrected integrated Core | 437 passed; zero failures/skips |
| Corrected integrated Release solution build | Zero warnings/errors |
| Regional en-US + UI en-US / zh-CN / zh-TW | 603 / 595 / 595 assertions passed |
| Regional zh-CN + UI en-US / zh-CN / zh-TW | 603 / 595 / 595 assertions passed |

The six final runs use the actual `--ui-language` switch and independently verify regional and catalog choices. An earlier trial used an unrecognized `--language` argument; its six reports were all English catalog and are not Chinese evidence. All raw reports remain in fork `artifacts/continuation-2026-10-11/`. Existing cross-group and scrolling assertions remain in the final harness. A/C/scrolling source review found no further confirmed defect; their owning fixtures execute in this combined qualification.

Local native Cargo/MSVC/Windows SDK prerequisites are absent. An official SHA256-verified repository-local rustfmt/compiler-driver package supports formatting only; it is not a native build qualification. Exact-source own-fork Windows validation is required for native changes. The new fork-only group workflow checks a full requested commit, runs native/managed checks and isolated synthetic modal/clipboard fixtures on its disposable runner, preserves results, and does not reserve versions or publish releases. Hosted continuation results are pending.

E is in progress on separate current-upstream branch `codex/fix-auth-input`: the unchanged modal PIN return failed the real fake `pair_pin` assertion; normalized input passes 19 Core cases and the initial 149-assertion modal fixture. Additional typing/state/paste/failure coverage and native qualification remain pending. F/G/H are not implemented by this record.

All local fixtures use temporary data, synthetic identities and fake engine/audio/discovery. No real receiver, credential store, user configuration, capture, endpoint mute or desktop MSI operation was used. Genuine clipboard paste is limited to an explicit disposable-runner fixture. Mock success does not qualify HomePod reliability, OS cancellation delivery or physical crash durability.
