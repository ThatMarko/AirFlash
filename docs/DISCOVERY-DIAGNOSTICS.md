# Discovery CPU and resource verification

AirFlash browses `_airplay._tcp.local` and `_raop._tcp.local` only while a receiver
list is visible, or during the ten-second automatic-connection startup window.
It admits one current resolve attempt per service instance and stops every browse
and resolve attempt on suspension. Device removals and PTR TTL expiry retire
individual resolvers. Cached records survive suspension and
receive a ten-second confirmation window when discovery resumes.

The first usable resolve result has a five-second monotonic deadline. Failures
retire that exact request and retry with bounded backoff while its advertisement
remains valid. A successful copied result schedules a fresh query after twenty
seconds; the existing catalog stays visible during refresh or retry until its PTR
expires. Native calls and helper startup run off the UI dispatcher.

Cancellation success does not establish callback quiescence. Browse cancellation
has a [documented final status 1223](https://learn.microsoft.com/windows/win32/api/windns/nf-windns-dnsservicebrowsecancel);
its inputs settle after that terminal indication and completion of tracked
Start/Cancel calls and managed payload/consumer sections. Those counters do not
observe arbitrary unmanaged callback return frames.
[Resolve callbacks describe individual results](https://learn.microsoft.com/windows/win32/api/windns/nc-windns-dns_service_resolve_complete),
and [resolve cancellation](https://learn.microsoft.com/windows/win32/api/windns/nf-windns-dnsserviceresolvecancel)
does not specify a final notification. Each resolve therefore runs in a hidden
AirFlash helper process using the same Windows API and exact selected interface.
It copies at most one bounded reply. Native inputs remain owned inside that child;
the parent releases its process owner only after successfully observing exit.
A result, successful kill request, failed wait, or logical timeout cannot release
that owner. [Process termination frees its address space](https://learn.microsoft.com/windows/win32/procthread/terminating-a-process);
[Kill is asynchronous](https://learn.microsoft.com/dotnet/api/system.diagnostics.process.kill), so the parent waits explicitly.

The helper enters before logging, WPF or any application data access. It starts no
descendants. A dedicated watchdog terminates it six seconds after helper entry,
even if native startup blocks or its parent exits. The parent also terminates it
after a reply, cancellation or its process deadline. Termination/wait failures
keep the exact process object rooted and report a diagnostic. Admission includes
starting and killed-but-unobserved processes, with eight helpers globally, two
unsettled resolves per name, 64 owners per discovery instance and 128 owners
globally across native browse and helper processes, including retired epochs.
Only unresolved ownership consumes these limits; ordinary settled suspend/resume
cycles reuse their slots. A limit pauses admission until ownership settles.

Per-query process startup adds cost compared with persistent native queries. The
receiver-free `--dns-resolve-helper-check --output <report.json>` measures synthetic
startup/reply time, observes normal and parent-killed exits, and verifies the
watchdog without making a DNS call. Its timings do not measure network discovery.

The opt-in `--discovery-perf` check uses real Windows DNS-SD without creating an
audio session, reading saved settings or credentials, or registering a tray icon:

```powershell
./scripts/dotnet.ps1 build AirFlash.App/AirFlash.App.csproj
& ./desktop/AirFlash.App/bin/Debug/net10.0-windows/win-x64/AirFlash.exe --discovery-perf --output "$PWD/artifacts/discovery-perf/report.json"
```

Allow roughly five to seven minutes. The check samples 150 seconds of active discovery,
opens and closes 25 additional sessions, then samples 120 seconds with discovery
suspended. The report contains per-second CPU percentages normalized by logical
processor count, thread and handle counts, private bytes, receiver counts, and
logical query counts and retained native ownership. `all_queries_stopped` describes
logical admission only; it does not claim all native inputs were released.
Resolve helper ownership is reported separately; `resolve_settlement` identifies
observed helper exit. Parent CPU/private memory samples exclude helper processes,
so they do not measure total discovery CPU or memory.
Compare stable samples after initial runtime startup;
OS thread-pool and runtime caches can retain threads and memory after cancellation.

Unit tests inject a DNS-SD adapter, process factory and the raw Windows API boundary to cover
early and repeated callbacks, cancellation callbacks on another thread, startup
failure, bounded retries, interface changes, PTR expiry and 100 fake open/close
cycles. The existing `--ui-smoke` mode additionally verifies demand shared between
the panel and receiver settings, the startup deadline, and playback continuity.
Raw fixtures separately verify persistent and late results, retained input buffers,
callback-safe cancellation and result cleanup exceptions. Only explicit teardown
of a known quiescent fake API releases unresolved synthetic resolve owners; that
teardown is not a Windows API guarantee.
Process fixtures independently control copied data and exit, blocked startup,
termination failures, exit-wait failures and later recovery, capacity across API
instances, malformed/bounded output and repeated slot reuse.

## Historical local verification, October 11, 2026

The measurements below were recorded by upstream commit
[`838e08a`](https://github.com/Ding-Kyoma/AirFlash/commit/838e08a882d0e38697c07ba0a5dc2f3dde0e7f6c),
whose cancellation policy freed query inputs on successful cancellation. They do
not verify the helper ownership policy or establish OS quiescence.

On a machine with 16 logical processors, the installed 0.3.0 process showed a
5.37% maximum and 0.99% average CPU during a 120-second baseline sample. The new
isolated discovery host averaged 0.031%, with a 0.097% maximum, during its
120-second suspended phase. All 25 additional sessions released their queries;
the hidden phase had zero active queries, and threads fell from 22 to 12.
The 150-second active phase retained both aggregated receivers and used at most
eight native queries (two browsers and six service resolvers).

These are discovery-host measurements, not a full playback benchmark. The existing
UI performance check also misses its cached-page 100 ms target on this machine:
approximately 227 ms p95 after this change versus 221 ms on unchanged HEAD.
The network page already refreshes adapter information synchronously; dispatcher,
apply-feedback, equalizer and endpoint-enumeration targets passed in both runs.
