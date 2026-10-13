# Settings scrolling acceptance

## Current Main and upstream integration — 2026-10-12

Fork Main `8989b3609dd1294f4d1d6eb1502f345cc22bc382` contains `codex/fix-settings-scroll` head `54cd18e028af9abc4d0b85af8197209a978c4603`; [PR #13](https://github.com/Ding-Kyoma/AirFlash/pull/13) is merged and [issue #12](https://github.com/Ding-Kyoma/AirFlash/issues/12) closed. Original stable `cf420d97700463c56adc31408510e8336e99e51c` already includes it. Current [XAML](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Ui/SettingsWindow.xaml), [handler](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Ui/SettingsWindow.xaml.cs) and [scroll fixture](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Verification/UiScrolling.cs) retain the fix.

This is a separate UI follow-up outside 17 reports. Final Main packaged UI/DPI qualification in [continuation acceptance](CONTINUATION-ACCEPTANCE.md) includes scrolling and expanded Monitor/recording content. Physical pointer observations below apply only to their historical binary/session, not current manual qualification.

## Source and behavior

Original `41190e0` had only 6 DIP page right padding and disabled inner vertical scrolling over Equalizer bands. Original-code fixture reproduced 26 vertical clearance failures and 8 wheel failures; actual minimum-width 16 kHz label overlapped horizontal scrollbar by 12 DIP before inner padding. A discarded arrow-glyph measurement was not accepted.

Fix reserves 24 DIP page-right and band-bottom padding. Historical measured gutter is 12 DIP; fixture requires at least 8 DIP with 0.5 DIP tolerance rather than promising 12 on every display. Preview handler marks original wheel handled and forwards one fresh bubbling event to PageScroll preserving device/delta/timestamp and WPF wheel distance. EQ processing/persistence/horizontal offset/focus/session behavior remain unchanged.

## Branch and build boundaries

Implementation `7d21728` based on original `41190e0`; later `94f6f5c`/`54cd18e` adjust public verification documentation only. Five-file feature diff/every outgoing commit exclude private analysis/issues/planning. Initial combined source `b78753c` retained all A/B behavior; 17 of 19 A/B blobs were identical, 2 differed only for intended handler/harness additions.

## Prior local validation

Historical `b78753cd260544019012cb5314d3204f72440a1e`: 201 English/193 Chinese focused checks; 218 Core; warning-free builds/publish; SDK 10.0.401. Exact EXE `36912295c3e91f7a3914e5e0a32481444d48ce6cd7787497371ed7dab59d32c4` passed 474 English/466 Chinese full mock checks. Native output was hosted, not recompiled locally.

Eight pages×two themes×default/minimum, minimum frequency labels, wheel distance/direction, curve/draft/store/playback preservation were asserted. A bounded actual-pointer session moved page 48 DIP per wheel, reached end 246.4 DIP and restored first card; all 201 final assertions passed. These use fake services and isolated windows, not accessories.

## GitHub build and exact downloaded binary

Historical `d30c043` passed [CI 37696850901](https://github.com/ThatMarko/AirFlash/actions/runs/37696850901) and [package 37696850934](https://github.com/ThatMarko/AirFlash/actions/runs/37696850934): 86 Python/47 native/218 Core/default+explicit installer/embedded hello/disposable lifecycle; one localhost soak ignored.

Exact downloaded EXE passed 498 English/490 Chinese because requested 880×640 default realized 660×500 and additional pages overflowed. Minimum 640×440, 32 configurations/two frequency labels, 12 DIP measured gutters. Blank renders/capture failure prevented pointer qualification of that binary. Stale local MSI payload discovery led to the separate [installer repair](INSTALLER-REFRESH-ACCEPTANCE.md).

## Final A+B+scrolling build

Exact historical `1303a09127760912c1cdf6a47bdb14e45904fd62` passed [CI 37711905828](https://github.com/ThatMarko/AirFlash/actions/runs/37711905828) and [integration 37711905836](https://github.com/ThatMarko/AirFlash/actions/runs/37711905836): 86 Python/47 native/218 Core/lint/icons/static CRT/build/publish/installer-host checks/runtime-only rejection/repeated payload/hello/disposable MSI. One soak ignored.

Artifact 11522064329 ZIP SHA-256 `88d73a4feefc95e0e6393130f7277d85b7e5fae77dc3c82bc78b8cb27e26f127`; repository/ref/source/run/metadata/all payload checksums verified before/after.

| Exact historical file | SHA-256 |
| --- | --- |
| EXE | `89982600d059a39875acfd5351b77796cc0023959d6c84f1112d6b18899e4bd7` |
| MSI | `e3a22a000e5aa3117604064245320b66ac742ce28843bd74346be2fd34f2896c` |
| Engine | `8836ed98936f075f712810d62945b63c6ed399e1e609f55ab2c1f925532a1cd0` |

Exact EXE passed 498 English/490 Chinese, 273 prior non-scroll each/32 layouts/default 660×500/minimum 640×440 / 12 DIP measured gutters. Local republish reused verified newly hosted engine and produced identical EXE; default/explicit and downloaded MSI read-only extraction matched 77,187,230-byte 0.3.1.0 payload. Full MSI hashes may differ across build identities; payload bytes are the criterion. No local MSI installation.

Local evidence: `artifacts/fork-integration/scroll-github-1303a09/`, archive/manifest/logs/TRX/UI/republish/payload receipts. Later documentation-only commits do not change that tested source. An initial blank-render capture retry failed `0x80070057` and sent no pointer input; later user-available desktop enabled the follow-up below.

## Final downloaded build: native pointer follow-up

October 7 exact `89982600…e4bd7`, English/light theme/current monitor, synthetic services: three accepted bounded pointer sessions passed 201 checks each with unchanged curve/draft/store/start count and EXE hash.

| Native input | Observed historical result |
| --- | --- |
| Enabled/disabled slider wheel |0→48→0 DIP; gain+7.5 dB unchanged; enabled state restored |
| Monitor vertical thumb |0→554→0 DIP; extent reached/final and first content visible |
| Minimum-width horizontal thumb |0→224→0 DIP; 16 kHz readable/clear; 31 Hz restored |
| Eight-page visual pass |Cards/controls/gutters fit; General/Network/About did not need vertical scrolling |

Native resizing reached 640×640; automated checks separately covered 640×440. Native bottom/corner height resize was not established and is not counted. Initial disabled-EQ trial expired before restoring checkbox and correctly failed draft preservation; it remains rejected procedural evidence. Accepted pointer directories/receipt remain under that artifact's local `desktop/`.

This covers a specific display/session; other scales/touchpads/physical input/accessory behavior are unqualified.

## Upstream Task 3 publication

October 7 authorized issue #12/PR #13 used original `41190e0` and head `54cd18e` with verified five-file/private-free history, `Fixes #12` and body/readback/app attachment. Upstream 37717295723 initially awaited maintainer approval with zero jobs; that historical state is not current PR status.

## Current upstream status on 2026-10-10

Retained historical anchor: PR #13 and A PR #11 were merged by that audit, stable then `d4aa068`. B PR #15 remained closed/frozen; C/D local evidence and A/B-only hosted binaries were distinct. Current stable `cf420d9` and fork Main `8989b36` contain scrolling; current full package UI/DPI results are in continuation acceptance.

## All-local-branch integration and deferred execution — 2026-10-10

The old `9e894ab` source review retained padding/handler/fixtures but ran no app/build/test before reboot. Deferral is now satisfied. Earlier `6fe467b`/`d70f30d`/`1303a09` results remain source-bound; current geometry is executed through final package rather than assumed from historical source.

No scrolling branch/worktree was removed. Current worktree `fix-settings-scroll/AirFlash` is preserved until separately authorized cleanup; ignored pointer receipts must be preserved independently of Git.
