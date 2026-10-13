# Historical A/B/C/D integration acceptance

## Current Main relationship — 2026-10-12

This record preserves the **October 9–10 composition**. It is not the current Main test result. Reviewed fork runtime baseline `8989b3609dd1294f4d1d6eb1502f345cc22bc382` contains the refreshed A–H/scrolling/installer heads; current source and full qualification are in [continuation acceptance](CONTINUATION-ACCEPTANCE.md). Original stable is now `cf420d97700463c56adc31408510e8336e99e51c`. The reboot gate is satisfied; C/D are pushed and integrated into fork Main but not upstream.

## Subsequent local main integration — 2026-10-10

Historical Main `9e894ab133f8cbdb3b5f5ac1f411023b182e65d7` integrated 17 then-existing local branch tips, preserving every branch and backup `codex/main-before-all-local-20261010` at `877a854`. Its desktop/native/scripts/tests/installer trees matched prior verified `6fe467b`; additional history included a Group A workflow checking historical `de13b6d`, not that Main source.

This dated integration was source-only and did not execute new tests before the then-required reboot. Those deferrals are historical, not current pending work. Subsequent B/D corrections and E–H implementation are recorded separately; the old binary is never relabeled `8989b36`.

## Standalone checks and corrections

| Historical group/source | Executed result and distinction |
| --- | --- |
| A `de13b6d` | 37 focused fake preflight tests/eight real SDK-resolution checks, intended desktop cwd even when invoked from an unrelated path with spaces |
| B `11c6046` | Three complete 60-case lifecycle runs; no runtime change |
| B regional `bceb440` | Four 299-check regional/catalog WPF pairs; invalid-culture controls; 253 Core; production session blobs unchanged |
| B catalog `0a2c1ac` | Old Traditional Chinese tray marker failed after 14 checks; English/Simplified 15 controls. Corrected four 300-check pairs/253 Core in both configurations |
| C `c3a1d5e` | Oversized future-schema 6 failures/2 controls on initial `e129466`; 22 added cases, 126 focused/261 Core per configuration, 208 WPF each language |
| D `48e7ab62` | Browse 1223-vs 995 failure on initial `4c79f22`; 69 focused repeated 10 times, 232 Core per configuration, 149 WPF each language |

Builds above were warning free. C/D original eight-file/two-commit boundaries were based on `41190e0`; final refreshed heads are different. D's old native resolve-settlement interpretation was superseded by process-exit ownership; [D acceptance](GROUP-D-ACCEPTANCE.md) explains the lifetime correction. The finite historical DNS-only probe observed two browses/four resolves/terminal 1223 with final counters zero; it did not qualify all callback ordering or later helper source.

## Local composition and cross-group fixtures

Retained worktree `verify-abcd/AirFlash`, branch `codex/verify-abcd`, qualified composition `6fe467b585e80fde0b8bfa47598195f51b8e9a5a`. It combined A/B/scrolling/installer plus C/D and stable `d4aa068`, with 19 then-current WPF cross-group assertions. Temporary real-store gates tested locked auto-flush, revision preservation, newer Pair/Stop, exact encoding and deferred cleanup failure notices; playback/mute/autostart effects and saved bytes were asserted.

Prior `031510e` passed 420 Core in both configurations/86 Python and a local unreserved 0.3.1 publish; EXE `478048723aeabfe36c36f4b24367a1dde54109f848990c1a109c388b2c7196ff`. Updated `5d219d1` passed 422 Release; subsequent catalog correction produced final source below. Those intermediate receipts remain separate.

## Historical local validation before the catalog assertion

`122500d117315a896fe34a9e8c541faa9fdca626` passed 422 Core per configuration and six portable 578 English/570 Chinese checks. EXE `ec1d4e8869e1e1968d62ffd03873662629017cae81cbea485e1e2f5c164f1572`,77,230,525 bytes. These reports lacked exact final static-catalog restoration; source/renders were correct before tray cleanup, but caller culture alone failed to detect later catalog drift. The desired marker failed on old cleanup, then final source corrected it.

Local evidence paths: `artifacts/abcd-tests/`, `abcd-publish/`, `abcd-ui/`, `abcd-final-2026-10-10/` and `abcd-extra-2026-10-09/`. They are ignored receipts, not GitHub document links.

## Verified local 6fe467b composition

| Exact historical check | Result |
| --- | --- |
| Core/build |422 Debug and 422 Release; zero warnings/errors |
| EXE |77,230,639 bytes; SHA-256 `d14cb80ad5a8ab13dbeeb3be7e4af814bf015c9ca2554b5d1f1fcd8f0d32d117` |
| Metadata |0.3.1.0; `local-abcd-validation` and full `6fe467b` source |
| Regional/catalog pairs |en-US/en-US, zh-CN/en-US: 579 each; en-US/zh-CN, zh-CN/zh-CN, en-US/zh-TW, zh-TW/zh-TW: 571 each |
| Assertion inventory |378 non-scroll (300 B/non-C, 59 C, 19 cross-group), 201 English/193 Chinese scrolling, once-only catalog marker |
| Geometry |880×640 default/640×440 minimum; 32 page/theme/size plus 2 frequency labels, measured 12 DIP gutters |
| MSI |Both actual SDK-host payload paths/two synthetic same-version refresh stages; hashes preserved, SDK 10.0.401/WiX 6.0.2; no local installation |

Sealed local evidence: `artifacts/abcd-catalog-final-2026-10-10/{final-local-receipt,build-receipt,packaging-receipt}.json` and `ui/receipt.json`. Local publish used hosted unchanged-source engine SHA-256 `7710ddcb58c4aa630f5f777e35d9ceac2c7e4e6dccf147a9c1a613ae21a3043c`; compilation/tests were hosted, not local.

## Separate hosted validation boundary

Historical public A/B-only `098c0a1` [run 38073990748](https://github.com/ThatMarko/AirFlash/actions/runs/38073990748) passed 86 Python/47 native/255 Core and package/MSI; one localhost soak ignored. Its exact EXE `5cc8c388b08dde6ce636c67fae18ebe203b6a988c214bf6d7d59fb4094ac5de5` passed six 500/492 runs, predating the catalog marker.

Corrected public A/B-only `d70f30dc32ef152c25c339f75ae09cabf29272ca`, [run 38075339970](https://github.com/ThatMarko/AirFlash/actions/runs/38075339970), passed 86 Python/47 native/255 Core, build/package/refresh/embedded hello/seven disposable MSI stages. It **excluded C/D**.

| Verified historical archive/file | SHA-256 |
| --- | --- |
| Package ZIP | `5623ccbaa369499177533880cad2ee29bf56a9c7f147c27224041e6ec57b0f91` |
| Report ZIP | `8de057b94180b86a7f06b0ba227b6dbd7629923825202b4610fb601137094e53` |
| EXE, 77,202,916 bytes | `d454de31d031a339bb616d3390b748279a6e5bb374bd11dc3de4d8956c2143c6` |
| MSI | `15be3dd53ebfd5e31547607ca5e9cd62301832400aef33c2caf972e560eeff6a` |
| Engine | `2be496034ad88bb6f82abf9122ea85623f671d6054dcfb1970d893d799506442` |

Archive/repository/ref/source/run/attempt 1 provenance preceded execution. Exact EXE passed six 501 English/493 Chinese regional/catalog runs, 300 non-scroll plus 201/193 scroll, all 32+2 layouts/gutters and separate JSONL v1 hello. All binary/manifest/checksum bytes remained unchanged. Local evidence: `artifacts/regional-verification/hosted-d70f30d/final-desktop-receipt.json`. Native binary parity with the private composition's different hosted input is not claimed.

## Upstream status and remaining work

Historical `d4aa068` status has been superseded by stable `cf420d9`. Current A/scrolling are merged, B PR #15 closed/frozen, issue #14 open, C–H/installer fork-only. Current qualification, source heads and publication gates are in [continuation acceptance](CONTINUATION-ACCEPTANCE.md).

This historical record establishes only its executed synthetic/portable/MSI scopes. It proves no real receiver, acoustics, crash durability or all Windows/DPI ordering. No historical reboot deferral or unpushed state remains a current blocker.
