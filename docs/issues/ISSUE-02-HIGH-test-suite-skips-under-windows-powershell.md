# [HIGH] Automated Release Verification Test Suite Skips Under Standard Windows PowerShell

- **Issue ID**: ISSUE-02
- **Severity**: **HIGH**
- **Subsystem**: Test Automation / Developer Tooling
- **Status**: Open / Triaged
- **Target Files**:
  - [`tests/test_release_version.py`](file:///C:/Users/marko/AirFlash/tests/test_release_version.py#L11-L15)
  - [`scripts/release-version.ps1`](file:///C:/Users/marko/AirFlash/scripts/release-version.ps1)

---

## 1. Summary
The Python test suite in [`tests/test_release_version.py`](file:///C:/Users/marko/AirFlash/tests/test_release_version.py) verifies the version bump and reservation engine in [`scripts/release-version.ps1`](file:///C:/Users/marko/AirFlash/scripts/release-version.ps1). However, the test file enforces an explicit requirement for PowerShell Core 7 (`pwsh`), skipping 33 out of 49 total repository tests on clean Windows development environments that only provide Windows PowerShell (`powershell.exe`).

Because standard Windows workstations and default GitHub Actions Windows runners default to Windows PowerShell 5.1, this guard leaves critical release verification logic untested locally unless PowerShell 7 is manually installed.

---

## 2. Technical Root Cause Analysis
In [`tests/test_release_version.py`](file:///C:/Users/marko/AirFlash/tests/test_release_version.py#L11-L14):

```python
PWSH = shutil.which("pwsh")
pytestmark = pytest.mark.skipif(PWSH is None, reason="PowerShell 7 is required")
```

When running `pytest`:
```text
tests/test_release_version.py: 33 skipped ('PowerShell 7 is required')
================ 16 passed, 33 skipped in 0.61s ================
```

### Verification Analysis:
Running [`scripts/release-version.ps1`](file:///C:/Users/marko/AirFlash/scripts/release-version.ps1) directly using `powershell.exe` demonstrates that all 33 test cases (git tag reservations, version bumps, clean tree validations) succeed without any PowerShell 7 specific features or language constructs. The script is 100% compatible with Windows PowerShell 5.1.

---

## 3. Reproduction Steps
1. On a standard Windows machine without PowerShell 7 (`pwsh.exe`) installed:
2. Run `uv run pytest`.
3. **Observed Result**: 33 tests are skipped. 0 release versioning scenarios are validated.
4. **Expected Result**: All 49 tests execute and pass using the available system PowerShell.

---

## 4. Proposed Solution
Update [`tests/test_release_version.py`](file:///C:/Users/marko/AirFlash/tests/test_release_version.py#L11-L14) to fall back gracefully:

```python
PWSH = shutil.which("pwsh") or shutil.which("powershell")
pytestmark = pytest.mark.skipif(PWSH is None, reason="PowerShell (pwsh or powershell.exe) is required")
```

This ensures full test coverage on standard Windows developer environments and CI pipelines.
