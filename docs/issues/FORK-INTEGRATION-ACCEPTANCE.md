# Historical fork integration acceptance

## Current Main relationship — 2026-10-12

This page retains the initial A+B and follow-up package evidence. Reviewed fork runtime baseline `8989b3609dd1294f4d1d6eb1502f345cc22bc382` contains all A–H/scrolling/installer fixes; final qualified code is `4ae1db4f01dcf7857482dd136ea8d9dffa3aa8a1` on stable `cf420d97700463c56adc31408510e8336e99e51c`. [Continuation acceptance](CONTINUATION-ACCEPTANCE.md) is the current complete Main qualification. Historical artifacts below are not relabeled current source.

## Subsequent all-branch local integration — 2026-10-10

Historical `9e894ab` contained 17 then-existing branch tips and stable `d4aa068`; every branch was preserved. Its desktop/native/tooling trees matched qualified `6fe467b`, while the added historical Group A workflow checked a different exact source. No runtime checks ran before the then-pending reboot. That gate is now satisfied, C/D are pushed and E–H are complete. [Historical composition](ABCD-INTEGRATION-ACCEPTANCE.md#subsequent-local-main-integration--2026-10-10) retains this boundary.

## Source and branch boundaries

Initial A `de13b6d` and B `342aeb7` were based directly on original stable `41190e0`. Fork Main preserved documentation through `76c9a76`, merged A at `ca84c7c`, B at `72dc7c7`, then added fork-only validation. Public feature complete diffs/every outgoing commit excluded these private documents/workflows. Backup branches preserved pre-integration source.

Initial independent code/assertion reviews were clear; later audits found further B defects and verification gaps, so that initial review is not universal completeness evidence. Historical local checks passed 70 relevant prerequisite/release cases, 86 Python/lint, 218 Core, warning-free Debug/Release builds, SDK 10.0.401/locked restore and icon verification.

## Hosted build and exact downloaded binary

Exact initial source `b2105c9c158769ca06f8dc4e39d0e525fb497402` passed [normal CI 37677853767](https://github.com/ThatMarko/AirFlash/actions/runs/37677853767) and [package 37677853674](https://github.com/ThatMarko/AirFlash/actions/runs/37677853674). All 19 integrated feature files matched their reviewed branch blobs.

The own-fork read-only workflow built/published directly and tested installers on disposable Windows; it did not invoke production version reservation/release. Numeric 0.3.1 was an **unreserved validation value**, not a chosen future release.

Hosted 86 Python/47 native/218 Core passed; one existing localhost soak ignored. Lint/icons/static CRT/build/publish/default+explicit installer hosts/runtime-only early rejection/embedded hello and disposable install/repair/upgrade/uninstall/data preservation passed.

Artifact 11509100627, verified ZIP SHA-256 `c5b00cceac18ca1063912ed6cfa565249d7a3198715a65b2db0301f7767006cc`, bound exact repository/ref/source/run and 3 payload hashes. Exact portable EXE passed 273 WPF checks per language and separate native hello without workers. Local managed publish reused verified hosted native output and produced identical EXE SHA-256 `f8dbe0568e632349059c34f0234fd7ce90f696ee8c0e953af8de5be4c41b9d19`.

## Local publishing and installer generation

An uncached WiX/empty CLI-home feed caused initial local MSI resolution failure. Temporary project-scoped official NuGet config populated WiX 6.0.2, was removed and default/explicit/cache-only builds passed with 0 warnings/errors. No global feed or production source change occurred; read-only inspection checked version 0.3.1 and no local installation.

Evidence remains in local ignored `artifacts/fork-integration/`, including archive/API/manifest/logs/TRX, `desktop-downloaded/receipt.json`, republish comparison and installer receipt. Hosted retention was 14 days; downloaded copies preserve provenance. Rust compilation/testing was hosted, not local.

## Follow-up validation on 2026-10-08

B `11c6046` corrected endpoint/awaited-cleanup intent races after decisive failures on `342aeb7`. Historical standalone 60 focused/253 Core per configuration/298 WPF per language passed; builds 0 warnings/errors. A remained `de13b6d` with 37 focused/86 Python; scrolling `54cd18e` passed 163 Core/350 English/342 Chinese and 32+2 layouts with 12 DIP measured gutters. [B acceptance](GROUP-B-ACCEPTANCE.md#follow-up-audit-on-2026-10-08) preserves underlying failures.

Separate temporary combined `bfed430b0578f14600004b87c70d3c7b379d0036` excluded then-unpublished C/D and passed [run 37870969984](https://github.com/ThatMarko/AirFlash/actions/runs/37870969984): 86 Python/47 native/253 Core and complete build/package/MSI; one soak ignored. Artifact 11590072723 ZIP SHA-256 `e34e05b97479bfdcbbad1c38d4e9c1bb5d206a1082e1d7bec398cbffefd1ca37` was verified before launch.

| Exact follow-up file | SHA-256 |
| --- | --- |
| EXE | `c62cadf2210f81129a460cfff96bea2334abeac7c7690354f797c67883c16295` |
| MSI | `bd6fb10e0fc70c8ca2aae5002a0862549d4953efb818c06e5f9a625aae6eb39f` |
| Engine | `8428019fb97a780ea0fb1560813b8a5ca0c605478e9e8904e66ee6d27e5fdcd6` |

Exact EXE passed 499 English/491 Chinese, including 298 non-scroll plus 201/193 scroll, realized 880×640/640×440, 32 page/theme/size plus 2 labels/12 DIP gutters. Native hello and unchanged payload hashes passed. No new pointer/hardware qualification. Local archive/report/blob-bound evidence: `artifacts/fork-integration/scroll-github-bfed430/` and `artifacts/pr-audit-2026-10-08/downloaded-desktop/`.

At publication, upstream runs were awaiting approval with zero jobs; fork success did not bypass that gate. No executed upstream failure log follows from a later zero-job failure state. A historical Copilot quota failure is not code review.

## Current upstream status on 2026-10-10

This retained anchor refers to the October 10 review: A/scrolling merged; B PR #15 closed/frozen `11c6046`, issue #14 open. Regional hidden-row expectation and Traditional Chinese static tray-catalog restoration were fixture errors corrected in `bceb440`/`0a2c1ac`, with unchanged production session code. Earlier culture labels did not prove final catalog restoration.

Private `6fe467b` passed 422 Core/configuration, six 579/571 portable and payload/MSI refresh. Public A/B-only `d70f30d` passed hosted 86 Python/47 native/255 Core and six 501/493 exact downloaded runs; it excluded C/D. Full archive/payload mappings and final marker proof remain in [A/B/C/D acceptance](ABCD-INTEGRATION-ACCEPTANCE.md#separate-hosted-validation-boundary). Different hosted engines are not byte-parity claims.

## Limits and next action

Current Main acceptance and source/runtime/toolchain limitations are in [continuation acceptance](CONTINUATION-ACCEPTANCE.md). This file preserves bounded history; it does not assert current unimplemented/unpushed/reboot-pending work.

No real receiver/config/credential/audio/endpoint operation or local MSI installation is established by these fixtures. Native tests/mock UI are not acoustic/hardware qualification. Current groups are ready for selected Task 3 publication after actual upstream refresh; upstream merges, releases and cleanup remain separately authorized.
