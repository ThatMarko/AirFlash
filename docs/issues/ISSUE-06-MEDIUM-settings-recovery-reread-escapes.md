# [MEDIUM] An unreadable settings file escapes the loader's recovery handler

## Verified update — 2026-10-12

- **Status**: Implemented in pushed C `5d72da3b77b4ac72f858966baef04b8a4ad8b1ad`, based on stable `cf420d97700463c56adc31408510e8336e99e51c` and integrated into tested runtime `dc521ae4236479cf21f9881b2d053f51a0fea7ee`; upstream does not contain this preservation fix.

[Load/save recovery](https://github.com/ThatMarko/AirFlash/blob/5d72da3b77b4ac72f858966baef04b8a4ad8b1ad/desktop/AirFlash.Core/SettingsStore.cs) captures readable bytes once inside the guard and preserves their exact BOM/encoding. Recovery never rereads the failed path unguarded. Genuine absence differs from unreadable/uncertain presence, and repeated Load does not discharge preservation. Defaults cannot silently replace an uncaptured original; Save and identity Save require verified raw backups before replacement and retain failed obligations for retry. Warnings distinguish captured-original backup from blocked unreadable-source preservation. Future-schema rejection remains fatal.

C refresh passed 129 focused/300 Core and build. Integrated temporary-store UI fixtures passed within six 1059-check runs, with 666 Core/build and completed CI. [Acceptance](GROUP-C-ACCEPTANCE.md) records locked/BOM/repeated-load/injected-failure cases. This does not coordinate arbitrary external writers or prove physical power-loss durability; no real user settings were read. Earlier source audits and deferred-reboot statements below are historical.

- **Issue ID**: ISSUE-06
- **Implementation plan**: [Work package ISSUE-06](IMPLEMENTATION_PLAN.md#issue-06)
- **PR group**: C — Settings preservation/validation
- **Severity**: MEDIUM; a recoverable read failure can abort normal startup
- **Kind**: Defect
- **Subsystem**: Core settings persistence / Desktop startup
- **Historical status (2026-10-10)**: Implemented in local main `9e894ab`; Group C remains unpushed/unpublished and unmerged upstream. Main execution/testing is deferred until after reboot.
- **Runtime baseline**: upstream `41190e0`; documentation baseline `c077a05`
- **Evidence status**: Original locked-read failure reproduced against unchanged Core; fixed Core/WPF checks recorded in [Group C acceptance](GROUP-C-ACCEPTANCE.md).
- **Historical implementation status (2026-10-10)**: Corrected Group C head `c3a1d5e` (after initial `e129466`) is integrated locally. [Feature acceptance](GROUP-C-ACCEPTANCE.md) records executed feature evidence; [integration acceptance](ABCD-INTEGRATION-ACCEPTANCE.md) keeps earlier composition results separate from this source-only main audit.
- **Target files**: [SettingsStore.cs:16–56](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SettingsStore.cs#L16), [App.xaml.cs:48–49,69–75](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/App.xaml.cs#L48)
- **Analysis context**: [Settings load/backups](../analysis/settings.md)

## Historical local implementation — 2026-10-10 (9e894ab)

Current [SettingsStore.Load](../../desktop/AirFlash.Core/SettingsStore.cs) reads bytes within one guarded operation and never rereads a failing path in its recovery catch. Readable invalid/migrated bytes remain pending for exact-byte backup; an unreadable source retains an obligation to read and preserve before replacement. Only actual missing-file/directory exceptions mean absence, and later disappearance does not discharge a previous uncaptured obligation. Save and identity Save share the store lock; failures retain captured revisions for retry. Existing different `.pre-wpf.bak` content survives while unique sibling backups preserve new revisions, each verified before replacement. Future-schema rejection remains fatal and latches persistence blocking until a valid reread; a pending save checks the now-readable source before backup/replacement.

Source main `9e894ab133f8cbdb3b5f5ac1f411023b182e65d7`, [SettingsRecoveryTests](../../desktop/AirFlash.Tests/SettingsRecoveryTests.cs) and [concrete-store WPF fixtures](../../desktop/AirFlash.App/Verification/UiSettingsRecovery.cs) were read, without app/build/test execution. Prior feature/composition counts remain historical. The following behavior/proposal/missing-test statements describe pinned `41190e0`. Store locking does not promise coordination with arbitrary external writers or power-loss durability.

## Original baseline behavior and effect

[SettingsStore.Load:18–21](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SettingsStore.cs#L18) checks for an existing file and reads it. Its [catch:51–55](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SettingsStore.cs#L51) explicitly handles JSON, I/O, access, and invalid-operation failures, sets a recovery warning, then tries to read the same path again before returning defaults.

If the file is still unreadable while `File.Exists` reports it exists—for example, a valid temporary file held open with exclusive sharing—the second `File.ReadAllText` at line 54 throws outside any nested recovery catch. `Load()` does not return defaults. The startup caller [App.xaml.cs:48–49](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/App.xaml.cs#L48) reaches the outer startup-failure handler at [lines 69–75](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/App.xaml.cs#L69), which logs/displays the failure and quits. No playback needs to have started.

This report concerns retrying an I/O read inside error recovery. Semantic validation of successfully read schema-2 JSON is separate work. A future-schema `NotSupportedException` is intentionally outside this recovery catch and must remain so.

## Root cause and backup boundary

The catch tries to preserve original text for a later `config.json.pre-wpf.bak`, but obtains that text through another unguarded read of the failing path. The warning already says the original "will be backed up" at [line 53](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SettingsStore.cs#L53). Actual backup happens only during [Save:65–69](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SettingsStore.cs#L65), from `_migrationSource`; no backup is created during `Load` itself.

Simply swallowing the second exception and returning defaults is incomplete: if no original text was captured, a later save after the file unlocks could replace it without its promised recovery backup. Recovery must preserve the user's file and distinguish captured-source backup from unreadable-source preservation.

## Minimal deterministic reproduction plan

Use a fresh disposable fixture path, never `%APPDATA%` or an actual user configuration:

1. Write synthetic `{"schema_version":2}` to a temporary file.
2. Hold it open with `FileStream` using `FileShare.None`, so metadata/existence can still succeed but another content read fails.
3. Call `new SettingsStore(temporaryPath).Load()` while the exclusive handle remains open.
4. Current source predicts an I/O exception from the catch's second read instead of defaults plus a usable warning.
5. Release the fixture handle and verify the original bytes remain intact; separately exercise any proposed recover-and-save policy.

This was a source-derived fixture specification during the original report. The later isolated Core reproduction and concrete-store WPF checks are recorded in Group C acceptance; they are not new main or real-user-configuration execution. No real settings, ACLs, or credential files were inspected or changed by this audit.

## Proposed implementation and acceptance

Capture original bytes once under the guarded read, decode compatibly for parsing, and retain the raw bytes when parse/migration subsequently fails. A byte-preserving backup must not rely only on ReadAllText/WriteAllText, which can change BOM/encoding. Do not unconditionally reread an unreadable file from the catch. For read failures without captured text, return/report an explicit recovery state, preserve the existing path, and defer or block replacing it until a verified backup/preservation step succeeds. The warning must accurately describe whether a backup is available or pending.

- Exclusive-share and read-access fixtures return a controlled recovery result without an uncaught secondary read exception.
- Normal startup can surface that recovery notice without losing the existing file.
- A successful original read followed by invalid JSON preserves the exact raw bytes, including BOM/encoding, for the recovery backup.
- An unreadable original is not silently overwritten by a subsequent defaults save, including after the file becomes readable.
- Schema greater than 2 still raises the existing unsupported-version result and remains unchanged.
- Missing-file/default startup, migration, atomic replacement, and schema-2 persistence continue working.

## Existing and missing tests

[SettingsTests.cs:112–125](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/SettingsTests.cs#L112) covers readable legacy migration/backup; [lines 204–213](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/SettingsTests.cs#L204) covers intentional future-schema rejection. [ReceiverIdentityTests.cs:259–271](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/ReceiverIdentityTests.cs#L259) covers failed identity-backup preservation, not this load catch. No existing unreadable-file/secondary-read recovery test was found. New tests should use only temporary fixtures and verify preservation before and after unlocking. No schema change, native-engine change, hardware test, or external audio-issue relationship is required.
