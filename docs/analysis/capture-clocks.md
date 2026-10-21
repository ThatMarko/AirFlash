# Capture and clocks

## Current-source boundary — 2026-10-21

Current stable `439de626` supports application capture/standby as well as endpoint loopback/recording. Combined `6c1e2ef` adds optional padded output-rate frames and wholly padded pulls before EQ, once per shared pull, while excluding captured and scheduled fade/recovery silence. Attempt resets and pre-fault snapshots remain distinct. Current native execution provenance is unchanged-tree evidence from `6eabb186`, not a fresh native run at `6c1e2ef`.

[Current source, acceptance and executed limits](../issues/PR-READINESS-2026-10-21.md) supersede older current/open/schema labels below. The remaining source links and test counts retain their recorded historical baselines; this documentation refresh claims no new test execution or upstream publication.

Source reviewed on 2026-10-12 at own-fork `main` `06237873dcb6394eb35e97a017c536843bd7743d`, containing original stable `cf420d97700463c56adc31408510e8336e99e51c`. This page describes that integrated fork source, not an unmerged feature checkout or original upstream runtime.

Current native capture supports both playback loopback and recording inputs, followed by one shared resampler, output queue and DSP path. Group H's added padding counters are integrated into fork main. They measure missing local output-rate frame slots, never network loss or sound reaching a receiver.

## Loopback

[Loopback::start](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/live.rs#L78) creates `wasapi-capture`, waits up to three seconds for initialization, and cancels/joins on failure. The receive timeout is not a hard join deadline. Capture initializes COM MTA and best-effort MMCSS `Pro Audio`.

[find_device](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/live.rs#L241) defaults to console Render for an empty selector. A supplied id searches active devices of both directions and wins over friendly names; a friendly name must be unique. [capture_flags](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/live.rs#L231) adds LOOPBACK only for Render; Capture uses EVENTCALLBACK/NOPERSIST without LOOPBACK. This is original stable's recording-support change, not Group H.

Shared-mode initialization prefers IAudioClient3's default engine period, otherwise a 20 ms duration. Mix format accepts float32 or 16/24/32-bit integer, 1–8 channels and 8–192 kHz. Only two channels remain; mono is duplicated; nonfinite input becomes zero. Silent/null-data packets provide genuine captured zero samples.

WASAPI timestamps normally convert 100 ns units to nanoseconds. Timestamp-error uses current QPC minus buffer duration. Data discontinuity clears input buffers/times and resets the resampler, but does not clear output already queued. After no event for over two seconds, a failing `GetNextPacketSize` becomes a capture-disconnected error; a successful probe does not drain packets until an event. Lack of packets alone permits padding and does not establish endpoint failure. [capture](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/live.rs#L271) owns initialization and this loop.

## The queue and its measurements

Output `VecDeque<Frame>`, metrics and DSP share `Arc<Mutex<State>>`. Packet pull holds the same blocking mutex while extracting/processing PCM; capture takes it for insertion.

| Stream rate | Target | Capacity |
| --- | --- | --- |
| 44,100 Hz | 882 frames / 20 ms | 2,646 frames / 60 ms |
| 48,000 Hz | 960 frames / 20 ms | 2,880 frames / 60 ms |

Overflow drops the oldest output and increments `dropped_frames`. Resampling uses input-rate/100 chunks, a 64-tap Blackman-Harris sinc and relative ratio correction:

```text
1 + clamp((target - queued) / stream_rate * 0.01, -0.0005, 0.0005)
```

The ±500 PPM correction reads local queue depth, not receiver feedback or DAC clock error. Frame timestamps estimate local age from input chunk start minus resampler delay.

[packet](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/live.rs#L124) extracts 352 stereo output frames once per shared pull before member fan-out. Every absent slot gets zero input stamped with current QPC, then EQ/gain/clamp/S16 conversion runs.

| Counter | Meaning |
| --- | --- |
| `underrun_packets` | One increment when any slot is absent; existing semantics preserved. |
| `padded_frames` | Saturating UInt64 sum of absent output-rate slots, before EQ. |
| `fully_padded_packets` | Saturating UInt64 count of pulls with all 352 slots absent, irrespective of final DSP output. |
| `capture_frames` | Input-rate WASAPI frames, including genuine captured silence. |
| `dropped_frames` | Capacity evictions plus schedule-recovery stale-frame removal. |

A partial pull with 351 captured slots adds one padded frame and one underrun, but no wholly padded pull; an empty pull adds 352/one/one. Captured zero samples and master gain zero are not padding. An EQ tail can produce nonzero output from wholly padded input. Reading metrics does not increment counts. One shared pull counts once even for two speakers. Only the new counters saturate; the existing counters retain their original arithmetic.

352 frames span approximately 7.982 ms at 44,100 Hz or 7.333 ms at 48,000 Hz. `padded_frames / stream_rate` represents substituted local frame duration when that stream rate is known; underrun-packet duration is not missing-frame duration for partial fills. No fill percentage is derived from input `capture_frames`.

Raw sample amplitude over 0.004, before EQ/master gain, updates `last_audio_qpc_ns` and onset. Standby compares it with desktop QPC or elapsed time since streaming; standby labels the continuing worker. Output muting alone does not count an audible source as silence.

`max_queue_age_ms` is a lifetime maximum at extraction. P95 sorts the latest 4,000 frame ages, including zero-age fill, and chooses index `len * 95 / 100`. It precedes packetization/socket send and is not end-to-end latency. [discard_stale](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/live.rs#L170) trims over-target or older-than-20-ms queued frames after schedule recovery.

## Three clocks

| Clock | Source | Consumers |
| --- | --- | --- |
| Capture | WASAPI QPC / qpc_ns | Frame age, onset and silence marker |
| Send schedule | Monotonic Instant, 352 frames per packet | Local deadlines and 60 ms lateness recovery |
| AirPlay | SystemTime epoch once plus monotonic elapsed | PTP, NTP response and RTP sync fields |

[Clock](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/clock.rs#L14) does not reread wall time or apply receiver offset. The resampler/schedule consume no PTP delay estimate; this source does not measure or correct PC-versus-receiver DAC drift.

Each playback/probe worker starts an ephemeral wildcard IPv4 NTP responder, restricted to 32-byte peer requests with masked type 0x52. Persistent Pair starts neither NTP nor PTP. Desktop PTP playback does not advertise that port. PTP uses wildcard UDP 319/320; selected discovery adapter is not a streaming socket bind policy. Details are in [Control and mute](control-and-mute.md).

## Scheduling and retransmission

[probe_with_controls](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/session.rs#L522) completes member RECORD/FLUSH before capture. It waits at most 200 ms for target fill, then starts MediaSchedule, feedback/volume and `streaming`. That event precedes first packet send and does not prove audibility.

At local lateness at least 60 ms, live playback skips expired slots, advances sequence/RTP timestamps without nonce reuse, trims capture and emits `sender_late_recovered`; finite probes fail qualification. Waiting sleeps in slices at most two ms. Best-effort MMCSS does not establish the system timer quantum; no timer-resolution API call is present.

Desktop latency modes remain realtime 120, normal 200, buffered 500, custom 0–2,000 ms. The default custom value is 1,000 ms but applies only in custom mode. There is no desktop 150 ms preset.

RTP sync every 100 ms carries the current timestamp and timestamp minus **requested** latency for PTP. Valid receiver latency (positive, at most ten seconds) changes retransmit/playout deadlines and retention; it does not replace requested sync latency or measure acoustic delay. [sync layout](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/rtp.rs#L153) and [latency adoption](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/transport.rs#L446) own those contracts.

History has both a 512-packet cap and age threshold max(one second, latency+250 ms); the cap prevents a minimum-retention guarantee. A packet can expire at its local playout deadline while bytes remain cached. Pending resends separately cap at 512. Audio uses dataPort; sync/retransmits use controlPort; feedback remains TCP.

## Desktop consumption and reset

[capture_metrics handling](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/SessionController.cs#L531) parses new counters independently as optional UInt64, accepting the full range. Missing, negative, fractional, overflow or wrong-type values display unavailable while other metrics remain usable. [Monitor properties](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L61) and [localized counters](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Ui/SettingsWindow.xaml#L239) show them; the existing diagnostics serializer includes current and pre-fault snapshots.

New native capture workers begin at zero. A desktop retry clears live values and preserves immutable `CaptureBeforeFault`; failure before new metrics does not erase the previous streaming snapshot. Explicit replacement start clears prior-session diagnostics. A stale session/version cannot repopulate a newer attempt.

The name `capture_to_send_p95_ms` survives for wire compatibility, but its native producer measures frame age at queue extraction, before socket send. Monitor's Local p95 and diagnostics must be read as that local statistic. `audio_onset` carries capture/send QPC observations; the current controller does not consume it to compute an acoustic delay. `measured_latency_ms` remains null.

## Separate qualification recorder

The optional [Microphone](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/wasapi.rs#L94) path is separate from the stream's selected recording input. When `record_mic_path` is supplied, it opens default console Capture before transport setup and retains the first channel at the input mix rate. Its ordinary worker exit writes a mono float WAV plus a timestamps JSON sidecar, including a stop/drop caused by later transport failure; a capture error can return before writing. It has its own three-second initialization wait, 55-second recording loop and 56-second sample-storage bound; joins and file writes are outside those waits. Start validation does not prohibit this field on an unbounded start, although the production desktop does not send it. These are implementation limits of a qualification facility; no recording was executed for this review.

## Verification coverage

Both-rate native [padding fixtures](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/live.rs#L534) cover partial/whole/resumed input, captured silence, zero gain, EQ tails, saturation/serialization, reset/error snapshots and unchanged queue recovery. [Core metrics tests](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/CapturePaddingMetricsTests.cs#L31) cover compatibility, UInt64 boundaries, invalid optional fields, stale events, retries/cancellation and terminal snapshots. [WPF diagnostics fixture](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Verification/UiDiagnostics.cs#L14) verifies actual bindings, refresh and 20-digit layout. [H acceptance](../issues/GROUP-H-ACCEPTANCE.md) retains the failing accounting and layout controls.

Executed suite/package evidence is centralized in [Verification](verification.md): [exact `0623787` normal CI](https://github.com/ThatMarko/AirFlash/actions/runs/38329156167) and the separate full package run at code-identical `4ae1db4`. Subsystem/group acceptance records retain earlier controls and source-specific results. This deeper review inspected source and regression assertions; it did not rerun suites or hardware checks.

These fixtures do not establish real capture disconnect behavior, timer quantum, remote drift, acoustic latency or full receiver delivery.

## Historical audit

At `41190e0` capture selected Render only and exposed packet-level underruns without missing-frame/wholly padded counts. Original stable later added recording support; H added accounting. The [previous pinned page](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/analysis/capture-clocks.md) retains original links/audit provenance. Its “native byte-identical”, “ISSUE-16 proposal” and pending reboot labels are historical.
