# Equalizer

Source reviewed on 2026-10-12 at own-fork `main` `8989b3609dd1294f4d1d6eb1502f345cc22bc382`, containing original stable `cf420d97700463c56adc31408510e8336e99e51c`. This page describes the integrated fork runtime; original upstream publication and historical test sources are separate.

One stereo processor handles resampled capture after queue extraction/padding and before master gain, clipping and S16 big-endian conversion. Its shared PCM is then packetized/encrypted separately for each member. Capture direction does not change this downstream path. EQ edits do not change the transport restart signature. Group F makes replies describe retained preparation truthfully; Group H counts missing frames before DSP.

## Settings and the filter

[native settings](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/equalizer.rs#L7) and [desktop settings](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/EqualizerSettings.cs#L14) use ten fixed peaking bands:
`31.25, 62.5, 125, 250, 500, 1000, 2000, 4000, 8000, 16000` Hz. Editor labels round the first two to 31/63 Hz. Default is disabled, zero preamp/ten zero gains. Every gain/preamp must be finite within ±12 dB, including disabled settings. Native fixed-array/unknown-field checks remain strict; desktop extension data is preserved but wire parameters send only defined fields.

Each RBJ section uses Q=1.4, A=10^(gain/40), ω=2πf/rate and α=sin(ω)/(2×Q), normalized by 1+α/A. Zero-gain sections are exact wires. Native [Prepared::new](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/equalizer.rs#L77) only accepts 44,100/48,000 Hz. App validation enforces those rates; the standalone desktop response helper does not independently reject all other rates.

Disabled or fully flat/zero-preamp settings bypass exactly. Otherwise ten f64 sections have separate channel histories, effective gain follows, then f32 output/master gain/clamp/S16.

## Sampled headroom

Enabled preparation samples summed band response at DC, Nyquist, band centers and 4,096 logarithmic points from 10 Hz to half rate. Peak is at least zero; attenuation is −max(0, preamp+peak), effective preamp is preamp+attenuation. This bounds the **sampled frequency response**, not arbitrary transient peaks/no-clipping for every signal. Packet conversion still clamps.

The desktop helper returns zero for disabled/invalid settings; native rejects invalid and stores zero disabled headroom. [headroom display](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/ViewModels/EqualizerEditor.cs#L47) waits 50 ms, calculates off-dispatcher and discards stale revisions. Continuous edits can produce further passes; it does not wait for 50 ms of edit-free quiet. Rate prefers active snapshot, otherwise draft/fallback 44,100.

Flat/Bass boost/Vocals/Pop/Rock presets replace gains and zero preamp; Custom derives from bands alone. Reset retains enabled; disable retains the curve. These names are dynamic localization inputs and survived catalog cleanup.

## Preview and persistence

[draft editing](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/ViewModels/SettingsViewModel.cs#L114) validates and previews with the panel owner GUID. [PreviewEqualizerAsync](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs#L46) clones the preview, advances sequence and gives it precedence over saved settings for live sends and future starts, including retries/switches. The 50 ms send loop coalesces an interval and repeats when changed during send; continuous dragging still sends. A semaphore serializes EQ with start.

Apply saves/applies first, then clears that panel preview and sends current saved EQ. Save failure keeps draft/preview. Disposal initiates clearing; owner comparison prevents an older panel clearing a newer preview. Clearing cancels pending send, increases sequence and uses saved settings. It does not prove completed engine DSP.

Replies are accepted only for current lifecycle generation/sequence. Current equalizer_error appears on the editor and stays nonfatal; success clears its error. A disconnected/not-ready transport caches preview for the next start rather than pretending to transmit it.

## Command acceptance and audio application

[equalizer_event](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/main.rs#L59) requires positive sequence, matching stored Running/start control slot and valid settings, preparing on the command thread. Probe/Pair slots reject EQ nonfatally. A stored start entry may exist before streaming or after its worker finished; this change does not define worker-liveness acknowledgment.

[Control::set](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/equalizer.rs#L151) returns outcome **and retained sequence/preparation**:

| Submitted request | Mailbox | JSONL result |
| --- | --- | --- |
| Newer sequence | Adopt preparation | equalizer_changed with retained sequence/headroom |
| Same sequence, identical full Prepared | Idempotent, unchanged | Same successful retained metadata |
| Older sequence | Unchanged | Nonfatal equalizer_error |
| Same sequence, different Prepared | Unchanged | Nonfatal equalizer_error |

Replay identity compares the **full Prepared value**: coefficients, gain, bypass and both headroom fields. Equality of coefficients/headroom alone is insufficient. Error sequence echoes the request so existing desktop filtering remains compatible. Subsequent valid updates still succeed.

This response acknowledges mailbox preparation adoption/idempotency. It does not claim audio-thread pickup, crossfade completion or receiver/acoustic adoption. No new JSONL version or optional outcome field is required.

[packet pull](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/live.rs#L124) uses mailbox try_lock once per packet; contention skips pickup for that packet. The surrounding State lock remains blocking. A newer changed curve gets fresh history and a rate/50-frame (20 ms) old/new-output crossfade; updates during a fade replace one pending curve. Unchanged active preparation preserves history; matching the target clears pending.

## Finite-probe restriction

Start validates enabled EQ separately and applies finite bounds to a disabled clone, allowing unbounded capture start. Finite probe rejects enabled EQ, remains at gain ≤0.1/duration ≤5,000 ms, and still validates disabled gains. Desktop production starts are not probes. See [ProbeOptions validation](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/session.rs#L73).

## Verification coverage

Current [serialized reply/retained mailbox fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/main.rs#L426) verify both rates, accepted/replayed/stale/conflicting outcomes, invalid payloads/wrong slots and later recovery. [full-preparation mailbox fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/equalizer.rs#L406) and DSP tests cover bypass, response, channel independence, history and fade/latest update. Core/actual WPF editor fixtures cover save/preview/cancel/owner/scrolling and current nonfatal feedback.

[Group F acceptance](../issues/GROUP-F-ACCEPTANCE.md) retains the submitted-success-metadata mutation that fails. It also distinguishes the original CI `expected 9 / actual 4` test failure (root timing cause inferred) from a deterministic unchanged-runtime quiet-read-timeout control. Current [healthy rollback fixture](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/SessionTests.cs#L35) gates actual cancellation-aware send; the separate [expired-connection fixture](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/SessionTests.cs#L73) proves cached preview reaches the next start. Production timeout/preview logic was not changed to hide that fixture failure.

[Scrolling acceptance](../issues/SETTINGS-SCROLL-ACCEPTANCE.md) covers the separately upstream-merged editor wheel/layout improvement. [H acceptance](../issues/GROUP-H-ACCEPTANCE.md) covers accounting before EQ and its retained failure snapshots.

The [continuation acceptance](../issues/CONTINUATION-ACCEPTANCE.md) records exact-main CI: 667 Core, 124 Python and 69 native tests passed; recording-device qualification and the thirty-minute localhost soak remained ignored. Complete packaged integration at `4ae1db4` passed six 1,059-assertion portable UI runs, three actual 125% DPI runs with 40 assertions each and disposable-runner installer checks. Main `8989b36` differs in documentation only. This refresh is source/documentation review, not a new hardware or suite execution.

No test claims arbitrary-signal no-clipping, measured acoustic response or physical receiver curve completion.

## Historical audit

Original `41190e0` silently ignored non-newer mailbox updates but returned successful **submitted** preparation metadata. The DSP sequence guard itself worked; the result contract was misleading. F resolves that mismatch without expanding to worker liveness. The [previous pinned page](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/analysis/equalizer.md) preserves the original audit/proposals; its unimplemented/feature-only labels are superseded.