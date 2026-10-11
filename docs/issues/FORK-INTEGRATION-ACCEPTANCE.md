# Combined fork integration acceptance

Initial verification completed on 2026-10-07 for the authorized Group A + Group B fork integration. Historical acceptance below covers initial Group B head `342aeb7`; the 2026-10-08 follow-up is separate. This is unofficial validation of `ThatMarko/AirFlash`; original upstream main was `41190e0d13a63a714c08dffe73ababca1804875c` during those checks. Current upstream status is in the final addendum. A fork merge does not imply upstream acceptance.

## Source and branch boundaries

- Group A: `codex/fix-build-preflight`, unchanged head `de13b6d3e22a177537ed9d5977fe693a5b497fc1`; upstream [PR #11](https://github.com/Ding-Kyoma/AirFlash/pull/11) was open during this validation and has since merged.
- Initial Group B: `codex/fix-session-lifecycle`, head `342aeb77cb85ff6f3f7f850161b3db79d8637945`, with its earlier Core contract commit `4662796`. Its [separate checklist](GROUP-B-ACCEPTANCE.md) records both this initial acceptance and the newer follow-up.
- At this initial integration, the Group B feature branch was pushed to the fork and its remote head verified as `342aeb77cb85ff6f3f7f850161b3db79d8637945`. Group A's existing remote branch remained `de13b6d`; no PR branch rewrite was required.
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

Merged fork main was pushed at **`b2105c9c158769ca06f8dc4e39d0e525fb497402`**. Its [normal CI](https://github.com/ThatMarko/AirFlash/actions/runs/37677853767) and [fork package validation](https://github.com/ThatMarko/AirFlash/actions/runs/37677853674) both completed successfully on that exact source. At that point both feature heads were unchanged, and all 19 integrated feature files matched their independently reviewed branch blobs exactly.

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

Subsequent user-authorized fork validation added the separate Settings scrolling and installer payload-refresh fixes. Their [final A+B+scrolling build acceptance](SETTINGS-SCROLL-ACCEPTANCE.md#final-abscrolling-build) records the later `1303a09` artifact and exact downloaded checks; the A/B-only build evidence above remains historical. Those artifacts contain the initial B head, not the new follow-up below.

No real configuration, credential store, receiver, capture or endpoint mutation is part of desktop validation. The mock UI does use isolated real tray/window registrations. Native localhost tests and mock tests do not establish hardware/acoustic reliability. MSI installation belongs exclusively to the disposable hosted runner.

After this validation, the user authorized Task 3 publication: scrolling [issue #12](https://github.com/Ding-Kyoma/AirFlash/issues/12) / [PR #13](https://github.com/Ding-Kyoma/AirFlash/pull/13) first, then Group B [issue #14](https://github.com/Ding-Kyoma/AirFlash/issues/14) / [PR #15](https://github.com/Ding-Kyoma/AirFlash/pull/15). Their separate acceptance records contain the exact heads, original-base boundaries and upstream CI runs, both awaiting maintainer approval with no jobs started at publication. Group A required no production amendments, so its existing PR head remains unchanged. No upstream PR merging or public release publication occurred. Later commits that only record this evidence do not alter the tested runtime source or the artifact's exact build SHA.

## Follow-up validation on 2026-10-08

A deeper audit of existing issues/PRs found additional lifecycle gaps in Group B's published `342aeb7` runtime. [Follow-up acceptance](GROUP-B-ACCEPTANCE.md#follow-up-audit-on-2026-10-08) records the reproduced endpoint observations and retarget/settings cleanup failures, fixes, source hashes and standalone results. Feature head at that audit was `11c60466465bc07c0d6ce519dcfc4d3a54b973f6`: 60 focused lifecycle tests, 253 full Core tests in each of Debug/Release, and 298 WPF checks in each language passed; builds had zero warnings/errors. Group A and scrolling required no source amendments in their fresh audits.

| Fresh existing-PR audit | Source head and results |
| --- | --- |
| Group A, PR #11 | Unchanged `de13b6d`: 37 focused prerequisite tests and all 86 Python tests passed; Ruff lint and the new test's formatting passed |
| Scrolling, PR #13 | Unchanged `54cd18e`: 163 Core tests, 350 English and 342 Chinese full WPF checks passed; Debug/Release builds had zero warnings/errors |
| Scrolling layout coverage | 32 page/theme/size configurations per language at actual 880×640 and 640×440; every overflowing page gutter and both minimum-width band-label gutters measured 12 DIP |
| Group B, PR #15 | Follow-up `11c6046`: 60 focused lifecycle tests, 253 full Core tests in each Debug/Release configuration, 298 WPF checks in each language; zero build warnings/errors |

The unchanged-feature receipts are retained in each worktree's ignored artifacts: Group A `artifacts/pr-audit-group-a-receipt.json` and scrolling `artifacts/pr-audit-2026-10-08/scroll-audit-receipt.json`. These fresh scrolling checks used simulated routed input; the earlier native pointer observations remain bound to the specific historical binary and desktop session recorded in the separate scrolling acceptance.

The separate validation branch `codex/verify-pr-session-audit` starts from fork `origin/main` at `78a61f5e91e331ef695e74e3cf99320eb875d09a` and merges only the Group B follow-up, producing source **`bfed430b0578f14600004b87c70d3c7b379d0036`**. It contains the existing A+B+scrolling/installer runtime, excludes the local unpublished C/D implementations, and has not merged this follow-up into fork `main` or upstream `main`.

[Fork integration validation run 37870969984](https://github.com/ThatMarko/AirFlash/actions/runs/37870969984) completed successfully on that exact temporary-branch source using the existing read-only validation workflow. All 86 Python tests, 47 native tests and 253 Core tests passed, with zero Python/managed skips and one intentionally ignored existing native soak. Lint/icons, Release build/publish, default/explicit-host MSI generation, runtime-only early rejection, repeated installer payload refresh, embedded native hello, and disposable-runner install/repair/upgrade/uninstall checks passed. The hosted Core TRX and count/source receipt are retained under ignored `artifacts/pr-audit-2026-10-08/`.

Artifact `fork-integration-bfed430b0578f14600004b87c70d3c7b379d0036-37870969984`, ID **11590072723**, was downloaded and its archive SHA-256 matched GitHub's API digest: `e34e05b97479bfdcbbad1c38d4e9c1bb5d206a1082e1d7bec398cbffefd1ca37`. The manifest binds repository `ThatMarko/AirFlash`, ref `refs/heads/codex/verify-pr-session-audit`, source `bfed430b0578f14600004b87c70d3c7b379d0036`, and run `37870969984`; its binary checksums were verified before desktop launch. SDK is 10.0.401. The informational version is `0.3.1-fork-validation.bfed430b0578f14600004b87c70d3c7b379d0036+bfed430b0578f14600004b87c70d3c7b379d0036`; numeric 0.3.1 remains an unreserved test value.

| Follow-up downloaded binary | SHA-256 |
| --- | --- |
| AirFlash.exe | `c62cadf2210f81129a460cfff96bea2334abeac7c7690354f797c67883c16295` |
| AirFlash-0.3.1.msi | `bd6fb10e0fc70c8ca2aae5002a0862549d4953efb818c06e5f9a625aae6eb39f` |
| airflash-engine.exe | `8428019fb97a780ea0fb1560813b8a5ca0c605478e9e8904e66ee6d27e5fdcd6` |

The **exact downloaded portable EXE passed 499 English and 491 Chinese mock UI checks on this desktop**. Each language retained all 273 prior non-scroll checks and passed 25 new Group B checks, for 298 non-scroll checks. The totals are derived from the current reports and measured layouts, rather than incrementing historical artifact totals with different realized window sizes.

| Exact downloaded desktop check | Result |
| --- | --- |
| Realized Settings dimensions | 880×640 default and 640×440 minimum in both languages |
| Unique page/theme/size configurations | 32 per language; 26 English and 24 Chinese configurations required vertical scrolling |
| Scrolling checks | 201 English and 193 Chinese; every overflowing page gutter and both minimum-width frequency-label gutters measured 12 DIP |
| Minimum-size visual inspection | English dark Equalizer top and Monitor bottom renders fit their content and scrollbar gutters |
| Separate downloaded native hello | Correlated JSONL v1 response passed; no playback/pairing worker created |
| Downloaded binary checksums | All three verified before and after desktop checks; unchanged |

Binaries, manifest and archive receipts are retained under ignored `artifacts/fork-integration/scroll-github-bfed430/`. Final desktop reports, the source/blob/manifest-bound `receipt.json` and native hello observations are under `artifacts/pr-audit-2026-10-08/downloaded-desktop/`. These checks use synthetic services and routed input; no new native pointer or real accessory qualification was performed. They touched no real config, credentials, receiver, capture or endpoint and performed no local MSI installation or normal engine-cache extraction. No release version was reserved or published. The earlier accepted artifacts above remain historical and are not proof for this new follow-up.

The follow-up push updated upstream PR #15 to `11c6046`. At the 2026-10-08 audit, [upstream run 37870953320](https://github.com/Ding-Kyoma/AirFlash/actions/runs/37870953320) reported `action_required` with zero jobs. Group A's [run 37660774239](https://github.com/Ding-Kyoma/AirFlash/actions/runs/37660774239) and scrolling's [run 37717295723](https://github.com/Ding-Kyoma/AirFlash/actions/runs/37717295723) were at the same gate then. Approval was separate from fork validation; no gate was bypassed or upstream PR merged by this work. At that dated refresh no new reviews, threads or comments appeared. Group A's historical Copilot review recorded a quota failure and contains no code finding.

## Current upstream status on 2026-10-10

[Group A PR #11](https://github.com/Ding-Kyoma/AirFlash/pull/11) and [scrolling PR #13](https://github.com/Ding-Kyoma/AirFlash/pull/13) are now merged; their issues #10/#12 are closed. Upstream stable main is `d4aa0686fc411de6655aac313243f0f6bf71e439`, including subsequent Traditional Chinese localization. Historical source hashes, package checksums and counts above remain unchanged; they are not proof of a later composition.

[B PR #15](https://github.com/Ding-Kyoma/AirFlash/pull/15) is closed without merge and issue #14 remains open. The maintainer's [regional-fixture diagnostic](https://github.com/Ding-Kyoma/AirFlash/pull/15#issuecomment-6073940012) was reproduced/corrected in `bceb440`; current pushed B `0a2c1ac` additionally restores the exact tray-verification catalog. Four current 300-check standalone regional/catalog runs and 253 Core tests per configuration passed, with production session blobs unchanged. Closed PR #15 still presents `11c6046`, without reopen. Old run 37870953320's failure/zero jobs supplies no executed upstream log.

[A/B/C/D acceptance](ABCD-INTEGRATION-ACCEPTANCE.md) preserves historical local `122500d`: 422 Core per configuration, six exact 578/570-check portable passes and actual MSI payload/refresh checks. Historical A/B-only source `098c0a196ba8ff6752517cb26f090890c686c78d` passed safe [fork run 38073990748](https://github.com/ThatMarko/AirFlash/actions/runs/38073990748), including 86 Python, 47 native (one soak ignored), 255 Core and all package/runner-installer checks. Its verified downloaded EXE passed six regional/catalog mock runs, 500 English/492 Chinese checks, plus native hello. Archive/manifest, run, binary and desktop receipts are retained in `artifacts/regional-verification/hosted-098c0a1/`.

A new desired marker assertion then reproduced tray-verification catalog drift on unchanged `098c0a1` cleanup: Traditional Chinese failed after 14 preceding checks while English/Simplified controls passed 15. Earlier full-report culture labels/renders did not check the final static catalog. The UiSmoke-only fix in `0a2c1ac` preserves exact language restoration and adds that assertion; production localization and session behavior are unchanged. Older passed suites remain valid for their executed checks, not this newly proved restoration gap.

Current private local `6fe467b585e80fde0b8bfa47598195f51b8e9a5a` passed 422 Core per configuration, zero-warning/error builds, six exact 579/571-check portable runs and actual MSI payload/refresh checks with the catalog assertion. Its exact EXE is 77,230,639 bytes, SHA-256 `d14cb80ad5a8ab13dbeeb3be7e4af814bf015c9ca2554b5d1f1fcd8f0d32d117`; final receipts are in the implementation worktree's `artifacts/abcd-catalog-final-2026-10-10/`. Local publish reused verified unchanged-source hosted native SHA-256 `7710ddcb...a3043c`; native compilation/tests were hosted, not local.

Current public A/B-only **`d70f30dc32ef152c25c339f75ae09cabf29272ca`** contains the fix without C/D. Safe [run 38075339970](https://github.com/ThatMarko/AirFlash/actions/runs/38075339970) passed 86 Python, 47 native (one existing soak ignored), 255 Core, lint/icons, publish/package/refresh and all seven disposable-runner installer stages. Package/report archive digests and repository/ref/source/run/attempt-1 manifest provenance were verified before execution.

Exact hosted EXE SHA-256 `d454de31d031a339bb616d3390b748279a6e5bb374bd11dc3de4d8956c2143c6`, 77,202,916 bytes, passed all six actual regional/catalog pairs: 501 English-catalog/493 Chinese-catalog checks, with 300 non-scroll, once-only catalog restoration and 201/193 scrolling checks. All 32+2 layouts retained 12 DIP gutters; correlated JSONL v1 native hello passed without a playback/pairing worker. All three binary hashes, manifest and checksum file were unchanged after desktop qualification.

Final source/run/archive/manifest/report/binary evidence is `artifacts/regional-verification/hosted-d70f30d/final-desktop-receipt.json`, linking hosted verification, desktop and hello receipts. Hosted native SHA-256 `2be496034ad88bb6f82abf9122ea85623f671d6054dcfb1970d893d799506442` differs from local composition's verified `098c0a1` input `7710ddcb...`; native binary parity is not claimed. [A/B/C/D acceptance](ABCD-INTEGRATION-ACCEPTANCE.md#separate-hosted-validation-boundary) records both archive digests/all file hashes. C/D qualification is separate local evidence; they remain unpublished. No real accessory/configuration/audio/credential/Windows-region mutation, desktop MSI installation, new issue/PR, reopening, release or upstream merge was performed.

Existing PR #15's description was then updated with the corrected branch/evidence and exact readback verified. It remains CLOSED at frozen `11c6046`, targeting `main`, with unchanged title and `Fixes #14`. [B acceptance](GROUP-B-ACCEPTANCE.md#current-upstream-status-on-2026-10-10) records prepared-body/readback paths and the distinction between current pushed code and the closed PR snapshot.
