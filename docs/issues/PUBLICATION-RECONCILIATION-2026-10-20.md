# Current publication reconciliation — 2026-10-20

This is the current preparation index. It supersedes older *current-state* claims in the dated acceptance records, without changing their source or execution provenance. Observation: October 20, 2026, 22:10 CDT (October 21, 03:10 UTC), followed by the final read-only refresh recorded in the audit receipt. No runtime changes or tests were needed for this reconciliation.

## Decision and next action

The user selected **one umbrella tracking issue and one consolidated draft PR**, using the already qualified public integration with explicit subsystem review sections. This replaces the original separate-group publication plan. The user also selected **local preparation only, with explicit approval before upstream publication**, and keeping PR #25 open until a qualified replacement is published and linked. No closure of #25 is authorized by that condition. Keep existing issue/PR history; do not delete or recreate completed work. Local self-contained candidates are ready for review: [tracking issue](upstream-drafts/reliability-current-issue.md) and [consolidated PR](upstream-drafts/reliability-current-pr.md).

The combined candidate is `codex/integrate-current-stable` / `codex/desktop-qualification` at **`0533cbff37cb7e0a2d0b80cee0d1aed48737ed15`**, based on original stable **`439de6260e01efbfca528a12cf4d4c1010b3583e`**. It contains 32 outgoing commits, including integration merges, and a 78-file diff (+14,117/-758). Of the added lines, 9,940 are in 31 test/fixture files; three native source files also contain inline tests. This is substantial work, not eight tiny fixes. The integrated harness and shared controller composition already have the strongest exact-source qualification; using that candidate avoids repackaging those fixtures into every overlapping branch.

A single draft makes the final composition visible and minimizes repeated shared-file conflict resolution. Its cost is a broad review and a larger rollback boundary. DNS resource/process ownership and durable mute recovery require separate, explicit review within it. If the maintainer prefers smaller units, split into four review boundaries: tooling; DNS; shared session/settings/authentication/IPC/mute recovery; diagnostics/capture accounting. These are review choices, not functional prerequisites. Eight separate PRs are possible but not required by source. Keep at most one or two review-ready PRs in flight.

Before any separately authorized submission, refresh actual upstream, remove newly integrated scope, inspect every outgoing commit and the resolved diff, and rerun checks affected by any source change. Preserve PR #25 until a concrete authorized refresh/replacement is ready and linked. A combined replacement can close #25 as superseded after that transition; it must not imply that #25 itself merged. Retain canonical issues #14/#24 and close them through verified implementation only when their complete acceptance scope is included. Keep incident reports #6/#7 open until their observed symptoms are independently resolved. A new tracking issue does not establish their root cause.

## Current repositories and publication state

The documentation checkout is fork `main`; its audit-start head is `e660a413f7a1c7fd3aef6014c2b5b776ef574cf4`. Its runtime/build/test trees match integration `0533cbf`. Original Main remains `439de626`, schema **4**, including software stereo, application capture, per-receiver profiles/rates, password support and German localization. Do not restore the briefing's historical schema-2 assumptions over those changes.

The user's four open authored issues are correct:

| Issue | Current disposition |
| --- | --- |
| [#6 receiver disappears during playback](https://github.com/Ding-Kyoma/AirFlash/issues/6) | Incident remains open. Retention and resolve fixes do not prove every disappearance is resolved. |
| [#7 crackles or brief silence](https://github.com/Ding-Kyoma/AirFlash/issues/7) | Incident remains open. Local padding metrics are diagnostic data, not proof of a network/acoustic cause or cure. |
| [#14 delayed session/receiver intent](https://github.com/Ding-Kyoma/AirFlash/issues/14) | Canonical remaining session issue. Already integrated retention is only part of its scope. |
| [#24 authentication input/credential lookup](https://github.com/Ding-Kyoma/AirFlash/issues/24) | Canonical remaining authentication issue. Refresh its current behavior description when separately authorized; upstream already supports dashed PINs/password pairing. |

There are **seven** open issues across all authors: #6, #7, #14, [#16 socket error](https://github.com/Ding-Kyoma/AirFlash/issues/16), #24, [#28 stereo member wait](https://github.com/Ding-Kyoma/AirFlash/issues/28), and [#30 media-key volume](https://github.com/Ding-Kyoma/AirFlash/issues/30). Matching symptoms do not assign #16/#28/#30 to these changes. [#22 configuration recovery](https://github.com/Ding-Kyoma/AirFlash/issues/22) was closed completed by ThatMarko at **21:39:01 CDT on October 20**. Main did not change with that closure; the new null software-group-ID follow-up remains fork-only.

All seven PRs authored by ThatMarko were checked:

| PR | Verified status and implication |
| --- | --- |
| [#11 SDK preflight](https://github.com/Ding-Kyoma/AirFlash/pull/11) | Merged; issue #10 closed. Original A is upstream. |
| [#13 scrolling](https://github.com/Ding-Kyoma/AirFlash/pull/13) | Merged; issue #12 closed. Scrolling is retained, not a new PR scope. |
| [#15 session ownership](https://github.com/Ding-Kyoma/AirFlash/pull/15) | Closed **without merge**; issue #14 open. Historical zh-CN hidden-active-row assertion failed. Do not reopen or claim merge from branch ancestry. |
| [#21 MSI payload refresh](https://github.com/Ding-Kyoma/AirFlash/pull/21) | Merged; issue #20 closed. Maintainer advanced its branch; the original tip is preserved. |
| [#23 configuration recovery](https://github.com/Ding-Kyoma/AirFlash/pull/23) | Closed without direct merge; original `107c7df` implementation retained through maintainer [#34](https://github.com/Ding-Kyoma/AirFlash/pull/34). Original C is upstream. |
| [#25 authentication](https://github.com/Ding-Kyoma/AirFlash/pull/25) | **Only open PR; conflicting**. Frozen published head `234167b`, branch `codex/fix-auth-input`; current refreshed E is a different branch/head. Old green checks do not qualify a current merge. |
| [#27 IPC/EQ results](https://github.com/Ding-Kyoma/AirFlash/pull/27) | Merged through maintainer [#33](https://github.com/Ding-Kyoma/AirFlash/pull/33); issue #26 closed. Original F is upstream. |

All 15 accessible repository PRs were inspected, not just ours: #1 adapter discovery; closed unmerged experimental #5; #8 Traditional Chinese; #11; #13; #15; #19 passive retention; #21; #23; #25; #27; #31 German; #32 SDK lock pin; #33 F integration; #34 C integration. #17 is unavailable through both issue and PR APIs; no missing item is invented. #29 is a German-localization issue closed by #31. Submitted review records contain bot comments where present; maintainer comments/merges are distinct from a human APPROVED review. No new review, comment or upstream edit was submitted here.

Original Main's exact [CI run 39401748686](https://github.com/Ding-Kyoma/AirFlash/actions/runs/39401748686) passed **141 Python / 403 Core / 87 native**, with two native ignores, plus stability/bundle/startup checks. This is upstream evidence, separate from the larger fork qualification. Latest original [release v0.4.0](https://github.com/Ding-Kyoma/AirFlash/releases/tag/v0.4.0) targets `81fa26b`; neither current Main nor all fork work is thereby released.

## All-author and commit coverage

The repository is not shallow. The audit inventoried **349 reachable commits** across retained refs and all **86 root-to-Main commits** in actual upstream Main: 55 nonmerge and 31 merge commits, 40 on the first-parent path. There are 59 commits after the original audited `41190e0`. API history and local stable SHAs agree. All 86 commit summaries/changed paths were reviewed; all 31 merge commits have remerge-diff evidence, with resolution hunks in 12. This is not a claim that every line of all 349 historical patches was executed or that metadata alone proves correctness.

GitHub maps those stable commits to **four contributors**: Ding-Kyoma/HooinKyoma (66), ThatMarko (18), DANVEZ (1, Git author Uncle Vez), and Ym20100702 (1). The user’s “three authors” does not limit the audit. Ym's Traditional Chinese PR also changes catalog routing/resources/tests. German localization is maintainer work. Maintainer conflict resolutions and integration adaptations are included, not treated as mechanical merges.

The resolved source comparison preserves software stereo (`62ff10f` and `63b0481`), application capture/standby (`123e70a`), schema-4 reset/profiles/rates (`e8b8105` / `77901a3`), DANVEZ retention and maintainer adaptation (`f8b3110` / `ee85d03`), scoped DNS (`838e08a`), development/release version separation (`a5f6372`), ILLink pin (`f4b4c44`), and language selection. Of 131 paths changed upstream since `41190e0`, **92 retain the same blob or absence** in integration. The 39 overlapping paths were reconciled through their current consumers; blob equality alone is not a runtime proof. No inspected patch overwrites the named upstream contracts. `upstream/dev` remains excluded.

Ignored raw evidence is under `artifacts/reconciliation-2026-10-20/`: complete graph/commit/path inventories, all merge-resolution patches, API bodies/timelines/reviews/checks, branch/tag refs, and `source/SOURCE-RECONCILIATION.md`. Existing failed and successful execution receipts remain in their original folders. This audit did not overwrite them.

## Remaining implementation and report map

These eight current heads each branch from actual stable `439de626`. The seventeen original report IDs remain an acceptance map, not a demand for seventeen issues or eight new PRs. Local report numbers are unrelated to GitHub numbers. Report **03 is retention; 11 is manual routing**. Scrolling and installer are separate, already integrated work.

| Group / reports | Prepared branch / head | Already upstream | Remaining source-confirmed scope |
| --- | --- | --- | --- |
| A / 05 | `codex/fix-package-boundaries` / `81948f2a` | SDK preflight #11, installer #21 | Cleanup reparse-point/ancestor containment; preview artifact agreement; first actual PATH host. |
| B / 01,02,03,10,11 | `codex/refresh-session-lifecycle` / `5749b74a` | #19/f8b3110 passive retention and guarded identity refresh | Atomic owner/intent admission; delayed settings/retarget safety; attempted filtering; exact manual routing; shell shutdown; Connecting publication and Stop presentation. |
| C / 06,07 | `codex/fix-settings-shapes` / `95051e0e` | Original recovery/validation through #34, schema 4 | Null/blank software-group-ID guard; controlled repeated-Load/raw-byte recovery regressions. |
| D / 04 | `codex/refresh-discovery-resolve` / `da3649d2` | Scoped DNS-SD/cancellation, offline selected-adapter pause | Finite deadlines/retries; TTL/token/epoch separation; bounded native/process ownership; observed helper-exit settlement once a child may have started (proven unstarted owners may settle directly). |
| E / 08,09 | `codex/refresh-auth-storage` / `ed034756` | Dashed PIN/password support, strict native validation | Shared/final input guards; raw whitespace/UTF-8/NUL contracts; consume terminal events during input; single-open bounded credential/password reads with preserved errors. |
| F / 12,14 | `codex/fix-ipc-consumer-errors` / `d5bd126b` | Terminal IPC routing and truthful retained EQ preparation through #27/#33 | Remaining hello, capabilities and application-catalog consumers preserve terminal classification. |
| G / 13 | `codex/refresh-mute-recovery` / `a2845662` | Exact-bit RAM restoration, recording/application capture exclusions | Durable intent; exclusive GUID/generation ownership; conservative unresolved-recovery admission and exact-bit cleanup. |
| H / 15,16,17 | `codex/refresh-capture-diagnostics` / `1e195791` | Existing diagnostics serializer/command and underruns | Monitor access; optional shared-pull output-rate padding counters before EQ; final dynamic localization cleanup. |

A/C/F are **follow-ups**, not missing original merges. B must not republish #19 as wholly new. E must not claim first support for dashed PINs. The current schema and application/stereo/authentication consumers are retained across every package. There is no source-established unmerged-group functional dependency, but controller/view-model/harness/catalog overlap requires deliberate composition after any neighboring merge.

### Standalone readiness corrections

The combined `0533cbf` source includes the corrected broad hidden-row assertion, actual modal fixtures and strict diagnostic routes. Two **standalone packaging/test gaps** remain; they do not invalidate the separate combined execution:

- **B `5749b74`:** its old broad `UiSmoke.cs` still requires hiding the active receiver to remove the row. The intended contract retains that owned row with Stop. Its three focused 115-check runs do not execute the stale broad assertion. Correct it and qualify the resulting exact head before publishing B alone. This audit establishes a source mismatch, not a freshly executed failure.
- **E `ed034756`:** production PIN/password guards match integration, but the standalone tree lacks the new actual modal harness and `--ui-pairing-smoke` / `--ui-password-smoke` routes. Its 535 Core/build and unchanged-native provenance remain valid; integration's modal runs cannot be relabeled as standalone tests. Port focused fixtures and safe argument dispatch, then qualify the resulting exact head before publishing E alone. The current E draft now states this explicitly.

The current fork desktop workflow requests all integrated routes. Standalone qualification needs a supported-mode profile and strict reported mode/culture checks; missing switches must never silently select broad smoke. Do not publish an old branch merely to reduce the combined diff, or imply those exact standalone heads contain later integration-only harness changes.

## Qualification retained, not rerun by this audit

| Source | Actual existing evidence | Practical limit |
| --- | --- | --- |
| Combined `0533cbf` | 965 Core, zero skips; selected SDK 10.0.402 locked Release build, zero warnings/errors; six local focused cells / 1,394 assertions | Local full raster still fails after 38 checks under both same-binary Normal/Hidden launch controls. |
| Combined `0533cbf` | Exact-source hosted [39517562675](https://github.com/ThatMarko/AirFlash/actions/runs/39517562675): 18 desktop cells / 10,914 assertions; seven full regional cells ×1,289; 247 initial/final source files unchanged; artifact digest/mode/cultures/binaries independently checked | Hosted DPI 96/scale 1; console build setting differs from default local GUI output. It does not explain or waive local raster failure. |
| Native/scripts/Python/installer trees unchanged from `6eabb186` | Earlier exact-source [39484878539](https://github.com/ThatMarko/AirFlash/actions/runs/39484878539): 174 Python / 105 native, two intentional ignores; native build/lint/static checks | Provenance by unchanged trees, not new native execution at `0533cbf`. Local Rust/MSVC unavailable. |
| B `5749b74` | 521 Core at unchanged Core/test parent `ba64a863`; later App-only build and three focused culture cells ×115 | Broad standalone assertion above still needs correction. |
| E `ed034756` | 535 Core and warning-free build; exact unchanged native tree from `932f7a5`, 97 native/two ignores | No actual standalone modal fixture execution. |
| Other standalone scopes | A 173 Python/one local privilege skip; C 19 targeted/422 Core; D 124 targeted/515 Core and 12 synthetic helpers; F 50 targeted/453 Core; G 97 targeted/500 Core; H 34 targeted/437 Core plus hosted 95 native/two ignores | Each retains its recorded exact source and acceptance; combined counts do not replace it. |

Historical D cancellation/publication timeout was diagnosed with controlled schedules, including either publication-drain winner and deferring the fake cancellation join until callback return. Failed/red/mutation/fixed evidence is retained in [D acceptance](GROUP-D-ACCEPTANCE.md). A later green run with unchanged runtime was not the fix. Deadline/cancel/result alone never proves native quiescence; once a child may have started, resolve capacity settles after observed exact helper exit. Proven unstarted owners may settle directly. No arbitrary native callback-return-stack or real DNS guarantee is inferred.

Physical DPI/multimonitor, Windows 10 execution, real receiver/network/audio reliability and physical power-loss durability remain unqualified. Two native device/soak qualifications remain ignored. Four inherited Rust formatting files and 152 unconfirmed static imports remain explicit. No personal configuration, credential, endpoint, receiver or local installer was touched.

## Complete branch disposition

At audit start all **39 local heads exactly match origin**, with **40 origin heads**, **27 registered worktrees** and **45 local tags** retained. Documentation Main may advance for this report only; implementation heads stay fixed. Eight package heads and two clean code integrations are listed above. The full local disposition follows; “historical/support” does not mean merge it again, and “verification” may deliberately contain a failing control.

| Local branch | Head | Disposition |
| --- | --- | --- |
| `codex/audit-analysis` | `69513875` | Private documentation, preserved |
| `codex/desktop-qualification` | `0533cbff` | Qualified combined public code |
| `codex/fix-auth-input` | `234167b3` | Historical E; actual open PR #25 head |
| `codex/fix-build-preflight` | `de13b6d3` | Original A integrated upstream |
| `codex/fix-diagnostics-metrics` | `1396c55e` | Historical H, preserved |
| `codex/fix-discovery-resolve` | `b8ee7951` | Historical D, preserved |
| `codex/fix-installer-refresh` | `3e743666` | Upstream-integrated installer; maintainer tip retained |
| `codex/fix-ipc-consumer-errors` | `d5bd126b` | Current F follow-up |
| `codex/fix-jsonl-control` | `28cb83e5` | Original F integrated upstream |
| `codex/fix-mute-recovery` | `f3afb9a3` | Historical G, preserved |
| `codex/fix-package-boundaries` | `81948f2a` | Current tooling follow-up |
| `codex/fix-sdk-package-lock` | `386cc4d2` | Historical SDK support; pin now upstream; do not wholesale merge remaining CI logging |
| `codex/fix-session-lifecycle` | `d7782a3d` | Historical B, preserved; closed #15 is a different frozen head |
| `codex/fix-settings-recovery` | `107c7dfc` | Original C integrated through #34 |
| `codex/fix-settings-scroll` | `54cd18e0` | Scrolling integrated upstream |
| `codex/fix-settings-shapes` | `95051e0e` | Current C follow-up |
| `codex/fork-main-before-ab` | `76c9a767` | Private preservation snapshot |
| `codex/fork-main-before-installer-refresh` | `d30c043b` | Private preservation snapshot |
| `codex/fork-main-before-scroll` | `1a517c19` | Private preservation snapshot |
| `codex/integrate-current-stable` | `0533cbff` | Qualified combined public code |
| `codex/main-before-all-local-20261010` | `877a8540` | Private preservation snapshot |
| `codex/organize-issue-backlog` | `774533c2` | Private documentation |
| `codex/refresh-auth-storage` | `ed034756` | Current E; equivalent production composed into integration |
| `codex/refresh-capture-diagnostics` | `1e195791` | Current H |
| `codex/refresh-discovery-resolve` | `da3649d2` | Current D |
| `codex/refresh-mute-recovery` | `a2845662` | Current G |
| `codex/refresh-session-lifecycle` | `5749b74a` | Current B; equivalent production composed into integration |
| `codex/verify-abcd` | `6fe467b5` | Historical verification |
| `codex/verify-auth-absence-regression` | `a7c8646a` | Regression control, not a publication branch |
| `codex/verify-b-regional` | `d70f30dc` | Historical verification |
| `codex/verify-build-preflight` | `c004d88a` | Historical verification |
| `codex/verify-eq-read-deadline` | `4a444f0f` | Regression control |
| `codex/verify-eq-reply-regression` | `78649b30` | Regression control |
| `codex/verify-monitor-nowrap` | `7461fb22` | Regression control |
| `codex/verify-monitor-old-bounds` | `dc6f85c0` | Regression control |
| `codex/verify-padding-accounting-regression` | `b3a1d088` | Regression control |
| `codex/verify-pr-session-audit` | `bfed430b` | Historical verification |
| `codex/verify-settings-scroll` | `c25657d4` | Historical verification |
| `main` | `e660a413` at audit start | Fork integration and private documentation; never a public implementation base |

Origin-only **`copilot/fix-releasing-job@c494741f`** is preserved and was inspected. Its three-file change normalizes four-part input into release X.Y.Z (with a workflow log message) and allows the fork origin through the original-repository reservation guard. Current Main deliberately requires canonical release X.Y.Z and distinguishes development/release versioning. This is an unselected release-policy change, **not a missing runtime fix**; do not merge it, dispatch it or move reservation tags as “sync.”

There are 78 reachable commits outside fork Main, including equivalent independently based B/E histories and negative controls. This count is **not** a merge backlog. Ancestry and source equivalence are different. Upstream has six heads; its maintenance/integration/adapter heads are already ancestors of Main, while `dev` remains excluded.

Origin has 37 tag refs and upstream 47. Shared reservation tags `reserved/0.2.4` and `reserved/0.4.1` point to different objects; origin also has its own `v0.2.4`/`v0.4.1`. These are repository-scoped version histories. No tag was moved, mirrored or deleted. Preserve the fork and original release boundaries.

## Completion boundary

The current-state audit, source/report mapping, branch inventory and local publication candidates are complete. Independent source, remote/history and documentation specialists checked the findings. Link/manifest/hash/privacy/ref-preservation checks are recorded in a new ignored audit receipt. No new runtime test pass is claimed, no old receipt was replaced, and no issue/PR was closed, deleted, reopened, edited or created. The user has selected the consolidated flow; the next external step still requires explicit publication authorization. Any changed source must be qualified before submission. Local raster diagnosis remains a separate engineering task.
