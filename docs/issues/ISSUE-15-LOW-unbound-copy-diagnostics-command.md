# [LOW] Settings does not expose its existing diagnostics-copy command

- **Issue ID**: ISSUE-15
- **Implementation plan**: [Work package ISSUE-15](IMPLEMENTATION_PLAN.md#issue-15)
- **PR group**: H — Diagnostics/observability
- **Severity**: LOW; diagnostics access/usability, without a demonstrated playback failure
- **Kind**: Defect — UI binding
- **Subsystem**: Desktop App (`AirFlash.App`)
- **Status**: Open — source-confirmed UI binding gap
- **Runtime baseline**: upstream `41190e0`; documentation baseline `c077a05`
- **Evidence status**: Command/XAML references inspected; no live WPF clipboard reproduction performed
- **Implementation status**: Proposal only; command binding remains absent
- **Target files**: [SettingsViewModel.cs:62,89](../../desktop/AirFlash.App/ViewModels/SettingsViewModel.cs#L62), [SettingsWindow.xaml:204–299,314–336](../../desktop/AirFlash.App/Ui/SettingsWindow.xaml#L204)
- **Analysis context**: [Settings](../analysis/settings.md), [Session diagnostics](../analysis/session.md), [Failures](../analysis/failures.md)

## Current behavior and effect

`SettingsViewModel` exposes `CopyDiagnosticsCommand` at [line 62](../../desktop/AirFlash.App/ViewModels/SettingsViewModel.cs#L62) and initializes it at [line 89](../../desktop/AirFlash.App/ViewModels/SettingsViewModel.cs#L89). Execution calls `Clipboard.SetText(App.Diagnostics())`, catching errors through `ShowError`. The localized label exists at [Strings.zh.json:199](../../desktop/AirFlash.Core/Strings.zh.json#L199).

There is no command binding or invocation elsewhere in the scanned desktop C#/XAML source. The Monitor page at [SettingsWindow.xaml:204–299](../../desktop/AirFlash.App/Ui/SettingsWindow.xaml#L204) displays status, capture/transport values, and fault history, but offers no diagnostics-copy action. The About page at [lines 314–336](../../desktop/AirFlash.App/Ui/SettingsWindow.xaml#L314) offers update, license, log-folder, and donation actions; it does not contain the previously alleged report-issues button either.

Users can still open logs and inspect the monitor. The specific gap is that the implemented combined clipboard report is inaccessible through this UI. The original claim that users can manually select all displayed values is not established: the monitor uses `TextBlock` controls, not a guaranteed selectable report editor.

## Root cause and report contents

The command is constructed but never connected to a UI action. [AppViewModel.Diagnostics():418](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L418) produces desktop/engine version, current status, local capture values, and serialized playback diagnostics, including saved fault state when available. Its text explicitly says acoustic latency is not measured. The command does not save settings, open receiver sockets, or alter playback.

## Deterministic reproduction plan

Use the existing WPF mock-service harness; no receiver is needed:

1. Construct Settings with an idle or synthetic streaming `AppViewModel` and visit Monitor/About.
2. Enumerate realized buttons/keyboard actions and their command bindings.
3. Current source predicts no element bound to `CopyDiagnosticsCommand`.
4. Execute the view-model command directly on an STA dispatcher with a controlled clipboard to verify that the report exists independently of the missing action.

This is a proposed UI regression scenario, not a recorded clipboard run. Use synthetic device/host identifiers when testing or sharing the resulting report.

## Proposed implementation and acceptance

Add one clearly labeled `Copy diagnostics` button to Monitor, bound to the existing command and using the existing localization entry. Keep clipboard work on the WPF dispatcher and preserve the current exception-to-error path. Do not add another diagnostics serializer.

- The action is visible and keyboard-accessible on Monitor in English and Chinese, including Idle/Error states.
- Clicking it copies the same snapshot text as `App.Diagnostics()` and does not restart/stop playback or save settings.
- A clipboard-access exception appears in the settings error surface without escaping the UI handler.
- A WPF fixture asserts the actual binding/action rather than only constructing the view model.

## Existing and missing verification

[UiSmoke.cs:137–176](../../desktop/AirFlash.App/Verification/UiSmoke.cs#L137) already walks settings pages and checks synthetic transport metrics; [UiRegression.cs:128–132](../../desktop/AirFlash.App/Verification/UiRegression.cs#L128) checks persistent monitor rows. Neither currently verifies a diagnostics-copy action. [SessionTests.cs:241–302](../../desktop/AirFlash.Tests/SessionTests.cs#L241) covers the controller's warning/fault snapshots, not clipboard access.

No JSONL or schema-2 configuration change is needed. No source-supported causal relationship to upstream audio/disconnect issues is claimed.
