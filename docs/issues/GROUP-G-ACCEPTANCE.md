# Group G acceptance — local mute recovery

## Current stable-base qualification — 2026-10-11

Current public head `96b9120116555f496d50ffc270ba54666b303534` is pushed against stable `cf420d9`. Independent source/assertion review is clear. Current owning Core suite passes 287 tests; Release desktop build has zero warnings/errors. Focused WPF runs pass 61 checks in English, Simplified Chinese and Traditional Chinese catalogs, with actual cultures recorded. The unchanged coordinator plus required direction/fake scaffolding fails 21 new regressions while 48 earlier controls pass; corrected production code passes. Two further explicit-ID regressions prove a valid but different returned ID rejects before direction, bit or journal access.

The actual opened endpoint determines direction before any mute getter/setter or new journal ownership. Recording inputs are excluded, including before initialization and when catalog labels disagree. Saved output obligations that open a recording/unknown endpoint remain pending with endpoint/prior-bit ownership preserved and no mute access; valid playback return permits reviewed recovery. Known journal state may change during a live restoration attempt or settlement of another entry; exact-byte claims apply only to the specific unchanged inspection/preservation fixtures. While local muting is enabled, unresolved recovery conservatively blocks admission even for recording capture; disabling local muting permits operation without discarding recovery. Fakes do not establish physical reboot/power-loss persistence or whether the user changed an endpoint after a crash. The failed new UI fixture read outside its lifetime lease is retained; it was corrected by capturing bytes while the lease was owned, without changing runtime behavior.

These current sections supersede earlier pending-reboot, unpushed-feature and proposal wording. Combined fork results and publication limits are recorded in [continuation acceptance](CONTINUATION-ACCEPTANCE.md). The dated evidence below retains its original source boundary.

## Earlier implementation evidence

The public feature is `codex/fix-mute-recovery`, commit
`baf9e28fa1738ed28fb3d18124f380c2f650ebb3`, based on original stable
`91698d8cd3bdf5666b417a12eaaf8d6a1ea36363`. It is pushed to the fork,
reviewed independently, and locally verified. Upstream publication remains
unauthorized; the original repository has not integrated this feature.

The journal preserves the actual endpoint identity and both possible prior mute
bits. An exclusive lifetime file lease, process GUID and acquisition generation
establish cooperating ownership. Durable intent precedes endpoint mutation;
cleanup still tries exact-bit restoration after persistence failures. Pending,
busy, unreadable, unsupported and unavailable recovery blocks new muting attempts.
The modeless review window revalidates choices and keeps Stop accessible. A live
process that restored successfully but failed to persist offers a saving retry,
without another endpoint write. That knowledge is lost after restart; a saved bit
alone does not establish that no later user or application change occurred.

Acceptance evidence is preserved in the feature worktree under
`artifacts/group-g-2026-10-11` and `artifacts/mute-admission-2026-10-11`:

- 48 coordinator/file tests and nine controller admission tests pass, including
  both original bits, mutating setters that throw, journal failure windows,
  exact-byte preservation, leases, endpoint replacement/unavailability,
  repeated restoration and unresolved recovery.
- The owning Core suite passes 256 tests; the locked Release solution build has
  zero warnings and errors.
- The complete English mock UI harness passes 609 checks. Focused Simplified and
  Traditional Chinese runs pass 45 checks each with the actual catalog recorded.
  These cover the MTA/dispatcher boundary, real bindings, recovery actions,
  Pair admission, visible Stop and shutdown using fake endpoints and stores.
- The decisive replacement regression failed with a stopped worker still shown
  as Streaming before the fix. Initial fixture and compilation failures remain
  separate from the corrected receipts.
- Independent source and regression review findings are resolved. In particular,
  completed-but-unsaved restoration offers persistence only, and Settings Pair
  reflects blocked admission.

Fork integration requires retaining Group B's synchronous explicit-intent capture
and immediate Pair admission. An initial merge added a settings-save wait to Pair;
the existing gated ownership fixture stalled. Its exact source, process metadata
and logs are retained under main's `artifacts/continuation-2026-10-11/integrated-g`.
Restoring immediate Pair admission while keeping serialized recovery checks passes
the combined 604-test Core suite and 1,010-check English UI run with Chinese
regional formatting. This receipt is integration evidence, not original-main
acceptance; final integration review and subsequent Group H checks are tracked in
the continuation acceptance page.

All fixtures use synthetic endpoints, temporary files, mock connections and
isolated application output. No actual endpoint, user configuration or credential
store was used. File flushing and mocked interruption do not prove physical reboot
or power-loss durability. The public [recovery contract](../MUTE-RECOVERY.md)
documents the durability and external-writer limits.
