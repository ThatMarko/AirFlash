# [MEDIUM] An attempted preferred receiver blocks another auto-connect candidate

- **Issue ID**: ISSUE-16
- **Severity**: MEDIUM — automatic connection to a new eligible receiver can be suppressed after another receiver failed.
- **Kind**: Defect
- **Subsystem**: Desktop auto-connect selection
- **Status**: Open — source-verified selection defect; fix proposed, not implemented
- **Runtime baseline**: upstream `41190e0`, retained by documentation commit `c077a05`
- **Evidence status**: Source inspected; the deterministic dispatcher/mock scenario below was not executed. No receiver failure is claimed to have been observed on hardware.
- **Implementation status**: Proposal only; no runtime change applied
- **Target file**: [AppViewModel.cs](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L318)
- **Analysis context**: [After discovery](../analysis/after-discovery.md)

## 1. Current behavior

`TryAutoConnectAsync` builds eligible receivers from online/completeness/visibility and auto-connect settings, then picks the last-used receiver or first name. Only after selecting it does the function consult `_autoAttempted`. If that preferred receiver was already attempted, the function returns without selecting another eligible receiver, even when the second receiver has just appeared and has never been attempted.

The attempted set correctly prevents repeated attempts to the same continuously visible receiver; the defect is applying that exclusion after candidate choice. Automatic connection defaults off, so this requires an enabled global setting or overrides and at least two candidates. See [After discovery](../analysis/after-discovery.md).

## 2. Root cause

```csharp
var eligible = AllReceivers.Where(r => r.Online && r.Complete
    && !_settings.ReadOptions(r.Id).Hidden
    && (_settings.ReadOptions(r.Id).AutoConnect ?? _settings.AutoConnectOnDiscover));
var receiver = eligible.OrderByDescending(r => r.Id == _settings.LastReceiverId).FirstOrDefault();
if (receiver is not null && !_autoAttempted.Contains(receiver.Id)) await ToggleAsync(receiver);
```

Exact source: [AppViewModel.cs:318–323](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L318). `ToggleAsync` marks the id before start and saves it as last-used ([292–310](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L292)); asynchronous connection failure leaves the id attempted. A new B clears B's attempted mark, not A's ([202–206](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L202)). Repeating A's online/complete snapshot does not clear A, so it continues winning selection and blocking B.

## 3. Minimal mock reproduction — planned, not run

Use the existing [automatic-attempt harness](../../desktop/AirFlash.App/Verification/UiRegression.cs#L202) with `MockDiscovery`, `MemoryStore`, fake audio, and [MockFactory.FailOpen](../../desktop/AirFlash.App/Verification/UiSmoke.cs#L344).

1. Enable global auto-connect, disable force reconnect, and publish only complete receiver A. Make the factory fail open. Wait for one automatic attempt and Error; A becomes last-used and attempted.
2. Allow the fake factory to succeed, keep A online/complete, and publish A plus newly discovered complete B. Choose names that keep A first as well, so either preference rule exercises the defect.
3. Pump the dispatcher through the restarted 900 ms timer. Source-predicted result: no second process is opened; B is eligible/unattempted but is never selected.
4. Verify a direct click on B succeeds with the same fake factory, demonstrating that candidate suppression rather than simulated transport failure blocked B.

All hosts can be documentation-only addresses and all connections must be fake. The desired automatic policy is to prefer last-used/name among unattempted eligible candidates, while retaining one automatic attempt per id until its allowed reset.

## 4. Proposed change — not applied

Exclude attempted ids before ranking/selecting candidates. Preserve the existing global/per-receiver override precedence, visibility/completeness checks, entry-time active-session check, and user-stop suppression. That entry check alone does not prevent a separately started session from being replaced after an awaited settings operation; [ISSUE-24](ISSUE-24-HIGH-delayed-autoconnect-replaces-new-session.md) addresses atomic auto-start admission. Do not clear all attempted ids on every snapshot: that would repeatedly retry A and defeat the guard. Keep existing alias migration and offline/incomplete transition reset semantics. No schema, IPC, engine, or network-policy change is needed.

## 5. Acceptance criteria and verification

- After A fails once, a newly eligible unattempted B can be selected on the next timer without another A attempt.
- Last-used/name priority applies among the remaining eligible unattempted ids.
- Identical snapshots do not retry an attempted id; if every candidate was attempted, the timer is a no-op.
- Hidden, offline, incomplete, explicitly auto-connect-disabled, and user-suppressed receivers remain excluded.
- Alias promotion preserves attempted status, while an allowed offline-to-online/incomplete-to-complete transition can make that same receiver retryable.
- Each timer invocation selects at most one candidate; a session already active at entry retains the existing no-op behavior. The overlapping-start protection is separately covered by ISSUE-24 and must not be claimed fixed by candidate filtering alone.

The [existing identity automatic-attempt regression](../../desktop/AirFlash.App/Verification/UiRegression.cs#L202) verifies one receiver's failure, alias promotion, and offline recovery. It does not include the second eligible candidate. Add that assertion, ideally with controllable timer scheduling rather than a timing-sensitive sleep, before implementation is considered verified.

Safety: mock-only verification; no hardware or probes. Any later audio qualification remains gain ≤0.1 and duration ≤5 seconds. No upstream issue #6/#7 or dev correspondence is claimed.
