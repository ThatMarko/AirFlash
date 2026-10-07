# [HIGH] WLAN Background Scan Latency Spikes Starve Realtime Audio Jitter Buffer

- **Issue ID**: ISSUE-10
- **Severity**: **HIGH**
- **Subsystem**: Wi-Fi Adapter Transport & Audio Engine (`native/airflash-engine` & `AirFlash.App`)
- **Status**: Open / Triaged
- **Target Files**:
  - [`native/airflash-engine/src/session.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/session.rs#L520-L550)
  - [`native/airflash-engine/src/transport.rs`](file:///C:/Users/marko/AirFlash/native/airflash-engine/src/transport.rs#L390-L425)
  - [`desktop/AirFlash.Core/Settings.cs`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/Settings.cs#L45-L65)
  - [`docs/analysis/capture-clocks.md`](../analysis/capture-clocks.md)
  - [`docs/analysis/source-check.md`](../analysis/source-check.md)

---

## 1. Summary
When streaming audio to an Apple HomePod over Wi-Fi on Windows 11 using the **"Real-time"** latency preset (120 ms), users report periodic micro-crackles, pops, and short dropouts (tracked in GitHub Issue #7).

The root cause is Windows 11 WLAN AutoConfig (`WlanSvc`), which periodically triggers background network scans every 60–120 seconds. During a background scan, the Wi-Fi adapter (e.g. Intel Wi-Fi 6E AX211) temporarily retunes off its 5 GHz operating channel to listen for beacon frames on other frequencies. This stalls all outbound UDP RTP transmission for 50–120 ms. Because the 120 ms preset leaves only ~20 ms of playout buffer cushion on the HomePod, the jitter buffer completely drains, producing audible glitches.

---

## 2. Technical Root Cause Analysis

### 2.1 The Playout Margin Deficit
On HomePod Software 27.0:
* Fixed internal hardware DAC playout latency: ~100 ms.
* At **120 ms Realtime Latency**:
  $$\text{Jitter Buffer Cushion} = 120\text{ ms} - 100\text{ ms} = 20\text{ ms}$$
* A 20 ms cushion can absorb at most two missed packets (at 7.98 ms per packet).

### 2.2 The Windows Wi-Fi Off-Channel Scan Phenomenon
By default, Windows periodically scans for surrounding Wi-Fi networks in the background. While the Wi-Fi card is off-channel:
1. No RTP UDP packets can be transmitted over the air.
2. The sender's socket queues outgoing packets in driver memory.
3. When the card returns to the home channel after 60–100 ms, buffered packets burst simultaneously into the network.
4. The HomePod's 20 ms playout cushion is exhausted within the first 25 ms of the stall, forcing the HomePod to output zero-samples or re-sync its DAC clock.

### 2.3 The Missing Windows Native Suppression API
Windows provides an explicit Native Wifi API (`wlanapi.dll`) to eliminate background scanning during active media sessions:
```c
WLAN_INTERFACE_CAPABILITY Capability;
BOOL bStreamingMode = TRUE;
WlanSetInterface(
    hClientHandle,
    &InterfaceGuid,
    wlan_intf_opcode_media_streaming_mode,
    sizeof(BOOL),
    &bStreamingMode,
    NULL
);
```
AirFlash does not call `wlanapi.dll` anywhere in its codebase, leaving media streams unprotected against periodic background Wi-Fi stalls.

---

## 3. Reproduction Steps
1. Connect a Windows 11 PC via Wi-Fi to a 5 GHz network with a HomePod.
2. Set Latency to **"Real-time" (120 ms)**.
3. Start streaming audio.
4. Open an administrative command prompt and manually trigger a Wi-Fi scan:
   `netsh wlan scan`
5. **Observed Result**: Playout immediately crackles or drops out momentarily. Settings Monitor displays a sudden burst of delayed packets.
6. **Expected Result**: Windows suppresses background scans while media is streaming, maintaining an uninterrupted packet stream.

---

## 4. Proposed Solution
1. **Enable Media Streaming Mode in Windows Wi-Fi**:
   In `desktop/AirFlash.App` (or a helper in `airflash-engine`), call `WlanSetInterface` with `wlan_intf_opcode_media_streaming_mode = TRUE` when `PlaybackState.Streaming` begins, and revert to `FALSE` when streaming stops.
2. **Raise Default Realtime Recommendation to 150 ms**:
   Adjust the default "Real-time" preset for standalone HomePods on OS 27 from 120 ms to 150 ms, increasing the jitter cushion from 20 ms to 50 ms.
