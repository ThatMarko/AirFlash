# How a session ends

One desktop playback attempt uses one engine process. The desktop cancels/disposes it for stop, discovery loss, engine fault, or lost/invalid stdout events. Native worker failure alone emits `error` and leaves its command loop alive until desktop cleanup/EOF; worker lifetime and process lifetime are distinct ([main.rs:157–172,324–340](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/main.rs#L157)). A retry is a new process/handshake. [Authentication](auth.md), [Engine](engine.md), [Session](session.md), and [Playback coupling](playback-coupling.md) own the detailed paths.

## Desktop outcomes

| Trigger | State after | Reconnect count | Local mute | Next audio |
| --- | --- | --- | --- | --- |
| User stop | Idle. Receiver cleared | Unchanged, then cleared on the next start | Restored to the bit saved at mute | Stays on the PC until the next `streaming` |
| Discovery snapshot misses the playing discovered receiver, or the pair is no longer 2/2 | Idle, by `StopAsync` | Does not increment. A later auto-connect starts at 0 | Restored | A new process only if auto-connect is allowed and the row returns complete |
| Address, port, stereo-member leader flag, capture endpoint, latency, or sample rate changes on an active non-pairing session | Connecting, then Streaming on success | Cleared, because `StartLockedAsync` installs empty diagnostics | Restored, then muted again on the new `streaming` | New handshake |
| Identity promotion with the same transport signature | The existing Streaming or Standby snapshot, with the new receiver object | Unchanged | Held | Same process |
| Engine `error` with `retryable: false`, or an authentication keyword with no retryable flag | Error. `LastFault` kept | Does not increment | Restore requested after cleanup | Stays stopped even if force reconnect is on |
| Engine `error` with `retryable: true`, force reconnect off | Error. Fault and pre-fault metrics kept | Does not increment | Restored | Stays stopped. The default |
| Same error, force reconnect on, attempts left, and the user has not cancelled | Connecting with "Disconnected; reconnecting (n/m)", then a new process | Increments when the next attempt begins. A stream that lasted at least 10 seconds resets the attempt budget first | Restored, then muted again if the new attempt reaches `streaming` | Full handshake. RTP does not continue |
| Connect-event deadline expires before `streaming`, or no accepted matching stdout event for 15 seconds afterward | Error, "The audio engine response timed out." from read timeout, or "Timed out connecting to the audio device." if the deadline already elapsed before the next read | Host-error retry rules | Restore requested after cleanup | Same |
| Playback IPv4 resolution, process spawn, or command/JSON read failure | Error | Retries only if force reconnect permits the host error; a spawn failure may open no successful process | Restore requested | Stopped or retried |
| Startup `hello`/engine extraction/spawn failure | Startup logs error, shows startup-failed message, and quits; this is outside the session retry loop | No playback attempt | Playback mute was not yet applied | App startup stops |
| Pairing PIN dialog returns blank/cancelled | Idle | Reset by pairing start | Previous playback was stopped/restored before pairing; pairing itself does not mute | `start` is not sent |
| Pairing PIN timeout or other pairing failure | Error, except cancellation of the whole lifetime | Reset by pairing start; no playback retry loop | Pairing itself does not mute | `start` is not sent |
| Incomplete stereo `StartAsync` or `PairAsync` | The exception reaches the panel notice | No process | Unchanged | Stopped |

`IsActive` is Connecting, Streaming, Standby, or Pairing. Error and Idle are inactive, so auto-connect may fire on a later discovery result. Auto-connect itself is off unless the global setting or the receiver override enables it. A user stop suppresses it until the next manual play, or until global auto-connect is turned from off to on.

Retry exhaustion remains Error; it does not publish Idle. Stop and fault mute restoration are best-effort and may follow a one-second stop-send budget and process disposal's two-second wait before kill. The saved bit is written back on successful restoration, not guaranteed synchronously at the user's stop click ([SessionController.cs:218–224,370–393,435–463](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SessionController.cs#L218)). Auto-connect also excludes ids in `_autoAttempted`, so being inactive alone does not guarantee another attempt; a later offline-to-online/incomplete-to-complete transition can clear that entry ([AppViewModel.cs:202–206,312–323](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L202)).

## How an engine error becomes retryable

[`Fault::from_error`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/transport.rs) classifies the error before the desktop sees it. An error that is already a `Fault` is forwarded with its own code and flag.

| Cause | Code | Retryable |
| --- | --- | --- |
| `WireError::Cancelled` | `cancelled` | false |
| `WireError::PeerClosed` | `peer_closed` | true |
| `WireError::ReadTimeout` | `read_timeout` | true |
| `WireError::WriteTimeout` or `Poisoned` | `write_failed` | true |
| `WireError::Authentication` | `authentication_failed` | false |
| I/O `ConnectionReset` | `connection_reset` | true |
| Any other I/O error | `socket_error` | true |
| Any other `anyhow` error | `protocol_error` | false |

The authentication-exchange wrapper rewrites generic `protocol_error` to `authentication_failed` with the flag still false. Transport closures/timeouts/I/O failures inside authentication keep their transport classification and may be true. Loading an existing credential file happens before that wrapper, so malformed/decrypt-failed files can be `protocol_error` on channel `setup`, and file I/O can be retryable `socket_error` ([session.rs:235–251,534–546](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/session.rs#L235)).

For worker errors not already `Fault`, `error_event` scans pairing, SRP, verification, credentials, signature, identity, or authentication. Matches become final `authentication_failed`; everything else becomes retryable `engine_error`. This fallback covers capture/PTP-bind errors and persistent-pairing errors; raw PIN messages need not match the scan. IPC `error` events stamped by `send` are final `command_error`; nonfatal `equalizer_error` and `device_volume_error` are distinct event kinds ([transport.rs:92–110](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/transport.rs#L92), [main.rs:22–27,181,200–201](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/main.rs#L22)).

The desktop stops retry when the structured flag is false. Only a missing/non-boolean flag invokes its keyword scan; explicit true is not overridden by authentication words. Retrying also requires force reconnect, an unexhausted attempt budget, and no lifetime cancellation ([SessionController.cs:373–393,432](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SessionController.cs#L373)). Pairing does not use this playback retry loop.

## Inside the engine process

The media worker exits on a stored health fault, capture/scheduling/packetization error, or cancellation. Normal-loop cleanup emits final metrics, drops volume, joins feedback, attempts TEARDOWN, then drops events. An earlier initialization failure still drops resources but may emit no final metrics. TEARDOWN has separate 300 ms write/read budgets using a temporary fresh cancellation token, with no timeout covering shared mutex acquisition or worker joins. Desktop `stop` or EOF sets the lifetime flag; a failed worker emits `error` instead of its normal `stopped` ([session.rs:430–446,596–700](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/session.rs#L430), [rtsp.rs:330–337](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/rtsp.rs#L330)).

| Watch | First symptom | Session ends when |
| --- | --- | --- |
| `POST /feedback`, 2 seconds after startup/previous completion | `feedback_delayed` at 4 seconds after acquiring the RTSP mutex/starting the request. Only 500/502/503/504 emit `feedback_retry` | No response by 12 seconds; third consecutive 500/502/503/504 gives retryable `feedback_rejected`; 401/403 are final authentication; other statuses (including 501) are `session_rejected`, retryable only for 454. Write/read/parse/CSeq errors can fail earlier |
| Event TCP | Idle/incomplete reads are allowed indefinitely | Read, parser, protocol-token, or response-write error on channel `events` |
| Audio UDP | `media_send_delayed` on the first failed send. Audio continues | 12 seconds with no successful send |
| Sender schedule | `sender_late_recovered` after skipping slots at least 60 ms late | Live playback recovers; a finite probe ends with final `qualification_late` |
| Capture thread | Silent endpoints may send no packets; sender pads silence | Any initialization/capture/resampler failure. After no event and more than 2 seconds without a packet, a failed `GetNextPacketSize` probe adds `capture endpoint disconnected` context; ordinary packet reads can fail sooner |
| NTP/PTP bind |  | Any bind/setup error, not only a busy port. NTP starts first, PTP only for `timing: ptp`, and failure precedes member TCP. Pairing binds neither |
| Handshake | `phase` info/authenticate/authenticated/setup events | Failed status, codec, response/port validation, or authentication checks. Protocol/crypto authentication faults are final; authentication transport failures can retry |

A warning/transport-metrics event does not itself change playback state or restore mute. Standby labels the same process; it is computed from `last_audio_qpc_ns` (resampled source amplitude >0.004, about −48 dBFS, before equalizer/master gain), or elapsed time since streaming if absent. Default threshold is 10 seconds and standby defaults off. RTP/feedback/PTP continue; master mute/gain alone does not make audible source samples count as silence ([live.rs:141–153](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/live.rs#L141), [SessionController.cs:360–366](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SessionController.cs#L360)).

## What the new process does not keep

Sequence numbers, SSRCs, RTP origin, per-member session UUIDs, PTP clock id, encryption counters, and underrun counters belong to their native worker. The desktop creates a fresh process/worker for every retry. The panel retains `LastFault`, `CaptureBeforeFault`, and `TransportBeforeFault` across successful retries too; a new `StartLockedAsync` clears them, including a restart for the same receiver. Live fields are cleared when the next retry process is opened and repopulated only by fresh metrics ([SessionController.cs:190–195,300–301,325–329,379–384](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SessionController.cs#L190), [SessionTests.cs:260–278](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/SessionTests.cs#L260)).

Discovery loss and engine faults use different paths. Discovery calls `StopAsync` without incrementing reconnect count; the next playback starts at zero and can accumulate its own later engine retries. Engine faults increment `ReconnectCount` only inside the force-reconnect loop, which defaults off.

## Exit status and verification limits

Native expected errors are JSONL events, not a defined nonzero exit-code taxonomy. `main` returns `()` and handles schema/worker errors by emitting events and continuing. EOF normally exits successfully even after an emitted error; panic, OS termination, or forced kill are separate cases. Desktop `ProcessEngineConnection` does not interpret `ExitCode`; it sees stdout EOF as "The audio engine exited." The IPC integration test explicitly expects successful exit after rejected commands ([main.rs:54–110,157–161,305–340](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/main.rs#L54), [EngineConnection.cs:40–59](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/EngineConnection.cs#L40), [equalizer_ipc.rs:9](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/tests/equalizer_ipc.rs#L9)).

Source audit: runtime baseline upstream `41190e0`, documentation baseline `c077a05`. Existing desktop tests cover cancellation/disposal, fresh-session retries, authentication rejection, timeout, mute requests, nonfatal warnings, saved diagnostics, and structured flags ([SessionTests.cs:126–302](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/SessionTests.cs#L126)). Native localhost tests cover feedback grace/status/CSeq, idle event sockets, simulated UDP failures, schedule recovery, cancellation/TEARDOWN, and parser/crypto errors ([continuity.rs](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/tests/continuity.rs), [rtsp_contract.rs](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/tests/rtsp_contract.rs)). They do not reproduce full HomePod handshakes, OS endpoint removal, process-kill latency, or acoustic output. No audio/network probes or new tests were created in this audit.


## Implemented Group B behavior

The preceding audit remains the description of stable `41190e0`. This addendum describes local branch `codex/fix-session-lifecycle`, tested head `342aeb77cb85ff6f3f7f850161b3db79d8637945`, based directly on that stable commit. This feature is included in local fork `main` at `72dc7c7df6649a95271e0a86ae62f371397b2ca4`; original upstream `main` remains at `41190e0` without these fixes. See the [feature acceptance checklist](../issues/GROUP-B-ACCEPTANCE.md) and [combined fork validation](../issues/FORK-INTEGRATION-ACCEPTANCE.md) for separate implementation and later integration evidence.

Discovery absence/incompleteness is no longer treated as a reason to cancel an owned session. Terminal worker/handshake/pairing errors, PIN cancellation and explicit stop still reach their existing cleanup paths. Retry enablement, structured retryability and attempt budgets remain effective. A permitted retry retains the desktop owner and fault/pre-fault diagnostics while creating a fresh native process/session id.

Metrics do not change lifecycle ownership. Stale identity reconciliation and rejected automatic intent cannot replace a newer owner or erase its receiver/settings/diagnostics; rejected automatic work also preserves valid persistence and newer Stop suppression. Seven isolated original-source observations reproduced the prior selection, admission and retention defects. The acceptance checklist records passing replacement coverage and independent review; these mocks do not establish a hardware fault cause.
