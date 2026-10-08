# Group D feature acceptance

Verified on 2026-10-08. Report 04 is implemented on local branch `codex/fix-discovery-resolve`, head `4c79f2254910365dc9f5931fcdc52851c7043abe`, directly based on refreshed original stable `41190e0d13a63a714c08dffe73ababca1804875c`. Worktree: `C:\Users\marko\.codex\worktrees\fix-discovery-resolve\AirFlash`. This branch has not been pushed, published, or merged into fork/upstream main. Group C was completed first on its separate branch.

## Implemented behavior

- `WindowsDiscovery` uses the injectable `IDnsServiceApi` and monotonic `TimeProvider`. Advertisements, cached records, logical pending requests and retained native owners have separate state. Only the current epoch and exact current owner/token may change the catalog or pending request.
- Resolves have a five-second deadline, checked by a one-second maintenance tick and by completion itself. A completion at the deadline is rejected regardless of timer/callback order. Failures retry after 1, 2, 4, 8, 16 then at most 30 seconds; repeated PTRs do not reset backoff. Successful records refresh after 20 seconds and expire after 90 seconds. Cache expiry preserves still-advertised names; PTR TTL expiry/removal retires their logical requests and removes cached data.
- Capacity includes old, cancelled and retired owners: at most two resolves per name and 64 total owners per discovery instance, plus 128 owners across all native API instances in the process. Capacity pauses affected requests with a diagnostic; they remain eligible once ownership settles. A callback that never settles may require restarting the app. Restart and Dispose do not reset these ownership caps.
- Native requests, query names and cancel handles are retained in heap buffers. Static callbacks use never-reused opaque tokens. Inputs are reclaimed once, after a non-pending Start has returned or terminal completion is observed, and only after native-call and managed-callback frames end. Cancel success and timeout alone never authorize reclamation. Returned instances/record lists are freed on success, failure, malformed, obsolete and unknown-context paths.
- Cancellation from any native callback stack, or against an active callback, uses a coalesced worker rather than waiting in that callback. Deferred errors reach the existing localized warning through weak handlers, removed at settlement and ignored after disposal. Reentrant publication defers native starts/cancels until its lock is released, prunes/coalesces work by token and rechecks admission before Start. Individual observer/logger exceptions cannot abandon that batch.
- The x64 request/instance layouts are asserted. Property count is unsigned, nonempty property arrays require both pointers, and PTR TTL is read at offset 24 independently of flags at offset 20. Selected unavailable adapters remain paused without fallback to index 0. Manual receiver settings, playback ownership, schema 2, JSONL v1 and Rust sources are unchanged.

## API lifetime decisions

Exa research reviewed 30 search results across three API/ownership workstreams; implementation decisions rely on primary Microsoft documentation and SDK headers. [Resolve](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsserviceresolve) and [Browse](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsservicebrowse) document `DNS_REQUEST_PENDING` (9506) as successful asynchronous admission. Returned status 0 is not treated as a pending request. Inline completion takes precedence over subsequent Start bookkeeping.

[BrowseCancel](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsservicebrowsecancel) promises a further callback with `ERROR_CANCELLED` (995); [ResolveCancel](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsserviceresolvecancel) does not specify comparable delivery/return ordering. Neither establishes general callback quiescence on return or callback-safe joining. Heap retention and callback-safe cancellation are conservative responses to those unspecified ordering guarantees, not evidence that Windows loses callbacks or self-deadlocks. Layouts were checked against [Microsoft's SDK header](https://github.com/microsoft/win32metadata/blob/main/generation/WinSDK/RecompiledIdlHeaders/um/WinDNS.h) and [DNS_RECORDW](https://learn.microsoft.com/en-us/windows/win32/api/windnsdef/ns-windnsdef-dns_recordw).

## Executed verification

| Check | Result |
| --- | --- |
| Original-source logical-state probes | Two desired assertions failed after successful build; original coordinator blob unchanged; no native operation started |
| Focused coordinator/native-owner regressions | 64 passed; zero failures/skips/compiler or analyzer warnings |
| Complete owning Core suite, Release | 227 passed; zero failures/skips |
| Locked restore and final Debug/Release solution builds | SDK 10.0.401; zero build warnings/errors |
| Full isolated WPF suite | 149 English and 149 Chinese checks passed |
| Publication regression comparison | Same gated fixture failed with only feature deferral reverted, then passed with it restored; not an upstream baseline |
| Independent final source/assertion review | Clear after failure-diagnostic, publication, callback-cancellation and batch exception findings were resolved |
| Complete diff and outgoing commit | Eight desktop runtime/test/localization files; no fork analysis/issues/planning |
| C/D virtual merge comparison | Clean after placing D's five translations beside the existing discovery messages; all translation key/value pairs unchanged |

The original probes seed expired-cache, advertised-name and blocked-token state into unchanged `WindowsDiscovery` and invoke its existing refresh path. They reproduce loss of still-advertised names and failure to clear an expired blocking token. They do not invoke original DNS-SD or establish native callback-loss/cancellation behavior.

Fixed regressions execute the actual source-linked coordinator and native owner with fake DNS calls and monotonic time. They cover suppressed initial/refresh completion, both deadline orderings, inline/late callbacks, failure/backoff, removal/new PTR, obsolete tokens/epochs, NIC pause/recovery, restart/disposal, local/global caps and eventual capacity recovery. Raw fixtures allocate actual unmanaged inputs and invoke the stored callback pointer, assert buffers remain valid during native/callback frames, free returned data exactly once and restore owner/allocation counters after safe settlement. Cancellation and settlement are independently controlled. Gated callback fixtures model possible cancellation joins without asserting that Windows implements those joins.

The first focused pass caught a missing failed-Start notice. Independent review and controlled comparisons exposed reentrant native calls under publication; subsequent review covered callback-stack cancellation and observer exceptions losing a detached work batch. Final checks include every correction. An initial final-restore command used an empty package cache/feed context; the corrected repository-scoped command used the verified shared cache and explicit NuGet feed, with locked dependencies and no package/source changes. The final C/D branch comparison found overlapping translation appends; D's entries were relocated beside discovery messages, with exact key/value parity, and final builds/Core/UI checks rerun. A clean virtual merge does not establish combined runtime behavior or integrate either branch into main.

## Evidence and limits

Ignored evidence is retained in the implementation worktree: `artifacts/group-d-tests/focused-final/`, `artifacts/group-d-tests/full-final/`, `artifacts/group-d-baseline/`, `artifacts/group-d-publication-regression/`, `artifacts/group-d-ui-en/` and `artifacts/group-d-ui-zh/`. Receipts bind actual counts, tested source hashes, TRX and UI reports. The original comparison worktree was archived after copying its evidence.

All tests use synthetic records, fake DNS/native calls and isolated window/tray identities. No production DNS-SD query, real settings, credentials, receivers, capture, endpoints, network fault, MSI installation or release/version reservation was used. WPF smoke checks use mock discovery and establish application compatibility, not DNS behavior. Actual Windows callback/cancellation ordering, real receiver discovery and native allocation growth remain unqualified. Unsettled ownership is bounded rather than freed on an assumed timeout; process exit is final recovery if the OS never settles it.

Next: await explicit Task 3 activation for publication. [Group C acceptance](GROUP-C-ACCEPTANCE.md) records the separately completed configuration package; neither package is integrated into either main branch.
