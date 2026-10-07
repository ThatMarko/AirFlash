# [MEDIUM] An unreadable settings file escapes the loader's recovery handler

- **Issue ID**: ISSUE-06
- **Implementation plan**: [Work package ISSUE-06](IMPLEMENTATION_PLAN.md#issue-06)
- **PR group**: C — Settings preservation/validation
- **Severity**: MEDIUM; a recoverable read failure can abort normal startup
- **Kind**: Defect
- **Subsystem**: Core settings persistence / Desktop startup
- **Status**: Open — source-confirmed recovery-path exception
- **Runtime baseline**: upstream `41190e0`; documentation baseline `c077a05`
- **Evidence status**: Source/catch path verified; locked-file reproduction specified but not executed against the .NET application
- **Implementation status**: Proposal only
- **Target files**: [SettingsStore.cs:16–56](../../desktop/AirFlash.Core/SettingsStore.cs#L16), [App.xaml.cs:48–49,69–75](../../desktop/AirFlash.App/App.xaml.cs#L48)
- **Analysis context**: [Settings load/backups](../analysis/settings.md)

## Current behavior and effect

[SettingsStore.Load:18–21](../../desktop/AirFlash.Core/SettingsStore.cs#L18) checks for an existing file and reads it. Its [catch:51–55](../../desktop/AirFlash.Core/SettingsStore.cs#L51) explicitly handles JSON, I/O, access, and invalid-operation failures, sets a recovery warning, then tries to read the same path again before returning defaults.

If the file is still unreadable while `File.Exists` reports it exists—for example, a valid temporary file held open with exclusive sharing—the second `File.ReadAllText` at line 54 throws outside any nested recovery catch. `Load()` does not return defaults. The startup caller [App.xaml.cs:48–49](../../desktop/AirFlash.App/App.xaml.cs#L48) reaches the outer startup-failure handler at [lines 69–75](../../desktop/AirFlash.App/App.xaml.cs#L69), which logs/displays the failure and quits. No playback needs to have started.

This report concerns retrying an I/O read inside error recovery. Semantic validation of successfully read schema-2 JSON is separate work. A future-schema `NotSupportedException` is intentionally outside this recovery catch and must remain so.

## Root cause and backup boundary

The catch tries to preserve original text for a later `config.json.pre-wpf.bak`, but obtains that text through another unguarded read of the failing path. The warning already says the original "will be backed up" at [line 53](../../desktop/AirFlash.Core/SettingsStore.cs#L53). Actual backup happens only during [Save:65–69](../../desktop/AirFlash.Core/SettingsStore.cs#L65), from `_migrationSource`; no backup is created during `Load` itself.

Simply swallowing the second exception and returning defaults is incomplete: if no original text was captured, a later save after the file unlocks could replace it without its promised recovery backup. Recovery must preserve the user's file and distinguish captured-source backup from unreadable-source preservation.

## Minimal deterministic reproduction plan

Use a fresh disposable fixture path, never `%APPDATA%` or an actual user configuration:

1. Write synthetic `{"schema_version":2}` to a temporary file.
2. Hold it open with `FileStream` using `FileShare.None`, so metadata/existence can still succeed but another content read fails.
3. Call `new SettingsStore(temporaryPath).Load()` while the exclusive handle remains open.
4. Current source predicts an I/O exception from the catch's second read instead of defaults plus a usable warning.
5. Release the fixture handle and verify the original bytes remain intact; separately exercise any proposed recover-and-save policy.

This is a source-derived fixture specification, not an executed locked-file or GUI run. No real settings, ACLs, or credential files were inspected or changed.

## Proposed implementation and acceptance

Capture original bytes once under the guarded read, decode compatibly for parsing, and retain the raw bytes when parse/migration subsequently fails. A byte-preserving backup must not rely only on ReadAllText/WriteAllText, which can change BOM/encoding. Do not unconditionally reread an unreadable file from the catch. For read failures without captured text, return/report an explicit recovery state, preserve the existing path, and defer or block replacing it until a verified backup/preservation step succeeds. The warning must accurately describe whether a backup is available or pending.

- Exclusive-share and read-access fixtures return a controlled recovery result without an uncaught secondary read exception.
- Normal startup can surface that recovery notice without losing the existing file.
- A successful original read followed by invalid JSON preserves the exact raw bytes, including BOM/encoding, for the recovery backup.
- An unreadable original is not silently overwritten by a subsequent defaults save, including after the file becomes readable.
- Schema greater than 2 still raises the existing unsupported-version result and remains unchanged.
- Missing-file/default startup, migration, atomic replacement, and schema-2 persistence continue working.

## Existing and missing tests

[SettingsTests.cs:112–125](../../desktop/AirFlash.Tests/SettingsTests.cs#L112) covers readable legacy migration/backup; [lines 204–213](../../desktop/AirFlash.Tests/SettingsTests.cs#L204) covers intentional future-schema rejection. [ReceiverIdentityTests.cs:259–271](../../desktop/AirFlash.Tests/ReceiverIdentityTests.cs#L259) covers failed identity-backup preservation, not this load catch. No existing unreadable-file/secondary-read recovery test was found. New tests should use only temporary fixtures and verify preservation before and after unlocking. No schema change, native-engine change, hardware test, or external audio-issue relationship is required.
