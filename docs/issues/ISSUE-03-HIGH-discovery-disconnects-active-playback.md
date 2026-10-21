# [HIGH] Passive discovery loss preserves owned playback and its Stop controls

## Current-source boundary — 2026-10-21

Passive playback retention is already upstream through PR #19 and remains present on stable `439de626`. Combined `6c1e2ef` preserves it while adding owner/intent, active-control, Connecting-publication and settings-continuation safeguards. Retention does not prove that the observed receiver disappearance incident (#6) is resolved.

[Current source, acceptance and executed limits](PR-READINESS-2026-10-21.md) supersede older current/open/schema labels below. The remaining source links and test counts retain their recorded historical baselines; this documentation refresh claims no new test execution or upstream publication.

- **Status**: Implemented and integrated in fork `main` `8989b3609dd1294f4d1d6eb1502f345cc22bc382`.
- **Group / feature**: B, pushed `442e8d8c8fd7649e3e039ec356e3a4f4b25f6d0b`.
- **Upstream status**: Original passive-Stop scope is already fixed in stable `cf420d97700463c56adc31408510e8336e99e51c`/merged [PR #19](https://github.com/Ding-Kyoma/AirFlash/pull/19); do not propose it again as absent.
- **Original severity**: HIGH interruption defect at `41190e0`, not proof of an unresolved current device failure.
- **Plan / evidence**: [ISSUE-03](IMPLEMENTATION_PLAN.md#issue-03), [B acceptance](GROUP-B-ACCEPTANCE.md), [current acceptance](CONTINUATION-ACCEPTANCE.md).

## 1. Current behavior

[Reconciliation](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L301) retains Connecting, Pairing, Streaming and Standby through empty, partial, overfull, offline or repeated discovery. An already complete transport remains distinct from truthful catalog availability. A complete online reappearance may retarget only the original atomic owner/intent under controller serialization; applicable signature changes can restart playback.

[RefreshReceivers](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L371) overlays an owned active card even when hidden/offline/removed from the selected-interface catalog. Its toggle remains Stop. New Play/automatic starts still reject hidden/offline/incomplete candidates; explicit Pair checks availability/completeness and may intentionally be offered on a hidden Settings row. Manual rows persist independently.

Adapter change deliberately retains owned playback while discovery obeys the selected interface, with no unavailable-adapter fallback. Explicit Stop, PIN cancellation, shutdown and terminal faults still cancel/clean up/retry according to policy. Exact playback mute restoration remains best effort after cleanup; recording endpoints are excluded. See [Playback coupling](../analysis/playback-coupling.md).

## 2. Historical root cause and observation

At [41190e0 AppViewModel](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L211), absent/incomplete discovered rows called Stop without inspecting native transport health. Baseline Restart published empty before new browse, so it could interrupt an otherwise healthy fake connection. Later new Start reset diagnostics; zero reconnects did not prove the cause. Current Restart/cache scheduling differs as documented in [Discovery](../analysis/discovery.md).

Original isolated fake observations reproduced empty/partial interruption. No Wi-Fi scan duration, OS-specific tolerance, HomePod chime, author's intent, packet-loss cause or relationship to upstream issues #6/#7 is established. Fixed tests replace the old baseline assertion that member disappearance must stop playback.

## 3. Regression coverage and acceptance

Current WPF [UiSessionLifecycle](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiSessionLifecycle.cs), [SessionTests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/SessionTests.cs) and [SessionLifecycleTests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/SessionLifecycleTests.cs) cover every retained state, empty/partial/repeated publications, reappearance, selected adapter, active controls, manual persistence, explicit Stop, terminal faults/retry and both original mute bits through fakes.

- Passive loss sends no Stop/new Start/disposal/restore for the retained healthy attempt.
- New incomplete/offline playback remains rejected.
- Retained controls provide accessible Stop; modal Pair fixtures respect PIN accessibility.
- Terminal faults and explicit cancellation preserve cleanup/diagnostics, with exact fake restoration.
- Complete reappearance respects owner/intent and transport signature.

These checks participate in current **667 Core** and six full packaged **1,059-assertion** UI runs. B/current acceptance retains narrower branch results, baseline failures and review. No new receiver/DNS/audio operation runs in this documentation task.

## 4. Publication boundary

Original passive-Stop scope is upstream; additional atomic ownership/admission/hidden-control/manual-selection work remains fork B scope. Old PR #15 stays closed at its older snapshot. Current local integration is not an upstream merge, release or hardware qualification.
