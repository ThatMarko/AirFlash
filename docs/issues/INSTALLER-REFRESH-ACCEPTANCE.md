# Repeated installer packaging acceptance

During the user-authorized A+B+Settings scrolling validation on 2026-10-07, local MSI builds returned success while containing the earlier A+B executable. The clean GitHub build contained the correct scrolling executable. This additional fix addresses a pre-existing packaging defect and does not activate another A–H backlog group.

## Reproduction and correction

Original upstream source `41190e0d13a63a714c08dffe73ababca1804875c` passed `AppExe` to WiX through preprocessor constants while reusing `installer/obj/Release`. WiX SDK 6.0.2's `CoreCompile` incremental inputs read the preceding binding tracking file. Changing the source path without changing its numeric version could therefore reuse an earlier package when the old inputs remained unchanged. A successful process exit and matching MSI version did not establish current payload bytes.

The separate `codex/fix-installer-refresh` branch starts directly from that original source. Its production change requests `Rebuild` when packaging, forcing WiX to bind the current executable. Group A's selected-SDK validation remains before output creation after integration; Groups A and B and the scrolling feature retain their separate branch heads.

The regression check uses PowerShell 7 on Windows and copies the installer project to an isolated path containing spaces. Two distinct, backdated synthetic PE copies retain the same file version, share the same project intermediates, and build using the default and explicit SDK selections. Read-only MSI inspection and extraction compare the contained executable's full checksum and File-table size with the intended input. The synthetic copies are never installed or executed.

| Check | Original helper | Fixed helper |
| --- | --- | --- |
| First package contains payload A | Passed | Passed |
| Second package contains payload B | Failed: contained A's bytes and size | Passed |
| Same executable version, different input bytes | Verified | Verified |
| First package survives second packaging unchanged | Passed | Passed |
| Published input and synthetic copies remain unchanged | Passed | Passed |
| MSI files unchanged by inspection/extraction | Passed | Passed |

Both fixed builds completed with zero warnings/errors using SDK 10.0.401 and WiX 6.0.2. Ignored reports retain the original failing and corrected runs. The new fork workflow repeats this payload regression and preserves its JSON/build/extraction logs alongside the existing disposable-runner installation checks.

## Final combined build

Feature `c15a37dad4655700100b202e908e531c2fa41aec` merged separately into the user's fork main at `c552529546b51fb0a242492d4c114c85444df077`. The only conflict joined Group A's SDK instructions with the new installer verification instructions, retaining both. The integrated SDK preflight suite passed all 37 tests, and the real repeated-payload regression passed again after integration.

The final source `1303a09127760912c1cdf6a47bdb14e45904fd62` passed [CI](https://github.com/ThatMarko/AirFlash/actions/runs/37711905828) and [fork packaging validation](https://github.com/ThatMarko/AirFlash/actions/runs/37711905836). The hosted repeated-payload report confirms both intended checksums, File-table sizes, unchanged inputs and preservation of the first package. Fresh install, repair, preceding stable upgrade, upgrade repair and uninstall also passed on the disposable runner with installed-file hash and user-data checks.

The [final build acceptance](SETTINGS-SCROLL-ACCEPTANCE.md#final-abscrolling-build) records artifact provenance, binary hashes and exact downloaded desktop results. The local managed EXE matches that downloaded EXE byte for byte. Read-only extraction confirms both local default/explicit-host MSIs and the downloaded MSI contain that exact EXE, SHA-256 `89982600d059a39875acfd5351b77796cc0023959d6c84f1112d6b18899e4bd7`, version `0.3.1.0`, size 77,187,230 bytes. Each package, the first local MSI and source EXE remain unchanged. The earlier stale local packages are rejected and retained only as diagnostic evidence.

The only outstanding UI validation is physical pointer/visual checking with an available desktop; it does not change the installer payload results. No local MSI installation or official release publication occurred. Group A/B branch heads and the scrolling implementation are unchanged by this fix. Fork-only acceptance documents and workflow changes are excluded from the three-file installer feature diff.
