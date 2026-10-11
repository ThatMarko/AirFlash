# [HIGH] A resolve without completion has no deadline and stalls discovery

- **Issue ID**: ISSUE-04
- **Implementation plan**: [Work package ISSUE-04](IMPLEMENTATION_PLAN.md#issue-04)
- **PR group**: D — DNS operation lifecycle
- **Severity**: HIGH — a stalled resolve can indefinitely suppress rediscovery and can expire a receiver participating in playback.
- **Kind**: Defect
- **Subsystem**: Windows DNS-SD resolve lifecycle
- **Status**: Implemented in local main `9e894ab`; Group D remains unpushed/unpublished and unmerged upstream. Main execution/testing is deferred until after reboot.
- **Runtime baseline**: upstream `41190e0`, retained by documentation commit `c077a05`
- **Evidence status**: Original logical-state probes executed against unchanged source; fixed coordinator/native-owner fault injections and mock WPF checks passed. [Group D acceptance](GROUP-D-ACCEPTANCE.md) distinguishes these from unmeasured Windows/AP/IGMP failures and native leaks.
- **Implementation status**: Corrected feature `48e7ab62` (after initial `4c79f22`) is now integrated locally. [Feature acceptance](GROUP-D-ACCEPTANCE.md) separates initial/follow-up and bounded Windows DNS evidence; [integration acceptance](ABCD-INTEGRATION-ACCEPTANCE.md) separates earlier compositions from this source-only main audit. The original behavior and proposed fixture/architecture sections below remain historical.
- **Target file**: [WindowsDiscovery.cs](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L82)
- **Analysis context**: [Discovery](../analysis/discovery.md)

## Current local implementation

[WindowsDiscovery](../../desktop/AirFlash.App/Services/WindowsDiscovery.cs) separates PTR advertisement expiry, cached records, logical pending requests and retained native owners. Five-second monotonic deadlines apply to initial and refresh resolves, checked by a one-second maintenance tick and completion. Failure retires the exact token and retries with 1–30-second exponential backoff while still advertised; old token/epoch results cannot mutate newer requests. Cache expiry no longer removes a still-advertised name. PTR removal/expiry retires that name. [DiscoveryPolicy](../../desktop/AirFlash.App/Services/DnsServiceApi.cs) caps two resolves per name and 64 owners per instance, including retired epochs.

[NativeDnsServiceApi](../../desktop/AirFlash.App/Services/NativeDnsServiceApi.cs) also caps process-wide retained owners at 128. Heap input buffers and opaque tokens survive cancellation return/deadline until terminal callback or non-pending admission plus native-call/managed-callback frame completion. Browse cancellation terminal status is the documented 1223; resolve callbacks independently settle. Publication/callback cancellation is deferred to avoid blocking native calls under those stacks, and cancellation failures remain observable. The selected unavailable interface retains no fallback. B separately prevents passive publication loss from revoking an owned stream.

Source main `9e894ab133f8cbdb3b5f5ac1f411023b182e65d7`, [coordinator](../../desktop/AirFlash.Tests/DiscoveryLifecycleTests.cs) and [raw native-owner](../../desktop/AirFlash.Tests/NativeDnsOperationTests.cs) assertions were read in this audit. They were not executed. Historical probes/results retain their recorded hashes and limits; callback loss, sustained growth and receiver incident causes are not established.

## 1. Original baseline behavior and impact

Each resolve records its operation token in `_pending`, and another resolve for that name is skipped while that token exists. There is no deadline that retires a resolve when its completion callback never arrives. A first resolve that remains pending never produces a receiver; a later refresh resolve that remains pending can outlive the cached record's expiry and prevent fresh resolution after that record disappears.

Operations retain their registered managed object and unmanaged name/cancel allocations until completion or a cleanup path. This is retained resource ownership during a stalled operation, not proof of an irreversible or cumulative leak: Restart and Dispose dispose registered operations. A PTR removal clears the pending marker but does not itself dispose the old native resolve, so repeated remove/add sequences with suppressed completions can retain additional operations until later cleanup. Actual loss of callbacks and actual allocation growth on Windows have not been demonstrated here. See [Discovery](../analysis/discovery.md).

## 2. Bounded root cause

| Mechanism | Evidence |
| --- | --- |
| Duplicate resolve skipped; pending marker records the token | [WindowsDiscovery.cs:125–134](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L125) |
| Expiry removes `_records` and `_instances`, but not `_pending` | [WindowsDiscovery.cs:91–99](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L91) |
| Completion normally clears pending and disposes the operation | [WindowsDiscovery.cs:136–162](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L136) |
| PTR removal clears pending but does not cancel that operation | [WindowsDiscovery.cs:112–118](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L112) |
| Token registration, allocations, completion and cancellation disposal | [WindowsDiscovery.cs:204–235](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L204) |
| Restart and owner Dispose provide recovery/cleanup | [WindowsDiscovery.cs:47–64](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L47), [179–190](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L179) |

The 90-second expiry is checked on 20-second refresh ticks. After expiry removes the name, the timer no longer enumerates it. A new PTR can restore `_instances`, but `Resolve` still skips while `_pending` contains the old token. A late valid callback can recover the name if `_instances` still contains it or a newer PTR restored it; otherwise that callback only clears pending and another PTR is needed. A removal followed by a new PTR, or Restart, also restores the ability to issue another resolve. The old report's “permanent until app restart,” Wi-Fi/IGMP attribution, and blanket “never freed” claims are removed.

## 3. Original fault-injection reproduction plan

At `41190e0` the constructor injects the adapter catalog, not DNS-SD calls or time. The original reproduction plan needs a fake DNS-SD API and clock seam, or an extracted resolve-lifecycle component; neither existed at that baseline. Group D later added those seams and assertions, with execution recorded separately in its acceptance record.

1. In that fixture, deliver a synthetic PTR for `Fixture._airplay._tcp.local`. Have its resolve start return pending, then deliberately withhold completion.
2. Advance several refresh ticks. Source-predicted result: one pending token remains and no second resolve is issued, although the name remains advertised.
3. In a second case, complete the initial resolve, then suppress a refresh resolve. Advance beyond the record's 90-second deadline and process a refresh tick. Source-predicted result: record/name are evicted while its pending token and operation remain retained.
4. Deliver another PTR for the same name. Source-predicted result: the name is restored but no fresh resolve is issued while the old token remains.
5. Complete the old token after that PTR, and separately test removal/new PTR and Restart. Confirm those existing recovery paths rather than assuming permanent loss.

No mDNS packet dropping or native pointer manipulation is required. Do not claim this fixture establishes that Windows fails to complete a request after ordinary network packet loss.

## 4. Original architecture proposal

Track each resolve's token, epoch, start/deadline, cancellation state, and native resource owner. Add a bounded deadline policy for resolves independently of record TTL, with a documented retry/backoff policy for still-advertised names. On deadline/removal, retire the matching token so it cannot publish stale data, request cancellation, and permit a bounded replacement resolve.

Do not merely remove `_pending` on record expiry: that misses an initial resolve with no record, does not release the old operation, and allows an old native callback to race a replacement. Preserve token/epoch matching so an old completion cannot clear a newer request. Native request/cancel memory must follow the DNS API's safe cancellation/completion lifetime; do not free a cancel handle solely because a managed timer elapsed. Cancellation failures must retain safe ownership and surface a bounded diagnostic/recovery policy rather than silently abandoning pointers. Resource reclamation and retry concurrency need separate tests.

Keep manual receivers, interface selection, ReceiverCatalog identity, schema 2, and JSONL v1 unchanged. An unavailable selected NIC must stay paused without browsing interface 0. Interaction with discovery-driven playback interruption is tracked by [ISSUE-03](ISSUE-03-HIGH-discovery-disconnects-active-playback.md).

## 5. Acceptance criteria and verification

- A suppressed first completion reaches its configured deadline and permits a bounded retry; no cached record is required to trigger recovery.
- A suppressed refresh completion cannot indefinitely suppress a later PTR-driven resolve after expiry.
- A late old callback cannot publish over, remove, or complete a newer pending token.
- In the fake API, terminal completion or cancellation settlement returns operation registration/allocation counts to baseline exactly once; cancellation return alone does not authorize reclamation, and removal/owner shutdown are idempotent.
- Failed/late cancellation leaves safely owned native memory, no use-after-free, and a documented diagnostic/recovery path.
- Repeated failed resolves are backoff/rate bounded; a missing selected NIC never falls back to all interfaces.
- Manual rows and their sessions are unaffected by resolver housekeeping.

[DiscoveryTests](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/DiscoveryTests.cs#L9) covers pure adapter selection and aggregation, not native resolve lifecycle. There is no existing fake-DNS/fake-clock assertion for missing callbacks, cancellation lifetime, pending-token replacement, or allocation reclamation. Those tests are required before applying this proposal; optional live DNS-SD verification remains separate work.

Safety: no hardware, network faults, real addresses, or credentials are needed. Any later authorized audio qualification retains gain ≤0.1 and duration ≤5 seconds. No upstream issue #6/#7 or dev-branch fix is claimed.
