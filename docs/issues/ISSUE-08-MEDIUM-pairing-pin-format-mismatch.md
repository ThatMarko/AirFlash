# [MEDIUM] Pairing PIN formatting — resolved in fork main

**Status:** Implemented and integrated in own-fork main `8989b3609dd1294f4d1d6eb1502f345cc22bc382`, reviewed 2026-10-12. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`; this implementation is not yet integrated there. Severity is the original triage priority, not an unqualified current hardware failure.

**Group:** E — Authentication inputs. **Feature head:** `0ebc59bd99e5ca9d1c7219a7b57bc9bb2b237c5a`. [Plan](IMPLEMENTATION_PLAN.md#issue-08), [group acceptance](GROUP-E-ACCEPTANCE.md), [authentication map](../analysis/auth.md).

## Current behavior and source trace

[PairingPin.Normalize](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/PairingPin.cs#L3) removes permitted ASCII dashes, requires 4–8 ASCII digits and enforces the 16-character editing bound. [PinDialog](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Ui/PinDialog.cs#L10) uses it for both button enablement and returned submission; typing filters characters and paste cannot bypass submission validation.

[PairMembersAsync](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs#L561) forwards that value through actual `pair_pin`. [Pair worker](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/main.rs#L288) receives it; [native strict validation](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/auth.rs#L113) remains 4–8 digit bytes. Synthetic `12-34` therefore submits `1234`, while letters, whitespace, Unicode characters/digits and invalid normalized lengths reject. Single-line WPF paste can truncate a newline suffix before normalization; acceptance records the resulting textbox prefix separately from direct invalid-text replacement. Cancel/token timeout retain their existing distinct cleanup outcomes.

## Acceptance and evidence

- Shared enable/submission predicate; every enabled payload contains exactly 4–8 ASCII digits.
- Actual fake pairing payload, editing/paste boundaries, native strict validation, cancel/timeout and no following playback/save on cancellation are covered.
- Credential identity/format, JSONL v1, config schema 2 and authentication selection remain unchanged.

Current [Core normalization cases](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/PairingPinTests.cs#L27) and [modal WPF fixture](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiPairingInputs.cs) assert observable behavior. Group E acceptance separately pins earlier hosted source/run with 225 modal assertions, including disposable-runner paste that restores clipboard in finally, and the decisive baseline failure. Those results are not relabelled as a fresh main run.

Exact-main CI passed 667 Core, 124 Python and 69 native tests (recording-device qualification and thirty-minute localhost soak ignored). Packaged integration at `4ae1db4` passed six 1,059-assertion UI runs, three actual 125% DPI runs with 40 each and disposable MSI lifecycle; current runtime/package inputs are identical, with main's last commit documentation only. This refresh reviewed sources/records; it did not rerun suites or use real user data/devices.

## Remaining work

Upstream publication remains separately authorized. Real receiver PIN policy/success is unqualified; normalizing a valid representation does not guarantee authentication.

## Historical finding

At original `41190e0`, the dialog accepted digits/dashes but returned the text unchanged, while native validation rejected any dash before SRP M3. The audited synthetic predicates for `12-34` were desktop true/native false; this was not a real receiver reproduction. The [archival report at the prior documentation commit](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/issues/ISSUE-08-MEDIUM-pairing-pin-format-mismatch.md) retains the original `41190e0` source links, source-derived reproduction/proposal and separately dated implementation updates. Its open/proposal-only/pending-reboot labels are historical; current acceptance is above.