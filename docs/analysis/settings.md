# Settings and persistence

## Current settings/application boundary — 2026-10-21

Original stable is now schema-4 `439de626`; the schema-2 table below belongs to its explicitly pinned historical source. Combined `6c1e2ef` preserves canonical persisted settings while projecting a private effective copy for an owned transport whose identity-save raced with newer Pair. Ordinary absence of an old option key is not an explicit reset; raw reset/default and normalized Apply membership intent remain observable, while canonical edits/clears need a unique matching current transport. [Current acceptance/source evidence](../issues/PR-READINESS-2026-10-21.md) records the forty-case Core and actual modal continuations. This pass does not change the settings-store recovery or serialized schema.

This page describes fork `main` at **`06237873dcb6394eb35e97a017c536843bd7743d`**, reviewed on 2026-10-12, including original stable `cf420d97700463c56adc31408510e8336e99e51c`. Final C `5d72da3` is pushed/integrated here, not merged upstream. [Audit](audit.md) and [C acceptance](../issues/GROUP-C-ACCEPTANCE.md) preserve provenance and limits.

Live `AppSettings` belongs to WPF and loads from `%APPDATA%\AirFlash\config.json`. The normal shell performs two Load calls on one store: initial language/theme inspection and view-model construction. Settings edits a draft; the controller clones playback settings; Rust receives selected JSONL values and never reads this file.

## Load and the two backups

[SettingsStore.Load](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/SettingsStore.cs) reads original bytes once inside its guarded read and decodes through BOM-compatible StreamReader. It never rereads an unreadable path inside an unguarded recovery catch. Initial file/directory-not-found means genuine absence; access/I/O uncertainty means preservation pending.

| Schema | Load behavior |
| --- | --- |
| Positive integer greater than 2 | Fatal unsupported-version result and persistence latch |
| Missing/null version or supported older integer | Schema-1 migration, then normalization/validation |
| 2 | Normalization/validation before return |
| Ambiguous/malformed version | Controlled invalid-input recovery; future integer detection wins over duplicate supported spelling |

ReadVersion examines every case-insensitive schema property before bounded conversion. A huge future integer cannot become defaults through overflow; fraction/exponent/string spellings follow controlled integer validation. Schema-1 migration changes receiver automatic false to inherited null, virtual capture to endpoint, clamps legacy custom buffers and sets schema 2. Schema-2 explicit automatic false remains false.

Normalization defaults null receiver/alias maps and manual list, removes null receiver-option values and retains only normalized broadcast-to-broadcast aliases. Null manual **elements**, invalid/null rates, bad structure/enums/ranges/EQ produce controlled recovery after [AppSettings.Validate](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/Settings.cs#L88), never an invalid object returned to catalog/playback. Missing rate retains its documented default. A selected unavailable adapter remains saved, not normalized to all interfaces.

Readable rejected/migrated bytes become exact-byte preservation obligations, including BOM/encoding. Unreadable originals retain an uncaptured obligation. Repeated Load, later disappearance or defaults do not discharge it. Warnings distinguish “original not read; saving blocked” from “captured original requires backup”; no backup is claimed before it exists.

Save and identity Save share the store lock. When recovery is pending they inspect/preserve current source revisions before replacement and reject newly readable future schema. Every required raw-byte recovery backup is written/reused only after byte comparison. An identical `.pre-wpf.bak` can be reused; differing existing content stays intact and a unique sibling preserves the next revision. Failure retains obligations for retry; successful replacement clears them and updates the warning with actual backups.

Identity migration separately requests `.pre-receiver-identity.bak` only when preferences/last-used moved; self-alias addition alone does not. Existing identity backup is preserved. [Identity](identity.md) owns that migration decision; unreadable preservation can still prevent identity Save from replacing defaults.

Save writes a unique UTF-8 temp, replaces the existing destination or moves when genuinely absent, and cleans failed temp output. It does not call semantic Validate itself. Unknown root and equalizer fields have extension-data stores; unknown fields inside ReceiverOptions/ManualReceiver do not. The lock does not coordinate arbitrary external writers. Ordinary save after a valid Load does not inspect every later external schema change. File replacement and byte backups are not a physical power-loss durability guarantee.

## What a field changes

| Field | Default | Runtime consumer |
| --- | --- | --- |
| Master volume | 100 | PCM gain/100; panel mute sends zero |
| Latency/custom buffer | normal/1000 | Realtime 120, normal 200, buffered 500, custom 0–2000 ms; receiver override wins |
| Stream sample rate | `44100` | Valid `44100`/`48000`; change restarts active non-Pairing stream |
| Capture mode/endpoint | loopback/null | Default playback loopback or selected endpoint id; current native capture supports recording endpoints too |
| Mute while streaming | true | Exact playback-endpoint mute/recovery; recording endpoints are excluded |
| Automatic connection | false | Dedicated 900 ms admission; per-receiver nullable override |
| Force reconnect/maximum | false/5 | Desktop retry; allowed maximum 1–20 |
| Standby/threshold | false/10 | Local silence label; threshold 5–300 seconds, no engine suspension |
| Equalizer | disabled/flat | Preview/apply/native preparation, described in [Equalizer](equalizer.md) |
| Discovery interface | empty | Empty means all; nonempty valid GUID can remain unavailable/paused |
| Startup/language/theme | false/system/system | Shell; language system/en/zh, theme system/dark/light |
| Last receiver/options/manual rows/aliases | empty | Ranking, independent preferences/manual endpoints and broadcast migration |

Stored ReceiverOptions.Volume remains preserved/validated/merged but is not master PCM gain or the runtime device-volume slider. DeviceVolumeState is not persisted in this schema. Retired panel layout flags deserialize but ignore saved values.

## Apply

[AppViewModel.ApplyAsync](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L545) serializes with discovery/panel persistence using the asynchronous settings gate:

1. Migrate draft and baseline with current receiver migrations.
2. Three-way field-level merge: unchanged draft fields keep latest live edits; changed fields take draft values, including nested options.
3. Validate; endpoint mode refreshes actual endpoint catalog and rejects a missing selection.
4. Apply changed autostart before saving; save failure attempts to restore its previous Run value.
5. Persist, then retain concurrent panel edits via another merge when revision changed.
6. Turning global automatic connection on resets suppression/attempts; interface change restarts demand under selected-interface policy; manual rows/controls/recovery readiness refresh.
7. Update controller settings after save. Failure preserves the valid save and returns “Settings saved, but audio could not be updated.” Applicable signature changes restart; gain/EQ/mute otherwise update in place.
8. Schedule automatic admission with current eligibility; a separate dirty panel flush persists edits that arrived during Apply.

The 200 ms panel flush saves a snapshot and updates the session without a full draft three-way merge. It tracks edit revisions and reschedules newer edits. Discovery waits behind persistence, but direct Pair invalidates older automatic intent immediately without holding the gate across PIN input. Saving success does not authorize stale auto-connect/retargeting; [After discovery](after-discovery.md) describes those guards.

Startup initializes mute recovery outside settings/lifecycle locks, then reads the default endpoint, applies autostart and migrates an old friendly endpoint name only for one exact match. That in-memory migration need not immediately save. Default-loopback device changes use guarded owner/intent restart; inactive endpoint-return restoration rechecks authority after worker settlement. No endpoint notification blindly starts a new receiver.

## Manual receivers

[ManualReceiverDialog](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Ui/ManualReceiverDialog.cs) requires syntax accepted by Uri.CheckHostName, excludes IPv6, and validates port 1–65535. The add path prevents duplicate host/port case-insensitively and uses lowercase host:port identity. Remove also removes options.

Loaded validation instead requires nonnull element, nonempty unique ordinal id, nonblank host and valid port. It does not perform DNS or duplicate every stricter dialog rule; valid nullable display names/overrides remain compatible. Manual rows persist independently of snapshots, and exact selection is never replaced by a stereo group merely sharing an address. Settings Pair can use an unsaved manual editor row, but session settings come from the applied application snapshot, not its unsaved receiver options. Cancelling Settings discards that configuration draft, not credentials already paired under the accessory identity. Before playback, hostname resolution selects IPv4 or fails under [Session](session.md).

## Shell around that file

[Desktop shell](desktop-shell.md) maps startup/readiness, tray and dialog lifetimes, placement, themes, external actions and shutdown through their production callers. This section retains the configuration-facing boundary.

[SingleInstance](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Services/SingleInstance.cs) uses a current-user local mutex and named-pipe activation; a second ordinary launch exits before loading another catalog. Autostart writes/deletes the current-user Run value with `--startup`, which hides the initial panel. Tray retains Open panel, Settings, Stop and Quit.

[L](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/L.cs) supports English, Simplified and Traditional Chinese. System zh-TW/zh-HK/zh-MO picks Traditional; other zh picks Simplified; missing translated keys fall back to source English. Persisted choices remain system/en/zh. Language Apply rebuilds panel/tray/Settings after its transaction. Harness regional culture and selected UI catalog are independent; tray fixtures restore initial culture/catalog exactly. H removes only reviewed unused keys, preserving dynamic recording labels/presets/format arguments.

[UpdateService](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/UpdateService.cs) still checks **original Ding-Kyoma releases/latest**, not the fork, with fifteen-second cancellation. Callers bound HTTP response buffering to 1 MiB. 404/draft/prerelease means no stable update; a validated version constructs a trusted original-repository tag link. It downloads/installs nothing. A fork build therefore does not automatically discover unofficial fork releases.

[EndpointCatalog](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Services/EndpointCatalog.cs) refreshes actual playback/recording endpoint metadata through MTA AudioService, joins concurrent enumeration and processes debounced notifications with generation checks. Missing selected endpoints remain unavailable in drafts; Apply requires fresh presence. Actual opened device direction/identity is authoritative for capture/mute, not the displayed catalog label.

## Check dispatch and outputs

[Program](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Program.cs) dispatches DNS helper/helper-check before WPF/logging/application data. Other actually dispatched diagnostic modes choose isolated startup data before logging/platform probes. [App.OnStartup](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/App.xaml.cs) handles them before ordinary single-instance/store startup.

| Mode | Boundary |
| --- | --- |
| `--self-check`/`--compat-check` | Extract/hello/compatibility checks; real short-lived engine, no playback |
| `--discovery-check`/`--discovery-perf` | Actual DNS-only checks; no receiver playback |
| `--audio-check` | Read-only playback/recording enumeration; no playback/mute |
| `--update-check` | Original GitHub update query |
| `--ui-smoke`/`--tray-smoke`/`--ui-perf` | Fake-service WPF harnesses, isolated outputs |
| `--stability-check` | Receiver-free repeated synthetic stability checks |
| `--dns-resolve-helper-check` | Six synthetic real children, no DNS query |

`--ui-language` selects catalog; `--ui-culture` sets harness culture before WPF async contexts; `--ui-diagnostics-dpi` verifies observed DPI only in the fake window. `--output`/`--self-check-output` selects JSON output; harness images use that directory or a temporary one. Diagnostic data/log isolation does not redirect `AppPaths.ExtractEngine`: self/compatibility checks can populate the normal `%LOCALAPPDATA%\\AirFlash\\engine\\{hash}` cache. Fake UI modes use mock factories instead. Exceptions report controlled failure/exit status. These flags are not a production release build or a device-qualification authorization.

## Source and verification scope

Current [SettingsRecoveryTests](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/SettingsRecoveryTests.cs), [SettingsTests](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/SettingsTests.cs), [LocalizationTests](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/LocalizationTests.cs), [UpdateTests](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/UpdateTests.cs) and separate WPF [UiSettingsRecovery](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Verification/UiSettingsRecovery.cs) cover locked/readable-invalid/BOM inputs, repeated loads, I/O failures, schema migration/future rejection, no rejected-input playback/mute, unknown fields and concurrent Apply. Integrated startup recovery assertions total twenty-two, superseding the older nineteen-case composition.

Current main's 667 Core/124 Python/build/CI and code-identical complete package UI/DPI/MSI qualification are recorded in [current acceptance](../issues/CONTINUATION-ACCEPTANCE.md). [Scrolling acceptance](../issues/SETTINGS-SCROLL-ACCEPTANCE.md) records actual wheel/layout evidence. No real configuration, registry, endpoint, credentials or receiver was accessed by this documentation review.

## Historical baseline

At [`41190e0`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SettingsStore.cs#L16), recovery reread the failing path unguarded and Load returned without semantic validation. The original reports [06](../issues/ISSUE-06-MEDIUM-settings-recovery-reread-escapes.md)/[07](../issues/ISSUE-07-MEDIUM-invalid-loaded-settings-reach-runtime.md) and C acceptance retain original controlled failures. Old source-only/pre-reboot audits and superseded branch counts remain historical evidence, not current pending work.
