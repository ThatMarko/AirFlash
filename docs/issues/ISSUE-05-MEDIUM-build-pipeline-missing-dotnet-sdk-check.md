# [MEDIUM] Validate the selected .NET SDK before release reservation and native build

- **Issue ID**: ISSUE-05
- **Implementation plan**: [Work package ISSUE-05](IMPLEMENTATION_PLAN.md#issue-05)
- **PR group**: A — Build readiness
- **Severity**: MEDIUM; build prerequisites fail after avoidable build/release side effects
- **Kind**: Defect — tooling
- **Subsystem**: Build & Tests
- **Status**: Published upstream as issue #10 / PR #11; not merged; upstream CI awaits maintainer approval
- **Runtime baseline**: upstream `41190e0`; documentation baseline `c077a05`
- **Evidence status**: Original audit was source-only; subsequent isolated implementation checks are recorded below
- **Implementation status**: `origin/codex/fix-build-preflight`, commit `de13b6d3e22a177537ed9d5977fe693a5b497fc1`
- **Target files**: [dotnet.ps1:5–24](../../scripts/dotnet.ps1#L5), [build.ps1:17–33](../../scripts/build.ps1#L17), [build-installer.ps1:39–45](../../scripts/build-installer.ps1#L39), [global.json:1](../../desktop/global.json#L1)
- **Analysis context**: [Audit/toolchain verification limits](../analysis/audit.md)

## Local implementation verification — 2026-10-07

Original stable `main` was freshly confirmed at `41190e0d13a63a714c08dffe73ababca1804875c`. The implementation branch starts there and contains only public tooling, regression tests and build documentation. Its final diff and sole outgoing commit exclude the fork's analysis/issues/planning files. Stable main still has the behavior described in the source audit below.

The shared helper resolves the selected host once and checks `--version` from `desktop`, leaving the actual `global.json` resolver in charge. Build preflight now precedes the release lock, reservation, cleanup and native work; publish and installer reuse the selected host. Explicit installer hosts adopt the desktop policy. The release workflow sets up and checks the SDK before reservation; the wrapper preserves native argument/output/exit forwarding and runtime-only informational commands. Independent final source/test review reported no concrete findings.

Executed checks: **86 Python tests passed with no skips**, including 37 new preflight/ordering tests; Ruff lint and new-test formatting passed. A separate temporary comparison using the original base scripts recorded fake reservation, cleanup, native and publish operations before SDK failure, while the fixed scripts rejected before all four. The SHA512-verified repository-local SDK **10.0.401** resolved the unchanged `10.0.100`/`latestFeature` policy; the actual runtime-only system host was rejected with contextual diagnostics. Locked solution restore, Release solution build (**zero warnings/errors**) and **163 managed tests** passed; package lockfiles and `global.json` remain unchanged. Restore used an explicit ignored NuGet configuration with the official package source because the isolated CLI home initially had no sources.

Initial implementation checks exercised release/build orchestration and installer behavior using copied scripts, fake operations and temporary outputs. A separate validation branch was subsequently pushed to the fork for the additional checks below. Task 3 later published the implementation branch and upstream issue/PR as recorded in the publication handoff.

### Additional desktop and fork verification

Prepublication verification tested the unchanged implementation commit on both the desktop and a fresh GitHub-hosted `windows-2025` runner. Desktop checks passed under PowerShell 7 and Windows PowerShell 5.1, in a real clean clone with its own Git directory, with 86 Python tests under CI's Python 3.12, 163 managed tests and 149 isolated English WPF smoke checks. PowerShell 7 retains fuller native failure diagnostics than 5.1. All WPF engine/audio/discovery/autostart services were simulated; no receivers or production endpoint settings were used.

The fork-only `codex/verify-build-preflight` branch contains a read-only validation workflow at `c004d88a4f3a8aacbf126eaea4fee628f8ffcf6d`. [Hosted run 37657692237](https://github.com/ThatMarko/AirFlash/actions/runs/37657692237) checked out the exact implementation commit `de13b6d` and passed actual SDK resolution, 86 Python tests, icons, 47 native tests (one existing soak ignored), native lint/build/static-CRT checks, the managed solution build and 163 managed tests. It also completed a real isolated publish, default and explicit-host MSI builds, and runtime-only override rejection before output creation. This used ordinary publish/installer commands, without calling the version-reserving release build or dispatching the Release workflow. No issue/PR, release, reservation tag or installer installation was created; fork `main` was unchanged.

Remaining platform boundaries: the desktop lacks Rust/MSVC/Windows SDK prerequisites for a full native release build. The existing reservation script intentionally rejects a fork `origin`; the fork's earlier [Release run](https://github.com/ThatMarko/AirFlash/actions/runs/37525792456) demonstrates that guard. GitHub account permissions do not change that repository policy. Group A preserves it while improving SDK failure ordering.

### Publication handoff — 2026-10-07

Published [issue #10](https://github.com/Ding-Kyoma/AirFlash/issues/10) and [PR #11](https://github.com/Ding-Kyoma/AirFlash/pull/11), targeting original `main` at verified `41190e0` from `ThatMarko:codex/fix-build-preflight` at `de13b6d`. No existing issue/PR covered the verified scope. Public bodies were independently reviewed and re-read after publication; GitHub recognizes the PR's `Fixes #10` relationship. The published nine-file diff and sole outgoing commit exclude all fork-only analysis/issues/planning files and the validation-only workflow. The PR is registered with this chat.

The exact-source fork validation remains green. Original-repository [CI run 37660774239](https://github.com/Ding-Kyoma/AirFlash/actions/runs/37660774239) is `action_required`, with no jobs, and its page explicitly awaits maintainer approval for the fork PR. Copilot's automatic review reported a quota limit; it performed no code review. Independent source/regression review found no concrete issues. Remaining external action: an upstream maintainer approves CI and reviews the PR. No merge, release workflow, reservation or release was performed. Stable main has not integrated the fix.

## Current behavior and scope

[dotnet.ps1:8–14](../../scripts/dotnet.ps1#L8) chooses the first existing executable from the checkout's `artifacts/dotnet/dotnet.exe`, the integration checkout's equivalent, then `dotnet` on PATH. It checks executable existence, not whether that selected installation can resolve the repository SDK. A runtime-only or stale local host can take precedence over a suitable system installation. With no executable, the wrapper already throws `Install the .NET 10 SDK.`; the gap concerns an executable that exists but cannot supply the required SDK.

[desktop/global.json:1](../../desktop/global.json#L1) requests SDK `10.0.100` with `rollForward: latestFeature`. [AirFlash.App.csproj:6](../../desktop/AirFlash.App/AirFlash.App.csproj#L6) targets `net10.0-windows`, and [AirFlash.Core.csproj:4](../../desktop/AirFlash.Core/AirFlash.Core.csproj#L4) targets `net10.0`. Those framework monikers alone do not describe SDK selection. `latestFeature` selects an installed compatible 10.0 patch/feature band at or above the specified SDK; finding any `10.` line in `--list-sdks` is not a complete resolution check. SDK selection also depends on working directory. [Microsoft global.json documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json) explains both rules.

The wrapper runs from `desktop` at [dotnet.ps1:22–23](../../scripts/dotnet.ps1#L22), so that `global.json` applies. Installer `-DotnetPath` bypasses the wrapper and executes from the repository root at [build-installer.ps1:8–9,39–43](../../scripts/build-installer.ps1#L39). Preflight must check the same executable and working-directory policy as the subsequent command, not an unrelated PATH host.

## Root cause and material effect

The build entry obtains a release lock, calls `Reserve-ReleaseVersion`, removes old build/dist directories, and builds Rust before invoking the .NET wrapper ([build.ps1:17–33](../../scripts/build.ps1#L17)). When reservation preconditions succeed, that function creates a remote reservation tag at [release-version.ps1:59–64](../../scripts/release-version.ps1#L59). A missing compatible SDK can then fail publishing after that version was consumed and native work/cleanup performed.

This is a conditional call-order defect, not an actual reservation/deletion performed by this audit. The personal-fork remote/clean-branch prerequisites may stop a real build earlier; they must not be bypassed to reproduce it. The CLI may already print a useful SDK-resolution error, so the original blanket claim of "cryptic MSBuild errors" is withdrawn.

Both workflows install `10.0.x` before .NET checks ([CI:16–18](../../.github/workflows/ci.yml#L16), [Release:43–45](../../.github/workflows/release.yml#L43)). That limits the usual CI case but does not validate arbitrary local candidate hosts. The release workflow itself reserves before SDK setup at [Release:33–45](../../.github/workflows/release.yml#L33).

## Minimal deterministic reproduction plan

Use disposable fixtures/fake commands, with no real build or network:

1. Supply a selected repository-local host whose SDK query fails or returns no SDK, alongside a compatible PATH host.
2. Exercise a factored selection/preflight function and record which executable/cwd it checks.
3. Stub reservation, cleanup, Rust build, and publish; record orchestration calls.
4. Current source predicts no SDK check before reservation/cleanup/native build. Desired behavior rejects the unsuitable selected SDK before those calls.
5. Cover no host, runtime-only host, unsupported SDK, compatible 10.0 host, and installer `-DotnetPath` separately.

This harness is proposed, not implemented/executed. Do not invoke real `scripts/build.ps1` as a prerequisite test.

## Proposed implementation and acceptance

Factor resolution/preflight so callers share the selected host and explicit cwd. Run an SDK-resolving command such as `--version` under the intended `global.json` policy, check exit/result, and report selected path, required policy, and an actionable [SDK installation link](https://dotnet.microsoft.com/download/dotnet/10.0). Do not replace resolution with a blanket `^10\.` filter or block diagnostic commands such as `--info`/`--list-sdks` on runtime-only installations.

- Local prerequisite failure occurs before reservation, cleanup, or native build. If the workflow is included in the fix, its SDK preparation/checks precede reservation too.
- Preflight checks the host later used, including explicit installer paths, with no hidden change between check/build.
- Compatible 10.0 roll-forward installations pass without requiring exactly `10.0.100`.
- Diagnostics distinguish missing host, unresolved SDK, and later genuine build failure.
- Fixture tests assert zero real pushes, output deletions, or compiler invocations.
- Runtime frameworks, JSONL v1, and configuration schema 2 remain compatible; this is tooling only.

## Existing and missing tests

[test_release_version.py](../../tests/test_release_version.py) covers reservation with fake git, not selected-host resolution/build order. [Lines 133–153](../../tests/test_release_version.py#L133) cover reservation conflict/pre-reserved handling, which must remain intact. No dedicated current test was found for local runtime-only precedence, preflight, or preventing reservation on SDK failure. The analysis audit's unavailable SDK is a toolchain observation, not an executed release-build reproduction. No audio/receiver failure or upstream issue-number relationship is asserted.
