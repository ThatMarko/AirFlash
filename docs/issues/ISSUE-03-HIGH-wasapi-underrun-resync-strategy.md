# [HIGH] WASAPI Loopback Audio Capture Lacks Burst Resync and Telemetry Distinguishing Idle Silence from Buffer Starvation

- **Issue ID**: ISSUE-03
- **Severity**: **HIGH**
- **Subsystem**: Native Audio Engine (`airflash-engine`)
- **Status**: Open / Triaged
- **Target Files**:
  - [`native/airflash-engine/src/live.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/live.rs#L125-L165)
  - [`native/airflash-engine/src/live.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/live.rs#L320-L335)
  - [`native/airflash-engine/src/transport.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/transport.rs#L398-L430)
  - [`desktop/AirFlash.Core/TransportMetrics.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/TransportMetrics.cs)
  - [`docs/analysis/capture-clocks.md`](../analysis/capture-clocks.md)

---

## 1. Summary
In [`live.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/live.rs), the audio capture queue converts Windows WASAPI loopback samples into fixed-frame RTP audio packets (352 frames per packet at 44.1 kHz, approximately 7.98 ms per packet).

Whenever the capture queue does not contain a full 352 frames, the engine synthesizes zero-amplitude silence frames (`left: 0.0, right: 0.0`) and increments `s.metrics.underrun_packets`. 

However, forensic analysis reveals a critical design flaw:
1. **Conflation of Idle Silence and Glitches**: When Windows has no audio playing, WASAPI loopback capture halts entirely. As explicitly noted in [`live.rs:L323`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/live.rs#L323) (`// Silent render endpoints may emit no loopback packets. Sender pads silence.`), the sender intentionally generates synthetic silence packets to keep the RTP stream and PTP clock alive. These intentional silence packets increment `underrun_packets`, causing telemetry to display thousands of "underruns" (e.g. 11,614 packets = 92.7 seconds of natural silence), misleading users and diagnostics into diagnosing severe audio corruption.
2. **Missing Adaptive Resync on Real Glitches**: When audio *is* actively playing and capture queue starvation occurs due to Windows DPC/interrupt spikes, no resynchronization or buffer adjustment signal is emitted to the host application or receiver.

---

## 2. Technical Root Cause Analysis

### 2.1 The Underrun Metric Inflation
In [`native/airflash-engine/src/live.rs:L125-L155`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/live.rs#L125-L155):

```rust
for _ in 0..FRAMES {
    let frame = if let Some(f) = s.frames.pop_front() {
        f
    } else {
        underrun = true;
        Frame {
            left: 0.0,
            right: 0.0,
            qpc: now,
        }
    };
    ...
}
if underrun {
    s.metrics.underrun_packets += 1;
}
```

When Windows media playback is paused or silent, WASAPI emits no buffer events. `live.rs` enters starvation fallback every 7.98 ms, generating ~125 silence packets per second. Over 90 seconds of paused playback, `underrun_packets` increments by over 11,000 packets.

### 2.2 The Lack of Burst-Glitch Handling
When audio is actively playing and a CPU spike or high DPC latency causes the WASAPI capture thread to miss its 15.6 ms scheduling quantum:
- The queue is drained dry.
- The sender sends zero-amplitude frames interspersed with real audio.
- The HomePod receiver's jitter buffer experiences timing flutter and clock-drift correction glitches.
- No `EngineNotice` is generated to inform [`AppViewModel.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/ViewModels/AppViewModel.cs) that the selected latency preset (e.g. "Real-time" 120 ms) is too aggressive for the host PC's current DPC latency profile.

---

## 3. Reproduction Steps
1. Start streaming with AirFlash to an Apple HomePod.
2. Pause all media on Windows for 60 seconds.
3. Open Settings -> Monitor.
4. **Observed Result**: `Local underrun packets` has surged by ~7,500 packets, despite flawless playback with no dropped audio.
5. Next, play music and run a heavy DPC load (e.g., LatencyMon or disk benchmark).
6. **Observed Result**: Micro-stutters and pops occur, but no warning or automatic latency adjustment is offered to the user.

---

## 4. Proposed Solution
1. **Differentiate Active Starvation from Intentional Silence**:
   Introduce two distinct metrics:
   - `padded_silence_packets`: Increments when the capture endpoint has been quiescent (> 100 ms of zero WASAPI samples).
   - `underrun_starvation_packets`: Increments only when the queue starves while actively receiving non-zero audio samples from WASAPI.
2. **Adaptive Burst Detection**:
   If `underrun_starvation_packets` exceeds 5 packets within a 500 ms window during active streaming:
   - Emit an `EngineNotice` (`"capture_latency_spike"`).
   - Prompt the user in the UI to switch from **Real-time (120 ms)** to **Normal (200 ms)** or **Buffered (500 ms)**.
