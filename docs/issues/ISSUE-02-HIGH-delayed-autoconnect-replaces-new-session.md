# [HIGH] Automatic admission rejects delayed work after newer Play, Pair or Stop

## Current-source boundary — 2026-10-21

On original stable `439de626`, passive retention does not supply complete automatic admission. Combined `6c1e2ef` separates automatic admission from explicit Toggle, invalidates stale intent synchronously and rechecks current eligibility after persistence; only admitted attempts consume candidates or update last-used state.

[Current source, acceptance and executed limits](PR-READINESS-2026-10-21.md) supersede older current/open/schema labels below. The remaining source links and test counts retain their recorded historical baselines; this documentation refresh claims no new test execution or upstream publication.

- **Status**: Implemented, pushed and integrated in fork `main` `8989b3609dd1294f4d1d6eb1502f345cc22bc382`.
- **Group / feature**: B, `codex/fix-session-lifecycle`, `442e8d8c8fd7649e3e039ec356e3a4f4b25f6d0b`.
- **Original defect / severity**: HIGH explicit-session disruption at `41190e0`; this is the historical defect rating.
- **Stable comparison**: `cf420d97700463c56adc31408510e8336e99e51c`/PR #19 do not implement atomic automatic admission.
- **Plan / evidence**: [ISSUE-02](IMPLEMENTATION_PLAN.md#issue-02), [B acceptance](GROUP-B-ACCEPTANCE.md), [current acceptance](CONTINUATION-ACCEPTANCE.md).
- **Related**: [01 ownership](ISSUE-01-HIGH-stale-discovery-snapshot-controls-new-session.md), [10 ranking](ISSUE-10-MEDIUM-attempted-preferred-receiver-blocks-autoconnect.md).

## 1. Current behavior

[TryAutoConnectAsync](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L488) is separate from explicit click Toggle. It captures atomic owner/intent before its first await, persists legitimate dirty settings, then revalidates current canonical candidate, online/completeness/visibility/options/attempts/readiness/suppression under the settings gate. Eligibility can change without an owner change, so ownership alone is insufficient.

The gate stays held through [TryStartAutomaticAsync](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs#L218), which checks owner, current intent and inactivity under serialization and rechecks intent after cleanup. Error remains eligible inactivity. Explicit Play/Pair/Stop/shutdown invalidate older intent synchronously before their waits. Settings Pair uses the shared intent path without waiting behind the held settings save or holding a gate across PIN input.

Only admitted attempts update attempted/last-used, including an admitted connection failure. Rejection sends no stale Start/Stop and preserves valid saves/newer Stop suppression. Multiple queued automatic tasks cannot all commit against the same inactive owner.

## 2. Historical root cause and observation

At [41190e0](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L292), automatic connection used Toggle: entry-time eligibility preceded awaited dirty save/settings gate, then unconditional Start replaced a newer Pair and reset later Stop suppression. The controller serialized the wrong requested replacement correctly; native auth was not the cause. Filtering attempted candidates alone could not repair this authority gap.

Isolated original-base fake observations failed desired replacement/admission assertions, retained in B acceptance. No physical receiver/PIN/OS mute incident is inferred. The initial source-only reproduction plan and pre-reboot snapshots are historical, not missing current tests.

## 3. Observable regression coverage

[SessionLifecycleTests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/SessionLifecycleTests.cs) and [UiSessionLifecycle](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiSessionLifecycle.cs) control save entry, serialization, command sends and Pair events. They cover delayed flush versus Pair/Streaming, user Stop, Stop waiting for controller serialization, eligibility/options changes during persistence without owner change, multiple queued requests, same-id replacement, valid Idle/Error admission and save failure.

Stop fixtures hold Pair **before** pin_required or finish pairing/close the prompt before ordinary Stop. CanExecute=true does not establish an accessible control behind a modal PIN dialog. Gates release in finally, and the owned controller/fixture is disposed awaitably; explicit task joins are asserted where the fixture performs them. Assertions inspect authority, command targets, cancellation, disposal and admitted-only bookkeeping.

## 4. Acceptance criteria and verification

- Delayed automatic work cannot replace newer Pair/Play or undo Stop/shutdown.
- Catalog/Apply changes prevent stale admission even with unchanged owner.
- Idle/Error valid admission and explicit switching remain functional.
- Admitted failures count; rejected work leaves valid saves/newer suppression intact.
- No lifecycle/settings gate spans a PIN prompt; cleanup restores exact fake original mute bits.

Current integrated **667 Core** and complete packaged six **1,059-assertion** regional/catalog runs pass; B acceptance records narrower branch checks/red controls and independent review. This docs-only audit reruns no runtime/hardware operations.

## 5. Compatibility and publication

[Session](../analysis/session.md), [After discovery](../analysis/after-discovery.md) and [Settings](../analysis/settings.md) describe callers. Schema 2/JSONL v1, complete new groups, selected-adapter no-fallback, exact manual endpoints and accessory-keyed DPAPI remain compatible. B is integrated/pushed to the fork; old PR #15 remains closed/unmerged. Upstream publication is separate.
