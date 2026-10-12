# Group H acceptance — diagnostics and capture accounting

## Current stable-base qualification — 2026-10-11

### Additional portable verification — 2026-10-12

The exact `dc521ae` portable executable failed the card-boundary assertion after 818 full UI checks, although its retained images and glyph/visibility geometry show complete counters. The old assertion used declared Border padding without DPI rounding. Harness-only fix `6855dfe27b4283a58ede7c37474f3e2aa3c31cf7` now uses the finite parent-assigned layout slot, retaining individual glyph, card and viewport guards and the same tolerance. Independent final code review is clear and its Release solution build has zero warnings/errors.

Exact-source hosted apphost run [38292183363](https://github.com/ThatMarko/AirFlash/actions/runs/38292183363) passes 40 focused checks in all three catalogs with Chinese regional formatting. All four measured cards per catalog actually observe 1.25 X/Y DPI; the normal window measures 835.2 DIP on that runner and the minimum measures 640 DIP. Source/run/binary records and downloaded ZIP digest were verified. This synthetic notification targets only the fixture's own HWND and changes no display preference.

Controlled source `dc6f85c06df9b61c04d0530d1f894b28c215b3da` restores only the legacy arithmetic. Run [38292409095](https://github.com/ThatMarko/AirFlash/actions/runs/38292409095) fails the intended normal-card fit assertion after 25 successful checks at observed 1.25 DPI, while all 20 glyphs remain contained. Controlled source `7461fb22ff9aece9ad4080a42dbae42df35c1fed` removes wrapping only after measuring the healthy normal window. Run [38292264290](https://github.com/ThatMarko/AirFlash/actions/runs/38292264290) fails the intended minimum-width assertion after 26 checks: normal cards still fit, the unwrapped minimum value overflows its finite allocated slot, and the second wrapped value fits. Both exact-source artifacts and specific failures were verified; arbitrary nonzero exits were not accepted as evidence. Their retained branches must never be integrated.

Main integrates the corrected harness at `107174a6b325245dca6add3d474790fed4624d79`. Its unofficial run [38292659730](https://github.com/ThatMarko/AirFlash/actions/runs/38292659730) passes all six complete portable catalog/region pairs with 1,059 assertions each and three observed fractional-DPI runs with 40 each, then passes disposable MSI install/repair/upgrade/uninstall/data preservation. Downloaded package/report ZIP digests, source/run/attempt and every EXE/MSI file hash are verified. Actual UI executable SHA-256 is `ab85446f73f1c4fd62c867447ebede6b7225adf613f788911e5ded94b23e72a4`, matching the package manifest and compatibility audit. These executed packaged results close the additional H geometry failure; earlier DLL and MSI receipts remain separate. Normal CI at the same source exposed an inherited EQ fixture issue, corrected separately in [F acceptance](GROUP-F-ACCEPTANCE.md).

Current public head `6855dfe27b4283a58ede7c37474f3e2aa3c31cf7` is pushed against stable `cf420d9`. Runtime qualification at predecessor `7f5135384d2245cfdf39d9c77b591c39df1981e3` passes 31 focused/233 owning Core tests, a zero-warning/error Release desktop build and 39 focused WPF checks in each catalog. The newer commit changes only the geometry harness and its opt-in DPI dispatch; runtime/Core/native source is unchanged. The fixture holds terminal audio cleanup explicitly and joins the exact owned worker before measuring diagnostics-copy effects; removing that join fails the held-worker assertion. Earlier 38-check/602-check receipts below predate this extra observable scheduling assertion and remain historical.

Current upstream recording selection, direction flags, two pure native tests and the additional ignored hardware qualification are retained. Both recording and loopback feed the shared output-rate queue; padding accounting remains before EQ, once per shared pull. New recording/Coffee consumers were included in the final dynamic localization re-audit; only still-unused candidates were removed. Fresh combined-source Windows native qualification is recorded in continuation acceptance, separate from the earlier exact feature/native mutation receipts below.

These current sections supersede earlier pending-reboot, unpushed-feature and proposal wording. Combined fork results and publication limits are recorded in [continuation acceptance](CONTINUATION-ACCEPTANCE.md). The dated evidence below retains its original source boundary.

## Earlier implementation evidence

The public feature `codex/fix-diagnostics-metrics` is pushed at
`c2223e90a28bb04aa67c79364ee6b01a113810a1`, based on verified original stable
`91698d8cd3bdf5666b417a12eaaf8d6a1ea36363`. Its complete diff and every outgoing
commit exclude fork-only planning. Independent managed, native, regression and
visual reviews are clear. No upstream issue or PR was created.

## Diagnostics access

Monitor exposes the existing localized `CopyDiagnosticsCommand`. The command
uses the existing serializer and Settings error surface; the test-only writer
injection leaves production clipboard work on the STA dispatcher. Actual bound
button tests cover Idle, old/new Streaming, Error and fresh attempts, with exact
snapshot equality and no settings save, engine command or audio mutation.
Removing only the new action reproduces the original unavailable-action behavior:
the realized-button fixture fails after five isolation checks. Writer/metric
scaffolding remains in that controlled baseline; it is not the entire unchanged
original tree. Source bytes are restored in finally and the restored build passes.

## Local padding

Native `padded_frames` counts substituted output-rate frame slots before EQ.
`fully_padded_packets` counts shared pulls with all 352 slots missing. Both new
UInt64 counters saturate, reset per worker attempt and increment once in the
shared capture pull before speaker fan-out. Existing underrun semantics, capture
onset, scheduling, queues and DSP processing are preserved. Captured silence,
zero output gain and audible EQ tails do not redefine missing input slots.

The desktop accepts these as independent optional UInt64 values. Missing or
invalid values display unavailable, measured zero remains zero, and the full
unsigned range survives parsing and diagnostics. Retry clears current metrics
while preserving the immutable pre-fault capture snapshot. No percentage is
derived from input-rate capture counts; these are neither network loss nor
acoustic measurements.

The original decoder, with only record-shape compatibility scaffolding, fails
both decisive rate cases (expected 353, actual unavailable). All 31 focused cases
and the final 230-test Core suite pass with the implementation. Six new native
queue fixtures cover both supported rates, partial/full fills, resumed capture,
silence, EQ tails, saturation, serialization, error and reset paths. Exact-source
Windows run `38169516498` passes 53 native tests, 230 Core tests, warnings-denied
lint, Release build and compatibility checks. One existing hardware qualification
test remains intentionally ignored.

The isolated branch `codex/verify-padding-accounting-regression` at
`b3a1d0887d3bba4a7db4f12bce8d05c925f3a4e7` restores the original packet accounting
with additive test/metric scaffolding. Exact-source run `38169613906` fails five
padding assertions while 27 library controls pass. It is retained as evidence
and must never be integrated into product branches.

## UI and localization

The owning English WPF harness passes 602 checks; focused English, Simplified
Chinese and Traditional Chinese runs pass 38 each with crossed regional cultures
recorded. A stronger card-boundary check exposed clipped 20-digit counters at
normal and minimum window widths. The wrapping fix now verifies every rendered
digit lies inside the value/card and both cards are fully visible; reviewed PNGs
show the complete values. The collapsed-expander fixture error, weak early pass,
actual clipping failure and corrected receipts remain separate.

Localization cleanup was performed after runtime and fixture implementation.
Exact-string searches and dynamic lookup review confirmed the ten candidates
remain unused in both Chinese catalogs. Only those keys were removed; current
labels, diagnostics action, presets, format arguments and serialized preferences
remain intact. No test merely mirrors the deleted-key list.

Receipts are in the feature's `artifacts/group-h-2026-10-11` and
`artifacts/group-h-native-2026-10-11`; verified hosted source records and raw logs
are preserved in main's `artifacts/continuation-2026-10-11/h-hosted`.
All execution uses fake services, synthetic queues/connections, temporary stores
and isolated output. No real capture, receiver, clipboard, credential or endpoint
operation occurred locally. Original stable has not integrated this feature.
