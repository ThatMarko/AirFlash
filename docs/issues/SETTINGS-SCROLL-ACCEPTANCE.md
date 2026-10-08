# Settings scrolling acceptance

Verified locally on 2026-10-07 after the user reported crowded Settings scrollbars. This is a separate UI fix, outside the existing Groups A–H. The user subsequently authorized merging it into the A+B fork main and building it with GitHub Actions. The feature merged at `96c7b5593a5a7d10b72980b814bf0ae7ddc7908d`; original upstream main remains unchanged at `41190e0`.

## Source and behavior

The Settings layout and theme are unchanged between audited original `41190e0` and the A+B fork main `1a517c1`. The reported layout therefore predates those implementations. Original [SettingsWindow.xaml](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Ui/SettingsWindow.xaml#L102) nests a horizontal Equalizer viewer with vertical scrolling disabled; the shared page viewer has only six pixels of right padding at line 389.

The original-code fixture reproduced 26 vertical-clearance failures (content extends six DIP into the scrollbar's bounds) and eight failures to scroll down over Equalizer labels/sliders. A subsequent check of the actual `16 kHz` label at minimum width found it extended 12 DIP into the horizontal scrollbar. This horizontal check ran before adding inner padding, with the vertical fix already applied. The earlier diagnostic that selected a scrollbar arrow glyph was discarded.

The final change reserves 24 DIP on the right of the page viewer and below the frequency bands, giving 12 DIP of measured clearance from each realized scrollbar. A preview wheel handler forwards a fresh bubbling wheel event to the page viewer once, preserving WPF's configured wheel distance. It does not change EQ processing, persistence, focus, horizontal scrolling or session ownership.

## Branch and build boundaries

- Implementation: `codex/fix-settings-scroll`, code commit `7d21728bd2d2e12d986089be89ac0ba0765bd3fd`, directly based on verified original `41190e0d13a63a714c08dffe73ababca1804875c`. Commit `94f6f5c57e3bd237ca027f801992fc314a4fb761` adds directly runnable PowerShell documentation examples; published head `54cd18e028af9abc4d0b85af8197209a978c4603` clarifies requested versus realized verification window dimensions. Both later commits change documentation only.
- Five-file implementation diff: Settings XAML/code-behind, the existing UI harness hookup, `UiScrolling.cs`, and public Settings verification documentation. Complete diff and outgoing commit exclude fork analysis/issues/planning.
- Earlier local integration: `codex/verify-settings-scroll`, source merge `b78753cd260544019012cb5314d3204f72440a1e`, combines this fix with the unchanged A+B fork main. Independent review compared all 19 A/B paths: 17 are identical; the other two contain only the approved Settings handler/harness additions. Group B's Pair wrapper and lifecycle checks remain present.
- The feature merges into the user's fork main separately from the fork-only documentation. The A and B feature branches retain their original heads. This validation performed no release reservation, official release or local MSI installation. Subsequent upstream issue/PR publication is recorded below.

The earlier integration-branch updates changed documentation only; the locally tested executable below was built from source merge `b78753c`.

## Prior local validation

| Check | Result |
| --- | --- |
| Independent production, fixture and combined-source reviews | No actionable findings |
| Focused eight-page layout/wheel fixture, both themes, 880×640 and 640×440 | English 201 passed; Chinese 193 passed |
| Vertical and minimum-width Equalizer horizontal clearance | 12 DIP in both themes/languages |
| Final content and highest frequency band | Reachable; native wheel distance applied once; curve/horizontal offset preserved |
| Combined Core suite | 218 passed, zero failures/skips |
| Locked restore, Debug build and combined Release publish | Passed with SDK 10.0.401 |
| Exact published EXE full mock UI suite | English 474 passed; Chinese 466 passed |
| Published EXE pointer wheel and vertical thumb dragging | Wheel moved page by 48 DIP over an enabled slider; lower end reached 246.4 DIP, matching its extent; upper end restored the first card visually |
| Checks after the bounded pointer session | All 201 passed; draft/curve/store/start counts preserved; test process exited |

The unofficial self-contained EXE has product version `0.3.1-scroll-validation.b78753c+b78753cd260544019012cb5314d3204f72440a1e` and SHA-256 `36912295c3e91f7a3914e5e0a32481444d48ce6cd7787497371ed7dab59d32c4`. Numeric 0.3.1 is only an unreserved validation version. The unchanged native executable was reused from the verified A+B GitHub build, SHA-256 `a43615e24cbe29b23ab5cf24ebad3ac5ced96d1590c5851184abe99e2eb99f98`; it was not recompiled locally.

All services and devices in these checks are simulated. No real config, credentials, discovery, endpoint control or accessory playback was used. Native pointer checks cover this desktop session; other monitor scaling and touchpads remain unverified. [Settings diagnostics](../SETTINGS-DIAGNOSTICS.md) describes the reproducible commands. Local ignored build receipts contain exact hashes, reports, Core TRX and synthetic renders.

## GitHub build and exact downloaded binary

Fork main contains A+B and the scrolling implementation. Source `d30c043b51765221e722d975ba06492160962040` passed [CI](https://github.com/ThatMarko/AirFlash/actions/runs/37696850901) and [fork build/package validation](https://github.com/ThatMarko/AirFlash/actions/runs/37696850934). The hosted checks passed 86 Python tests, native checks (47 passed, one intentionally ignored soak), 218 Core tests, both SDK installer paths, runtime-only rejection, embedded native hello and disposable-runner installer lifecycle checks. The downloaded ZIP digest matched GitHub's artifact metadata; all three binary hashes and source/version metadata matched the manifest.

The exact downloaded EXE passed 498 English and 490 Chinese mock UI checks, including all 273 existing non-scroll checks per language, 32 unique page/theme/size configurations, and two minimum-width `16 kHz` label measurements with 12 DIP clearance. The fixture requests 880×640 for its default window, but this desktop realized 660×500; the additional 24 checks reflect three more overflowing pages in both themes. They do not indicate changed application source or missing coverage. Native hello also passed, and verification left the downloaded files unchanged.

During that first downloaded-build run, the desktop reported a 640×432 work area and produced blank UI renders. Native pointer/visual checks of that `d30c043` binary were not completed; the prior pointer checks above apply only to the earlier local binary. No real accessory test or local MSI installation was performed in that stage. The later final-build pointer results are recorded below.

Local packaging uncovered a separate, pre-existing installer cache defect: a successful same-version package could still contain an earlier executable. The clean-checkout GitHub MSI contains the correct EXE; both local MSIs from the cached checkout contained old A+B bytes and are rejected. The separate original-base [installer fix and regression check](INSTALLER-REFRESH-ACCEPTANCE.md) is now integrated and verified below.

## Final A+B+scrolling build

Exact source `1303a09127760912c1cdf6a47bdb14e45904fd62` passed [CI](https://github.com/ThatMarko/AirFlash/actions/runs/37711905828) and [fork integration validation](https://github.com/ThatMarko/AirFlash/actions/runs/37711905836). The hosted run repeated the full 86 Python / 47 native / 218 Core test sets, lint and icons, both installer SDK paths, runtime-only rejection, the new repeated-payload regression, embedded native hello, and disposable-runner installation/repair/upgrade/uninstall with hashes and user-data preservation. One existing native soak remains intentionally ignored.

Downloaded artifact ID `11522064329` has verified archive SHA-256 `88d73a4feefc95e0e6393130f7277d85b7e5fae77dc3c82bc78b8cb27e26f127`. Manifest source/run/repository/version, file sizes and all binary checksums matched before and after desktop verification. Informational version is `0.3.1-fork-validation.1303a09127760912c1cdf6a47bdb14e45904fd62+1303a09127760912c1cdf6a47bdb14e45904fd62`; the numeric version remains an unreserved test value.

| Final binary | SHA-256 |
| --- | --- |
| AirFlash.exe | `89982600d059a39875acfd5351b77796cc0023959d6c84f1112d6b18899e4bd7` |
| AirFlash-0.3.1.msi | `e3a22a000e5aa3117604064245320b66ac742ce28843bd74346be2fd34f2896c` |
| airflash-engine.exe | `8836ed98936f075f712810d62945b63c6ed399e1e609f55ab2c1f925532a1cd0` |

The exact final downloaded EXE passed 498 English and 490 Chinese mock UI checks. Each language retained all 273 A+B checks and covered 32 distinct page/theme/size configurations. Every measured overflowing page gutter and both minimum-width frequency-label gutters were 12 DIP, with zero scrolling failures. Native hello passed without creating a playback/pairing worker. The desktop still realized 660×500 for the requested default window; minimum cases realized 640×440.

Local managed publish, with SDK 10.0.401, locked dependencies and the exact newly compiled GitHub native output, produced a byte-for-byte identical EXE. Full native compilation/tests occurred on GitHub. Both local installer paths rebuilt successfully with zero warnings/errors. Read-only extraction of those two MSIs and the downloaded MSI found the exact final EXE checksum in all three, matching File-table version `0.3.1.0` and size 77,187,230 bytes. Inspection did not alter packages; later packaging preserved the first local MSI and source EXE. Complete MSI hashes may differ because each package build creates its own identity; payload checksums are the acceptance criterion.

The first final-build UI run produced blank images while desktop capture was unavailable. A fresh-window capture retry returned `IGraphicsCaptureItemInterop.CreateForMonitor failed: Could not capture the given monitor. (0x80070057)`; no pointer input was sent after that failure. The user subsequently reported that manual testing mostly looked good and made the desktop available. The exact same downloaded binary was then checked with native mouse input as recorded below. Agent verification used mock services throughout, without restarting the user's normal app, accessing real settings/credentials/accessories, or installing an MSI locally.

Evidence is retained under ignored `artifacts/fork-integration/scroll-github-1303a09/`: archive/API metadata, binaries/manifest, hosted logs/TRX/regression reports, desktop reports/receipt, local republish comparison, and `fixed-installer-payload-receipt.json`. The original failing installer and corrected original-base/integrated receipts are copied there too. Artifacts have 14-day GitHub retention; local copies preserve this build. Later evidence-only documentation commits leave the tested runtime and build configuration unchanged.

## Final downloaded build: native pointer follow-up

On 2026-10-07, capture succeeded for an isolated Settings window opened by the exact final downloaded EXE (`89982600…e4bd7`). Physical checks used the current monitor, light theme and English, with simulated discovery/audio/engine/settings services. Three accepted bounded sessions finished with all 201 focused checks passing, including unchanged edited curve/draft, unchanged store and no playback start. The EXE checksum remained unchanged.

| Native input / visual check | Observed result |
| --- | --- |
| Wheel down/up over enabled Equalizer slider | Page moved 0 → 48 → 0 DIP; first gain remained +7.5 dB |
| Wheel down/up over disabled Equalizer slider | Same 0 → 48 → 0 DIP movement; gain unchanged; Enabled restored before final checks |
| Monitor vertical thumb down/up | 0 → 554 → 0 DIP; lower value matched measured scrollable height; final content and first card visible at their respective ends |
| Horizontal band thumb at minimum width | 0 → 224 → 0 DIP; actual `16 kHz` label readable and clear of the bar; `31 Hz` restored at the left end |
| Default-size visual pass through all eight Settings pages | Cards, controls and visible scrollbars spaced correctly; General, Network and About fit without unnecessary vertical scrolling |

Native width resizing reached logical 640×640; automated checks separately covered the exact 640×440 minimum layout in both themes. Bottom/corner resize attempts did not establish a successful native height resize and are not counted as such. Source review found no fixed-height resize lock, and no additional scrolling defect was found. Other monitor scales, touchpads and accessory/audio reliability remain outside this mock UI validation.

The initial mouse trial intentionally disabled EQ but expired before the operator restored its checkbox, so the final draft-preservation assertion correctly failed. That trial is retained as a procedural failure and excluded from acceptance. Repeating the disabled-band check with restoration passed. The accepted session directories are `pointer-c16b824807ff4be683bfcce32c24505e` (disabled wheel/Monitor), `pointer-65ae29d8d76c4c99970dcbc8f5cd5d9c` (horizontal thumb), and `pointer-029eb3f43d66440b9d922d790c31a1ac` (enabled wheel/all-page visual pass), under the final artifact's ignored `desktop/` folder. `native-pointer-receipt.json` records the observations and scope. No production implementation changed during this follow-up.

## Upstream Task 3 publication

On 2026-10-07, after explicit user authorization, published [issue #12](https://github.com/Ding-Kyoma/AirFlash/issues/12) and ready-for-review [PR #13](https://github.com/Ding-Kyoma/AirFlash/pull/13). The PR targets original stable `41190e0d13a63a714c08dffe73ababca1804875c` from `ThatMarko:codex/fix-settings-scroll` at `54cd18e028af9abc4d0b85af8197209a978c4603`. Its five-file diff and all three outgoing commits exclude fork analysis/issues/planning documents. `Fixes #12`, the published bodies and branch identities were read back and verified; the PR is attached to the Codex task.

At publication, [upstream CI run 37717295723](https://github.com/Ding-Kyoma/AirFlash/actions/runs/37717295723) reported `action_required` and explicitly awaited maintainer approval, with no jobs started. No review had been submitted. The next upstream action is maintainer approval of the fork workflow and review; this status is separate from the completed local/fork validation above. No upstream merge occurred.
