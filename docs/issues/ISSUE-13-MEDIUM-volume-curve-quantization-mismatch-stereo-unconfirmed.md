# [MEDIUM] HomePod Non-Linear Volume Curve Quantization Mismatch Causes False "Unconfirmed" Status

- **Issue ID**: ISSUE-13
- **Severity**: **MEDIUM**
- **Subsystem**: Volume Control Subsystem (`native/airflash-engine` & `AirFlash.Core`)
- **Status**: Open / Triaged
- **Target Files**:
  - [`native/airflash-engine/src/volume.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/volume.rs#L25-L75)
  - [`desktop/AirFlash.Core/DeviceVolumeState.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/DeviceVolumeState.cs#L15-L35)

---

## 1. Summary
When setting receiver volume in AirFlash (especially when controlling a HomePod stereo pair), the volume status frequently fails to confirm, remaining in `pending` for 3 seconds before snapping to `unconfirmed` with an exclamation warning in the UI, even though the physical sound volume adjusted properly on the speakers.

This is caused by a floating-point quantization mismatch between AirFlash's linear dB formula (`((db + 30.0) / 0.3).round()`) and Apple's internal non-linear AirPlay 2 volume curve. Because `volume.rs` requires an exact mathematical equality match (`*v == Some(command.percent)`) across all stereo members, a single-percent rounding discrepancy causes the confirmation check to fail.

---

## 2. Technical Root Cause Analysis

### 2.1 The Strict Equality Assertion
In [`native/airflash-engine/src/volume.rs:L65-L80`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/volume.rs#L65-L80):
```rust
fn to_db(percent: u8) -> f64 {
    if percent == 0 { -144.0 } else { -30.0 + f64::from(percent) * 0.3 }
}
fn from_db(db: f64) -> Result<u8> {
    Ok(if db <= -30.0 { 0 } else { ((db + 30.0) / 0.3).round() as u8 })
}
...
if !self.write_failed
    && readable
    && values.iter().all(|v| *v == Some(command.percent)) // <-- STRICT EQUALITY
{
    self.settled = true;
    return "confirmed";
}
```

### 2.2 Quantization Disparity
1. If the user sets volume to **50%**, AirFlash transmits `-15.0 dB` via RTSP `SET_PARAMETER volume: -15.000000`.
2. The Apple HomePod internal DSP quantizes volume levels according to hardware amplifier steps. In `GET /info`, the HomePod returns its internal floating point value, for example `-15.214 dB`.
3. AirFlash computes:
   $$\text{percent} = \text{round}\left(\frac{-15.214 + 30.0}{0.3}\right) = \text{round}(49.286) = 49$$
4. Because `49 != 50`, `values.iter().all(|v| *v == Some(50))` evaluates to `false`.
5. After 3 seconds, `volume.rs` times out and declares the status `"unconfirmed"`.

In a stereo pair, this probability doubles because *both* members must simultaneously round to the identical integer.

---

## 3. Reproduction Steps
1. Pair AirFlash with a stereo pair of Apple HomePods.
2. Drag the volume slider to values such as 33%, 50%, or 67%.
3. Observe the volume slider status indicator.
4. **Observed Result**: The status spinner spins for 3 seconds, then displays an exclamation mark or `unconfirmed` status, despite the audio volume clearly changing on both speakers.
5. **Expected Result**: The volume change is confirmed promptly if the reported volume settles within $\pm 1\%$ of the target.

---

## 4. Proposed Solution
1. **Allow a $\pm 1\%$ Tolerance in Volume Confirmation**:
   In `volume.rs:L68`:
   ```rust
   values.iter().all(|v| v.map_or(false, |actual| actual.abs_diff(command.percent) <= 1))
   ```
2. **Accept Settled Target in Core**:
   In `desktop/AirFlash.Core/DeviceVolumeState.cs`, allow incoming values within a 1-unit delta of `Target` to resolve the pending state to `confirmed`.
