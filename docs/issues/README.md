# AirFlash implementation backlog

The **17 active reports** are now numbered continuously from **ISSUE-01 through ISSUE-17**: four HIGH, nine MEDIUM, and four LOW. They describe fourteen source-supported defects, one recovery gap, and two enhancements. File order follows severity, then related subsystem/priority; implementation follows the dependency plan rather than numeric order.

[Implementation plan](IMPLEMENTATION_PLAN.md) defines a small set of coherent PR groups, the work for every issue, dependencies, parallel work, regression fixtures, and completion gates. **The next new report is ISSUE-18.**

The original runtime evidence remains pinned to `41190e0`, unchanged by documentation baseline `c077a05`. Read [docs/analysis](../analysis/README.md) before investigating an area. Source-supported means the control flow or missing mechanism is visible in that source; it does not attribute a real receiver incident. Subsequent tested feature branches are recorded separately from original stable behavior: [Group B feature acceptance](GROUP-B-ACCEPTANCE.md) covers the session/receiver package. Group A and then Group B are integrated into local fork `main` at `72dc7c7`; [combined fork validation](FORK-INTEGRATION-ACCEPTANCE.md) records the subsequent build/download checks. Original upstream `main` has not merged these fixes. Unimplemented groups remain proposals.

Source links in baseline report sections resolve to the full audited upstream commit. Implementation sections link their feature source and record later checks separately, so current fork code does not silently replace the evidence for an original report.

The separately reported Settings scrollbar issues have an [acceptance record](SETTINGS-SCROLL-ACCEPTANCE.md). A+B, scrolling and the separate [installer cache fix](INSTALLER-REFRESH-ACCEPTANCE.md) are integrated into fork main after the user's authorization. Final GitHub build/installer lifecycle checks, exact downloaded mock UI checks, local installer payload checks, and the subsequent current-monitor native wheel/thumb/visual checks passed. These changes remain outside upstream main and do not activate another backlog group.

## Active reports

| Current ID | Severity | Kind | Concrete scope |
| --- | --- | --- | --- |
| [ISSUE-01](ISSUE-01-HIGH-stale-discovery-snapshot-controls-new-session.md) | HIGH | Defect | Stale discovery reconciliation can stop or retarget a newer session |
| [ISSUE-02](ISSUE-02-HIGH-delayed-autoconnect-replaces-new-session.md) | HIGH | Defect | A delayed automatic start can replace Pair or undo a newer Stop |
| [ISSUE-03](ISSUE-03-HIGH-discovery-disconnects-active-playback.md) | HIGH | Defect | Passive discovery absence/incompleteness stops established playback |
| [ISSUE-04](ISSUE-04-HIGH-dns-resolve-without-deadline-stalls-discovery.md) | HIGH | Defect | A resolve without completion has no deadline and suppresses future queries |
| [ISSUE-05](ISSUE-05-MEDIUM-build-pipeline-missing-dotnet-sdk-check.md) | MEDIUM | Defect — tooling | Selected SDK compatibility is checked after avoidable build/release side effects |
| [ISSUE-06](ISSUE-06-MEDIUM-settings-recovery-reread-escapes.md) | MEDIUM | Defect | Settings recovery rereads a failing file and can throw from its catch |
| [ISSUE-07](ISSUE-07-MEDIUM-invalid-loaded-settings-reach-runtime.md) | MEDIUM | Defect — persisted configuration validation | Semantically invalid loaded JSON reaches unchecked runtime assumptions |
| [ISSUE-08](ISSUE-08-MEDIUM-pairing-pin-format-mismatch.md) | MEDIUM | Defect | The PIN dialog accepts formatted values rejected by native validation |
| [ISSUE-09](ISSUE-09-MEDIUM-credential-probe-errors-look-like-absence.md) | MEDIUM | Defect — credential error classification | Credential existence errors can select transient authentication |
| [ISSUE-10](ISSUE-10-MEDIUM-attempted-preferred-receiver-blocks-autoconnect.md) | MEDIUM | Defect | An attempted preferred receiver blocks an unattempted candidate |
| [ISSUE-11](ISSUE-11-MEDIUM-manual-play-redirected-to-stereo-by-address.md) | MEDIUM | Defect | Manual Play can redirect to a stereo group by address alone |
| [ISSUE-12](ISSUE-12-MEDIUM-uncorrelated-ipc-errors-become-timeouts.md) | MEDIUM | Defect — IPC contract | Uncorrelated terminal IPC rejections become unrelated host timeouts |
| [ISSUE-13](ISSUE-13-MEDIUM-endpoint-mute-crash-recovery-gap.md) | MEDIUM | Recovery gap | Prior endpoint mute bits have no durable crash-recovery record |
| [ISSUE-14](ISSUE-14-LOW-equalizer-ignored-update-success-metadata.md) | LOW | Defect — IPC result contract | Ignored EQ updates return success metadata for the submitted curve |
| [ISSUE-15](ISSUE-15-LOW-unbound-copy-diagnostics-command.md) | LOW | Defect — UI binding | Settings has no action bound to its diagnostics-copy command |
| [ISSUE-16](ISSUE-16-LOW-capture-padding-metrics-lack-frame-accounting.md) | LOW | Enhancement | Capture padding lacks missing-frame/whole-packet accounting |
| [ISSUE-17](ISSUE-17-LOW-orphaned-translation-keys-cleanup.md) | LOW | Enhancement | Review ten statically unused localization entries before removal |

## Execution order

1. Establish the selected toolchains and isolated test fixtures; address the SDK preflight package before using affected release/build entry points.
2. Group shared session ownership, automatic admission and receiver-selection fixes, then decouple healthy active playback from passive discovery absence. Resolve DNS operation deadlines independently.
3. Group settings read recovery with post-load validation; prepare PIN formatting and credential-error classification as a separate authentication package.
4. Repair IPC error routing and equalizer result semantics with explicit protocol compatibility checks.
5. Complete the mute-recovery design and interruption tests before enabling a journal; finish diagnostics/telemetry and reviewed translation cleanup.

The [plan](IMPLEMENTATION_PLAN.md) distinguishes hard dependencies from convenient sequencing and identifies work that can run in parallel. Numbering is a stable identifier within this new series, not a promise that the lowest number must be implemented first.

## Severity, identity, and reporting

| Severity | Evidence/impact required |
| --- | --- |
| CRITICAL | Demonstrated severe data loss, security impact, or similarly broad unrecoverable failure; no active report establishes this |
| HIGH | Substantial playback/session/discovery disruption with a concrete trigger |
| MEDIUM | Scoped correctness, validation, recovery, or tooling failure |
| LOW | Limited contract/UI impact, observability improvement, or maintenance cleanup |

Defects report incorrect behavior; recovery gaps qualify missing recovery and unmeasured OS outcomes; enhancements do not claim proven playback failure. Preserve these distinctions when implementing or closing a report.

Use [ISSUE_TEMPLATE.md](ISSUE_TEMPLATE.md). These are local Markdown report ids, not GitHub issue numbers. Future implementation branches start from the original repository's stable main, and their PR diffs exclude this fork's `docs/analysis/` and `docs/issues/`. Push implementation branches to `origin`, never directly to `upstream`; see the plan's branch workflow.

Each implementation must supply a focused diff, its reproduced regression and relevant checks, and an updated analysis page describing the resulting behavior. Existing test sources and proposed fixtures are not passing-test evidence. The preceding [analysis audit](../analysis/audit.md) ran 49 Python tests with no skips; .NET/Rust builds and WPF verification were unavailable then. This reorganization changes documents only and does not establish new runtime test results.

Preserve schema 2, JSONL v1, no blocking DNS/RTSP/capture work on the dispatcher, selected-adapter no-fallback, persistent manual receivers, one authentication method per connection, accessory-keyed current-user DPAPI, and exact prior mute-bit restoration. Use synthetic temporary fixtures. Do not commit real addresses, MACs, PINs, credentials, or private logs. Any separately authorized real-device finite probe remains gain at most 0.1 and duration at most five seconds; no hardware soak.
