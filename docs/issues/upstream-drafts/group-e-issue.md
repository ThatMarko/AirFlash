# Normalize accepted pairing PINs and preserve credential lookup errors

## Current behavior

Reviewed original stable source: `cf420d97700463c56adc31408510e8336e99e51c`.

The PIN dialog accepts ASCII digits and dashes, enables Pair when there are four to eight digits, and returns the original text. Native pairing requires four to eight digit bytes. A synthetic input such as `12-34` is therefore enabled by the desktop but rejected by native validation.

Credential loading separately uses `Path::exists()` before opening the accessory's file. A metadata/access failure can be collapsed to false and returned as credential absence. Playback then selects transient pair-setup. Failures after a successful existence probe already propagate, and pair-verify failure already does not fall back to transient setup; the defect is the uncertain-presence classification.

Relevant original source:

- [PIN editing and enablement](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.App/Ui/PinDialog.cs#L18-L23), [unchanged submitted text](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.App/Ui/PinDialog.cs#L31-L36).
- [Strict native PIN validation](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/native/airflash-engine/src/auth.rs#L113-L117).
- [Boolean existence probe and credential loading](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/native/airflash-engine/src/credentials.rs#L105), [authentication selection](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/native/airflash-engine/src/session.rs#L229).

## Reproduction and expected behavior

Use a real modal dialog with a fake pairing connection: enter `12-34`, submit Pair and inspect the actual `pair_pin` payload. The required payload is `1234`. This is a deterministic input/command fixture, not a real accessory pairing observation.

Use injected credential-open/read failures and a temporary synthetic store. Initial NotFound should establish absence; PermissionDenied/other open errors and every read-stage failure should remain errors and initiate no authentication exchange. Valid saved credentials should select verify exactly once; genuine absence should select transient exactly once.

## Bounded fix and acceptance

- Use one ASCII normalizer for button enablement, final submission validation and the returned PIN: remove permitted ASCII dashes, then require four to eight ASCII digits within the existing editing bound. Keep native validation strict.
- Cover typing, selection replacement, paste, leading zeros, length boundaries, letters, whitespace, Unicode digits/characters, Cancel and token timeout. Assert actual fake command payloads and no following playback after cancelled pairing. Single-line WPF paste may truncate a newline suffix before validation; distinguish that textbox result from directly supplied invalid text.
- Open the credential file once and read that opened object with a finite size bound. Only initial open NotFound selects absence. Preserve original open/read/DPAPI/format/JSON/identity errors; failed verify must never select another authentication method.
- Preserve current-user DPAPI, the existing versioned file format, accessory `/info deviceID` keys, normalization and atomic replacement. Do not introduce plaintext backups or catalog-key migration.
- Verify synthetic saved-file replacement, path disappearance after opening, oversize data, tampering, malformed protected payloads and both loaded-credential/absent method selections.

## Verification and limits

Existing receiver-free modal and native loader/authentication fixtures provide the acceptance paths. A [source-equivalent error-hiding lookup control](https://github.com/ThatMarko/AirFlash/actions/runs/38162296485) fails the decisive initial-open assertion with controls passing; it is a mutation, not literal execution of the unchanged loader. Historical modal and Windows-native checks remain bound to their recorded source commits. Final standalone qualification must test the exact prepared branch; combined-fork suites/packages do not prove this branch independently.

Missing/non-string/empty playback `/info deviceID` retains the existing no-lookup transient path; that identity policy is outside this fix. Preserving storage errors does not promise every error is classified as `authentication_failed`. Prompt-open engine fault ordering, general IPC reader/liveness changes and real accessory PIN policy are separate concerns. No real credential store, receiver or pairing exchange is required for these tests.

## Prepared implementation and executed verification

The prepared implementation is [codex/fix-auth-input](https://github.com/ThatMarko/AirFlash/tree/7dfcbb27331c700846c552de28e8eef882c57cf6), commit `7dfcbb27331c700846c552de28e8eef882c57cf6`, independently based on the original stable source above. Independent final source/regression review found no unresolved findings.

Fresh standalone Release build passed with zero warnings/errors; all **223 Core tests** passed with no skips. Separate isolated WPF runs passed **686 assertions each in 3 catalog/region combinations**, covering English, Simplified Chinese and Traditional Chinese. These are this branch’s results, separate from combined-fork qualification.

[Exact-source Windows validation](https://github.com/ThatMarko/AirFlash/actions/runs/38359126296) checked out this commit and passed **59 native tests**, Clippy, native Release build and the executable compatibility audit, plus the branch’s managed build/Core suite. Two existing tests remain ignored: recording-endpoint qualification and the thirty-minute localhost soak.

The exact-source hosted modal paste harness passed **225 assertions**. Two controlled PIN-deadline/lifetime-Stop cases distinguish Error after joined cleanup from Idle cancellation, with no PIN/start command or subsequent playback. Their initial failure was a fixture cancellation-registration lifetime race; the corrected signal remains registered until cleanup is joined, without changing production classification.
