# [LOW] Remove ten statically unused keys from the Chinese catalogs after reference review

## Implementation update — 2026-10-11

Implemented as Group H's final cleanup, source-confirmed on refreshed `codex/fix-diagnostics-metrics` at `7f5135384d2245cfdf39d9c77b591c39df1981e3` against original stable `cf420d97700463c56adc31408510e8336e99e51c`. The ten retained candidates below are removed from both [Simplified](https://github.com/ThatMarko/AirFlash/blob/7f5135384d2245cfdf39d9c77b591c39df1981e3/desktop/AirFlash.Core/Strings.zh.json) and Traditional catalogs after final literal, markup and dynamic lookup review. Current diagnostics/capture-direction labels, EQ presets, format arguments, generated version text and serialized settings compatibility fields remain. [Group H acceptance](GROUP-H-ACCEPTANCE.md) records localization and English/Chinese mock UI checks; no test mirrors the removed-key list. This supersedes earlier unimplemented labels; original stable has not integrated H, and the original candidate inventory and proposal below remain historical.

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
- **Merged-main review (2026-10-10)**: Still unimplemented at local source `9e894ab133f8cbdb3b5f5ac1f411023b182e65d7`. The ten keys below are now present in both [Simplified Chinese](../../desktop/AirFlash.Core/Strings.zh.json) and upstream-added [Traditional Chinese](../../desktop/AirFlash.Core/Strings.zh-TW.json); a fresh fixed-string scan still finds no desktop C#/XAML reference for any of them. Both withdrawn keys remain absent from both catalogs. [L.SelectLanguage/Initialize](../../desktop/AirFlash.Core/L.cs) selects the Traditional catalog for zh-TW/zh-HK/zh-MO preferences, so cleanup must review both catalogs. No catalog was edited and no app, build, or test was executed.
- **Target files**: [Strings.zh.json](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Strings.zh.json), [LocalizationTests.cs:35–46](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/LocalizationTests.cs#L35)
- **Additional current targets**: [Strings.zh-TW.json](../../desktop/AirFlash.Core/Strings.zh-TW.json), current [LocalizationTests](../../desktop/AirFlash.Tests/LocalizationTests.cs) for both embedded catalogs
- **Analysis context**: [Settings/localization](../analysis/settings.md), [Equalizer editor](../analysis/equalizer.md)

## Corrected inventory and effect

These ten keys were present in the historical Simplified catalog and are present in both current Chinese catalogs, but have no exact-text occurrence in scanned `desktop/**/*.cs` or `desktop/**/*.xaml`, including test/harness code. They are static cleanup candidates, not a playback defect or proof about all possible dynamically constructed lookups. The table pins the original evidence; the merged-main review above records the additional Traditional catalog.

| Key | Catalog evidence |
| --- | --- |
| `More` | [Strings.zh.json:133](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Strings.zh.json#L133) |
| `Control panel` | [154](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Strings.zh.json#L154) |
| `Use wider volume controls` | [155](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Strings.zh.json#L155) |
| `Show latency mode` | [156](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Strings.zh.json#L156) |
| `Show master volume` | [157](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Strings.zh.json#L157) |
| `Device volume / % (blank uses 100%)` | [182](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Strings.zh.json#L182) |
| `Underrun packets` | [189](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Strings.zh.json#L189) |
| `Dropped frames` | [190](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Strings.zh.json#L190) |
| `Statistics come from the current audio session and update about once per second. Target latency and local timing are not measurements of audio reaching HomePod.` | [194](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Strings.zh.json#L194) |
| `Desktop 0.2.0 · WPF` | [195](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Strings.zh.json#L195) |

The original `Local output is muted while streaming.` and `Always retry connection` keys are absent from both current catalogs and are withdrawn from the inventory. This validates that report's candidate list; it does not claim exactly ten unused keys across either complete dictionary.

Monitor uses `Local underrun packets` and `Local dropped frames` at [SettingsWindow.xaml:227,233](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Ui/SettingsWindow.xaml#L227), and an expanded caveat at [line 297](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Ui/SettingsWindow.xaml#L297). About uses a computed label at [line 319](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Ui/SettingsWindow.xaml#L319), backed by [AppPaths.cs:11](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/AppPaths.cs#L11). Removing old entries must preserve the current labels/translations. Keep `Copy diagnostics` for ISSUE-15's existing command.

## Lookup mechanism and scope

[L.cs:32–38](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/L.cs#L32) loads the dictionary and looks up exact English strings, falling back to English. A statically unused entry mainly adds stale maintenance work; it does not cause an exception or affect playback. Retired layout settings properties remain accepted separately for compatibility ([Settings.cs:41–44](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Settings.cs#L41)); removing labels must not remove serialized property support.

Some lookups use runtime page/choice values. A universal CI assertion that every translation key must occur as a source literal is therefore unjustified. Intentional translations can also precede UI bindings, as ISSUE-15 illustrates.

## Verification, proposal, and acceptance

The original issue-validation pass parsed the Simplified JSON dictionary and, for each original candidate, ran a case-sensitive fixed-string search over desktop `*.cs` and `*.xaml`, with the equivalent of `rg -l -F -g '*.cs' -g '*.xaml' -- <key> desktop`. None of the ten present keys had a match; the other two were absent from the parsed catalog. The merged-main pass repeated this scan and parsed both current catalogs with the results above. This searches exact text occurrences, not an AST or runtime lookup graph. No catalog modification or WPF run was performed. Review dynamic lookup sources before removal, then remove only these ten candidates from each catalog. An optional scanner should report review candidates and support dynamic/intentional entries, rather than failing on substring counts.

- The ten listed keys are removed from both Chinese catalogs after dynamic-reference review; current Monitor labels/caveat, version display, and `Copy diagnostics` remain supported.
- Embedded JSON loads, translations are nonempty, and format arguments remain intact.
- No configuration property, JSONL command, controller, or audio code changes.
- English, Simplified Chinese, and Traditional Chinese mock WPF page checks pass after cleanup, including exact catalog restoration after a temporary language switch.

[LocalizationTests.cs:35–46](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/LocalizationTests.cs#L35) checks catalog content/format parity, not reachability. [UiEqualizer.cs:22](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Verification/UiEqualizer.cs#L22) and [UiSmoke.cs:137–162](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Verification/UiSmoke.cs#L137) provide relevant future page verification. No receiver/audio failure or external issue-number relationship is established.
