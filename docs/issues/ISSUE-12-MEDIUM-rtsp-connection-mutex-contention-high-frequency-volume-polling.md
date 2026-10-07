# [MEDIUM] RTSP Connection Mutex Contention from High-Frequency Volume Polling

- **Issue ID**: ISSUE-12
- **Severity**: **MEDIUM**
- **Subsystem**: Native Audio Engine & RTSP Protocol Layer (`native/airflash-engine`)
- **Status**: Open / Triaged
- **Target Files**:
  - [`native/airflash-engine/src/volume.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/volume.rs#L125-L150)
  - [`native/airflash-engine/src/transport.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/transport.rs#L285-L315)
  - [`docs/analysis/control-and-mute.md`](../analysis/control-and-mute.md)

---

## 1. Summary
In `airflash-engine`, a single encrypted RTSP TCP connection (`Arc<Mutex<Connection>>`) is shared between two independent background worker threads:
1. The **Volume Worker** (`volume.rs`), which executes an HTTP/RTSP `GET /info` request every **1.0 second**.
2. The **Feedback Worker** (`transport.rs`), which executes an HTTP/RTSP `POST /feedback` request every **2.0 seconds** to calculate RTCP round-trip time and jitter health.

On Apple HomePod OS 27, responding to `GET /info` while streaming audio can take between 50 ms and 250 ms. When Wi-Fi contention occurs, the volume worker holds the RTSP connection mutex, blocking the feedback worker. This produces artificial round-trip latency spikes, triggering transient `feedback_delayed` warnings and risking false transport timeout teardowns.

---

## 2. Technical Root Cause Analysis

### 2.1 Shared Lock Contention
In [`volume.rs:L125-L135`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/volume.rs#L125-L135):
```rust
for (host, _, connection) in &peers {
    if done.is_cancelled() { return; }
    match read(&mut connection.lock().unwrap()) {
        Ok(value) => values.push(Some(value)),
        Err(error) => { values.push(None); errors.push(format!("{host}: {error:#}")); }
    }
}
...
next = Instant::now() + Duration::from_secs(1); // 1-SECOND POLLING
```
Where `read()` performs:
```rust
let info = connection.request("GET", "/info", &[], &[])?.plist()?;
```
In [`transport.rs:L285-L295`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/transport.rs#L285-L295):
```rust
let mut connection = connection.lock().unwrap(); // CONTENDS WITH VOLUME WORKER
let started = Instant::now();
connection.begin_request("POST", "/feedback", &[], &[])?;
```

### 2.2 Consequence on Telemetry and Health
* `POST /feedback` measures the time from lock acquisition until the receiver returns HTTP 200 OK.
* If `GET /info` is currently in flight or awaiting a TCP ACK from the HomePod, the feedback worker blocks waiting on the mutex.
* This inflated elapsed duration is recorded in `s.metrics.feedback_rtt_ms`. If it exceeds 1,500 ms, the engine flags `feedback_delayed = true`, giving the false impression that audio transport is degrading.

---

## 3. Reproduction Steps
1. Connect AirFlash to a HomePod over a moderately congested 2.4 GHz or 5 GHz Wi-Fi link.
2. Monitor the Settings -> Monitor tab for `Feedback RTT`.
3. **Observed Result**: Spikes of 200–500 ms in Feedback RTT periodically appear even when RTP audio UDP transmission exhibits 0% loss.
4. **Expected Result**: RTSP control telemetry should reflect genuine network RTT without mutex contention from non-essential status polling.

---

## 4. Proposed Solution
1. **Throttle Background Volume Polling**:
   Increase the background `GET /info` interval in `volume.rs` from 1.0 second to 10.0 or 15.0 seconds during active streaming.
2. **On-Demand Polling**:
   Only poll volume eagerly (e.g. at 500 ms) when the user opens the AirFlash flyout UI, and throttle down when the UI is closed.
3. **Separate RTT Measurement**:
   Start `Instant::now()` in `transport.rs` **after** acquiring the lock rather than before, so mutex wait time is not conflated with network transit time.
