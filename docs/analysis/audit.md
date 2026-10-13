# Analysis source audits

## Later preparation supplement — 2026-10-13

The pinned 2026-10-12 source map below remains an audit of `0623787`. Current code Main `892a458078930b7b25d973d773e0a93e356ef36b` adds E timeout/Stop regression signals, G Pair/save regression propagation, strictly lower installer fixture selection before side effects, and fork-only exact-source/optional-UI package validation. Ordinary application behavior described by the map is unchanged; test/tool/workflow tree equality with `4ae1db4` no longer applies. [Current acceptance](../issues/CONTINUATION-ACCEPTANCE.md) and [standalone drafts/branches](../issues/upstream-drafts/README.md) bind source/results. Main CI/package now pass 669 Core, 138 Python and 69 native tests (two ignored), six portable UI runs × 1,064, three observed 125% DPI runs × 40 and disposable MSI lifecycle. Each standalone group has separate results. No source review is relabeled runtime execution and no hardware/release claim is added.

## Current implementation map — 2026-10-12

This audit reviews **your fork's `main` at `06237873dcb6394eb35e97a017c536843bd7743d`**. It includes freshly verified original stable `cf420d97700463c56adc31408510e8336e99e51c` and all ten implementation heads: Groups A–H, scrolling and installer refresh. The reported fixes are integrated here; public integration, additional source limits and hardware qualification are separate.

The desktop/native/scripts/installer/tests/workflow trees match runtime `8989b3609dd1294f4d1d6eb1502f345cc22bc382` and tested code `4ae1db4f01dcf7857482dd136ea8d9dffa3aa8a1`; subsequent commits update documentation only. This pass rechecks every analysis page, follows callers and consumers, corrects overbroad claims and adds missing shell/build/verification maps. [Current acceptance](../issues/CONTINUATION-ACCEPTANCE.md) binds executed evidence to actual sources/binaries. It is not reclassified as a new runtime test by a source review.

## Pinned evidence

| Identity | Purpose |
| --- | --- |
| `06237873dcb6394eb35e97a017c536843bd7743d` | Exact fork-main checkout reviewed by this all-analysis pass; normal push CI succeeded |
| `8989b3609dd1294f4d1d6eb1502f345cc22bc382` | Previous current-runtime/documentation source, with fresh local managed/Python verification |
| `4ae1db4f01dcf7857482dd136ea8d9dffa3aa8a1` | Code-identical complete packaged integration qualification |
| `cf420d97700463c56adc31408510e8336e99e51c` | Original stable main already contained locally |
| `41190e0d13a63a714c08dffe73ababca1804875c` | Historical original runtime audit and original defect traces |
| `c077a0577de26ad7e44b8fd50f62456e12965fb0` | Historical documentation checkout, runtime unchanged from 41190e0 |
| `67435a4e0df6a40e5e709e2b75dc9b24a457dda6` | Historical excluded upstream/dev comparison, never a runtime source base |

Current source links pin **ThatMarko/06237873**. Stable comparisons pin **Ding-Kyoma/cf420d9**. Original defects retain **Ding-Kyoma/41190e0**. Archival documentation links retain their actual older commit. A current Git checkout, an opened PR, a proposal and a real-device measurement are not interchangeable evidence.

## Coverage of every original page

All **seventeen pre-existing analysis pages** receive a fresh independent source review. Three justified additions bring the map to **twenty pages**: desktop shell, build/release and verification. Authors own disjoint files; final review rotates to a different source specialist and includes the final regression descriptions. The per-file receipt identifies author/reviewer, final content hash, source boundary and resolved findings. Separate pages describe distinct ownership boundaries.

The preceding `06237873` documentation commit reviewed seventeen analysis and thirty-two issue/plan/acceptance/template files together. This deeper follow-up focuses on `docs/analysis`; `docs/issues` is unchanged. It qualifies remaining source limits instead of claiming that completing the earlier report acceptance eliminates every issue in the program. Directory/component coverage is indexed in [README](README.md#source-directories-and-fork-composition).

| Page | Current source and important boundary |
| --- | --- |
| [README](README.md) | Startup, architecture, source map and current status |
| [Discovery](discovery.md) | Visibility-scoped demand, PTR-TTL cache, deadlines/token/epoch/backoff, exact-child exit and native browse settlement |
| [After discovery](after-discovery.md) | Atomic authority through save, truthful catalog, active controls, automatic admission and manual routing |
| [Identity](identity.md) | Canonical/broadcast/endpoint separation, preference migration and explicit unique selection |
| [Settings](settings.md) | Both Load calls, exact-byte preservation, loaded validation, future-schema latch, Apply and shell |
| [Desktop shell](desktop-shell.md) | Startup/readiness overlap, single instance/tray, commands/dialogs, themes/localization/placement, updates/autostart and actual Quit ordering |
| [Session](session.md) | Owner/intent serialization, Pair/retry/process lifetimes, signatures, endpoint recovery, metrics |
| [Engine](engine.md) | Native auth/RTSP/capture/timing/media watches and worker/command-loop distinction |
| [Authentication](auth.md) | ASCII PIN, error-preserving credential read, exclusive auth path and accessory identity |
| [Failures](failures.md) | Process-owned IPC rejection, fault/retry/detail/nonfatal EQ and cleanup |
| [Playback coupling](playback-coupling.md) | Passive retention, truthful availability, Stop accessibility and explicit/fault cleanup |
| [Capture and clocks](capture-clocks.md) | Recording/render capture, resampling, local pre-EQ padding accounting and timing limits |
| [Control and mute](control-and-mute.md) | Durable intent/exclusive ownership, actual device direction, conservative recovery and exact-bit restore |
| [Credentials and IPC](credentials-and-ipc.md) | Current-user DPAPI/accessory key, JSONL limits/correlation and compatibility |
| [Equalizer](equalizer.md) | Retained mailbox outcome, preparation versus DSP adoption, preview ownership and geometry |
| [Build and release](build-and-release.md) | Selected-host resolver, managed/native dependencies, embedding/audits, reservation/failure side effects, MSI and original/fork workflows |
| [Verification](verification.md) | Linked App/Core seams versus WPF, fake/backend/localhost boundaries, negative controls, diagnostic side effects and exact package/CI provenance |
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

## Findings from the complete analysis peer review

| Earlier overbroad claim | Source-grounded correction |
| --- | --- |
| Every worker completion emits `stopped` | Playback, handshake-only, Pair, early cancellation and worker panic have distinct paths; the command slot can outlive the worker. [Engine](engine.md) traces producers and consumers |
| A stored/acknowledged gain update always changes subsequent PCM | Live capture samples atomic gain; finite WAV probes eagerly load initial-gain PCM and do not reread it in the send loop |
| Pair process rejection is always acted on immediately | The sole Pair reader awaits PIN handling; a prompt-open rejection can wait, and cancellation/timeout may win. Existing IPC regression scripts rejection before `pin_required`; [auth](auth.md) and [failures](failures.md) preserve this coverage limit |
| Persistent multi-member Pair is transactional | Credentials save after each successful member; later failure/cancellation does not roll back earlier saved credentials |
| Shutdown invalidates intent before any wait | View-model/controller disposal does; shell Quit first awaits Settings Apply and engine initialization disposal. [Shell](desktop-shell.md) records the source-derived admission window |
| Browse settlement proves all native callback frames returned | The owner tracks Start/Cancel calls and managed callback sections; release can occur inside the reverse P/Invoke return frame. [Discovery](discovery.md) separates tracked safety conditions from undocumented native-stack quiescence |
| The forty-second read deadline excludes initial-send elapsed time | It starts before the EQ writer lock and Start send; those waits reduce later read budget but are not themselves canceled by this deadline |
| Only catalog mutations notify catalog consumers | Reconciliation uses a catalog-change flag, but readiness/recovery notifications also raise `CatalogChanged` |
| Every changed TXT field immediately publishes discovery | Resolve compares a description omitting `gpn`, `igl`, `pk` and `cn`; those changes enter the cache but alone do not trigger immediate Changed/log publication |
| Diagnostic data isolation covers every local write | Embedded engine extraction uses the normal SHA-keyed local cache independently of isolated logs/settings; modes have different real-service boundaries |
| All prerequisite failures precede version allocation | Selected-SDK preflight does; later native/Python/installer failures can leave a persisted reservation. Equal-version packaging rebuild also does not guarantee same-version MSI upgrade acceptance |
| Core tests contain no desktop implementation | Selected App sources are linked into the Core test assembly; actual WPF/XAML/dialog testing remains separate |

These are documentation corrections or source-derived limitations, not newly executed device reproductions or runtime repairs. The verification page marks which assertions cover each boundary and which paths remain unqualified.

## Non-negotiable invariants: implementation check

WPF and Rust share no memory/C-ABI; JSONL v1 and config schema 2 remain compatible. SDK preflight respects actual desktop/global.json selection. Blocking DNS/RTSP/capture work stays off the UI dispatcher. Selected unavailable discovery adapter never falls back to all interfaces. Manual endpoints/options persist independently; new stereo starts require complete groups.

Native retries create new processes, session UUIDs, clocks and SSRCs, while desktop owner survives. Process command-loop and worker lifetimes differ. Credentials remain current-user DPAPI and accessory `/info deviceID` keyed; storage/verify errors do not silently choose transient setup.

Local playback mute records the exact original bit before mutation and restores best effort after cleanup. Actual opened recording devices are excluded from all mute ownership/access. Recovery interaction occurs outside settings/audio/lifecycle locks. Capture metrics are local accounting, not acoustic/network estimates. No real household data is added to code or documentation.

## Validation actually performed

| Source/evidence | Actual result |
| --- | --- |
| Current `8989b36` local sync/readiness | SDK 10.0.401 resolution; locked Release build with zero warnings/errors; 667 Core, 124 Python, lint passed |
| Code `4ae1db4` local validation | 667 Core in Debug and Release; warning-free Release build |
| [Reviewed-main CI 38329156167](https://github.com/ThatMarko/AirFlash/actions/runs/38329156167) | `06237873` succeeded: 124 Python/lint, 667 Core, 69 native with two ignored, native checks/build, discovery stability, bundle audit and packaged self/compat startup |
| [Exact-main CI 38296750592](https://github.com/ThatMarko/AirFlash/actions/runs/38296750592) | Green: managed/Python/native checks, stability, bundle and packaged startup |
| [Exact-code integration 38294493405](https://github.com/ThatMarko/AirFlash/actions/runs/38294493405) | Green: 667 Core, 124 Python/lint, 69 native; six complete portable regional/catalog runs, 1,059 assertions each; three observed 125% DPI runs, 40 each; disposable-runner MSI lifecycle |
| Native exclusions | Recording-device qualification and thirty-minute localhost soak remain two ignored tests; not both hardware tests |
| Earlier `dc521ae` actual-child/PIN evidence | Six synthetic real DNS helpers and 225 modal PIN checks, still bound to that earlier source; not relabeled as newly run on 8989 |
| This documentation task | Source/caller/test/evidence audit, all-file Markdown/UTF-8/fence/JSON/relative heading/tracked-file/pinned-source checks, changed-path/whitespace review and Mermaid syntax parsing |
| Independent documentation review | Rotated source/provenance/cross-page review; final receipt records review completion and resolved findings |

No local runtime suite is rerun solely for Markdown changes. Structural counts and per-file hashes belong to the new all-analysis receipt, preserving the preceding all-49-document receipts. Normal CI's already completed `06237873` run is independently inspected; a later documentation push must retain its own actual CI state. Native execution is hosted because local Rust/MSVC prerequisites remain absent. No real config/credential/receiver/endpoint operation, local MSI install, release workflow or branch deletion is performed by this documentation update.

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
