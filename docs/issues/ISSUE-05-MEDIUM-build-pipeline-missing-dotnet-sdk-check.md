# [MEDIUM] Selected SDK preflight precedes avoidable build and release side effects

## Current stable refresh — 2026-10-20

[Current headless implementation and evidence](CURRENT-HEADLESS-IMPLEMENTATION-2026-10-20.md) supersedes older branch/base and remaining-work claims below. The code follows original stable `439de626` and its schema-4 features; the linked report binds current branches, source, actual headless checks, reviews and limitations. The subsequent [desktop continuation](CURRENT-HEADLESS-IMPLEMENTATION-2026-10-20.md#desktop-continuation--2026-10-20) records newly authorized executions, Connecting, modal-terminal and password-contract repairs, actual modal/dispatcher results and the remaining full-raster limitation. Earlier headless-only claims retain their historical source.

Prior dated records retain their own executed provenance. Own-fork integration is not upstream integration or a release. New upstream publication, B reopening/replacement, upstream merges and releases still require explicit authorization.

### Historical issue metadata

- **Status**: Implemented in fork `main` `8989b3609dd1294f4d1d6eb1502f345cc22bc382` and **already resolved upstream**.
- **Group / feature**: A, `codex/fix-build-preflight`, `de13b6d3e22a177537ed9d5977fe693a5b497fc1`.
- **Upstream**: [PR #11](https://github.com/Ding-Kyoma/AirFlash/pull/11) merged, [issue #10](https://github.com/Ding-Kyoma/AirFlash/issues/10) closed; stable `cf420d97700463c56adc31408510e8336e99e51c` contains A. No original Group A implementation scope remains.
- **Original severity**: MEDIUM avoidable release/build effects before prerequisite failure at `41190e0`.
- **Plan / evidence**: [ISSUE-05](IMPLEMENTATION_PLAN.md#issue-05), [current acceptance](CONTINUATION-ACCEPTANCE.md), [installer acceptance](INSTALLER-REFRESH-ACCEPTANCE.md).
- **Analysis**: [Audit](../analysis/audit.md).

## 1. Current behavior

[dotnet-common.ps1](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/scripts/dotnet-common.ps1) resolves one executable/context for preflight and later invocation. Deliberate precedence remains explicit override, checkout-local artifacts/dotnet, integration-checkout artifacts/dotnet, then PATH. A preferred runtime-only/incompatible host is rejected with its path, resolver directory/policy, original output and actionable replacement/install advice; it is not silently substituted.

The selected host runs `--version` from **desktop**, letting [global.json](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/global.json) resolve `10.0.100` with `latestFeature`. Compatible feature-band roll-forward is accepted; exact-version or major-prefix regex is not substituted. Later invocation uses the same executable/cwd. [dotnet.ps1](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/scripts/dotnet.ps1) preserves runtime informational commands, argument forwarding and native exit propagation without forcing SDK preflight onto every diagnostic.

[build.ps1](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/scripts/build.ps1) checks before release output/lock/reservation, cleanup, cargo or publishing. [Release workflow](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/.github/workflows/release.yml) installs/checks the SDK before reservation. [build-installer.ps1](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/scripts/build-installer.ps1) gives default and -DotnetPath hosts the same desktop resolver/cwd before output creation; unrelated existing EXE/version inspection can precede preflight.

Main also contains separate installer payload-Rebuild and fork release-helper improvements. They are not new original A scope; [installer acceptance](INSTALLER-REFRESH-ACCEPTANCE.md) and current acceptance identify them separately. A release workflow with publish:false still reserves a version and is not a harmless prerequisite test.

## 2. Historical root cause and bounded reproduction

At [41190e0 build.ps1](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/scripts/build.ps1#L17), reservation/cleanup/native work preceded the .NET wrapper. The wrapper selected executable existence, not actual SDK resolution, and installer override used a different root cwd. Conditional release guards could fail sooner; no guard was bypassed to reproduce the ordering.

Temporary fake hosts/operations recorded baseline reservation, cleanup, cargo and publish before SDK failure, while fixed preflight recorded **zero** such operations. Existing reservation tests are controls, not proof of this ordering. No real reservation/deletion/release dispatch was needed.

## 3. Regression coverage

[test_dotnet_preflight.py](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/tests/test_dotnet_preflight.py) copies scripts to isolated fixtures and covers selection precedence, no host, runtime-only/incompatible shadowing, compatible roll-forward, desktop cwd/restoration, explicit installer override, contextual diagnostics, forwarding/exit codes, informational commands and early zero-operation failure. Reservation behavior remains independently tested.

## Local implementation verification — 2026-10-07

Original A at de13b6d passed **86 Python tests** (37 new), Ruff, SHA512-verified local SDK 10.0.401 resolution, locked restore, Release build with zero warnings/errors and 163 managed tests. Additional PowerShell 7 / 5.1 / plain-checkout checks and 149 isolated English UI assertions passed. [Hosted run 37657692237](https://github.com/ThatMarko/AirFlash/actions/runs/37657692237) checked exact de13b6d and passed native/managed checks, ordinary isolated publish, default/override MSI builds and runtime-only override rejection. Those results are historical source-bound evidence, not current-main release execution.

### Additional desktop and fork verification

Current `8989b36` sync/readiness passes SDK 10.0.401, warning-free Release build, 667 Core, 124 Python and lint. [Exact-main CI 38296750592](https://github.com/ThatMarko/AirFlash/actions/runs/38296750592) is green. Code-identical `4ae1db4` complete portable/UI/DPI/disposable-MSI qualification is in current acceptance. Native compilation is hosted; local Rust/MSVC prerequisites remain absent.

### Publication handoff — 2026-10-07

Issue #10 / PR #11 were created from the focused public feature and later merged upstream. Initial action_required/Copilot quota messages were publication-time observations; the user later identified automatic review settings. They are not current account/CI status. The archived [original detailed report](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/issues/ISSUE-05-MEDIUM-build-pipeline-missing-dotnet-sdk-check.md) retains those dated snapshots. No release or upstream publication occurs in this documentation update.

## 4. Acceptance criteria and compatibility

- Check same selected executable/cwd/policy used later.
- Failure precedes reservation/cleanup/cargo/publish, with explicit zero-operation assertions.
- Compatible roll-forward passes; informational commands/forwarding/exit behavior remain intact.
- Installer override and workflow ordering share the policy.
- JSONL/schema/runtime frameworks remain compatible.

This issue is resolved in current main and original stable. Current scripts preserve release repository/version guards; authorization to edit docs or run isolated checks is not authorization to dispatch a production release.
