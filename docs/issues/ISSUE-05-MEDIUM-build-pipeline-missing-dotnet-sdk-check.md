# [MEDIUM] Missing Pre-Flight Check for .NET 10 SDK in Build Pipeline

- **Issue ID**: ISSUE-05
- **Severity**: **MEDIUM**
- **Subsystem**: Build Scripts & Developer Experience
- **Status**: Open / Triaged
- **Target Files**:
  - [`scripts/dotnet.ps1`](file:///C:/Users/marko/AirFlash/scripts/dotnet.ps1#L1-L25)
  - [`scripts/build.ps1`](file:///C:/Users/marko/AirFlash/scripts/build.ps1)
  - [`scripts/build-installer.ps1`](file:///C:/Users/marko/AirFlash/scripts/build-installer.ps1)
  - [`desktop/AirFlash.App/AirFlash.App.csproj`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/AirFlash.App.csproj#L5)

---

## 1. Summary
The build scripts rely on [`scripts/dotnet.ps1`](file:///C:/Users/marko/AirFlash/scripts/dotnet.ps1) to discover and invoke the `dotnet` CLI. If a developer or build machine has a .NET Runtime installed (or an older .NET 8 or 9 SDK) but does not have the **.NET 10 SDK** installed, `dotnet` will exist in PATH and execute runtime commands, but `dotnet build` or `dotnet publish` will fail with obscure MSBuild versioning errors or command routing failures.

---

## 2. Technical Root Cause Analysis
In [`desktop/AirFlash.App/AirFlash.App.csproj:L5`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.App/AirFlash.App.csproj#L5) and [`desktop/AirFlash.Core/AirFlash.Core.csproj:L3`](file:///C:/Users/marko/AirFlash/desktop/AirFlash.Core/AirFlash.Core.csproj#L3):
```xml
<TargetFramework>net10.0-windows</TargetFramework>
```
and
```xml
<TargetFramework>net10.0</TargetFramework>
```

In [`scripts/dotnet.ps1:L10-L15`](file:///C:/Users/marko/AirFlash/scripts/dotnet.ps1#L10-L15):
```powershell
$dotnetCommand = Get-Command dotnet -ErrorAction SilentlyContinue
$dotnetPath = $sdkCandidates | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } | Select-Object -First 1
if (-not $dotnetPath) { $dotnetPath = if ($dotnetCommand) { $dotnetCommand.Source } else { throw 'Install the .NET 10 SDK.' } }
```

The script verifies that `dotnet.exe` exists in `PATH`, but does not verify:
1. Whether an actual SDK is installed (`dotnet --list-sdks`).
2. Whether the installed SDK version meets the minimum target (`10.0.x` required by `AirFlash.App.csproj` and `AirFlash.Core.csproj`).

On machines with only the runtime installed or older SDKs (such as .NET 8 or 9), `dotnet build` fails with:
`The command could not be loaded, possibly because: You intended to execute a .NET application: The application 'build' does not exist.`
or
`The current .NET SDK does not support targeting .NET 10.0. Either target .NET 8.0/9.0 or update to a newer .NET SDK.`

---

## 3. Reproduction Steps
1. On a machine with only .NET 9 SDK or .NET 10 Desktop Runtime installed (without .NET 10 SDK):
2. Run `powershell -File scripts/build.ps1`.
3. **Observed Result**: Fails abruptly with cryptic MSBuild or dotnet command syntax error.
4. **Expected Result**: An actionable error message explaining that the .NET 10 SDK is required and providing the installation guide (`https://dotnet.microsoft.com/download/dotnet/10.0`).

---

## 4. Proposed Solution
Update [`scripts/dotnet.ps1`](file:///C:/Users/marko/AirFlash/scripts/dotnet.ps1) to validate SDK availability:

```powershell
$sdks = & $dotnetPath --list-sdks 2>$null
if (-not ($sdks | Where-Object { $_ -match '^10\.' })) {
    Write-Error "A .NET 10 SDK is required to build AirFlash. Detected SDKs: $(if ($sdks) { $sdks -join '; ' } else { 'none' }). Install .NET 10 SDK: https://dotnet.microsoft.com/download/dotnet/10.0"
    exit 1
}
```
