# [HIGH] A transient discovery snapshot stops active discovered playback

- **Issue ID**: ISSUE-01
- **Severity**: HIGH — a passive discovery refresh can interrupt an otherwise running stream; no data loss or security failure is established.
- **Kind**: Defect
- **Subsystem**: Desktop discovery/catalog/session orchestration
- **Status**: Open — source-verified behavior; proposed change, not implemented
- **Runtime baseline**: upstream `41190e0`, retained by documentation commit `c077a05`
- **Evidence status**: Source and existing mock-harness assertions inspected. The reproduction below was not executed; no real-device dropout or network trigger was reproduced.
- **Implementation status**: Proposal only; no runtime change applied
- **Target files**: [AppViewModel.cs](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L166), [WindowsDiscovery.cs](../../desktop/AirFlash.App/Services/WindowsDiscovery.cs#L47)
- **Analysis context**: [Playback coupling](../analysis/playback-coupling.md), [Discovery](../analysis/discovery.md), [After discovery](../analysis/after-discovery.md)

## 1. Current behavior and impact

The desktop stops an active non-manual session when an applied discovery snapshot lacks its receiver or contains a stereo row whose member count is not exactly two. This decision does not inspect the active engine's transport health. A normal discovery `Restart` clears its cache and publishes an empty list before starting the new browse, so that intermediate empty result can stop playback even when the current connection remains usable.

With all interfaces selected, missing discovered receivers remain in `_known` as offline; with a selected NIC, they are removed. The panel filters both out. Discovery does not set the user's auto-connect suppress flag, but playback resumes only through a permitted automatic attempt or an explicit user action. A new playback begins with fresh diagnostics; zero reconnects alone do not diagnose its cause. See [Playback coupling](../analysis/playback-coupling.md), [Discovery](../analysis/discovery.md), and [After discovery](../analysis/after-discovery.md).

The old report's CRITICAL rating, hypothetical 10–30-second absence, HomePod chime, author's intent, nonexistent `_receivers` removal snippet, periodic full-cache flush every 20 seconds, and RTP/RTCP acknowledgement claims are removed. The ordinary 20-second timer prunes expired records and refreshes known names; it restarts only on selected-interface index changes or its adapter-query error path. Feedback is TCP RTSP, not a universal acknowledgement of UDP audio delivery.

## 2. Bounded root cause

| Step | Exact evidence |
| --- | --- |
| Restart clears the cache and publishes before opening the replacement browse | [WindowsDiscovery.cs:47–80](../../desktop/AirFlash.App/Services/WindowsDiscovery.cs#L47) |
| Network-address and availability handlers invoke Restart without identifying the changed NIC | [WindowsDiscovery.cs:45–46](../../desktop/AirFlash.App/Services/WindowsDiscovery.cs#L45) |
| Missing/incomplete non-manual snapshot calls `Session.StopAsync` | [AppViewModel.cs:211–218](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L211) |
| Known-map policy and panel filtering remove usable controls from the panel | [AppViewModel.cs:196–210](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L196), [234–245](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L234) |
| Cancellation stops the worker; a new Start resets diagnostics | [SessionController.cs:190–224](../../desktop/AirFlash.Core/SessionController.cs#L190), [372–393](../../desktop/AirFlash.Core/SessionController.cs#L372) |

The discovery/session decision follows any required identity save. If that save fails, this transaction exits before applying the stop. The captured session can also become stale during an asynchronous save; that separate race is [ISSUE-15](ISSUE-15-HIGH-stale-discovery-snapshot-controls-new-session.md).

## 3. Minimal mock reproduction — planned, not run

Use the WPF dispatcher harness with `MockDiscovery`, in-memory settings, fake audio, and a fake engine that reports Streaming and keeps its event stream alive. All addresses can be documentation-only literals, such as `192.0.2.10`; the fake engine must never open a socket.

1. Set auto-connect and force reconnect off. Publish one complete discovered receiver A and start it through `ToggleAsync`; wait for the simulated Streaming snapshot.
2. Keep that fake engine healthy and publish an empty discovery list.
3. Pump the dispatcher until reconciliation finishes. Source-predicted result: A becomes offline or is removed according to interface mode; its panel row disappears; Session reaches Idle and the fake connection is disposed.
4. Publish A again. Source-predicted result: its row returns, but playback remains stopped with auto-connect disabled.
5. Repeat with a persisted manual receiver as the captured active receiver. It should skip the discovery-driven stop branch. Repeat separately with a complete stereo group becoming `1/2` to cover the current intentional coupling.

This tests the desktop policy directly. It does not require packet loss, Wi-Fi roaming, a network reset, or a HomePod, and does not establish that Windows will deliver any particular browse-removal event.

## 4. Proposed architecture — not applied

Separate discovery presence from ownership/health of an already established stream. A passive absence or temporary group incompleteness should update discoverability without immediately cancelling that stream. Retain the active receiver snapshot and its explicit stop controls while showing that discovery is currently unavailable. Continue to reject new starts for incomplete/offline discovered rows.

Let terminal native transport/capture errors end or retry playback under the existing session policy. Do not introduce continuous UI-thread DNS/RTSP polling, and do not treat a local UDP send or a successful feedback response as proof of audible output. Explicit user stop and shutdown must remain responsive while initiating cancellation and awaiting asynchronous cleanup; acquiring the lifecycle semaphore and restoring mute after cleanup are not an immediate-completion guarantee. Decide explicit adapter-selection changes as a separate user-action policy; a selected unavailable adapter must remain paused without fallback to all interfaces.

Keep JSONL v1 and config schema 2 unchanged. Session ownership guards from ISSUE-15 are needed for any delayed discovery action. Apply the same last-known active snapshot policy to stereo without inventing RTP resume or accepting an incomplete group for a new handshake.

## 5. Acceptance criteria and verification

- A fake Streaming receiver survives an empty passive discovery publication with no new `start`, `stop`, process disposal, or mute restore.
- A complete active stereo session survives an intermediate `1/2` snapshot; a new start on that incomplete row remains rejected.
- The active row retains working stop controls and shows its discovery status; inactive missing rows still follow the selected-interface catalog policy.
- Manual receivers remain independent of empty discovery publications.
- A terminal fake engine fault still ends playback, preserves its diagnostic fault, restores the exact saved mute bit through the audio abstraction, and retries only under current retry settings.
- Explicit stop, shutdown, and selected-adapter no-fallback behavior remain covered. No schema or IPC change is needed.

Existing coverage: the [stereo identity regression](../../desktop/AirFlash.App/Verification/UiRegression.cs#L222) currently asserts that member disappearance stops playback; that assertion would intentionally change under this proposal. [SessionTests](../../desktop/AirFlash.Tests/SessionTests.cs#L126) covers cancellation and [incomplete-group rejection](../../desktop/AirFlash.Tests/SessionTests.cs#L220). A dedicated healthy-stream/empty-snapshot retention assertion and active-row rendering assertion are missing.

Safety: perform these checks with mocks only. Hardware qualification is separate authorized work; any later audio probe must remain at gain ≤0.1 and duration ≤5 seconds. No correspondence to upstream issues #6/#7 or `upstream/dev` is claimed.
