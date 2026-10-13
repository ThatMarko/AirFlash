# Group E acceptance — PIN input and credential lookup

## Current Main integration — 2026-10-12

Reviewed fork runtime baseline `8989b3609dd1294f4d1d6eb1502f345cc22bc382` contains this feature; desktop/native/tooling trees match qualified code `4ae1db4f01dcf7857482dd136ea8d9dffa3aa8a1`. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`. The public branch is pushed to origin and its complete diff/every outgoing commit exclude private planning. [Combined acceptance](CONTINUATION-ACCEPTANCE.md) binds final 667 Core, 124 Python, 69 native, portable UI/DPI and MSI results. No upstream publication is authorized by this documentation update.

Feature `codex/fix-auth-input`, head `0ebc59bd99e5ca9d1c7219a7b57bc9bb2b237c5a`; worktree `fix-auth-input/AirFlash`. Reports 08/09 are complete. Independent review is clear; refreshed standalone qualification passed 52 focused/221 owning Core tests and zero-warning/error Release build.

Current source: [ASCII normalizer](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/PairingPin.cs), [dialog](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Ui/PinDialog.cs), [authentication](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/auth.rs) and [credential storage](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/credentials.rs).

## PIN acceptance

One normalizer removes ASCII dashes and then requires 4–8 ASCII digits. Dialog enablement, final validation and returned PIN use it. Native validation remains strict; arbitrary whitespace/letters/Unicode digits are not accepted. Tests cover leading zeros, 4/8 boundaries, sixteen-character edit limits, selection replacement, invalid typing/text, cancel and actual fake `pair_pin`/`start` payloads.

Real single-line WPF paste is verified only on an explicitly enabled disposable hosted fixture, with clipboard restoration in finally. WPF truncates newline suffixes before submission. Ordinary local harness calls do not access the clipboard.

The unchanged modal return with a narrow test seam failed the normalized fake-command assertion. Evidence remains at worktree `artifacts/group-e-2026-10-11/baseline-pin/`. Initial fixed `47d0823` passed 19 normalizer cases/184 Core, warning-free build and expanded isolated UI; historical modal run [38162234703](https://github.com/ThatMarko/AirFlash/actions/runs/38162234703) passed 220 actual typing/paste assertions.

## Credential acceptance

Accessory `/info deviceID` still keys the current-user DPAPI file with existing normalization/magic/payload format. Loading opens one handle; **only initial open NotFound means absence**. Permission/other open errors and every read-stage failure, including NotFound, remain errors. Reads reject over 16,384 bytes. Deletion after successful open reads that opened object. DPAPI, JSON, identity and pair-verify failures cannot select transient pair-setup; methods remain exclusive.

Synthetic temporary-root/injected fixtures assert opened-object replacement/disappearance, bounds, malformed protected JSON, tampering, empty identity, serialized I/O detail and exactly one method. Existing localhost crypto/verify tests are controls. Preserved `codex/verify-auth-absence-regression` `a7c8646` emulates old error-hiding lookup; [run 38162296485](https://github.com/ThatMarko/AirFlash/actions/runs/38162296485) fails the decisive initial-open test with 35 controls passing. It is a source-equivalent mutation, not literal unchanged loader execution, and must never be integrated.

## Hosted qualification and startup isolation

Final historical feature `fc8b919`, [exact-source run 38164451047](https://github.com/ThatMarko/AirFlash/actions/runs/38164451047): 57 native passed/one existing thirty-minute localhost soak ignored; warnings-denied clippy, static-CRT Release build, compatibility audit, 218 Core, warning-free solution build and **225 actual modal assertions** passed. Source receipt identifies the tested feature SHA rather than workflow Main. Logs/receipts are retained under `artifacts/continuation-2026-10-11/ci-e-fc8b919/`.

The earlier refreshed run passed native compilation/tests but compatibility audit failed because its workflow lacked `uv`; setup correction and successful rerun remain distinct receipts. This was infrastructure, not an authentication runtime fix.

Refreshed upstream logs before ordinary harness setup. Initial isolated UI trials appended startup lines to the default log directory but did not read settings/credentials/endpoints. Integrated startup now selects isolated data before either log, recognizes exactly dispatched diagnostic modes and preserves unknown/ordinary startup. Five guards cover these choices; a broad predicate was rejected and the unknown-switch mutation fails. No earlier trial is relabeled fully isolated.

The combined `dc521ae` exact-source [run 38188371849](https://github.com/ThatMarko/AirFlash/actions/runs/38188371849) separately re-executed 225 modal assertions plus six synthetic DNS helpers. These invocation results are not assigned to final `4ae1db4` integration, which uses its own complete UI matrix.

## Limits

Local Rust/MSVC/Windows SDK are unavailable; native qualification is hosted, not formatting. Temporary synthetic credentials never inspect the user's store or real accessories. No real pairing/capture/endpoints or local MSI installation occurred. E is integrated in fork Main, not upstream; real hardware behavior remains unqualified.
