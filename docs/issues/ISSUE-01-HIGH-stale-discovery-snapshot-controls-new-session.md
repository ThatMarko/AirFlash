# [HIGH] Stale discovery control is bound to its original session owner

## Current-source boundary — 2026-10-21

Original stable `439de626` already includes passive retention and guarded identity refresh from PR #19. Combined `6c1e2ef` supplies atomic owner/intent capture and serialized delayed-mutation checks, including the later owned-settings continuation repair; those remaining guards are prospective PR scope.

[Current source, acceptance and executed limits](PR-READINESS-2026-10-21.md) supersede older current/open/schema labels below. The remaining source links and test counts retain their recorded historical baselines; this documentation refresh claims no new test execution or upstream publication.

- **Status**: Implemented, pushed and integrated in fork `main` at `8989b3609dd1294f4d1d6eb1502f345cc22bc382`.
- **Group / feature**: B, `codex/fix-session-lifecycle` at `442e8d8c8fd7649e3e039ec356e3a4f4b25f6d0b`.
- **Original defect / severity**: HIGH session-ownership disruption at `41190e0`; severity describes that original defect, not an unresolved current failure.
- **Original stable comparison**: `cf420d97700463c56adc31408510e8336e99e51c` already contains PR #19's passive retention/fresh snapshot/Pairing exclusions. The full atomic-owner guard remains fork scope.
- **Plan / evidence**: [ISSUE-01](IMPLEMENTATION_PLAN.md#issue-01), [B acceptance](GROUP-B-ACCEPTANCE.md), [current integrated acceptance](CONTINUATION-ACCEPTANCE.md).
- **Analysis**: [After discovery](../analysis/after-discovery.md), [Session](../analysis/session.md), [Playback coupling](../analysis/playback-coupling.md).

## 1. Current behavior

[AppViewModel.ReconcileDiscoveryAsync](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L301) atomically captures snapshot, lifecycle owner and intent before awaited identity persistence. A valid save/migration remains valid if independent Pair changes ownership. Afterward, fresh state must retain the original authority before a complete online replacement reaches [TryUpdateReceiverAsync](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs#L397). That operation rechecks owner/intent under controller serialization, including replacement cleanup.

Metrics publications and native retries retain desktop ownership; receiver-id reuse does not. Stale work cannot modify the replacement's receiver, settings, worker or diagnostics. Same-owner promotion still succeeds, including Pairing metadata; unchanged effective transport preserves the engine and applicable latency/transport changes can restart playback. Passive absence does not issue Stop, so unused conditional-stop plumbing is absent.

## 2. Historical root cause and observation

At [original AppViewModel 41190e0](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L166), discovery captured A, awaited saving, then unconditionally stopped/updated whichever session was current. Settings Pair could replace A with B outside the settings gate during that await. Captured A's non-manual exemption did not protect current manual B. A reread alone was also insufficient because replacement could occur before controller admission.

Original isolated fake observations reproduced both stale-control paths; [B acceptance](GROUP-B-ACCEPTANCE.md) preserves actual baseline failures and source hashes. No household dropout is attributed to this race. Current stable removes already-fixed passive branches from prospective upstream scope; it does not supply the full atomic token/serialization contract.

## 3. Regression coverage

Current [SessionLifecycleTests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/SessionLifecycleTests.cs) and WPF [UiSessionLifecycle](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiSessionLifecycle.cs) use save-entry signals, controlled connections/Pair events and awaitable reconciliation. They assert absent/changed A versus new Pair, same-id/new-owner replacement, metrics during delay, identity-save failure preservation, valid promotion and changed-signature restart. Gates release in finally; command targets/disposal/ownership are asserted rather than inferred from logs.

## 4. Acceptance criteria and verification

- New Pair/replacement survives delayed old reconciliation with no stale retarget, settings/diagnostic mutation or cancellation.
- Same-owner metrics/promotions remain valid; signature change retains its intended restart.
- Failed identity save does not replace that transaction's catalog/session.
- Controller admission, not merely a view-model reread, rejects stale authority.

These regressions participate in current main's **667 Core tests** and code-identical complete packaged WPF regional/catalog runs (**six × 1,059 assertions**). Exact checks, earlier red controls and independent review are in B/current acceptance. This documentation update performs source/evidence review, not new hardware execution.

## 5. Compatibility and publication

JSONL v1/schema 2, accessory-keyed credentials, manual selection and exact mute restoration remain compatible. Old [PR #15](https://github.com/Ding-Kyoma/AirFlash/pull/15) stays closed at its older snapshot; [PR #19](https://github.com/Ding-Kyoma/AirFlash/pull/19) is merged. The new fork branch is integrated locally, not automatically upstream. Publication requires the separately authorized coherent Group B submission.
