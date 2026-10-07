# [MEDIUM] Semantically invalid loaded settings reach startup and playback

- **Issue ID**: ISSUE-23
- **Severity**: MEDIUM
- **Kind**: Defect — persisted configuration validation
- **Status**: Open; proposal only, not implemented
- **Evidence status**: Source and serializer contract verified; the synthetic fixtures below have not been run against AirFlash
- **Implementation status**: Proposal only; no runtime change applied
- **Runtime baseline**: `41190e0`; documentation baseline `c077a05` and audited [settings](../analysis/settings.md), [session](../analysis/session.md), and [after-discovery](../analysis/after-discovery.md) pages
- **Target files**: [SettingsStore.cs:16–55](../../desktop/AirFlash.Core/SettingsStore.cs#L16-L55), [Settings.cs:88–110](../../desktop/AirFlash.Core/Settings.cs#L88-L110), [AppViewModel.cs:78–97](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L78-L97)
- **Analysis context**: [Settings](../analysis/settings.md), [Session](../analysis/session.md), [After discovery](../analysis/after-discovery.md)

## 1. Current behavior and impact

`SettingsStore.Load` parses and deserializes configuration, normalizes several collections, and returns without invoking `AppSettings.Validate`. It filters null entries from `Receivers` but only replaces a null `ManualReceivers` collection; it does not reject null elements. Valid JSON can therefore enter the running app while violating assumptions already checked by Settings Apply.

Two concrete source paths demonstrate the gap:

- `manual_receivers: [null]` survives loading. The view-model constructor calls `MergeManualReceivers`, whose `r.Id` projection dereferences the null element. Normal startup has no usable view model and follows its startup-failure handling.
- `stream_sample_rate: "invalid"` survives loading despite `Validate` rejecting it. A later playback attempt reaches `int.Parse` while constructing `start` and fails before that command is sent.

These inputs are synthetic malformed configuration, not normal Settings UI output. No claim is made that ordinary saves produce them or that a real installation currently contains them. The defect is that the persistence boundary does not enforce its own runtime assumptions.

## 2. Source trace

| Step | Evidence |
| --- | --- |
| Loader deserializes, removes null receiver-option values, normalizes collections/aliases, then returns without validation | [SettingsStore.cs:40–49](../../desktop/AirFlash.Core/SettingsStore.cs#L40-L49) |
| Manual receivers are reference records in an observable collection | [Settings.cs:27–30,73](../../desktop/AirFlash.Core/Settings.cs#L27-L30) |
| Sample-rate validator exists; manual validation itself assumes non-null elements | [Settings.cs:96–109](../../desktop/AirFlash.Core/Settings.cs#L96-L109) |
| Constructor loads settings and immediately merges manual rows | [AppViewModel.cs:78–97](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L78-L97) |
| Merge dereferences every manual entry | [AppViewModel.cs:222–227](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L222-L227) |
| Playback parses unchecked saved sample-rate text | [SessionController.cs:303–310](../../desktop/AirFlash.Core/SessionController.cs#L303-L310) |
| Normal startup catches construction failure and follows startup failure/quit handling | [App.xaml.cs:48–75](../../desktop/AirFlash.App/App.xaml.cs#L48-L75) |

Microsoft documents that [System.Text.Json does not enforce collection-element nullability](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/nullable-annotations#limitations). Enabling nullable annotations alone would not fix the null-element case. Simply calling the current `Validate` method is also insufficient: its manual-entry predicate can dereference the same null.

## 3. Deterministic reproduction to implement

Write the following fixtures to separate temporary files and pass their paths to `SettingsStore`; never modify the real user configuration:

```json
{"schema_version":2,"manual_receivers":[null]}
```

```json
{"schema_version":2,"stream_sample_rate":"invalid"}
```

1. Load the null-element fixture, then construct `AppViewModel` using the existing mock discovery/audio/autostart/engine services. The source-derived current result is a null dereference during manual-row merge.
2. Load the invalid-rate fixture and inspect validation separately: loading returns it, while `Validate` reports an invalid rate.
3. Start a synthetic receiver through a fake engine factory and fake audio service with that loaded configuration. The source-derived current result is a failed command-construction attempt, rather than a valid `start` command. Force reconnect should be off for this fixture.
4. A fixed loader should detect these cases before construction/autoconnect and surface a precise recoverable configuration diagnostic, preserving the original file.

These steps describe future tests, not observed hardware or executed WPF behavior. [ISSUE-19](ISSUE-19-MEDIUM-settings-recovery-reread-escapes.md) separately addresses an I/O exception escaping the loader's recovery catch; this report concerns successful deserialization of invalid contents.

## 4. Proposed architectural fix

Add an explicit post-migration validation stage before returning settings to runtime callers. First check collection shape/null entries and required values, then apply semantic validation. Make the policy for invalid fields explicit: controlled rejection/recovery, or narrowly defined normalization with a warning. Do not blindly return an invalid object or silently overwrite the original with defaults.

Preserve schema 1 migration, schema 2, root/equalizer unknown fields, nullable receiver overrides, manual persistence, and future-schema rejection. Capture a readable original once for backup and coordinate recovery with ISSUE-19. If recovery blocks startup or requires user correction, present a useful diagnostic without creating a partially initialized playback session.

A selected unavailable adapter is not invalid merely because it is offline. Preserve that id and paused-discovery behavior; do not normalize it to all interfaces. Validate before auto-connect, engine spawn, local mute, or other playback side effects.

## 5. Acceptance criteria and coverage

- A null manual collection still uses its documented default; null elements and malformed manual entries are rejected or normalized through a documented safe policy, without a null dereference.
- Unsupported/null/invalid sample-rate values never reach `int.Parse`, native playback, or an automatic retry loop.
- Validation itself is total for deserialized shapes: it returns a diagnostic rather than throwing on null elements.
- Invalid readable configuration remains recoverable and is preserved before any replacement save; unreadable-file recovery follows ISSUE-19 rather than an unguarded reread.
- Valid existing schema 1/2 files, manual rows, extension fields, nullable overrides, and unavailable selected NICs retain their behavior.
- Mock constructor/playback tests verify that no engine command or local mute occurs for rejected input.

[SettingsTests.cs:135–138](../../desktop/AirFlash.Tests/SettingsTests.cs#L135-L138) covers valid manual-record round trips; [169–179](../../desktop/AirFlash.Tests/SettingsTests.cs#L169-L179) covers direct sample-rate validation and a missing-rate default. These do not test persisted invalid rates or null manual elements through the loader/constructor. The WPF [UiRegression harness](../../desktop/AirFlash.App/Verification/UiRegression.cs) provides mock services for integration assertions. No new tests, real config reads, endpoint changes, or receiver probes were performed for this report. Future verification should run the desktop unit suite and relevant mocked WPF harness; the preceding audit could not execute them without the SDK.
