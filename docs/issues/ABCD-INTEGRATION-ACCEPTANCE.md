# Additional A/B/C/D integration acceptance

## Continuation on 2026-10-11

The [continuation acceptance](CONTINUATION-ACCEPTANCE.md) supersedes the earlier pending-reboot and unpushed-branch statements below. The manual reboot is verified; current B/D follow-ups are reviewed, pushed to the fork and integrated locally, with 437 Core tests and six crossed regional/catalog WPF runs passing. E is actively in progress; F/G/H remain unimplemented. Upstream publication gates remain unchanged. Earlier source audits and receipts retain their dated boundaries.

Additional testing corrected two C/D implementation defects and B's regional mock-fixture expectation, then proved/fixed a tray-harness catalog restoration gap. B feature head is `0a2c1ac`; previously verified private composition **`6fe467b585e80fde0b8bfa47598195f51b8e9a5a`** passed build/Core, six exact portable regional/catalog runs and MSI payload/refresh checks. Hosted A/B-only `d70f30d` also passed build/package/runner checks and six exact downloaded desktop runs. These results retain their executed scope. C/D remain unpublished.

## Subsequent local main integration — 2026-10-10

At the user's instruction, all 17 pre-existing local branch tips were integrated into fork `main`, ending at source **`9e894ab133f8cbdb3b5f5ac1f411023b182e65d7`**. The four merge commits preserve the composition, separate regional-validation ancestry, historical Group A workflow and earlier scrolling-validation history. The older scrolling branch conflicted only in four documentation files; its evidence already existed in the newer records, which were retained. Every original non-main branch tip remains unchanged, and `codex/main-before-all-local-20261010` preserves pre-merge main `877a854`.

The merged desktop/native/scripts/tests/installer trees are identical to verified `6fe467b`. Private analysis/issue records also differ; the only additional tracked non-documentation file is the historical Group A workflow. It checks out `de13b6d` and is not proof for this main. C/D and all B follow-ups are now locally integrated. No branch was deleted, and no remote push/publication was performed.

**This merge and documentation audit run no application, build or tests.** The user deferred execution until after a manual reboot. Existing binaries and receipts below retain their original source metadata and must not be relabeled `9e894ab`. [All-file source audit](../analysis/audit.md#merged-main-documentation-audit--2026-10-10) records the 44-page review. After reboot, validate the current merged main through isolated managed/UI/tooling checks and appropriately source-bound packaging; any real-device testing requires its own bounded scope.

## Standalone checks and corrections

| Group | Tested head | Additional verification |
| --- | --- | --- |
| A | `de13b6d3e22a177537ed9d5977fe693a5b497fc1`, unchanged | 37 focused prerequisite tests and eight real-SDK resolution checks passed; invocation from an unrelated directory containing spaces resolved SDK 10.0.401 from the intended desktop directory |
| B | `11c60466465bc07c0d6ce519dcfc4d3a54b973f6`, before the regional-fixture follow-up | Three complete passes of 60 lifecycle tests, 180 case executions, no failures/skips/hang timeouts; no production changes in these repetitions |
| B regional fixture | `bceb44035435d871452e9f0341132b52d7f689e6`, now pushed | Four full WPF passes of 299 checks under independent English/Simplified Chinese regional and UI catalog combinations; both invalid culture-argument cases passed, 253 Release Core tests and zero-warning/error Release build; session production blobs unchanged |
| B exact catalog restoration | `0a2c1ac2f95fa51d040b4ef0beb84f1e6aae6c26`, current pushed head | Traditional Chinese desired tray-marker assertion failed after 14 preceding checks on unchanged old cleanup; English/Simplified controls passed 15. Fixed standalone runs passed 300 checks for each of four regional/catalog pairs, with Debug/Release 253 Core passes and zero-warning/error builds |
| C | `c3a1d5e4bf2879054e729b77440f97aa259155e9` | Six new decisive failures on unchanged initial `e129466` were corrected; 22 added cases, 126 focused tests and 261 full Core tests in each Debug/Release configuration passed; fresh WPF checks 208 per language |
| D | `48e7ab62dd034085a53825d17e52b6bbf3714315` | One documented-status failure on unchanged initial `4c79f22` was corrected; five added cases, 69 focused tests repeated ten times and 232 full Core tests in each Debug/Release configuration passed; fresh WPF checks 149 per language |

C's correction prevents oversized positive future-schema integers from permitting recovery/defaults replacement. D's correction recognizes documented browse terminal cancellation status 1223 rather than 995, while retaining pointers until native calls/callback frames end. Their separate [C](GROUP-C-ACCEPTANCE.md) and [D](GROUP-D-ACCEPTANCE.md) records preserve the original verification, decisive new failures, corrected source hashes and limitations. Both corrected branches contain two commits/eight intended files based on historical original stable `41190e0`; fork-only planning remains excluded.

D also executed a finite Windows DNS-SD-only probe: two browse operations, four successful resolves and cancellation with raw status 1223 observed while Cancel was running. All owners settled and final owner/buffer counters were zero. This did not call playback, mute, application-settings or credential APIs. It establishes the recorded run's behavior, not sustained allocation safety or every possible Windows ordering.

## Local composition and cross-group fixtures

Worktree: `C:\Users\marko\.codex\worktrees\verify-abcd\AirFlash`, retained local branch `codex/verify-abcd`. It combines A/B/scrolling/installer runtime with corrected C/D, upstream `d4aa068` and 19 WPF cross-group assertions. After regional-fixture source `122500d`, it incorporated UiSmoke-only catalog restoration at **`6fe467b585e80fde0b8bfa47598195f51b8e9a5a`**. C/D production blobs and all 19 assertions are unchanged. This was a separate composition during verification; it is now an ancestor of local main as recorded above, and remains distinct from the public A/B-only branch.

The cross-group fixtures use a concrete temporary SettingsStore with gated I/O, synthetic engine/audio services and the real application dispatcher. They exercise a locked automatic preflush before engine admission, changed/relocked configuration revisions, exact-encoding preservation during a delayed valid save, newer Pair/Stop overriding stale automatic admission, and deferred endpoint cleanup errors preserving the newer notice through a later real-store save. Assertions count playback/mute/autostart effects and inspect saved bytes/ownership rather than merely invoking helper methods.

| Composition check on `031510e` | Result at resume |
| --- | --- |
| Complete Core suite, Debug | 420 passed; zero failures/skips |
| Complete Core suite, Release | 420 passed; zero failures/skips |
| Python tooling suite | 86 passed in 24.64 seconds |
| Local isolated publish | Portable EXE produced with numeric test version 0.3.1 and informational `local-abcd-validation`; no release reservation |
| Published EXE SHA-256 | `478048723aeabfe36c36f4b24367a1dde54109f848990c1a109c388b2c7196ff` |
| Updated-upstream composition | Later `5d219d16a8ad64b2212c3e72779f1af5a683313e` incorporated upstream `d4aa068`; its 422-test Release pass is superseded by the final-source checks below |

Ignored evidence is in the validation worktree's `artifacts/abcd-tests/final/`, `artifacts/abcd-publish/` and `artifacts/abcd-ui/`. Initial UI output remains historical and is not proof of final corrected sources. Standalone A/B receipts are in each implementation worktree's `artifacts/abcd-extra/`; corrected C/D receipts are in `artifacts/more-testing-2026-10-09/`, with fresh UI reports copied into the fork checkout's `artifacts/abcd-extra-2026-10-09/`.

## Upstream status and remaining work

The 2026-10-10 refresh found [A PR #11](https://github.com/Ding-Kyoma/AirFlash/pull/11) and [scrolling PR #13](https://github.com/Ding-Kyoma/AirFlash/pull/13) merged. Actual **upstream** stable main is **`d4aa0686fc411de6655aac313243f0f6bf71e439`**, including later Traditional Chinese localization. Original source proofs and earlier build artifacts remain pinned to their historical SHA; they do not validate a new composition automatically.

[B PR #15](https://github.com/Ding-Kyoma/AirFlash/pull/15) is closed without merge; issue #14 is open and A/scrolling issues #10/#12 are closed. The [maintainer's regional assertion request](https://github.com/Ding-Kyoma/AirFlash/pull/15#issuecomment-6073940012) was reproduced/corrected in `bceb440`; later catalog restoration was fixed in pushed current B `0a2c1ac`. Closed PR #15 still presents `11c6046`, with no reopen/new PR action. [B acceptance](GROUP-B-ACCEPTANCE.md#current-upstream-status-on-2026-10-10) records exact proof/current results. Old run 37870953320 had failure/zero jobs, distinct from executed desktop proof.

## Historical local validation before the catalog assertion

Source `122500d117315a896fe34a9e8c541faa9fdca626` passed 422 Core tests in both configurations, zero-warning/error builds and six exact portable runs: regional/catalog pairs en-US/en-US and zh-CN/en-US passed 578 checks; en-US/zh-CN, zh-CN/zh-CN, en-US/zh-TW and zh-TW/zh-TW passed 570. Each retained 377 non-scroll checks (299 B, 59 C, 19 cross-group) plus 201 English/193 Chinese scrolling checks. The source-bound receipt verifies prior assertion multiplicities and the deliberate hidden-row replacement.

Its EXE was 77,230,525 bytes, SHA-256 `ec1d4e8869e1e1968d62ffd03873662629017cae81cbea485e1e2f5c164f1572`, with file version 0.3.1.0 and source-bound `local-abcd-validation` metadata. Both actual MSI host paths contained that exact payload; two synthetic same-version refresh stages passed, preserving input/package hashes. SDK 10.0.401/WiX 6.0.2 were used without desktop installation or reservation.

Those six runs realized 880×640/640×440, 32 page/theme/size and two frequency-label configurations each, with 12 DIP overflowing/minimum-band gutters. English minimum Equalizer/Monitor-bottom and Traditional Chinese minimum Equalizer renders fit. Reports/render sets and payload/refresh receipts remain in `artifacts/abcd-final-2026-10-10/`. The earlier `031510e`/420/`478048...` evidence above also remains historical.

These checks ran before the new final-catalog assertion. Main Traditional Chinese renders/recovery checks ran with the correct catalog, but the old tray helper subsequently restored its static catalog to Simplified Chinese. Async caller culture reporting alone did not detect that global drift. A desired marker assertion on unchanged old cleanup failed after 14 preceding Traditional Chinese tray checks while English/Simplified controls passed 15. The before-proof receipt and current [B acceptance](GROUP-B-ACCEPTANCE.md#current-upstream-status-on-2026-10-10) distinguish that verification defect from production behavior. Older passed reports do not establish exact catalog restoration.

## Verified local 6fe467b composition

| Check on local `6fe467b` | Current result |
| --- | --- |
| Debug/Release Core and solution builds | 422 Core tests passed in each configuration; zero failures/skips and zero build warnings/errors |
| Portable publish | File version 0.3.1.0; informational `0.3.1-local-abcd-validation.6fe467b585e80fde0b8bfa47598195f51b8e9a5a+6fe467b585e80fde0b8bfa47598195f51b8e9a5a` |
| Exact current EXE | 77,230,639 bytes; SHA-256 `d14cb80ad5a8ab13dbeeb3be7e4af814bf015c9ca2554b5d1f1fcd8f0d32d117`; unchanged after all six runs |
| Six regional/catalog portable runs, including exact catalog marker | All six passed; matrix below |
| Default/explicit-host packages and same-version refresh | Two actual MSI payload checks and two synthetic refresh stages passed; input/package hashes preserved, zero package-build warnings/errors |

| Actual regional culture | UI catalog | Passed checks | Non-scroll / scrolling |
| --- | --- | ---: | ---: |
| en-US | en-US | 579 | 378 / 201 |
| en-US | zh-CN | 571 | 378 / 193 |
| zh-CN | en-US | 579 | 378 / 201 |
| zh-CN | zh-CN | 571 | 378 / 193 |
| en-US | zh-TW | 571 | 378 / 193 |
| zh-TW | zh-TW | 571 | 378 / 193 |

Each run retains 300 B/non-C checks, 59 C recovery checks and all 19 cross-group assertions, totaling 378 non-scroll checks. The new final-catalog assertion occurs exactly once. Source-bound report comparisons verify prior assertion multiplicities and the deliberate hidden-active-row replacement; counts come from executed reports. Each run realizes 880×640 default/640×440 minimum, 32 page/theme/size plus two frequency-label configurations, and 12 DIP overflowing/minimum-band gutters. These are mock/routed-input geometry checks, separate from historical native pointer observations.

Both actual MSI host paths contain exact current EXE hash `d14cb80a...d32d117`, 77,230,639 bytes, File-table version 0.3.1.0 and product version 0.3.1. SDK 10.0.401/WiX 6.0.2 builds passed; read-only extraction/table queries preserved input/package hashes. Two same-version synthetic refresh stages passed without installing or executing synthetic packages.

Final receipts are in `artifacts/abcd-catalog-final-2026-10-10/`: aggregate `final-local-receipt.json`, `build-receipt.json`, `ui/receipt.json` and `packaging-receipt.json`, with source/report-bound logs, TRX, renders and payload evidence. The aggregate pins B/C/D heads, exact component Git blobs, once-only catalog marker, all 19 cross-group checks and measured gutters. Rust source is unchanged; local publish uses verified hosted `098c0a1` engine SHA-256 `7710ddcb58c4aa630f5f777e35d9ceac2c7e4e6dccf147a9c1a613ae21a3043c`. Compilation/tests were hosted, not local. C/D production/standalone results are unchanged. Their added notices retain English fallback in Traditional Chinese; current recovery/layout/catalog checks passed with it.

## Separate hosted validation boundary

Historical public source `098c0a196ba8ff6752517cb26f090890c686c78d` passed safe [fork run 38073990748](https://github.com/ThatMarko/AirFlash/actions/runs/38073990748): 86 Python, 47 native (one existing soak ignored), 255 Core, lint/icons, publish/package, payload refresh and disposable-runner installer lifecycle checks. Verified downloaded EXE SHA-256 `5cc8c388b08dde6ce636c67fae18ebe203b6a988c214bf6d7d59fb4094ac5de5` passed six regional/catalog mock runs, 500 English/492 Chinese checks, and separate native hello; binary hashes remained unchanged. Exact archive/manifest/run and desktop receipts are in the fork checkout's `artifacts/regional-verification/hosted-098c0a1/`. This source predates the catalog restoration assertion.

Current public A/B-only `codex/verify-b-regional`, **`d70f30dc32ef152c25c339f75ae09cabf29272ca`**, contains upstream and B `0a2c1ac` without C/D. Safe [fork run 38075339970](https://github.com/ThatMarko/AirFlash/actions/runs/38075339970) passed 86 Python, 47 native (one existing soak ignored), 255 Core, lint/icons, exact publish, MSI host/payload refresh, embedded hello and all seven disposable-runner installer stages. Current counts come from retained logs/TRX; earlier `098c0a1` results are not assigned to this source.

Both package/report archive SHA-256 digests were verified, respectively `5623ccbaa369499177533880cad2ee29bf56a9c7f147c27224041e6ec57b0f91` and `8de057b94180b86a7f06b0ba227b6dbd7629923825202b4610fb601137094e53`. Manifest provenance matched `ThatMarko/AirFlash`, `refs/heads/codex/verify-b-regional`, exact `d70f30d`, run 38075339970 and attempt 1 before desktop execution.

| Exact hosted/downloaded file | SHA-256 |
| --- | --- |
| AirFlash.exe, 77,202,916 bytes | `d454de31d031a339bb616d3390b748279a6e5bb374bd11dc3de4d8956c2143c6` |
| AirFlash-0.3.1.msi | `15be3dd53ebfd5e31547607ca5e9cd62301832400aef33c2caf972e560eeff6a` |
| airflash-engine.exe | `2be496034ad88bb6f82abf9122ea85623f671d6054dcfb1970d893d799506442` |

The exact downloaded EXE passed all six actual regional/catalog pairs used in the local matrix: 501 checks for either English-catalog pair and 493 for each Simplified/Traditional Chinese pair. Each includes 300 non-scroll checks, once-only final catalog restoration and 201/193 scrolling checks; all 32+2 realized layouts retained 12 DIP gutters at 880×640/640×440. Separate correlated JSONL v1 native hello passed without a playback/pairing worker. All three binaries, manifest and checksum file were unchanged after qualification.

The source/run/archive/manifest/report/binary-bound `artifacts/regional-verification/hosted-d70f30d/final-desktop-receipt.json` links hosted and desktop receipts plus native hello evidence. This hosted engine hash differs from local composition's verified `098c0a1` input `7710ddcb...a3043c`; native binary parity is not claimed. Public A/B-only validation excludes C/D, whose runtime acceptance is the separate local proof above. No desktop MSI installation or real accessory/audio/credential/configuration/Windows-region mutation occurred.

Prior local-composition and hosted/downloaded verification is complete within those recorded scopes; new-main execution is deferred as stated above. Keep C/D unpublished until Task 3 activation and review clean standalone diffs against actual upstream before publication. Mocks, native hello and the bounded DNS probe do not qualify real accessories, physical mute restoration, acoustic output, crash durability, physical multi-monitor/DPI behavior or a hardware soak. No Windows regional setting change, desktop MSI installation, release or upstream merge was performed.
