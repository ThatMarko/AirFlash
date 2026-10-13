# Expose diagnostic copying and distinguish partial from wholly padded capture pulls

## Current behavior

Reviewed original stable source: `cf420d97700463c56adc31408510e8336e99e51c`.

Settings constructs a localized `CopyDiagnosticsCommand` using the existing serializer and error surface, but no Monitor control binds it. Users cannot invoke that combined-copy action through the visible page.

Native capture already pads a 352-frame pull with zero-valued frames when input is missing and increments `underrun_packets` once for any padding. A pull missing one frame and a pull missing all 352 frames both add one underrun. The amount of substituted audio is discarded, limiting local diagnostic interpretation. This does not establish a network, receiver or acoustic fault.

Relevant original source:

- [Existing diagnostics-copy command](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.App/ViewModels/SettingsViewModel.cs#L89), [Monitor template](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.App/Ui/SettingsWindow.xaml#L207).
- [Capture substitution and packet-level accounting](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/native/airflash-engine/src/live.rs#L120-L159), [desktop metric decoding](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.Core/SessionController.cs#L361-L367).

## Reproduction and expected behavior

Inspect the realized Monitor page with fake services and an injected diagnostics writer. There is currently no button/binding for the command. Expected behavior is an accessible localized action whose copied text equals the existing serializer snapshot and whose failures use the existing Settings error surface.

Use synthetic output queues at 44.1 and 48 kHz: provide zero, 351 and 352 frames for one pull. Expected added counters are respectively `352/1`, `1/0` and `0/0` for padded frames/wholly padded pulls. Full captured silence remains `0/0`. Accounting occurs before EQ and once per shared pull, before speaker fan-out.

## Bounded fix and acceptance

- Bind an accessible Monitor action to the existing copy command; preserve dispatcher/STA execution. Assert realized visibility, focusability, localization, automation invocation, exact text and error reporting across Idle, Streaming, Error and a fresh attempt. Do not add upload or duplicate report/logging systems.
- Add saturating UInt64 `padded_frames` and `fully_padded_packets`, preserving existing underrun semantics. Reset each worker attempt; retain immutable pre-fault snapshots through retry and later pre-capture failure.
- Parse counters independently as optional full-range UInt64. Missing/invalid fields display unavailable; measured zero remains zero. Preserve old/new JSONL v1 compatibility and show the values in Monitor and diagnostics.
- Cover partial/whole/resumed pulls, real silence, zero gain, EQ tails, saturation, serialization, reset/error snapshots, stale-session filtering and hidden/reopened Monitor refresh. A wholly padded input can still produce an EQ tail; classify its input availability before DSP.
- Keep long values visible under finite WPF layout slots and actual synthetic fractional DPI. Gate/join owned fault cleanup before asserting Copy caused no engine, save or audio side effects.
- Review literal and dynamic localization consumers after final UI changes, then remove only the confirmed unused Chinese entries. Preserve current labels, presets, formats, recording controls and serialized settings compatibility. Avoid tests that merely mirror deleted keys.

## Verification and limits

A [source-equivalent old-accounting control](https://github.com/ThatMarko/AirFlash/actions/runs/38169613906) fails the decisive padding assertions with controls passing. Historical fractional-DPI fixtures separately distinguish incorrect border arithmetic from actual unwrapped overflow; their corrected assertion uses the parent-assigned finite layout slot without relaxing glyph/viewport checks. These controls remain tied to their exact source, rather than being relabelled as unchanged original source or fresh standalone qualification.

Final standalone managed/native/UI qualification must test the exact prepared branch in all three language catalogs, including regional formatting and synthetic 125% DPI. Combined-fork suite/package results are not branch proof. Padding counts missing frames in the local output-rate capture queue, not network loss or measured acoustic silence. Do not derive a fill percentage from input-rate `capture_frames`; real capture, physical displays and receiver performance remain unqualified. Temporary queues, fake endpoints and an injected writer suffice for the required tests.

## Prepared implementation and executed verification

The prepared implementation is [codex/fix-diagnostics-metrics](https://github.com/ThatMarko/AirFlash/tree/6855dfe27b4283a58ede7c37474f3e2aa3c31cf7), commit `6855dfe27b4283a58ede7c37474f3e2aa3c31cf7`, independently based on the original stable source above. Independent final source/regression review found no unresolved findings.

Fresh standalone Release build passed with zero warnings/errors; all **233 Core tests** passed with no skips. Separate isolated WPF runs passed **602 assertions each in 6 catalog/region combinations**, covering English, Simplified Chinese and Traditional Chinese. These are this branch’s results, separate from combined-fork qualification.

[Exact-source Windows validation](https://github.com/ThatMarko/AirFlash/actions/runs/38359132112) checked out this commit and passed **55 native tests**, Clippy, native Release build and the executable compatibility audit, plus the branch’s managed build/Core suite. Two existing tests remain ignored: recording-endpoint qualification and the thirty-minute localhost soak.

Three focused Monitor runs passed **39 assertions each**; three actual owned-window 125% DPI fixtures passed **40 each**, including both widths and full counter glyph visibility. These establish isolated rendering/binding behavior, not physical-display or acoustic qualification.
