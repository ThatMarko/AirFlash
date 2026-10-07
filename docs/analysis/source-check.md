# Source check

This page keeps the claims from the removed discovery and transport investigation reports, checked against this tree and against [the workflow notes](README.md). Items under "In the source" are behavior you can read in the files named there. Items under "Not in the source" are absent, or they contradict a constant or a call that is present. Those reports also proposed patches. Those patches are not applied in this tree.

Household addresses and adapter addresses from those notes are not repeated here.

## In the source

| Claim | Where it holds |
| --- | --- |
| A discovery snapshot that lacks the playing receiver, or a stereo row that is not 2/2, calls `Session.StopAsync` when the receiver is not manual | [`AppViewModel.ReconcileDiscoveryAsync`](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs). Manual receivers skip the block because the pattern is `{ IsManual: false }` |
| `Restart` clears the record cache and publishes that empty list before the new browse returns | [`WindowsDiscovery.Restart`](../../desktop/AirFlash.App/Services/WindowsDiscovery.cs) |
| `NetworkAddressChanged` and `NetworkAvailabilityChanged` call `Restart` without inspecting which adapter changed | The same file. The handler has no adapter argument |
| The control panel drops a row that is offline or removed from `_known` | `RefreshReceivers` keeps `Online && !Hidden`. All-interfaces mode marks the entry offline. A selected NIC removes it. Settings still shows offline history only in all-interfaces mode. Saved options stay in `config.json` either way |
| `StopAsync` cancels the attempt, and that cancellation returns before `ReconnectCount` increments. The next `StartLockedAsync` installs empty diagnostics | [`SessionController`](../../desktop/AirFlash.Core/SessionController.cs). A later auto-connect is a new playback, so its reconnect count starts at 0 |
| Auto-connect is what starts playback again after that stop, and only when the setting allows it | [After discovery](after-discovery.md). The default is off |
| Mute-while-streaming stores the previous mute bit and restore writes it back. Stop, failure, and discovery stop all restore | [`AudioService`](../../desktop/AirFlash.App/Services/AudioService.cs) and the session `finally` / `StopLockedAsync` |
| Expiry removes `_records` and `_instances` and does not remove `_pending`. A later `Resolve` for that name returns while `_pending` still contains it | [`WindowsDiscovery.Refresh`](../../desktop/AirFlash.App/Services/WindowsDiscovery.cs) and `Resolve`. Recovery is a browse removal plus a new PTR, or `Restart` |
| `Discovery resolved:` is logged when the name is missing or `Describe` changed. Identical repeats are cache misses | `Resolved`. The 2026-10-06 workspace log repeats all 8 descriptions. See [Playback coupling](playback-coupling.md) |
| Volume and feedback share one mutex, and volume calls `GET /info` about once a second on every member | [`volume.rs`](../../native/airflash-engine/src/volume.rs), [`transport.rs`](../../native/airflash-engine/src/transport.rs) |
| PTP binds `0.0.0.0:319` and `0.0.0.0:320`, sends sync every 125 ms, and announces about once a second, only to session peers | [`clock.rs`](../../native/airflash-engine/src/clock.rs). The engine README calls this an AirPlay unicast master, not a full best-master election |
| The capture queue is 20 ms target and 60 ms capacity. An empty pull zero-fills and increments `underrun_packets` once per packet | [`live.rs`](../../native/airflash-engine/src/live.rs). At 44100 Hz, 11614 such packets are about 92.7 seconds of empty-queue packets |
| The resampler ratio is clamped to ±500 PPM from local queue depth | The same file. It does not read a receiver clock |
| Credentials are DPAPI blobs with magic `W2AP\x01`, written via a temp file and `MoveFileExW` with replace and write-through | [`credentials.rs`](../../native/airflash-engine/src/credentials.rs) |
| JSONL is UTF-8 without a BOM, one writer at a time, 64 KiB maximum, and the process is killed two seconds after stdin closes | [`EngineConnection.cs`](../../desktop/AirFlash.Core/EngineConnection.cs), [`main.rs`](../../native/airflash-engine/src/main.rs). An overlong line is `command_error` on channel `ipc`, not a separate `line_too_long` code |

## Not in the source

| Claim | What the tree does instead |
| --- | --- |
| `Discovery resolved:` is logged only when the cache lacks the name | It is also logged when `Describe` changes. The prose in the disconnect note says "only". The snippet above that sentence includes the `Describe` comparison |
| Normal latency is 2000 ms, and 150 ms is a mode with a stated Wi-Fi margin | Modes are 120, 200, 500, and custom 0–2000. Normal is the default and is 200 ms. There is no 150 ms preset and no margin table in the app. [`scripts/e2e_stream.py`](../../scripts/e2e_stream.py) defaults its finite probe to `--latency-ms 150`. That flag is not a settings mode |
| Loopback capture lives in `wasapi.rs`, uses `eMultimedia`, and still needs to be switched to event callbacks | Loopback is `live.rs`, `eRender` / `eConsole`, and already sets `AUDCLNT_STREAMFLAGS_EVENTCALLBACK`. `wasapi.rs` records a microphone for probes |
| The frame queue is lock-free | It is `Mutex<VecDeque<Frame>>` |
| PTP timestamps come from `QueryPerformanceCounter` | [`Clock`](../../native/airflash-engine/src/clock.rs) uses `SystemTime` plus `Instant`. QPC is the capture timestamp and the standby comparison |
| Feedback and retransmission are one UDP RTCP flow to port 7000 | `POST /feedback` is RTSP on the control TCP connection. Retransmit and timing sync are UDP to the receiver's `controlPort`. Audio RTP goes to `dataPort`. Port 7000 is the usual RTSP port |
| `MAX_PENDING = 512` is a 4-second audio ring, and `LATE_LIMIT` means the HomePod playout deadline | `MAX_HISTORY` in `rtp.rs` is the 512-packet audio history. `MAX_PENDING` caps queued retransmit requests. `LATE_LIMIT` compares the sender's `Instant` with its own packet deadline |
| The engine already needs `timeBeginPeriod(1)` because that call is missing and the packet period is shorter than 15.6 ms | The call is absent. The media thread uses `thread::sleep` and MMCSS "Pro Audio". The tree does not measure the timer quantum |
| HomePod OS 27 changes jitter tolerance, rejects a latency, or requires 150 ms | No OS version is branched on. `sourceVersion` is logged. The README states support for HomePod OS 27 as this one HAP, PTP, and encrypted-RTP path |
| A Tailscale or APIPA event is identified in code, or PTP to a LAN address follows the default route | The code restarts on every `NetworkAddressChanged` and binds PTP to `0.0.0.0`. `send_to` uses the peer IP. Which route Windows picks is not encoded |
| Wi-Fi background scans and `wlan_intf_opcode_media_streaming_mode` are part of the session | The tree does not call `wlanapi.dll` |
| DPAPI replace cannot be corrupted by power loss | The destination is replaced by `MoveFileExW` after `sync_all` on the temp file. A crash before that move leaves the previous file. The source does not claim a power-loss proof |
| `GET /info` on this HomePod takes 50–200 ms and that delay is what raises `feedback_delayed` | The lock can delay feedback for the length of the `/info` round trip. The only thresholds in the source are 4 seconds for the warning and 12 seconds for failure |
| Underrun packets prove the network was healthy | They count local packets that contained silence fill. Transport health is a separate set of counters |

## How to read a gap report against this tree

A playback that ends with reconnects at 0, after the receiver vanishes and returns, matches `StopAsync` plus a later `StartAsync`. The engine watches did not fail that process. Whether the empty snapshot came from `Restart` or from a single expired name depends on whether every speaker was logged together. The 2026-10-06 log shows the all-at-once cache-miss pattern. It does not print the network event.

A large `underrun_packets` value with `dropped_frames` at 0 matches silence fill from an idle loopback endpoint. Divide packet count by the stream rate using 352 frames per packet before treating it as a transport fault.

A `feedback_delayed` warning with the session still in Streaming matches the 4-second soft threshold. The process ends on that channel only at 12 seconds, or on the status rules in [Engine](engine.md).
