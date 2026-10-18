# [MEDIUM] Settings recovery preserves originals without an unguarded reread

- **Status**: Implemented, pushed and integrated in fork `main` `8989b3609dd1294f4d1d6eb1502f345cc22bc382`.
- **Group / feature**: C, `codex/fix-settings-recovery`, `5d72da3b77b4ac72f858966baef04b8a4ad8b1ad`.
- **Stable comparison**: `cf420d97700463c56adc31408510e8336e99e51c` lacks this preservation repair.
- **Original severity**: MEDIUM recoverable read escaping startup at `41190e0`.
- **Plan / evidence**: [ISSUE-06](IMPLEMENTATION_PLAN.md#issue-06), [C acceptance](GROUP-C-ACCEPTANCE.md), [current acceptance](CONTINUATION-ACCEPTANCE.md).
- **Related**: [07 loaded validation](ISSUE-07-MEDIUM-invalid-loaded-settings-reach-runtime.md), [Settings](../analysis/settings.md).

## Publication status — 2026-10-18

Published as [issue #22](https://github.com/Ding-Kyoma/AirFlash/issues/22) and [PR #23](https://github.com/Ding-Kyoma/AirFlash/pull/23). Both remain open; the PR is unmerged at current standalone head `107c7dfcb192e8bc37648b9f324d2c030247f893`. Earlier feature/main hashes, source links and verification counts below retain their recorded provenance; [C acceptance](GROUP-C-ACCEPTANCE.md) records later qualification separately.

## 1. Current behavior

[SettingsStore.Load](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SettingsStore.cs) captures readable original bytes once under its guarded read, decodes BOMs compatibly and never rereads a failing path unguarded in recovery. Genuine initial absence differs from unreadable/uncertain presence. Both normal startup Loads share pending preservation; defaults/repeated Load/later disappearance cannot silently discharge an unreadable-original obligation.

Readable invalid/migrated bytes retain exact BOM/encoding for backup. Save and identity Save use one store lock and require preservation before replacement. A different existing `.pre-wpf.bak` remains intact; a unique sibling preserves another captured revision. Backup/replacement failure retains obligations for retry. Warnings distinguish an uncaptured original with blocked save, a captured original still awaiting backup and actual completed preservation. Future-schema rejection remains fatal and latches persistence blocking until valid reread.

## 2. Historical root cause and observation

At [41190e0 SettingsStore](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SettingsStore.cs#L51), the recovery catch reread the same still-unreadable file before returning defaults. A temporary exclusive-share fixture reproduced that secondary escaping read. Simply swallowing it would lose the original before later defaults save; byte preservation, both Loads and later automatic persistence had to share one recovery contract.

Original baseline failures, initial/follow-up source hashes and corrected fixtures remain in C acceptance. No real user configuration/ACL was inspected or changed.

## 3. Regression coverage

Current [SettingsRecoveryTests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/SettingsRecoveryTests.cs) and separate WPF [UiSettingsRecovery](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiSettingsRecovery.cs) use temporary locked/readable-invalid/BOM files and injected read/backup/replacement failures. They assert controlled startup/defaults plus truthful notice, pending state across repeated Loads, blocked destructive automatic/identity save, exact backups, preservation retry, backup collision and future-schema refusal. Gates/handles release in finally.

## 4. Acceptance criteria and verification

- No uncaught secondary recovery read.
- Exact readable bytes preserved before replacement, including encoding/BOM.
- Uncaptured unreadable original cannot be silently overwritten.
- Existing different backup and failed-save obligations survive.
- Missing/default startup, schema migration and valid persistence remain compatible.
- Warnings describe actual preservation state.

C's narrower 129 focused/300 Core/build results and current integrated **667 Core / six × 1,059 packaged UI assertions** are recorded separately in acceptance. Current main's build/CI is green; this Markdown task audits source/receipts without real configuration or playback.

## 5. Limits and publication

Schema 2, supported extension fields and separate identity backup remain compatible. The store lock does not coordinate arbitrary external writers; ordinary save after valid Load does not inspect every later external schema edit. Replacement/backup is not physical power-loss qualification. C is integrated/pushed to the fork; its published upstream PR still awaits review and integration.
