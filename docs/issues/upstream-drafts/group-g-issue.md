# Retain local endpoint mute ownership across an interrupted desktop process

The source baseline is original stable main at `cf420d97700463c56adc31408510e8336e99e51c`.

## Current behavior

[AudioService.MuteAsync](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.App/Services/AudioService.cs#L63-L72) captures a playback endpoint's mute bit, sets mute, then stores the original only in an in-memory dictionary. Normal [Restore/disposal](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.App/Services/AudioService.cs#L74-L105) can attempt the exact prior bit while that desktop process remains alive.

If the desktop is interrupted after mutation, the next process has no retained endpoint/prior-bit ownership information. A setter that changes the endpoint and then throws also occurs before that dictionary entry is written. The Rust worker and WPF process are separate, so engine Stop alone does not persist desktop mute ownership.

This establishes an information/ownership gap. It does not establish that every crash or reboot leaves speakers muted, or that local muting caused a receiver disconnection.

## Reproduction and observation boundary

A deterministic receiver-free fixture can use a fake playback endpoint with original mute false, interrupt after mutation, and construct a new service/coordinator over the same temporary store. Repeat with original mute true. Without retained intent, the new instance cannot recover the exact owned original from the live dictionary.

Additional fixtures can make the setter mutate then throw, fail journal writes/restores, change actual endpoint direction/id, hold another owner's lease or return an unavailable endpoint. Existing synthetic coordinator/file/admission/UI tests execute those interruption and failure boundaries. They do not demonstrate real Windows mute persistence after process kill, reboot or power loss, and no real-device reproduction is required for this issue.

## Expected behavior and bounded fix

Retain actual endpoint identity and the exact original true/false bit in a separate owned recovery journal before endpoint mutation. Establish exclusive cooperating ownership using a lifetime lease and an owner GUID/generation, rather than a PID. An in-memory obligation must also precede a setter that may mutate before throwing.

Normal cleanup continues best-effort exact-bit restoration after worker/process cleanup, including when journal writes fail. Repeated mute must not overwrite an unresolved original. The journal remains separate from preferences and credentials.

Recovery must conservatively distinguish actively owned, pending, restored, corrupt/unsupported/unreadable and unavailable records. A saved bit cannot prove no later user/application change. A differing stale bit therefore needs an explicit revalidated Restore saved/Keep current/Later choice, outside audio/settings/lifecycle locks; Stop must remain accessible. Successful restore followed by failed persistence is known while alive and permits a persistence-only retry; after restart that knowledge may be lost and must remain ambiguous.

Actual opened endpoint direction/id governs access. Recording inputs must never read/write endpoint mute or acquire a new obligation, even when catalog labels disagree. Saved output obligations that open recording/unknown endpoints remain pending. Unresolved recovery blocks admission with local muting enabled; disabling that option permits playback without discarding records.

## Acceptance criteria

- Both original mute bits survive every controlled acquisition/mutation/restore/persistence interruption with an explicit, conservative outcome.
- Intent persists before mutation; a mutating setter failure still receives best-effort tracked exact-bit cleanup.
- Repeated acquisition, multiple entries, lease conflicts and unavailable endpoints cannot overwrite an unresolved original.
- Known restore plus failed Save retries persistence without another endpoint write while alive; restarted ambiguity is not blindly replayed.
- Corrupt, unsupported, unreadable and changed journals remain preserved or blocking; an explicit unsupported-record archive preserves exact bytes before consumption.
- Explicit id mismatch rejects before direction/bit/journal access; opened recording/unknown direction never authorizes mute access or new ownership.
- Modeless review revalidates revision/owner/generation/endpoint/observed state and leaves Stop available; no interaction is held across audio/settings/lifecycle locks.
- Controller admission checks recovery before/after cleanup and before retry; read-only enumeration and diagnostic construction do not initialize recovery.
- Explicit Pair keeps using the applied settings and remains available while an unrelated settings save is pending; adding recovery admission must not make it await that persistence gate.
- Temporary fake endpoint/file/coordinator, Core admission and separate WPF MTA/review assertions verify observable state and failure paths.

Keep normal restoration best effort, without reconnect grace or a hard immediate-completion deadline. Flushing/replacement requests durability but does not prove physical storage behavior. The lease coordinates cooperating AirFlash instances, not arbitrary external writers. Schema 2, JSONL v1 and accessory-keyed encrypted credentials remain unchanged.

## Prepared implementation and executed verification

The prepared implementation is [codex/fix-mute-recovery](https://github.com/ThatMarko/AirFlash/tree/fcded6b2a211db2bade087a17bcd46b0dfabe24d), commit `fcded6b2a211db2bade087a17bcd46b0dfabe24d`, independently based on the original stable source above. Independent final source/regression review found no unresolved findings.

Fresh standalone Release build passed with zero warnings/errors; all **287 Core tests** passed with no skips. Separate isolated WPF runs passed **629 assertions each in 3 catalog/region combinations**, covering English, Simplified Chinese and Traditional Chinese. These are this branch’s results, separate from combined-fork qualification.

A new gated app test failed with unchanged branch production code because explicit Pair waited behind unrelated settings persistence. The corrected Pair path preserves readiness/recovery checks and applied settings while reaching the fake pair command before save release; explicit Stop remains available before any PIN prompt. The focused recovery harness now passes **66 assertions**; all endpoints and journal data remain synthetic.
