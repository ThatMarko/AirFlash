# AirFlash implementation backlog

## Linear review history — 2026-10-21

New code-only review branch **`codex/reliability-review` at `bbe283c`** starts from unchanged original stable `439de626`. Nine linear commits retain every implementation/test blob from qualified `6c1e2ef`; all nine clean checkpoints pass their complete Core suite and a zero-warning Release build. The original 37-commit branches, all historical refs and upstream contributors remain preserved. All documentation changes stay on fork Main; the review branch inherits upstream documentation unchanged.

The live upstream inventory now has eight open issues (#6/#7/#14/#16/#24/#28/#30/#35) and only PR #25 open/conflicting. The new iPhone-wake dropout report #35 has no established root cause. Source regressions and final qualification support #14/#24 closing references. The user considers #6/#7 fixed after a few days using fork release v0.4.1 (`fb6721d`), explicitly reporting no disconnects. This is positive field evidence for the older fork build; the observation does not manually qualify `bbe283c` or establish an individual cause. The user selected preparing closing references for all four authored issues, with this evidence distinction. The other incidents/features remain separate. Upstream publication, #25 closure, merges and releases remain gated.

## Earlier qualification — 2026-10-21, before history cleanup

Before the history cleanup, [readiness](PR-READINESS-2026-10-21.md) bound candidate `6c1e2ef`, five reviewed commits, the eight-section acceptance overlay, 1,016 exact-commit Core and 3,626 local preparation-snapshot assertions. Exact-source hosted validation passes 18 cells / 11,386 assertions with all 249 source files unchanged. It supersedes older current-candidate and blanket local-raster-failure statements below: a full GUI run now passes, while earlier unexplained raster failures remain explicit. Public branches and all 37 outgoing trees exclude fork-only material. Upstream publication and #25 closure remain gated.

## Selected next step — user questionnaire

The user selected one consolidated draft PR plus one umbrella issue, with local preparation only and explicit approval before upstream publication. Keep #25 open until a qualified replacement is published and linked; no closure is authorized. [Selected package and reviewable drafts](PUBLICATION-RECONCILIATION-2026-10-20.md#decision-and-next-action) retain the audit's source, test and limitation boundaries.

## Current publication reconciliation — 2026-10-20 evening

[The current reconciliation](PUBLICATION-RECONCILIATION-2026-10-20.md) checks all branches, all seventeen reports, all authors' stable commits and all accessible PRs. It confirms four open issues authored by ThatMarko (#6/#7/#14/#24), seven open repository issues, and only PR #25 open/conflicting at its old head. Issue #22 closed at 21:39 CDT without changing Main. Original A, scrolling, installer, C and F are integrated; the current A/C/F branches contain new follow-ups. Earlier observations below retain their own timestamps.

Proposed next flow: preserve existing issue/PR history, review one umbrella tracking issue and one consolidated **draft** PR from qualified `0533cbf`, with separate subsystem acceptance and optional smaller review boundaries. [Local candidates and full inventory](PUBLICATION-RECONCILIATION-2026-10-20.md#decision-and-next-action) are prepared; nothing is submitted. Standalone B/E still have specific fixture-packaging gaps despite the qualified combined harness. Local full raster remains failed. No new runtime test result is claimed by this audit.

## Current stable refresh — 2026-10-20

[Current headless implementation and evidence](CURRENT-HEADLESS-IMPLEMENTATION-2026-10-20.md) supersedes older branch/base and remaining-work claims below. The code follows original stable `439de626` and its schema-4 features; the linked report binds current branches, source, actual headless checks, reviews and limitations. The subsequent [desktop continuation](CURRENT-HEADLESS-IMPLEMENTATION-2026-10-20.md#desktop-continuation--2026-10-20) records newly authorized executions, Connecting, modal-terminal and password-contract repairs, actual modal/dispatcher results and the remaining full-raster limitation. Earlier headless-only claims retain their historical source.

Prior dated records retain their own executed provenance. Own-fork integration is not upstream integration or a release. New upstream publication, B reopening/replacement, upstream merges and releases still require explicit authorization.

## Upstream integration recheck — 2026-10-20

[Current verification](UPSTREAM-INTEGRATION-VERIFICATION-2026-10-20.md) confirms installer #21 and F #27 merged; C #23 closed without direct merge while its commits reached Main through #34. Issue #22 remains open; E #25 is open/conflicting. At that earlier observation, stable Main was `439de626` with schema 4 and fork runtime was `a1182b0`; the current refresh is linked above. Earlier dated base, schema-2 and four-open-PR statements below describe their historical source.

Fresh upstream checks pass 403 Core, 141 Python, focused settings70/scrolling214 and a warning-free SDK 10.0.402 build; exact Main CI passes 87 native. Added controls reproduce null software-group-id recovery and new IPC consumer gaps (11 failures / 9 healthy controls), plus preview filename and parent-junction cleanup defects using safe operations. Full WPF remains failed: current upstream and the previously qualified fork binary fail a raster assertion, as does an independent opaque rectangle. No production fix or full UI pass is claimed. The report records source/evidence/limits and next actions; no new upstream publication or release occurred.

## Comprehensive recheck — 2026-10-18

[The source/fixture/publication audit](COMPREHENSIVE-VERIFICATION-2026-10-18.md) found and repaired a remaining B shutdown gap that earlier direct-VM-disposal tests missed. Actual shell Quit now closes admission before waiting for Settings Apply or engine-probe cleanup. Accepted saves persist; owned Stop remains usable and publishes Idle; earlier shutdown-stage failures cannot skip VM cleanup. B `d7782a3` passes 304 Core on both SDKs, 51 focused shell assertions each, six observed full WPF cells ×810 on SDK 10.0.402 and another full 810 on 10.0.401. Independent final and integration reviews are clear. Fork runtime merge `a1182b0` passes 677 Core / 113 focused discovery on both SDKs and 1,121 full WPF assertions locally. The report binds current hosted qualification separately from the successful pre-correction baseline.

All seventeen report contracts were rechecked; the new B evidence supersedes earlier blanket shutdown acceptance. D's old intermittent failure retains controlled fixture/red/mutation diagnosis, rather than an unexplained rerun. Stale C/E/F publication prose and D lifetime overclaims are corrected. Existing PRs #21/#23/#25/#27 remain open with successful CI and no submitted maintainer reviews; B #15 remains closed/unmerged. Upstream issues #16 and #28 lack evidence to assign a cause. No upstream publication, reopen, merge, release or real-device test occurred. Earlier dated heads/counts below remain historical.

## Continuation qualification — 2026-10-18

All seventeen reports were already implemented before this continuation. The old unversioned handoff's reboot/E–H/D to-do state is historical. Boot on October 16 satisfies the execution gate; upstream stable remains `cf420d9`. Fresh starting Main `ed418c6` passes **677 Core / 113 focused discovery, 138 Python**, a warning-free SDK 10.0.402 Release build and one observed **1,075-assertion zh-TW/zh-CN WPF** run. Independent review confirms the explained D fixture corrections using retained red/mutation/fixed evidence; no production deadlock is claimed.

Remaining standalone preparation is now complete: [B](GROUP-B-ACCEPTANCE.md) `002c4e1` passes **304 Core / 166 focused** on SDK 10.0.402 and two observed full WPF cells ×764; [G](GROUP-G-ACCEPTANCE.md) `f3afb9a` passes **287 / 85** on both SDKs and six full cells ×630; [H](GROUP-H-ACCEPTANCE.md) `1396c55` passes **233 / 43** on both SDKs and six full cells ×603, Monitor39 and three observed 125% DPI cells ×40. G/H now report actual full-run cultures and restore the exact selected catalog. The three new feature heads are pushed and integrated at fork merge `36cd87f`; all six code/build trees remain byte-identical to starting Main, which already contained equivalent seams. Local tests, standalone hosted checks and older full-package results retain distinct provenance.

Independent source/regression/integration reviews are clear. All eight candidate complete diffs and every outgoing commit tree exclude private planning. All 29 branches and 21 worktrees remain preserved. The [draft index](upstream-drafts/README.md) and [SDK acceptance](SDK-LOCK-ACCEPTANCE.md) record current preparation; logs/source/binary bindings are under ignored `artifacts/implementation-2026-10-18`. Existing upstream PRs #21/#23/#25/#27 remain open with successful checks; B remains closed/unmerged and D/G/H remain unpublished. Next external action is maintainer review or separately authorized publication. No new upstream issue/PR, B reopen, upstream merge, release or real-device test occurred.

## Current fork Main — 2026-10-13

**All 17 reports are implemented, independently reviewed and integrated into fork Main.** Current code baseline `c5ddb894e8bf0f977a94920ff26878c3cf5e1004` is pushed and locally qualified on SDK 10.0.402 with 677 Core; its normal CI and full package run succeed, with actual reports recorded in [D acceptance](GROUP-D-ACCEPTANCE.md). Earlier `7a4f9f5` retains its two-SDK/full-package qualification. Earlier integrated code `892a458` retains its source-bound full-package qualification. The preparation pass added joined PIN timeout/Stop coverage, propagated the Pair persistence correction to standalone G, strengthened installer fallback-version qualification and enabled exact-source fork package validation. Main already retained synchronous B intent and immediate Pair admission. All runtime packages are integrated; the four updated public PR heads are ancestors of this code baseline. The refreshed D head is also an ancestor; its metadata/fixture corrections are integrated. Earlier `8989b36`/`4ae1db4` qualification remains historical; current test/harness/tool/workflow trees have changed. [Prepared drafts and branches](upstream-drafts/README.md) binds eight standalone heads/results separately. Local evidence is under `artifacts/pr-preparation-2026-10-13`. Later documentation-only commits may advance Main without changing this qualified code.

Original stable Main is `cf420d97700463c56adc31408510e8336e99e51c`, already contained in the fork. A and scrolling are upstream. B's [PR #15](https://github.com/Ding-Kyoma/AirFlash/pull/15) is closed without merge at frozen `11c6046`; [issue #14](https://github.com/Ding-Kyoma/AirFlash/issues/14) is open. Merged [PR #19](https://github.com/Ding-Kyoma/AirFlash/pull/19) supplies partial discovery retention/fresh-snapshot safeguards, while B retains atomic ownership, intent, admission and routing fixes. C–H and installer repairs are fork implementations, not upstream integrations.

| Group | Reports | Integrated feature head | Acceptance / upstream state |
| --- | --- | --- | --- |
| A | 05 | `de13b6d` | [Build preflight](ISSUE-05-MEDIUM-build-pipeline-missing-dotnet-sdk-check.md); PR #11 merged |
| B | 01, 02, 03, 10, 11 | `442e8d8` | [Session/receiver package](GROUP-B-ACCEPTANCE.md); PR #15 closed, issue #14 open |
| C | 06, 07 | `107c7df` | [Settings preservation/validation](GROUP-C-ACCEPTANCE.md); [issue #22](https://github.com/Ding-Kyoma/AirFlash/issues/22) and [PR #23](https://github.com/Ding-Kyoma/AirFlash/pull/23) open; not merged |
| D | 04 | `b8ee795` | [Bounded DNS process ownership](GROUP-D-ACCEPTANCE.md); publication pending authorization |
| E | 08, 09 | `234167b` | [PIN and credential lookup](GROUP-E-ACCEPTANCE.md); [issue #24](https://github.com/Ding-Kyoma/AirFlash/issues/24) and [PR #25](https://github.com/Ding-Kyoma/AirFlash/pull/25) open; not merged |
| F | 12, 14 | `28cb83e` | [IPC errors and EQ outcomes](GROUP-F-ACCEPTANCE.md); [issue #26](https://github.com/Ding-Kyoma/AirFlash/issues/26) and [PR #27](https://github.com/Ding-Kyoma/AirFlash/pull/27) open; not merged; SDK 10.0.402 CI is green |
| G | 13 | `fcded6b` | [Owned mute recovery](GROUP-G-ACCEPTANCE.md); publication pending authorization |
| H | 15, 16, 17 | `6855dfe` | [Diagnostics/padding/localization](GROUP-H-ACCEPTANCE.md); publication pending authorization |
| Scrolling | Separate UI follow-up | `54cd18e` | [Scrolling](SETTINGS-SCROLL-ACCEPTANCE.md); PR #13 merged |
| Installer | Separate packaging follow-up | `74cba07` | [Payload refresh and qualification fixtures](INSTALLER-REFRESH-ACCEPTANCE.md); [issue #20](https://github.com/Ding-Kyoma/AirFlash/issues/20) and [PR #21](https://github.com/Ding-Kyoma/AirFlash/pull/21) open; not merged |

## Current acceptance

[Refreshed D](GROUP-D-ACCEPTANCE.md): standalone `b8ee79511e2fa41ee02651dad83d50e008f9f680` passes **303 Core / 113 focused on each SDK**, six observed WPF cells **× 580**, focused dispatcher 16, actual synthetic helpers and [hosted Windows 49 native / 303 Core](https://github.com/ThatMarko/AirFlash/actions/runs/38471035620). Integrated `c5ddb894e8bf0f977a94920ff26878c3cf5e1004` passes 677 Core locally; its completed CI/package qualification also passes 138 Python / 69 native (two ignored), six portable UI cells × 1,075, three observed 125% DPI cells × 40 and disposable MSI lifecycle. Those integrated results remain separate from standalone D. Earlier SDK and combined results below retain their source-bound counts.

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
