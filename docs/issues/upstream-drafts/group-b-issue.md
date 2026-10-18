# Preserve receiver selection and session ownership through delayed desktop work

This is a proposed update to the existing [receiver/session issue #14](https://github.com/Ding-Kyoma/AirFlash/issues/14), rather than a duplicate issue. The source baseline is original stable main at `cf420d97700463c56adc31408510e8336e99e51c`.

## Current behavior

[PR #19](https://github.com/Ding-Kyoma/AirFlash/pull/19) already preserves an owned attempt through passive discovery loss and rereads the session after identity persistence. Those improvements should remain. The following selection and authority gaps remain in the verified stable source:

- Discovery reconciliation rereads the current snapshot after an awaited save, then calls unconditional `UpdateReceiverAsync`. A session can be replaced again while that operation waits for controller serialization; the request carries no desktop lifecycle owner or explicit-intent token. A receiver id or a snapshot-reference check cannot identify that owner.
- Automatic connection checks inactivity/suppression before calling the same Toggle path as an explicit click. Toggle awaits persistence and the settings gate, then starts unconditionally. A newer Pair, Play or Stop can be superseded by that delayed request.
- Candidate ranking selects the preferred receiver before checking whether it was already attempted. An attempted preferred receiver can therefore prevent an eligible unattempted receiver from being considered.
- Explicit selection redirects to the first discovered stereo group sharing the selected address, including a manual row with a different port. Catalog coexistence does not make that later substitution safe.
- An owned active row still passes the hidden-options filter, so hiding it can remove its panel Stop control despite the retained attempt.
- The shell's Quit action sets its own quitting flag, then awaits Settings Apply and engine cleanup before disposing the application view model. That order does not synchronously close playback admission: delayed automatic work or an accepted settings update can reach runtime control while Quit is waiting. An earlier Apply/close/engine failure also skips view-model cleanup in this source path.

Source anchors: [reconciliation and active-row presentation](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L227-L319), [Toggle and automatic selection](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L358-L391), [controller receiver update](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.Core/SessionController.cs#L246-L259), and [shell Quit ordering](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.App/App.xaml.cs).

## Reproduction and observation boundary

These are source-derived schedules against the stable baseline, suitable for deterministic fake-service fixtures:

1. Hold controller serialization and queue a newer Pair/start while the old snapshot remains current. Let reconciliation reread that snapshot and queue its update behind the replacement, then release serialization. The delayed update must not touch the newer receiver, settings, worker or diagnostics, including when the receiver id is reused. Separately gate identity saving to cover replacement during persistence and valid same-owner promotion.
2. Start automatic work with a dirty setting and hold its save. Invoke Pair, Play or user Stop, then release the save. The older automatic request must not replace the newer decision or clear Stop suppression.
3. Mark preferred receiver A attempted while eligible receiver B remains unattempted. Publish an otherwise unchanged catalog; B should still be selectable.
4. Use synthetic manual `192.0.2.10:7001` beside a discovered stereo member at `192.0.2.10:7000`. Assert the manual click sends exactly its single requested peer/port and retains its identity/options.
5. Hide an owned active row and exercise the actual retained Stop control. Keep modal PIN accessibility separate from command enablement.
6. Hold a real Settings Apply save, queue an automatic request behind it, then enter the shell shutdown sequence and release persistence while engine cleanup remains held. Accepted edits must still save, but no Start, mute or replacement may occur after Quit has closed admission. Separately retain an existing transport and invoke its bound Stop during the held cleanup.

Earlier isolated fixtures on original `41190e0` reproduced delayed-control, automatic-selection and manual-target failures. The current stable source review confirms the remaining contracts above, while PR #19 has already removed the old unconditional passive-discovery Stop. No new real-receiver reproduction, household dropout cause or native authentication cause is claimed.

The Quit schedule above is source-derived from the stable shell. Its executed failing control extracted that original shutdown order into the shared helper while retaining the prior feature's application view model unchanged. Using a gated real Settings Apply and held fake engine cleanup, it failed the first no-Start/no-mute assertion after the queued-request control passed. This is a controlled extraction, not an execution of the untouched original application or a real endpoint observation.

## Expected behavior and bounded fix

Capture snapshot, desktop lifecycle owner and explicit intent atomically, then validate delayed control under controller serialization. That owner survives metrics and native-process retries, while desktop replacement changes it. Preserve legitimate same-owner identity promotion and applicable signature-driven restart.

Separate automatic admission from explicit Toggle. Capture intent before persistence; invalidate older automatic intent at explicit action entry and at shell Quit before its first await; revalidate the canonical candidate, availability, completeness, visibility, options and attempts under the application settings gate through serialized admission. Preserve Error as eligible inactivity. Only admitted attempts affect attempted/last-used state; an admitted connection failure still counts. Initial stale rejection must not enter cleanup; rejection after older-worker cleanup has begun may finish that cleanup but must not start a replacement or control a newer owner.

The actual Quit action uses a shared shutdown helper that closes admission synchronously, then waits for accepted Apply work, closes Settings, disposes engine initialization and disposes the view model. Accepted Apply and dirty saves may commit their preferences while closing, but skip runtime settings updates. Keep the owned transport's Stop usable and publish its resulting Idle/disabled state while cleanup is held. Attempt view-model cleanup despite failures in earlier helper stages; preserve a single exception's identity or aggregate multiple failures in stage order after cleanup.

Filter attempted candidates before ranking. Keep explicit manual endpoints and selected groups; discovered member redirection requires a unique confirmed canonical/broadcast owner, without address-only substitution. Retain truthful offline/incomplete availability and accessible controls for an owned attempt. New unavailable/incomplete starts still reject.

## Acceptance criteria

- Controlled save/serialization races cannot retarget, stop or alter a newer lifecycle, including same-id replacement and metrics published during delay.
- Eligibility changed during persistence rejects even without an owner change; multiple queued requests admit at most one current eligible request.
- Stop suppression, successful persistence and admitted-only bookkeeping survive rejected work.
- Attempted A does not block eligible B; aliases, overrides and permitted reset transitions remain covered.
- Manual shared-address/different-port selection, complete/incomplete/hidden groups and ambiguous ownership produce exact or rejected commands as appropriate.
- All retained active states survive empty/partial/repeated discovery and selected-adapter browse restart; explicit Stop, cancellation and terminal faults still clean up with exact fake prior mute restoration.
- Shell Quit closes admission before waiting for Apply or engine cleanup. Pending valid saves remain committed without a new/replacement transport or rejected-attempt bookkeeping; new Play, Pair and automatic requests cannot start while closing.
- The owned receiver's bound Stop remains usable during held shell cleanup, then displays Idle with stopped commands disabled. Apply, window-close and engine-cleanup failures still reach view-model cleanup and exact restoration of either prior mute bit, with original exception identity or ordered aggregation retained.
- Core controller/selection assertions and a separate fake-service WPF harness verify observable commands and accessible controls. No real credentials, endpoints or receivers are required.

JSONL v1, configuration schema 2, accessory-keyed credentials and selected-adapter no-fallback remain compatible. Terminal events during an already open PIN prompt remain a separate source limit, outside this bounded issue.

## Prepared implementation and executed verification — 2026-10-18

The current prepared implementation is [codex/fix-session-lifecycle](https://github.com/ThatMarko/AirFlash/tree/d7782a3d069f996b6d1c7c2fe65b224b6f13aa8e), commit `d7782a3d069f996b6d1c7c2fe65b224b6f13aa8e`, independently based on the original stable source above. Independent final source/regression review is clear. This branch is pushed to the fork; PR #15 remains closed/unmerged and has not been reopened.

On the current source, **SDK 10.0.401 and 10.0.402** each pass forced locked restore, a Release solution build with zero warnings/errors, **304 Core / 103 focused tests** and **51 focused shell WPF assertions**. Lock hashes remain unchanged and the ILLink 10.0.12 analyzer is consumed. Six full WPF runs on SDK 10.0.402 pass **810 assertions each**, with observed English/Simplified Chinese/Traditional Chinese catalogs crossed with en-US/zh-CN formatting; an additional English/en-US run on SDK 10.0.401 also passes **810**. These are standalone feature results, separate from combined-fork qualification; fake services establish no physical receiver or endpoint behavior.

### Earlier preparation retained

The 2026-10-13 preparation at `442e8d8c8fd7649e3e039ec356e3a4f4b25f6d0b` passed a warning-free Release build, **304 Core tests** and **764 WPF assertions each in six catalog/region combinations**. The 2026-10-18 SDK consistency head `002c4e133e43f8332015b4bde44d7a75660eedcc` separately passed **304 Core / 166 focused tests** and two observed full WPF cells of **764** on SDK 10.0.402. Those historical counts retain their original source and filter provenance; they are not relabelled as current shell-shutdown qualification.
