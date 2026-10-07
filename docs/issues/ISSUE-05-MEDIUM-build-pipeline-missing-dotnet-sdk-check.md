# [MEDIUM] Validate the selected .NET SDK before release reservation and native build

- **Issue ID**: ISSUE-05
- **Implementation plan**: [Work package ISSUE-05](IMPLEMENTATION_PLAN.md#issue-05)
- **PR group**: A — Build readiness
- **Severity**: MEDIUM; build prerequisites fail after avoidable build/release side effects
- **Kind**: Defect — tooling
- **Subsystem**: Build & Tests
- **Status**: Open — source-confirmed preflight/order gap
- **Runtime baseline**: upstream `41190e0`; documentation baseline `c077a05`
- **Evidence status**: Scripts/configuration verified; no release build, reservation, or SDK installation attempted
- **Implementation status**: Proposal only
- **Target files**: [dotnet.ps1:5–24](../../scripts/dotnet.ps1#L5), [build.ps1:17–33](../../scripts/build.ps1#L17), [build-installer.ps1:39–45](../../scripts/build-installer.ps1#L39), [global.json:1](../../desktop/global.json#L1)
- **Analysis context**: [Audit/toolchain verification limits](../analysis/audit.md)

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
