# Group B acceptance — session ownership and receiver selection

## Comprehensive audit correction — 2026-10-18

The broader audit found a missing part of the original shutdown contract. Earlier shutdown regressions called VM disposal directly; actual shell Quit first awaited Settings Apply and engine-probe disposal. Those preceding waits left admission open. The new shared shell fixture reproduces an actual fake Start and mute after releasing a real held Settings Apply under the unchanged original shutdown ordering. The failed control is retained separately; passing earlier runs did not repair or cover this gap.

Correction [`d7782a3d069f996b6d1c7c2fe65b224b6f13aa8e`](https://github.com/ThatMarko/AirFlash/commit/d7782a3d069f996b6d1c7c2fe65b224b6f13aa8e), on the same original-base B branch/worktree, closes admission synchronously at actual shell Quit before its first await. Accepted Apply/dirty saves still persist, while post-save runtime updates cannot replace playback during closing. The owned bound Stop remains usable; its successful cleanup publishes Idle and disables inactive controls during held probe cleanup. Apply/window/probe failures cannot skip VM cleanup; original exception identity or ordered multiple errors remain observable. Both original fake mute bits are restored. This supersedes earlier blanket shutdown acceptance and the former first-Quit-await exclusion in the prepared draft.

Fresh standalone **SDK 10.0.401 and 10.0.402** each pass locked restore, unchanged locks/active ILLink analyzer, zero-warning/error Release build, **304 Core / 103 focused controller-selection tests** and **51 focused shell assertions**. The focused route contains five diagnostic-startup controls plus 46 new shell assertions. Six complete observed catalog/formatting runs on 10.0.402 pass **810 assertions each** (three UI catalogs × two formatting cultures); an additional English 10.0.401 full run also passes 810. Independent source and regression review resolved the initially stale post-Stop presentation and found no remaining concrete finding.

Fork merge `a1182b0c984d882e9cd3fb3cbe443dbf2a10ed48` integrates this correction while preserving G's mute-readiness guards, recovery-window closure and audio cleanup `finally`. Separate integration review is clear. Both SDKs pass **677 Core / 113 focused discovery** with warning-free builds; current combined UI/package qualification is recorded in [the comprehensive verification report](COMPREHENSIVE-VERIFICATION-2026-10-18.md). Receipts, failed-control extraction, exact source hashes and binary-bound UI reports remain under ignored `artifacts/comprehensive-audit-2026-10-18`; they do not replace source-bound older evidence below.

The B branch is normally pushed only to origin. Issue #14 remains open; PR #15 remains closed/unmerged at its frozen historical head. No reopen/replacement, new upstream publication, release or real-device test occurred. The complete public diff and every outgoing commit remain free of fork planning. Terminal-event handling inside an already open PIN prompt remains a separate documented limit.

## Standalone SDK preparation — 2026-10-18

Prepared `codex/fix-session-lifecycle` is now `002c4e133e43f8332015b4bde44d7a75660eedcc`, based on original stable `cf420d97700463c56adc31408510e8336e99e51c`. The unchanged previous head `442e8d8` reproduces SDK 10.0.402 locked-restore NU1004. The shared two-file dependency/CI correction preserves the existing ILLink 10.0.12 lock and actual analyzer consumption, SDK roll-forward, host selection and publishing flags; no runtime source changes.

Fresh isolated 10.0.402 verification passes forced locked restore, a Release solution build with zero warnings/errors, **166 focused / 304 owning Core tests** without skips, and **764 WPF assertions each** for en-US/en-US and zh-TW/zh-CN catalog/formatting cells. Both reports verify observed cultures and exact tray catalog restoration. The earlier six-cell qualification remains bound to its original source/SDK; this supplement does not relabel it. Three lock hashes remain unchanged. Independent final source/evidence review is clear; the branch is normally pushed to origin and complete diff/all ten outgoing commit trees exclude private planning. Evidence is under ignored `artifacts/implementation-2026-10-18/b-sdk`.

Canonical issue #14 remains open and PR #15 remains closed without merge at its frozen old head. No reopen/replacement, upstream publication or release occurred. Fork Main already contains the equivalent dependency correction; updated branch ancestry and diagnostic preparation are recorded separately from upstream integration.

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
