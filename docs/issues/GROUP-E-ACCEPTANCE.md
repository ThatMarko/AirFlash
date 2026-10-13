# Group E acceptance — PIN input and credential lookup

## Current SDK correction — 2026-10-13

Current branch head `234167b31092339b2e909bfa2cbee5748b6006e1`, independently based on original `cf420d97700463c56adc31408510e8336e99e51c`, includes the two-file shared analyzer-reference/CI logging correction. [SDK acceptance](SDK-LOCK-ACCEPTANCE.md) explains the executed unchanged-base control and preserved lock/SDK/analyzer contracts. [PR #25](https://github.com/Ding-Kyoma/AirFlash/pull/25) remains open and unmerged.

Fresh selected SDK 10.0.402 locked restore, warning-free Release solution build and **223 Core tests** pass with unchanged lock hashes and the 10.0.12 analyzer actually consumed. [Complete upstream CI 38461050044](https://github.com/Ding-Kyoma/AirFlash/actions/runs/38461050044) explicitly logs 10.0.402 and passes **223 Core / 59 native**, with two existing native ignores, discovery stability, bundle audit and packaged startup/compatibility. Earlier publication, WPF/modal/MSI and source/control results below retain their original commits; they are not relabeled as new executions. The shared fix changes no group runtime policy.

## Upstream publication — 2026-10-13

Explicit approval published [issue #24](https://github.com/Ding-Kyoma/AirFlash/issues/24) and ready-for-review [PR #25](https://github.com/Ding-Kyoma/AirFlash/pull/25), targeting original `main` from `ThatMarko:codex/fix-auth-input` at `7dfcbb27331c700846c552de28e8eef882c57cf6`. Original stable remains `cf420d97700463c56adc31408510e8336e99e51c`. Fresh open/closed issue/PR search found no matching PIN/credential-lookup root cause; connection timeout and session-authority reports are separate. Published titles/bodies, `Fixes #24`, nine changed files and five commits match the reviewed package. Complete diff and every outgoing commit exclude fork-only planning. Independent native/desktop/regression/evidence and final text reviews are clear.

Fresh prepublication Release Core verification passed **223 tests**, no failures/skips, with selected SDK 10.0.401 and locked restore. Recorded exact-head standalone Release build and three **686-assertion** WPF runs retain their source provenance. Exact-source hosted run 38359126296 is still successful; its raw-log hash and source manifest were rechecked: source `7dfcbb2` differs from workflow `f181e120`, native **59 passed / two ignored**, managed **223 passed**, and actual modal paste **225 assertions**. This is not new local native or clipboard execution. [Upstream PR CI](https://github.com/Ding-Kyoma/AirFlash/actions/runs/38450769820) is tracked separately; publication does not merge the code into original stable. No release/reservation, real credential/configuration/receiver/endpoint access or other-group publication occurred. The issue body saved in [the kit](upstream-drafts/group-e-issue.md) matches the published text.

## Standalone preparation update — 2026-10-13

Prepared head `7dfcbb27331c700846c552de28e8eef882c57cf6` is integrated into code Main `892a458` and normally pushed. New gated PIN deadline and lifetime Stop cases prove Error after joined cleanup versus Idle cancellation, without pair_pin/start/subsequent playback. The first added fixture failed because its delegate-scoped registration could be disposed before cancellation observation; retaining that registration until finally joins cleanup fixed the fixture without changing production policy. Final standalone Release build/223 Core tests and three full WPF runs × 686 passed. [Exact-source Windows validation 38359126296](https://github.com/ThatMarko/AirFlash/actions/runs/38359126296) passed 59 native tests (two ignored), Clippy/build/audit and 225 modal paste assertions. [Draft index](upstream-drafts/README.md) and ignored preparation receipts preserve exact source/hash/control evidence. Earlier results below remain source-bound.

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

Local Rust/MSVC/Windows SDK are unavailable; native qualification is hosted, not formatting. Temporary synthetic credentials never inspect the user's store or real accessories. No real pairing/capture/endpoints or local MSI installation occurred. E is integrated in fork Main and published as issue #24 / PR #25; original stable integration remains pending maintainer review, and real hardware behavior remains unqualified.
