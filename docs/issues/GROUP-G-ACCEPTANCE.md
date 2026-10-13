# Group G acceptance — owned local mute recovery

## Standalone preparation update — 2026-10-13

Prepared head `fcded6b2a211db2bade087a17bcd46b0dfabe24d` removes an unnecessary settings persistence wait introduced by the standalone Pair readiness wrapper. A real gated app save reproduced the failure on unchanged `96b9120` production; fixed Pair reaches its fake command before save release, preserves applied settings/recovery guards, permits Stop before pin_required and never mutes. Final standalone Release build and 287 Core tests, focused 66 recovery assertions and three full WPF runs × 629 passed. Main already had immediate Pair/synchronous B intent; integration retains that path and adds the regression. Final independent review is clear; all endpoints/stores are synthetic. [Draft index](upstream-drafts/README.md) separates this proof from the earlier qualification below.

## Current Main integration — 2026-10-12

Reviewed fork runtime baseline `8989b3609dd1294f4d1d6eb1502f345cc22bc382` contains this feature; desktop/native/tooling trees match qualified code `4ae1db4f01dcf7857482dd136ea8d9dffa3aa8a1`. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`. The public branch is pushed to origin and its complete diff/every outgoing commit exclude private planning. [Combined acceptance](CONTINUATION-ACCEPTANCE.md) binds final 667 Core, 124 Python, 69 native, portable UI/DPI and MSI results. No upstream publication is authorized by this documentation update.

Feature `codex/fix-mute-recovery`, head `96b9120116555f496d50ffc270ba54666b303534`; worktree `fix-mute-recovery/AirFlash`. Report 13 is complete. Independent source/assertion review is clear. Standalone owning Core suite passes 287 cases; focused UI passes 61 each in English/Simplified/Traditional Chinese; Release build has zero warnings/errors.

Current [coordinator](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/MuteRecoveryCoordinator.cs), [file journal](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/FileMuteRecoveryJournal.cs), [audio adapter](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Services/AudioService.cs) and [modeless recovery window](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Ui/MuteRecoveryWindow.cs) are integrated. Public [recovery contract](../MUTE-RECOVERY.md) defines durability/external-writer limits.

## Implemented ownership and recovery

Actual endpoint id and exact original true/false mute bit are recorded in a separate version-1 journal. Exclusive lifetime file lease, process GUID and acquisition generation establish cooperating ownership. Durable intent precedes endpoint mutation. Failed journal persistence does not prevent best-effort exact-bit cleanup. Repeated mute does not recapture/overwrite an unresolved original.

Busy, unreadable, unsupported, corrupt or unavailable state is handled conservatively. Modeless review runs outside audio/settings/lifecycle locks, revalidates choices and leaves Stop accessible. Already matching saved state requires no endpoint write; a differing state requires reviewed recovery/abandonment. A saved bit cannot prove that the user/another app made no subsequent change.

Known successful restoration followed by failed persistence permits a live-process saving retry without another endpoint write. That knowledge may be lost after restart; records remain ambiguous and cannot be blindly replayed. Pending entries preserve endpoint/prior-bit ownership. **They do not universally preserve unchanged serialized bytes:** live restoration state or settlement of another entry can persist a new journal revision.

## Recording-direction protection

The actual opened endpoint's direction is checked before mute Get/Set or new journal ownership, including before initialization and when catalog labels disagree. Recording inputs acquire no new mute obligation. Explicit requested-id mismatch rejects before direction/bit/journal access. Saved output obligations that open recording/unknown endpoints remain Pending without mute access; a valid playback endpoint returning permits reviewed exact-bit recovery.

While local muting is enabled, unresolved recovery conservatively blocks new admission even for a recording source. Disabling local muting permits operation without discarding recovery. This is the implemented admission policy, not a recording workaround.

## Decisive regressions and integration

Unchanged coordinator plus required direction/fake scaffolding fails 21 new regressions while 48 prior controls pass; corrected code passes. Two additional id-mismatch cases reject before direction/bit/journal access. The failed UI fixture read outside its own lifetime lease; capturing bytes while owned corrected it without runtime change.

Original historical `baf9e28` passed 48 coordinator/file cases, nine admission cases/256 owning Core, warning-free build, 609 full English UI and 45 narrow checks in each Chinese catalog. Fixtures cover both original bits, mutating setters that throw, intent/write/restore/persistence interruption, leases, endpoint replacement/disappearance/return, multiple entries, repeated recovery and unresolved state. Evidence remains in worktree `artifacts/group-g-2026-10-11/` and `artifacts/mute-admission-2026-10-11/`.

An initial combined merge inserted a settings-save wait before Pair, stalling B's existing delayed-save ownership fixture. Main restored synchronous explicit-intent capture and immediate Pair admission while preserving serialized recovery checks. Failed exact source/process/logs remain at `artifacts/continuation-2026-10-11/integrated-g/`; corrected combined 604 Core/1,010 UI receipt precedes final 667/1,059 qualification and is not its substitute.

## Limits and publication

All endpoints/stores/connections are fake, temporary and isolated. No real endpoint, user config or credential store was touched. Flushing/mocked interruptions do not establish physical reboot/power-loss durability or every COM/OS result. Normal restoration remains best effort after cleanup, without reconnect grace or a hard immediate deadline.

The journal stays separate from schema-2 preferences and credentials. G is complete in fork Main; original stable has not integrated it. Selected Task 3 publication must retain explicit durability limits and the recording protection/admission policy.
