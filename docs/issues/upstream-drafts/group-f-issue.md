# Preserve process IPC rejection details and report actual equalizer preparation acceptance

## Current behavior

Reviewed original stable source: `cf420d97700463c56adc31408510e8336e99e51c`.

Native malformed/oversized command rejection emits a version-1 error with explicit empty request/session strings, `command_error`, `ipc`, null host and false retryability. Desktop playback/Pair filtering requires the active session id, so it discards this process-owned rejection. The original detail can be replaced by a later read timeout, and retry policy can then treat it as a different failure.

The equalizer mailbox separately ignores nonnewer sequence updates, but the command handler still emits `equalizer_changed` with metadata prepared from the submitted request. A stale or conflicting request can therefore receive success describing preparation that was not retained.

Relevant original source:

- [Error envelope](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/native/airflash-engine/src/main.rs#L22), [empty-correlated command rejection](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/native/airflash-engine/src/main.rs#L89).
- [Desktop session filtering](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.Core/SessionController.cs#L442-L454), [Pair reading, PIN wait and error handling](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.Core/SessionController.cs#L405-L423).
- [Mailbox ignores nonnewer updates](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/native/airflash-engine/src/equalizer.rs#L133), [handler replies from submitted preparation](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/native/airflash-engine/src/main.rs#L185).

## Reproduction and expected behavior

Use a fake owning engine connection to inject the exact empty-correlated rejection during startup, connecting, streaming and Pair before PIN is requested. Inspect the displayed failure, structured diagnostics, cleanup and process count with force reconnect enabled. Required behavior is the original IPC cause and false retryability, without an unrelated timeout retry. Near matches and ordinary stale-session events must retain normal filtering.

Use the production EQ handler and a synthetic stored streaming slot with an EQ mailbox: submit `10/A`, `9/B`, `10/B`, `10/A`, `11/B`. Assert serialized replies together with retained sequence/preparation. A stale update and same-sequence conflict must not claim success or alter the mailbox; an identical prepared-value replay is idempotent. Successful metadata must describe retained preparation.

These are deterministic parser/controller/mailbox cases. They establish conditional source defects, not the frequency of malformed commands or a receiver dropout cause.

## Bounded fix and acceptance

- Recognize only the defined process-owned rejection shape before ordinary session filtering, including startup hello. Preserve detail, code, channel and retryability through playback and Pair diagnostics; obsolete owners cannot record or publish faults.
- Convert malformed/non-object desktop output into a controlled final IPC protocol failure. Preserve cancellation and normal stale/version filtering, and keep EQ feedback nonfatal.
- Return mailbox outcome and retained preparation atomically: Accepted, Idempotent, Stale or Conflict. Prepared-value equality includes gain, bypass, coefficients and headroom.
- Retain compatible `equalizer_changed` for accepted/idempotent results. Use existing nonfatal `equalizer_error` for stale/conflicting requests, echoing the submitted sequence/message without success headroom fields. Preparation acceptance does not acknowledge DSP pickup or crossfade completion.
- Cover both sample rates, invalid structures/gains/sequences, wrong or absent sessions, pair/probe slots and a subsequent valid command. Actual receiver-free child parser tests must verify the existing 65,536-byte command limit including newline and recovery with hello/EOF.

## Verification and limits

Historical Core fixtures against unchanged stable runtime separate expected failures from healthy controls. A [source-equivalent EQ reply control](https://github.com/ThatMarko/AirFlash/actions/runs/38165179020) fails the decisive serialized reply assertion with library controls passing. Existing parser children start no receiver/capture worker. The prepared branch has been qualified independently at its exact commit below; combined-fork results are not substituted for its standalone evidence.

The Pair rejection fixture does not open a PIN prompt. While that prompt is awaited, the sole event reader is paused, so engine rejection/EOF may be consumed later or lose to cancellation/timeout; this task does not change that ordering. Worker liveness, symmetric IPC limits and generalized bounded desktop reads remain outside scope. A quiet-stream EQ fixture must keep the owned transport alive when testing ownership, and test read expiry separately; a later green run alone does not diagnose an intermittent failure.

## Prepared implementation and executed verification

The prepared implementation is [codex/fix-jsonl-control](https://github.com/ThatMarko/AirFlash/tree/421e1ea8159803fbc339dff3782b1858e8a797d4), commit `421e1ea8159803fbc339dff3782b1858e8a797d4`, independently based on the original stable source above. Independent final source/regression review found no unresolved findings.

The standalone Release build passed with zero warnings/errors. A fresh prepublication Release run passed all **239 Core tests** with no skips. Recorded separate isolated WPF runs passed **568 assertions each in 3 catalog/region combinations**, covering English, Simplified Chinese and Traditional Chinese. These are this branch’s results, separate from combined-fork qualification.

[Exact-source Windows validation](https://github.com/ThatMarko/AirFlash/actions/runs/38359129170) checked out this commit and passed **53 native tests**, Clippy, native Release build and the executable compatibility audit, plus the branch’s managed build/Core suite. Two existing tests remain ignored: recording-endpoint qualification and the thirty-minute localhost soak.
