# Playback coupling

## Discovery preparation supplement — 2026-10-13

Current fork code `c5ddb894e8bf0f977a94920ff26878c3cf5e1004` integrates standalone D `b8ee79511e2fa41ee02651dad83d50e008f9f680`, based on original `cf420d97700463c56adc31408510e8336e99e51c`. The pinned062 description-subset notification limitation below is superseded by full record/TXT publication. gpn affects group name; igl affects leader ordering/TransportKey and applicable owned retarget/restart; cn affects subsequent start codec payloads, not an automatic codec-only engine restart; pk affects compatible endpoint merging. This adds no authority to stale callbacks: current token/epoch/deadline and existing app/controller guards still apply. Offline/partial transport retention and manual routing policy are unchanged.

[D acceptance](../issues/GROUP-D-ACCEPTANCE.md) records source, tests, reviews and limits. The earlier source map below retains its pinned historical baseline.

This page describes fork `main` at **`06237873dcb6394eb35e97a017c536843bd7743d`**, reviewed on 2026-10-12, including original stable `cf420d97700463c56adc31408510e8336e99e51c`. Discovery availability and owned transport now have separate lifetimes. [Audit](audit.md), [B acceptance](../issues/GROUP-B-ACCEPTANCE.md) and [current qualification](../issues/CONTINUATION-ACCEPTANCE.md) define evidence limits.

## The check

[ReconcileDiscoveryAsync](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L301) captures snapshot, desktop owner and intent atomically before any awaited identity save. A valid save/migration can complete even if the session changes, but applying a replacement requires fresh owner/intent equality and [serialized controller validation](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/SessionController.cs#L397).

Missing, offline, partial, overfull or repeated discovery does not Stop an owned Connecting, Pairing, Streaming or Standby attempt. Only complete online data can update its receiver/settings. A valid same-owner identity promotion leaves transport intact when the effective signature is unchanged. Peer address/port/group-leader flags, capture endpoint, latency or rate changes can trigger the applicable owned non-Pairing restart. A new Pair, explicit intent or same-id replacement rejects stale control work.

A save failure exits before that transaction replaces catalog/session data; independent worker events can still happen. There is no unused conditional passive-Stop plumbing. Manual receivers retain exact explicit endpoints and are not retargeted by discovery.

## What produces the empty snapshot

[WindowsDiscovery](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Services/WindowsDiscovery.cs) publishes cached aggregation on selected change triggers. A resolve compares a description subset rather than all TXT fields: a gpn/igl/pk/cn-only refresh updates its cache without an immediate publication, so group name/leader, endpoint-merge key or codec changes can reach the application only with a later qualifying publication. Explicit interface change clears records; TTL removal/expiry can remove records. Restart retires old queries but retains ten seconds of cache while rediscovery proceeds; it does **not** guarantee a synchronous empty publication before new inline results. Suspended visibility-scoped browsing retains catalog data.

Adapter change policy deliberately retains the already owned attempt while browsing obeys the selected interface. An unavailable selection pauses without fallback. The selected DNS interface is not a playback routing binding. [Discovery](discovery.md) describes PTR-TTL cache lifetime, five-second resolve deadlines and helper ownership separately.

Repeated `Discovery resolved:` descriptions can reflect cache absence or metadata change/reversion. They do not identify a network event, receiver reboot or native-watch condition.

## The row leaves the panel

The ordinary panel filter remains online/non-hidden. [RefreshReceivers](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L371) overlays a current active row excluded by that filter. Offline/incomplete catalog availability stays truthful; the session keeps its complete transport snapshot. `ReceiverViewModel.Active` makes that row's toggle Stop even when new Play is unavailable. Hidden or selected-adapter-removed active rows also retain Stop. Once the attempt ends, a card retained only for ownership disappears.

This is not marking an absent receiver online. New Play/automatic starts reject offline, incomplete or hidden candidates; explicit Pair requires online/completeness and can be offered on a hidden Settings row. Manual rows persist independently. Settings can retain offline history/preferences under its own catalog policy.

## Stop does not increment reconnects

Explicit user Stop and view-model/controller disposal invalidate older automatic intent before waiting for lifecycle serialization. They cancel the actual worker; cancellation during read/retry delay prevents a later retry count increment. The shell's initial Quit request precedes that disposal barrier and first awaits Settings/probe work; [desktop shutdown](desktop-shell.md#shutdown-and-failure-boundaries) describes that source-derived interval. Stop suppression stays set until an intended reset/explicit Play, and rejected stale automatic work cannot clear it.

Force reconnect applies to eligible terminal worker faults, not passive availability loss. Each permitted retry opens a fresh native process/session UUID/PTP/SSRCs while retaining desktop lifecycle ownership. A new Start/restart clears diagnostic history; retries preserve last fault/pre-fault capture/transport while resetting live data. Zero counters do not prove a discovery cause or transport health. [Session](session.md) gives the admission, fault and retry ordering.

## Local mute follows the stop

Mute-while-streaming defaults on and is attempted after the Streaming event. For playback endpoints the [owned journal](control-and-mute.md) records actual identity and exact prior true/false before mutation. Recording endpoints never read/write mute or acquire mute ownership. Normal restore follows owned worker/connection cleanup and is best effort; initially muted stays muted. COM/journal failures remain visible/pending.

Passive discovery disappearance no longer invokes cleanup/restoration. Explicit Stop, PIN cancellation, shutdown and terminal faults still do. If a terminal fault ends one attempt and a later fresh attempt reaches Streaming, exact restoration/remuting may occur between them; there is no immediate restoration deadline or acoustic continuity guarantee. Unresolved recovery conservatively blocks new attempts with local muting enabled.

## What this check does not see

Retaining an attempt does not certify native feedback/event/media watches, UDP receipt or audible output. A native terminal transport/capture/auth/IPC fault still ends or retries under controller policy. A successful feedback response or local UDP send cannot prove speaker output. Network route, receiver OS tolerance, Wi-Fi behavior and physical endpoint recovery need separate evidence.

The PIN dialog remains modal. Stop regression fixtures hold Pair **before** `pin_required`, or complete pairing and close the prompt before exercising normal Stop. CanExecute alone does not establish an accessible underlying control through a modal prompt.

## Source and verification scope

Current [UiSessionLifecycle](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Verification/UiSessionLifecycle.cs), [SessionLifecycleTests](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/SessionLifecycleTests.cs), [SessionTests](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/SessionTests.cs) and [ReceiverSelectionTests](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/ReceiverSelectionTests.cs) cover all four retained active states, empty/partial/repeated results, reappearance, hidden/offline controls, selected-adapter change, exact manual targets, explicit Stop, terminal fault cleanup/retry and both prior mute bits through fake services.

Current main passes 667 Core with warning-free build; code-identical packaged six full regional/catalog runs pass 1,059 assertions each. Detailed source-bound and negative-control evidence is in B/current acceptance. This documentation update runs no real audio, configuration, credentials, receiver or endpoint mutation.

## Implemented Group B behavior

At [historical `41190e0`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L211), reconciliation called unconditional Stop when a captured non-manual receiver was absent/incomplete; a required identity save preceded that decision. An awaited save could make that captured owner stale. Discovery Stop did not set user suppression or increment retry count, and a later Start reset diagnostics. Those facts explain the original policy, not a specific household incident.

Original stable `cf420d9` contains [PR #19](https://github.com/Ding-Kyoma/AirFlash/pull/19)'s passive retention/fresh snapshot/Pairing exclusions. Fork B `442e8d8` adds atomic admission/retargeting, exact selection and retained hidden controls; it is integrated here while old PR #15 stays closed. Reports [01](../issues/ISSUE-01-HIGH-stale-discovery-snapshot-controls-new-session.md)/[03](../issues/ISSUE-03-HIGH-discovery-disconnects-active-playback.md) and B acceptance retain baseline failures. Old pre-reboot/unpushed/incomplete-group statements belong to dated historical audits.
