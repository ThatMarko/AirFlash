# [HIGH] A stale discovery snapshot can stop an independently started session

- **Issue ID**: ISSUE-15
- **Severity**: HIGH — an unrelated discovery transaction can cancel a new user-selected pairing session or overwrite its receiver.
- **Kind**: Defect
- **Subsystem**: Desktop discovery/session concurrency
- **Status**: Open — source-verified race possibility; proposed change, not implemented
- **Runtime baseline**: upstream `41190e0`, retained by documentation commit `c077a05`
- **Evidence status**: The interleaving below follows existing asynchronous boundaries. A deterministic mock reproduction is specified but was not executed; no field incident is attributed to it.
- **Implementation status**: Proposal only; no runtime change applied
- **Target files**: [AppViewModel.cs](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L166), [SettingsWindow.xaml.cs](../../desktop/AirFlash.App/Ui/SettingsWindow.xaml.cs#L20), [SessionController.cs](../../desktop/AirFlash.Core/SessionController.cs#L212)
- **Analysis context**: [Playback coupling](../analysis/playback-coupling.md), [After discovery](../analysis/after-discovery.md), [Session](../analysis/session.md)

## 1. Current behavior and impact

Discovery reconciliation captures `Session.Snapshot`, then may await an identity save while holding `_settingsGate`. Settings Pair bypasses that settings gate and starts a new session through `Session.PairAsync`. After the save, reconciliation applies its stop/update decision using the old captured receiver, while `Session.StopAsync` and `UpdateReceiverAsync` operate on whichever session is current at that moment.

Consequently an absent old discovered receiver A can cause reconciliation to cancel a newly started manual pairing session B. The nominal `{ IsManual: false }` exemption tests captured A, not current B. This is independent of whether passive absence should stop A at all ([ISSUE-01](ISSUE-01-HIGH-discovery-disconnects-active-playback.md)). See [Playback coupling](../analysis/playback-coupling.md) and [After discovery](../analysis/after-discovery.md).

## 2. Root cause and exact interleaving

1. [AppViewModel.cs:168–174](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L168) captures A and awaits `SaveReceiverIdentityAsync` when reconciliation changes aliases/preferences.
2. [SettingsWindow.xaml.cs:20–24](../../desktop/AirFlash.App/Ui/SettingsWindow.xaml.cs#L20) calls `app.Session.PairAsync` directly, outside `_settingsGate`.
3. [SessionController.cs:183–210](../../desktop/AirFlash.Core/SessionController.cs#L183) serializes replacement, stops A, advances lifecycle generation, and publishes Pairing for B. It returns after scheduling the worker rather than holding `_serial` for the whole pairing workflow.
4. Once saving finishes, [AppViewModel.cs:211–218](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L211) uses captured A's id and active state. Its call to [StopAsync](../../desktop/AirFlash.Core/SessionController.cs#L212) has no expected-session argument, so it cancels B.

The present-receiver update branch has the same ownership gap: [UpdateReceiverAsync](../../desktop/AirFlash.Core/SessionController.cs#L246) checks no captured generation/id. While Pairing it avoids a signature restart but can overwrite `_receiver` and `_settings` with the old receiver's reconciled values. That is the same race, not a separate issue.

## 3. Minimal mock reproduction — planned, not run

Use the WPF dispatcher and the existing [MemoryStore save gates](../../desktop/AirFlash.App/Verification/UiSmoke.cs#L276). Extend a scripted fake engine connection to emit `pin_required` for `pair`; the existing UI-smoke mock connection does not implement pairing events. Use only fake audio/discovery/engine services and documentation-only addresses.

1. Persist manual B, disable automatic starts/retries, publish discovered A, and reach fake Streaming on A.
2. Set `SaveGate`/`SaveEntered`. Publish a snapshot containing new discovered C but no A. C's new broadcast self-alias forces an identity save. Await `SaveEntered` so captured A is held before reconciliation's session decision.
3. Invoke the same direct `Session.PairAsync(B, liveSettings.Clone(), requestPin)` path used by Settings. The fake connection emits `pin_required`; make `requestPin` wait on a controllable completion bound to its cancellation token. Wait for B's Pairing snapshot.
4. Release the save gate. Source-predicted result: reconciliation of A calls `StopAsync`, cancels B's PIN wait/worker, and clears its receiver to Idle.
5. Variant: include changed A in the delayed snapshot, then start B. Check whether the update branch replaces B's receiver metadata with A while B's worker is still pairing.

Assertions must record both the lifecycle ownership change and subsequent command/cancellation; do not infer the race from log ordering or snapshot names alone. No PIN entry or receiver connection is needed.

## 4. Proposed architecture — not applied

Give discovery updates an expected lifecycle token and make ownership validation atomic with stop/update under the controller's `_serial` semaphore. A conditional operation must ignore a request whose captured generation no longer owns the current session. The token should identify start/stop/replacement generations, not a snapshot object's reference: ordinary metrics also replace snapshots and should not invalidate a legitimate same-session identity update.

Do not rely on rereading `Session.Snapshot` in the view model alone; a replacement can still occur between that read and the controller operation. Apply the guard to both missing-receiver stop and receiver/settings retargeting. Bringing Pair into a shared app lifecycle coordinator is an alternative, but it must avoid holding the settings gate across a PIN prompt or introducing a lock-order deadlock. Keep normal identity/preference persistence and migration behavior intact. No config-schema or JSONL change is necessary.

## 5. Acceptance criteria and verification

- Releasing a delayed reconciliation captured for A cannot stop, retarget, or alter settings/diagnostics for a newer B generation.
- A new manual pairing session B survives that interleaving; its prompt/worker remains owned by B.
- Both absent-A and changed-A branches are covered, including same receiver id restarted with a newer generation.
- Metrics updates within the same generation do not suppress legitimate identity updates; changed effective latency can still trigger its intended same-owner restart.
- User Stop cancels the actual current session promptly through asynchronous cleanup, with no deadlocks among settings/lifecycle/writer semaphores.
- A failed identity save still preserves the catalog/session as before, and ordinary nonconcurrent discovery behavior remains explicit.

Existing [identity-save harness coverage](../../desktop/AirFlash.App/Verification/UiRegression.cs#L163) exercises failure preservation and open drafts, not replacement during a delayed save. [SessionTests](../../desktop/AirFlash.Tests/SessionTests.cs#L153) covers stale worker events across switching, not a stale caller invoking an unconditional stop/update. A delayed-save/direct-Pair ownership regression is missing.

Safety: mocks only; no credential files, PINs, device pairing, or audio probes. Any later authorized audio probe is limited to gain ≤0.1 and ≤5 seconds. No correspondence to upstream issues #6/#7 or dev is claimed.
