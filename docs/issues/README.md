# AirFlash implementation backlog

## Continuation on 2026-10-11

The [continuation acceptance](CONTINUATION-ACCEPTANCE.md) supersedes the earlier pending-reboot and unpushed-branch statements below. The manual reboot is verified; current B/D follow-ups are reviewed, pushed to the fork and integrated locally, with 437 Core tests and six crossed regional/catalog WPF runs passing. E is actively in progress; F/G/H remain unimplemented. Upstream publication gates remain unchanged. Earlier source audits and receipts retain their dated boundaries.

## Fork synchronization — 2026-10-11

At the user's instruction, all 18 retained local branches were pushed to `ThatMarko/AirFlash` and every remote head was verified against its local commit. This includes C/D and all preserved backup/validation branches. Fresh upstream stable `d4aa068` was already contained in main, so no new upstream merge was needed. Earlier statements that C/D feature branches were unpushed describe the dated 2026-10-10 audit and are superseded by this synchronization. C/D still have no upstream issue/PR submission. Runtime source is unchanged; local application/build/testing remains deferred until after reboot.

The **17 retained reports** are numbered continuously from **ISSUE-01 through ISSUE-17**. Their original `41190e0` classifications were four HIGH, nine MEDIUM and four LOW: fourteen source-supported defects, one recovery gap and two enhancements. Nine reports are now implemented in local `main`; eight remain proposals. File order preserves the original severity grouping; implementation follows the dependency plan rather than numeric order.

[Implementation plan](IMPLEMENTATION_PLAN.md) defines a small set of coherent PR groups, the work for every issue, dependencies, parallel work, regression fixtures, and completion gates. **The next new report is ISSUE-18.**

The original runtime evidence remains pinned to `41190e0`, unchanged by documentation baseline `c077a05`. Read [docs/analysis](../analysis/README.md) before investigating an area. Source-supported means the control flow or missing mechanism is visible in that source; it does not attribute a real receiver incident. Subsequent tested feature branches are recorded separately from that baseline. [Group B acceptance](GROUP-B-ACCEPTANCE.md) and [combined fork validation](FORK-INTEGRATION-ACCEPTANCE.md) retain the earlier standalone, fork-main and exact downloaded build evidence. The 2026-10-08 B follow-up `11c6046` was tested with A/scrolling/installer fixes on temporary combined source `bfed430`, outside fork main. Unimplemented groups remain proposals.

Current upstream status refreshed on **2026-10-10**: [A PR #11](https://github.com/Ding-Kyoma/AirFlash/pull/11) and [scrolling PR #13](https://github.com/Ding-Kyoma/AirFlash/pull/13) are merged; stable main is `d4aa0686fc411de6655aac313243f0f6bf71e439`. [B PR #15](https://github.com/Ding-Kyoma/AirFlash/pull/15) stays closed at frozen `11c6046`. Pushed B `0a2c1ac` passed four 300-check UI runs/253 Core per configuration; its current A/B-only hosted build and six exact downloaded 501/493-check runs passed. No reopen was performed; earlier gates/results stay historical.

Source links in baseline report sections resolve to the full audited upstream commit. Implementation sections link their feature source and record later checks separately, so current fork code does not silently replace the evidence for an original report.

[Group C acceptance](GROUP-C-ACCEPTANCE.md) records the configuration-preservation/validation package for reports 06 and 07. Additional testing fixed oversized future-schema handling in head `c3a1d5e`: 261 Core tests in each build configuration and 208 WPF checks per language passed. It is now merged into local fork main, without feature push or issue/PR publication; upstream does not contain C. Original audit and reproduction-plan text remains historical.

[Group D acceptance](GROUP-D-ACCEPTANCE.md) records the DNS operation lifecycle package for report 04. Additional testing corrected the browse terminal cancellation status in head `48e7ab62`: 232 Core tests in each build configuration, 149 WPF checks per language and a bounded DNS-only Windows probe passed. It is now merged into local fork main and remains unpushed/unpublished. Lifetime decisions, the initial incorrect status value, its regression and the limited live observation are explicit in that record.

[A/B/C/D acceptance](ABCD-INTEGRATION-ACCEPTANCE.md) records final private `6fe467b`: 422 Core per configuration, zero-warning/error builds, six exact 579/571-check portable runs and MSI payload/refresh checks with catalog restoration. Public A/B-only `d70f30d` passed hosted 86 Python/47 native/255 Core plus installer checks and six exact downloaded 501/493-check runs; its source excludes C/D. Distinct receipts bind each source/binary, including different native hashes. Historical pre-assertion counts remain historical. C/D are unchanged, unpublished and await Task 3.

The Settings scrollbar issues have an [acceptance record](SETTINGS-SCROLL-ACCEPTANCE.md). Initial A+B, scrolling and the separate [installer cache fix](INSTALLER-REFRESH-ACCEPTANCE.md) were integrated into fork main after the user's authorization. Their recorded hosted build/installer checks, exact downloaded mock UI checks and native wheel/thumb observations remain bound to the historical sources listed there. Earlier artifacts do not validate later feature heads. Scrolling is now merged upstream; the installer cache fix remains fork-only and does not activate another backlog group.

On 2026-10-07, the user authorized Task 3 publication in this order: scrolling [issue #12](https://github.com/Ding-Kyoma/AirFlash/issues/12) / [PR #13](https://github.com/Ding-Kyoma/AirFlash/pull/13), then Group B [issue #14](https://github.com/Ding-Kyoma/AirFlash/issues/14) / [PR #15](https://github.com/Ding-Kyoma/AirFlash/pull/15). Both originally targeted the verified `41190e0` stable base and excluded fork-only documents. Their original upstream runs awaited maintainer approval with no jobs started. Current status is stated above; acceptance records retain the exact historical heads/run links. These GitHub numbers are separate from the local report identifiers below.

## Current local integration

On 2026-10-10, all 17 pre-existing local branch tips were merged into fork `main` at **`9e894ab133f8cbdb3b5f5ac1f411023b182e65d7`**, with every branch preserved and a separate pre-merge preservation branch. This includes current B/C/D plus upstream Traditional Chinese support. C/D are now locally integrated but remain unpushed/unpublished. Remote main and upstream PRs were not changed. The source audit covers every report and analysis page; see [audit coverage](../analysis/audit.md#merged-main-documentation-audit--2026-10-10).

Application/build/test execution on this merged main is **deferred until after the user's reboot**. Earlier branch/artifact results are preserved as prior evidence. E/F/G/H are unimplemented; their proposed fixes and fixtures have not been executed by this merge/audit.

## Retained reports and current local status

| Report | Original severity | Kind | Baseline scope (`41190e0`) | Local main status |
| --- | --- | --- | --- | --- |
| [ISSUE-01](ISSUE-01-HIGH-stale-discovery-snapshot-controls-new-session.md) | HIGH | Defect | Stale discovery reconciliation can stop or retarget a newer session | Implemented; new-main execution deferred |
| [ISSUE-02](ISSUE-02-HIGH-delayed-autoconnect-replaces-new-session.md) | HIGH | Defect | A delayed automatic start can replace Pair or undo a newer Stop | Implemented; new-main execution deferred |
| [ISSUE-03](ISSUE-03-HIGH-discovery-disconnects-active-playback.md) | HIGH | Defect | Passive discovery absence/incompleteness stops established playback | Implemented; new-main execution deferred |
| [ISSUE-04](ISSUE-04-HIGH-dns-resolve-without-deadline-stalls-discovery.md) | HIGH | Defect | A resolve without completion has no deadline and suppresses future queries | Implemented; new-main execution deferred |
| [ISSUE-05](ISSUE-05-MEDIUM-build-pipeline-missing-dotnet-sdk-check.md) | MEDIUM | Defect — tooling | Selected SDK compatibility is checked after avoidable build/release side effects | Implemented; new-main execution deferred |
| [ISSUE-06](ISSUE-06-MEDIUM-settings-recovery-reread-escapes.md) | MEDIUM | Defect | Settings recovery rereads a failing file and can throw from its catch | Implemented; new-main execution deferred |
| [ISSUE-07](ISSUE-07-MEDIUM-invalid-loaded-settings-reach-runtime.md) | MEDIUM | Defect — persisted configuration validation | Semantically invalid loaded JSON reaches unchecked runtime assumptions | Implemented; new-main execution deferred |
| [ISSUE-08](ISSUE-08-MEDIUM-pairing-pin-format-mismatch.md) | MEDIUM | Defect | The PIN dialog accepts formatted values rejected by native validation | Unimplemented proposal |
| [ISSUE-09](ISSUE-09-MEDIUM-credential-probe-errors-look-like-absence.md) | MEDIUM | Defect — credential error classification | Credential existence errors can select transient authentication | Unimplemented proposal |
| [ISSUE-10](ISSUE-10-MEDIUM-attempted-preferred-receiver-blocks-autoconnect.md) | MEDIUM | Defect | An attempted preferred receiver blocks an unattempted candidate | Implemented; new-main execution deferred |
| [ISSUE-11](ISSUE-11-MEDIUM-manual-play-redirected-to-stereo-by-address.md) | MEDIUM | Defect | Manual Play can redirect to a stereo group by address alone | Implemented; new-main execution deferred |
| [ISSUE-12](ISSUE-12-MEDIUM-uncorrelated-ipc-errors-become-timeouts.md) | MEDIUM | Defect — IPC contract | Uncorrelated terminal IPC rejections become unrelated host timeouts | Unimplemented proposal |
| [ISSUE-13](ISSUE-13-MEDIUM-endpoint-mute-crash-recovery-gap.md) | MEDIUM | Recovery gap | Prior endpoint mute bits have no durable crash-recovery record | Unimplemented proposal |
| [ISSUE-14](ISSUE-14-LOW-equalizer-ignored-update-success-metadata.md) | LOW | Defect — IPC result contract | Ignored EQ updates return success metadata for the submitted curve | Unimplemented proposal |
| [ISSUE-15](ISSUE-15-LOW-unbound-copy-diagnostics-command.md) | LOW | Defect — UI binding | Settings has no action bound to its diagnostics-copy command | Unimplemented proposal |
| [ISSUE-16](ISSUE-16-LOW-capture-padding-metrics-lack-frame-accounting.md) | LOW | Enhancement | Capture padding lacks missing-frame/whole-packet accounting | Unimplemented proposal |
| [ISSUE-17](ISSUE-17-LOW-orphaned-translation-keys-cleanup.md) | LOW | Enhancement | Review ten candidate keys; current scope includes both Chinese catalogs | Unimplemented proposal |

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
