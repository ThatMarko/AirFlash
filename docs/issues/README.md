# AirFlash implementation backlog

## Current fork Main — 2026-10-13

**All 17 reports are implemented, independently reviewed and integrated into fork Main.** Current code baseline `7a4f9f58e9222e12a98afa398c51ebf31b31a4f0` is pushed and locally qualified on SDK 10.0.401/402; its hosted runs are tracked separately in [SDK acceptance](SDK-LOCK-ACCEPTANCE.md). Earlier integrated code `892a458` retains its source-bound full-package qualification. The preparation pass added joined PIN timeout/Stop coverage, propagated the Pair persistence correction to standalone G, strengthened installer fallback-version qualification and enabled exact-source fork package validation. Main already retained synchronous B intent and immediate Pair admission. All runtime packages are integrated; the four updated public PR heads are ancestors of this code baseline. The separate D fixture follow-up is included by cherry-pick. Earlier `8989b36`/`4ae1db4` qualification remains historical; current test/harness/tool/workflow trees have changed. [Prepared drafts and branches](upstream-drafts/README.md) binds eight standalone heads/results separately. Local evidence is under `artifacts/pr-preparation-2026-10-13`. Later documentation-only commits may advance Main without changing this qualified code.

Original stable Main is `cf420d97700463c56adc31408510e8336e99e51c`, already contained in the fork. A and scrolling are upstream. B's [PR #15](https://github.com/Ding-Kyoma/AirFlash/pull/15) is closed without merge at frozen `11c6046`; [issue #14](https://github.com/Ding-Kyoma/AirFlash/issues/14) is open. Merged [PR #19](https://github.com/Ding-Kyoma/AirFlash/pull/19) supplies partial discovery retention/fresh-snapshot safeguards, while B retains atomic ownership, intent, admission and routing fixes. C–H and installer repairs are fork implementations, not upstream integrations.

| Group | Reports | Integrated feature head | Acceptance / upstream state |
| --- | --- | --- | --- |
| A | 05 | `de13b6d` | [Build preflight](ISSUE-05-MEDIUM-build-pipeline-missing-dotnet-sdk-check.md); PR #11 merged |
| B | 01, 02, 03, 10, 11 | `442e8d8` | [Session/receiver package](GROUP-B-ACCEPTANCE.md); PR #15 closed, issue #14 open |
| C | 06, 07 | `107c7df` | [Settings preservation/validation](GROUP-C-ACCEPTANCE.md); [issue #22](https://github.com/Ding-Kyoma/AirFlash/issues/22) and [PR #23](https://github.com/Ding-Kyoma/AirFlash/pull/23) open; not merged |
| D | 04 | `cb9da32` | [Bounded DNS process ownership](GROUP-D-ACCEPTANCE.md); publication pending authorization |
| E | 08, 09 | `234167b` | [PIN and credential lookup](GROUP-E-ACCEPTANCE.md); [issue #24](https://github.com/Ding-Kyoma/AirFlash/issues/24) and [PR #25](https://github.com/Ding-Kyoma/AirFlash/pull/25) open; not merged |
| F | 12, 14 | `28cb83e` | [IPC errors and EQ outcomes](GROUP-F-ACCEPTANCE.md); [issue #26](https://github.com/Ding-Kyoma/AirFlash/issues/26) and [PR #27](https://github.com/Ding-Kyoma/AirFlash/pull/27) open; not merged; SDK 10.0.402 CI is green |
| G | 13 | `fcded6b` | [Owned mute recovery](GROUP-G-ACCEPTANCE.md); publication pending authorization |
| H | 15, 16, 17 | `6855dfe` | [Diagnostics/padding/localization](GROUP-H-ACCEPTANCE.md); publication pending authorization |
| Scrolling | Separate UI follow-up | `54cd18e` | [Scrolling](SETTINGS-SCROLL-ACCEPTANCE.md); PR #13 merged |
| Installer | Separate packaging follow-up | `74cba07` | [Payload refresh and qualification fixtures](INSTALLER-REFRESH-ACCEPTANCE.md); [issue #20](https://github.com/Ding-Kyoma/AirFlash/issues/20) and [PR #21](https://github.com/Ding-Kyoma/AirFlash/pull/21) open; not merged |

## Current acceptance

[SDK correction](SDK-LOCK-ACCEPTANCE.md): actual Main **669 Core** and warning-free Release builds pass on **both 10.0.401 and 10.0.402**; a fresh 10.0.402 full WPF run passes **1,064 assertions**. Main's independent normal CI and full package run on 10.0.402 also pass **138 Python / 669 Core / 69 native**, six portable UI runs with **1,064 each**, three 125% DPI fixtures with **40 each**, and disposable MSI lifecycle. F passes 239 Core on both and 568 WPF assertions on 10.0.402. All four updated upstream PRs have complete green 10.0.402 CI. The separate discovery cancellation-count fixture was reproduced and corrected with controlled scheduling; runtime/native lifetime policy is unchanged.

Earlier integrated `892a458` qualification passed **669 Release Core tests, 138 Python tests, lint and a Release solution build with zero warnings/errors**. [Code-baseline CI 38360425572](https://github.com/ThatMarko/AirFlash/actions/runs/38360425572) and [full package 38360425230](https://github.com/ThatMarko/AirFlash/actions/runs/38360425230) succeeded at `892a458`: 69 native tests (two ignored), six portable UI runs with **1,064 each**, three actual 125% DPI runs with **40 each**, and disposable MSI lifecycle. Eight standalone branches have their own independently inspected results in the draft index. Earlier exact code `4ae1db4` also passed all 667 Core tests in both Debug and Release. [Exact Main CI 38296750592](https://github.com/ThatMarko/AirFlash/actions/runs/38296750592) is successful and includes native checks, receiver-free discovery stability, bundle audit and packaged startup.

[Exact-code unofficial integration 38294493405](https://github.com/ThatMarko/AirFlash/actions/runs/38294493405) passed 69 native tests, 667 Core tests, 124 Python tests/lint, six complete portable regional/catalog runs with **1,059 assertions each**, three actual 125% DPI runs with **40 each**, and disposable-runner MSI install/repair/upgrade/uninstall/data preservation. The two ignored native tests are specifically recording-device qualification and the thirty-minute localhost soak. Native tooling is verified on hosted Windows; local Rust/MSVC/Windows SDK are unavailable. Modal clipboard and MSI installation checks run only on disposable hosted fixtures.

The reboot execution gate is satisfied. This preparation pass ran source-bound standalone and integrated checks; its final documentation update does not relabel prior executions. Earlier results remain bound to their actual source, run, configuration and binary. [Continuation acceptance](CONTINUATION-ACCEPTANCE.md) records final provenance, controlled failures and limitations.

## Retained reports

Filenames preserve original `41190e0` classifications: four HIGH, nine MEDIUM and four LOW; fourteen defects, one recovery gap and two enhancements. They are local ids, not GitHub issue numbers. **The next report is ISSUE-18.** Current bodies describe implemented Main behavior and keep original defective-source evidence separately.

| Report | Group | Implemented scope |
| --- | --- | --- |
| [ISSUE-01](ISSUE-01-HIGH-stale-discovery-snapshot-controls-new-session.md) | B | Guard delayed discovery retargeting with atomically captured lifecycle ownership |
| [ISSUE-02](ISSUE-02-HIGH-delayed-autoconnect-replaces-new-session.md) | B | Preserve explicit intent and revalidate automatic admission after persistence |
| [ISSUE-03](ISSUE-03-HIGH-discovery-disconnects-active-playback.md) | B | Retain owned active attempts and Stop controls through passive discovery loss |
| [ISSUE-04](ISSUE-04-HIGH-dns-resolve-without-deadline-stalls-discovery.md) | D | Bound logical lookups, retries and helper-process ownership |
| [ISSUE-05](ISSUE-05-MEDIUM-build-pipeline-missing-dotnet-sdk-check.md) | A | Validate the actual selected SDK before avoidable build/release side effects |
| [ISSUE-06](ISSUE-06-MEDIUM-settings-recovery-reread-escapes.md) | C | Guard settings reads and preserve bytes across repeated Load/save recovery |
| [ISSUE-07](ISSUE-07-MEDIUM-invalid-loaded-settings-reach-runtime.md) | C | Validate normalized settings before runtime consumption |
| [ISSUE-08](ISSUE-08-MEDIUM-pairing-pin-format-mismatch.md) | E | Share ASCII PIN normalization between enablement and submission |
| [ISSUE-09](ISSUE-09-MEDIUM-credential-probe-errors-look-like-absence.md) | E | Preserve lookup errors; only genuine initial absence selects setup |
| [ISSUE-10](ISSUE-10-MEDIUM-attempted-preferred-receiver-blocks-autoconnect.md) | B | Filter attempted candidates before ranking |
| [ISSUE-11](ISSUE-11-MEDIUM-manual-play-redirected-to-stereo-by-address.md) | B | Preserve explicit manual endpoint, port, identity and preferences |
| [ISSUE-12](ISSUE-12-MEDIUM-uncorrelated-ipc-errors-become-timeouts.md) | F | Route defined process-owned terminal IPC errors before session filtering |
| [ISSUE-13](ISSUE-13-MEDIUM-endpoint-mute-crash-recovery-gap.md) | G | Journal exact owned mute state and conservatively review recovery |
| [ISSUE-14](ISSUE-14-LOW-equalizer-ignored-update-success-metadata.md) | F | Return mailbox outcomes without claiming DSP completion |
| [ISSUE-15](ISSUE-15-LOW-unbound-copy-diagnostics-command.md) | H | Expose the existing diagnostics serializer through Monitor |
| [ISSUE-16](ISSUE-16-LOW-capture-padding-metrics-lack-frame-accounting.md) | H | Count substituted output-rate frames and wholly padded pulls |
| [ISSUE-17](ISSUE-17-LOW-orphaned-translation-keys-cleanup.md) | H | Remove unused translation candidates after dynamic consumer review |

## Publication and release readiness

[Implementation plan](IMPLEMENTATION_PLAN.md) is the completed-contract and publication plan. Maintain one coherent PR per group, normally one or two ready PRs open at a time. Refresh overlapping groups against actually merged upstream Main, remove integrated scope and rerun affected checks. An open PR is not an integrated prerequisite.

Fork Main is the combined build for manual inspection. A release built from its tracked code includes all fixes; existing upstream `v0.4.0` is an older original-repository tag, not this combined fork. [Release notes](../RELEASE-NOTES.md) still describe original 0.4.0 and need refresh before publication. Local production release needs native tooling; production release workflows may reserve a version even with publishing disabled. No release or new upstream publication is authorized by documentation work.

Public implementation branches exclude private analysis/issues/planning in both complete diffs and every outgoing commit. Read these documents from the fork; never merge documentation Main into a public feature branch. Push code branches only to origin, never directly upstream.

## Repository preservation

The local evidence `artifacts/repository-sync-2026-10-12/run-224100/INVENTORY.md` records 28 then-existing local branches backed up to origin and 20 then-existing clean worktrees, plus the fork's remote-only Copilot branch. Six deliberately failing regression controls remain outside Main and must never be merged. The SDK correction adds one original-base branch/worktree, for current totals of 29 local branches and 21 worktrees; no branches/worktrees were removed.

There are now 45 local tags and 35 fork tags. Ten older upstream reservation tags were imported locally by the initial fetch on 2026-10-13; they were preserved and not pushed. The prior 35-tag fork set still has the preserved local/origin `reserved/0.2.4` conflict. No version was allocated and no release workflow ran. Review inherited reservation history before any future fork release. Ignored SDKs, logs, packages and receipts remain local; Git push does not upload them.

## Evidence rules and limits

Read [analysis maps](../analysis/README.md), then trace actual source and consumers. Reports are not proof. Original runtime audit `41190e0` and documentation baseline `c077a05` remain historical; excluded development `67435a4` is not imported. Incident causes, acoustic latency, real receiver reliability and physical power-loss durability are not established by mocks or native hello.

Use [the report template](ISSUE_TEMPLATE.md). Preserve schema 2, JSONL v1, manual persistence, selected-adapter no-fallback, authentication exclusivity, accessory-keyed current-user DPAPI, exact prior mute bits and nonblocking dispatcher behavior. Fixtures use synthetic temporary data. Separately authorized real-device probes remain gain at most 0.1 and at most five seconds; never a hardware soak.

## Current implementation — stable refresh on 2026-10-11

Historical cross-link: the refresh culminated in qualified code `4ae1db4`, later recorded on Main `8989b36`. Current implementation/results are above.

## Fork synchronization — 2026-10-11

Historical cross-link: that synchronization backed up 18 then-existing branches. The 2026-10-12 inventory supersedes its branch count and reboot deferral; historical artifacts are not relabeled as current Main execution.
