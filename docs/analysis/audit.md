# Analysis source audits

## Current implementation map — 2026-10-12

This audit describes **your fork's `main` at `8989b3609dd1294f4d1d6eb1502f345cc22bc382`**. It includes freshly verified original stable `cf420d97700463c56adc31408510e8336e99e51c` and all ten implementation heads: Groups A–H, scrolling and installer refresh. Every report in `docs/issues` is implemented here; public integration and hardware qualification are separate.

The desktop/native/scripts/installer/tests/workflow trees match tested code `4ae1db4f01dcf7857482dd136ea8d9dffa3aa8a1`; `8989b36` adds private documentation. This update rewrites the subsystem/report narratives around that current production source, rather than requiring an appended correction to reinterpret an obsolete baseline body. [Current acceptance](../issues/CONTINUATION-ACCEPTANCE.md) binds executed evidence to actual sources/binaries.

## Pinned evidence

| Identity | Purpose |
| --- | --- |
| `8989b3609dd1294f4d1d6eb1502f345cc22bc382` | Current fork-main source reviewed here; pushed before this docs-only update |
| `4ae1db4f01dcf7857482dd136ea8d9dffa3aa8a1` | Code-identical complete packaged integration qualification |
| `cf420d97700463c56adc31408510e8336e99e51c` | Original stable main already contained locally |
| `41190e0d13a63a714c08dffe73ababca1804875c` | Historical original runtime audit and original defect traces |
| `c077a0577de26ad7e44b8fd50f62456e12965fb0` | Historical documentation checkout, runtime unchanged from 41190e0 |
| `67435a4e0df6a40e5e709e2b75dc9b24a457dda6` | Historical excluded upstream/dev comparison, never a runtime source base |

Current source links pin **ThatMarko/8989b36**. Stable comparisons pin **Ding-Kyoma/cf420d9**. Original defects retain **Ding-Kyoma/41190e0**. A current Git checkout, an opened PR, a proposal and a real-device measurement are not interchangeable evidence.

## Coverage of every original page

All seventeen analysis pages and all thirty-two issue/plan/acceptance/template files are reviewed. The per-file source/evidence receipt identifies callers, consumers, tests and material corrections. Separate pages remain because they describe distinct ownership boundaries.

| Page | Current source and important boundary |
| --- | --- |
| [README](README.md) | Startup, architecture, source map and current status |
| [Discovery](discovery.md) | Visibility-scoped demand, PTR-TTL cache, deadlines/token/epoch/backoff, exact-child exit and native browse settlement |
| [After discovery](after-discovery.md) | Atomic authority through save, truthful catalog, active controls, automatic admission and manual routing |
| [Identity](identity.md) | Canonical/broadcast/endpoint separation, preference migration and explicit unique selection |
| [Settings](settings.md) | Both Load calls, exact-byte preservation, loaded validation, future-schema latch, Apply and shell |
| [Session](session.md) | Owner/intent serialization, Pair/retry/process lifetimes, signatures, endpoint recovery, metrics |
| [Engine](engine.md) | Native auth/RTSP/capture/timing/media watches and worker/command-loop distinction |
| [Authentication](auth.md) | ASCII PIN, error-preserving credential read, exclusive auth path and accessory identity |
| [Failures](failures.md) | Process-owned IPC rejection, fault/retry/detail/nonfatal EQ and cleanup |
| [Playback coupling](playback-coupling.md) | Passive retention, truthful availability, Stop accessibility and explicit/fault cleanup |
| [Capture and clocks](capture-clocks.md) | Recording/render capture, resampling, local pre-EQ padding accounting and timing limits |
| [Control and mute](control-and-mute.md) | Durable intent/exclusive ownership, actual device direction, conservative recovery and exact-bit restore |
| [Credentials and IPC](credentials-and-ipc.md) | Current-user DPAPI/accessory key, JSONL limits/correlation and compatibility |
| [Equalizer](equalizer.md) | Retained mailbox outcome, preparation versus DSP adoption, preview ownership and geometry |
| [Source check](source-check.md) | Current supported/rejected claims and incident inference limits |
| [Upstream dev](upstream-dev.md) | Excluded development snapshot only |
| This audit | Recursive coverage, source qualification and historical corrections |

Groups A and scrolling are already upstream. PR #19 supplies bounded passive retention/fresh-snapshot/Pairing protection, not the full B contract. Old B PR #15 remains closed at its older snapshot. C–H and installer improvements are fork-ready implementations; no new upstream publication or release is implied. All ten public feature diffs **and outgoing commits** exclude fork-only planning.

## Corrections to the continuation briefing

| Overbroad or obsolete statement | Current verified formulation |
| --- | --- |
| A discovery loss stops playback | Passive absence/partial data retains all owned active states; guarded complete updates may still restart on signature |
| Receiver id/snapshot reference establishes ownership | Atomic desktop owner plus intent identifies authority; metrics/retries retain owner, same-id replacement does not |
| Current discovery has ninety-second cached-record expiry | Cache follows capped PTR advertisement TTL, with ten seconds on restart/resume |
| Resolve callback/Cancel success proves quiescence | Neither does; production uses bounded children, releasing started owners only after exact exit observation |
| Invalid settings recover without preservation state | Raw bytes/uncaptured obligations persist across both Load calls and later saves |
| Abrupt death always loses previous mute | Separate owned journal preserves durable intent, but ambiguity/user changes/power-loss limits remain |
| EQ success means audio application finished | It truthfully reflects accepted/idempotent retained preparation; DSP adoption/crossfade are unacknowledged |
| Padding metrics describe network loss | They count missing local output-rate frame slots before EQ and whole 352-frame pulls |
| Tests still await manual reboot/E–H unimplemented | Reboot gate is satisfied; all reports are integrated and qualified within stated fake/hosted bounds |

## Non-negotiable invariants: implementation check

WPF and Rust share no memory/C-ABI; JSONL v1 and config schema 2 remain compatible. SDK preflight respects actual desktop/global.json selection. Blocking DNS/RTSP/capture work stays off the UI dispatcher. Selected unavailable discovery adapter never falls back to all interfaces. Manual endpoints/options persist independently; new stereo starts require complete groups.

Native retries create new processes, session UUIDs, clocks and SSRCs, while desktop owner survives. Process command-loop and worker lifetimes differ. Credentials remain current-user DPAPI and accessory `/info deviceID` keyed; storage/verify errors do not silently choose transient setup.

Local playback mute records the exact original bit before mutation and restores best effort after cleanup. Actual opened recording devices are excluded from all mute ownership/access. Recovery interaction occurs outside settings/audio/lifecycle locks. Capture metrics are local accounting, not acoustic/network estimates. No real household data is added to code or documentation.

## Validation actually performed

| Source/evidence | Actual result |
| --- | --- |
| Current `8989b36` local sync/readiness | SDK 10.0.401 resolution; locked Release build with zero warnings/errors; 667 Core, 124 Python, lint passed |
| Code `4ae1db4` local validation | 667 Core in Debug and Release; warning-free Release build |
| [Exact-main CI 38296750592](https://github.com/ThatMarko/AirFlash/actions/runs/38296750592) | Green: managed/Python/native checks, stability, bundle and packaged startup |
| [Exact-code integration 38294493405](https://github.com/ThatMarko/AirFlash/actions/runs/38294493405) | Green: 667 Core, 124 Python/lint, 69 native; six complete portable regional/catalog runs, 1,059 assertions each; three observed 125% DPI runs, 40 each; disposable-runner MSI lifecycle |
| Native exclusions | Recording-device qualification and thirty-minute localhost soak remain two ignored tests; not both hardware tests |
| Earlier `dc521ae` actual-child/PIN evidence | Six synthetic real DNS helpers and 225 modal PIN checks, still bound to that earlier source; not relabeled as newly run on 8989 |
| This documentation task | Source/caller/test/evidence audit, all-file Markdown/UTF-8/fence/JSON/relative heading/tracked-file/pinned-source checks, changed-path/whitespace review and Mermaid syntax parsing |
| Independent documentation review | Rotated source/provenance/cross-page review; final receipt records review completion and resolved findings |

No runtime suite is rerun solely for Markdown changes. Structural counts and per-file hashes belong to the new dated documentation receipt, not the older 8989 audit receipt. Native execution is hosted because local Rust/MSVC prerequisites remain absent. No real config/credential/receiver/endpoint operation, local MSI install, release workflow or branch deletion is performed by this documentation update.

## Remaining evidence limits

Source/fake/hosted tests establish software contracts within their boundaries. They do not establish a household dropout cause, actual routing, acoustic latency, firmware-specific jitter tolerance, PTP lock, general Windows callback delivery, current-user DPAPI on this machine, physical stereo continuity or physical power-loss recovery. Native hello's partial qualification remains separate from tests passing.

Normal mute cleanup is best effort, not immediate. Recovery cannot infer no later user/application mute changes from a stored bit. Settings store locking does not coordinate arbitrary external writers after valid Load. Child process exit supplies a memory lifetime boundary, not a proof DNS always finds every receiver.

## Original audit — 2026-10-06

The original sixteen pages were audited against `41190e0`, while fork documentation `c077a05` left runtime/test trees unchanged. Executed Python verification was **49 passed in 11.32 seconds, no skips**. Managed/WPF execution was unavailable because no compatible SDK existed; cargo/rustc were absent. Source inspection was not counted as test execution. No receiver probe, endpoint mutation or credential access ran in that audit.

The immutable [historical audit text](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/analysis/audit.md#original-audit--2026-10-06) preserves the original full tables, 539-target/368-anchor checks and subsequent dated A/B/C/D integration snapshots. Those source-only/pre-reboot/unimplemented/unpushed labels are historical. Repeated continuation diaries are condensed here; detailed executed failures and receipts remain in group acceptance.

## Subsequent Group A local verification — 2026-10-07

A `de13b6d` passed isolated fake ordering and selected-host checks, was published as issue #10/PR #11 and subsequently merged upstream. [Report 05](../issues/ISSUE-05-MEDIUM-build-pipeline-missing-dotnet-sdk-check.md#local-implementation-verification--2026-10-07) retains original evidence. Earlier approval/quota messages describe their publication-time observation, not current CI or account credit state.

## Merged-main documentation audit — 2026-10-10

The old `9e894ab` all-branch integration was source-only while execution awaited reboot. It contained A–D and scrolling/installer, with E–H still proposals at that time. Its immutable record and [historical combined acceptance](../issues/ABCD-INTEGRATION-ACCEPTANCE.md) remain evidence of that stage; they do not describe current readiness or publication.
