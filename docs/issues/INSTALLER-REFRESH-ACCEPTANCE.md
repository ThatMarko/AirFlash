# Installer payload refresh acceptance

## Standalone preparation update — 2026-10-13

Prepared head `b6ae0955aa6b632c95adbc4ebbbc28d771cdf7e1` is integrated into code Main `892a458` and pushed. An actual current-stable cf420d9 control packaged A correctly but retained stale A bytes/size for B; standalone Rebuild passed both default/explicit hosts with preserved inputs/retained first package. New minimum-version control failed on unchanged lifecycle source; corrected predecessor/fallback selection occurs before user-data/install operations, yields a strictly lower valid MSI version or rejects 0.0.0. Local 134 Python tests, 38 predecessor cases, Ruff and selected SDK 10.0.401 passed. [Exact-source disposable run 38360540188](https://github.com/ThatMarko/AirFlash/actions/runs/38360540188) passed 202 Core, 134 Python and 49 native tests (two ignored), publish, bundle audit and embedded hello, refresh and full MSI lifecycle/hash/data preservation. Its source manifest and three downloaded file hashes are verified; optional integrated diagnostics UI was skipped on this standalone installer source. The first malformed branch-name dispatch 38360424366 rejected before builds and remains preserved, not qualification. No MSI was installed locally. [Draft index](upstream-drafts/README.md) binds final evidence separately from historical sections below.

## Current Main integration — 2026-10-12

Fork Main `8989b3609dd1294f4d1d6eb1502f345cc22bc382` contains feature `codex/fix-installer-refresh`, head `a20f9dbcecea8926f8c582fa89a935332442defa`, worktree `fix-installer-refresh/AirFlash`. Stable base is `cf420d97700463c56adc31408510e8336e99e51c`. Complete public diff and all outgoing commits exclude private analysis/issues/planning. Independent review is clear; no upstream publication is authorized.

Current [installer helper](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/scripts/build-installer.ps1) validates the actual selected SDK in desktop resolver context before output creation, then invokes WiX `-t:Rebuild` with absolute current `AppExe`. [Refresh fixture](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/scripts/test-installer-refresh.ps1) compares actual embedded bytes/size without executing synthetic payloads. [Predecessor selection](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/scripts/installer-test-common.ps1) is separately qualified below.

[Current combined acceptance](CONTINUATION-ACCEPTANCE.md) binds Main source trees to final `4ae1db4` package run 38294493405, where repeated payload refresh and full disposable MSI lifecycle passed. Fresh Main 124 Python/lint includes installer fixture coverage. Native/package/MSI checks are hosted; no current local MSI was installed.

## Reproduction and correction

Original `41190e0` helper used WiX SDK 6.0.2 incremental `build` and cached `installer/obj/Release`. Distinct executables with equal numeric version and changed source path could retain prior binding-tracking inputs, yielding a successful package containing stale bytes. Process success/version matching did not prove current payload.

Separate original-base feature changes packaging to `Rebuild`. Isolated PowerShell 7 fixture uses a path with spaces and two backdated synthetic PE copies with the same file version and shared intermediates. Read-only MSI extraction/File-table inspection checks checksum/size, both default and explicit SDK hosts, preserved first package and unchanged input/MSI bytes. Synthetic PE copies are neither installed nor executed.

| Observable check | Original helper | Fixed helper |
| --- | --- | --- |
| First payload A | Pass | Pass |
| Second equal-version payload B | Fail: still A bytes/size | Pass |
| Equal versions/distinct bytes | Verified | Verified |
| Earlier package/input unchanged | Pass | Pass |
| Read-only extraction/inspection | Verified | Verified |

Historical fixed SDK 10.0.401/WiX 6.0.2 builds had zero warnings/errors. Failed/corrected receipts remain in ignored local artifacts and historical integration below.

## Current stable refresh and installer fixture repair — 2026-10-11

Earlier run 38186854508 passed compilation/native/Core and audited publish but MSI upgrade failed 1603: unofficial 0.3.1 was installed over newly published actual 0.4.0. Downgrade protection was correct; the qualification fixture selected the wrong predecessor.

The fixture now reads actual published EXE numeric version, selects the nearest strictly lower canonical stable release and falls back to a lower synthetic WiX fixture when none exists. No downgrade/release guard is bypassed. Twenty-four pure cases cover equal/newer/missing/malformed releases, numeric ordering, arrays/scalar booleans, captured regex state, StrictMode and empty selection. Independent findings are resolved. Final package 38294493405 passes repair/upgrade/uninstall/filehash/user-data checks on disposable Windows.

## Final A+B+scrolling build — 2026-10-07

Historical feature `c15a37d` merged separately at `c552529`, retaining A SDK preflight. Historical exact `1303a09127760912c1cdf6a47bdb14e45904fd62` passed [CI 37711905828](https://github.com/ThatMarko/AirFlash/actions/runs/37711905828) and [integration 37711905836](https://github.com/ThatMarko/AirFlash/actions/runs/37711905836): 37 SDK preflight, repeated payload refresh and disposable MSI lifecycle.

Default/explicit local MSIs and downloaded MSI contained exact EXE `89982600d059a39875acfd5351b77796cc0023959d6c84f1112d6b18899e4bd7`, version 0.3.1.0, 77,187,230 bytes. Input/first package remained unchanged. Prior stale cached packages are rejected diagnostic evidence. [Historical scrolling acceptance](SETTINGS-SCROLL-ACCEPTANCE.md#final-abscrolling-build) preserves full archive/run/binary mapping and specific pointer follow-up.

## All-local-branch integration — 2026-10-10

Historical `9e894ab` contained the helper/fixture but was source-reviewed before then-required reboot; it was not freshly built in that audit. Prior `6fe467b` payload `d14cb80ad5a8ab13dbeeb3be7e4af814bf015c9ca2554b5d1f1fcd8f0d32d117`,0.3.1.0/77,230,639 bytes, passed both SDK-host paths/refresh. Historical public `d70f30d` passed separate hosted installer checks. None is relabeled current Main binary.

Current Main and exact final packaged payload are qualified as above. Reboot deferral/unpushed wording is obsolete, not current pending work. This separate installer package is ready for selected publication after upstream recheck. Future production release still needs separately authorized version/reservation/release-note review; no release workflow was dispatched for these checks.
