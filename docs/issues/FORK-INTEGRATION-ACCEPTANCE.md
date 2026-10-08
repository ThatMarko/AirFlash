# Combined fork integration acceptance

Verification completed on 2026-10-07 for the authorized Group A + Group B fork integration. This is unofficial validation of `ThatMarko/AirFlash`; original `Ding-Kyoma/AirFlash:main` remains `41190e0d13a63a714c08dffe73ababca1804875c`. A fork merge does not imply upstream acceptance.

## Source and branch boundaries

- Group A: `codex/fix-build-preflight`, unchanged head `de13b6d3e22a177537ed9d5977fe693a5b497fc1`; upstream [PR #11](https://github.com/Ding-Kyoma/AirFlash/pull/11) is still open.
- Group B: `codex/fix-session-lifecycle`, unchanged head `342aeb77cb85ff6f3f7f850161b3db79d8637945`, with its earlier Core contract commit `4662796`. Its [separate checklist](GROUP-B-ACCEPTANCE.md) remains the report-by-report acceptance record.
- The Group B feature branch was pushed to the fork and its remote head verified as `342aeb77cb85ff6f3f7f850161b3db79d8637945`. Group A's existing remote branch remains `de13b6d`; no PR branch rewrite was required.
- Local fork documentation history was preserved through `76c9a76`. Group A merged at `ca84c7c`, then Group B at `72dc7c7df6649a95271e0a86ae62f371397b2ca4`, without source conflicts.
- A local backup branch preserves the pre-integration fork main. Both implementation branches remain based directly on original stable main; their complete diffs and every outgoing commit exclude fork analysis/issues/planning and this validation workflow.
- Historical source links in the fork's analysis/issues are now pinned to original `41190e0`; new feature behavior has separate links to the tested feature heads.

## Fresh combined desktop checks before push

- Independent deep reviews of both production changes and regression assertions found no actionable code defects. No new production changes were required.
- Group A's relevant prerequisite/release tests: 70 passed without skips.
- Combined Python suite: 86 passed without skips; Ruff passed.
- Selected SDK `10.0.401`; locked managed restore passed. Combined Release and Debug desktop builds passed with zero warnings/errors; all 218 Core tests passed without failures/skips.
- Generated WPF/PNG/ICO icons verified.
- The new fork-only workflow passed independent review and YAML/PowerShell parsing. A caught runtime-only rejection now explicitly exits successfully after its expected-failure assertions; package metadata must match the source SHA.

## Hosted build and exact downloaded binary

Merged fork main was pushed at **`b2105c9c158769ca06f8dc4e39d0e525fb497402`**. Its [normal CI](https://github.com/ThatMarko/AirFlash/actions/runs/37677853767) and [fork package validation](https://github.com/ThatMarko/AirFlash/actions/runs/37677853674) both completed successfully on that exact source. Both feature heads remain unchanged, and all 19 integrated feature files match their independently reviewed branch blobs exactly.

The fork-only `Fork integration validation` workflow checks out the triggering SHA with a read-only token, runs Python/native/managed checks, publishes directly, builds both installer-host variants, verifies the embedded native hello and performs installer lifecycle checks only on the disposable hosted runner. It never invokes the version-reserving production build or Release workflow.

The package's numeric version is an **unreserved test value `0.3.1`**, chosen above the then-current upstream `v0.3.0` solely for hosted MSI upgrade testing. Its informational version includes `fork-validation` and the exact source SHA. This does not alter the project version, reserve a tag, publish a release, or establish the next stable version.

Executed hosted checks: **86 Python tests, 47 native tests and 218 Core tests passed**, with one existing native localhost soak ignored and no managed/Python skips. Python/native lint, generated icons, static-CRT validation, Release build/publish, both default/explicit-host MSI builds, and runtime-only rejection before installer output creation passed. Managed and installer builds reported zero warnings/errors. The published EXE's embedded native hello passed; the disposable runner passed fresh install, repair, prior stable upgrade, upgrade repair, uninstall, file hashes and user-data preservation.

The successful build artifact is `fork-integration-b2105c9c158769ca06f8dc4e39d0e525fb497402-37677853674`, ID **11509100627**. Its API-reported archive digest was verified after raw download: `sha256:c5b00cceac18ca1063912ed6cfa565249d7a3198715a65b2db0301f7767006cc`. Manifest repository/ref/source SHA/run ID and all three EXE/MSI/native-engine file hashes were checked before launch and again after desktop verification. The immutable archive/manifest binds these binaries to the successful run, rather than a floating latest build.

The **exact downloaded portable EXE passed 273 isolated WPF checks in English and 273 in Chinese on this desktop**. Retained discovery cards/screenshots were visually inspected in both languages; unavailable status and actual Stop control fit. The separate downloaded native engine passed correlated JSONL v1 hello and exited normally without creating a playback/pairing worker. Verification processes exited; no desktop MSI installation occurred. Bundled native extraction/hello was checked on the hosted runner, preserving the desktop's normal engine cache.

## Local publishing and installer generation

The combined source was republished locally with the same SDK **10.0.401**, locked package policy, version/source metadata and the exact native engine compiled by GitHub. The resulting portable EXE is **byte-for-byte identical** to the downloaded build; both SHA256 hashes are `f8dbe0568e632349059c34f0234fd7ce90f696ee8c0e953af8de5be4c41b9d19`. The completed desktop binary checks therefore apply to both copies without redundant execution.

The first local MSI attempt found an uncached WiX SDK and zero configured NuGet sources in the selected CLI-home setup. Normal NuGet SDK resolution loads configuration from the project context ([resolver source](https://source.dot.net/Microsoft.Build.NuGetSdkResolver/NuGetSdkResolver.cs.html)); the explicit restore config outside that search hierarchy did not supply installer sources. A temporary official-only `installer/NuGet.Config` populated WiX `6.0.2` and its dependencies. It was removed after default/explicit-host MSI builds passed; another build without that temporary config passed from the populated cache. All three builds had zero warnings/errors, and read-only MSI inspection confirmed version `0.3.1`. No global NuGet settings or production code changed.

Full Rust compilation/tests occurred on GitHub, not locally. Local publish reused that verified native compiler output; local MSI generation did not install the application.

Local evidence is retained under ignored `artifacts/fork-integration/`: CI/package logs, hosted TRX/MSI logs, archive metadata/ZIP, downloaded assets/manifest, `desktop-downloaded/receipt.json` and UI reports/screenshots, `local-republish-comparison.json`, and `local-installer-receipt.json`. GitHub artifacts expire under the configured 14-day retention policy; these local copies preserve the tested build.

## Limits and next action

Subsequent user-authorized fork validation added the separate Settings scrolling and installer payload-refresh fixes. Their [final A+B+scrolling build acceptance](SETTINGS-SCROLL-ACCEPTANCE.md#final-abscrolling-build) records the later artifact and exact downloaded checks; the A/B-only build evidence above remains historical. The original A and B feature branches retain their reviewed heads.

No real configuration, credential store, receiver, capture or endpoint mutation is part of desktop validation. The mock UI does use isolated real tray/window registrations. Native localhost tests and mock tests do not establish hardware/acoustic reliability. MSI installation belongs exclusively to the disposable hosted runner.

Group B is ready for Task 3 upstream issue/PR submission from its separate original-base branch. Group A required no production amendments, so its existing PR head remains unchanged. Neither upstream PR merging nor public release publication occurred during this fork-validation run. Later commits that only record this evidence do not alter the tested runtime source or the artifact's exact build SHA.
