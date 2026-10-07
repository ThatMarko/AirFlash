# Playback coupling

A live HomePod session stays up only while the desktop still sees that receiver in the latest discovery snapshot. The engine's TCP and UDP sockets are not consulted for this decision. This page is the behavior of the current tree. [Discovery](discovery.md) describes how a snapshot becomes empty. [Session](session.md) describes the stop and the new process that may follow.

## The check

[`ReconcileDiscoveryAsync`](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs) runs on every discovery result, including the empty list `Restart` publishes before the new browse has resolved anything. For the receiver in the current snapshot:

```csharp
if (currentSnapshot.Receiver is { IsManual: false } current)
{
    var id = ReceiverMigrations.GetValueOrDefault(current.Id, current.Id);
    if (!discovered.TryGetValue(id, out var next) || !next.Complete)
    {
        if (currentSnapshot.IsActive) await Session.StopAsync();
    }
    else if (!ReceiverEqual(current, next) || !SettingsMerge.Equal(originalSettings, _settings))
        await Session.UpdateReceiverAsync(next, _settings.Clone());
}
```

`IsActive` includes Connecting, Streaming, Standby, and Pairing. A discovered receiver that is missing, or a stereo row that is no longer 2/2, stops the session. A manual receiver skips this block. Its stream continues across an empty browse. That split is the `{ IsManual: false }` pattern: manual rows are not members of the mDNS map, so the check does not apply to them.

An address, port, or leader change still reaches `UpdateReceiverAsync`. That call opens a new engine process when the transport signature changes. An identity promotion with the same address does not. See [After discovery](after-discovery.md).

## What produces the empty snapshot

[`WindowsDiscovery.Restart`](../../desktop/AirFlash.App/Services/WindowsDiscovery.cs) clears `_records`, `_instances`, `_browsers`, and `_pending`, then calls `Publish` while the cache is empty. These calls reach `Restart`:

- `Start`
- `SetInterface` when the saved adapter id changes
- `NetworkAddressChanged`
- `NetworkAvailabilityChanged`
- The 20-second timer, when a selected adapter's IPv4 index no longer matches the browse

The network handlers do not receive the adapter that changed. Any address change on the machine, including a virtual adapter, takes the same path as a change on the NIC that reaches the HomePod.

A second path drops one name without clearing the others. `Refresh` deletes a record whose 90-second expiry has passed, and a hung resolve can leave `_pending` set so that name is not queried again. The details are in [Discovery](discovery.md).

`Discovery resolved:` is logged when the name is missing from `_records` or when its description changed. Identical lines logged again are cache misses. In `wpf-2026-10-06.log`, all 8 distinct resolve descriptions are repeated (88 lines, one description 12 times), and 10 separate 8-second windows each contain at least 4 distinct resolves. Several speakers are logged together. That matches a cache that was empty for all of them at once. The log does not name the event that cleared it.

## The row leaves the panel

`RefreshReceivers` keeps a row only when it is online and not hidden. Marking a device `Online = false` removes it from the control panel. Removing it from `_known` does the same. With all interfaces selected, a missing device stays in the map as offline and can still appear as history in Settings. With a NIC selected, the map entry is deleted. Saved options in `config.json` remain either way. The user cannot press play on a row that the panel has removed.

## Stop does not increment reconnects

`StopAsync` cancels the session lifetime and waits for the worker. `RunStreamAsync` returns on that cancellation before it records a fault or adds to `ReconnectCount`. `StartLockedAsync` on the next attempt replaces diagnostics with an empty value. A monitor opened on the playback that auto-connect starts afterward shows reconnects at 0, sender recoveries at 0, and empty live counters. Those zeros mean the previous process was stopped and a new process reached `streaming`. They do not mean the engine retried inside one process.

Auto-connect starts that new process only when it is enabled, the user has not suppressed it with stop, and the receiver is online, complete, and not hidden. The timer is 900 ms after the last discovery result. With auto-connect off, the stop is the end of playback until the user presses play. The setting defaults off. See [After discovery](after-discovery.md).

The suppress flag is set by `AppViewModel.StopAsync` when the stop is user-initiated, which is the panel and the tray. The discovery path calls `Session.StopAsync` and leaves the flag clear, so a receiver that vanishes and returns can auto-connect again.

## Local mute follows the stop

Mute-while-streaming defaults on. `streaming` mutes the capture endpoint. The worker's `finally`, and `StopLockedAsync`, both call `RestoreAudioAsync`. Restore writes back the mute bit captured before the stream. If that bit was unmuted, PC speakers play again as soon as the session task ends, while system audio is still running. A later auto-connect mutes them again when the new process emits `streaming`. The same restore runs for a user stop, an engine error, and a discovery stop. [Control and mute](control-and-mute.md) has the endpoint calls.

## What this check does not see

The engine may still be inside its feedback, event, and media watches when the desktop calls `stop`. Those watches are in [Engine](engine.md). A `feedback_timeout` or `media_send_timeout` ends the process from inside and, when force reconnect is on, increments `ReconnectCount`. A discovery stop never enters that loop.

Force reconnect defaults off. It does not apply to a discovery stop anyway, because that path cancels rather than returning a retryable `error`.
