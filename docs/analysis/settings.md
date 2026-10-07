# Settings and the desktop shell

The runtime analysis below retains the audited original behavior. The subsequently reported Settings scrollbar overlap and Equalizer wheel routing are documented separately in [scrolling acceptance](../issues/SETTINGS-SCROLL-ACCEPTANCE.md). Those UI changes are verified locally with A+B and now integrated into the user's fork main; original upstream main remains unchanged.

The desktop panel and discovery configuration use the view model's live `AppSettings`. That object is loaded from `%APPDATA%\AirFlash\config.json` and written back by identity migration, by the 200 ms panel-edit timer, and by Settings → Apply. Settings edits a separate draft and the session controller clones playback settings. The native engine receives selected values through JSONL commands; it shares no `AppSettings` object and never reads this file.

## Load and the two backups

[`SettingsStore.Load`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SettingsStore.cs) returns defaults when the file is missing. A present file is a JSON object. `schema_version` defaults to 1 when the field is absent.

| Version | What load does |
| --- | --- |
| Greater than 2 | Throws `NotSupportedException` ("This configuration is from a newer version. Update the app first."). That exception is not turned into defaults |
| Less than 2 | Rewrites the object in memory and remembers the original text. A receiver `auto_connect: false` becomes null, which follows the global switch. A schema-2 explicit false stays false across a reload. `capture_mode: virtual` becomes `endpoint`, and `capture_endpoint` is taken from `virtual_output_device` when that field exists. `custom_buffer_ms` on the root and on each receiver is clamped to 0–2000. `schema_version` is set to 2 |
| 2 | Deserialized as-is |

The recovery handler for `JsonException`, `IOException`, `UnauthorizedAccessException`, or `InvalidOperationException` sets `LoadWarning`, rereads the original file when it still exists, and returns defaults. That recovery read is outside another catch: if the file remains unreadable, its `File.ReadAllText` exception escapes instead of returning defaults. When recovery succeeds, the original text is remembered as for a schema migration, and startup shows the warning in the panel notice. `Load` does not run `Validate`; even schema-2 data can deserialize with unsupported enum strings or out-of-range values that are rejected only on Apply or handled by downstream clamps.

After deserialize, null receiver/alias maps and a null manual list become empty, and receiver options whose entire value is null are removed. `ReceiverAliases` keeps an entry only when the key and the value are both broadcast identities. Keys and values are normalized. Duplicate normalized keys keep the first value. An `endpoint:` string, a `stereo:` id, or a parsed endpoint written by an older save does not come back. Current [identity reconciliation](identity.md) uses unique endpoints only in transient receiver aliases and migrations; it does not add them to `ReceiverAliases`.

The next `Save` writes the remembered original text to `config.json.pre-wpf.bak` once, when that backup is absent, then clears the remembered text. `SaveReceiverIdentityAsync` first copies the current file to `config.json.pre-receiver-identity.bak` when `NeedsBackup` requests it and that backup is absent, then calls `Save`. These are different backups and may both be created during the same identity save. Alias additions alone do not request the identity backup.

`Save` writes `config.json.<guid>.tmp` and then `File.Replace` when the destination exists, or `File.Move` when it does not. The temp file is deleted afterward. A failure before replacement leaves the previous configuration; this is not a source-level power-loss durability guarantee, and a backup may already have been created. Unknown root fields ride along in `AppSettings.Extra`; the equalizer has its own `Extra`. `ReceiverOptions` and `ManualReceiver` have no extension-data store, so unknown fields inside those records are not preserved. `Save` itself does not call `Validate`.

## What a field changes

[`AppSettings.Validate`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Settings.cs) rejects a bad value before Apply. Validated playback values are selected by [Session](session.md); loaded settings are not automatically validated. Signature-based restarts below apply to an active non-pairing stream.

| Field | Default | What uses it |
| --- | --- | --- |
| `master_volume` | 100 | PCM gain is this value divided by 100, or 0 while the panel mute is on. `Gain` ignores every per-receiver volume |
| `latency_mode`, `custom_buffer_ms` | `normal`, 1000 | 120, 200, 500, or the custom value clamped to 0–2000. A receiver override wins when it is set |
| `stream_sample_rate` | `44100` | `44100` or `48000`. A change restarts the process |
| `capture_mode`, `capture_endpoint` | `loopback`, null | Loopback sends a null endpoint, which is the default console render device. Endpoint mode requires a device id and restarts an active non-pairing stream when its effective endpoint changes |
| `mute_while_streaming` | true | Local endpoint mute after `streaming`. See [Control and mute](control-and-mute.md) |
| `auto_connect_on_discover` | false | Arms the 900 ms timer. A receiver `auto_connect: false` overrides a global true |
| `force_reconnect`, `max_reconnect_attempts` | false, 5 | The retry loop in [Failures](failures.md). Attempts must be 1–20 |
| `standby_enabled`, `standby_silence_seconds` | false, 10 | UI label only. Threshold 5–300, with an optional per-receiver override |
| `equalizer` | disabled, flat | The filter in [Equalizer](equalizer.md) |
| `discovery_interface_id` | empty | Empty is all interfaces. Any other value must be a GUID. Apply calls `SetInterface` |
| `start_at_login`, `ui_language`, `theme` | false, `system`, `system` | Shell only. Language is `system`, `en`, or `zh`. Theme is `system`, `dark`, or `light` |
| `last_receiver_id` | empty | Auto-connect preference and identity priority |
| `receivers` | empty | Per-id hidden, auto-connect, latency, custom buffer, standby seconds, and a stored volume |
| `manual_receivers` | empty | Rows that discovery does not own |
| `receiver_aliases` | empty | Broadcast identity map. See [Identity](identity.md) |

`ReceiverOptions.Volume` is stored, validated as 0–100, copied by `CopyFrom`, and merged when an identity moves. Nothing in the panel binds it, and `Gain` does not read it. The slider the user moves is [`DeviceVolumeState`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/DeviceVolumeState.cs), which is runtime state inside the session and is never written to this file. The comment on that type says it is not a PCM gain. Retired layout flags (`show_wider_volume`, `show_streaming_modes`, `show_master_control`) deserialize and then ignore the saved values.

## Apply

Settings edits a clone. Apply does not copy the draft over the live object.

1. `ReceiverCatalog.ApplyMigrations` rewrites both the draft and the baseline with the current alias map.
2. [`SettingsMerge.Merge`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Settings.cs) is a three-way JSON patch. A field the draft did not change keeps the newest live value. A field the draft changed is taken from the draft. Nested objects, including one receiver's options, are patched the same way, field by field. This is why a master-volume edit made in the panel during an open Settings window survives Apply.
3. `Validate` runs on the merged object. Endpoint mode also refreshes the endpoint list and refuses an id that is not in it.
4. If `start_at_login` changed, the Run key is updated before the file write. A failed save tries to put the previous Run value back.
5. The live settings become the merged object, unless the panel edited them during the save. In that case a second merge keeps the panel edit. Receiver options removed in the draft are removed from the live map.
6. Turning global auto-connect from off to on clears the suppress flag and the attempted-id set.
7. A discovery-interface change calls `SetInterface`, which restarts browsing. Manual rows are merged and the panel is redrawn.
8. `Session.UpdateSettingsAsync` runs after the file is saved. A failure there leaves the file in place and returns "Settings saved, but audio could not be updated." A signature change restarts the process. Gain, equalizer, and the mute bit update in place. See [Session](session.md).

The 200 ms panel timer is a smaller save: it writes a settings snapshot and calls `UpdateSettingsAsync`. It is used for master-volume, panel latency, and last-receiver edits, not device-volume changes. It does not run the three-way merge. Discovery uses the same asynchronous settings gate, so a discovery result waits until that save finishes; a concurrent panel edit remains dirty and is saved afterward. During Apply, `UpdateSettingsAsync` receives the saved `merged` snapshot even if a panel edit arrived during the save; the later dirty-panel flush brings that newer edit to disk and playback.

Startup, still under that gate, applies the Run key from `start_at_login` and migrates a capture endpoint that was saved as a friendly name. One exact name match replaces it with the device id in memory; this helper does not immediately save that migration. Zero or several matches leave the value and set the notice that the previous endpoint is unavailable. A later change of the default console render device, while capture mode is loopback and a session is active and not pairing, calls `StartAsync` again. With no active session, the endpoint notification refreshes the catalog and attempts mute restore without starting playback.

## Manual receivers

[`ManualReceiverDialog`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Ui/ManualReceiverDialog.cs) requires a host that `Uri.CheckHostName` accepts and that is not IPv6. Unknown names are rejected. The port must be 1–65535. The box starts at 7000. Settings rejects a second manual row with the same host and port, compared case-insensitively.

These host-syntax and duplicate checks belong to the UI add path. `AppSettings.Validate` only checks that each manual host is nonblank and its port is in range; it does not perform DNS, reject IPv6 syntax, validate ids, or detect duplicate manual rows in a hand-written file. `Uri.CheckHostName` is a syntax check, not proof that a DNS hostname exists.

`AddManual` stores the id as `host.ToLowerInvariant():port`. The name falls back to the host. Removing the row also removes its `ReceiverOptions`. Manual rows are written back over the known map on every discovery result. They stay online, and a discovery snapshot does not stop their session. Before `start`, the desktop resolves a hostname to one IPv4 address and fails the attempt when none exists.

## Shell around that file

For an ordinary GUI launch, a second process exits before loading a second catalog. [`SingleInstance`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/SingleInstance.cs) takes a `Local\` mutex named from the current user's SID (username fallback) and listens on a same-user named pipe. The second process writes one byte and exits. The owner treats any received byte as activation: an open Settings window is activated, otherwise the panel is shown. Check and UI-harness flags are handled before the normal single-instance path.

[`Autostart`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/Autostart.cs) writes `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\AirFlash` as `"<process>" --startup` or deletes the value. `--startup` skips the initial panel. The tray menu is Open panel, Settings, Stop streaming, and Quit.

[`L`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/L.cs) is initialized before startup UI creation, then reinitialized from saved language settings during normal startup. `system` asks `GetUserPreferredUILanguages` and keeps the first preference whose language is `zh` or `en`; if none match, it uses English. The Chinese catalog is the embedded `Strings.zh.json`. Unknown English source strings stay English. Applying a different language reinitializes `L` after the settings transaction finishes, then rebuilds the panel, the tray, and an open Settings window at `ContextIdle`.

[`UpdateService`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/UpdateService.cs) GETs `https://api.github.com/repos/Ding-Kyoma/AirFlash/releases/latest` with a 15-second budget. The Settings and command-check callers configure their HTTP clients with a 1 MiB response buffer; the service accepts an injected client and does not itself set that limit. HTTP 404, a draft, or a prerelease returns no stable release. The tag must match optional lowercase `v` plus three numeric parts; numeric version comparison happens in `UpdateCheckState`. The download link is built from the repository constant and that tag. The response `html_url` is not used as a destination. The check does not download or install.

[`EndpointCatalog`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/EndpointCatalog.cs) caches active render endpoints supplied by the MTA `AudioService`, ordered by id. Device notifications restart a 200 ms dispatcher timer and then refresh. Concurrent refresh requests join one task; a generation change during enumeration triggers another serial enumeration. A selected id that disappeared is shown as unavailable in the Settings draft, and Apply with endpoint mode rejects it unless it has returned by the fresh validation enumeration.

These process flags bypass the normal single-instance/catalog startup and exit when their check or harness completes. The UI harnesses instantiate mock session controllers:

| Flag | What it does |
| --- | --- |
| `--self-check` | Extracts the engine, runs `hello`, writes the desktop version and the hello object |
| `--discovery-check` | Browses for eight seconds and writes receivers and errors. No engine |
| `--audio-check` | Lists render endpoints. The note in the output says it does not play or mute |
| `--update-check` | Runs the GitHub check and writes the result |
| `--ui-smoke`, `--tray-smoke`, `--ui-perf` | UI harnesses. `--ui-language` can force the catalog for those runs |

`--output` or `--self-check-output` selects the JSON file through `App.WriteOutput`; no JSON result file is written unless a flag is followed by a path. An exceptional check failure includes `error` alongside `ok: false` and exits 1; a discovery check with collected failure strings instead writes its normal receivers/errors object with `ok: false`. UI smoke/tray smoke choose the directory for rendered artifacts from `--output`, otherwise a temporary location, but their final JSON still goes through `App.WriteOutput`. `--self-check` opens a real short-lived engine for `hello`, without a playback session.

## Source and verification scope

| Claim group | Source anchors |
| --- | --- |
| Load, sanitization, warning recovery, two backup paths, replacement | [`SettingsStore.cs:16–84`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SettingsStore.cs#L16) |
| Defaults, effective gain/latency/endpoint, validation, extension data | [`Settings.cs:7–110`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Settings.cs#L7), [`EqualizerSettings.cs:13`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/EqualizerSettings.cs#L13) |
| Field-level merge | [`Settings.cs:144–167`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/Settings.cs#L144) |
| Startup migration and endpoint restart | [`AppViewModel.cs:103–149`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L103) |
| Panel persistence and Apply transaction | [`AppViewModel.cs:338–415`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L338) |
| Manual UI checks, add/remove, session IPv4 resolution | [`ManualReceiverDialog.cs:30–34`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Ui/ManualReceiverDialog.cs#L30), [`SettingsViewModel.cs:224–240`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/SettingsViewModel.cs#L224), [`SessionController.cs:455–459`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SessionController.cs#L455) |
| Single instance and autostart | [`SingleInstance.cs:12–40`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/SingleInstance.cs#L12), [`Autostart.cs:12–17`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/Autostart.cs#L12), [`App.xaml.cs:45–66`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/App.xaml.cs#L45) |
| Language, update and endpoint catalogs | [`L.cs:13–63`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/L.cs#L13), [`App.xaml.cs:77–101`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/App.xaml.cs#L77), [`UpdateService.cs:10–33`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/UpdateService.cs#L10), [`EndpointCatalog.cs:19–83`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/EndpointCatalog.cs#L19) |
| Check dispatch and outputs | [`App.xaml.cs:21–75, 125–166`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/App.xaml.cs#L21) |

[`SettingsTests`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/SettingsTests.cs#L9) covers cloning, schema migration/backups, root unknown fields, three-way merging, defaults, latency/gain, sample rates, and future-schema rejection. [`ReceiverIdentityTests.cs:242–272`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/ReceiverIdentityTests.cs#L242) covers identity backups. [`LocalizationTests`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/LocalizationTests.cs#L16) and [`UpdateTests`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/UpdateTests.cs#L18) cover pure language selection and mocked release responses. The WPF [`UiRegression` harness](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Verification/UiRegression.cs#L11), invoked by `--ui-smoke`, adds mock-based checks of save responsiveness, concurrent panel edits, startup rollback, endpoint validation, and network/manual catalog behavior. Those are available checks, not a live registry, DNS-SD, GitHub-service, or receiver playback validation performed by this document audit.
