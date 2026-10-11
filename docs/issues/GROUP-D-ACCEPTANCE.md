# Group D feature acceptance

Initial verification completed on 2026-10-08; additional testing below advanced local branch `codex/fix-discovery-resolve` to **`48e7ab62dd034085a53825d17e52b6bbf3714315`**. Report 04 is implemented in two commits based on original stable `41190e0d13a63a714c08dffe73ababca1804875c`; the initial implementation head was `4c79f2254910365dc9f5931fcdc52851c7043abe`. Worktree: `C:\Users\marko\.codex\worktrees\fix-discovery-resolve\AirFlash`. Neither commit has been pushed or published, and neither is integrated into fork/upstream main. Group C was completed first on its separate branch. Upstream has since advanced; publication requires checking the branch against that actual updated base.

## Implemented behavior

- `WindowsDiscovery` uses the injectable `IDnsServiceApi` and monotonic `TimeProvider`. Advertisements, cached records, logical pending requests and retained native owners have separate state. Only the current epoch and exact current owner/token may change the catalog or pending request.
- Resolves have a five-second deadline, checked by a one-second maintenance tick and by completion itself. A completion at the deadline is rejected regardless of timer/callback order. Failures retry after 1, 2, 4, 8, 16 then at most 30 seconds; repeated PTRs do not reset backoff. Successful records refresh after 20 seconds and expire after 90 seconds. Cache expiry preserves still-advertised names; PTR TTL expiry/removal retires their logical requests and removes cached data.
- Capacity includes old, cancelled and retired owners: at most two resolves per name and 64 total owners per discovery instance, plus 128 owners across all native API instances in the process. Capacity pauses affected requests with a diagnostic; they remain eligible once ownership settles. A callback that never settles may require restarting the app. Restart and Dispose do not reset these ownership caps.
- Native requests, query names and cancel handles are retained in heap buffers. Static callbacks use never-reused opaque tokens. Inputs are reclaimed once, after a non-pending Start has returned or terminal completion is observed, and only after native-call and managed-callback frames end. Cancel success and timeout alone never authorize reclamation. Returned instances/record lists are freed on success, failure, malformed, obsolete and unknown-context paths.
- Cancellation from any native callback stack, or against an active callback, uses a coalesced worker rather than waiting in that callback. Deferred errors reach the existing localized warning through weak handlers, removed at settlement and ignored after disposal. Reentrant publication defers native starts/cancels until its lock is released, prunes/coalesces work by token and rechecks admission before Start. Individual observer/logger exceptions cannot abandon that batch.
- The x64 request/instance layouts are asserted. Property count is unsigned, nonempty property arrays require both pointers, and PTR TTL is read at offset 24 independently of flags at offset 20. Selected unavailable adapters remain paused without fallback to index 0. Manual receiver settings, playback ownership, schema 2, JSONL v1 and Rust sources are unchanged.

## API lifetime decisions

Exa research reviewed 30 search results across three API/ownership workstreams; implementation decisions rely on primary Microsoft documentation and SDK headers. [Resolve](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsserviceresolve) and [Browse](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsservicebrowse) document `DNS_REQUEST_PENDING` (9506) as successful asynchronous admission. Returned status 0 is not treated as a pending request. Inline completion takes precedence over subsequent Start bookkeeping.

[BrowseCancel](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsservicebrowsecancel) promises a further callback with `ERROR_CANCELLED`, whose [documented value is 1223](https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--1000-1299-). The initial implementation and this record incorrectly used 995 (`ERROR_OPERATION_ABORTED`); the correction and its reproduced failure are recorded below. [ResolveCancel](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsserviceresolvecancel) does not specify comparable delivery/return ordering. Neither establishes general callback quiescence on return or callback-safe joining. Heap retention and callback-safe cancellation are conservative responses to those unspecified ordering guarantees, not evidence that Windows loses callbacks or self-deadlocks. Layouts were checked against [Microsoft's SDK header](https://github.com/microsoft/win32metadata/blob/main/generation/WinSDK/RecompiledIdlHeaders/um/WinDNS.h) and [DNS_RECORDW](https://learn.microsoft.com/en-us/windows/win32/api/windnsdef/ns-windnsdef-dns_recordw).

## Initial verification on 2026-10-08

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

Initial verification used synthetic records, fake DNS/native calls and isolated window/tray identities. It used no production DNS-SD query, real settings, credentials, receiver control, capture, endpoint mutation, network fault, MSI installation or release/version reservation. WPF smoke checks use mock discovery and establish application compatibility, not DNS behavior. The later bounded DNS-only observation below adds a specific Windows cancellation trace without qualifying all native ordering or long-running allocation behavior. Unsettled ownership is bounded rather than freed on an assumed timeout; process exit is final recovery if the OS never settles it.

## Additional testing and fix on 2026-10-09

A documented-status regression ran against unchanged `4c79f22` and failed: after a browse cancel, terminal callback 1223 did not settle the owner because the implementation checked 995. That retained inputs and consumed the bounded native ownership budget. This is a defect in the initial Group D implementation, separate from the two original logical-state probes above.

Commit `48e7ab62` uses the documented browse terminal status 1223. Five added cases verify settlement, retain ownership on status 0/5/995 after cancellation, and keep inputs valid when the terminal callback runs during Cancel until that native call returns. Resolve callbacks remain independently terminal; no timer-only reclamation or assumed callback quiescence was added.

| Additional check | Result |
| --- | --- |
| Documented-status case on unchanged initial implementation | One failed after successful build |
| Focused coordinator/native-owner suite | 69 passed; zero failures/skips |
| Repeated focused suite | Ten complete passes, 690 case executions; no hangs or failures |
| Complete Core suite, Debug and Release | 232 passed in each configuration; zero failures/skips |
| Debug/Release solution builds | SDK 10.0.401; zero warnings/errors |
| Fresh full isolated WPF checks | 149 English and 149 Chinese passed on corrected source |
| Bounded live Windows DNS-SD probe | Two browse operations and four successful resolves; both browses returned terminal 1223 during Cancel; final owner/buffer counters zero |
| Complete feature scope | Two commits, eight desktop runtime/test/localization files; fork-only planning remains excluded |

The DNS-only probe used physical interface index 4 and invoked the production callback thunk while recording raw callback status. In this finite run all owners settled and no Cancel timed out. It performed no playback, AirPlay sender command, mute, application-settings access or credential access. This observation establishes this run's Windows API behavior; it does not prove all Windows ordering, discovery fault recovery, sustained allocation behavior or acoustic reliability.

Implementation-worktree evidence is in `artifacts/more-testing-2026-10-09/d-fix-final-receipt.json`, `live-dns-receipt.json` and associated TRX/probe sources. The final test receipt was captured before committing and labels its corrected files `uncommittedFiles`; those SHA-256 values match committed `48e7ab62`: NativeDnsServiceApi `e84d89a590bcc390dc029b7a047d6a0a9abe2113d99144cf9e021a0eb75089e9`, NativeDnsOperationTests `dede2306c94bef78dd378a15c2aa82fc49d950f7eaa27ab7985ba24ca8491e0d`. Fresh UI reports are in the fork checkout's ignored `artifacts/abcd-extra-2026-10-09/d-ui-en-US/` and `d-ui-zh-CN/`.

Final local [A/B/C/D source](ABCD-INTEGRATION-ACCEPTANCE.md) `6fe467b` passed 422 Core per configuration, six 579/571-check portable runs with exact catalog restoration and MSI payload/refresh checks. D production/standalone results are unchanged; WPF discovery is simulated and the bounded API observation above remains separate. Before publication, review the standalone branch against actual updated main and await Task 3 for D. [C acceptance](GROUP-C-ACCEPTANCE.md) records settings; neither is integrated into either main.
