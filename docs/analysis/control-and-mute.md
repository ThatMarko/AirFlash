# Control traffic and local mute

Three independent loops share the work of a live session: local mute in the WPF process, device volume and RTSP feedback in the engine, and PTP on its own sockets. None of them is driven by the discovery browse.

## Local mute

[`AudioService`](../../desktop/AirFlash.App/Services/AudioService.cs) runs endpoint calls on one MTA thread. `MuteAsync` opens the default console render endpoint, or the endpoint id when capture mode is `endpoint`. If that device id is already in `_originalMute`, it returns. Otherwise it stores `AudioEndpointVolume.Mute` and sets mute to true.

`Restore` writes each stored bit back and removes the entry. A COM failure leaves the entry so a later restore can retry. Dispose of the audio service restores before it unregisters endpoint notifications.

[`SessionController`](../../desktop/AirFlash.Core/SessionController.cs) calls mute when the engine emits `streaming` and `MuteWhileStreaming` is true. The default is true. Restore runs from:

- the stream task's `finally`, which covers cancellation, completion, and errors that leave the task
- `StopLockedAsync`, after that task has finished
- `UpdateSettingsAsync`, when the setting is turned off during Streaming or Standby
- turning the setting on during Streaming or Standby, which mutes again instead of restoring

A discovery stop, a user stop, and an engine failure all restore. There is no grace period and no separate path for auto-connect. If the saved bit was unmuted, the PC endpoint plays as soon as restore runs. The next `streaming` event mutes it again. [Playback coupling](playback-coupling.md) is the discovery case of this same restore.

Endpoint enumeration is separate from mute. `GetEndpointsAsync` lists active render devices. `OnDefaultDeviceChanged` for console render, and the other `IMMNotificationClient` callbacks, raise `EndpointsChanged`. The view model debounces those for 200 ms. A default-device change while capture mode is loopback starts the current receiver's session again. That path is in [Session](session.md).

## One RTSP connection, two workers

Each HomePod member has one encrypted control connection behind `Arc<Mutex<Connection>>`. Two threads take that mutex:

| Thread | When it runs | Request |
| --- | --- | --- |
| `airplay-feedback` | Every 2 seconds, and not while a previous attempt is still inside the 12-second failure budget | `POST /feedback` with an empty body |
| `airplay-volume` | Immediately when the volume sequence changes, and otherwise about once a second | `SET_PARAMETER` `volume:` on change, then `GET /info` on every pass |

`GET /info` parses `initialVolume` from the binary plist. The lock is held for the whole round trip, including the read of the response. Feedback's lock is held the same way. A slow `/info` delays the feedback write that is waiting, and a feedback read delays the next volume poll. The engine does not measure the HomePod's time inside `/info`. It records feedback RTT from `begin_request` until a matching response, and it raises `feedback_delayed` when that exceeds 4 seconds. The failure at 12 seconds is unchanged. Timeouts and status codes are in [Engine](engine.md).

The volume pass walks every member. The event's `host` and `volume` are the first peer, which the desktop ordered as the leader. `available` is true only when every member returned a volume. The desktop shows that event only for the leader host.

`Pending::status` turns those readings into one word:

| Condition | Status |
| --- | --- |
| A new command was written, every member reads that percent, and the write did not fail | `confirmed`, and the command is marked settled |
| The write succeeded and fewer than 3 seconds have passed without that match | `pending` |
| The write failed, or 3 seconds passed without a match | `unconfirmed`, then settled |
| No command is outstanding and every member answered | `confirmed` |
| Any member's `GET /info` failed | `unsynced` |

A settled command stays settled. Later polls report `confirmed` or `unsynced` from the reads alone. The desktop treats a missing confirmation within its own 3-second timer as unconfirmed as well. A new process clears the desktop volume state.

The event-channel thread uses a different TCP socket. It does not take the control mutex. Media UDP does not take it either.

## PTP sockets

[`PtpMaster::start`](../../native/airflash-engine/src/clock.rs) binds `0.0.0.0:319` and `0.0.0.0:320`. If either bind fails, no member is contacted. The clock id is random and masked to 63 bits. The engine README describes this as an AirPlay unicast master, not a general IEEE 1588 daemon and not a full best-master-clock election.

Every 125 ms the thread sends a sync on UDP 319 and a follow-up on UDP 320 to each session peer. When a second has passed, it also sends an announce on 320. Delay requests are read from both sockets. A packet is ignored unless its source address is one of the session peers, it is at least 34 bytes, and the version nibble is 2. Replies go back to that peer on 319 or 320. The announce comment in the source says the priority fields express a local-oscillator preference.

`send_to` uses the peer IP. Windows chooses the egress interface from the routing table for that destination. The discovery adapter id is not passed into `bind` or `send_to`. A more-specific route for the HomePod's subnet stays the route for that IP. The wildcard bind does accept a delay request that arrives on any local address, and the filter then keeps only the session peers.

An NTP socket is also bound on `0.0.0.0` and an ephemeral port for the life of the process. It answers only a 32-byte datagram from a session peer whose second byte, masked with `0x7f`, is `0x52`, and the reply starts `0x80 0xd3`. Production SETUP advertises PTP, so the HomePod is not given this port. The desktop start command does not select NTP.

The control TCP connection, and the UDP sockets bound to its local address, are a separate choice. `TcpStream::connect` does not bind the discovery NIC. The local address Windows selected becomes the address passed in `timingPeerInfo` and used for audio and control UDP. See [Engine](engine.md).
