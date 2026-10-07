# [LOW] Remove ten statically unused Chinese catalog entries after reference review

- **Issue ID**: ISSUE-17
- **Implementation plan**: [Work package ISSUE-17](IMPLEMENTATION_PLAN.md#issue-17)
- **PR group**: H — Diagnostics/observability
- **Severity**: LOW; catalog maintenance only
- **Kind**: Enhancement
- **Subsystem**: Core localization (`AirFlash.Core`)
- **Status**: Open — source-confirmed cleanup candidates; original twelve-entry inventory corrected
- **Runtime baseline**: upstream `41190e0`; documentation baseline `c077a05`
- **Evidence status**: Exact-string catalog/C#/XAML scan and lookup traced; no live language-switch test run
- **Implementation status**: Proposal only; entries remain present
- **Target files**: [Strings.zh.json](../../desktop/AirFlash.Core/Strings.zh.json), [LocalizationTests.cs:35–46](../../desktop/AirFlash.Tests/LocalizationTests.cs#L35)
- **Analysis context**: [Settings/localization](../analysis/settings.md), [Equalizer editor](../analysis/equalizer.md)

## Corrected inventory and effect

These ten keys are present in the catalog but have no exact-string occurrence in scanned `desktop/**/*.cs` or `desktop/**/*.xaml`, including test/harness code. They are static cleanup candidates, not a playback defect or proof about all possible dynamically constructed lookups.

| Key | Catalog evidence |
| --- | --- |
| `More` | [Strings.zh.json:133](../../desktop/AirFlash.Core/Strings.zh.json#L133) |
| `Control panel` | [154](../../desktop/AirFlash.Core/Strings.zh.json#L154) |
| `Use wider volume controls` | [155](../../desktop/AirFlash.Core/Strings.zh.json#L155) |
| `Show latency mode` | [156](../../desktop/AirFlash.Core/Strings.zh.json#L156) |
| `Show master volume` | [157](../../desktop/AirFlash.Core/Strings.zh.json#L157) |
| `Device volume / % (blank uses 100%)` | [182](../../desktop/AirFlash.Core/Strings.zh.json#L182) |
| `Underrun packets` | [189](../../desktop/AirFlash.Core/Strings.zh.json#L189) |
| `Dropped frames` | [190](../../desktop/AirFlash.Core/Strings.zh.json#L190) |
| `Statistics come from the current audio session and update about once per second. Target latency and local timing are not measurements of audio reaching HomePod.` | [194](../../desktop/AirFlash.Core/Strings.zh.json#L194) |
| `Desktop 0.2.0 · WPF` | [195](../../desktop/AirFlash.Core/Strings.zh.json#L195) |

The original `Local output is muted while streaming.` and `Always retry connection` keys are absent from the current catalog and are withdrawn from the inventory. This validates that report's candidate list; it does not claim exactly ten unused entries across the entire dictionary.

Monitor uses `Local underrun packets` and `Local dropped frames` at [SettingsWindow.xaml:227,233](../../desktop/AirFlash.App/Ui/SettingsWindow.xaml#L227), and an expanded caveat at [line 297](../../desktop/AirFlash.App/Ui/SettingsWindow.xaml#L297). About uses a computed label at [line 319](../../desktop/AirFlash.App/Ui/SettingsWindow.xaml#L319), backed by [AppPaths.cs:11](../../desktop/AirFlash.App/Services/AppPaths.cs#L11). Removing old entries must preserve the current labels/translations. Keep `Copy diagnostics` for ISSUE-15's existing command.

## Lookup mechanism and scope

[L.cs:32–38](../../desktop/AirFlash.Core/L.cs#L32) loads the dictionary and looks up exact English strings, falling back to English. A statically unused entry mainly adds stale maintenance work; it does not cause an exception or affect playback. Retired layout settings properties remain accepted separately for compatibility ([Settings.cs:41–44](../../desktop/AirFlash.Core/Settings.cs#L41)); removing labels must not remove serialized property support.

Some lookups use runtime page/choice values. A universal CI assertion that every translation key must occur as a source literal is therefore unjustified. Intentional translations can also precede UI bindings, as ISSUE-15 illustrates.

## Verification, proposal, and acceptance

The issue-validation pass parsed the JSON dictionary and, for each original candidate, ran a case-sensitive fixed-string search over desktop `*.cs` and `*.xaml`, with the equivalent of `rg -l -F -g '*.cs' -g '*.xaml' -- <key> desktop`. None of the ten present keys had a match; the other two were absent from the parsed catalog. This searches exact text occurrences, not an AST or runtime lookup graph. No catalog modification or WPF run was performed. Review dynamic lookup sources before removal, then remove only these ten candidates. An optional scanner should report review candidates and support dynamic/intentional entries, rather than failing on substring counts.

- The ten listed entries are removed; current Monitor labels/caveat, version display, and `Copy diagnostics` remain supported.
- Embedded JSON loads, translations are nonempty, and format arguments remain intact.
- No configuration property, JSONL command, controller, or audio code changes.
- English/Chinese mock WPF page checks pass after cleanup.

[LocalizationTests.cs:35–46](../../desktop/AirFlash.Tests/LocalizationTests.cs#L35) checks catalog content/format parity, not reachability. [UiEqualizer.cs:22](../../desktop/AirFlash.App/Verification/UiEqualizer.cs#L22) and [UiSmoke.cs:137–162](../../desktop/AirFlash.App/Verification/UiSmoke.cs#L137) provide relevant future page verification. No receiver/audio failure or external issue-number relationship is established.
