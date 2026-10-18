# Comprehensive source, fixture and publication verification — 2026-10-18

This is fork implementation evidence, separate from upstream integration and receiver qualification. Original stable remains `cf420d97700463c56adc31408510e8336e99e51c`. Starting fork Main was `5c798ae8d30131eac294ae9c8c38539eb0b813ff`; the reviewed runtime correction is integrated at `a1182b0c984d882e9cd3fb3cbe443dbf2a10ed48`. Later documentation changes do not relabel earlier executions. Boot on October 16 satisfies the deferred execution gate. Two local SDK hosts resolve 10.0.401 and 10.0.402 from `desktop`; native qualification uses hosted Windows because local Rust/MSVC are unavailable.

## Findings and corrections

The audit traced all seventeen reports through current source, callers, persistence, cancellation, failure paths and assertion owners. Independent specialists investigated A-D, E-H and live publication evidence. It found one remaining B contract gap: actual shell Quit awaited Settings Apply and probe cleanup before entering VM disposal, while existing shutdown fixtures called VM disposal directly. A gated shared-shell regression reproduced a real fake Start and mute after an older automatic request's save completed during Quit. The failed control retains the original shell ordering and unchanged VM behavior; a later green run is not offered as its repair.

Standalone B correction [`d7782a3`](https://github.com/ThatMarko/AirFlash/commit/d7782a3d069f996b6d1c7c2fe65b224b6f13aa8e) blocks admission synchronously before Quit's first await, preserves accepted saves without applying replacement transports, retains owned Stop, updates Idle/disabled presentation after Stop, and attempts VM cleanup despite earlier shutdown-stage failures. Forty-six new shell assertions cover the actual shared path, exact prior mute bits and original/aggregated error outcomes. Independent final code/assertion review and separate conflict-resolution review are clear. Integration preserves G's mute-readiness checks, recovery-window closure and audio cleanup `finally`.

The original D intermittent publication/cancellation failure retains a controlled fixture diagnosis: fake cancellation synchronously waited for its own callback/drainer to return. The correction separates requesting cancellation from joining it and controls both deferred-drain winners. Retained red and deferral-mutation controls establish what changed; a production deadlock was not demonstrated. Current discovery tests pass with those assertions enabled. No D runtime change was needed in this audit. D prose now consistently distinguishes tracked Start/Cancel calls and managed callback consumer sections from unobserved arbitrary reverse-P/Invoke return-stack quiescence.

Six C/E/F report endings incorrectly described already-published packages as awaiting publication. Their dated supplements now link the actual open issue/PR and exact heads, preserving historical source/count snapshots. The four public issue/PR bodies themselves were accurate and were not changed.

## Actual verification

| Source | Executed result |
| --- | --- |
| B `d7782a3`, each local SDK | Locked restore, unchanged locks and active ILLink 10.0.12 analyzer; Release build 0 warnings/errors; **304 Core / 103 focused controller-selection**; focused shared-shell WPF **51 assertions** |
| B `d7782a3`, SDK 10.0.402 | **6 full WPF runs × 810**, observed en-US/zh-CN/zh-TW catalogs × en-US/zh-CN formatting; exact tray catalog restoration |
| B `d7782a3`, SDK 10.0.401 | Additional full English WPF **810** |
| Main `a1182b0`, each local SDK | Locked restore/build 0 warnings/errors; **677 Core / 113 focused discovery**, no skips; focused shutdown **51** |
| Main `a1182b0`, SDK 10.0.402 | Focused shutdown **51**; full observed zh-TW/zh-CN WPF **1,121** |
| Main `a1182b0`, synthetic UI performance | `--ui-perf` exits 0 with all five reported targets true: Apply feedback, cached pages, warm dispatcher, EQ edits and expected fake endpoint enumeration count. Machine-specific synthetic timings are not real-device latency. |
| Main `a1182b0`, hosted normal CI | [39170722402](https://github.com/ThatMarko/AirFlash/actions/runs/39170722402) succeeds: **138 Python / 677 Core / 69 native**, native lint/build, discovery stability, bundle audit and packaged startup |
| Main `a1182b0`, hosted native/PIN/DNS | [39170722485](https://github.com/ThatMarko/AirFlash/actions/runs/39170722485) succeeds: **69 native / 677 Core**, **225 modal clipboard PIN assertions**, **6 synthetic helpers**, settled parent and zero retained native owners |
| Main `a1182b0`, complete hosted package | [39170722309](https://github.com/ThatMarko/AirFlash/actions/runs/39170722309) succeeds: **138 Python / 677 Core / 69 native**, **6 portable WPF runs ×1,121**, **3 observed DPI runs ×40**, payload refresh and disposable MSI lifecycle |

The **51** focused checks comprise five existing diagnostic-startup controls and 46 new shell assertions. The **1,121** combined and **810** standalone full counts differ because the combined fork includes other packages. The narrower 103-test filter is recorded exactly; it does not replace older source-bound 139/166-test selections.

Downloaded final logs/TRX/reports bind all three completed runs to exact `a1182b0` and selected SDK 10.0.402. Portable/native/MSI hashes match the package manifest and audit bindings. Every full UI cell reports its actual requested catalog/formatting culture; every DPI card reports X/Y 1.25, twenty rendered digits and finite visible geometry within its allocation. MSI install/repair/upgrade/uninstall/hash/data-preservation and predecessor payload-refresh checks pass; the summary does not invent individual MSI exit codes. Each native run discloses two ignored tests. Compatibility audits retain unconfirmed imports and do not qualify actual Windows 10 execution. A separate verifier records every consumed evidence hash in ignored `final-hosted-verification.json`.

Starting-source controls remain distinct: exact `5c798ae` package run [39169282122](https://github.com/ThatMarko/AirFlash/actions/runs/39169282122) and native/PIN/DNS run [39169283765](https://github.com/ThatMarko/AirFlash/actions/runs/39169283765) succeeded. They exercise the pre-correction **677 Core / 138 Python / 69 native**, six portable regional cells ×1,075, three observed fractional-DPI cells ×40, disposable MSI lifecycle, 225 modal clipboard PIN checks and six synthetic DNS helpers. Their success did not cover the newly reproduced shell-Quit schedule.

## Report and fixture coverage

| Group / reports | Observable assertion owners and important limits |
| --- | --- |
| A / 05 | Python/PowerShell fake selected hosts and operations verify resolver/working directory, forwarding, installer override and zero reservation/cleanup/cargo/publish after rejected preflight. Production release workflows are not dispatched. |
| B / 01,02,03,10,11 | Core controller/catalog/selection tests plus WPF lifecycle fixtures cover atomic owner/intent, delayed saves/serialization, real bound Stop, exact manual routing, retained states and the newly shared actual shell ordering. |
| C / 06,07 | Core recovery/validation fixtures and WPF startup integration use temporary invalid/BOM/locked/I/O-failure settings and assert preservation/refusal before playback or mute. |
| D / 04 | Core DNS ownership/deadline/wire/process tests, actual synthetic helper processes and gated WPF coordinator fixtures cover stale token/epoch, blocked Start/Cancel, observer recursion, settlement and metadata changes. Tracked sections do not prove arbitrary native return-stack quiescence. |
| E / 08,09 | Actual fake `pair_pin` submissions, typing/paste/ASCII/Unicode/cancel fixtures; native temporary-root credential lookup/DPAPI/error fixtures preserve one authentication method. Clipboard interaction runs on disposable hosted Windows. |
| F / 12,14 | Core fake owning-child IPC events and native parser/mailbox/handler tests assert exact serialized replies plus retained EQ preparation. Preparation adoption is not DSP pickup/crossfade completion. Already-open PIN-prompt terminal handling remains outside the bounded reader fix. |
| G / 13 | Temporary journals/fake endpoints test both original bits, interrupted/failing persistence/COM, exclusive lease, stale/corrupt/future/missing endpoints, repeated restore and deferred recovery; WPF uses actual recovery commands. Physical power-loss durability is not inferred. |
| H / 15,16,17 | Bound localized Monitor diagnostics/copy/error/refresh, optional old/new metrics and fractional-DPI checks; native partial/whole pull/silence/EQ-tail/reset tests assert local output padding once per shared pull. Dynamic catalog consumers were source-reviewed before deleting unused keys. |
| Scrolling / installer | Real WPF wheel/layout assertions use fake services. Isolated payload-refresh fixtures and disposable-runner MSI install/repair/upgrade/uninstall/data-preservation check actual package behavior. No local MSI install occurs. |

Full WPF runs include the common UI regression, settings recovery, lifecycle, startup/readiness, pairing input, mute recovery, diagnostics, EQ, discovery coordinator, scrolling and tray fixtures. Tests deliberately excluded from normal suites remain explicit: recording-device qualification and a thirty-minute native soak are ignored. Live audio/discovery diagnostic modes, personal settings/credentials and physical receiver/reboot tests are not part of this verification.

## Public issues and PR information

The four open PRs are mergeable/clean against verified stable, with successful executed upstream CI and matching exact feature/merge trees. Their full titles/bodies, canonical issue links, `Fixes` references, changed files, commits, checks and review/conversation state were inspected. No submitted reviews or maintainer approval were present. The Code Review plugin retrieved CI diagnostics only; independent source review is recorded separately.

| PR | Exact head | Issue | Successful upstream CI |
| --- | --- | --- | --- |
| [#21 installer](https://github.com/Ding-Kyoma/AirFlash/pull/21) | `74cba079e34cb1e607762f7e9af5a8da54b9368f` | [#20](https://github.com/Ding-Kyoma/AirFlash/issues/20) | 134 Python / 202 Core / 49 native |
| [#23 configuration](https://github.com/Ding-Kyoma/AirFlash/pull/23) | `107c7dfcb192e8bc37648b9f324d2c030247f893` | [#22](https://github.com/Ding-Kyoma/AirFlash/issues/22) | 96 / 300 / 49 |
| [#25 authentication](https://github.com/Ding-Kyoma/AirFlash/pull/25) | `234167b31092339b2e909bfa2cbee5748b6006e1` | [#24](https://github.com/Ding-Kyoma/AirFlash/issues/24) | 96 / 223 / 59 |
| [#27 JSONL/EQ](https://github.com/Ding-Kyoma/AirFlash/pull/27) | `28cb83e55eda2e4b50847aafd783e75244697d23` | [#26](https://github.com/Ding-Kyoma/AirFlash/issues/26) | 96 / 239 / 53 |

A #11, scrolling #13 and partial retention #19 are merged upstream. B #15 remains **closed/unmerged**, frozen at `11c6046`; its current fork branch is different and pushing it does not reopen or update that closed PR. D/G/H remain unpublished. All eight candidate complete diffs and every outgoing commit are checked separately from the fork-only planning tree. Preserved draft hashes/forms and published snapshots remain distinct from current branch heads.

All nine currently open upstream issues were inspected: #6/#7 retain historical user observations and later reported fork improvement without new exact-build hardware qualification; #14 is B's broader canonical issue; #20/#22/#24/#26 have the open PRs above. [#16](https://github.com/Ding-Kyoma/AirFlash/issues/16) reports a connection/setup socket error after restart, and [#28](https://github.com/Ding-Kyoma/AirFlash/issues/28) reports an incomplete stereo group. Both lack enough app-version/log/adapter/group evidence to assign a cause or claim resolution by B/D/authentication. A community reboot/network guess is not verified evidence.

## Handoff and limits

Managed B worktree: `fix-session-lifecycle/AirFlash`; branch `codex/fix-session-lifecycle`; original base `cf420d9`; reviewed/pushed commit `d7782a3`. The separate fork documentation/integration checkout is on branch `main`, runtime merge `a1182b0`. Absolute local paths, raw results and source/assembly bindings are preserved under ignored `artifacts/comprehensive-audit-2026-10-18` alongside prior evidence. Existing branches/worktrees/tags and useful artifacts are preserved; no force push or deletion is used.

Next external action is maintainer review of the four existing PRs or explicit selection of a prepared group for publication. Reopening/replacing B, new upstream issues/PRs, upstream merges and releases still need separate authorization. No such action, version reservation, real-device probe or hardware soak occurred. Passing these fixtures does not establish acoustic performance, HomePod reliability, physical reboot persistence or universal absence of defects.
