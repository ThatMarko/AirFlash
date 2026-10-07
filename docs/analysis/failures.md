# How a session ends

One playback attempt is one engine process. It ends because the user stopped it, because discovery removed the receiver, because the engine reported a fault, or because the desktop stopped hearing events. The next attempt, when there is one, is a new process and a new handshake. [Authentication](auth.md), [Engine](engine.md), [Session](session.md), and [Playback coupling](playback-coupling.md) each own one of these paths. This page is the map.

## Desktop outcomes

| Trigger | State after | Reconnect count | Local mute | Next audio |
| --- | --- | --- | --- | --- |
| User stop | Idle. Receiver cleared | Unchanged, then cleared on the next start | Restored to the bit saved at mute | Stays on the PC until the next `streaming` |
| Discovery snapshot misses the playing discovered receiver, or the pair is no longer 2/2 | Idle, by `StopAsync` | Does not increment. A later auto-connect starts at 0 | Restored | A new process only if auto-connect is allowed and the row returns complete |
| Address, port, leader, capture endpoint, latency, or sample rate changes on an active session | Connecting, then Streaming on success | Cleared, because `StartLockedAsync` installs empty diagnostics | Restored, then muted again on the new `streaming` | New handshake |
| Identity promotion with the same transport signature | The existing Streaming or Standby snapshot, with the new receiver object | Unchanged | Held | Same process |
| Engine `error` with `retryable: false`, or an authentication message | Error. `LastFault` kept | Does not increment | Restored | Stays stopped even if force reconnect is on |
| Engine `error` with `retryable: true`, force reconnect off | Error. Fault and pre-fault metrics kept | Does not increment | Restored | Stays stopped. The default |
| Same error, force reconnect on, attempts left, and the user has not cancelled | Connecting with "Disconnected; reconnecting (n/m)", then a new process | Increments when the next attempt begins. A stream that lasted at least 10 seconds resets the attempt budget first | Restored, then muted again if the new attempt reaches `streaming` | Full handshake. RTP does not continue |
| No stdout event for 40 seconds before `streaming`, or 15 seconds after | Error, "The audio engine response timed out." | Follows the same retry rules as an engine error | Restored | Same |
| `hello` or process spawn failure | Error | One open was attempted | Restored if mute had been applied | Stopped |
| Pairing PIN cancelled | Idle |  | Unchanged, because `streaming` was not reached | `start` is not sent |
| Incomplete stereo `StartAsync` or `PairAsync` | The exception reaches the panel notice | No process | Unchanged | Stopped |

`IsActive` is Connecting, Streaming, Standby, or Pairing. Error and Idle are inactive, so auto-connect may fire on a later discovery result. Auto-connect itself is off unless the global setting or the receiver override enables it. A user stop suppresses it until the next manual play, or until global auto-connect is turned from off to on.

## How an engine error becomes retryable

[`Fault::from_error`](../../native/airflash-engine/src/transport.rs) classifies the error before the desktop sees it. An error that is already a `Fault` is forwarded with its own code and flag.

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

The authentication step rewrites a `protocol_error` from that classifier to `authentication_failed`. The flag stays false. `error_event`, used when the worker returns an error that is not already a `Fault`, scans the message for pairing, SRP, verification, credentials, signature, identity, or authentication. A match becomes `authentication_failed` and not retryable. Anything else becomes `engine_error` and retryable. IPC failures that `send` stamps in [Credentials and IPC](credentials-and-ipc.md) are `command_error` and not retryable.

The desktop stops the retry loop when `retryable` is false. When the field is missing, it applies the same keyword scan to the exception text. A retryable engine fault retries only while force reconnect is on.

## Inside the engine process

The media thread exits when `Health::check` returns a stored fault, when capture or scheduling returns an error, or when the cancellation flag is set. `Member::close` then sends `TEARDOWN` with a 300 ms budget if SETUP had started a session. The desktop's `stop`, or stdin closing, is what sets the flag on a user or discovery stop.

| Watch | First symptom | Session ends when |
| --- | --- | --- |
| `POST /feedback` every 2 seconds | `feedback_delayed` after 4 seconds. Audio continues. A 500–504 response emits `feedback_retry` and continues | No response by 12 seconds. Or the third 500-class response. Or 401/403 as authentication, not retryable. Or any other status as `session_rejected`, retryable only for 454 |
| Event TCP |  | Any read or parse error on channel `events` |
| Audio UDP | `media_send_delayed` on the first failed send. Audio continues | 12 seconds with no successful send |
| Sender schedule | `sender_late_recovered` after a skip of slots that were more than 60 ms late | A live stream does not end. A finite probe does |
| Capture thread |  | `GetNextPacketSize` fails after 2 seconds without a loopback packet. The string is a capture-endpoint disconnect |
| PTP bind |  | Port 319 or 320 already taken. No member is contacted |
| Handshake | `phase` events through info, authenticate, and setup | Any failed RTSP status, codec list, or authentication check. Authentication is not retryable |

A warning event does not change playback state and does not restore mute. `transport_metrics` does not either. Standby is a desktop label over the same process. It is entered when standby is enabled and capture has been under about −48 dBFS for the threshold, default 10 seconds. RTP, feedback, and PTP continue.

## What the new process does not keep

Sequence numbers, SSRCs, the RTP origin, the session UUID, the PTP clock id, the control encryption counters, and the in-process underrun counters all belong to the process that created them. The panel keeps `LastFault`, `CaptureBeforeFault`, and `TransportBeforeFault` until a later attempt reaches `streaming` or the user starts a different receiver. Live metric fields are empty during that gap.

Discovery loss and an engine fault are different counters. Discovery calls `StopAsync` and the reconnect field stays 0 on the playback that follows. An engine fault increments `ReconnectCount` only inside the force-reconnect loop, which defaults off.
