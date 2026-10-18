# [MEDIUM] Loaded settings are validated before catalog and playback consumption

- **Status**: Implemented, pushed and integrated in fork `main` `8989b3609dd1294f4d1d6eb1502f345cc22bc382`.
- **Group / feature**: C, `codex/fix-settings-recovery`, `5d72da3b77b4ac72f858966baef04b8a4ad8b1ad`.
- **Stable comparison**: `cf420d97700463c56adc31408510e8336e99e51c` lacks this loaded-validation repair.
- **Original severity**: MEDIUM invalid configuration reaching startup/runtime at `41190e0`.
- **Plan / evidence**: [ISSUE-07](IMPLEMENTATION_PLAN.md#issue-07), [C acceptance](GROUP-C-ACCEPTANCE.md), [current acceptance](CONTINUATION-ACCEPTANCE.md).
- **Analysis / related**: [Settings](../analysis/settings.md), [Session](../analysis/session.md), [06 preservation](ISSUE-06-MEDIUM-settings-recovery-reread-escapes.md).

## Publication status — 2026-10-18

Published with configuration preservation as [issue #22](https://github.com/Ding-Kyoma/AirFlash/issues/22) and [PR #23](https://github.com/Ding-Kyoma/AirFlash/pull/23). Both remain open; the PR is unmerged at current standalone head `107c7dfcb192e8bc37648b9f324d2c030247f893`. Earlier feature/main hashes, source links and verification counts below retain their recorded provenance; [C acceptance](GROUP-C-ACCEPTANCE.md) records later qualification separately.

## 1. Current behavior

After supported migration and documented normalization, [SettingsStore.Load](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SettingsStore.cs) calls [AppSettings.Validate](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/Settings.cs#L88) before returning settings. Null manual elements, blank/duplicate ordinal ids, invalid ports, unsupported/null sample rates, enums, ranges and malformed EQ receive controlled recovery rather than downstream dereference/parse exceptions.

Null supported collections/options normalize as documented; valid schema 1 migration, schema 2, root/equalizer extension fields, nullable manual display names/receiver overrides and unavailable selected-adapter ids remain compatible. Loaded manual hosts require nonblank text/range, not DNS or every stricter add-dialog host/IPv6/duplicate-endpoint rule. Future positive-integer schema is fatal and blocks persistence; recovery preserves rejected bytes under report06's contract.

Rejected input returns defaults with automatic connection disabled, plus warning/preservation; it does not issue playback Start or mute. Ordinary startup's separate read-only engine hello remains—“no playback” does not mean eliminating initialization.

## 2. Historical root cause and observations

At [41190e0 Load](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SettingsStore.cs#L40), deserialization returned without Validate. `manual_receivers:[null]` later crashed MergeManualReceivers; invalid saved rate reached Start construction's int.Parse. Calling the old validator alone was insufficient because its manual predicate also assumed nonnull elements.

Original isolated loader/validation failures and mock construction/playback observations are retained in C acceptance. These synthetic malformed files are not alleged normal UI saves or actual household configuration.

## 3. Regression fixtures and coverage

```json
{"schema_version":2,"manual_receivers":[null]}
```

```json
{"schema_version":2,"stream_sample_rate":"invalid"}
```

Current [SettingsRecoveryTests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/SettingsRecoveryTests.cs), [SettingsTests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/SettingsTests.cs) and WPF [UiSettingsRecovery](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiSettingsRecovery.cs) use concrete temporary stores/counted fake playback/audio/autostart. They cover invalid/null/missing rate, manual shape/id/range, migration/unknown fields/nullable overrides, future duplicate/large schemas, unavailable adapter and no rejected-input playback/mute or uncontrolled replacement save. Startup catalog assertions request browsing explicitly; hidden default startup need not browse.

## 4. Acceptance criteria and verification

- Validator returns a diagnostic for deserialized null/manual shapes instead of throwing.
- Unsupported/null rate never reaches unchecked playback parsing.
- Rejected readable bytes preserve before replacement; unreadable recovery remains controlled.
- Compatible migrated/valid data and unavailable adapter selection retain behavior.
- Mock startup issues no playback or mute for rejected input; separate hello remains compatible.

Current integrated **667 Core**, warning-free build and six **1,059-assertion** packaged UI runs pass. Twenty-two cross-group startup recovery assertions supersede the old nineteen-case inventory. C/current acceptance separates earlier feature checks, baseline failures and code/binary identities. No new runtime suite or real user data operation runs for this documentation update.

## 5. Compatibility and limits

Save itself is not an arbitrary-input semantic validator, and its lock does not coordinate external file writers after valid Load. Schema 2/JSONL v1, accessory credential keys, manual persistence and selected-adapter no-fallback remain intact. This report is implemented in the fork; its published upstream PR remains unmerged.
