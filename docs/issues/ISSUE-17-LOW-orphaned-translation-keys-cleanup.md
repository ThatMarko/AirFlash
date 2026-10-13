# [LOW] Unused Chinese translation cleanup — completed in fork main

**Status:** Implemented and integrated in own-fork main `8989b3609dd1294f4d1d6eb1502f345cc22bc382`, reviewed 2026-10-12. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`; this implementation is not yet integrated there. Severity is the original triage priority.

**Group:** H — Diagnostics/observability. **Feature head:** `6855dfe27b4283a58ede7c37474f3e2aa3c31cf7`. [Plan](IMPLEMENTATION_PLAN.md#issue-17), [acceptance](GROUP-H-ACCEPTANCE.md), [settings/localization](../analysis/settings.md), [EQ dynamic presets](../analysis/equalizer.md).

## Current state and bounded scope

Ten reviewed entries are removed from both [Simplified](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/Strings.zh.json) and [Traditional](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/Strings.zh-TW.json) catalogs:

| Removed historical key |
| --- |
| More |
| Control panel |
| Use wider volume controls |
| Show latency mode |
| Show master volume |
| Device volume / % (blank uses 100%) |
| Underrun packets |
| Dropped frames |
| Statistics come from the current audio session and update about once per second. Target latency and local timing are not measurements of audio reaching HomePod. |
| Desktop 0.2.0 · WPF |

This is the specific reviewed cleanup, not a claim that every unused catalog entry has been proven absent. The original two extra candidates (“Local output is muted while streaming.” and “Always retry connection”) were already absent and withdrawn.

## Caller and consumer trace

[language selection](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/L.cs#L15) supports English, Simplified and Traditional preferences (zh-TW/HK/MO choose Traditional); exact-English lookup falls back to English. Format uses current regional culture, so UI catalog selection is not a numeric formatting guarantee.

Literal C#/XAML references and dynamically selected page/choice/preset/status values were reviewed on the final implementation tree before removal. Recording labels, Coffee/donation text, EQ preset names, localized diagnostics/padding labels, current Monitor caveat and generated version text remain. Retired serialized layout options remain accepted separately for settings compatibility.

No universal “every key must appear as a source literal” test was added: it would misclassify dynamic or intentionally prepared translations. No deletion-mirroring key list was added to tests.

## Acceptance and evidence

- Only the reviewed ten candidates were removed from both catalogs after dynamic review.
- Both embedded JSON dictionaries load with nonempty values and format-argument parity.
- Current page/control/preset/capture-direction labels work in all three catalogs and regional matrices.
- Temporary language switches restore the selected catalog; no runtime/controller/config/JSONL fields were removed.

[localization tests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/LocalizationTests.cs) and actual WPF matrices supply compatibility evidence. H acceptance pins the final source and retained tests; cleanup itself claims maintenance benefit rather than playback repair.

Exact-main CI passed 667 Core, 124 Python and 69 native tests (recording-device qualification and thirty-minute localhost soak ignored). Packaged integration at `4ae1db4` passed six 1,059-assertion UI runs, three actual 125% DPI runs with 40 each and disposable MSI lifecycle; current runtime/package inputs are identical, with main's last commit documentation only. This refresh reviewed sources/records; it did not rerun suites or use real user data/devices.

## Remaining work

Upstream publication is separate. Future candidates need their own dynamic/caller review; avoid turning a static occurrence count into proof of reachability.

## Historical finding

Original `41190e0` had ten present Simplified entries without exact desktop literal occurrences; that scan alone was not dynamic reachability proof. Stable later added Traditional. Final H implementation reviewed both catalogs and current dynamic sources before removing only the retained candidates. The [archival report at the prior documentation commit](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/issues/ISSUE-17-LOW-orphaned-translation-keys-cleanup.md) preserves original `41190e0` source anchors, reproduction/proposal and dated updates. Its proposal-only/open/pending-reboot labels are historical, superseded by the current resolution above.