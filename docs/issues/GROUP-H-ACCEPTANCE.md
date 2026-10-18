# Group H acceptance — diagnostics, padding and localization

## Standalone SDK and diagnostic qualification — 2026-10-18

Prepared head is `1396c55eb6cd4c314a95a94a94adb213e381755e` on `codex/fix-diagnostics-metrics`, based on original stable `cf420d97700463c56adc31408510e8336e99e51c`. SDK commit `7ae2aae` supplies the shared ILLink 10.0.12 dependency/selected-host CI correction. The following diagnostic-only change adds actual culture fields to full/tray success and full-run failure, restores the exact selected catalog in finally and asserts its localized marker. H already parsed formatting culture before startup; that parser, native accounting, Monitor behavior, compatibility fields and translation catalogs are unchanged.

The assertions-only pre-fix control retains old Boolean restoration and fails its exact-catalog marker after 19 tray controls. Its full-run failure report records zh-CN formatting/zh-TW caller UI culture; async caller culture restoration alone cannot prove the shared translation catalog is correct. Corrected tray passes 20 assertions. Six complete full WPF cells now pass **603 each**, independently observing en-US/zh-CN/zh-TW catalogs crossed with en-US/zh-CN formatting and exact catalog restoration. This supersedes the earlier requested six-cell result's missing full-report/restoration proof; earlier executions remain attached to their actual source.

Fresh **SDK 10.0.401 and 10.0.402** each pass forced locked restore, **43 focused / 233 owning Core**, unchanged lock hashes, consumed ILLink analyzer and a zero-warning/error Release solution build. The tested working tree was committed unchanged as `1396c55`; raw precommit head/changed-file receipts and final source/DLL hash bindings are preserved separately. Focused Monitor passes **39**, and all three catalogs pass **40 each** at observed X/Y 1.25 DPI with both widths, twenty digits, finite layout slots and viewport/glyph fit. Local output is isolated under `artifacts/implementation-2026-10-18`; no real clipboard, receiver, capture or endpoint backend is used.

[Exact-source hosted Windows run 39143700861](https://github.com/ThatMarko/AirFlash/actions/runs/39143700861) succeeds at `1396c55`: **55 native / 233 Core**, native lint/Release build and compatibility audit, with selected SDK 10.0.402. Source.json separates actual feature checkout from workflow carrier `ed418c6`. Recording-endpoint qualification and the thirty-minute localhost soak remain two ignored tests. Independent final source/regression/evidence review is clear, including all twelve measured counter cards at actual 1.25 DPI with complete digit visibility.

Fork merge `36cd87f` preserves Main's already-qualified equivalent diagnostics plus all cross-group modes; its runtime/test/tool/workflow trees equal starting `ed418c6`. The branch is normally pushed to origin; no upstream issue/PR, merge or release is performed. Source/fake/hosted results do not establish physical display/receiver reliability, real DNS delivery, arbitrary native callback return-stack quiescence or physical power-loss durability.

## Current Main integration — 2026-10-12

Reviewed fork runtime baseline `8989b3609dd1294f4d1d6eb1502f345cc22bc382` contains this feature; desktop/native/tooling trees match qualified code `4ae1db4f01dcf7857482dd136ea8d9dffa3aa8a1`. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`. The public branch is pushed to origin and its complete diff/every outgoing commit exclude private planning. [Combined acceptance](CONTINUATION-ACCEPTANCE.md) binds final 667 Core, 124 Python, 69 native, portable UI/DPI and MSI results. No upstream publication is authorized by this documentation update.

Feature `codex/fix-diagnostics-metrics`, head `6855dfe27b4283a58ede7c37474f3e2aa3c31cf7`; worktree `fix-diagnostics-metrics/AirFlash`. Reports 15/16/17 are complete. Independent managed/native/assertion/visual review is clear. Runtime predecessor `7f51353` passed 31 focused/233 Core, zero-warning/error build and 39 focused UI checks per catalog. Current head changes the harness only; exact-source fractional-DPI qualification passes 40 checks per catalog.

Current [capture metrics](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs#L8) and [diagnostic/pre-fault wrappers](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/TransportMetrics.cs), [Monitor](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Ui/SettingsWindow.xaml), [diagnostics assertions](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiDiagnostics.cs), [capture queue](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/live.rs) and [native events](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/session.rs) implement the behavior.

## Diagnostics access

Monitor's localized accessible action binds existing `CopyDiagnosticsCommand`, serializer and Settings error surface. Production clipboard work stays on STA; the narrow injected writer permits isolated fixtures. Idle, old/new Streaming, Error and fresh-attempt tests check the realized binding/copy, exact snapshot and no save/engine/audio side effect.

Removing only the action makes the realized-button fixture fail after five isolation checks; writer/metric scaffolding remains, so it is a controlled equivalent of the original unavailable action rather than literal whole original source. H's terminal-copy fixture gates cleanup and joins the exact owned worker before measuring nonmutation. Removing that join fails the held-worker assertion.

## Local output-padding accounting

Native `padded_frames` counts missing output-rate frame slots **before EQ**. `fully_padded_packets` counts shared pulls missing all 352 slots. Both UInt64 counters saturate/reset per worker attempt and increment once before speaker fan-out. Existing underrun semantics, queue recovery, capture onset and DSP scheduling remain unchanged. Real captured silence, zero gain and EQ tails are not missing frames.

Desktop fields are independent optional UInt64 values. Missing/invalid is unavailable, measured zero stays zero, full unsigned range survives decoding/diagnostics. Retry clears current attempt metrics while immutable pre-fault snapshots retain their values. No fill percentage from input-rate capture counts and no acoustic/network-loss interpretation are claimed.

The original decoder plus record-shape scaffolding fails two decisive rate assertions (expected 353/actual unavailable). Fixed historical `c2223e9` passes 31 focused/230 Core and [run 38169516498](https://github.com/ThatMarko/AirFlash/actions/runs/38169516498): 53 native, clippy/static-CRT/compatibility/build. Preserved `codex/verify-padding-accounting-regression` `b3a1d08` restores old accounting with additive scaffolding; [run 38169613906](https://github.com/ThatMarko/AirFlash/actions/runs/38169613906) fails five padding assertions with 27 controls. It must never merge.

Both supported rates cover partial/full fills, resumed capture, silence, EQ tails, saturation, event serialization/reset/fault. Current upstream recording selection/direction/native flags remain; recording and loopback share output-rate padding accounting before EQ.

## Additional portable verification — 2026-10-12

Exact `dc521ae` portable UI failed after 818 assertions at card fit, despite retained images/geometry showing all 20 digits contained/visible. Legacy arithmetic subtracted declared Border padding without fractional-DPI rounding. Harness-only `6855dfe` uses finite parent-assigned layout slots, keeping glyph/card/viewport checks and the same 0.5 DIP tolerance. Independent review is clear; no runtime/control semantics changed.

Healthy [run 38292183363](https://github.com/ThatMarko/AirFlash/actions/runs/38292183363) passes 40 checks in all 3 catalogs with Chinese regional formatting. Every measured card actually observes X/Y 1.25 DPI; normal window 835.2 DIP/minimum 640 DIP on that runner. Synthetic WM_DPICHANGED targets only the fixture HWND, frees owned native data in finally and changes no display preferences.

Legacy control `dc6f85c`, [run 38292409095](https://github.com/ThatMarko/AirFlash/actions/runs/38292409095), fails the intended normal-fit assertion after 25 controls at observed 125%, all 20 glyphs still contained. NoWrap control `7461fb2`, [run 38292264290](https://github.com/ThatMarko/AirFlash/actions/runs/38292264290), fails intended minimum fit after 26 controls: healthy normal cards fit, finite minimum slot overflows for the deliberately unwrapped value, sibling wrapped value fits. Exact source/archive/PNG/geometry/failure were checked; arbitrary nonzero exit was not accepted. Both branches must never merge.

Main `107174a` [packaged run 38292659730](https://github.com/ThatMarko/AirFlash/actions/runs/38292659730) then passes 6 complete portable pairs×1,059 and 3 DPI×40 plus MSI lifecycle. Its EXE SHA-256 is `ab85446f73f1c4fd62c867447ebede6b7225adf613f788911e5ded94b23e72a4`. Its normal CI still failed inherited EQ; [F acceptance](GROUP-F-ACCEPTANCE.md) documents the separate correction.

Final `4ae1db4` [run 38294493405](https://github.com/ThatMarko/AirFlash/actions/runs/38294493405) re-executes all 6×1,059/3×40 and MSI stages; actual EXE `420fa97cf8f848e3479eb17f76a29c418a27033cc2e8c270d8f4c826e6ab1f5d` matches manifest/bundle audit. Independent comparisons preserve prior assertion multiplicities; native and current Main sources are bound separately in continuation acceptance.

## Localization and historical evidence

Dynamic/literal/markup/preset consumer review was performed after final UI/metrics changes, including new recording/Coffee consumers. Only the ten still-unused candidates were removed from both Chinese catalogs. Current labels, formats, presets, diagnostics/Monitor caveats and serialized preferences remain. No deletion-mirroring test was added.

Earlier 602 full/38 focused UI counts predate the owned-worker join assertion. Earlier clipped 20 digit failure led to wrapping; collapsed-expander fixture error/weak pass/actual clipping/corrected reports remain distinct. Receipts stay in `artifacts/group-h-2026-10-11/`, `group-h-native-2026-10-11/`, fork `h-hosted/`, `final-cf420/dpi-hosted/` and source-bound final package directories.

All local fixtures use fakes/synthetic queues, temporary stores and isolated output. Hosted modal clipboard/MSI operations are disposable. No local real capture/receiver/endpoint/store is used. H is integrated into fork Main, not upstream; these counters and DPI checks do not qualify physical displays, network/acoustic reliability or hardware capture.
