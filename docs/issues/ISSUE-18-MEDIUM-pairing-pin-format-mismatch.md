# [MEDIUM] Pairing dialog accepts dashed PINs that the engine rejects

- **Issue ID**: ISSUE-18
- **Severity**: MEDIUM; an enabled pairing action can send a predictably rejected PIN
- **Kind**: Defect
- **Subsystem**: Desktop App / native authentication across JSONL v1
- **Status**: Open — source-confirmed validation mismatch
- **Runtime baseline**: upstream `41190e0`; documentation baseline `c077a05`
- **Evidence status**: Source verified and synthetic `12-34` predicates evaluated; no WPF/native pairing exchange or real receiver run performed
- **Implementation status**: Proposal only
- **Target files**: [PinDialog.cs:18–36](../../desktop/AirFlash.App/Ui/PinDialog.cs#L18), [SessionController.cs:412–422](../../desktop/AirFlash.Core/SessionController.cs#L412), [auth.rs:111–117](../../native/airflash-engine/src/auth.rs#L111)
- **Analysis context**: [Authentication](../analysis/auth.md), [Session pairing](../analysis/session.md), [Credentials and IPC](../analysis/credentials-and-ipc.md)

## Current behavior and effect

The PIN dialog permits digits and dashes while typing at [PinDialog.cs:18](../../desktop/AirFlash.App/Ui/PinDialog.cs#L18). Its Pair button enables when the text contains four through eight ASCII digits and every other character is a dash, at [line 22](../../desktop/AirFlash.App/Ui/PinDialog.cs#L22). The box permits up to sixteen total characters. Submission returns the text unchanged at [line 34](../../desktop/AirFlash.App/Ui/PinDialog.cs#L34).

For example, `12-34` enables Pair: it contains four digits and one permitted dash. The native validator rejects it because every byte must be an ASCII digit. The Settings command has therefore accepted a representation that cannot complete native pairing. This does not mean an all-digit PIN succeeds with a particular receiver; receiver authentication is a separate condition.

## Code path and root cause

[SessionController.cs:412–418](../../desktop/AirFlash.Core/SessionController.cs#L412) awaits the dialog and forwards its returned text directly in `pair_pin`. [main.rs:228–235](../../native/airflash-engine/src/main.rs#L228) sends that string to the pairing worker. After setup M2, [auth.rs:113–117](../../native/airflash-engine/src/auth.rs#L113) requires byte length 4–8 and `pin.bytes().all(is_ascii_digit)`, otherwise returning `PIN must contain 4..8 digits`. No layer strips separators.

The failure occurs before native SRP M3/proof generation for that supplied PIN, but after the pairing connection and M2 exchange have begun. The command-loop worker emits an error at [main.rs:305–310](../../native/airflash-engine/src/main.rs#L305); [SessionController.cs:422](../../desktop/AirFlash.Core/SessionController.cs#L422) exits pairing, so the following playback start is not sent. Persistent credential save is after successful authentication at [main.rs:293–297](../../native/airflash-engine/src/main.rs#L293), and is not reached for this validation rejection. Pair-verify/transient playback selection is not the cause.

## Minimal deterministic reproduction

The audited predicates were evaluated using synthetic string `12-34`: desktop enable predicate **true**, native-equivalent byte/digit predicate **false**. This is a predicate reproduction, not an executed UI/accessory protocol test.

A future fixture should show the actual path without hardware:

1. In the WPF STA harness, set the PIN textbox to `12-34` and verify Pair currently enables.
2. Use a mocked PIN request/pairing connection or localhost synthetic pairing M2 response; capture the submitted text.
3. Assert the current unchanged string fails the native validator and playback/credential save does not follow.
4. Test typing and paste with four/eight digits, permitted dashed forms, too few/many digits, letters, whitespace, Unicode digits, and cancel. Preserve cancellation/timeouts separately.

Do not record or expose a real accessory PIN; synthetic values suffice.

## Proposed implementation and acceptance

Normalize permitted dash separators before both enable validation and submission, then require four through eight ASCII digits in the normalized value. Alternatively remove separator support consistently from typing, paste validation, and enabling. Normalization is the proposed default because the existing UI intentionally allows dashes; the native strict digit-only contract should remain unchanged.

- Every enabled submission sends exactly 4–8 ASCII digits; `12-34` sends `1234` under the normalization proposal.
- Paste cannot bypass validation, Unicode digits/letters are rejected, and empty/cancelled results retain existing cancellation behavior.
- Too many separators/characters cannot produce an enabled action with an invalid normalized PIN.
- Engine PIN validation remains strict; no downgrade/fallback authentication or credential format change is introduced.
- UI and protocol fixtures verify the transmitted string and the rejected-input path without real pairing or playback.

## Existing and missing tests

[SessionTests.cs:226–238](../../desktop/AirFlash.Tests/SessionTests.cs#L226) tests blank cancellation and pairing two members using already valid digit strings, not dialog separators. [auth.rs:438–451](../../native/airflash-engine/src/auth.rs#L438) tests authenticated persistent setup/signature rejection using `1234`, not UI formatting. No existing test was found for dashed PIN normalization or agreement between the dialog and native validator. JSONL v1 and schema 2 need no change. No verified causal mapping to upstream issue numbers is claimed.
