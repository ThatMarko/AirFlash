# Consolidated PR readiness — 2026-10-21

## Linear review history — 2026-10-21

New candidate **`bbe283c8e641b059370d07033622df65522c9d61`** is on **`codex/reliability-review`**, directly based on unchanged original stable `439de626`. Its nine linear commits preserve every implementation/test blob from qualified `6c1e2ef`. All nine clean checkpoints passed their complete Core suite and zero-warning Release build. New commits use truthful current metadata; the original 37-commit DAG, every older branch and all upstream contributors remain preserved.

The live upstream inventory now has eight open issues (#6/#7/#14/#16/#24/#28/#30/#35) and only PR #25 open/conflicting. The new iPhone-wake dropout report #35 has no established root cause. Source regressions and final qualification support #14/#24 closing references. The user considers #6/#7 fixed after a few days using fork release v0.4.1 (`fb6721d`), explicitly reporting no disconnects. This is positive field evidence for the older fork build; the observation does not manually qualify `bbe283c` or establish an individual cause. The other incidents/features remain separate. Upstream publication, #25 closure, merges and releases remain gated.

This is the current implementation and qualification overlay. Earlier dated audits, standalone heads, failures and execution receipts keep their original provenance. The selected publication package remains **Improve AirFlash reliability, pairing and diagnostics**: one umbrella tracking issue and one consolidated draft PR. Upstream publication and closing #25 still require explicit authorization.

## Source and publication boundary

The code-only review diff is **75 files (+15,047/-755)** in **nine commits, zero merges**. All nine complete trees and their diffs exclude fork-only planning/artifact material and contain no documentation changes. Upstream Main is an exact ancestor, retaining the other authors' stable work. Normal own-fork push has been verified at `bbe283c`.

The user explicitly selected **all documentation changes on fork Main only**. Therefore `docs/` and `installer/README.md` on the new branch match original stable unchanged. Exactly five old-to-new document differences are intentional: discovery diagnostics, mute recovery, preview notes, releasing and installer README. Every other tracked Git blob matches qualified `6c1e2ef` exactly; whole-tree identity is deliberately not claimed. Old `codex/desktop-qualification` and `codex/integrate-current-stable` stay at `6c1e2ef`, preserving all 37 original commits, their merge resolutions and earlier execution provenance.

Fresh upstream still has only PR #25 open, conflicting at old `234167b`; authored open issues are #6/#7/#14/#24, with #16/#28/#30/#35 also open across other authors. Original A, scrolling, installer refresh, C and F integrations remain distinguished from new follow-ups. The [all-author/branch reconciliation](PUBLICATION-RECONCILIATION-2026-10-20.md) retains the complete inventory; its old current-candidate and local-raster observations are superseded here.

The [issue](upstream-drafts/reliability-current-issue.md) and [PR](upstream-drafts/reliability-current-pr.md) drafts are unsent and retain eight acceptance/review sections. #14/#24 remain canonical reports; #6/#7 are incidents whose causes and resolution are not established by these changes. Keep #25 open through any separately authorized qualified replacement/linking step; no closure is authorized by preparation alone.

## Linear commits and intermediate qualification

| Order / scope | New commit | Core passed at this exact clean checkpoint |
| --- | --- | ---: |
| 1 / package boundaries and host selection | `a30c050` | 403 |
| 2 / loaded settings shape | `46f4f8d` | 422 |
| 3 / terminal IPC query consumers | `3be9fbb` | 472 |
| 4 / DNS lifetime and publication | `c4c292a` | 587 |
| 5 / diagnostics and capture padding | `7e7ecb8` | 621 |
| 6 / owned mute recovery | `9cd4338` | 726 |
| 7 / session intent and owned settings | `039dd84` | 884 |
| 8 / authentication errors and complete input | `4334e6e` | 1,016 |
| 9 / shared desktop verification | `bbe283c` | 1,016 |

Every row has zero failures/skips and a locked Release build with zero warnings/errors using selected SDK 10.0.402 from `desktop`. The eight behavior review sections remain; the ninth commit contains shared actual-control, routing and page-access assertions. All 26 original nonmerge commit IDs map once to these nine scopes. Original merge-resolved code is retained exactly. Independent history review confirmed ancestry, messages, dates, per-commit boundaries, TRX counters and source-tree identities.

Fresh final-head Python passed **173**, with one Windows symlink-privilege skip. Exact-source [desktop 39630589971](https://github.com/ThatMarko/AirFlash/actions/runs/39630589971) and [headless 39630594304](https://github.com/ThatMarko/AirFlash/actions/runs/39630594304) both **passed** for `bbe283c`, distinct from workflow carrier `abb0883`:

- Desktop: **18 cells / 11,386 assertions**, seven full regional cells and clipboard; all **248** initial/final source-byte entries equal the frozen clean checkout. Archive/report/mode/culture and stable apphost/DLL checks pass. Observed DPI 96 / scale 1.
- Headless: **1,016 Core / 174 Python / 105 native**, zero Core/Python skips and two intentional native ignores (physical recording endpoint and thirty-minute localhost soak). Locked Release builds, Ruff, Clippy and static checks pass. All 248 initial source entries match; its final control is HEAD/tracked status, not a second full-byte map.
- Desktop artifact **12360595412** SHA-256 `4897d624d3f78076115f202a46198280d9522e841f420eb8b5beb0d5302e8b8e`; headless artifact **12360545000** SHA-256 `6414749e4a6ee55312b82e6558186999316861647f459f47fd4ae44f4b20b964`. Both ZIPs and every extracted file were verified.

The first desktop artifact verifier failed only when summing OrderedDictionary records. Original checker, raw artifact and failed receipt remain preserved. A separately frozen v2 projects numeric counts and writes a new review directory; its negative controls and full artifact verification pass without runtime changes, CI rerun or redownload. A lead wrapper initially used v1's manifest field name against v2; a separate strict check confirms all 16 frozen v1/v2 files match their pre-execution READY hashes. Prior executions below retain their recorded source; no result is relabelled by history cleanup.

The user considers #6/#7 fixed after a few days using [fork release v0.4.1](https://github.com/ThatMarko/AirFlash/releases/tag/v0.4.1), explicitly reporting no disconnects. The tag and release run bind this to **`fb6721d70da072c289fa36e7d729cc743f53f505`**, whose runtime parent is `892a458`; it is not current `bbe283c`.

Release [run 38428521695](https://github.com/ThatMarko/AirFlash/actions/runs/38428521695) checks out that SHA and its compatibility artifact binds the published EXE to SHA-256 `c3139ba42604de2c0a8093aba95739f83ed79ed9b06d812183c9e62a639707aa`, matching the release API and published checksums. This is recorded pipeline provenance, not a local inspection of the user's installed executable or a reproducible-build claim. The release description retains stale 0.4.0 wording; tag, assets, build logs and audit identify 0.4.1. No release was edited.

The tested fork already contains passive retention, finite DNS/helper ownership and padding counters. Current `bbe283c` carries those safeguards forward while retaining later upstream software topology, application capture/standby and audio-continuity changes. The old/new whole builds differ materially; telemetry is not an audio repair. The concrete observation is several days without disconnects, with no separately detailed crackle-specific measurement. Record positive owner evidence for the tested fork, without automatically closing upstream #6/#7 or assigning a cause. [Prepared incident updates](upstream-drafts/incident-status-updates.md) remain unsent.

Exact-head [portable/integration run 39634754382](https://github.com/ThatMarko/AirFlash/actions/runs/39634754382) is now pending for input `bbe283c`, carried by Main `2b4c526`. It uses unreserved validation version 0.3.1 and synthetic/disposable checks, producing a candidate for optional manual comparison. No local installation, receiver test, release/reservation or publication is authorized by that build; no pending result is counted as passed.

## Repairs retained from the qualified source

| Commit | Behavior and decisive evidence |
| --- | --- |
| `2a9c78f` | Reject tray verification combined with focused/modifier/unknown UI routes before platform/WPF work. Unchanged `0533cbf` actually ran password verification for a tray/password request and exited zero. Five corrected process launches reject at `stage=arguments`; valid password and isolated-tray controls pass. |
| `af29b9e` | Contain exceptions from secondary Trace logging when a discovery observer/logger fails. Three unchanged-runtime regressions fail because the fallback exception escapes. Fixed tests require the secondary sink to have thrown, later subscribers/publication and detached native cancel/start work to complete, and unsettled ownership to remain retained. |
| `b6bd554` | Add eight wrong-returned-endpoint controls for both prior mute bits, normal refresh/restore and both reviewed recovery choices. Rejection precedes direction/bit access, closes handles, retains the obligation and permits later recovery with the correct endpoint. No production mute change. |
| `c004187` | Preserve an owned session's effective receiver options when canonical persistence outlives a newer Pair. The actual WPF pre-fix continuation restarts at defaults (333→200 ms, 48000→44100 Hz), disposes the working process and adds a start/stop on a volume-only save. Forty Core cases and actual modal continuations now cover Pairing/Streaming flushes, stale owner/intent, endpoint restart, resets, empty discovery/migrations and deliberate software-member edits. |
| `6c1e2ef` | Repair the broad fixture's assumption that General must overflow. The retained failure screenshot already shows Reset fully accessible. Verify the actual page viewer's final offset and Reset/final Monitor footer bounds within its content presenter. Dedicated real-overflow/wheel assertions and raster checks remain intact. |

The settings context is captured under the application settings gate and projected only after serialized current owner/intent checks. Canonical persisted settings remain canonical. Ordinary software-group saves preserve owned member identities by channel, even after adapter browsing loses migration evidence; only the private effective copy removes redirecting aliases. Explicit membership edits follow their own admission path. Canonical options/clears require a unique current matching transport; an unproved canonical clear is persisted but does not erase another owned transport's effective overrides. Direct callers without a context retain authoritative whole-settings replacement. The latency hint evaluates the current panel global mode against effective owned overrides.

## Eight-section acceptance overlay

| Group / reports | Current combined acceptance and evidence |
| --- | --- |
| A / 05 | Original preflight is upstream; remaining package containment/preview/PATH follow-ups are retained. Python/tooling trees are unchanged; this pass ran 173 Python tests with one Windows symlink-privilege skip. [Prior acceptance/source inventory](PUBLICATION-RECONCILIATION-2026-10-20.md#remaining-implementation-and-report-map). |
| B / 01,02,03,10,11 | Existing owner/admission/selection/manual/retention/shutdown and Connecting controls remain. New 40-case settings-context suite passes; actual session route passes 174 and existing App Apply regression 167 in each of three independently selected culture/catalog cells. [B acceptance](GROUP-B-ACCEPTANCE.md) retains earlier results; new repairs above apply to the combined candidate, not unchanged standalone branches. |
| C / 06,07 | Original recovery is upstream; null software-group identity follow-up remains. Full Core and focused settings recovery (104) pass; schema 4 and unknown-field/original-byte protections remain. [C acceptance](GROUP-C-ACCEPTANCE.md). |
| D / 04 | Existing deadlines/token/TTL/helper-exit ownership and controlled cancellation/publication schedules remain. New three-case secondary-sink regression passes; focused WPF scheduling passes 50. Native settlement contracts are unchanged by this pass. [D acceptance](GROUP-D-ACCEPTANCE.md) retains the historical failed/red/mutation/fixed evidence. |
| E / 08,09 | Current pairing/password, one-reader terminal cleanup and bounded error-preserving lookup are retained. Actual modal pairing/password routes pass 379/243. Native credential/authentication tree is unchanged from its earlier Windows qualification. [E acceptance](GROUP-E-ACCEPTANCE.md). |
| F / 12,14 | Original native IPC/EQ results are upstream; remaining query consumers retain terminal classification and cleanup. Full Core includes their fake-event failures/recovery controls; native protocol tree is unchanged. [F acceptance](GROUP-F-ACCEPTANCE.md). |
| G / 13 | Durable intent/lease/ownership and conservative unresolved recovery remain. Eight additional mismatched-endpoint cases pass within full Core; focused recovery passes 103. No real endpoint or physical power-loss qualification. [G acceptance](GROUP-G-ACCEPTANCE.md). |
| H / 15,16,17 | Monitor copy/optional padding counters and localization cleanup remain. Focused diagnostics passes 73; new page-end checks cover the final explanation's viewport access. Native shared-pull accounting tree is unchanged. [H acceptance](GROUP-H-ACCEPTANCE.md). |

## Earlier verification retained at its recorded source

- Fresh unchanged `0533cbf` baseline: forced locked restore, zero-warning/error Release solution build and **965 Core**, zero skips. This distinguishes inherited behavior from new regressions.
- Exact clean final commit **`6c1e2ef`**: forced locked restore, **1,016 Core**, zero skips, and Release solution build with **zero warnings/errors** on selected SDK **10.0.402**, resolved from `desktop/global.json` (`10.0.100/latestFeature`). Generated application revision metadata also identifies `6c1e2ef`. The earlier precommit candidate had independently passed the same owning suite; its receipts remain retained.
- Local preparation snapshots: **15 intended application cells / 3,626 assertions**. Fourteen focused cells / 2,292 use the precommit source later committed as `c004187`; the subsequent full GUI cell / 1,334 uses the reviewed source committed unchanged as `6c1e2ef`. Only the broad `UiSmoke.cs` page-end fixture changed between these builds; their generated informational-version metadata still identifies the respective predecessor heads. These are source-matched snapshots, not fifteen binaries rebuilt from the final commit. Session and general regression run in `en-US/en`, `fr-FR/zh-CN` and `zh-TW/zh-TW`; the other eight focused routes use English. Both builds are warning-free. Receipts include actual route/cultures, process outcomes, application/Core/runtime DLL hashes and four page-end geometry measurements.
- Current Python run: **173 passed / one symlink-privilege skip**. Native/scripts/Python/installer trees equal earlier qualified `6eabb186`. Its [Windows qualification](https://github.com/ThatMarko/AirFlash/actions/runs/39484878539) retains **105 native / two intentional ignores**, native build and lint/static evidence. Local Rust/MSVC is unavailable; no new native execution is claimed.
- Final exact-source hosted [desktop qualification 39552755674](https://github.com/ThatMarko/AirFlash/actions/runs/39552755674) **passes 18 cells / 11,386 assertions**, including seven full regional cells ×1,334 and a 438-assertion clipboard cell on the disposable runner. Workflow Main is `1838732`, but its checkout/input is exact **`6c1e2ef`**. All **249** before/after source files match the independent exact checkout; route/culture, report hashes and stable application DLL/apphost hashes have zero discrepancies. Preserved artifact `12323494900` matches GitHub SHA-256 `7ace561a508fdcd4cf12609ad134aefac0ec888cec3843b415e06d22dcc43da9`. Observed DPI is 96 / scale 1. The workflow performs no reservation, native build, installer or release operation.

Independent source/test reviews cover lifecycle/context, routing/discovery/mute controls, the corrected page-end assertions and every outgoing public tree. Concrete review findings were fixed before final qualification. All 249 final tracked-source hashes are recorded. Raw results, earlier failed runs, pre-review unlaunched build cells, reviews and source/binary companions are retained under ignored `artifacts/pr-readiness-2026-10-20/`.

## Limits and next action

Earlier local full runs failed SettingsIcon alpha checks. Later runs pass those checks without a rendering-code repair; the earlier failure's cause/intermittence remains unexplained. Minimal DrawingVisual probes pass before/after Application, with the exact manifest, and before/during/after a plain Window; these do not explain full application initialization. A separate broad-fixture fitting-page failure was source-confirmed and corrected. One local run was cut off by an incorrectly shortened 60-second runner deadline while later fixtures were still progressing; the identical binary completed in about 75 seconds under the existing 120-second qualification limit. These are distinct outcomes, not green reruns relabelled as a raster or DNS fix.

Four initial supplemental cells requested unsupported `zh-Hans`/`zh-Hant` values and actually selected `zh-CN`. Their raw passes remain retained; they do not qualify Traditional Chinese. The corrected intended cells require exact observed `zh-CN`/`zh-TW`. Apphost hashes alone are insufficient to identify managed source; new receipts include DLLs, and earlier cells receive a separate provenance companion without replacing their original receipts.

The first local source-export helper failed its own temporary environment-restoration check, despite retaining the shared index and producing all 249 correct source files. That worker exited. A separate corrected export records the variable absent before/after and an unchanged shared index; all four original/corrected/CI source maps match. The initial false flag and runtime/archive receipts remain untouched. `hosted/export-control-resolution.json` records the control correction and late checker-script edits explicitly; no CI/runtime rerun or historical script hash is invented.

The selected SDK's first-run output reported installing an ASP.NET Core HTTPS development certificate. No trust/removal/certificate-inspection command was run; later SDK invocations set `DOTNET_GENERATE_ASPNET_CERTIFICATE=false`. Ordinary tests accessed only synthetic services/stores/identities; no personal config, credentials, receiver, capture or audio endpoint was inspected or changed.

Physical DPI/multimonitor, Windows 10, real receiver/audio/network reliability and physical power-loss durability remain unqualified. Four inherited Rust formatting files, 152 unconfirmed static imports and two intentional native device/soak ignores keep their earlier evidence boundaries. Standalone B/E packaging gaps remain separate from the selected combined source.

Earlier hosted evidence remains verified at its recorded source. Fork Main retains the operational documentation and identical implementation blobs; the new review branch deliberately preserves upstream documentation unchanged. Exact-new-head CI verification is complete; finish final prose/attribution review before requesting upstream publication of the concrete umbrella issue and consolidated draft PR. Do not close/reopen existing upstream items, merge upstream PRs or release as part of preparation.
