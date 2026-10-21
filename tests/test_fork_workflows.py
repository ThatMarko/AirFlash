"""Execute workflow report guards with synthetic data, never the application."""

import base64
import json
import os
import shutil
import subprocess
from pathlib import Path

import pytest

ROOT = Path(__file__).resolve().parents[1]
SOURCE = Path(os.environ.get("AIRFLASH_WORKFLOW_SOURCE_ROOT", ROOT))
PWSH = shutil.which("pwsh")
pytestmark = pytest.mark.skipif(PWSH is None, reason="PowerShell 7 is required")


def _workflow(name):
    return (SOURCE / ".github" / "workflows" / name).read_text(encoding="utf-8")


def _guard(name, message):
    # The legacy optional routes had no report guard: exit zero alone passed.
    return next(
        (line.strip() for line in _workflow(name).splitlines() if message in line),
        "",
    )


def _layout_guard(name):
    lines = _workflow(name).splitlines()
    start = next(i for i, line in enumerate(lines) if "if (@($layout.normal).Count" in line)
    end = next(
        i
        for i in range(start, len(lines))
        if "AllGlyphsInsideValue" in lines[i] and "throw" in lines[i]
    )
    return "\n".join(lines[start : end + 2])


def _evaluate(fragment, case, tmp_path):
    environment = os.environ.copy()
    environment["AIRFLASH_WORKFLOW_CASE"] = json.dumps(case)
    script = (
        r"""
$ErrorActionPreference = 'Stop'
$case = $env:AIRFLASH_WORKFLOW_CASE | ConvertFrom-Json
$result = $case.report; $observed = $case.report; $layout = $case.layout
$exitCode = $case.exit_code; $culture = 'en-US'; $language = 'zh-TW'
$Clipboard = $true; $problems = [Collections.Generic.List[string]]::new()
$program = Join-Path $env:AIRFLASH_WORKFLOW_FIXTURE 'Program.cs'
try {
"""
        + fragment
        + r"""
    if ($problems.Count) { throw ($problems -join '; ') }
    @{accepted=$true} | ConvertTo-Json -Compress
} catch { @{accepted=$false;error=$_.Exception.Message} | ConvertTo-Json -Compress }
"""
    )
    environment["AIRFLASH_WORKFLOW_FIXTURE"] = str(tmp_path)
    encoded = base64.b64encode(script.encode("utf-16-le")).decode("ascii")
    result = subprocess.run(
        [PWSH, "-NoLogo", "-NoProfile", "-NonInteractive", "-EncodedCommand", encoded],
        env=environment,
        capture_output=True,
        text=True,
        timeout=15,
        check=True,
    )
    return json.loads(result.stdout)


def _report(mode):
    return {
        "ok": True,
        "mode": mode,
        "culture": "en-US",
        "ui_culture": "zh-TW",
        "checks": ["synthetic assertion"],
    }


REPORT_GUARDS = [
    (
        "fork-ui-diagnostics-validation.yml",
        "Synthetic apphost DPI check failed:",
        "--ui-diagnostics-smoke",
        "zh-CN",
    ),
    ("fork-integration-validation.yml", "Portable UI failed:", "full", "en-US"),
    (
        "fork-integration-validation.yml",
        "Portable DPI check failed:",
        "--ui-diagnostics-smoke",
        "zh-CN",
    ),
]


@pytest.mark.parametrize("workflow,message,mode,culture", REPORT_GUARDS)
@pytest.mark.parametrize(
    "change",
    ["healthy", "wrong-mode", "empty-checks", "missing-checks", "not-array", "wrong-culture"],
)
def test_requested_report_route_and_assertions(workflow, message, mode, culture, change, tmp_path):
    report = _report(mode)
    report["culture"] = culture
    if change == "wrong-mode":
        report["mode"] = "--ui-password-smoke"
    elif change == "empty-checks":
        report["checks"] = []
    elif change == "missing-checks":
        report.pop("checks")
    elif change == "not-array":
        report["checks"] = "synthetic assertion"
    elif change == "wrong-culture":
        report["culture"] = "fr-FR"
    actual = _evaluate(_guard(workflow, message), {"report": report, "exit_code": 0}, tmp_path)
    assert actual["accepted"] == (change == "healthy"), actual


@pytest.mark.parametrize(
    "workflow",
    ["fork-ui-diagnostics-validation.yml", "fork-integration-validation.yml"],
)
@pytest.mark.parametrize("change", ["healthy", "duplicate-counter", "missing-counter", "bad-dpi"])
def test_both_distinct_counters_are_measured(workflow, change, tmp_path):
    first = {
        "Name": "MonitorPaddedFramesValue",
        "DpiX": 1.25,
        "DpiY": 1.25,
        "Fits": True,
        "Visible": True,
        "AllGlyphsInsideValue": True,
        "RenderedCharacters": 20,
    }
    second = dict(first, Name="MonitorFullyPaddedPacketsValue")
    cards = [first, second]
    if change == "duplicate-counter":
        cards = [first, first]
    elif change == "missing-counter":
        cards = [first]
    elif change == "bad-dpi":
        second["DpiY"] = 1.0
    actual = _evaluate(
        _layout_guard(workflow), {"layout": {"normal": cards, "minimum": cards}}, tmp_path
    )
    assert actual["accepted"] == (change == "healthy"), actual


@pytest.mark.parametrize(
    "change", ["healthy", "broad-route", "empty-checks", "not-array", "no-paste", "failed"]
)
def test_optional_clipboard_requires_actual_paste_proof(change, tmp_path):
    report = _report("--ui-pairing-smoke")
    report["ui_culture"] = "en-US"
    marker = "real PIN paste applies the single-line and sixteen-character editing bounds"
    report["checks"].append(marker)
    if change == "broad-route":
        report["mode"] = "full"
    elif change == "empty-checks":
        report["checks"] = []
    elif change == "not-array":
        report["checks"] = marker
    elif change == "no-paste":
        report["checks"] = ["pairing without clipboard"]
    elif change == "failed":
        report["ok"] = False
    actual = _evaluate(
        _guard("group-native-validation.yml", "PIN clipboard report did not prove"),
        {"report": report},
        tmp_path,
    )
    assert actual["accepted"] == (change == "healthy"), actual


@pytest.mark.parametrize(
    "change", ["healthy", "missing", "not-synthetic", "incomplete", "unsettled", "native-owner"]
)
def test_optional_dns_requires_complete_synthetic_helper_proof(change, tmp_path):
    report = {
        "ok": True,
        "synthetic": True,
        "completedHelpers": 6,
        "parentAdapterSettled": True,
        "parentNativeOwners": 0,
        "startupAndReplyMilliseconds": [1, 2, 3],
        "watchdogMilliseconds": 5000,
    }
    if change == "missing":
        report = {}
    elif change == "not-synthetic":
        report["synthetic"] = False
    elif change == "incomplete":
        report["completedHelpers"] = 5
    elif change == "unsettled":
        report["parentAdapterSettled"] = False
    elif change == "native-owner":
        report["parentNativeOwners"] = 1
    actual = _evaluate(
        _guard("group-native-validation.yml", "DNS helper report did not prove"),
        {"report": report},
        tmp_path,
    )
    assert actual["accepted"] == (change == "healthy"), actual


@pytest.mark.parametrize("change", ["healthy", "missing-program", "unsupported-source"])
def test_unsupported_historical_source_cannot_launch_normal_app(change, tmp_path):
    if change != "missing-program":
        text = (
            'if (args[0] == "--dns-resolve-helper-check") return Check.Run(args);'
            if change == "healthy"
            else "static void Main() { StartRealServices(); }"
        )
        (tmp_path / "Program.cs").write_text(text, encoding="utf-8")
    actual = _evaluate(
        _guard("group-native-validation.yml", "Historical source lacks the isolated DNS"),
        {},
        tmp_path,
    )
    assert actual["accepted"] == (change == "healthy"), actual


def _first_run(name):
    lines = _workflow(name).splitlines()
    start = next(i for i, line in enumerate(lines) if line.strip() == "run: |")
    indent = len(lines[start]) - len(lines[start].lstrip())
    end = start + 1
    while end < len(lines):
        if lines[end].strip() and len(lines[end]) - len(lines[end].lstrip()) <= indent:
            break
        end += 1
    return "\n".join(line[indent + 2 :] for line in lines[start + 1 : end])


@pytest.mark.parametrize("workflow", ["desktop-validation.yml", "headless-validation.yml"])
def test_accepted_uppercase_sha_is_normalized_for_later_evidence(workflow, tmp_path):
    environment = os.environ.copy()
    environment.update(
        REQUESTED_SOURCE="A" * 40,
        GITHUB_REPOSITORY="ThatMarko/AirFlash",
        GITHUB_OUTPUT=str(tmp_path / "outputs"),
        GITHUB_ENV=str(tmp_path / "environment"),
        RUNNER_TEMP=str(tmp_path),
        GITHUB_WORKSPACE=str(tmp_path),
    )
    script = _first_run(workflow)
    encoded = base64.b64encode(script.encode("utf-16-le")).decode("ascii")
    subprocess.run(
        [PWSH, "-NoLogo", "-NoProfile", "-NonInteractive", "-EncodedCommand", encoded],
        env=environment,
        capture_output=True,
        text=True,
        timeout=15,
        check=True,
    )
    assert f"sha={'a' * 40}" in (tmp_path / "outputs").read_text(encoding="utf-8-sig")
    assert f"REQUESTED_SOURCE={'a' * 40}" in (tmp_path / "environment").read_text(
        encoding="utf-8-sig"
    )


@pytest.mark.parametrize("has_paste", [True, False])
def test_desktop_clipboard_cell_requires_paste_proof(has_paste, tmp_path):
    report = _report("--ui-pairing-smoke")
    if has_paste:
        report["checks"].append(
            "real PIN paste applies the single-line and sixteen-character editing bounds"
        )
    actual = _evaluate(
        _guard("desktop-validation.yml", "Clipboard route omitted actual PIN paste assertions."),
        {"report": report},
        tmp_path,
    )
    assert actual["accepted"] == has_paste, actual
