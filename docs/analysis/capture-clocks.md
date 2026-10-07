# Capture and clocks

Live loopback capture is in [live.rs](../../native/airflash-engine/src/live.rs), and packet pacing is in [session.rs](../../native/airflash-engine/src/session.rs). [wasapi.rs, lines 93–101 and 143–149](../../native/airflash-engine/src/wasapi.rs#L93-L149) instead opens a default `eCapture` / `eConsole` microphone when `record_mic_path` is supplied. That option is normally used for finite qualification; the session code does not restrict it to finite probes ([session.rs, lines 515–520](../../native/airflash-engine/src/session.rs#L515-L520)).

## Loopback

`Loopback::start` creates the `wasapi-loopback` worker, waits up to three seconds for its initialization result, and requests cancellation followed by a join if initialization does not succeed. The receive timeout is not a hard bound on that join. Capture initializes COM in MTA and makes a best-effort MMCSS registration for `Pro Audio`; an MMCSS registration failure is ignored. An empty capture id uses `GetDefaultAudioEndpoint(eRender, eConsole)`. A non-empty id matches an active render device id or friendly name exactly. ([live.rs, lines 74–110 and 209–252](../../native/airflash-engine/src/live.rs#L74-L110); [wasapi.rs, lines 24–51](../../native/airflash-engine/src/wasapi.rs#L24-L51).)

The stream is shared-mode with `AUDCLNT_STREAMFLAGS_LOOPBACK`, `EVENTCALLBACK`, and `NOPERSIST`. The code prefers `IAudioClient3` and its default shared engine period. The fallback `Initialize` duration is `200_000` in 100-nanosecond units, which is 20 ms. Any event wait result other than `WAIT_OBJECT_0` takes the no-event path. After two seconds without a packet, that path calls `GetNextPacketSize`; a failing call becomes `capture endpoint disconnected`, but a successful call does not itself drain returned packets until an event arrives. Silence can therefore leave the sender zero-filling without being treated as a capture failure. ([live.rs, lines 266–332](../../native/airflash-engine/src/live.rs#L266-L332).)

Accepted mix formats are float32 or 16-, 24-, or 32-bit integer, 1–8 channels, and 8–192 kHz. Only the first two channels are retained; mono is copied into both. Non-finite input samples become zero, and a silent or null-data buffer supplies zeros. Normal WASAPI QPC positions are converted from 100-nanosecond units to nanoseconds. A timestamp-error flag substitutes current QPC minus buffer duration for the buffer start, then derives frame timestamps from the input rate. A data-discontinuity flag clears the resampler input samples and timestamps and resets the resampler; it does not clear already queued output frames. ([live.rs, lines 293–316 and 333–413](../../native/airflash-engine/src/live.rs#L293-L413).)

## The queue and its measurements

Output `VecDeque<Frame>` is inside `State`, shared as `Arc<Mutex<State>>`; this is not a lock-free queue. The media packet pull holds the same mutex while it reads frames, applies the equalizer, and converts PCM. Capture also takes this mutex while inserting output. At the selected stream rate:

| Rate | Target (`rate * 20 / 1000`) | Capacity (`rate * 60 / 1000`) |
| --- | --- | --- |
| 44100 | 882 frames, 20 ms | 2646 frames, 60 ms |
| 48000 | 960 frames, 20 ms | 2880 frames, 60 ms |

`push_frame` drops the oldest output frame when capacity is reached and increments `dropped_frames`. The resampler consumes chunks of `input_rate / 100` frames, with base output/input ratio `stream_rate / input_rate` and the relative correction:

```text
1 + clamp((target - queued) / stream_rate * 0.01, -0.0005, 0.0005)
```

The clamp is ±500 PPM. This is local queue-depth control against the 20 ms target; it reads no receiver clock, feedback timestamp, or DAC error. Resampled frame timestamps are derived from the input chunk start minus `resampler.output_delay()`, plus frame offsets at the stream rate. They are estimates for local age reporting, not observations of receiver playback. ([live.rs, lines 49–84 and 390–413](../../native/airflash-engine/src/live.rs#L49-L84).)

`packet` pulls 352 frames for one RTP packet. Each missing frame is replaced with zero PCM input stamped with the current QPC time. If **any** frame is missing, `underrun_packets` increments once. It counts packets containing fill, not missing samples, network loss, or exclusively fully silent packets. The equalizer runs after this substitution, so its stored filter history can still produce a tail from zero input. A wholly empty pull with a bypassed equalizer is a silent packet. ([live.rs, lines 120–159](../../native/airflash-engine/src/live.rs#L120-L159); [equalizer.rs, lines 160–178](../../native/airflash-engine/src/equalizer.rs#L160-L178).)

A 352-frame packet spans about 7.982 ms at 44100 Hz or 7.333 ms at 48000 Hz. Only when the queue is empty for every frame in N packets does `N * 352 / rate` measure the amount of inserted silence. For partial underruns that formula overstates missing-frame duration. `dropped_frames == 0` means neither capacity eviction nor scheduler stale-frame removal was recorded; it does not prove transport health. Transport health is reported separately by the feedback, event, and media watches in [Engine](engine.md).

Samples whose largest absolute channel value exceeds `0.004` (about −48 dBFS), measured before equalizer and master gain, set `last_audio_qpc_ns` and the onset marker. The desktop converts `Stopwatch.GetTimestamp()` into nanoseconds and compares it with that value for standby; if no audio marker exists, it uses time since `streaming`. Standby changes UI state while the native send loop continues. See [Session](session.md). ([live.rs, lines 128–153](../../native/airflash-engine/src/live.rs#L128-L153); [SessionController.cs, lines 360–366](../../desktop/AirFlash.Core/SessionController.cs#L360-L366).)

`max_queue_age_ms` is a lifetime maximum, sampled at packet pull. `capture_to_send_p95_ms` sorts the most recent 4000 **frame** ages and selects index `len * 95 / 100`; filled frames count with age zero. This is age at PCM extraction, before packetization and socket send, rather than end-to-end or receiver latency. After late-schedule recovery, `discard_stale` removes frames older than 20 ms or above target depth and includes those removals in `dropped_frames`. ([live.rs, lines 145–176](../../native/airflash-engine/src/live.rs#L145-L176).)

## Three clocks

| Clock | Source | Used for |
| --- | --- | --- |
| Capture time | WASAPI QPC timestamps and `qpc_ns()` | Frame ages, onset markers, and `last_audio_qpc_ns` |
| Media schedule | `Instant` at send-loop initialization, advancing by 352 frames per packet | Sender deadlines and the 60 ms lateness check |
| AirPlay clock | `SystemTime` epoch at construction plus `Instant` elapsed | PTP packets, NTP responses, and RTP timing announcements |

`Clock::now_ns` does not call `qpc_ns` or read fresh wall time. Subsequent wall-clock changes do not reset its epoch. The media schedule and resampler do not consume PTP delay estimates or receiver clock measurements. This tree does not measure or correct PC-versus-receiver drift. ([clock.rs, lines 14–36](../../native/airflash-engine/src/clock.rs#L14-L36); [transport.rs, lines 605–645](../../native/airflash-engine/src/transport.rs#L605-L645).)

The shared `Clock` feeds an ephemeral wildcard-bound NTP socket started on each streaming/probe worker attempt. It replies only to a 32-byte datagram from a session peer whose second byte masked with `0x7f` is `0x52`, with a packet starting `0x80 0xd3`. Desktop playback always advertises PTP and does not provide this NTP port to receivers. The socket is dropped when its worker attempt exits, even if the engine process remains alive. See [Control and mute](control-and-mute.md) for PTP sockets and [Engine](engine.md) for timing packet layout. ([session.rs, lines 154–195, 296–308, and 523–530](../../native/airflash-engine/src/session.rs#L154-L195).)

RTP timing announcements are scheduled every 100 ms on the control UDP socket. With PTP, byte 1 is `0xd7` (the marked payload-type byte), and the packet carries AirPlay nanoseconds and the clock id. Its first RTP field is the current timestamp, while its second RTP field is that timestamp minus **requested** latency in samples. Normal packetization advances RTP by 352, and schedule recovery additionally advances it across skipped packet slots. A receiver `latencyMin` that is positive and at most ten seconds of audio changes local retransmission deadlines/retention and the `receiver_latency_ms` metric. It does not change the requested-latency value used in sync announcements, and none of these fields measures acoustic latency. ([session.rs, lines 379–389 and 579–629](../../native/airflash-engine/src/session.rs#L379-L389); [rtp.rs, lines 69–79 and 153–185](../../native/airflash-engine/src/rtp.rs#L153-L185); [transport.rs, lines 446–488](../../native/airflash-engine/src/transport.rs#L446-L488).)

## Scheduling and retransmission

Loopback starts after all members complete `RECORD`/`FLUSH`. The sender then waits for the 20 ms target for at most 200 ms, allowing silent endpoints to begin streaming with fill. `MediaSchedule` starts afterward, just before feedback/volume workers and the `streaming` event; that event precedes the first packet send. ([session.rs, lines 555–595](../../native/airflash-engine/src/session.rs#L555-L595).)

`LATE_LIMIT` is 60 ms on the sender's own deadline. At lateness **greater than or equal to** 60 ms, an unbounded live stream skips missed slots, advances member sequence numbers and RTP timestamps, trims stale capture frames, and emits `sender_late_recovered`. A finite probe returns `qualification_late` instead. This does not measure HomePod DAC deadlines. Waiting uses `thread::sleep` in slices of at most 2 ms. Capture and the media worker each request MMCSS `Pro Audio` best-effort; the source has no `timeBeginPeriod` or `NtSetTimerResolution` call, and does not establish the actual Windows timer quantum. ([transport.rs, lines 18–23 and 619–645](../../native/airflash-engine/src/transport.rs#L619-L645); [session.rs, lines 521–522 and 605–621](../../native/airflash-engine/src/session.rs#L605-L621).)

Desktop modes request realtime 120 ms, normal 200 ms, buffered 500 ms, or custom 0–2000 ms. Normal is the default; the stored default custom value is 1000 ms and applies only in custom mode. There is no desktop 150 ms preset, although packetizer/transport constructors have 150 ms initial values that session setup overwrites, and finite harnesses may request that value. ([Settings.cs, lines 55–58 and 81–85](../../desktop/AirFlash.Core/Settings.cs#L55-L85); [rtp.rs, lines 47–57](../../native/airflash-engine/src/rtp.rs#L47-L57); [transport.rs, lines 440–449](../../native/airflash-engine/src/transport.rs#L440-L449).)

Retransmission history has both a 512-packet cap (`MAX_HISTORY`) and an age threshold `max(1 second, playout latency + 250 ms)`. Insertion evicts on either condition; this is not a guarantee to retain at least one second when the cap is reached. At 44100 Hz the count cap represents about 4.09 seconds. A cached packet becomes unusable at its local playout deadline even if its bytes remain in history. `MAX_PENDING = 512` separately limits pending packet resends derived from requests, not audio history or the number of request datagrams. Audio RTP uses SETUP's `dataPort`; timing and retransmission use its UDP `controlPort`, while `POST /feedback` uses RTSP TCP. ([rtp.rs, lines 8–18 and 69–140](../../native/airflash-engine/src/rtp.rs#L69-L140); [transport.rs, lines 543–591](../../native/airflash-engine/src/transport.rs#L543-L591); [session.rs, lines 373–378](../../native/airflash-engine/src/session.rs#L373-L378).)

## Verification coverage

The source includes queue-fill/resume, overflow/stale-frame removal, and equalizer-before-gain tests in [live.rs, lines 420–490](../../native/airflash-engine/src/live.rs#L420-L490); 48 kHz schedule tests in [transport.rs, lines 648–678](../../native/airflash-engine/src/transport.rs#L648-L678); and two-member scheduler recovery/nonce tests in [continuity.rs, lines 299–330](../../native/airflash-engine/tests/continuity.rs#L299-L330). [clock.rs, lines 189–201](../../native/airflash-engine/src/clock.rs#L189-L201) tests PTP wire layout, not network accuracy. [SessionTests.cs, lines 211–217](../../desktop/AirFlash.Tests/SessionTests.cs#L211-L217) uses a fake engine to check standby without restart. These tests do not establish hardware endpoint behavior, timer quantum, remote DAC drift, or acoustic latency.
