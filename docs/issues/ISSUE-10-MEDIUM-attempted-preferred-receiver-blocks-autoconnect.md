# [MEDIUM] Automatic selection ranks only eligible unattempted candidates

## Current-source boundary — 2026-10-21

Combined `6c1e2ef` retains the remaining selection fix against stable `439de626`: attempted candidates are filtered with eligibility before ranking, so an attempted preferred receiver cannot hide another eligible candidate. Alias migration, overrides and admitted-attempt accounting remain coordinated with the session-intent changes.

[Current source, acceptance and executed limits](PR-READINESS-2026-10-21.md) supersede older current/open/schema labels below. The remaining source links and test counts retain their recorded historical baselines; this documentation refresh claims no new test execution or upstream publication.

- **Status**: Implemented, pushed and integrated in fork `main` `8989b3609dd1294f4d1d6eb1502f345cc22bc382`.
- **Group / feature**: B, `codex/fix-session-lifecycle`, `442e8d8c8fd7649e3e039ec356e3a4f4b25f6d0b`.
- **Stable comparison**: `cf420d97700463c56adc31408510e8336e99e51c`/PR #19 do not fix this candidate rule.
- **Original severity**: MEDIUM eligible candidate blocked after another attempt at `41190e0`.
- **Plan / evidence**: [ISSUE-10](IMPLEMENTATION_PLAN.md#issue-10), [B acceptance](GROUP-B-ACCEPTANCE.md), [current acceptance](CONTINUATION-ACCEPTANCE.md).
- **Related**: [02 atomic admission](ISSUE-02-HIGH-delayed-autoconnect-replaces-new-session.md), [After discovery](../analysis/after-discovery.md).

## 1. Current behavior

[ReceiverSelection.SelectAutoConnect](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/ReceiverSelection.cs) excludes attempted ids **with eligibility before ranking**. Eligible means online, complete, visible and effectively opted in; receiver false overrides global true, receiver true can opt in independently. Remaining candidates rank by last-used id, current-culture name then ordinal id. Attempted preferred A cannot hide unattempted B.

[TryAutoConnectAsync](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L488) rechecks canonical identity/current options after persistence and uses serialized owner/inactive/intent admission. Only admitted attempts become attempted/last-used; an admitted connection failure counts. Identical online/complete snapshots do not reset A. Alias migration carries its attempt; new/offline-return/incomplete-to-complete transitions retain permitted resets. Global automatic enable retains its explicit reset behavior.

## 2. Historical root cause and observation

At [41190e0 AppViewModel](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L318), ranking chose preferred A first, then returned if A was attempted. Newly appeared B was eligible but never considered. Original isolated fake observations reproduced this blocking; no real receiver failure is attributed.

Filtering fixes ranking, not the separate delayed-start ownership race. Report02 supplies that admission contract; resetting all attempts on every snapshot would defeat both the intended guard and failure policy.

## 3. Regression coverage and acceptance

Current [ReceiverSelectionTests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/ReceiverSelectionTests.cs) and WPF [UiSessionLifecycle](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiSessionLifecycle.cs) assert A-failure/B-fallback, last-used/name/id ordering among remaining candidates, all-attempted no-op, hidden/offline/incomplete/override exclusion, identical snapshot no-repeat, alias carry/reset transitions and admitted-only bookkeeping.

- Candidate B can be admitted without another A attempt.
- At most one current request commits against inactive owner/intent.
- User suppression and active-session guards remain.
- Legitimate reset/migration semantics persist.

Current integrated **667 Core** and complete packaged six **1,059-assertion** regional/catalog runs pass; B acceptance preserves narrower branch results/red controls/review. Source review here executes no real discovery/engine/receiver.

## 4. Compatibility and publication

Schema 2/JSONL v1 and credentials are unchanged. Atomic admission, exact manual routing and passive retention remain distinct acceptance scopes in coherent B. The fork branch is pushed/integrated; old PR #15 remains closed, and this package is not merged upstream.
