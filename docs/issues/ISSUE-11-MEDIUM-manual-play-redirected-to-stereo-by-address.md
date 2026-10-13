# [MEDIUM] Explicit manual playback keeps its requested endpoint and identity

- **Status**: Implemented, pushed and integrated in fork `main` `8989b3609dd1294f4d1d6eb1502f345cc22bc382`.
- **Group / feature**: B, `codex/fix-session-lifecycle`, `442e8d8c8fd7649e3e039ec356e3a4f4b25f6d0b`.
- **Stable comparison**: `cf420d97700463c56adc31408510e8336e99e51c`/PR #19 do not fix this routing rule.
- **Original severity**: MEDIUM explicit manual target replaced by another group/port at `41190e0`.
- **Plan / evidence**: [ISSUE-11](IMPLEMENTATION_PLAN.md#issue-11), [B acceptance](GROUP-B-ACCEPTANCE.md), [current acceptance](CONTINUATION-ACCEPTANCE.md).
- **Analysis**: [Identity](../analysis/identity.md), [After discovery](../analysis/after-discovery.md).

## 1. Current behavior

[ReceiverSelection.ResolvePlayback](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/ReceiverSelection.cs) immediately retains an explicit manual row or selected group. Manual host/port/id/preferences remain authoritative beside discovered complete, incomplete or hidden groups sharing its address.

Discovered member selection needs unique online canonical identity or confirmed broadcast-alias ownership. Distinct physical owners, including a competing standalone alias owner, or multiple group owners reject as ambiguous before command creation. There is **no address-only or address-plus-port ownership fallback**. An endpoint-shaped canonical member id may match exactly by id, not inferred address.

[ToggleAsync](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L434) rechecks discovered availability/completeness and hidden options before Play. Pair shares the selection resolver but requires online/completeness, not the Play hidden gate; a hidden Settings row can still have Pair available. Credential identity remains accessory `GET /info deviceID`, not catalog/manual id.

## 2. Historical root cause and observation

At [41190e0 Toggle](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L295), the first group containing a member with the clicked address replaced the selection regardless of manual status, port, id or alias. A synthetic manual `192.0.2.10:7001` could therefore send two group peers on port 7000; an incomplete group could reject an otherwise valid manual target. Catalog coexistence protections did not govern that later branch.

Original isolated fake command observations failed desired manual target assertions; B acceptance records them. The fixed resolver adopts the stricter identity-only ownership option from the original proposal. No receiver/credential/network routing incident is inferred.

## 3. Regression coverage and acceptance

Current [ReceiverSelectionTests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/ReceiverSelectionTests.cs) and WPF [UiSessionLifecycle](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiSessionLifecycle.cs) assert exact manual single peer/port/id/last-used/options, complete/incomplete/hidden groups sharing an address, preserved explicit group, canonical/broadcast member routing, ambiguous groups/physical aliases and missing address fallback.

- Manual Play sends only its requested peer and retains its identity.
- Explicit group starts its complete peers.
- Confirmed member routes only to its unique owner.
- Ambiguous or unavailable discovered selection sends no Start.
- Manual persistence/session health remains independent of discovery.

Current integrated **667 Core** and six complete packaged **1,059-assertion** UI runs pass; B acceptance binds narrower branch checks, decisive baseline failures and independent review. Fixtures use synthetic identities/counted fake commands without credential access.

## 4. Compatibility and publication

Schema 2/JSONL v1, independent manual preferences, complete new stereo groups, current-user DPAPI/accessory keys and exact local mute restoration remain compatible. The fork branch is pushed/integrated; old PR #15 remains closed and coherent B upstream publication is separate.
