# SDK and locked analyzer dependency consistency

## Later D preparation — 2026-10-13

Standalone D `b8ee79511e2fa41ee02651dad83d50e008f9f680` now includes SDK consistency `03432df` and passes 303 Core / 113 focused on both SDKs; [D acceptance](GROUP-D-ACCEPTANCE.md) records its additional metadata/fixture corrections, six actual culture cells and hosted source qualification. Main `c5ddb894e8bf0f977a94920ff26878c3cf5e1004` contains it and passes 677 Core locally and in complete SDK 10.0.402 hosted/package validation; the linked D acceptance binds its actual UI/MSI results separately. Thus the remaining branches awaiting SDK 10.0.402 preparation are B/G/H; the D limitation recorded at the earlier repair completion below is historical, not a current blocker.

## Current correction — 2026-10-13

Qualified fork code is `7a4f9f58e9222e12a98afa398c51ebf31b31a4f0`. Shared two-file correction `386cc4d24e3ce2a88d449b523dd5aaf39f031fca` was independently based on original stable `cf420d97700463c56adc31408510e8336e99e51c`, reviewed, and applied to Main and the four existing open PR branches. It changes the WPF project and normal CI workflow; no runtime controller, Rust source, lock file or SDK resolver policy changes.

Local selected SDK remains **10.0.401** in the existing preferred installation. A separate official **10.0.402** installation under ignored artifacts supplies exact-host checks without changing global PATH or replacing that preferred SDK.

## Reproduced failure and cause

Installer PR #21, configuration PR #23, authentication PR #25 and successful Main CI used SDK **10.0.401**. [Initial F CI 38455062214](https://github.com/Ding-Kyoma/AirFlash/actions/runs/38455062214) and subsequent Main CI installed **10.0.402** and failed managed locked restore, before Core tests, with `NU1004`: implicit ILLink request **10.0.13** versus locked **10.0.12**. Python/native success did not exercise this managed dependency graph.

Actual unchanged-stable controls now establish both sides: explicit 10.0.401 locked restore succeeds; explicit 10.0.402 locked restore fails with the same isolated `NU1004`. Temporary official NuGet configuration and fresh output directories avoid reading or changing personal configuration. The initial local probe also encountered unavailable runtime packages from its inherited feed configuration; the isolated official-feed control separates that setup issue from the decisive version mismatch.

Single-file publishing enables the ILLink analyzer even when trimming is false. The SDK supplies its default package version; 10.0.401 bundles 10.0.12, while the exact official 10.0.402 archive bundles 10.0.13. The prior job did not print its wrapper-selected SDK; the corrected CI does. [NuGet's locked-restore contract](https://learn.microsoft.com/en-us/nuget/reference/errors-and-warnings/nu1004) rejects a changed requested dependency, so an existing package cache or an earlier green run cannot repair this graph mismatch.

## Implementation

[App project](https://github.com/ThatMarko/AirFlash/blob/7a4f9f58e9222e12a98afa398c51ebf31b31a4f0/desktop/AirFlash.App/AirFlash.App.csproj) explicitly declares `Microsoft.NET.ILLink.Tasks` **10.0.12** with `PrivateAssets="all"`. The SDK removes matching explicit references from its implicit additions. This preserves the already-qualified lock request/resolution rather than shifting the mismatch to the next SDK default change. Private assets retain analyzer/build consumption without propagating this dependency to consumers.

The [normal CI workflow](https://github.com/ThatMarko/AirFlash/blob/7a4f9f58e9222e12a98afa398c51ebf31b31a4f0/.github/workflows/ci.yml) runs the existing selected-SDK check immediately after installation, logging the actual executable, desktop working directory and resolved version. `global.json` remains 10.0.100/latestFeature, host precedence is preserved, and locked restore/analyzers/single-file/self-contained/untrimmed settings remain enabled. The application runtime is not pinned by this tooling reference. Intentional future tool upgrades must update the explicit reference and lock together.

## Exact-host verification

| Source | SDKs actually checked | Release Core result |
| --- | --- | --- |
| Main `7a4f9f5` | 10.0.401 and 10.0.402, separate installations/output trees | 669 passed each |
| F `28cb83e` | 10.0.401 and 10.0.402 | 239 passed each |
| C `107c7df` | 10.0.402 | 300 passed |
| E `234167b` | 10.0.402 | 223 passed |
| Installer `74cba07` | 10.0.402 | 202 passed |

Each cell performs forced **locked** restore, a Release solution build, owning Core tests, unchanged lock-hash checks and actual ILLink analyzer-item inspection. All solution builds have zero warnings/errors. Candidate upstream-only builds additionally passed 202 Core tests on each SDK. Fresh isolated SDK 10.0.402 UI checks passed **568 assertions** for F and **1,064** for Main. An initial Main caller stopped at its 45-second bound with no completed report; the apphost invocation completed in 52.5 seconds under a 120-second caller bound. This is test-launcher qualification, not a production timeout change.

| Existing upstream PR | Current head | Complete SDK 10.0.402 CI |
| --- | --- | --- |
| [Installer #21](https://github.com/Ding-Kyoma/AirFlash/pull/21) | `74cba079e34cb1e607762f7e9af5a8da54b9368f` | [38461067260](https://github.com/Ding-Kyoma/AirFlash/actions/runs/38461067260): 134 Python, 202 Core, 49 native |
| [C #23](https://github.com/Ding-Kyoma/AirFlash/pull/23) | `107c7dfcb192e8bc37648b9f324d2c030247f893` | [38461059958](https://github.com/Ding-Kyoma/AirFlash/actions/runs/38461059958): 96 Python, 300 Core, 49 native |
| [E #25](https://github.com/Ding-Kyoma/AirFlash/pull/25) | `234167b31092339b2e909bfa2cbee5748b6006e1` | [38461050044](https://github.com/Ding-Kyoma/AirFlash/actions/runs/38461050044): 96 Python, 223 Core, 59 native |
| [F #27](https://github.com/Ding-Kyoma/AirFlash/pull/27) | `28cb83e55eda2e4b50847aafd783e75244697d23` | [38461045917](https://github.com/Ding-Kyoma/AirFlash/actions/runs/38461045917): 96 Python, 239 Core, 53 native |

All four runs explicitly log SDK 10.0.402 and pass discovery stability, self-contained bundle audit and packaged startup/compatibility. Each retains the two existing ignored native cases: recording-endpoint qualification and thirty-minute localhost soak. Code Review supplied CI diagnostics; independent agents reviewed source, regressions and evidence. The inherited Node action deprecation annotation remains nonblocking.

Main `7a4f9f5` independently passes [normal CI 38462862266](https://github.com/ThatMarko/AirFlash/actions/runs/38462862266) and [unofficial package 38462862242](https://github.com/ThatMarko/AirFlash/actions/runs/38462862242), both explicitly selecting 10.0.402: **138 Python / 669 Core / 69 native**, with two existing native ignores. The latter also passes repeated packaging, embedded-engine/package verification, six portable UI runs with **1,064 assertions each**, three actual 125% DPI fixtures with **40 each**, and disposable MSI fresh install/repair/upgrade/uninstall/hash/user-data preservation. Downloaded reports confirm exact source, cultures, counts and DPI layout; these are executed current-code results, separate from earlier qualification. Raw logs, reports, exact source/host receipts and final publication snapshots remain under ignored `artifacts/sdk-lock-fix-2026-10-13`.

## Separate discovery fixture correction

The first Main SDK 10.0.401 suite passed 668/669 and exposed an early cancellation-count assertion. A following unchanged-runtime green did not resolve it. Both existing publication-drain cases now hold one resolve cancellation worker before its counter increment: callback producers and browse settlement finish while only three native Cancel entries have occurred. The captured cancellation-worker task remains incomplete, with two resolve owners/six input buffers retained.

The fixture releases and joins those registered workers before expecting four calls. It verifies that cancellation still does not settle resolve inputs; only the independent synthetic API-lifetime boundary releases the remaining owners. Gates and worker joins run in finally. Controlled old assertions fail **2/2** with expected 4/actual 3; corrected cases pass **2/2**, and standalone D passes **295 Core tests** on 10.0.401. Only `NativeDnsOperationTests.cs` changes, in D `cb9da32104fb98715f0b06db1730255fa78a4e2b` and Main `7a4f9f5`; production/native cancellation and resource policies are unchanged. Independent final review is clear.

## Boundaries and next action

All four public PR diffs and every outgoing commit exclude private planning; no new upstream issue/PR, merge or release was created by this repair. All existing branches, worktrees and failed controls are preserved. Remaining unpublished B/D/G/H branches still need the common SDK correction or a refresh from an actually integrated original base before a new 10.0.402 publication qualification. D's fixture-only follow-up does not itself supply that dependency correction.

Fake/hosted checks do not establish receiver reliability, acoustic behavior, physical journal durability or Windows 10 execution. Next external step is maintainer review of the four open PRs; further group publication and upstream merging remain separately authorized.
