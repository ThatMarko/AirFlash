# Group B feature acceptance

Verified on 2026-10-07. This records a tested implementation branch, **not behavior integrated into original stable main**. Original `upstream/main` was refreshed before implementation and again after the local commits; it remains `41190e0d13a63a714c08dffe73ababca1804875c`.

- Branch: `codex/fix-session-lifecycle`, based directly on original stable main.
- Core contract/selection commit: `46627962d8df6c476eb98c39da7a9d561a2a32e9`.
- Application integration/final head: `342aeb77cb85ff6f3f7f850161b3db79d8637945`.
- Integration: the tested feature is included in local fork `main` at `72dc7c7df6649a95271e0a86ae62f371397b2ca4`, after Group A's merge `ca84c7c`. Original upstream `main` has not merged Group B.
- Publication: [issue #14](https://github.com/Ding-Kyoma/AirFlash/issues/14) and ready-for-review [PR #15](https://github.com/Ding-Kyoma/AirFlash/pull/15) were submitted upstream on 2026-10-07 after explicit user authorization. Branch push and hosted validation are tracked separately in [combined fork validation](FORK-INTEGRATION-ACCEPTANCE.md).
- Review: independent source and regression-assertion review completed with no actionable findings. Its required automatic-save-failure fixture was added and reviewed.

Implementation source at the tested feature head: [session ownership/admission](https://github.com/ThatMarko/AirFlash/blob/342aeb77cb85ff6f3f7f850161b3db79d8637945/desktop/AirFlash.Core/SessionController.cs), [receiver selection](https://github.com/ThatMarko/AirFlash/blob/342aeb77cb85ff6f3f7f850161b3db79d8637945/desktop/AirFlash.Core/ReceiverSelection.cs), [application integration](https://github.com/ThatMarko/AirFlash/blob/342aeb77cb85ff6f3f7f850161b3db79d8637945/desktop/AirFlash.App/ViewModels/AppViewModel.cs), and [isolated WPF lifecycle fixtures](https://github.com/ThatMarko/AirFlash/blob/342aeb77cb85ff6f3f7f850161b3db79d8637945/desktop/AirFlash.App/Verification/UiSessionLifecycle.cs). These files are separate from each report's pinned original-source evidence.

## Separate report acceptance

| Report | Accepted behavior and executed regression evidence |
| --- | --- |
| 01 | Atomic snapshot/desktop-owner capture; serialized guarded retargeting. Delayed identity saves cannot overwrite newer Pair, including changed/absent old receiver and same-id replacement. Metrics and native retries retain the owner. Save failure preserves catalog/settings; legitimate promotion remains valid and a changed effective latency restarts exactly once. |
| 02 | Automatic start has separate owner/inactive/intent admission. Explicit Play, Pair, Stop and shutdown invalidate older intent before awaiting. Eligibility is rechecked after persistence while holding the app settings gate through controller admission; intent is checked again after awaited cleanup. Tests cover Pair/Streaming/Stop, Stop waiting for serialization, same-id replacement, queued requests, unchanged-owner removal/incompleteness/hide/disable, Idle/Error admission, save failure/retry, admitted failure accounting, and preserved valid saves/Stop suppression. |
| 03 | Owned Connecting, Pairing, Streaming and Standby survive empty, partial, overfull and repeated discovery publications. The full active transport stays separate from unavailable catalog data. A retained visible card has an actual bound Stop button and availability label. Reappearance, pending PIN cancellation, handshake/pair terminal faults, retry with a fresh native UUID, shutdown and both original mute bits are exercised. Adapter browse restart retains ownership; discovery still follows the selected interface without fallback. New unavailable/incomplete starts remain rejected. |
| 10 | Attempted receivers are excluded before last-used/name ranking. Tests cover failed preferred A followed by B, all-attempted no-op, option precedence, aliases, permitted offline/incomplete transition resets, and global off-to-on reset. Rejected automatic requests do not consume the candidate. |
| 11 | Manual selection preserves its exact host, port, id and preferences alongside complete, incomplete or hidden groups, at matching and differing ports. Explicit groups stay selected. Discovered members route through a unique confirmed canonical/broadcast-alias owner; ambiguous ownership rejects. Address-only fallback was deliberately omitted. Actual fake command peers, gain, latency and last-used are asserted; credential namespaces are unchanged. |

## Actual verification

- Locked managed restore with selected repository-local .NET SDK `10.0.401`, resolving the unchanged desktop `global.json` policy.
- Focused new Core tests: **55 passed**, zero failures or skips.
- Owning Core suite after final fixture revision: **218 passed**, zero failures or skips.
- Desktop solution Debug verification build and ordinary Release build: **zero warnings/errors**.
- Separate WPF `--ui-smoke`: **273 checks passed in English and 273 in Chinese**. Both retained-card screenshots were visually inspected; unavailable discovery text and Stop control fit correctly.
- Seven signal-gated original-base observations reproduced the reported defects. All seven desired assertions failed on original behavior. Runtime source blob hashes were unchanged; only the isolated verification entry/fixture differed. The temporary managed comparison worktree was archived after evidence preservation.
- Complete final code diff and both outgoing implementation commits contain only the intended ten desktop implementation/test files; no fork analysis/issues/planning documents are included. The implementation checkout is clean.

Evidence is kept in the implementation worktree's ignored `artifacts/group-b-test-results/`, `artifacts/group-b-ui-en/`, `artifacts/group-b-ui-zh/`, and `artifacts/group-b-baseline/`. These local receipts are not public issue attachments.

## Compatibility and limits

Config schema 2, JSONL v1, independent manual persistence, selected-adapter no-fallback, authentication-method exclusivity, accessory `/info deviceID` credential keys, and exact prior mute-bit restoration are preserved. No native, discovery-service or production audio-service source changed. No real config, credentials, receiver, capture or endpoint mutation was used; the UI harness's tray/window registrations are isolated test identities. No hardware/acoustic reliability or physical OS mute guarantee follows from these mocks.

## Upstream Task 3 publication

PR #15 targets original stable `41190e0d13a63a714c08dffe73ababca1804875c` from `ThatMarko:codex/fix-session-lifecycle` at the unchanged tested head `342aeb77cb85ff6f3f7f850161b3db79d8637945`. Canonical issue/PR searches found no existing report covering these reproduced ownership/selection cases. The separate HomePod incident reports remain unproven as to cause; this issue does not claim to resolve them. The ten-file diff and both outgoing commits exclude Group A, scrolling, installer and fork planning changes. The issue/PR bodies, branch identities and `Fixes #14` closing reference were read back and verified; the PR is attached to the Codex task.

At publication, [upstream CI run 37717476182](https://github.com/Ding-Kyoma/AirFlash/actions/runs/37717476182) reported `action_required` and explicitly awaited maintainer approval, with no jobs started. No review had been submitted. The next upstream action is maintainer approval of the fork workflow and review. Completed build/download validation is recorded in [combined fork validation](FORK-INTEGRATION-ACCEPTANCE.md) and the [final A+B+scrolling build](SETTINGS-SCROLL-ACCEPTANCE.md#final-abscrolling-build). The fork integration does not make Group A an accepted upstream dependency; each feature branch retains its original-base implementation boundary. No upstream merge occurred.
