# [HIGH] A delayed automatic start replaces a newer session and can undo user Stop

- **Issue ID**: ISSUE-02
- **Implementation plan**: [Work package ISSUE-02](IMPLEMENTATION_PLAN.md#issue-02)
- **PR group**: B — Session lifecycle
- **Severity**: HIGH — a stale automatic action can cancel a newly selected manual pairing session and start another receiver after an explicit Stop.
- **Kind**: Defect
- **Subsystem**: Desktop automatic-start admission and session ownership
- **Status**: Implemented and verified locally on `codex/fix-session-lifecycle`; not integrated into original stable main.
- **Evidence status**: The original audit inspected relevant view-model, Settings Pair, controller and fake-harness/test sources on 2026-10-07 without executing the proposed reproductions. Subsequent isolated baseline observation reproduced stale automatic replacement; the local implementation passes the [Group B checks](GROUP-B-ACCEPTANCE.md). No receiver incident, audio output or frequency was measured.
- **Runtime baseline**: upstream `41190e0`, retained by documentation baseline `c077a05` and the audited [analysis index](../analysis/README.md)
- **Implementation status**: Local head `342aeb7`; see [Group B acceptance and actual verification](GROUP-B-ACCEPTANCE.md).
- **Analysis context**: [After discovery](../analysis/after-discovery.md), [Session](../analysis/session.md), [Settings](../analysis/settings.md), [Control and mute](../analysis/control-and-mute.md)
- **Target files**: [AppViewModel.cs:292–355](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L292-L355), [SettingsWindow.xaml.cs:20–24](../../desktop/AirFlash.App/Ui/SettingsWindow.xaml.cs#L20-L24), [SessionController.cs:176–224](../../desktop/AirFlash.Core/SessionController.cs#L176-L224)
- **Related items**: [ISSUE-01](ISSUE-01-HIGH-stale-discovery-snapshot-controls-new-session.md) covers stale discovery stop/update ownership; [ISSUE-10](ISSUE-10-MEDIUM-attempted-preferred-receiver-blocks-autoconnect.md) covers candidate filtering, a separate defect.

## 1. Current behavior and impact

`TryAutoConnectAsync` checks that no session is active and that user Stop has not suppressed automatic starts only when it enters. It chooses a receiver and calls the same `ToggleAsync` path as an explicit click. That path may await a dirty master-volume/settings save and the settings gate before it calls `Session.StartAsync`. It does not repeat the automatic admission checks after those waits. ([AppViewModel.cs:292–323](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L292-L323), [339–355](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L339-L355).)

Settings Pair calls the controller directly without acquiring that settings gate. It can therefore start manual receiver B while a previously selected automatic start for A remains delayed. When A's save/gate wait finishes, `StartAsync` unconditionally replaces the current session, cancelling B's pairing/PIN wait and opening playback for A. This is a stale automatic request acting on a newer owner; it is not a violation by the native engine of its one-process/session policy. The controller correctly performs the replacement that its caller requests. ([SettingsWindow.xaml.cs:20–24](../../desktop/AirFlash.App/Ui/SettingsWindow.xaml.cs#L20-L24), [SessionController.cs:176–210](../../desktop/AirFlash.Core/SessionController.cs#L176-L210).)

The same pending request can defeat a later user Stop. After B becomes active but before its PIN dialog opens, Stop sets `_autoSuppressed = true` and cancels B. The already-running automatic `ToggleAsync` later resets that flag to false and starts A without another suppression check. This claim concerns Stop on active B without a modal prompt; it does not assume the user can press the disabled Stop command while Idle or reach an ordinary Stop control during the modal `ShowDialog` call. ([AppViewModel.cs:94](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L94), [303–315](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L303-L315), [PinDialog.cs:34](../../desktop/AirFlash.App/Ui/PinDialog.cs#L34).)

HIGH follows the backlog's concrete session-ownership disruption criterion: automatic work can interrupt an explicit manual pairing operation and override an explicit user Stop. The save/wait and interleaving prerequisites limit occurrence; no data loss, credential corruption, OS mute failure, or observed hardware incident is established. Automatic connection must be enabled for A. No discovery-loss snapshot, failed authentication, or engine retry is needed for this case.

## 2. Source trace and bounded root cause

| Boundary | Exact source and consequence |
| --- | --- |
| One automatic admission check | [AppViewModel.cs:318–323](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L318-L323) checks `_closing`, `_autoSuppressed`, and `Session.Snapshot.IsActive`, then awaits `ToggleAsync`; it retains no lifecycle/automatic-intent token. Active includes Connecting, Streaming, Standby, and Pairing ([SessionController.cs:7–11](../../desktop/AirFlash.Core/SessionController.cs#L7-L11)). |
| Save/gate yields before start | [AppViewModel.cs:298–305](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L298-L305) awaits the flush and settings gate. A dirty flush takes that gate, awaits `SaveAsync`, then updates controller settings ([339–355](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L339-L355)). The dispatcher remains available during the await. |
| Pair can become the newer owner | [SettingsWindow.xaml.cs:20–24](../../desktop/AirFlash.App/Ui/SettingsWindow.xaml.cs#L20-L24) calls `Session.PairAsync` directly. It acquires the controller semaphore, replaces the previous lifecycle, publishes Pairing, and returns after scheduling its worker ([SessionController.cs:183–210](../../desktop/AirFlash.Core/SessionController.cs#L183-L210)). |
| Resumption does not validate automatic intent | [AppViewModel.cs:302–307](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L302-L307) rechecks closing only, clears suppression, marks A attempted, starts A, and saves A as last-used. The initial toggle/stop check at line 295 is also before the waits. |
| Serialization permits the erroneous replacement | [SessionController.cs:176–198](../../desktop/AirFlash.Core/SessionController.cs#L176-L198) serializes Start but imposes no expected-owner or inactive-state condition. `StartLockedAsync` calls `StopLockedAsync` before assigning A. Stop advances generation, cancels the lifetime, awaits the worker, and restores audio ([218–224](../../desktop/AirFlash.Core/SessionController.cs#L218-L224)). |
| A pending PIN waits on B's lifetime | [SessionController.cs:412–419](../../desktop/AirFlash.Core/SessionController.cs#L412-L419) links PIN cancellation to the session lifetime. Pair cleanup sends Stop and disposes its connection ([396–429](../../desktop/AirFlash.Core/SessionController.cs#L396-L429)). |
| New Stop does not invalidate the old automatic caller | [AppViewModel.cs:312–323](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L312-L323) sets suppression before controller Stop, but a caller already past line 320 reaches line 303 without another check. Controller generation advances even for a Stop with no current worker ([SessionController.cs:212–224](../../desktop/AirFlash.Core/SessionController.cs#L212-L224)). |

An ordinary settings flush during Pairing need not replace B: `UpdateSettingsAsync` excludes Pairing from its signature-restart branch. The later unconditional automatic `StartAsync` is the replacement in this reproduction. ([SessionController.cs:226–244](../../desktop/AirFlash.Core/SessionController.cs#L226-L244).)

Filtering `_autoAttempted` before choosing A, as proposed in ISSUE-10, cannot repair this ownership gap. ISSUE-01's guarded discovery stop/update also does not automatically guard this start caller. The three fixes must share lifecycle ownership semantics without conflating their triggers. The existing `_closing` recheck handles shutdown admission at this point; this report does not claim a delayed start bypasses it.

## 3. Deterministic mock reproductions — planned, not run

Use the WPF dispatcher with `MemoryStore`, fake discovery/audio, and scripted engine connections that open no sockets. The existing [save gates](../../desktop/AirFlash.App/Verification/UiSmoke.cs#L276-L292) can hold a dirty flush. The existing [UI-smoke engine](../../desktop/AirFlash.App/Verification/UiSmoke.cs#L344-L395) captures commands but does not emit pairing events; extend a scripted fake to record each connection/session, emit `pin_required` for B, and emit `streaming` for A.

Production timers are private and run at 200 ms/900 ms ([AppViewModel.cs:90–93](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L90-L93)). Add a test-only scheduler/entry seam, or a harness-only helper, that stops both timers and invokes one `TryAutoConnectAsync` task on the dispatcher. This seam does not exist today. The fixture must control task completion rather than rely on sleeps winning a timer race.

### A. A delayed automatic start cancels manual pairing B

1. Persist complete manual B at `192.0.2.20:7000`, with its own manual id and per-receiver auto-connect explicitly false. Enable automatic connection for discovered A at `192.0.2.10:7000`; disable force reconnect and local mute in the fake fixture. Apply initial discovery/startup fully with timers controlled, and confirm Idle with no engine opened.
2. Set `SaveGate` and `SaveEntered` using asynchronous continuations. Change `app.MasterVolume` to a different valid value so `_volumeDirty` is true. Invoke exactly one automatic entry task, selecting A. Await `SaveEntered`; assert that task remains incomplete and no engine connection has opened. Do not await its completion yet.
3. Invoke the direct `Session.PairAsync(B, app.Settings.Clone(), requestPin)` path used by Settings. Have the fake emit `pin_required`. Make `requestPin` signal its entry and await a controllable task using the supplied cancellation token; return no PIN. Await that signal and B's Pairing snapshot, including the dispatched app snapshot.
4. Release the save gate and await the automatic task. **Source-predicted current result:** B's PIN token is cancelled, B's connection receives Stop/is disposed, and a new connection receives `start` for A. The active/last-used receiver becomes A. No `paired` event or `pair_pin` is needed to reach this result.
5. **Expected fixed result:** the stale automatic admission is rejected; no A connection/`start` is created, B's PIN wait and ownership remain intact, and rejected A does not become last-used/attempted. Legitimate master-volume persistence is still allowed to finish. Cancel B through the fixture's ordinary stop/cleanup path afterward.

Record connection/session identities, PIN cancellation, disposal, command peers, and receiver ownership. Snapshot text alone is insufficient. Always release gates in `finally` so fixture disposal cannot hang.

### B. User Stop after B starts is undone by the old automatic request

Repeat steps 1–2, keeping the automatic save held. Start B through the same direct Pair path, but hold its fake pairing response **before** emitting `pin_required`; do not open or simulate an outstanding modal PIN dialog in this variant. Await B's dispatched Pairing snapshot and the fake's recorded `pair` command. Confirm `StopCommand.CanExecute` is true, invoke `app.StopAsync(userInitiated: true)`, and await completion: B is cancelled/disposed and Session becomes Idle. Then release the automatic save gate. A second variant may complete pairing and close the prompt before stopping B in Streaming.

**Source-predicted current result:** the old `ToggleAsync` clears the newly set suppression flag at line 303 and sends A's `start`. **Expected fixed result:** A never starts; a subsequent automatic tick while still Idle remains suppressed, until an action that is intended to re-enable automatic connection occurs. This isolates the Stop/suppression path with an active receiver and no modal prompt; it is a planned mock test, not an executed physical click or an artificial Idle Stop button.

Neither fixture has been executed in this documentation pass. No .NET/Rust build, real config/credential access, DNS/RTSP request, audio capture, or hardware probe was performed for this issue.

## 4. Proposed fix and compatibility — unimplemented

Give an automatic start a lifecycle generation and an automatic-intent invalidation token captured before its first await. Add a controller admission operation that, under the same `_serial` ownership used by start/stop, checks the expected generation and requires that no session is active before calling the replacement/start routine. Preserve the currently allowed inactive states, including Error; requiring only the literal Idle enum would unintentionally block ISSUE-10's failure-to-next-candidate case. Use a lifecycle token, not snapshot-reference equality, because ordinary events also replace snapshots.

After awaited persistence, revalidate the current canonical receiver, online/completeness/visibility and automatic options under the app settings gate, coordinated with controller admission. A catalog/Apply change can invalidate eligibility without changing lifecycle generation or explicit intent; keep the selected data stable through admission.

Separate automatic admission from the explicit-click toggle semantics. Automatic work must not clear a newer user-stop suppress flag. Invalidate pending automatic intent synchronously when a newer explicit Play/Pair/Stop or shutdown takes ownership, before awaiting its controller work. Coordinate that invalidation with serialized admission so Stop cannot be missed while waiting for `_serial`; a view-model recheck followed by unconditional `StartAsync` still has a check/use race. A cancellation/invalidation token must be checked at admission, not merely before waiting for a semaphore. Route the direct Settings Pair entry through the shared intent coordination when needed, without holding the settings gate or lifecycle semaphore across a PIN prompt.

Return an explicit admitted/rejected result. Update automatic attempt marks and last-used id only for an admitted attempt; an admitted connection failure still counts as an attempt under existing policy. Rejecting stale admission must not cancel the newer owner, clear its diagnostics, or cause any engine Start/Stop command. An already valid settings save remains valid; this fix should not discard unrelated persisted master-volume edits.

Coordinate the generation/token API with ISSUE-01 rather than introduce competing ownership mechanisms. Keep explicit user receiver switching functional and preserve candidate ranking/reset rules from ISSUE-10. Maintain one active engine process, existing cleanup and exact saved mute-bit restoration, persistent manual identity, selected-offline-adapter no-fallback, and complete-group validation. No blocking DNS/RTSP/capture work belongs on the dispatcher.

This is a managed desktop ownership change. `config.json` schema 2 and JSONL v1 remain unchanged; no native command or RTP resume is needed. Pair-verify/transient exclusivity and current-user DPAPI credentials keyed by accessory `deviceID` remain unchanged. A manual selection keeps its explicit endpoint/preferences without introducing a manual credential namespace.

## 5. Acceptance criteria and verification

- Releasing a held automatic flush after B enters Pairing does not cancel B, retarget its receiver, clear its diagnostic history, or open/send Start for A.
- A user Stop on active B while the old automatic request is held cancels B normally; releasing that request and running another automatic tick does not restart playback or clear suppression.
- The same ownership check holds if B finishes pairing and reaches Streaming before admission, or if the receiver id is reused with a newer lifecycle generation.
- Automatic admission checks generation, active state, and newer intent atomically with starting; replacements arriving between a view-model check and controller entry cannot bypass it. Multiple queued automatic tasks admit at most one current request.
- A queued discovery/Apply transaction that removes, makes incomplete, hides or disables the selected candidate during the save prevents stale admission even when the owner token is unchanged.
- A legitimate automatic start from Idle or Error still works; successful explicit Play/switching and the existing permitted attempt resets still work. An admitted connection failure counts as attempted; a rejected stale request does not overwrite attempts/last-used.
- Valid settings persistence can finish while a stale start is rejected. Save failure still prevents that caller from starting, and shutdown retains its closing guard and cleanup.
- No settings/lifecycle/writer lock is held across user PIN entry. Both initially muted and initially unmuted audio states retain their exact-bit cleanup contract through fake-audio assertions; no physical OS mute outcome is inferred.
- Schema 2, JSONL v1, manual endpoint identity, stereo completeness, selected-NIC no-fallback, authentication exclusivity, and accessory-keyed DPAPI remain compatible.

Existing [automatic-attempt identity harness](../../desktop/AirFlash.App/Verification/UiRegression.cs#L202-L220) covers failed attempts, alias promotion, and offline recovery; it does not overlap an automatic admission with Pair or Stop. The [gated-save harness](../../desktop/AirFlash.App/Verification/UiRegression.cs#L35-L59) contains responsive-save assertions, not this ownership interleaving. [Controller tests](../../desktop/AirFlash.Tests/SessionTests.cs#L126-L174) cover nonblocking start, cleanup serialization, and stale engine-event rejection; [pairing tests](../../desktop/AirFlash.Tests/SessionTests.cs#L226-L238) cover cancellation and pair-then-stream. None asserts rejection of a stale automatic caller. These sources were inspected, not executed for this report.

Add both dispatcher/save-gate cases and focused controller admission tests before verifying an implementation. Later verification should run the relevant core xUnit tests and the separate WPF `--ui-smoke` harness with these fake assertions; Rust/hardware tests are unnecessary for this desktop-only ownership repair unless implementation scope changes. Any separately authorized finite audio probe must remain at gain at most 0.1 and duration at most five seconds; no probe or soak is needed to reproduce this race. No upstream issue #6/#7 or dev-branch relationship is claimed.

## Executed local implementation

The source and proposed-fixture discussion above describes the original 41190e0 audit. This report's acceptance criteria are now covered by the combined locally tested Group B package. [Group B local acceptance](GROUP-B-ACCEPTANCE.md) records the separate checklist, original-base failure observations, branch commits, actual Core/WPF checks, independent review and limitations. Original stable main remains unchanged; no hardware incident is attributed to this defect.
