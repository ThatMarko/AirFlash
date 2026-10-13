# Group B acceptance — session ownership and receiver selection

## Current Main integration — 2026-10-12

Reviewed fork runtime baseline is `8989b3609dd1294f4d1d6eb1502f345cc22bc382`, with source trees equal to qualified code `4ae1db4f01dcf7857482dd136ea8d9dffa3aa8a1`. The feature head below is an ancestor of Main and is pushed to origin. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`. Public complete diffs and every outgoing commit exclude private planning. This is fork implementation acceptance, not upstream integration. [Combined Main acceptance](CONTINUATION-ACCEPTANCE.md) records 667 Core/124 Python/native/package/UI qualification and limits.

Feature `codex/fix-session-lifecycle`, head `442e8d8c8fd7649e3e039ec356e3a4f4b25f6d0b`; worktree `fix-session-lifecycle/AirFlash`. Reports 01, 02, 03, 10 and 11 are complete. Independent source and regression-assertion review is clear. Standalone refreshed qualification: 139 focused / 304 owning Core tests, zero-warning/error Release build, 764 complete English UI assertions with Chinese regional formatting and 28 receiver-free stability assertions.

[Upstream stable](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.App/ViewModels/AppViewModel.cs) contains merged PR #19's partial passive retention/fresh-snapshot safeguards. The fork's [view model](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/ViewModels/AppViewModel.cs), [controller](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs), [selection](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/ReceiverSelection.cs) and [WPF lifecycle fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiSessionLifecycle.cs) retain the stronger integrated contract.

## Separate report acceptance

| Report | Implemented behavior and executed assertions |
| --- | --- |
| 01 | Atomic snapshot/desktop-owner capture and serialized retarget guards. Delayed identity saves cannot mutate newer Pair or same-id replacement. Owner survives metrics/native retry. Changed/absent old rows, save failure, valid identity promotion and one signature-driven restart are covered. |
| 02 | Automatic admission is separate from explicit Toggle. Explicit actions invalidate old intent before awaits; one action token persists through save/admission/cleanup. Canonical candidate/options/availability/visibility are revalidated under the app settings gate through serialized admission. Only admitted attempts affect attempted/last-used; admitted failure counts. Idle/Error, queued requests, Pair/Streaming/Stop, serialization waits, eligibility changes without owner change, save failure and newer notice/suppression are covered. |
| 03 | Owned Connecting/Pairing/Streaming/Standby survive passive empty/partial/repeated discovery and adapter browse restart. Active transport remains separate from offline catalog data; new unavailable/incomplete starts reject. Actual bound Stop remains accessible through retained presentation. Reappearance, handshake/PIN/native faults, cancellation, new retry UUIDs and both mute bits are covered. |
| 10 | Attempted candidates are removed before ranking, permitting unattempted B after preferred A. Overrides, aliases, all-attempted no-op and allowed reset transitions remain covered; stale rejected requests consume no candidate. |
| 11 | Manual clicks retain exact endpoint/port/id/options alongside complete/incomplete/hidden groups. Explicit groups remain selected. Discovered routing requires a unique confirmed canonical/alias owner and rejects ambiguity; address-only fallback is omitted. Fake command peers, gain, latency and last-used are asserted. Credentials remain accessory keyed. |

An explicit Play/Pair/Stop/shutdown decision cannot be replaced by a stale continuation. Cleanup of an ended worker cannot leave a falsely active snapshot when replacement is rejected. Endpoint recovery waits outside controller serialization/settings gate, then rechecks owner/intent/inactive/disposal; obsolete recovery cannot restore another session's audio.

## Original regression evidence

At original `41190e0`, seven signal-gated observations failed their desired assertions with relevant production blobs unchanged. Initial fixed `342aeb7` passed 55 focused/218 owning Core and 273 WPF checks per language. These are historical sources, not the current feature/Main counts.

Implementation-worktree evidence: `artifacts/group-b-baseline/`, `group-b-test-results/`, `group-b-ui-en/` and `group-b-ui-zh/`. Source/assertion review resolved findings, but the deeper follow-up found further races; the initial clear review did not establish universal completeness.

## Follow-up audit on 2026-10-08

Unchanged published `342aeb7` reproduced delayed endpoint work replacing newer Pair/restarting after Stop and four retarget/settings-cleanup failures. Fix `11c6046` carries captured owner and intent through awaited operations and rechecks after cleanup. Historical standalone qualification passed 60 focused lifecycle, 253 Core in each configuration and 298 WPF checks in each language; builds were warning free.

Receipts remain in `artifacts/pr-deep-audit/`, including endpoint observations, `retarget-before.trx`, `settings-before.trx`, final TRX/UI results and source-bound `final-verification-receipt.json`. Separate combined source `bfed430` passed [run 37870969984](https://github.com/ThatMarko/AirFlash/actions/runs/37870969984); its exact downloaded binary passed 499 English/491 Chinese, excluding then-unimplemented C/D. [Historical combined acceptance](FORK-INTEGRATION-ACCEPTANCE.md#follow-up-validation-on-2026-10-08) preserves its archive/binary identities.

## Current upstream status on 2026-10-10

This heading is a retained historical cross-link. The October 10 review reproduced a culture-dependent hidden-row assertion: production Streaming/Stop behavior was correct, but the fixture selected an index whose meaning changed with regional sorting. Fix `bceb440` selects the owned receiver and invokes its actual retained Stop control. A later Traditional Chinese tray-marker assertion proved static catalog restoration drift; `0a2c1ac` restores the exact entering catalog in finally. Production controller/view-model blobs were unchanged by these fixture corrections.

Final historical regional qualification passed four 300-check standalone pairs and 253 Core in both configurations. Evidence: `artifacts/regional-ui-audit-2026-10-10/`, `catalog-ui-audit-2026-10-10/` and source-bound before-proof receipts. Older 299/500/492 counts lacked the final marker assertion.

Current publication status, rechecked for this Main review: [PR #15](https://github.com/Ding-Kyoma/AirFlash/pull/15) remains CLOSED/unmerged at frozen `11c60466465bc07c0d6ce519dcfc4d3a54b973f6`; [issue #14](https://github.com/Ding-Kyoma/AirFlash/issues/14) is open. It is distinct from current feature `442e8d8`. PR #19 is merged but does not replace the full atomic owner/admission/selection package. No reopen/new PR is authorized by this documentation update.

## Compatibility, limits and publication

Schema 2, JSONL v1, manual persistence, selected-adapter no-fallback, authentication exclusivity, accessory `/info deviceID` keys and exact-bit restoration remain intact. Fixtures use fake engine/audio/discovery and temporary stores. They establish observable ownership/selection/UI behavior, not HomePod reliability, acoustic output or physical OS mute persistence.

Before selected Task 3 publication, review against then-current upstream, remove integrated overlap, rerun affected checks and use issue #14 accurately. Keep a focused group submission; public bodies do not link private planning. Initial upstream CI runs `37717476182`/`37870953320` had approval/zero-job states, which are not executed failure evidence. Maintainer diagnostic, executed regional controls and current fork CI remain separate.
