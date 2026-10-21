# [HIGH] DNS resolution has finite logical deadlines and explicit resource ownership

## Current-source boundary — 2026-10-21

Stable `439de626` already provides scoped Windows DNS-SD and selected-adapter pause. Combined `6c1e2ef` adds finite logical deadlines/retries, TTL/token/epoch separation, bounded owners and observed helper-exit settlement, plus containment of secondary diagnostic failures. A timeout or cancel result alone still cannot justify native reclamation.

[Current source, acceptance and executed limits](PR-READINESS-2026-10-21.md) supersede older current/open/schema labels below. The remaining source links and test counts retain their recorded historical baselines; this documentation refresh claims no new test execution or upstream publication.

- **Status**: Implemented, pushed and integrated in fork `main` `c5ddb894e8bf0f977a94920ff26878c3cf5e1004`.
- **Group / feature**: D, `codex/fix-discovery-resolve`, `b8ee79511e2fa41ee02651dad83d50e008f9f680`.
- **Stable comparison**: `cf420d97700463c56adc31408510e8336e99e51c` does not contain this deadline/process-ownership fix.
- **Original severity**: HIGH logical rediscovery suppression at `41190e0`; no demonstrated Windows leak/network cause is claimed.
- **Plan / evidence**: [ISSUE-04](IMPLEMENTATION_PLAN.md#issue-04), [D acceptance](GROUP-D-ACCEPTANCE.md), [current acceptance](CONTINUATION-ACCEPTANCE.md).
- **Analysis**: [Discovery](../analysis/discovery.md).

## Refreshed preparation — 2026-10-13

Final standalone b8ee79511e2fa41ee02651dad83d50e008f9f680 passes 303 Core / 113 focused on both SDKs, six observed WPF catalog/format cells × 580, actual synthetic helpers and exact-source hosted Windows qualification. Full TXT publication and case-insensitive helper TXT lookup preserve stable metadata behavior. [Current D acceptance](GROUP-D-ACCEPTANCE.md) binds failed controls, Main integration and all limits; no upstream publication is performed. The runtime narrative and earlier executions below retain their named historical sources.

## 1. Recorded runtime behavior

[WindowsDiscovery](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Services/WindowsDiscovery.cs) separates advertisement, cached record, pending logical token, retained owner and epoch. Five-second monotonic initial/refresh deadlines work even without a cached record. Failure retires only the exact token, and a still-advertised name retries with one-to-thirty-second exponential backoff. Same-epoch obsolete tokens and old epochs cannot publish stale records or clear a newer request.

Current advertisement/cache expiry follows positive PTR TTL capped at one day; renewed PTR extends cached expiry. Restart/resume keeps existing cache for ten seconds. The historical ninety-second cache is not current policy. Maintenance is one second; successful refresh/adapter checks are twenty seconds. Selected unavailable adapters remain paused without fallback.

[ResolveProcessDnsServiceApi](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Services/ResolveProcessDnsServiceApi.cs) roots exact child owners before Start and globally caps eight, including blocked starts/cancelled/unobserved-exit owners. Once started, only successfully observed exact-child exit releases ownership. Reply, deadline, Cancel, Kill or read result alone does not. Read/Kill failure can still settle if exit is confirmed; failed exit observation retains capacity/diagnostics until later successful observation.

[DnsResolveHelper](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Services/DnsResolveHelper.cs) dispatches before WPF/logging/settings/COM, emits the first bounded copied result, starts no descendants and self-terminates after six seconds. Browse owner settlement requires the documented terminal 1223 callback or proved non-pending admission, plus no outstanding tracked Start/Cancel call or managed callback consumer section. Arbitrary reverse-P/Invoke return-stack quiescence remains unqualified; cancellation return alone never proves quiescence. Coordinator/per-name/shared caps include old epochs. B separately retains owned transport through passive availability loss.

## 2. Historical root cause and observations

At [41190e0](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L83), a missing resolve callback left `_pending` indefinitely. Ninety-second expiry could remove the name while leaving that token, so another PTR skipped resolving. Completion, removal/new PTR or Restart could recover; “permanent until app restart” was too broad. Original logical-state probes against unchanged source are recorded in D acceptance. Dropped packets do not establish Windows callback loss.

The failed cancellation/publication run **38134996290** was not resolved by its later documentation-only green. Controlled investigation found the fake drainer synchronously joining its own callback; both drain schedules now use explicit signals and deferred fake cancellation. That establishes a fixture defect, not a production deadlock.

Earlier in-process D treated ordinary resolve callbacks as settlement. API review did not establish quiescence; its zero-owner probe counters do not prove safe native reclamation. The conservative retained-input prototype then exhausted per-name capacity after two browse cycles and was rejected. Current helper exit is the production boundary. These superseded conclusions and raw failures remain explicit in acceptance.

## 3. Regression coverage

[DiscoveryLifecycleTests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/DiscoveryLifecycleTests.cs), [NativeDnsOperationTests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/NativeDnsOperationTests.cs) and [ResolveProcessTests](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/ResolveProcessTests.cs) inject DNS, monotonic time and child operations. They cover missing/inline/late callbacks; failed Start/Cancel/Kill/exit wait; timeout order; expiry/removal/new PTR; same-token/epoch rejection; exact settlement; caps across instances/epochs; one hundred attempts/scoped cycles; Restart/Dispose and selected-adapter no-fallback. Raw unmanaged fixtures cover x64 layout/TTL, payload freeing and tracked Start/Cancel and managed callback consumer lifetimes.

## 4. Acceptance criteria and verification

- Missing first/refresh callback reaches deadline and permits bounded retry.
- Old callbacks cannot affect newer requests/catalog.
- Cancellation/result does not release unsettled resource ownership.
- Exact observed child exit releases once; failed observation retains bounds.
- Observer failures/reentrant publication cannot abandon remaining native work.
- Selected adapter/manual persistence/owned playback remain compatible.

D's narrower 108 focused/295 Core/build and **six synthetic actual-child** results remain bound to their recorded sources; the actual-child receipt is `dc521ae`, not relabeled as 8989. Current integrated **667 Core**, normal CI/stability and complete code-identical packaged UI/DPI/MSI checks pass. No current live DNS/receiver probe is claimed, and no runtime test is rerun for this documentation update.

## 5. Limits and publication

JSONL v1/schema 2 and catalog identity remain compatible. Caps are safe bounds, not guarantees of callback delivery or rediscovery. Unknown exit can retain capacity until observation or app restart. No real network incident is a required reproduction; no upstream issue/PR is created by this update.
