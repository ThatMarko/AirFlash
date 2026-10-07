# Playback coupling

When the desktop applies a discovery snapshot that lacks an active non-manual receiver, or contains its incomplete stereo group, it calls `Session.StopAsync`. The engine's TCP and UDP health is not consulted for this decision. Reconciliation performs any required identity save before reaching this check; a save failure exits that transaction and leaves the catalog/session untouched by that snapshot. This page describes the current tree's source behavior. [Discovery](discovery.md) describes cache publication. [Session](session.md) describes stop and the new process that may follow.

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

`currentSnapshot` is captured before any awaited identity save. The settings gate serializes discovery and Settings Apply, but Pair invokes the session directly and the worker can change state independently. The later stop/update decision is therefore based on the captured snapshot, not an atomic check of the receiver still active after the save. The nominal manual-receiver exemption above is a branch property; this audit did not exercise a concurrent pairing/session switch during a delayed identity save.

An address, port, or stereo leader change still reaches `UpdateReceiverAsync`. An active non-pairing session is restarted when its signature changes: peer addresses/ports/stereo-leader flags, effective capture endpoint, latency, or sample rate. An identity promotion with unchanged transport and effective settings does not restart; migrated latency preferences can make an otherwise identical address restart. Pairing is excluded from signature-triggered restarts, but can still be stopped by the missing/incomplete-receiver check. See [After discovery](after-discovery.md).

## What produces the empty snapshot

[`WindowsDiscovery.Restart`](../../desktop/AirFlash.App/Services/WindowsDiscovery.cs) clears `_records`, `_instances`, `_browsers`, and `_pending`, then calls `Publish` while the cache is empty. These calls reach `Restart`:

- `Start`
- `SetInterface` when the saved adapter id changes
- `NetworkAddressChanged`
- `NetworkAvailabilityChanged`
- The 20-second timer, when a selected adapter's IPv4 index no longer matches the browse

The network handlers do not receive the adapter that changed. Any address change on the machine, including a virtual adapter, takes the same path as a change on the NIC that reaches the HomePod.

A second path drops one name without clearing the others. A flags-zero PTR removes the record and pending marker. A refresh tick deletes a record whose 90-second expiry has passed, including its instance name, and a hung resolve can leave `_pending` set so that name is not queried again. The expiry is processed by the 20-second timer rather than at a precise 90-second alarm. The details are in [Discovery](discovery.md).

`Discovery resolved:` is logged when the name is missing from `_records` or when its description changed. An identical description logged again can follow a cache miss, or an intervening description change that later reverted. The log does not label restart, expiry, or PTR-removal causes. Several resolve lines near one another are insufficient to identify a network event, a receiver reboot, or the state of the engine watches; this audit does not use local log counts as repository ground truth.

## The row leaves the panel

`RefreshReceivers` keeps a row only when it is online and not hidden. Marking a device `Online = false` removes it from the control panel. Removing it from `_known` does the same. With all interfaces selected, a missing device stays in the map as offline and can still appear as history in Settings. With a NIC selected, the map entry is deleted. Saved options in `config.json` remain either way. The user cannot press play on a row that the panel has removed.

## Stop does not increment reconnects

`StopAsync` cancels the session lifetime and waits for the worker. `RunStreamAsync` returns on lifetime cancellation before recording another fault or entering its next retry; cancellation during retry delay prevents that retry's `ReconnectCount` increment as well. Stop does not reset or prove the absence of an earlier fault. `StartLockedAsync` on the next explicit/automatic start replaces diagnostics with an empty value, so reconnect count starts at 0 and live metrics begin unavailable. Sender recovery values arrive in later engine metrics and may already be nonzero. Zero counters alone do not prove discovery caused a stop, that streaming was reached, or that engine watches remained healthy.

At timer entry, auto-connect checks that it is enabled, the user has not suppressed it with stop, no session is active, and the receiver is online, complete, and not hidden. Those checks precede awaited settings work and are not an atomic admission guard against a later Pair or user Stop. The timer is 900 ms after the last discovery result. With auto-connect off, the stop is the end of playback until the user presses play. The setting defaults off. See [After discovery](after-discovery.md) for the selection and concurrency limits.

The suppress flag is set by `AppViewModel.StopAsync` when the stop is user-initiated, which is the panel and the tray. The discovery path calls `Session.StopAsync` without changing the flag. It does not clear a flag already set by a previous user stop. A receiver that vanishes and returns can auto-connect again when the flag and attempt-selection rules permit it; an already-attempted preferred receiver can block fallback to a different receiver. See [After discovery](after-discovery.md).

## Local mute follows the stop

Mute-while-streaming defaults on. On `streaming`, the audio service captures the endpoint's mute bit immediately before first setting it to muted, then the desktop publishes Streaming. The worker's `finally`, and `StopLockedAsync` after cancellation/worker cleanup, call `RestoreAudioAsync`. Restore attempts to write that exact bit back. If it was unmuted and restore succeeds, local audio can become audible before a later auto-connect reaches `streaming` and mutes again. This is not a promise of immediate physical playback: engine cleanup precedes these stop-path restore calls, and an unavailable endpoint's COM failure retains its original bit for a later retry. Session-level restore errors are logged. The same restore paths apply to user stop, engine failure, and discovery stop. [Control and mute](control-and-mute.md) has the endpoint calls.

## What this check does not see

The engine may still be running its feedback, event, and media watches when the desktop calls `stop`. Those watches are in [Engine](engine.md). An engine fault such as `feedback_timeout` or `media_send_timeout` can cause the desktop to enter its retry loop. The reconnect count increments only after a retry is permitted and its delay completes, before the next process attempt. An error by itself does not increment it. A discovery-driven lifetime cancellation does not initiate that retry path, though an engine fault could already have occurred independently before cancellation.

Force reconnect defaults off. It does not apply to a discovery stop anyway, because that path cancels rather than returning a retryable `error`.

## Source and verification scope

| Claim group | Source anchors |
| --- | --- |
| Save-before-check transaction and manual/incomplete split | [`AppViewModel.cs:166–220`](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L166) |
| Empty restart, per-name removal/expiry, log predicate | [`WindowsDiscovery.cs:47–161`](../../desktop/AirFlash.App/Services/WindowsDiscovery.cs#L47) |
| Panel filtering and user suppress flag | [`AppViewModel.cs:234–249, 312–324`](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L234) |
| Active states, fresh diagnostics, stop, signature update | [`SessionController.cs:9–11, 175–260`](../../desktop/AirFlash.Core/SessionController.cs#L190) |
| Cancellation, fault/retry ordering, count increment | [`SessionController.cs:370–394`](../../desktop/AirFlash.Core/SessionController.cs#L370) |
| First mute and restore handling | [`SessionController.cs:325–329, 461–464`](../../desktop/AirFlash.Core/SessionController.cs#L325), [`AudioService.cs:63–82`](../../desktop/AirFlash.App/Services/AudioService.cs#L63) |

[`SessionTests`](../../desktop/AirFlash.Tests/SessionTests.cs#L126) covers pending-start cancellation, fresh-process retries (line 169), mute restore (lines 199–208), incomplete-group rejection (line 220), and preserving diagnostics until a fresh playback (line 260). The WPF [`UiRegression` harness](../../desktop/AirFlash.App/Verification/UiRegression.cs#L163) checks failed identity-save preservation, offline-to-online auto-attempt recovery (line 202), and stereo-member disappearance stopping playback (line 223), using mock services. Neither those tests nor this source audit establishes the cause of any particular real-world dropout or guarantees mute restoration on unavailable hardware.


## Locally tested Group B behavior

The preceding audit remains the description of stable `41190e0`. This addendum describes local branch `codex/fix-session-lifecycle`, tested head `342aeb77cb85ff6f3f7f850161b3db79d8637945`, based directly on that stable commit. Group B has not been pushed or integrated. See the [acceptance checklist](../issues/GROUP-B-ACCEPTANCE.md).

Empty, partial, overfull or repeated discovery results do not cancel an owned handshake/stream or reopen its engine. Catalog availability remains truthful while the full active transport stays in the session. An active card remains visible, including when hidden or removed from the selected-adapter catalog, with an availability label and a bound Stop button. After stop or terminal completion, a card retained solely by ownership disappears.

A complete online reappearance can still trigger a guarded receiver update, including a restart when its effective signature changes; stale saved discovery work cannot retarget a newer owner. Adapter browse restart retains playback without making the selected discovery interface an egress binding. Existing worker failures, explicit stop, PIN cancellation and retry/mute cleanup remain effective. The mock evidence establishes these desktop contracts, not a cause for a particular hardware dropout.
