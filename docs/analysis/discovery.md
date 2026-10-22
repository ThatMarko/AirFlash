# Discovery

## Current queue and ownership distinction — 2026-10-22

Fork Main [`39d34e1`](https://github.com/ThatMarko/AirFlash/commit/39d34e1f108ad8709e0dcc3651876167284b6600) integrates public candidate [`b947081`](https://github.com/ThatMarko/AirFlash/commit/b94708143b381cd53df54253596b7d9985c8ff43); the discovery production/test blobs match. Correction `42baed8` distinguishes ordinary helper concurrency from retained-ownership capacity. The source and tests linked here pin that current candidate; the earlier maps and counts below retain their historical baselines.

The [process-wide limit remains eight](https://github.com/ThatMarko/AirFlash/blob/b94708143b381cd53df54253596b7d9985c8ff43/desktop/AirFlash.App/Services/ResolveProcessDnsServiceApi.cs#L31). A ninth advertised name stays due for maintenance rather than immediately reporting unsafe ownership. [Each name records its own continuous deferral](https://github.com/ThatMarko/AirFlash/blob/b94708143b381cd53df54253596b7d9985c8ff43/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L231); after the [default five seconds](https://github.com/ThatMarko/AirFlash/blob/b94708143b381cd53df54253596b7d9985c8ff43/desktop/AirFlash.App/Services/DnsServiceApi.cs#L57), one coordinator-wide notice reports waiting for earlier queries and automatic retry. Repeated PTRs cannot postpone that timer. Admission clears that name's timer and rearms the notice; removal/reappearance starts a fresh wait. The separate retained-owner caps still report their existing uncertainty warning.

This changes admission diagnostics, not resource reclamation: [only observed exit of the exact started helper settles its owner](https://github.com/ThatMarko/AirFlash/blob/b94708143b381cd53df54253596b7d9985c8ff43/desktop/AirFlash.App/Services/ResolveProcessDnsServiceApi.cs#L118). Reply, cancellation, timeout and Kill remain insufficient. The [healthy-nine-name regression](https://github.com/ThatMarko/AirFlash/blob/b94708143b381cd53df54253596b7d9985c8ff43/desktop/AirFlash.Tests/ResolveProcessTests.cs#L239) proves eventual publication without a false ownership notice; the [retained-batch regression](https://github.com/ThatMarko/AirFlash/blob/b94708143b381cd53df54253596b7d9985c8ff43/desktop/AirFlash.Tests/ResolveProcessTests.cs#L299) proves the five-second boundary, coalescing, removal/reappearance and recovery after one exact exit while seven old owners remain retained. Both explicitly passed in qualified [headless run 39704642045](https://github.com/ThatMarko/AirFlash/actions/runs/39704642045).

[The field regression report](../issues/FIELD-REGRESSION-2026-10-21.md) separates this notice defect from the observed PTP connection failures. Their physical clock cause remains unproven. The first corrective 6.0.2-rc.1 [preview run 39803323131](https://github.com/ThatMarko/AirFlash/actions/runs/39803323131) failed before packaging on a separate inherited standby fixture. Its test-only correction leaves this discovery code intact; a new-source preview qualification and successful field retest remain required.

## Diagnostic containment — 2026-10-21

Combined `6c1e2ef` contains failures from secondary Trace logging after a log/event observer throws. Three red/fixed regressions prove later publication/subscribers and deferred native work continue while unsettled resource ownership is retained. This changes neither cancel/quiescence contracts nor helper-exit settlement. [Current evidence](../issues/PR-READINESS-2026-10-21.md) retains the earlier cancellation/publication diagnosis separately; a later unchanged green run is not its repair.

## Discovery preparation supplement — 2026-10-13

Current fork code `c5ddb894e8bf0f977a94920ff26878c3cf5e1004` integrates standalone D `b8ee79511e2fa41ee02651dad83d50e008f9f680`, based on original `cf420d97700463c56adc31408510e8336e99e51c`. Full ServiceRecord/TXT equality now triggers publication, restoring cf420d9 behavior; Describe remains log formatting only. The older gpn/igl/pk/cn notification limitation below belongs to pinned0623787 and is superseded. Helper JSON copies TXT into OrdinalIgnoreCase dictionaries without changing values and rejects case-folded duplicates. Successful replies explicitly emit LF. Browse settlement remains terminal 1223 plus tracked calls/managed consumer sections, not arbitrary native return-stack observation.

[D acceptance](../issues/GROUP-D-ACCEPTANCE.md) records source, tests, reviews and limits. The earlier source map below retains its pinned historical baseline.

This page describes fork `main` at **`06237873dcb6394eb35e97a017c536843bd7743d`**, reviewed on 2026-10-12, including original stable `cf420d97700463c56adc31408510e8336e99e51c`. Final Group D `277efc5` is pushed and integrated here, not merged upstream. [Audit](audit.md) and [D acceptance](../issues/GROUP-D-ACCEPTANCE.md) bind results and historical failures to their sources.

The desktop uses Windows DNS-SD without Bonjour. Long-lived browse operations remain in the WPF process; production resolves run one query per bounded helper process because an ordinary resolve callback or successful Cancel return does not prove callback quiescence. [WindowsDiscovery](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Services/WindowsDiscovery.cs) owns logical state, [NativeDnsServiceApi](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Services/NativeDnsServiceApi.cs) owns native browse inputs, and [ResolveProcessDnsServiceApi](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Services/ResolveProcessDnsServiceApi.cs) owns resolve-child lifetimes.

## When browsing starts

[AppViewModel.UpdateDiscoveryDemand](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L198) requests browsing while the panel or Settings receiver page is visible, or during a ten-second startup search when automatic connection is enabled globally or for a non-hidden receiver. Hidden startup with no automatic intent can remain paused. Suspended discovery preserves its catalog and does not revoke playback.

Start checks DNS-SD availability, subscribes network notifications and arms one-second monotonic maintenance. Successful resolve refresh and adapter enumeration use twenty-second cadence. Production queues potentially blocking Start/Cancel off the dispatcher and outside publication frames.

## Which interface is queried

[NetworkAdapterCatalog](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Services/NetworkAdapterCatalog.cs) retains unavailable non-loopback adapters to explain a saved pause. [DiscoveryInterface.ResolveIndex](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/Receiver.cs) uses index 0 only for an empty selection. An up adapter with IPv4/nonzero index uses that index; a missing, down or address-less selection pauses without fallback.

Browse and helper resolve receive this index. It does not bind Rust playback egress. Media sockets follow the RTSP connection's local address; timing sockets retain their wildcard/routing boundaries in [Engine](engine.md). Settings display alone does not change live selection.

## The browse

The service types are `_airplay._tcp.local` and `_raop._tcp.local`. Start must remain pending (`9506`); unexpected non-pending results retire/retry the logical request and report diagnostics. Callbacks match exact owner and epoch. Native contexts are opaque never-reused tokens, not managed pointers.

Positive PTR expiry is monotonic time plus TTL, capped at 86,400 seconds. TTL zero removes the advertisement, cached record and exact pending logical request, then retires/cancels its owner. Browse errors retire/retry with backoff. Raw x64 parsing uses type offset 16, flags 20 and TTL 24; fixtures distinguish flags from TTL. Stale/unknown callbacks still free their returned DNS payloads. Observer/logging errors are isolated.

### Restart, suspension and epochs

General network notifications restart without identifying which adapter changed; twenty-second adapter checks restart on index changes. Restart retires owners, clears advertisements/pending requests, resets browsers and gives surviving cached records ten seconds for rediscovery. It schedules native work before any resulting publication: **it does not promise an unconditional empty Changed event before an inline new resolve**.

An explicit interface change clears records and publishes that transition. A selected unavailable interface starts no browser/helper. Stop suspends demand and clears advertisements/pending requests while keeping the catalog. Dispose clears logical requests/advertisements/catalog and retires owners, but outstanding owners remain retained until their separate settlement conditions succeed; disposal is not proof of resource release. Old epochs and obsolete same-epoch tokens cannot publish stale data or clear newer requests.

## The resolve

Every initial/refresh request has a five-second monotonic logical deadline, including requests with no cached result. Timeout, failed Start, invalid/failing result, removal or expiry retires its exact token. Still-advertised names retry with exponential one-to-thirty-second backoff; repeated PTRs do not reset failures. Success resets failures and schedules refresh after twenty seconds.

[ResolveWire](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Services/ResolveProcessDnsServiceApi.cs) validates one bounded UTF-8 JSON reply (65,536 bytes excluding newline), version/status consistency, matching instance/type, IPv4, port 1–65535, at most 256 TXT pairs and bounded strings. Native instance copying checks unsigned count/array shape, IPv4 pointer and port before freeing result memory. IPv6/hostname are not substituted.

Acceptance requires current advertisement, epoch, token and unexpired deadline. A copied reply completes logical resolution, **not resource ownership**.

## Native and process ownership

[DnsResolveHelper](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Services/DnsResolveHelper.cs) dispatches before WPF, logging, settings, COM or engine extraction. It starts no descendants, emits only its first bounded result and has a six-second self-termination watchdog, including blocked native Start. It does not reclaim pending native resolve inputs on callback return; its address space ends instead.

The parent roots the exact process object before Start. Its global maximum of eight child owners includes unstarted, starting, cancelled and killed-but-unobserved children across instances/epochs. A throwing Start can still associate a process. Only proved absence of association permits pre-start settlement.

Once started, **only successfully observed exit of that exact child releases ownership**. Reply, deadline, Cancel or Kill alone is insufficient. Read/Kill failures can still settle if exit is confirmed. Failed exit observation retains capacity, reports uncertainty and retries observation; it never substitutes a PID. Parent read/process budgets and termination complement the helper watchdog and logical deadline.

Browse request/name/cancel inputs remain until documented terminal cancellation `ERROR_CANCELLED = 1223`, or proved non-pending admission, with no tracked Start/Cancel call or managed callback payload/consumer section outstanding. `ExitCallback` can settle after decrementing that tracked section while the reverse P/Invoke is still returning; the counters do not observe arbitrary unmanaged stack frames. Cancel return is not settlement. Ordinary result payloads have their own freeing obligation. Cancellation from callbacks/publication is deferred and coalesced, and callback-exit cancellation uses a worker to avoid reentering the returning native callback.

Coordinator caps are 64 retained owners per instance and two unresolved owners per name, including old epochs; the shared table caps 128. The eight-child process boundary lets completed queries reclaim capacity, unlike the rejected retained-input in-process prototype. Unsettled ownership may pause requests and require app restart. Bounds do not promise Windows callbacks arrive.

## Cache lifetime

| Concept | Current lifetime |
| --- | --- |
| Advertised name | Positive PTR TTL, capped at one day; zero TTL/expiry removes it |
| Cached resolved record | Current advertisement expiry; renewed PTR extends it |
| Resume/restart cache | Ten seconds until advertised/resolved again |
| Pending logical request | Five-second deadline and exact-token retirement |
| Resource owner | Browse terminal plus tracked call/consumer completion, or observed exact-child exit |

There is **no fixed ninety-second cached-record timeout in current main**. Cache eviction alone need not remove still-advertised knowledge.

```mermaid
flowchart TD
  PTR[PTR refreshes advertisement TTL]
  Q[Current epoch and token; five-second deadline]
  C[Valid reply updates cache]
  B[Failure retires token; bounded backoff]
  E[Observed helper exit releases owner]
  R[PTR removal or expiry removes name and cache]
  PTR --> Q
  Q --> C
  Q --> B
  B --> Q
  Q --> E
  C --> Q
  PTR --> R
```

The one-second tick checks advertisement/cache expiry, logical deadlines and retry schedules. This is scheduling resolution, not a hardware disappearance alarm. A failed network packet does not establish a lost Windows callback.

## Fields the resolve keeps

| Field | Consumer |
| --- | --- |
| `deviceid` or RAOP prefix before `@` | Physical catalog identity |
| RAOP suffix/AirPlay-first metadata | Name/address selection |
| `model`, otherwise `am` | Model |
| `tsid`, `gpn`, `igl` | Stereo id/name/leader |
| `pk` | Compatible-endpoint check; ordinal comparison |
| RAOP `cn` | Later playback codec list |
| Other TXT, including `gid` | Retained, not the stereo grouping key |

`Discovery resolved:` and a resolve-triggered Changed publication depend on cache absence **or changed description**. Describe compares instance, service type, address, port, deviceid, model/am and tsid; it omits gpn, igl, pk and cn. A refresh changing only those omitted TXT fields replaces the cached record but does not immediately log or publish. A later publication from another qualifying change/removal exposes the latest cached metadata to aggregation. This is a current notification limitation, with no dedicated TXT-only publication regression claiming its repair.

Repeated descriptions can follow restart/expiry/removal or a compared metadata change/reversion; they identify no Wi-Fi scan, receiver reboot or transport health. [After discovery](after-discovery.md) and [Playback coupling](playback-coupling.md) keep availability separate from owned transport.

## Failures during browse

DNS-SD/adapter unavailability, failed admission/callback, timeout, cancellation/exit uncertainty and capacity pause report through `Failed` to the application notice. Notices do not independently mutate the map; publication carries availability. Manual rows come from settings and remain independent.

## Check mode

`--discovery-check` is a real eight-second DNS-only check, not executed by this documentation refresh. `--dns-resolve-helper-check` uses six actual child processes with synthetic responses/watchdog/cancellation and no DNS query. Its previous receipt remains bound to `dc521ae`; it is not relabeled as new-main live DNS. Diagnostic flags select isolated startup data; ordinary helpers dispatch even earlier.

## Source and verification scope

Current [DiscoveryLifecycleTests](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/DiscoveryLifecycleTests.cs), [NativeDnsOperationTests](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/NativeDnsOperationTests.cs) and [ResolveProcessTests](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Tests/ResolveProcessTests.cs) inject DNS/clock/child boundaries, use real unmanaged fixture buffers and explicit gates, and cover missing/inline/late callbacks, deadline races, token/epoch replacement, failed Start/Cancel/Kill/exit wait, owner caps, one hundred finite attempts/scoped cycles, restart/dispose, PTR expiry and selected-adapter no-fallback.

The original intermittent `DisposeFromChangedSubscriberCannotCancelWhileHoldingPublicationLock` failure was a fake drainer joining its own callback. Both drainer schedules now use explicit signals/deferred fake cancellation. A later unchanged-runtime green was not its repair; the original log established no production deadlock. [D acceptance](../issues/GROUP-D-ACCEPTANCE.md) preserves the failed run, controlled diagnosis and mutations. Current normal CI passes 667 Core plus stability/bundle checks; packaged UI qualification is separate. No new live DNS/receiver/route/acoustic experiment ran here.

## Historical baseline and superseded lifetime conclusions

At [`41190e0`](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L83), a pending resolve had no deadline. Ninety-second cache expiry could remove its instance while leaving `_pending`; another PTR skipped resolving until completion/removal/restart cleared it. [Report 04](../issues/ISSUE-04-HIGH-dns-resolve-without-deadline-stalls-discovery.md) preserves that root cause.

## Historical in-process Group D implementation (48e7ab62; lifetime conclusions superseded)

Earlier D treated resolve callback completion as settlement. Review of Microsoft's [Resolve](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsserviceresolve) and [ResolveCancel](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsserviceresolvecancel) contracts did not establish quiescence. The old six-operation live DNS probe's zero-owner counters therefore establish no general safe resolve reclamation. Browse terminal status was separately corrected from 995 to documented 1223. The conservative retained-input prototype exhausted per-name capacity after two browse cycles and was rejected. Current production uses helpers. D acceptance retains old observations with the retracted lifetime conclusion explicit.

## Earlier Group B implementation (through 9e894ab)

B changed the publication consumer independently of D's resolver. Its dated fake retention evidence is in [B acceptance](../issues/GROUP-B-ACCEPTANCE.md); current behavior is the production narrative above.
