# Group C acceptance — configuration preservation and validation

## Upstream publication — 2026-10-13

Explicit approval published [issue #22](https://github.com/Ding-Kyoma/AirFlash/issues/22) and ready-for-review [PR #23](https://github.com/Ding-Kyoma/AirFlash/pull/23), targeting original `main` from `ThatMarko:codex/fix-settings-recovery` at `5d72da3b77b4ac72f858966baef04b8a4ad8b1ad`. Fresh original stable remains `cf420d97700463c56adc31408510e8336e99e51c`. Open/closed issue/PR search found no duplicate configuration-recovery root cause; issue #14's delayed session authority and merged Settings scrolling work are separate. Published title/body, `Fixes #22`, nine changed files and four commits match the reviewed package. Complete diff and every outgoing commit exclude fork-only planning. Independent source/regression/evidence and final publication-text reviews are clear.

Fresh prepublication Release Core verification passed **300 tests**, no failures/skips, with selected SDK 10.0.401 and locked restore. Recorded exact-head standalone Release build and three **633-assertion** WPF catalog/region runs retain their original provenance; no new GUI failure is claimed on unchanged original stable. The SettingsStore/Settings blobs match historical `41190e0`, supporting the eight controlled Core observations below. Public wording now limits automatic startup playback/mute claims, describes controlled future-schema refusal with saving blocked, permits defaults replacement only after preservation, and makes the synthetic automatic-start conditions explicit. [Upstream PR CI](https://github.com/Ding-Kyoma/AirFlash/actions/runs/38434164036) is tracked separately; an open PR is not integration into stable. No release/reservation, local installation, real configuration/endpoint/receiver access or other-group publication occurred.

## Current Main integration — 2026-10-12

Reviewed fork runtime baseline is `8989b3609dd1294f4d1d6eb1502f345cc22bc382`, with source trees equal to qualified code `4ae1db4f01dcf7857482dd136ea8d9dffa3aa8a1`. The feature head below is an ancestor of Main and is pushed to origin. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`. Public complete diffs and every outgoing commit exclude private planning. This is fork implementation acceptance, not upstream integration. [Combined Main acceptance](CONTINUATION-ACCEPTANCE.md) records 667 Core/124 Python/native/package/UI qualification and limits.

Feature `codex/fix-settings-recovery`, head `5d72da3b77b4ac72f858966baef04b8a4ad8b1ad`; worktree `fix-settings-recovery/AirFlash`. Reports 06/07 are complete. Independent review is clear. Refreshed standalone checks pass 129 focused/300 owning Core tests and a zero-warning/error Release build.

Current [store](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SettingsStore.cs), [settings validation](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/Settings.cs), [Core recovery assertions](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/SettingsRecoveryTests.cs) and [WPF startup/recovery](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiSettingsRecovery.cs) remain integrated. Combined final WPF reports contain **22** cross-group settings assertions; older 19-count inventories are historical.

## Implemented behavior

- Both startup Load calls share locked preservation state. Bytes are captured through guarded I/O and decoded compatibly; recovery never rereads an unreadable path in its catch. Only genuine missing-file/directory results mean absence.
- Readable invalid/migrated originals retain exact bytes, including UTF-8/16/32 BOMs. Pending distinct revisions remain available until verified backups and successful replacement. Existing backups survive; differing content receives unique siblings. Failed backup/replacement, deleted/corrupt backup and notice updates remain controlled/retryable.
- Defaults cannot silently replace unreadable/uncertain input. Save preserves it successfully or fails safely; disappearance without a captured original does not silently release the obligation. Supported positive integer future schemas are fatal, including oversized values/case variants/duplicates; rejected stores cannot save until a valid reread.
- Migration and documented collection/alias normalization precede total semantic validation. Null manual entries, invalid/null rates, malformed shape, duplicates, blank hosts, invalid ports/enums/ranges/EQ values recover before runtime consumption. Schema 1→2, unknown fields, nullable overrides, valid manual rows and selected-offline adapter ids remain compatible.
- Recovery notice persistence describes actual backup state. Normal read-only startup hello remains unchanged; rejected input creates no mock playback/mute/autostart or unpreserved replacement. Safe recovered hidden startup remains paused until the fixture explicitly requests browsing.

Host validation does not perform DNS or import the manual dialog's stricter syntax/IPv6/duplicate-endpoint rules. Ordinary Save after a valid Load is not a universal external-writer coordination system.

## Original regression evidence

Original `41190e0` probes ran with production blobs unchanged: eight desired assertions failed for locked reads, null manual elements, invalid/null rates, UTF-16 backup, pre-existing backup and case-insensitive future schema. These prove Core failures, not an original GUI launch failure. Fixed initial `e129466` passed 76 focused/239 Core and 208 WPF per language, with warning-free builds.

Preserved worktree receipts: `artifacts/group-c-baseline/`, `group-c-tests/`, `group-c-ui-en-final/`, `group-c-ui-zh-final/`. Review fixed revision-loss on retry and stale completed-backup notices. A fake broadcast identity caused an unrelated legitimate alias save; using a synthetic endpoint corrected the fixture without relaxing preservation assertions.

## Additional testing and fix on 2026-10-09

Unchanged initial `e129466` failed six oversized-future-schema cases while two ordinary controls passed. Fix `c3a1d5e` classifies integer spelling before bounded conversion, rejecting future positive integer values during Load and pending preservation Save/identity Save. Fractional/exponent/string/negative spellings retain controlled recovery and exact-byte backup.

Historical corrected qualification: 22 added cases, 126 focused/261 Core in each configuration and 208 WPF checks per language; zero build warnings/errors. `artifacts/more-testing-2026-10-09/future-schema-after-receipt.json` binds corrected source hashes despite its pre-commit metadata. These counts are not relabeled current Main results.

## Stable refresh — 2026-10-11

Historical `3055988` integrated stable `91698d8` without changing configuration production code. New scoped browse startup required explicit fixture demand after proving recovered defaults pause correctly. It passed 172 focused/297 Core and 70 narrow UI checks in both English/Simplified Chinese; integration-only recovery checks passed separately. Failed and corrected browse fixtures remain retained. Traditional Chinese retains existing English fallback for some recovery notices.

## Compatibility and limits

Tests use synthetic temporary files, controlled locks/I/O failures, fake services and isolated WPF output. They do not read user settings/credentials or mutate endpoints/receivers. Serialization/atomic replacement do not establish physical power-loss durability or coordination with arbitrary external writers. Native source, schema 2 and JSONL v1 remain unchanged by C.

C is integrated and pushed in fork Main and is now published as issue #22 / PR #23 above. Original stable integration remains pending maintainer review; no merge or release is authorized by this publication.
