# [MEDIUM] Owned local mute recovery — implemented in fork main

**Status:** Implemented and integrated in own-fork main `8989b3609dd1294f4d1d6eb1502f345cc22bc382`, reviewed 2026-10-12. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`; this implementation is not yet integrated there. Severity is the original triage priority, not an unqualified current hardware failure.

**Group:** G — Owned mute recovery. **Feature head:** `96b9120116555f496d50ffc270ba54666b303534`. [Plan](IMPLEMENTATION_PLAN.md#issue-13), [acceptance](GROUP-G-ACCEPTANCE.md), [control/mute map](../analysis/control-and-mute.md), [recovery contract](../MUTE-RECOVERY.md).

## Current behavior and source trace

[AudioService](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Services/AudioService.cs#L29) serializes [recovery/endpoint operations](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/MuteRecoveryCoordinator.cs#L88) on MTA. A separate version-1 journal holds actual endpoint id, exact prior true/false bit, owner GUID/generation under [exclusive lifetime file lease](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/FileMuteRecoveryJournal.cs#L14). Intent is persisted before mutation, memory ownership before a setter that can mutate then throw.

Normal restoration attempts the exact live-owned bit even after journal failure. Known completed restore/failed save retries persistence only while alive; after restart the old state is ambiguous. Stale differing bits require explicit revalidated Restore saved/Keep current/Later, through [modeless review](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Ui/MuteRecoveryWindow.cs#L10) outside locks with Stop accessible.

Actual opened direction/id wins over catalog metadata. Explicit different id rejects before direction/bit/journal access. Recording never reads/sets mute or gains new ownership; a saved output now Recording/unknown/unavailable retains pending identity/prior-bit obligation. Valid journal bytes can change during live Restoring/mixed-entry persistence; exact-byte preservation is scoped to unsupported archival/unchanged-inspection cases.

Muted-start admission checks before/after cleanup and before retry; unresolved recovery conservatively blocks enabled local-muting even for Recording. Disabling muting permits playback without discarding records. Constructor/enumeration/read-only checks do not initialize recovery.

## Acceptance and evidence

- Original false and true bits, repeated/multiple ownership, actual id/direction, durable intent and every meaningful interruption have controlled results.
- Lease conflicts, corrupt/unsupported/unreadable/changed journals and unavailable outputs never silently become absence/new ownership.
- Restore/save/COM failures preserve obligations; recording return to Playback can later recover.
- Modeless UI, notices, MTA/dispatcher integration and before-engine admission use actual fake service boundaries.

[coordinator/file fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/MuteRecoveryTests.cs#L15), [admission fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/MuteRecoveryAdmissionTests.cs#L15) and [WPF service/review fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiMuteRecovery.cs) use temporary stores/synthetic endpoints only.

Exact-main CI passed 667 Core, 124 Python and 69 native tests (recording-device qualification and thirty-minute localhost soak ignored). Packaged integration at `4ae1db4` passed six 1,059-assertion UI runs, three actual 125% DPI runs with 40 each and disposable MSI lifecycle; current runtime/package inputs are identical, with main's last commit documentation only. This refresh reviewed sources/records; it did not rerun suites or use real user data/devices.

## Remaining work and limits

Physical endpoint persistence after process kill/reboot/power loss is unqualified. Flush/replace requests durability but cannot prove storage hardware behavior. A saved bit cannot establish no subsequent user/application change, recover a bit lost before journal introduction, or promise immediate normal restoration. Upstream publication remains separately authorized.

## Historical finding

Original `41190e0` held prior bits only in memory after successful mute, so forced loss discarded ownership information. Source proved that information gap, not that every crash/reboot permanently muted speakers or caused receiver disconnections. The [archival report at the prior documentation commit](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/issues/ISSUE-13-MEDIUM-endpoint-mute-crash-recovery-gap.md) retains the original `41190e0` source links, source-derived reproduction/proposal and separately dated implementation updates. Its open/proposal-only/pending-reboot labels are historical; current acceptance is above.