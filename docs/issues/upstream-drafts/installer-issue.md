# MSI packaging can retain an older executable when the publish input changes

## Description

Repeated MSI builds can succeed while packaging bytes from an earlier publish. The installer helper invokes WiX's incremental build with an `AppExe` input and a shared intermediate directory. Changing the input path or replacing the executable while retaining its numeric file version can leave the earlier binding cached. Matching EXE/MSI versions and a successful build do not establish that the MSI contains the current executable.

The expected behavior is that every successful installer build embeds the exact current `AppExe` bytes and size, including repeated builds with the same numeric version. Packages retained in separate output locations and publish inputs should remain unchanged; rebuilding into the same output filename replaces that package normally.

This affects packaging integrity, independently of receiver playback, pairing or discovery. It does not mean Windows Installer should permit a downgrade or every same-version major upgrade.

## Steps to reproduce

Use an isolated temporary checkout/output directory and synthetic payload copies; no installation or executable launch is required.

1. Obtain a valid published AirFlash executable and make two copies, A and B, with identical file-version metadata but different bytes and sizes. Backdate both inputs so the check does not depend on a newer timestamp invalidating WiX's cache.
2. Package A through the installer helper and retain the resulting MSI.
3. Reuse the same installer project/intermediate directory and numeric version, but package B from a different publish path. Exercise both default and explicit .NET host selection.
4. Read the MSI File table and extract its `ApplicationExe` payload without installing it.
5. Compare its embedded executable checksum and size with B. Also verify that A, B and the first package remain unchanged.

A new isolated execution against unchanged original stable `cf420d97700463c56adc31408510e8336e99e51c` reproduced the same stale-B hash and File-table size mismatch. The standalone rebuild helper then passed both default-A and explicit-B packaging with the same synthetic inputs.

## Reproduction frequency and environment

The controlled equal-version/backdated-input fixture reproduced the mismatch. Ordinary builds with other timestamps or intermediate states are not claimed to fail every time.

The executed tooling used Windows, PowerShell 7, .NET SDK 10.0.401 and WiX Toolset 6.0.2. This is a source-build packaging problem; no household endpoint, credential, receiver or local installation was involved. Hosted package/lifecycle verification uses disposable Windows runners.

## Source anchors

- [Current stable installer invocation and inputs](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/scripts/build-installer.ps1#L32-L46).
- [Pinned WiX SDK and installer project](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/installer/AirFlash.Package.wixproj).
- [Published EXE as the MSI file source](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/installer/Package.wxs).

## Bounded fix and acceptance

- Rebuild the WiX intermediates for each helper invocation so changed equal-version payloads bind to the current executable.
- Keep selected-host SDK validation, explicit host overrides, version limits and output checks intact.
- Add an isolated packaging regression with equal versions, distinct/backdated inputs, a path containing spaces, default/explicit hosts, actual embedded checksums/File-table sizes and preservation assertions. Do not execute or install the synthetic payloads.
- Keep deployment validation separate: install, repair, strictly-lower-version upgrade and uninstall on disposable runners, checking installed bytes, registration, shortcuts and synthetic user-data preservation.
- Choose upgrade-test predecessors by the actual new EXE's numeric version. A test package older than the latest published release must not accidentally be installed over that newer release; select a strictly lower valid stable predecessor or a lower isolated fixture. Preserve downgrade protection.

## Prepared implementation and executed verification

The prepared implementation is [codex/fix-installer-refresh](https://github.com/ThatMarko/AirFlash/tree/b6ae0955aa6b632c95adbc4ebbbc28d771cdf7e1), commit `b6ae0955aa6b632c95adbc4ebbbc28d771cdf7e1`, independently based on the original stable source above. Independent final source/regression review found no unresolved findings.

[Exact-source Windows validation](https://github.com/ThatMarko/AirFlash/actions/runs/38360540188) checked out this commit and passed **49 native tests**, Clippy, native Release build and the executable compatibility audit, plus the branch’s managed build/Core suite. Two existing tests remain ignored: recording-endpoint qualification and the thirty-minute localhost soak.

Fresh standalone checks passed **134 Python tests**, including **38 predecessor/fallback cases**, plus whole-tree Ruff. Actual SDK 10.0.401 followed desktop/global.json latestFeature; real repeated MSI packaging/extraction passed default and explicit hosts, embedded checksum/size and retained-input/package preservation. Synthetic payloads were neither launched nor installed locally.

The minimum-version lifecycle regression failed on the previous helper, then passed after predecessor selection moved before user-data/install operations. Synthetic fallback is now strictly lower than the actual three-part MSI version, or rejects 0.0.0 before those side effects. Product downgrade protection remains intact.

[Exact-source disposable package/lifecycle run](https://github.com/ThatMarko/AirFlash/actions/runs/38360540188) passed **202 Core tests**, **134 Python tests/lint**, native checks/build, audited portable publish/embedded hello, repeated MSI payload refresh and install/repair/upgrade/uninstall with hash/data preservation. Its manifest identifies this source separately from the workflow commit; all downloaded package file hashes match. Integrated diagnostics UI matrix was intentionally skipped for this installer-only source. No release reservation/tag or local installation was performed.
