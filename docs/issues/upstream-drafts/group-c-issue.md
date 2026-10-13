# Recover unreadable or invalid configuration without losing the original

The source baseline is original stable main at `cf420d97700463c56adc31408510e8336e99e51c`.

## Current behavior

[SettingsStore.Load](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.Core/SettingsStore.cs#L16-L56) catches a configuration read/parse failure but rereads the same path inside that catch without another guard. An exclusively locked readable path can therefore throw a second I/O exception instead of returning controlled defaults. `File.Exists` also cannot distinguish every unreadable path from genuine absence.

Recovery stores decoded text and writes a text backup later. That does not preserve the original encoding/BOM bytes. An existing backup or repeated Load/failed Save can leave insufficient information for safely replacing the source. Normal startup calls Load twice on the same store, so preservation must survive both calls and later automatic/identity saves.

After migration and collection normalization, Load returns deserialized settings without semantic validation. A null manual element can fail later catalog construction; an invalid or null stream rate can reach playback parsing. [AppSettings.Validate](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.Core/Settings.cs#L88-L111) itself assumes nonnull manual elements. Future-schema recognition also depends on one exact-spelled bounded integer lookup.

Additional source anchors: [backup and save](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.Core/SettingsStore.cs#L62-L84), [manual catalog consumption](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L285-L291), and [unchecked stream-rate conversion](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.Core/SessionController.cs#L306).

## Reproduction and observation boundary

Use temporary files and injected file operations, rather than a user's configuration:

1. Hold an exclusive file handle across both Load calls. Expect controlled defaults plus a warning, and a blocked Save until the unreadable original can be captured and preserved.
2. Load malformed readable JSON encoded with a BOM, then Save. Compare backup bytes exactly with the original, including the BOM/encoding.
3. Precreate a different recovery backup; fail backup or replacement, change/disappear the source, and retry. No captured revision or uncaptured-original obligation may be silently lost.
4. Load either fixture below and assert it never reaches playback/mute or an uncontrolled replacement save.

```json
{"schema_version":2,"manual_receivers":[null]}
```

```json
{"schema_version":2,"stream_sample_rate":"invalid"}
```

5. Exercise future positive integer schema tokens, oversized integer tokens, case variants and duplicate schema properties. Future-version refusal must not degrade into defaults replacement. Fractional/exponent spellings follow integer validation and controlled malformed-input recovery; they are not part of the lexical future-integer rule.

Earlier isolated Core controls on original `41190e0` reproduced the locked reread, invalid/null values and byte-backup failures. The stable source retains these paths. This is not a claim of a fresh original GUI launch failure or a real user's corrupted configuration.

## Expected behavior and bounded fix

Capture readable original bytes once through guarded I/O and decode compatibly. Carry exact-byte or uncaptured preservation obligations across repeated Loads, recovery retries and automatic/identity saves. Distinguish genuine initial absence from uncertain presence. Preserve an existing different backup and write/verify a separate revision backup before replacing the source. A failed preservation/replacement retains the obligation. Warnings must describe whether the original was read, still needs preservation or was actually backed up.

After supported migration and documented normalization, validate structure and semantics before returning settings. Validation must return controlled diagnostics for null/manual shapes and unsupported/null stream rates. Future schema remains rejected and persistence blocked.

## Acceptance criteria

- Locked/read-failed input produces no unguarded secondary exception and cannot be overwritten by defaults.
- Rejected/migrated readable bytes, including supported BOM encodings, are preserved exactly before replacement.
- Both startup Loads, failed backup/replacement, existing-backup collision, source disappearance and identity Save retain preservation state.
- Future positive integer schema tokens remain fatal, including overflow-sized integers and case/duplicate variants.
- Null manual entries and invalid/null rates produce controlled recovery before catalog/playback consumption.
- Schema 1 migration, schema 2, supported root/equalizer unknown fields, nullable overrides/display names, valid manual rows and selected-offline adapter ids remain compatible.
- Core temporary/injected-I/O fixtures and a separate fake-service WPF startup harness assert no rejected-input playback, endpoint mute or unpreserved replacement save. Read-only startup engine hello remains compatible.

The store's lock coordinates this store; it does not claim arbitrary external-writer coordination after a valid Load. Byte preservation/file replacement does not prove physical power-loss durability. No credential format or JSONL change is required.

## Prepared implementation and executed verification

The prepared implementation is [codex/fix-settings-recovery](https://github.com/ThatMarko/AirFlash/tree/5d72da3b77b4ac72f858966baef04b8a4ad8b1ad), commit `5d72da3b77b4ac72f858966baef04b8a4ad8b1ad`, independently based on the original stable source above. Independent final source/regression review found no unresolved findings.

Fresh standalone Release build passed with zero warnings/errors; all **300 Core tests** passed with no skips. Separate isolated WPF runs passed **633 assertions each in 3 catalog/region combinations**, covering English, Simplified Chinese and Traditional Chinese. These are this branch’s results, separate from combined-fork qualification.
