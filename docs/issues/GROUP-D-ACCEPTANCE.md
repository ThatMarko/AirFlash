# Group D acceptance — bounded DNS operation ownership

## Current Main integration — 2026-10-12

Reviewed fork runtime baseline is `8989b3609dd1294f4d1d6eb1502f345cc22bc382`, with source trees equal to qualified code `4ae1db4f01dcf7857482dd136ea8d9dffa3aa8a1`. The feature head below is an ancestor of Main and is pushed to origin. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`. Public complete diffs and every outgoing commit exclude private planning. This is fork implementation acceptance, not upstream integration. [Combined Main acceptance](CONTINUATION-ACCEPTANCE.md) records 667 Core/124 Python/native/package/UI qualification and limits.

Feature `codex/fix-discovery-resolve`, head `277efc5b389acd2b9699c9b25baf83450274096f`; worktree `fix-discovery-resolve/AirFlash`. Report 04 is complete. Independent review is clear. Refreshed qualification passes 108 focused/295 owning Core tests, a zero-warning/error Release build and six actual synthetic helper-process checks.

Current implementation: [WindowsDiscovery](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Services/WindowsDiscovery.cs), [process API](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Services/ResolveProcessDnsServiceApi.cs), [helper](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Services/DnsResolveHelper.cs), [native browse ownership](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Services/NativeDnsServiceApi.cs) and [regressions](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Tests/NativeDnsOperationTests.cs).

## Implemented lifetime contract

Logical advertisement, cached TTL, pending token, epoch, deadline and retry state are separate from native/process owners. Five-second resolve deadlines and monotonic maintenance/completion checks reject expired results. Backoff is bounded at 30 seconds; successful resolution refreshes after 20 seconds. Current cache lifetime follows advertisement expiry, renewed PTR extends it, and restart gives retained cache ten seconds for rediscovery. There is no fixed ninety-second cache timeout in current Main. Obsolete same-epoch tokens and old epochs cannot publish or clear newer work.

The parent roots a per-query helper-process owner **before Start**. A global **eight-owner cap** includes blocked/failed starts and unsettled children across epochs and API instances. Native resolve inputs remain rooted in that child until exit. A bounded first JSON reply and six-second child watchdog constrain the helper attempt. Capacity is reusable only after successful observation of the exact child's exit. Reply, timeout, native Cancel/Kill result or failed exit wait alone is insufficient. Read/Kill failure may still settle if exit is confirmed; failed exit observation retains ownership until a later successful observation.

Browse has a separate native owner contract: documented terminal cancel status 1223 and native/managed frame completion permit settlement. Callback-stack or publication recursion defers native work through a coalesced drainer. Inputs remain valid across active calls/callbacks; result allocations are freed on normal/malformed/obsolete paths. Observer/logger exceptions do not abandon detached batches. Selected missing/offline adapters stay paused without all-interface fallback.

## Discovery cancellation fixture

Original failing CI `38134996290` and documentation-only green `38135036967` remain separate. The later green did not repair the failure. A controlled outer publication recursion reproduced the original fake's circular wait: the drainer executing cancellation synchronously waited for its own callback to return. No production deadlock was established.

Correction `68cae6f` separates fake cancellation request from joining, controls both possible deferred-drain winners and asserts real unmanaged-owner buffers. Removing callback deferral fails the concurrent-drainer assertion; removing publication deferral fails coordinator cases. Mutations were restored byte-for-byte. Receipts remain in worktree `artifacts/cancellation-fixture-2026-10-11/`, including failed source, scheduling controls and corrected TRX.

## Stable refresh and lifetime correction — 2026-10-11

Rechecking ordinary Windows resolve result/Cancel contracts showed they do not establish callback quiescence. An older permanent retained-native-owner prototype exhausted one name after two ordinary queries and was rejected, even though bounded pointer retention avoided premature freeing. The current helper boundary replaces that prototype; its old reclamation description is not the Main implementation.

Historical refreshed `accb48a` passed 292 Core, warning-free build and six actual receiver-free helpers; exact-source [run 38166102166](https://github.com/ThatMarko/AirFlash/actions/runs/38166102166) passed native/managed/lint/static-CRT/compatibility checks plus helpers. Fakes cover missing/inline/late callbacks, failed/blocked starts, failed kill/wait, deadlines/retries, malformed wire output, old epochs/tokens, repeated ordinary queries and 100 browse cycles. Exit observation, not native cancel success, is the settlement assertion.

## API lifetime decisions

Primary [Resolve](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsserviceresolve), [ResolveCancel](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsserviceresolvecancel), [BrowseCancel](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsservicebrowsecancel), [system error values](https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--1000-1299-) and [SDK layouts](https://github.com/microsoft/win32metadata/blob/main/generation/WinSDK/RecompiledIdlHeaders/um/WinDNS.h) establish API assumptions. They do not establish callback loss or Windows deadlock. The current design uses process exit as the resolve reclamation boundary rather than unspecified native ordering.

## Initial verification on 2026-10-08

Historical original `41190e0` logical-state probes failed two assertions with unchanged coordinator blobs and no native query. Initial `4c79f22` passed 64 focused/227 Core and 149 WPF per language. Those initial owner tests are bounded historical evidence; their former resolve-settlement inference was superseded by the helper design.

Preserved evidence: `artifacts/group-d-baseline/`, `group-d-tests/`, `group-d-publication-regression/`, `group-d-ui-en/` and `group-d-ui-zh/`. The original probes establish expired logical blocking/cache behavior, not OS callback delivery.

## Additional testing and fix on 2026-10-09

Unchanged `4c79f22` failed the documented browse cancellation regression because it expected 995 rather than 1223. Corrected `48e7ab62` passed 69 focused cases, ten repeated suites/690 case executions, 232 Core in each configuration and 149 WPF per language with warning-free builds.

A finite historical DNS-only Windows probe observed two browses/four successful resolves and terminal 1223 during Cancel, with final counters zero. It did not call playback, mute, settings or credentials. It qualifies that run, not all ordering, fault behavior or the later helper implementation. Receipts `artifacts/more-testing-2026-10-09/d-fix-final-receipt.json` and `live-dns-receipt.json` preserve its exact source.

## Limits and publication

Current fixtures use synthetic DNS/native/process boundaries and isolated output; six helper checks execute actual child processes with synthetic behavior. No current live-network incident, receiver, capture or mute test is required or claimed. Helper startup cost on every machine, actual Windows DNS reliability and acoustic behavior remain unqualified. Logical no-fallback, manual persistence and playback ownership remain intact.

D is integrated and pushed to the fork; no upstream issue/PR is submitted. Recheck actual upstream and focused helper/browse contracts before selected Task 3 publication.
