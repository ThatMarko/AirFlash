# Manual source review of the reliability PR

The review covers all 103 changed paths against original Main `a52803dac1696a56b7067356e09bd7e7ebb9dab6`, including added fixtures and deleted code. The reviewers read the changed lines, followed their callers and consumers, and recorded how each change behaves when work overlaps, fails, or is cancelled. The final source is `be2c98209292ff9e2fe777988ac220f142bea368` on `codex/reliability-refresh-v062`. [Final upstream comparison](https://github.com/Ding-Kyoma/AirFlash/compare/a52803dac1696a56b7067356e09bd7e7ebb9dab6...ThatMarko:codex/reliability-refresh-v062)

## Corrections from this review

- **Quit now continues cleanup after an admission notification fails.** `BeginShutdown` closes admission, invalidates intent, stops timers and refreshes receiver rows. A throwing notification subscriber could escape before the shutdown helper joined Apply or disposed the controller and audio service. The helper now captures that exception, attempts the remaining stages in order, then surfaces the original exception or aggregates it with later failures. [Shutdown helper](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/desktop/AirFlash.App/Services/ShellShutdown.cs#L9)
- **The restoration fixture releases its gate if handoff fails.** All five callers await the helper before entering their own cleanup blocks. The helper now releases its final gate if the wait or assertion throws, allowing the worker to finish restoration and the original assertion to be reported. [Restoration fixture](https://github.com/ThatMarko/AirFlash/blob/be2c98209292ff9e2fe777988ac220f142bea368/desktop/AirFlash.Tests/SessionLifecycleTests.cs#L1048)

## How the changes propagate

| Area | Source path followed |
| --- | --- |
| Build and release | Host selection flows into the desktop SDK resolver and later invocation. Numeric version bounds apply before reservation. The producer, verifier and preview upload share the artifact name. |
| Session and receiver intent | Play, Pair, Stop and discovery capture the appropriate intent or owner. Settings persistence and live application have separate authority. Controller serialization guards the transport and keeps the owned Stop control visible. |
| Settings recovery | Loaded group IDs pass validation before consumers dereference them. Invalid input enters the existing original-byte recovery path. |
| DNS | Browse results create logically timed resolves. Adapter ownership settles separately, and stale results stop at the token boundary. Only current resolved data reaches catalog reconciliation. |
| Pairing and credentials | Input validation precedes JSONL submission. Lookup preserves file access and decoding errors. Authentication selects one path using the accessory identity. |
| IPC | Consumers recognize a process-owned terminal error before applying session filters. Pairing joins the prompt and reader before surfacing failure. |
| Local mute | The owner lease and saved endpoint bit precede mutation. Cleanup attempts restoration and retains unresolved records for the recovery window. |
| Clock, capture and UI | Grant validation controls subscription leases. Clock exchanges control readiness and timeline recovery. Capture padding counts travel through optional JSONL fields to Monitor. Layout and dismissal changes retain the existing control commands. |

The detailed reviews are in `artifacts/manual-source-review-2026-11-05` in the fork documentation checkout. `lead.md`, `managed.md`, `dns-mute.md` and `native-ipc.md` record the line ranges, rationale, affected consumers and failure paths. `index.json` ties the 103 paths and 407 three-context diff hunks to the reviewed source. The DNS notes also provide a finer zero-context index. The three files changed by the final correction received independent source review.

GitHub supplied the comparison and the open recovery proposal. Exa and Context7 supplied platform contracts. The reviewers traced application behavior in the repository. [Recovery PR #45](https://github.com/Ding-Kyoma/AirFlash/pull/45) · [WPF scroll template parts](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/scrollviewer-styles-and-templates) · [Content aliasing](https://learn.microsoft.com/en-us/dotnet/api/system.windows.controls.contentpresenter.contentsource?view=windowsdesktop-10.0) · [Process wait semantics](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process.kill?view=net-10.0)

The focused check for the two corrections completed cleanly on a rerun. The first execution reported passing assertions and then exited with a test adapter disposal exception. Broader qualification recorded in the earlier report applies to `367dbb3`.

The 39 outgoing commits preserve the publication boundary for fork-only documents. The PR draft is ready for user review. Upstream publication remains subject to the user's explicit authorization.
