# Control traffic and local mute

Source reviewed on 2026-10-12 at own-fork `main` `8989b3609dd1294f4d1d6eb1502f345cc22bc382`, containing original stable `cf420d97700463c56adc31408510e8336e99e51c`. This page describes the integrated fork runtime; original upstream publication and historical test sources are separate.

WPF owns local endpoint mute; Rust owns receiver volume, feedback and PTP workers. Passive discovery absence no longer stops an owned attempt or restores its endpoint. Explicit Stop, terminal failure, cancellation and actual replacement still enter cleanup. See [Playback coupling](playback-coupling.md) and [Failures](failures.md).

## Local mute ownership and restoration

[AudioService](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Services/AudioService.cs#L29) serializes endpoint/journal work on one MTA worker. It implements the separate [IMuteRecoveryService](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/MuteRecovery.cs#L24) capability. Constructor/notifications and endpoint enumeration do not initialize recovery; [normal startup](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L167) explicitly initializes it before settings/lifecycle locks. Read-only diagnostic paths return before ordinary shell construction.

The actual opened endpoint id and direction are authoritative. Null selects the default console Render endpoint. Explicit ids must match the returned identity before direction, mute-bit or journal access. Recording inputs never acquire new ownership and never read/write their mute bit; unknown/failed direction cannot acquire ownership. A saved playback id now resolving to Recording or uncertain direction remains pending until a playback endpoint returns. Catalog labels cannot override this boundary.

[Mute](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/MuteRecoveryCoordinator.cs#L88) records owner GUID, positive acquisition generation, actual endpoint id and exact prior true/false bit. Recovery ownership is separate from desktop lifecycle generation and engine session id. The coordinator holds one exclusive lifetime file lease; PID alone is never ownership proof.

| State | Meaning |
| --- | --- |
| Intent | Prior bit persisted before the endpoint setter can mutate. |
| Owned | Live mute acquisition recorded; repeated mute does not recapture the prior bit. |
| Restoring | Exact-bit restoration has been requested; interrupted completion can remain ambiguous. |
| Resolved | Restoration was completed/observed and persistence may settle the obligation. |
| Abandoned | Explicit choice keeps the current bit and settles without claiming restoration. |

Intent is persisted before mutation; in-memory ownership is also established before a setter that can mutate and throw. Acquisition failure attempts tracked exact-bit rollback. Failed initial persistence cannot intentionally create an untracked mute. A repeated owned mute neither overwrites the saved bit nor reasserts a later external change.

[FileMuteRecoveryJournal](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/FileMuteRecoveryJournal.cs#L14) uses a constant `mute-recovery.lease`, held open with FileShare.None for the lifetime. Its separate version-1 journal is under per-user local `AirFlash/mute-recovery`; diagnostic data overrides use isolated output. It is not config schema 2 or credential storage. Parsing bounds are 64 KiB, 32 entries, 1,024-character ids and depth eight; duplicate/missing/unknown fields, invalid ids/owners/generations/states and unsupported versions are rejected conservatively.

Writes create a unique same-directory temporary, WriteThrough/Flush(true), replace, then flush the committed handle. A final flush can fail after replacement; the coordinator may recognize its exact intended bytes without claiming success. This requests file-data durability, not proof about filesystem metadata or physical power loss. [The separate recovery contract](../MUTE-RECOVERY.md) details storage limits.

## Recovery choices and admission

[Refresh](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/MuteRecoveryCoordinator.cs#L25) acquires the lease, reads/validates once and checks later reads against expected bytes. A stale record whose current bit already matches its saved bit can settle without an endpoint write. Differing bits remain pending: Restore saved, Keep current or Later. Decisions revalidate revision, owner, generation, endpoint direction/id, journal bytes and the displayed current bit before mutation.

Busy lease, unreadable/unknown data or unavailable output blocks another attempt that would mute local audio. Enabled local-muting conservatively requires a ready journal even when the selected capture source is Recording. Disabling local muting permits playback without discarding recovery information. [start admission](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs#L308) checks before/after old-worker cleanup, [retry admission](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs#L444) before opening another engine, and the coordinator again before mutation.

[modeless review](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Ui/MuteRecoveryWindow.cs#L10) runs outside audio/settings/lifecycle locks and keeps Stop accessible. Its notices are independent of playback notices. Unsupported readable bytes can be explicitly archived: exact bytes are backed up/flushed before consuming the original. A failed/ambiguous archive remains blocking across refresh.

Unresolved obligations preserve endpoint identity, prior bit and ownership. They do **not** promise permanently byte-identical serialization: live Restoring transitions or persisting another settled entry can rewrite a valid journal while retaining its unresolved obligations. Exact-byte guarantees apply to unsupported-source preservation/archive and the specified unchanged-inspection fixtures.

## Normal cleanup and endpoint return

[Restore](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/MuteRecoveryCoordinator.cs#L140) only acts on live/known-settled records, not arbitrary stale records. It attempts exact restoration even when persisting Restoring fails. If the endpoint write succeeds but saving fails, the living process retains KnownRestored; later attempts retry disk only and do not overwrite a subsequent external bit. That certainty is lost on restart, so old Intent/Restoring data requires conservative inspection again.

[streaming](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs#L498) mutes only after native capture initialization/streaming, when enabled. Stream catch/finally, Stop after awaiting the owned worker, disabling local muting, controller disposal and AudioService disposal attempt restoration. Standby keeps the same owned mute. Native stop-send has a one-second budget and process disposal waits two seconds before kill; joins/other cleanup are additional. There is no reconnect grace or hard immediate restoration deadline.

Endpoint enumeration now covers active Playback and Recording, while the default remains console Render. Notifications are debounced by EndpointCatalog. [OnEndpointsChangedAsync](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L268) captures owner/intent before asynchronous work, guards default-loopback restart/inactive restoration, then refreshes recovery outside the settings gate. [deferred endpoint restoration](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs#L246) waits for the specific owned terminal worker, rechecks owner/intent/inactivity and surfaces failure; disposal drains admitted recovery.

## One control connection per member

[Member](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/session.rs#L237) places each encrypted RTSP control connection behind Arc<Mutex<Connection>>. Setup/teardown, member feedback and shared volume acquire it for complete transactions. Event TCP and media/retransmit UDP are separate.

| Worker | Schedule | Operation |
| --- | --- | --- |
| Feedback per member | First two seconds after start; subsequent two seconds after completion | Empty POST /feedback |
| Volume shared by members | Immediate initial read, next check for new sequence, otherwise one second after completed pass | Changed SET_PARAMETER volume to all members, then GET /info sequentially |

Volume idle polling sleeps 20 ms. New edits do not interrupt an in-flight request. No initial receiver-volume write occurs merely because streaming begins. Shared mutex contention can delay feedback/volume; feedback's four/twelve-second timers begin **after** acquiring the lock and exclude that wait. [feedback](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/transport.rs#L278) and [volume worker](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/volume.rs#L98) own these schedules.

## Volume confirmation

Receiver volume is independent of PCM gain. Zero maps to −144 dB; 1–100 maps to percent×0.3−30 dB. GET /info accepts finite nonpositive real/signed integer initialVolume; invalid/missing values are unavailable. Top-level host/value is the first peer (desktop leader first); available requires every member readable, not matching pair values.

[Pending::status](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/volume.rs#L58) checks in order: an outstanding write with all matching reads/no write failure confirms; otherwise less than three seconds remains pending; write failure or mismatch after budget is unconfirmed and settles. Later readable polls report confirmed even if externally changed or member values differ; missing reads are unsynced. Matching is checked before timer expiry, so this is not a hard acknowledgment deadline.

Desktop [device-volume editing](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs#L123) coalesces at 200 ms, uses newest sequence and a separate three-second wait after IPC send. It rejects obsolete results, preserves last known value when unavailable, disables control and prevents late pending events resurrecting expired targets. New engine attempts reset receiver volume.

## PTP sockets and interface selection

[PtpMaster::start](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/clock.rs#L96) binds wildcard IPv4 319/320 before member TCP; failure of either bind aborts the attempt. Clock id is random/masked 63-bit. Every 125 ms it unicasts sync/follow-up, with announce initially/about every second. Send failures are ignored. Incoming packets must be from a peer, version nibble two and valid declared length; received counts valid packets, not proven synchronization. Delay/pdelay replies do not compute/apply a remote clock offset.

Discovery adapter id is not supplied to these sockets or the ordinary unbound TCP connect. Windows routing chooses egress; TCP local address then binds member media/control UDP and supplies timingPeerInfo. Selected discovery interface is therefore not a general streaming bind guarantee.

The per-worker NTP socket is wildcard/ephemeral, accepts only 32-byte peer requests masked type 0x52 and responds to their source. Desktop PTP omits its port from SETUP. See [Capture and clocks](capture-clocks.md).

## Verification coverage

[journal/endpoint fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/MuteRecoveryTests.cs#L15) cover exact prior bits, every meaningful mutation/persistence interruption, multiple endpoints, lease conflict, corrupt/bounded records, external changes, recording/wrong identity and conservative recovery. [admission fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/MuteRecoveryAdmissionTests.cs#L15) and [MTA/modeless UI checks](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiMuteRecovery.cs) verify the actual service/controller/dispatcher integration with injected endpoints and temporary stores. [G acceptance](../issues/GROUP-G-ACCEPTANCE.md) records executed checks and limitations.

Native volume/continuity fixtures use localhost synthetic peers. They do not qualify real COM behavior, route choice, PTP accuracy or physical mute persistence after reboot/power loss; a saved bit cannot prove no later user/application change.

The [continuation acceptance](../issues/CONTINUATION-ACCEPTANCE.md) records exact-main CI: 667 Core, 124 Python and 69 native tests passed; recording-device qualification and the thirty-minute localhost soak remained ignored. Complete packaged integration at `4ae1db4` passed six 1,059-assertion portable UI runs, three actual 125% DPI runs with 40 assertions each and disposable-runner installer checks. Main `8989b36` differs in documentation only. This refresh is source/documentation review, not a new hardware or suite execution.

## Implemented Group B behavior

Passive absence/incompleteness retains Connecting, Pairing, Streaming and Standby, including Stop controls and the complete owned transport. Present complete data can only retarget the captured owner/intent under serialization. Adapter browse restart retains that attempt while discovery obeys the selected interface. This complements the journal; it does not reinterpret missing discovery as endpoint recovery.

## Historical audit

Original `41190e0` kept prior bits only in an in-memory dictionary after successful mute. Group B initially added guarded normal endpoint recovery; G later supplied durable ownership and recording protection. The [previous pinned page](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/analysis/control-and-mute.md) retains the full dated audit. Its in-memory-only/current-unimplemented/pending-reboot labels no longer describe fork main.