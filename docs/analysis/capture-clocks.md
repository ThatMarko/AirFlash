# Capture and clocks

Live audio is captured in [`live.rs`](../../native/airflash-engine/src/live.rs) and paced in [`session.rs`](../../native/airflash-engine/src/session.rs). [`wasapi.rs`](../../native/airflash-engine/src/wasapi.rs) is the microphone recorder used when a finite probe sets `record_mic_path`. It opens `eCapture` / `eConsole`. It is not the loopback path.

## Loopback

The capture thread is `wasapi-loopback`. It initializes COM, requests the MMCSS profile named "Pro Audio", and opens a render endpoint for loopback. An empty capture id uses `GetDefaultAudioEndpoint(eRender, eConsole)`. A non-empty id matches the device id or the friendly name.

The stream is shared-mode with `AUDCLNT_STREAMFLAGS_LOOPBACK`, `EVENTCALLBACK`, and `NOPERSIST`. The code prefers `IAudioClient3` and the device's shared engine period. The fallback `Initialize` duration is `200_000` in 100-nanosecond units, which is 20 ms. The client is already event-driven. A wait of 50 ms with no event is treated as a silent render endpoint. After two seconds without a packet, `GetNextPacketSize` is called and a failure becomes "capture endpoint disconnected."

Accepted mix formats are float32 and 16-, 24-, or 32-bit integer, 1–8 channels, 8–192 kHz. The first two channels are kept. A mono mix is copied into both. Samples that are not finite become zero. A buffer flagged silent is stored as zeros. A timestamp-error flag synthesizes each frame's QPC as the current counter minus the buffer's duration.

## The queue

The queue is `Mutex<VecDeque<Frame>>`. At the stream rate:

| Rate | Target (`rate * 20 / 1000`) | Capacity (`rate * 60 / 1000`) |
| --- | --- | --- |
| 44100 | 882 frames, 20 ms | 2646 frames, 60 ms |
| 48000 | 960 frames, 20 ms | 2880 frames, 60 ms |

`push_frame` drops the oldest frame when the queue is at capacity and increments `dropped_frames`. The resampler then runs on chunks of `rate / 100` input frames. Its relative ratio is

```text
1 + clamp((target - queued) / stream_rate * 0.01, -0.0005, 0.0005)
```

The clamp is ±500 PPM. The error signal is the local queue depth against the local 20 ms target. The ratio does not read a HomePod clock, a feedback timestamp, or a DAC error. A discontinuity flag from WASAPI clears the input queues and calls `resampler.reset()`.

`packet` pulls 352 frames for one RTP packet. Each missing frame is replaced with silence at the current QPC time. If any of the 352 frames was missing, `underrun_packets` increments by one. The counter is packets, not samples. A full underrun packet is `352 / rate` seconds: about 7.982 ms at 44100 Hz and 7.333 ms at 48000 Hz.

A run of N such packets, with the queue empty for the whole run and the stream at 44100 Hz, covers `N * 352 / 44100` seconds. For 11614 packets that is about 92.7 seconds. The metric supports that reading when capture is producing no frames, which is what a silent render endpoint does. It does not say the network path failed or succeeded. `dropped_frames` staying at 0 means the 60 ms cap was not hit. Transport health is the feedback, event, and media watches in [Engine](engine.md).

Samples louder than `0.004` (about −48 dBFS) set `last_audio_qpc_ns`. The desktop standby timer subtracts that QPC value from `Stopwatch.GetTimestamp`, which on Windows is the same counter, and compares the gap with the standby threshold. Standby changes the UI state. The send loop continues. See [Session](session.md).

After a late-schedule recovery, `discard_stale` drops frames older than 20 ms or above the target depth, and those drops also increment `dropped_frames`.

## Three clocks

| Clock | Source | Used for |
| --- | --- | --- |
| Capture time | `QueryPerformanceCounter` in `qpc_ns` | Frame timestamps, `last_audio_qpc_ns`, capture-to-send age |
| Media schedule | `Instant` at `streaming`, plus 352 frames per packet | When the next RTP packet is due, and the 60 ms lateness check |
| AirPlay clock | `SystemTime` at construction plus `Instant` elapsed, in [`clock.rs`](../../native/airflash-engine/src/clock.rs) | PTP sync, follow-up, announce, and the RTP timing packet |

PTP timestamps are not read from QPC. The media thread does not slew the schedule from PTP. The resampler does not slew from the HomePod. Drift between this PC and the speaker is not measured or corrected in this tree.

The same `Clock` feeds an ephemeral UDP socket that starts on every attempt. It replies only to a 32-byte datagram from a session peer whose timing type is `0x52`, with a packet that starts `0x80 0xd3`. Desktop playback advertises PTP in SETUP, so that socket is not the timing path the HomePod is told to use. The byte layout is in [Engine](engine.md).

The RTP timing packet is sent every 100 ms on the control UDP socket. With PTP it carries the AirPlay clock in nanoseconds and the clock id. The payload type is `0xd7`. The second RTP field is the current timestamp minus the requested latency in samples. The packetizer's own timestamp only advances by 352 frames per packet. `latencyMin` returned by the HomePod, when it is positive and at most ten seconds of audio, changes the local playout offset stored for retransmission retention and for the "receiver latency" metric. The sync packet still uses the latency the desktop requested.

## Scheduling

`LATE_LIMIT` is 60 ms on the sender's own deadline. If `Instant::now()` is more than 60 ms past the time the next packet should have been sent, a live stream skips the missed slots, advances sequence numbers and timestamps, and keeps going. That path emits `sender_late_recovered`. It is not a test of when the packet will reach the HomePod DAC. A finite probe returns an error instead of skipping.

The wait is `thread::sleep` in slices of at most 2 ms. Nothing in the engine calls `timeBeginPeriod` or `NtSetTimerResolution`. Both the capture thread and the media thread request MMCSS "Pro Audio". That raises scheduling priority for those threads. It is not a 1 ms timer-resolution request.

There is no 150 ms latency mode. The desktop modes are realtime 120 ms, normal 200 ms, buffered 500 ms, and custom 0–2000 ms. Normal is 200 ms, not 2000 ms. The default mode is normal. The default custom value stored in settings is 1000 ms, and it is used only when the mode is custom.

History for retransmission holds up to 512 packets (`MAX_HISTORY` in [`rtp.rs`](../../native/airflash-engine/src/rtp.rs)), and at least one second or the playout latency plus 250 ms, whichever is longer. At 44100 Hz, 512 packets is about 4.09 seconds. `MAX_PENDING` in [`transport.rs`](../../native/airflash-engine/src/transport.rs) is a different limit: at most 512 retransmit requests waiting to be served. Audio RTP goes to the `dataPort` from SETUP. RTSP stays on the TCP port, usually 7000. Feedback is a TCP `POST /feedback`, not an RTCP packet. Retransmit requests and the timing sync use the UDP control port.
