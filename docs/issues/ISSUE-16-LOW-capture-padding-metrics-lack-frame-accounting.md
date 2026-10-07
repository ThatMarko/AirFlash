# [LOW] Capture-padding telemetry lacks missing-frame accounting

- **Issue ID**: ISSUE-16
- **Implementation plan**: [Work package ISSUE-16](IMPLEMENTATION_PLAN.md#issue-16)
- **PR group**: H — Diagnostics/observability
- **Severity**: LOW
- **Kind**: Enhancement
- **Type / Status**: Open enhancement — source-verified observability limitation; no verified playback defect
- **Subsystem**: Native capture metrics and desktop Monitor
- **Runtime baseline**: `41190e0d13a63a714c08dffe73ababca1804875c`, unchanged by documentation baseline `c077a0577de26ad7e44b8fd50f62456e12965fb0`
- **Evidence status**: Implementation and existing test sources inspected on 2026-10-07. The reproduction harness below is proposed and unexecuted. No receiver or audio-endpoint test was performed. No claim about current remote branch tips or excluded `67435a4` behavior is made.
- **Implementation status**: Proposal only; no runtime change applied
- **Target files**: [live.rs](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/live.rs#L39-L48), [SessionController.cs](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SessionController.cs#L360-L366), [AppViewModel.cs](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L43-L46), [SettingsWindow.xaml](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Ui/SettingsWindow.xaml#L227-L228)
- **Analysis context**: [Capture and clocks](../analysis/capture-clocks.md), [Equalizer](../analysis/equalizer.md), [Source check](../analysis/source-check.md)

## 1. Current behavior and bounded impact

A 352-frame media pull supplies zero-valued input for missing capture frames and increments `underrun_packets` once if **any** frame was absent. An empty queue and a queue containing 351 frames both add one to the same counter. `Metrics` includes cumulative input `capture_frames`, but no missing-output-frame count or wholly padded-packet count. The desktop displays this accurately as `Local underrun packets`; the limitation is that this count cannot quantify how many frames were padded. It also cannot establish why capture stopped producing frames. ([live.rs:39–48](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/live.rs#L39-L48), [120–159](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/live.rs#L120-L159), [SettingsWindow.xaml:227–228](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Ui/SettingsWindow.xaml#L227-L228).)

Silent render endpoints **may** emit no loopback packets, so fill is permitted normal operation. Multiplying the counter by `352 / sample_rate` gives the duration of packets containing some fill; it gives missing-frame duration only if every frame in those packets was absent. The equalizer follows padding and may produce a filter tail, so padded input is not necessarily a wholly silent output packet. ([live.rs:325–330](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/live.rs#L325-L330), [151–153](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/live.rs#L151-L153), [equalizer.rs:160–178](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/equalizer.rs#L160-L178).)

No audibility, DPC cause, fixed Windows scheduling quantum, Wi-Fi loss, receiver clock correction, or incident duration has been established. The original HIGH report's claim that recovery is missing is rejected: local queue-depth resampling correction, stale-frame trimming, and live sender-schedule recovery already exist. They are not receiver jitter-buffer measurements. ([live.rs:161–168](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/live.rs#L161-L168), [397–403](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/live.rs#L397-L403), [transport.rs:619–645](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/transport.rs#L619-L645).)

## 2. Source-visible mechanism

`packet` has one `underrun` boolean for the entire pull. The `else` branch sets that boolean for each absent frame but keeps no frame total; after the loop, one packet increment is recorded. The information needed to distinguish one missing frame from 352 missing frames is discarded. This is an accounting limitation, not evidence that padding or the existing 20 ms target/60 ms cap is defective. ([live.rs:129–158](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/live.rs#L129-L158).)

The desktop copies `underrun_packets` into `StreamMetrics` and renders the number without additional frame accounting. Separately, queue-age p95 includes zero-age synthetic frames and is sampled before actual socket send. Those existing values must not be relabeled as network loss or acoustic latency. ([SessionController.cs:360–366](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SessionController.cs#L360-L366), [AppViewModel.cs:43–46](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L43-L46), [live.rs:145–175](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/live.rs#L145-L175).)

## 3. Proposed reproduction — not executed

Use an isolated queue fixture derived from the existing `source()` test helper; open no capture device or network socket.

1. Pull one packet with zero queued frames. Inspect packet count and missing-frame accounting.
2. In a fresh fixture, enqueue 351 frames and pull once.
3. In another fixture, enqueue 352 frames and pull once.
4. Repeat at 44100 and 48000 Hz.

**Source-predicted current result:** cases 1 and 2 each add one underrun packet; case 3 adds none. No current metric records 352 versus one padded frame. This prediction has not been presented as an observed runtime result. A no-event WASAPI endpoint is not necessary to demonstrate the accounting gap.

## 4. Proposed change — unimplemented

Add cumulative missing-frame accounting, for example `padded_frames`, and optionally `fully_padded_packets`, alongside the existing `underrun_packets`. Count zero-valued substitute **input frames**, not output samples after DSP. Surface the counters or an explanatory Monitor description without inferring that no capture packets means intentional idle silence. If recent captured-frame activity is shown, describe it as evidence rather than an authoritative idle-versus-starvation classifier.

Keep the existing counter and its semantics for JSONL v1 compatibility; new event fields and desktop properties should be optional, and missing fields should display unavailable. Do not change schema 2 settings, presets, RTP pacing, capture queue limits, resampler control, or retry policy as part of this issue. Do not automatically expand receiver latency from a local padding count. Preserve exact-bit local mute restoration, mutually exclusive pair-verify/transient authentication, and credential ownership.

## 5. Testable acceptance criteria

- An empty 352-frame pull adds one underrun packet, 352 padded frames, and one wholly padded packet if that optional counter exists.
- A 351-frame queue adds one underrun packet and one padded frame; it is not wholly padded.
- A complete queue adds no padding counts. Counts accumulate independently of stream rate, gain, and equalizer output tails.
- Existing underrun/resume and scheduler-recovery behavior remains intact at both supported rates.
- Monitor text states local fill semantics. It does not claim an audible glitch, network loss, exact silence duration from packet count, or measured receiver latency.
- Older events lacking additional fields still parse; older readers retain their existing counters.

## 6. Existing coverage and verification plan

[live.rs:442–455](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/live.rs#L442-L455) tests empty-queue fill followed by resumed audio, and [472–489](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/native/airflash-engine/src/live.rs#L472-L489) tests overflow and stale-frame discard. [SessionTests.cs:211–217](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/SessionTests.cs#L211-L217) checks fake-engine standby without restart. These sources were inspected, not executed for this issue review, and do not cover a partial-packet missing-frame counter or real endpoint idle detection.

Add the isolated accounting cases above and desktop optional-field/wording checks. No hardware probe is needed. If later hardware qualification is independently authorized, use finite probes with sender gain at most 0.1, duration at most five seconds, and the existing disabled-EQ restrictions; do not use a long receiver soak to validate a telemetry enhancement.
