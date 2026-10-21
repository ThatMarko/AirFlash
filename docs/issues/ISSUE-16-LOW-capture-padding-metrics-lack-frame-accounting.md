# [LOW] Local missing-frame accounting — implemented in fork main

## Current-source boundary — 2026-10-21

Combined `6c1e2ef` adds optional padded-output-frame and wholly-padded-pull metrics to stable `439de626`, before EQ and once per shared pull. It preserves existing underruns, excludes captured/scheduled silence, resets per worker and retains pre-fault snapshots. These local counters cannot establish a network/acoustic cause or justify closing incident #7.

[Current source, acceptance and executed limits](PR-READINESS-2026-10-21.md) supersede older current/open/schema labels below. The remaining source links and test counts retain their recorded historical baselines; this documentation refresh claims no new test execution or upstream publication.

**Status:** Implemented and integrated in own-fork main `8989b3609dd1294f4d1d6eb1502f345cc22bc382`, reviewed 2026-10-12. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`; this implementation is not yet integrated there. Severity is the original triage priority.

**Group:** H — Diagnostics/observability. **Feature head:** `6855dfe27b4283a58ede7c37474f3e2aa3c31cf7`. [Plan](IMPLEMENTATION_PLAN.md#issue-16), [acceptance](GROUP-H-ACCEPTANCE.md), [capture/clocks](../analysis/capture-clocks.md), [EQ](../analysis/equalizer.md).

## Current behavior and source trace

[Loopback::packet](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/live.rs#L124) counts absent **output-rate frame slots before EQ**, once per shared 352-frame pull before speaker fan-out. Existing underrun_packets still increments once for any fill. Saturating UInt64 padded_frames adds exact missing slots; fully_padded_packets adds one only when all 352 were absent.

| Input available for one pull | Underruns | Padded frames | Wholly padded pulls |
| --- | --- | --- | --- |
| 0 frames | +1 | +352 | +1 |
| 351 frames | +1 | +1 | +0 |
| 352 frames, including captured silence | +0 | +0 | +0 |

Gain zero/real captured silence are not missing input; EQ history can produce a tail from wholly padded input. Playback and Recording use the same downstream path. Existing queue/resampler/onset/scheduling/underrun behavior remains.

[desktop parsing](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs#L531) accepts new counters independently as optional full-range UInt64. Absent/invalid is unavailable, not invented zero. Current metrics reset per worker attempt; immutable pre-fault values survive retry and pre-capture retry failure. [Monitor](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Ui/SettingsWindow.xaml#L239) and existing diagnostics display them. No fill percentage uses input-rate capture_frames.

## Acceptance and evidence

Both-rate [native accounting fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/live.rs#L534) cover partial/whole/resumed fills, silence/gain zero/EQ tails, saturation/serialization and error/reset snapshots. [Core compatibility/failure fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/CapturePaddingMetricsTests.cs#L31) cover old events, invalid/full UInt64, stale sessions, retry/cancellation and immutable fault data. Actual WPF checks cover refresh/copy/layout.

H acceptance retains the failing old-accounting control and fractional-DPI investigation. Current layout assertions use finite parent-assigned WPF slot, actual 125% DPI, every digit/viewport and unchanged tolerance. Restoring old rectangle arithmetic and disabling wrapping each fail their intended independent controls; neither is merged into main.

Exact-main CI passed 667 Core, 124 Python and 69 native tests (recording-device qualification and thirty-minute localhost soak ignored). Packaged integration at `4ae1db4` passed six 1,059-assertion UI runs, three actual 125% DPI runs with 40 each and disposable MSI lifecycle; current runtime/package inputs are identical, with main's last commit documentation only. This refresh reviewed sources/records; it did not rerun suites or use real user data/devices.

## Remaining work and limits

This is observability, not an identified network/acoustic playback cause. Local queue p95 precedes socket send; packet-level underrun duration overstates partial missing frames. Padding can be normal during silence; no receiver delivery, Wi-Fi/DPC cause or measured remote clock correction is inferred. Upstream publication is separate.

## Historical finding

Original `41190e0` exposed packet-level underruns but discarded one-versus-352 missing-slot information. The earlier speculative claim that sender recovery was absent was rejected: local depth correction, stale trimming and live schedule recovery already existed. The resolved scope adds bounded truthful accounting. The [archival report at the prior documentation commit](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/issues/ISSUE-16-LOW-capture-padding-metrics-lack-frame-accounting.md) preserves original `41190e0` source anchors, reproduction/proposal and dated updates. Its proposal-only/open/pending-reboot labels are historical, superseded by the current resolution above.