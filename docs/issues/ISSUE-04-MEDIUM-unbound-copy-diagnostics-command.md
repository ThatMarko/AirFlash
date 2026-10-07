# [MEDIUM] Unbound `CopyDiagnosticsCommand` in Settings UI

- **Issue ID**: ISSUE-04
- **Severity**: **MEDIUM**
- **Subsystem**: Desktop Frontend UI (`AirFlash.App`)
- **Status**: Open / Triaged
- **Target Files**:
  - [`desktop/AirFlash.App/ViewModels/SettingsViewModel.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/ViewModels/SettingsViewModel.cs#L62)
  - [`desktop/AirFlash.App/Ui/SettingsWindow.xaml`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Ui/SettingsWindow.xaml#L240-L340)
  - [`desktop/AirFlash.Core/Strings.zh.json`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/Strings.zh.json#L24)

---

## 1. Summary
The desktop application ViewModel implements [`CopyDiagnosticsCommand`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/ViewModels/SettingsViewModel.cs#L62), which formats current session health, engine status, queue ages, dropped frames, and member metrics, and places them onto the system clipboard.

Furthermore, localization for this feature is already complete in [`Strings.zh.json`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/Strings.zh.json) (`"Copy diagnostics": "复制诊断信息"`). However, no button or interactive element is bound to this command in [`SettingsWindow.xaml`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Ui/SettingsWindow.xaml). Consequently, users and testers have no easy mechanism to copy diagnostic telemetry when troubleshooting connection issues.

---

## 2. Technical Root Cause Analysis
In [`desktop/AirFlash.App/ViewModels/SettingsViewModel.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/ViewModels/SettingsViewModel.cs#L62):

```csharp
CopyDiagnosticsCommand = new(() =>
{
    try { Clipboard.SetText(App.Diagnostics()); }
    catch (Exception error) { ShowError(error); }
});
```

In [`desktop/AirFlash.App/Ui/SettingsWindow.xaml`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Ui/SettingsWindow.xaml):
- Page 5 (Monitor) displays real-time text blocks for latency, queue age, underruns, and member transport metrics, but contains no copy button.
- Page 7 (About) contains action buttons for checking updates, opening logs, and reporting issues, but omits the diagnostics copy action.

---

## 3. Reproduction Steps
1. Launch AirFlash.
2. Open Settings and navigate to the **Monitor** or **About** tab.
3. Attempt to copy the session diagnostics to report a problem.
4. **Observed Result**: Only individual text fields can be manually selected/inspected; no "Copy diagnostics" button exists in the interface.
5. **Expected Result**: A "Copy diagnostics" button is present and copies the structured telemetry report produced by `AppViewModel.Diagnostics()`.

---

## 4. Proposed Solution
Add a button in [`SettingsWindow.xaml`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/Ui/SettingsWindow.xaml) in either the Monitor tab or the About tab:

```xaml
<Button Content="{loc:Text 'Copy diagnostics'}" 
        Command="{Binding CopyDiagnosticsCommand}" 
        Margin="0,0,0,12" 
        HorizontalAlignment="Left"/>
```
