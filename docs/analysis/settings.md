# Settings and the desktop shell

The panel, discovery, and the engine all read one in-memory `AppSettings`. That object is loaded from `%APPDATA%\AirFlash\config.json` and written back by identity migration, by the 200 ms volume timer, and by Settings → Apply. Playback uses a clone of it. The engine never reads the file.

## Load and the two backups

[`SettingsStore.Load`](../../desktop/AirFlash.Core/SettingsStore.cs) returns defaults when the file is missing. A present file is a JSON object. `schema_version` defaults to 1 when the field is absent.

| Version | What load does |
| --- | --- |
| Greater than 2 | Throws `NotSupportedException` ("This configuration is from a newer version. Update the app first."). That exception is not turned into defaults |
| Less than 2 | Rewrites the object in memory and remembers the original text. A receiver `auto_connect: false` becomes null, which follows the global switch. A schema-2 explicit false stays false across a reload. `capture_mode: virtual` becomes `endpoint`, and `capture_endpoint` is taken from `virtual_output_device` when that field exists. `custom_buffer_ms` on the root and on each receiver is clamped to 0–2000. `schema_version` is set to 2 |
| 2 | Deserialized as-is |

A `JsonException`, `IOException`, `UnauthorizedAccessException`, or `InvalidOperationException` becomes defaults plus `LoadWarning`. The original text is remembered the same way as a schema migration. Startup shows that warning in the panel notice.

After deserialize, null receiver maps become empty. `ReceiverAliases` keeps an entry only when the key and the value are both broadcast identities. Keys and values are normalized. Duplicate normalized keys keep the first value. An `endpoint:` string, a `stereo:` id, or a `host:port` written by an older save does not come back. The in-memory alias map built by [Identity](identity.md) can still contain a unique endpoint until the next launch.

The next `Save` writes the remembered original text to `config.json.pre-wpf.bak` once, when that backup is absent, then clears the remembered text. A later identity migration copies the current file to `config.json.pre-receiver-identity.bak` once, before the same `Save`. Those are different backups.

`Save` writes `config.json.<guid>.tmp` and then `File.Replace` when the destination exists, or `File.Move` when it does not. The temp file is deleted afterward. A failed save leaves the previous file. Unknown JSON fields ride along in `Extra` and are written back.

## What a field changes

[`AppSettings.Validate`](../../desktop/AirFlash.Core/Settings.cs) rejects a bad value before Apply. The ranges that reach playback are the same ones [Session](session.md) sends.

| Field | Default | What uses it |
| --- | --- | --- |
| `master_volume` | 100 | PCM gain is this value divided by 100, or 0 while the panel mute is on. `Gain` ignores every per-receiver volume |
| `latency_mode`, `custom_buffer_ms` | `normal`, 1000 | 120, 200, 500, or the custom value clamped to 0–2000. A receiver override wins when it is set |
| `stream_sample_rate` | `44100` | `44100` or `48000`. A change restarts the process |
| `capture_mode`, `capture_endpoint` | `loopback`, empty | Loopback sends a null endpoint, which is the default console render device. Endpoint mode requires a device id and restarts when it changes |
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

`ReceiverOptions.Volume` is stored, validated as 0–100, copied by `CopyFrom`, and merged when an identity moves. Nothing in the panel binds it, and `Gain` does not read it. The slider the user moves is [`DeviceVolumeState`](../../desktop/AirFlash.Core/DeviceVolumeState.cs), which is runtime state inside the session and is never written to this file. The comment on that type says it is not a PCM gain. Retired layout flags (`show_wider_volume`, `show_streaming_modes`, `show_master_control`) deserialize and then ignore the saved values.

## Apply

Settings edits a clone. Apply does not copy the draft over the live object.

1. `ReceiverCatalog.ApplyMigrations` rewrites both the draft and the baseline with the current alias map.
2. [`SettingsMerge.Merge`](../../desktop/AirFlash.Core/Settings.cs) is a three-way JSON patch. A field the draft did not change keeps the newest live value. A field the draft changed is taken from the draft. Nested objects, including one receiver's options, are patched the same way, field by field. This is why a master-volume edit made in the panel during an open Settings window survives Apply.
3. `Validate` runs on the merged object. Endpoint mode also refreshes the endpoint list and refuses an id that is not in it.
4. If `start_at_login` changed, the Run key is updated before the file write. A failed save tries to put the previous Run value back.
5. The live settings become the merged object, unless the panel edited them during the save. In that case a second merge keeps the panel edit. Receiver options removed in the draft are removed from the live map.
6. Turning global auto-connect from off to on clears the suppress flag and the attempted-id set.
7. A discovery-interface change calls `SetInterface`, which restarts browsing. Manual rows are merged and the panel is redrawn.
8. `Session.UpdateSettingsAsync` runs after the file is saved. A failure there leaves the file in place and returns "Settings saved, but audio could not be updated." A signature change restarts the process. Gain, equalizer, and the mute bit update in place. See [Session](session.md).

The 200 ms panel timer is a smaller save: it writes the current settings and calls `UpdateSettingsAsync`. It does not run the three-way merge. Discovery holds the same settings lock, so a discovery result waits until that save finishes, and a dirty volume edit is rescheduled afterward.

Startup, still under that lock, applies the Run key from `start_at_login` and migrates a capture endpoint that was saved as a friendly name. One name match replaces it with the device id. Zero or several matches leave the value and set the notice that the previous endpoint is unavailable. A later change of the default console render device, while capture mode is loopback and a session is active and not pairing, calls `StartAsync` again. If nothing is playing, that notification only restores mute.

## Manual receivers

[`ManualReceiverDialog`](../../desktop/AirFlash.App/Ui/ManualReceiverDialog.cs) requires a host that `Uri.CheckHostName` accepts and that is not IPv6. Unknown names are rejected. The port must be 1–65535. The box starts at 7000. Settings rejects a second manual row with the same host and port, compared case-insensitively.

`AddManual` stores the id as `host.ToLowerInvariant():port`. The name falls back to the host. Removing the row also removes its `ReceiverOptions`. Manual rows are written back over the known map on every discovery result. They stay online, and a discovery snapshot does not stop their session. Before `start`, the desktop resolves a hostname to one IPv4 address and fails the attempt when none exists.

## Shell around that file

A second process does not load a second catalog. [`SingleInstance`](../../desktop/AirFlash.App/Services/SingleInstance.cs) takes a local mutex named from the current user's SID and listens on a same-user named pipe. The second process writes one byte and exits. The owner treats that byte as "show the panel."

[`Autostart`](../../desktop/AirFlash.App/Services/Autostart.cs) writes `HKCU\Software\Microsoft\Windows\CurrentVersion\Run\AirFlash` as `"<process>" --startup` or deletes the value. `--startup` skips the initial panel. The tray menu is Open panel, Settings, Stop streaming, and Quit.

Language is chosen once in [`L`](../../desktop/AirFlash.Core/L.cs). `system` asks `GetUserPreferredUILanguages` and keeps the first preference whose language is `zh` or `en`. Anything else is English. The Chinese catalog is the embedded `Strings.zh.json`. Unknown English source strings stay English. Applying a different language waits until the settings transaction finishes, then rebuilds the panel, the tray, and an open Settings window at `ContextIdle`.

[`UpdateService`](../../desktop/AirFlash.Core/UpdateService.cs) GETs `https://api.github.com/repos/Ding-Kyoma/AirFlash/releases/latest` with a 15-second budget and a 1 MiB buffer. A missing release, a draft, or a prerelease yields no update. The tag must match `v?` plus three numeric parts. The download link is built from the repository constant and that tag. The response HTML is not used as a URL. The check does not download or install.

[`EndpointCatalog`](../../desktop/AirFlash.App/Services/EndpointCatalog.cs) lists active render endpoints, ordered by id. Device notifications restart a 200 ms timer and then refresh. A selected id that disappeared is shown as unavailable until Apply rejects it.

These process flags exit before a session exists:

| Flag | What it does |
| --- | --- |
| `--self-check` | Extracts the engine, runs `hello`, writes the desktop version and the hello object |
| `--discovery-check` | Browses for eight seconds and writes receivers and errors. No engine |
| `--audio-check` | Lists render endpoints. The note in the output says it does not play or mute |
| `--update-check` | Runs the GitHub check and writes the result |
| `--ui-smoke`, `--tray-smoke`, `--ui-perf` | UI harnesses. `--ui-language` can force the catalog for those runs |

`--output` or `--self-check-output` selects the JSON file. A failure on a check flag writes `{ ok: false }` and exits 1.
