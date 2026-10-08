# Settings scrolling acceptance

Verified locally on 2026-10-07 after the user reported crowded Settings scrollbars. This is a separate UI fix, outside the existing Groups A–H. The user subsequently authorized merging it into the A+B fork main and building it with GitHub Actions. The feature merged at `96c7b5593a5a7d10b72980b814bf0ae7ddc7908d`; original upstream main remains unchanged at `41190e0`.

## Source and behavior

The Settings layout and theme are unchanged between audited original `41190e0` and the A+B fork main `1a517c1`. The reported layout therefore predates those implementations. Original [SettingsWindow.xaml](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Ui/SettingsWindow.xaml#L102) nests a horizontal Equalizer viewer with vertical scrolling disabled; the shared page viewer has only six pixels of right padding at line 389.

The original-code fixture reproduced 26 vertical-clearance failures (content extends six DIP into the scrollbar's bounds) and eight failures to scroll down over Equalizer labels/sliders. A subsequent check of the actual `16 kHz` label at minimum width found it extended 12 DIP into the horizontal scrollbar. This horizontal check ran before adding inner padding, with the vertical fix already applied. The earlier diagnostic that selected a scrollbar arrow glyph was discarded.

The final change reserves 24 DIP on the right of the page viewer and below the frequency bands, giving 12 DIP of measured clearance from each realized scrollbar. A preview wheel handler forwards a fresh bubbling wheel event to the page viewer once, preserving WPF's configured wheel distance. It does not change EQ processing, persistence, focus, horizontal scrolling or session ownership.

## Branch and build boundaries

- Implementation: `codex/fix-settings-scroll`, code commit `7d21728bd2d2e12d986089be89ac0ba0765bd3fd`, directly based on verified original `41190e0d13a63a714c08dffe73ababca1804875c`. Head `94f6f5c57e3bd237ca027f801992fc314a4fb761` adds only directly runnable PowerShell documentation examples.
- Five-file implementation diff: Settings XAML/code-behind, the existing UI harness hookup, `UiScrolling.cs`, and public Settings verification documentation. Complete diff and outgoing commit exclude fork analysis/issues/planning.
- Earlier local integration: `codex/verify-settings-scroll`, source merge `b78753cd260544019012cb5314d3204f72440a1e`, combines this fix with the unchanged A+B fork main. Independent review compared all 19 A/B paths: 17 are identical; the other two contain only the approved Settings handler/harness additions. Group B's Pair wrapper and lifecycle checks remain present.
- The feature now merges into the user's fork main separately from the fork-only documentation. The A and B feature branches retain their original heads. No upstream issue/PR, release reservation, official release or local MSI installation is part of this validation.

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

The desktop currently reports a 640×432 work area and produces blank UI renders. Native pointer/visual checks of this downloaded build remain unverified; the prior pointer checks above apply only to the earlier local binary. No real accessory test or local MSI installation was performed.

Local packaging uncovered a separate, pre-existing installer cache defect: a successful same-version package could still contain an earlier executable. The clean-checkout GitHub MSI contains the correct EXE; both local MSIs from the cached checkout contained old A+B bytes and are rejected. A separate original-base installer fix and repeated-payload regression check are being integrated before the final combined build. Final-build results will be recorded here after downloading and verifying that new artifact.
