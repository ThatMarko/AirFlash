# [MEDIUM] Playing a manual receiver redirects to a stereo group by address alone

- **Issue ID**: ISSUE-11
- **Implementation plan**: [Work package ISSUE-11](IMPLEMENTATION_PLAN.md#issue-11)
- **PR group**: B — Session lifecycle
- **Severity**: MEDIUM — an explicit manual-row selection can target a different receiver/group and port.
- **Kind**: Defect
- **Subsystem**: Desktop receiver selection
- **Status**: Implemented in local main `9e894ab`; feature and earlier compositions were verified separately. B remains unmerged upstream; PR #15 is closed without merge. New main testing is deferred until after reboot.
- **Runtime baseline**: upstream `41190e0`, retained by documentation commit `c077a05`
- **Evidence status**: The original audit inspected source without executing the proposed command-capture scenario. Subsequent isolated baseline observation reproduced manual endpoint redirection; the local implementation passes the [Group B checks](GROUP-B-ACCEPTANCE.md). No real receiver was contacted.
- **Implementation status**: Pushed feature head `0a2c1ac`, including production follow-up `11c6046` and regional/catalog verification corrections, is merged locally with C/D and updated upstream. See [Group B acceptance](GROUP-B-ACCEPTANCE.md) for dated results and [integration acceptance](ABCD-INTEGRATION-ACCEPTANCE.md) for this source-only merge boundary.
- **Target file**: [AppViewModel.cs](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L292)
- **Analysis context**: [After discovery](../analysis/after-discovery.md), [Identity](../analysis/identity.md)

## Current local implementation

`ReceiverSelection.ResolvePlayback` immediately preserves manual and explicitly selected group rows. An automatic member can resolve only by canonical id or confirmed broadcast alias among online non-manual peers, with unique physical identity and group ownership. Shared address/port alone is not an ownership fallback. Ambiguity fails before an engine command. `ToggleAsync` rechecks discovered availability and hidden options before start, while manual endpoints and credential identity remain independent.

Source audit: [AppViewModel](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs), [SessionController](../../desktop/AirFlash.Core/SessionController.cs), [ReceiverSelection](../../desktop/AirFlash.Core/ReceiverSelection.cs) and their [lifecycle](../../desktop/AirFlash.Tests/SessionLifecycleTests.cs), [selection](../../desktop/AirFlash.Tests/ReceiverSelectionTests.cs) and [WPF](../../desktop/AirFlash.App/Verification/UiSessionLifecycle.cs) assertions were read against source main `9e894ab133f8cbdb3b5f5ac1f411023b182e65d7`. No application/build/test execution occurs in this audit. The following original behavior/proposal/coverage statements remain pinned to `41190e0`; missing-fixture statements describe that baseline, not the current test tree.

## 1. Original baseline behavior

The catalog deliberately preserves a manual receiver independently of a discovered device at the same endpoint. `ToggleAsync` nevertheless replaces any clicked receiver with the first known stereo group whose member has the same address. The lookup does not require a non-manual receiver, matching port, member id, or confirmed alias.

A manual receiver at `192.0.2.10:7001` can therefore start a discovered pair whose member is at `192.0.2.10:7000`. The playback command then contains both pair peers and the pair becomes last-used. With an incomplete pair, the same redirection can reject an otherwise complete manual row instead. This is a selection problem before JSONL dispatch, not an engine routing or credential problem. See [After discovery](../analysis/after-discovery.md), [Identity](../analysis/identity.md), and [Settings](../analysis/settings.md).

## 2. Root cause

[AppViewModel.cs:295–297](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L295):

```csharp
var group = AllReceivers.FirstOrDefault(r => r.IsGroup
    && r.Members.Any(m => m.Address == receiver.Address));
if (group is not null) receiver = group;
```

The substituted receiver is passed to Session and persisted as last-used ([304–307](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L304)). Manual rows are separately constructed with `IsManual = true` ([222–227](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L222)); identity reconciliation excludes manual ids from migrations ([ReceiverCatalog.cs:10–11,35–38,66–71](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/ReceiverCatalog.cs#L35)). That catalog protection does not govern this later address-only branch.

## 3. Original mock reproduction plan

Use the WPF dispatcher harness, in-memory settings, fake discovery/audio, and a fake engine capturing JSONL command parameters. The existing [MockFactory.Commands](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Verification/UiSmoke.cs#L344) can capture `start` without opening sockets.

1. Persist manual M with host `192.0.2.10`, port `7001`, and its own endpoint id. Disable auto-connect and force reconnect.
2. Publish a complete stereo row `stereo:fixture` with members A at `192.0.2.10:7000` and B at `192.0.2.11:7000`. Both addresses are documentation-only fixtures; the fake factory must never connect to them.
3. Wait for both the group and manual row to appear, then call `ToggleAsync` with M while the session is Idle.
4. Source-predicted result: `Session.Snapshot.Receiver.Id` and `LastReceiverId` become `stereo:fixture`, and captured `start.params.peers` contains two peers on port 7000 instead of M's single peer on port 7001.
5. Repeat with a one-member group. Source-predicted result: the manual click is substituted with an incomplete group and rejected by [SessionController.StartAsync](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SessionController.cs#L176).

The positive comparison is explicit: choosing M should send one peer with its requested host/port and keep M's id. A separate automatic member click may intentionally resolve to its confirmed stereo owner.

## 4. Original architecture proposal

Never substitute a manual selection with a discovered group. Preserve its explicit host, port, id, options, and independent lifecycle. For an automatic individual-member selection, resolve the owning group by canonical member id or confirmed broadcast alias; if an endpoint fallback is required, compare normalized address and port and require one unambiguous owner. An already selected group should remain that same group.

Keep manual preferences independent of discovered preference records, and do not use address equality as proof of physical identity. Native credentials remain DPAPI-encrypted and keyed by the accessory `deviceID` returned by `GET /info`; manual/catalog row ids do not create separate credential namespaces. Preserve complete-group validation for actual group starts and existing identity migration protections. This fix can stay within desktop selection; config schema 2 and JSONL v1 need no change.

## 5. Acceptance criteria and verification

- Clicking a manual row at a stereo member's address starts exactly its one requested endpoint, including a differing port, and retains the manual id as active/last-used.
- The same guarantee holds when the matching discovered group is incomplete or hidden.
- Clicking the stereo row starts its two peers normally; clicking a confirmed automatic member routes only to its own unique group.
- Same address/different port and ambiguous group ownership cannot trigger substitution by address alone.
- A manual stream remains exempt from ordinary discovery-loss handling; its own stop/error cleanup remains functional.
- Existing manual and automatic preference records remain independent.

The [manual identity test](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/ReceiverIdentityTests.cs#L178) and [UI identity harness](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Verification/UiRegression.cs#L197) establish coexistence, not selection behavior at a shared stereo-member address. The [stereo harness](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Verification/UiRegression.cs#L222) starts the group itself. A manual-click command-capture assertion and an incomplete-group negative case are missing.

Safety: fake engine only; no discovery service, receiver pairing, real addresses, or credentials. Any later authorized audio probe retains gain ≤0.1 and duration ≤5 seconds. No upstream issue #6/#7 or dev relationship is claimed.

## Executed implementation and fork integration

The source and proposed-fixture discussion above describes the original `41190e0` audit. [Group B acceptance](GROUP-B-ACCEPTANCE.md) records reproduced original-base failures, implementation/follow-up commits and their executed Core/WPF evidence. Current pushed B `0a2c1ac` is now included in local main `9e894ab`; original upstream has merged A/scrolling, while B PR #15 remains closed without merge. [Combined acceptance](ABCD-INTEGRATION-ACCEPTANCE.md) keeps earlier tested compositions separate from this merge's deferred main testing. No hardware incident is attributed to this defect.
