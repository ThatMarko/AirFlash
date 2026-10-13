# MSI packaging can retain an older executable when the publish input changes

## Describe the bug

Repeated MSI builds can succeed while embedding an executable from an earlier publish. On stable main `cf420d97700463c56adc31408510e8336e99e51c`, `scripts/build-installer.ps1` uses an incremental WiX build with a shared intermediate directory. Changing the `AppExe` path while retaining the executable's numeric file version can leave the previous payload in the MSI. A successful build and matching EXE/MSI versions do not prove that the current bytes were packaged.

## Steps to reproduce

Use an isolated checkout and temporary outputs; do not install or execute the synthetic payloads.

1. Copy a valid published AirFlash executable into two publish directories, A and B. Append distinct fixture bytes so the copies have identical file-version metadata but different checksums and sizes. Backdate both inputs.
2. Build an MSI from A with the installer helper and retain it in a separate output directory.
3. Reuse the same installer project, intermediates and numeric version, but build from B into another output directory. Exercise default host selection for A and an explicit compatible `-DotnetPath` for B.
4. Extract `ApplicationExe` without installing the MSI and inspect its File-table size. Compare the embedded checksum and size with B.
5. Verify that both inputs and the retained A package remain unchanged.

An isolated run against unchanged stable `cf420d97700463c56adc31408510e8336e99e51c` reproduced the mismatch: A packaged correctly, while B contained A's checksum and File-table size. B was 77,335,085 bytes; the MSI still recorded 77,334,957 bytes. The same controlled packaging check passed with the prepared Rebuild change.

## Expected behavior

Each successful installer build should embed the exact current `AppExe` bytes and size, including repeated equal-version builds. Inputs and packages retained in separate output locations should remain unchanged. Rebuilding into the same output filename may replace that package normally.

This is a packaging defect; changing the packaging build must preserve Windows Installer's downgrade protection and existing upgrade/repair policy.

## Reproduction frequency and environment

Reproduced with the controlled equal-version/backdated-input fixture. This does not establish that every ordinary MSI build is affected.

- Windows 11 Pro Insider Preview, OS build 26220; PowerShell 7; .NET SDK 10.0.401; WiX Toolset 6.0.2.
- Installation method: built from source; no local MSI installation was performed.
- Original source: `cf420d97700463c56adc31408510e8336e99e51c`. The fixture input's numeric EXE version was `0.3.1.0`.

## Source and acceptance criteria

- [Incremental installer invocation and inputs](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/scripts/build-installer.ps1#L32-L46).
- [Pinned WiX project](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/installer/AirFlash.Package.wixproj) and [ApplicationExe source](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/installer/Package.wxs#L55-L58).

Acceptance:

- Rebuild stale intermediates so the current executable is bound on every helper invocation.
- Verify embedded checksums and File-table sizes using distinct, backdated, equal-version inputs, a path containing spaces, and default/explicit hosts; preserve inputs and the retained first package.
- Preserve selected-host SDK preflight, version limits, output checks and downgrade protection.
- Validate install, repair, strictly-lower-version upgrade and uninstall separately on disposable runners. Select upgrade predecessors below the actual new EXE version; use a valid lower synthetic fixture when no suitable stable release exists, or reject a minimum version before installation/user-data side effects.

Open and closed upstream issues/PRs were checked; no existing report covers this root cause. Merged #11 supplies SDK preflight, which this fix preserves.
