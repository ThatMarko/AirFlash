# How a session ends

## Current-source boundary — 2026-10-21

Against stable `439de626`, combined `6c1e2ef` preserves terminal IPC classification in the remaining hello/query consumers. Pairing now observes terminal events while input is open, cancels/joins the prompt and retains one pending reader across successful input. Explicit ownership, recovery admission and exact-bit cleanup remain separate from passive catalog availability. A later green run neither explains the earlier raster failure nor establishes a physical receiver incident cause.

[Current source, acceptance and executed limits](../issues/PR-READINESS-2026-10-21.md) supersede older current/open/schema labels below. The remaining source links and test counts retain their recorded historical baselines; this documentation refresh claims no new test execution or upstream publication.

Source reviewed on 2026-10-12 at own-fork `main` `06237873dcb6394eb35e97a017c536843bd7743d`, containing original stable `cf420d97700463c56adc31408510e8336e99e51c`. This page describes the integrated fork runtime; original upstream publication and historical test sources are separate.

Each desktop playback attempt owns one engine process; a native worker stopping does not terminate its command loop. Explicit Stop, terminal fault, pipe failure or cancellation disposes that child. A retry performs a new handshake/process with new native clock/RTP identities. Passive receiver absence/incompleteness retains the owned attempt. [Session](session.md), [Playback coupling](playback-coupling.md), [Authentication](auth.md) and [Engine](engine.md) describe the other boundaries.

## Desktop outcomes

| Trigger | Result | Diagnostics and local mute |
| --- | --- | --- |
| Explicit Stop | Cancel/await worker, clear receiver and publish Idle; no automatic restart until allowed intent | Cleanup attempts exact captured endpoint bit; no immediate-completion guarantee |
| Missing/partial/repeated discovery snapshot, or adapter browse restart | Retain owned Connecting/Pairing/Streaming/Standby and Stop controls | Preserve complete transport/diagnostics; no passive mute restore or offline retarget |
| Guarded complete present receiver with changed transport signature, or actual capture/latency/rate change | Replace non-Pair attempt through serialized new start | New-owner diagnostics clear; old attempt cleans up/restores, later streaming mutes again |
| Same-owner identity promotion with unchanged transport | Retarget snapshot/preferences, keep worker | Metrics/native retry do not change owner; mute stays owned |
| Explicit terminal IPC process rejection when consumed by the owned reader | Error with original command_error/ipc detail and false retryability | Final even under force reconnect; cleanup preserves pre-fault state/restores |
| Other final engine error | Error; no retry | LastFault retained; restore after cleanup |
| Retryable engine/host error, force reconnect off or budget exhausted | Error; stopped | Fault/pre-fault snapshots retained |
| Retryable error with force reconnect/budget/current lifetime | Connecting/backoff then fresh child | ReconnectCount increments for next attempt; live metrics clear, pre-fault retained; exact restore attempted between attempts |
| Pair blank/cancel | Idle; playback start never sent | Prior playback already cleaned up; pairing does not mute |
| Pair token timeout/other failure | Error except whole-lifetime cancellation; no pairing retry loop | Original structured error preserved |
| Unavailable/incomplete new start or unresolved recovery with muting enabled | Reject admission before new engine | No new mute; pending prior recovery remains visible |
| Engine initialization/probe failure | Existing shell shows Failed and Retry engine | No playback admission/mute; async probe failure is not itself forced app exit |
| Shell-construction/startup exception | Startup error notice and Quit path | Outside playback retry |

The authoritative controller is [start/stop](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/SessionController.cs#L308) and [RunStreamAsync](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/SessionController.cs#L444); asynchronous initialization is [EngineInitialization](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/EngineInitialization.cs#L14). Default Connect/Read/PIN/Retry budgets are 40 seconds/15 seconds/120 seconds/one second. Nonmatching events share the same read deadline. Read expiry yields “The audio engine response timed out.”; an already-expired connect deadline yields the distinct connect message.

The playback connect deadline starts after desktop DNS resolution/process creation and before waiting for the EQ writer or sending Start. Time spent in that wait/send reduces the remaining event-read budget, but the connect deadline does not itself cancel or bound those operations. Pair uses Connect for each event read, with a separate PIN callback token. While awaiting that callback, its sole reader does not process a queued rejection/EOF; cancellation or timeout can win before the original queued fault is read. Consequently these values are operation budgets, not hard end-to-end Start/Pair completion guarantees. A normal matching event after Streaming can begin another 15-second read even if its event name is ignored; this is pipe inactivity detection, not a worker heartbeat or proof of audio delivery.

Idle and Error are inactive. Auto-connect needs current intent/canonical visible available complete eligibility and an unattempted candidate under app settings/controller gates. Rejected stale work does not count an attempt or clear newer Stop suppression; admitted failed connection does count. [After discovery](after-discovery.md) owns ranking/reset rules.

## How an engine error becomes retryable

[Fault::from_error](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/transport.rs#L63) forwards existing structured Faults or classifies:

| Cause | Code | Retryable |
| --- | --- | --- |
| Cancellation | cancelled | false |
| Peer closure | peer_closed | true |
| Read timeout | read_timeout | true |
| Write timeout/poisoned cipher | write_failed | true |
| Authentication tag | authentication_failed | false |
| I/O reset | connection_reset | true |
| Other I/O | socket_error | true |
| Unclassified protocol | protocol_error | false |

Authentication rewrites generic protocol_error to final authentication_failed, retaining transport errors. Credential loading precedes that wrapper; malformed/decrypt data can be final setup protocol_error, while preserved file I/O can be setup socket_error/retryable. Neither retries transient authentication after failed verification.

Native error_event's fallback scans pairing/SRP/verification/credentials/signature/identity/authentication; matches are final authentication_failed, otherwise retryable engine_error. Raw PIN messages need not match, although Pair still stops. The process envelope stamps otherwise unclassified command errors final command_error/ipc. equalizer_error/device_volume_error are separate nonfatal controls.

Desktop explicit false prevents retry; explicit true is not overridden by keywords. Only missing/non-Boolean retryability uses its authentication scan. Force reconnect, unexhausted budget and current cancellation are also required. A successful stream of at least ten seconds resets the retry budget before the next failure.

[exact empty-correlated rejection recognition](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/EngineEvents.cs#L8) occurs before ordinary version/session filtering. Malformed/non-object output becomes controlled final protocol_error/ipc; stale ordinary events remain ignored. General symmetric line guards and worker-liveness reporting were not added.

## Inside the engine process

| Watch | Warning/allowed intermediate behavior | Terminal condition |
| --- | --- | --- |
| Feedback | Due two seconds after start/completion; feedback_delayed at four seconds; only 500/502/503/504 get retry notices | Twelve-second transaction timeout; third consecutive transient status; 401/403 final authentication, 454 retryable rejection, others final rejection |
| Event TCP | Idle/incomplete data may persist | Read/parser/protocol-token/write fault |
| Media UDP | First failed local send warns; audio continues | Twelve seconds without successful local send |
| Scheduler | Live ≥60 ms lateness skips expired slots and recovers | Finite probe qualification_late |
| Capture | Silent endpoints may supply no packets, permitting padding | Initialization/capture/resampler fault; failed disconnected probe after >two seconds without event |
| NTP/PTP setup | Before member TCP | Any bind/setup failure, not only busy ports |
| Member handshake | Info/auth/setup phases | Protocol/auth/response/codec/port failure |

Feedback's four/twelve-second timers exclude shared RTSP mutex wait. Warnings/metrics alone do not stop playback or restore mute. Standby labels the same worker from raw source audio markers before EQ/gain; transport continues.

Normal-loop cleanup emits final capture/transport/aggregate metrics, joins volume, then each member's feedback, best-effort TEARDOWN and events. Earlier initialization failure may have no final metrics. TEARDOWN uses a fresh cancellation token and separate 300 ms write/read budgets; mutex acquisition and joins are not covered. [Member::close](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/session.rs#L449) and [worker cleanup](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/session.rs#L708) establish order.

Media-loop failure emits error rather than its normal stopped; earlier setup cancellation can also emit an error. Successful handshake-only qualification emits handshake_complete and successful Pair emits paired. Each leaves the main command loop/stored worker slot alive. A later Stop can independently emit stopped. EOF/drop cancels/joins and normally returns success even after a JSONL rejection. A join failure is ignored, so the current IPC contract does not guarantee a terminal event for a worker panic. There is no nonzero-exit taxonomy for expected command errors. Current process diagnostics record exit status; playback stdout EOF remains a host exception.

## What the new process does not keep

Native sequence/SSRC/RTP origin, per-member UUID, PTP clock, encryption counters and capture counters reset per worker. Desktop owner survives a retry. [RecordFailure](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/SessionController.cs#L595) retains LastFault, CaptureBeforeFault and TransportBeforeFault; a retry failing before fresh metrics cannot erase the last streaming snapshot. Fresh attempts clear live metrics/warnings/volume; explicit new start clears prior-session diagnostics.

Group H's optional padding values remain in immutable pre-fault capture snapshots. Monitor/Copy displays available values without inventing zeros. No counter is an acoustic/network-loss measurement.

## Local mute and recovery failure

[RestoreAudioAsync](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/SessionController.cs#L646) attempts exact-bit restore after cleanup and logs failures best-effort. [RestoreRecord](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/MuteRecoveryCoordinator.cs#L157) still attempts the live-owned endpoint even if journal persistence fails. Known successful restore/save failure retries persistence only while alive; restart loses that certainty. Busy/corrupt/unavailable/pending recovery blocks a new muted attempt, including a selected Recording source when the option is enabled.

Saved playback ids resolving to Recording/unknown direction are never read/written via mute; their identity/prior-bit obligation remains pending. Modeless recovery choices are outside locks. See [Control and mute](control-and-mute.md) and [the recovery contract](../MUTE-RECOVERY.md). Normal restoration has no hard wall-clock deadline, and mocks do not prove physical reboot durability.

## Exit status and verification limits

Core [IPC fixtures](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/IpcFailureTests.cs#L21), [retry snapshot fixtures](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/CapturePaddingMetricsTests.cs#L180), [recovery admission fixtures](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/MuteRecoveryAdmissionTests.cs#L15) and lifecycle tests cover observable failure/cancellation/cleanup using injected services/gates. Native continuity, RTSP, EQ and credential tests use localhost/synthetic fixtures. WPF tests exercise actual controls, dispatcher/dialog/bindings with fake services. [B](../issues/GROUP-B-ACCEPTANCE.md), [F](../issues/GROUP-F-ACCEPTANCE.md), [G](../issues/GROUP-G-ACCEPTANCE.md), [H](../issues/GROUP-H-ACCEPTANCE.md) acceptance records retain exact executed sources and controlled failures.

Executed suite/package evidence is centralized in [Verification](verification.md): [exact `0623787` normal CI](https://github.com/ThatMarko/AirFlash/actions/runs/38329156167) and the separate full package run at code-identical `4ae1db4`. Subsystem/group acceptance records retain earlier controls and source-specific results. This deeper review inspected source and regression assertions; it did not rerun suites or hardware checks.

No fixture establishes real receiver handshake reliability, acoustic delivery, OS endpoint persistence across power loss or hard process-kill/restore timing.

## Implemented Group B behavior

All retained states survive passive discovery absence. Current owner/intent guards prevent delayed identity/settings/endpoint work from changing a replacement attempt; metrics and retries preserve owner. Full Group B is in fork main. Original PR #19 contains partial stable retention work, not all atomic ownership/admission changes; PR #15 is closed/unmerged. Publication status is in B acceptance.

## Historical audit

At `41190e0`, missing/incomplete discovery called Stop, async identity/autoconnect lacked current ownership gates, and empty-correlated IPC rejection could become a substituted timeout. Before stable's current shell initialization path, startup hello failure also quit rather than showing Failed/retry. The [previous pinned page](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/analysis/failures.md) retains original outcome tables; their discovery-stop, in-memory-only mute and pending-reboot/current-proposal statements are historical.
