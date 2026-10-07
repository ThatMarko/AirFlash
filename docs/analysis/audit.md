# Analysis audit — 2026-10-06

All sixteen original files in `docs/analysis/` were read and checked against their implementation, callers, state transitions, and relevant test sources. The resulting pages correct the claims that were false or too broad and identify what source inspection cannot establish. This report adds provenance and a cross-cutting verification record; the dedicated subsystem pages remain the detailed map.

## Pinned evidence

| Item | Audited identity |
| --- | --- |
| Fork/documentation baseline | `c077a0577de26ad7e44b8fd50f62456e12965fb0` (`origin/main` local ref at audit start) |
| Runtime baseline | `41190e0d13a63a714c08dffe73ababca1804875c` (`upstream/main` local ref at audit start) |
| Excluded development comparison | `67435a4e0df6a40e5e709e2b75dc9b24a457dda6`, inspected only to verify [upstream-dev.md](upstream-dev.md) |
| Local audit branch | `codex/audit-analysis`, created from the clean fork baseline |
| Allowed document changes | `docs/analysis/` only |

`git diff 41190e0 c077a05 -- desktop native scripts tests` is empty: the documentation commit does not change those implementation/test trees. GitHub's commit endpoint also confirmed the fork commit's identity and documentation purpose. No remote branch was refreshed or treated as a moving source of truth; no development code was merged. There were no repository `AGENTS.md` instructions in the checked workspace or parent paths.

The audit followed dependencies in both directions: DNS-SD callbacks through aggregation/catalog and view-model reconciliation; session commands through native validation/authentication/RTSP; capture through resampling/EQ/packetization/timing; and native events back through desktop state, volume, diagnostics, settings, and cleanup. Existing mock tests were read as evidence of intended coverage, not counted as passing without execution. External documentation is used only where an OS/API contract needs qualification; it does not substitute for this repository's behavior.

## Coverage of every original page

| Page | Verified touchpoints and material correction |
| --- | --- |
| [README](README.md) | Startup call order, default settings, process boundary, JSONL payload, and peer ownership. Pins the source separately from the docs commit; distinguishes a playback worker from its command process and native two-peer validation from discovered stereo membership |
| [Discovery](discovery.md) | DNS-SD record parsing, interface selection, epochs, cache refresh, pending tokens, native operation ownership, and aggregation. Separates failed callbacks from lost callbacks and implementation assumptions from Windows API guarantees |
| [After discovery](after-discovery.md) | Aggregation, compatible/conflicting endpoints, stereo rows, dispatcher reconciliation, manual/offline rows, and play/pair/auto-connect gates. Aggregation is on the discovery publisher's thread; reconciliation can affect an already running session. Attempted-receiver selection can block fallback to another row |
| [Identity](identity.md) | Canonical-id precedence, alias normalization/walk, conflict ownership, migration of options and active ids. Endpoint aliases/migrations are distinct from the persisted broadcast-only `ReceiverAliases` map |
| [Settings](settings.md) | Schema 1/2 loading, defaults, validation, migration, atomic save, three-way Apply, endpoints, shell, and update checks. Distinguishes settings serialization compatibility, draft/live state, and runtime behavior from proposed issue fixes |
| [Session](session.md) | Start/pair worker, generation filters, DNS resolution, retry budgets, signature restarts, standby, volume/EQ events, and cancellation. Retry exhaustion remains Error; fault/pre-fault diagnostics survive a successful retry until a fresh playback start |
| [Engine](engine.md) | Option validation, PTP/NTP creation, auth selection, sequential member setup, event/feedback/media watches, and teardown. Corrects PIN timing, exact feedback statuses, per-operation deadlines, native stereo limits, and worker lifetime |
| [Authentication](auth.md) | Transient M1–M4, persistent M1–M6, pair-verify, control ciphers, PIN exchange, and credential identity. PIN prompt follows M2, before SRP proof. Failures after a positive file-existence check do not fall back to transient; authentication-phase network faults can remain retryable |
| [Failures](failures.md) | Desktop outcomes, structured/fallback classification, native watches, process EOF/kill, and retry diagnostics. Stops are asynchronous/best effort; an error event does not by itself exit the process; 501 is not a transient feedback retry |
| [Playback coupling](playback-coupling.md) | Empty/incomplete snapshot through StopAsync, auto-connect gating, diagnostic reset, manual exemption, and mute cleanup. Removes incident-log correlation as proof of a specific network trigger or healthy native watches |
| [Capture and clocks](capture-clocks.md) | WASAPI formats/events, resampler, queue, QPC/Instant/SystemTime, pacing recovery, RTP sync and history. Partial underrun counts are not exact silence duration; queue cap/target describe local buffering, and the history age threshold is not a retention guarantee |
| [Control and mute](control-and-mute.md) | MTA ownership, captured mute bits, restore failure/retry, RTSP mutex, poll/confirmation order, routing and PTP receive filters. Cleanup precedes restore, mutex wait is outside feedback timing, and readable settled volume is not stereo target confirmation |
| [Credentials and IPC](credentials-and-ipc.md) | Engine extraction, pipe encodings/writers, line parser, command/event correlation, DPAPI format/path/save/load and pairing. Limits differ by direction and units; `hello` returns a reply rather than exiting the native command loop |
| [Equalizer](equalizer.md) | Defaults/validation, filters/headroom, preview ownership, persistence, native mailbox, DSP and crossfade tests. An acknowledgement validates command preparation for a matching stored start entry; stale sequences can be silently ignored, and no audio-application completion event exists |
| [Source check](source-check.md) | Rechecks both accepted and rejected earlier claims with code evidence. Qualifies underrun math, mute immediacy, routing, auth retryability, IPC bounds, EQ acknowledgement, and incident inference |
| [Upstream dev](upstream-dev.md) | Pinned local diff and referenced development files only. Removes unsupported live branch/release-publication claims, bounds negotiation/fallback assertions, and preserves exclusion from baseline behavior |

The separate pages are retained because they describe different ownership and failure boundaries. No original analysis page was deleted or consolidated into another topic. This report is the new seventeenth page.

## Corrections to the continuation briefing

Most architectural statements are supported, but the following formulations need the narrower wording now used in the subsystem pages.

| Briefing formulation | Verified formulation |
| --- | --- |
| JSONL has a maximum 64 KiB per line | Native command input rejects over 65,536 bytes, including newline. Desktop event reading checks 65,536 UTF-16 characters after allocating a whole line. Neither sender enforces a corresponding output byte cap. Oversize/malformed native commands use empty correlation ids and are skipped by the desktop's matching-session reader |
| An unrecoverable failure terminates the engine | A playback/pair worker can emit an error and return while the native JSONL command loop remains alive. Desktop disposal closes stdin, waits two seconds, then attempts to kill the process tree. EOF cancels/joins owned work; this is not a guarantee every cleanup finishes within two seconds |
| Dropped or hung resolves leave `_pending` set | A lost/hung callback can leave the name pending. A normal failed callback removes its token, as does a rejected immediate submission. Source cannot establish that a dropped network response makes the Windows API lose its callback |
| Engine supports one speaker or one existing stereo pair | Desktop/catalog require a complete stereo row. Native `start`/`probe` validate one or two distinct IPv4 peers and cannot establish that those peers are an existing pair. The separately parsed `pair` command lacks the same IPv4/nonzero-port validation |
| Losing either member aborts the process | A detected fatal member fault ends both peers' playback worker. Silence/dropout alone need not cause a fatal fault; local successful UDP sends do not prove receipt. Discovery loss is a separate desktop stop policy |
| PTP UDP 319/320 bind before TCP | True for baseline PTP-selected playback, which the desktop sends. Persistent pairing and NTP-selected native playback do not bind those ports. The NTP responder belongs to the playback attempt, not every possible command/process |
| Previous mute bit is captured at stream start and restored immediately on stop/failure/dispose | Capture happens when `MuteAsync` runs after the engine's streaming event. Restore preserves the exact stored bit but follows cancellation/connection cleanup and runs asynchronously on the MTA worker. COM errors can defer restore; abrupt app death loses the in-memory record |
| Feedback every two seconds measures delay from mutex contention | Next feedback is scheduled two seconds after a completed pass, and request timing begins after acquiring the mutex. Lock contention can delay a request without contributing to its recorded 4/12-second soft/hard interval |
| Equalizer command acknowledgement confirms application | It confirms preparation/command handling for a matching stored start entry. Mailbox sequence acceptance, later audio-thread pickup, and completion of the 20 ms crossfade are separate and unacknowledged |

These corrections describe the existing code. They do not implement a repair or endorse a proposed issue solution.

## Non-negotiable invariants: implementation check

| Invariant | Evidence and limit |
| --- | --- |
| No blocking RTSP, DNS, or capture work on the UI thread | [`SessionController.cs:190–209`](../../desktop/AirFlash.Core/SessionController.cs#L190-L209) launches pairing/playback with `Task.Run`; [`ResolveAsync:455–459`](../../desktop/AirFlash.Core/SessionController.cs#L455-L459) resolves hosts in that worker. Native RTSP/capture are in another process. [`AudioService.cs:16–46`](../../desktop/AirFlash.App/Services/AudioService.cs#L16-L46) owns endpoint calls on an MTA thread. WPF handlers legitimately `await` asynchronous controller/audio operations, yielding the dispatcher; the invariant should mean no synchronous blocking I/O, not no `await` syntax. A long discovery callback can still hold its own lock; this is not a proof of a latency budget |
| Selected down adapter never becomes all interfaces | [`DiscoveryInterface.ResolveIndex`](../../desktop/AirFlash.Core/Receiver.cs#L110-L123) distinguishes empty selection from invalid/down selection. [`WindowsDiscovery.Restart`](../../desktop/AirFlash.App/Services/WindowsDiscovery.cs#L47-L79) pauses instead of submitting interface zero for the latter. [DiscoveryTests](../../desktop/AirFlash.Tests/DiscoveryTests.cs) covers selection logic; actual DNS-SD calls were not exercised here |
| Empty discovery cannot remove/abort manual receivers | [`AppViewModel.cs:188–227`](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L188-L227) guards both missing-row cleanup and active-session stop with non-manual checks. [`UiRegression.cs`](../../desktop/AirFlash.App/Verification/UiRegression.cs) includes missing-adapter/manual preservation using fakes. The stop decision uses a snapshot captured before an awaited identity save; independent pairing can change the session during that await. Thus the guard is verified for the captured manual row, but source inspection does not prove atomic immunity across every concurrent transition; see [After discovery](after-discovery.md) |
| Pair-verify and transient setup are mutually exclusive per playback connection | [`session.rs:232–252`](../../native/airflash-engine/src/session.rs#L232-L252) branches once on credential load. Failures after a positive existence check, including invalid credentials/verification, do not fall back to transient. Missing/empty `/info` id or `load` returning no credential selects transient. [`credentials.rs:107–109`](../../native/airflash-engine/src/credentials.rs#L107-L109) uses `Path::exists`, which also returns false on metadata/permission errors, so an inaccessible existence probe can be treated as absence; this API behavior is documented by [Rust](https://doc.rust-lang.org/std/path/struct.Path.html#method.exists), not reproduced here |
| Credentials remain DPAPI-encrypted and accessory-id keyed | [`credentials.rs:18–120`](../../native/airflash-engine/src/credentials.rs#L18-L120) encrypts with current-user DPAPI and hashes normalized `/info` `deviceID`. The saved HAP accessory id is checked during pair-verify, but source does not establish equality between that id and the `/info` filename key. If `APPDATA` is absent, the directory helper falls back to a relative path |
| Restore writes the exact captured mute bit | [`AudioService.cs:63–100`](../../desktop/AirFlash.App/Services/AudioService.cs#L63-L100) stores/restores a boolean, not unconditional unmute. The dictionary is memory-only and restoration is best effort; normal cleanup and process-crash recovery have different guarantees |
| Probes stay within gain 0.1 and five seconds | [`ProbeOptions::validate`](../../native/airflash-engine/src/session.rs#L89-L127), native probe `gain_limit` and `set_gain` in [`main.rs`](../../native/airflash-engine/src/main.rs), and [`native_probe.py:22–23,86–93`](../../scripts/native_probe.py#L86-L93) enforce the limits. The native WAV guard accepts absolute signed-16 amplitude at most 1639 (about 0.05 full scale), and finite probes disable EQ. `start` is intentionally unbounded playback with gain up to 1. No receiver probe or hardware soak was run during this audit |
| Never add real addresses, MACs, or credentials to docs | Changes are limited to analysis Markdown and use source identifiers or symbolic paths. No runtime credential/config files were opened or added. Synthetic values already used in source/tests are not household evidence; addresses/keys from local logs are not reproduced |

## Validation actually performed

| Check | Result |
| --- | --- |
| Runtime/test tree diff between `41190e0` and `c077a05` | Empty for `desktop/`, `native/`, `scripts/`, and `tests/` |
| `uv run --locked pytest -q -ra` | **49 passed in 11.32 seconds; no skips**. Covers probe safety/analysis and release-version reservation harness. The release tests use a fake `git` command and did not push anything |
| `dotnet build desktop/AirFlash.sln --nologo` | **Could not build:** the available .NET host reports no installed SDK. `dotnet --list-sdks` returns no SDKs |
| .NET tests and WPF verification harness | **Not executed:** missing SDK/build. Relevant test sources were read; mock session/catalog coverage is not a real DNS-SD, audio-endpoint, or HomePod test |
| Rust build and tests | **Not executed:** `cargo`/`rustc` are unavailable on PATH and the checked default user toolchain location has no `cargo.exe`. Existing unit and simulated-wire test sources were inspected |
| Final Markdown links, cited line bounds, UTF-8, whitespace and changed-path scope | **Passed:** 539 local link targets, 368 line anchors, 446 displayed line ranges, and 11 pinned development source targets resolve within their audited trees. JSON command example parses and has ten EQ gains. Code fences/link delimiters, UTF-8, documentation-only IPv4 examples, and `git diff --check` pass. All sixteen originals are modified, with this report the only new file; every changed path is under `docs/analysis/` |
| Independent cross-page review | Catalog/audio auditors reviewed the lifecycle pages and this report/index/source check. Source-backed contradictions in ordering, signatures, API limits, and numeric bounds were corrected before the final checks |

No SDK/toolchain was installed, no receiver connection was opened, no mute/volume was changed, and no real credential store was read. The audit changes documentation only. Software behavior, tests, `docs/issues/`, remotes, and remote branches were not modified.

## Remaining evidence limits

The source does not establish why a particular HomePod disconnected, a Wi-Fi scan duration, an OS-specific playout tolerance, actual routing, acoustic latency, PTP clock lock, or safe buffer margins. Those require identified runtime/hardware measurements. There is no new such measurement in this audit, and the engine itself labels qualification partial and `production_ready: false` in its hello response.

Native Windows DNS-SD callback/cancellation races, lost-callback cleanup, abrupt process death during mute, actual current-user DPAPI behavior, real WASAPI device removal, and physical stereo continuity were not exercised. The documented invariants are therefore a combination of implemented source paths, existing test intent, and explicitly stated runtime limitations. A source audit must not silently turn them into hardware guarantees.

Changes to runtime source require refreshing the corresponding dedicated page and this audit baseline. The claims rejected in [Source check](source-check.md) must remain rejected unless new source or reproducible, appropriately scoped evidence establishes them.
