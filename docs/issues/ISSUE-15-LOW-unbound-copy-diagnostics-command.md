# [LOW] Monitor diagnostics-copy access — resolved in fork main

**Status:** Implemented and integrated in own-fork main `8989b3609dd1294f4d1d6eb1502f345cc22bc382`, reviewed 2026-10-12. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`; this implementation is not yet integrated there. Severity is the original triage priority.

**Group:** H — Diagnostics/observability. **Feature head:** `6855dfe27b4283a58ede7c37474f3e2aa3c31cf7`. [Plan](IMPLEMENTATION_PLAN.md#issue-15), [acceptance](GROUP-H-ACCEPTANCE.md), [settings map](../analysis/settings.md), [failure snapshots](../analysis/failures.md).

## Current behavior and source trace

[Monitor Copy diagnostics button](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Ui/SettingsWindow.xaml#L211) binds the existing [CopyDiagnosticsCommand](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/ViewModels/SettingsViewModel.cs#L89), with localized content/automation name and keyboard access. It calls the same [Diagnostics serializer](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L609) on the STA dispatcher; clipboard exceptions still use Settings ShowError. A writer is injectable for isolated verification, avoiding the user's clipboard.

The report includes desktop/engine state, current local capture values and serialized fault/transport/pre-fault diagnostics. Added padding values are local fill accounting; end-to-end latency remains unmeasured. Copy does not save settings, send engine controls or mutate local audio.

## Acceptance and evidence

- Realized button/binding, visible/focusable/localized/automation invocation is verified in Idle, Streaming, Error and a fresh attempt.
- Copied text equals the existing serializer snapshot; writer sees STA/dispatcher.
- Clipboard-writer failure appears through the existing error surface.
- Hidden updates/reopen and immutable pre-fault snapshots appear in actual Monitor text.
- Cleanup is gated/joined before asserting Copy's absence of saves/engine/mute effects.

[actual-button fixture](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiDiagnostics.cs#L138) performs these observable checks. Three languages and both regional UI matrices are in H acceptance; no live clipboard/hardware operation is claimed.

Exact-main CI passed 667 Core, 124 Python and 69 native tests (recording-device qualification and thirty-minute localhost soak ignored). Packaged integration at `4ae1db4` passed six 1,059-assertion UI runs, three actual 125% DPI runs with 40 each and disposable MSI lifecycle; current runtime/package inputs are identical, with main's last commit documentation only. This refresh reviewed sources/records; it did not rerun suites or use real user data/devices.

## Remaining work

Upstream publication remains separate. No upload/duplicate logging/report serializer or unsupported report-issues button was added. Diagnostics may include endpoint/receiver identifiers; use synthetic examples when sharing fixtures.

## Historical finding

Original `41190e0` constructed CopyDiagnosticsCommand and localized its label but exposed no button/binding. Source established inaccessible combined-copy UI, not a playback fault or guaranteed selectable TextBlock report. The [archival report at the prior documentation commit](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/issues/ISSUE-15-LOW-unbound-copy-diagnostics-command.md) preserves original `41190e0` source anchors, reproduction/proposal and dated updates. Its proposal-only/open/pending-reboot labels are historical, superseded by the current resolution above.