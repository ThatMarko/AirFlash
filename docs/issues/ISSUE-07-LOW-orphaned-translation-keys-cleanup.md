# [LOW] Prune Superseded and Orphaned Translation Keys in `Strings.zh.json`

- **Issue ID**: ISSUE-07
- **Severity**: **LOW**
- **Subsystem**: Localization (`AirFlash.Core`)
- **Status**: Open / Triaged
- **Target Files**:
  - [`desktop/AirFlash.Core/Strings.zh.json`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/Strings.zh.json)
  - [`desktop/AirFlash.Tests/LocalizationTests.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Tests/LocalizationTests.cs)

---

## 1. Summary
A deep scan of [`Strings.zh.json`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/Strings.zh.json) against all C# and XAML references identified 12 localization keys that are no longer used anywhere in the codebase. These keys are remnants from earlier iterations of UI controls and diagnostics strings.

While these orphaned keys do not cause runtime failures, they bloat the embedded dictionary, increase translation maintenance overhead, and can mislead contributors.

---

## 2. Technical Inventory of Orphaned Keys
The following 12 keys exist only in `Strings.zh.json` and are unreferenced across the application:

1. `"Show latency mode"`
2. `"Show master volume"`
3. `"Use wider volume controls"`
4. `"Desktop 0.2.0 · WPF"`
5. `"Underrun packets"`
6. `"Dropped frames"`
7. `"Control panel"`
8. `"Device volume / % (blank uses 100%)"`
9. `"More"`
10. `"Statistics come from the current audio session and update about once per second. Target latency and local timing are not measurements of audio reaching HomePod."`
11. `"Local output is muted while streaming."`
12. `"Always retry connection"`

---

## 3. Proposed Solution
1. Remove the 12 obsolete keys from [`desktop/AirFlash.Core/Strings.zh.json`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/Strings.zh.json).
2. Add a test in [`desktop/AirFlash.Tests/LocalizationTests.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Tests/LocalizationTests.cs) that verifies all keys in `Strings.zh.json` are referenced in the codebase, preventing future dead string accumulation.
