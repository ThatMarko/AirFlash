# Group E acceptance

## Current stable-base qualification — 2026-10-11

Current public head `0ebc59bd99e5ca9d1c7219a7b57bc9bb2b237c5a` is pushed against stable `cf420d9`: independent refresh review, 52 focused/221 owning Core tests and a zero-warning/error Release build pass. PIN and native authentication runtime code remains the reviewed implementation qualified by the exact-source Windows receipts below. Current combined source additionally requalifies the latest upstream native capture implementation; it is recorded separately in continuation acceptance. Modal clipboard mutation runs only on an explicitly enabled disposable hosted fixture, with finally restoration; local tests never use the real clipboard/store/accessories.

These current sections supersede earlier pending-reboot, unpushed-feature and proposal wording. Combined fork results and publication limits are recorded in [continuation acceptance](CONTINUATION-ACCEPTANCE.md). The dated evidence below retains its original source boundary.

## Earlier implementation evidence

Feature branch `codex/fix-auth-input`, managed worktree `fix-auth-input/AirFlash`, retains original implementation `47d0823060be4c817b3ee8bf27d360cc16a3dc5d` and normally merges freshly verified original stable `91698d8cd3bdf5666b417a12eaaf8d6a1ea36363`. Final reviewed head `fc8b91942bf93e320b04ec74d5ad0e433e092f69` is pushed to the user's fork and qualified below. No upstream issue/PR was created. Its complete diff has eight intended public source/test files; every outgoing commit excludes fork-only analysis/issues/planning. It is not integrated into original stable or fork main by this record.

## Input behavior

One ASCII normalizer removes ASCII dashes and then requires 4–8 ASCII digits. Dialog enablement, click validation and the actual returned PIN share it. The native PIN validator remains strict. Leading zeros, sixteen-character editing limits, selection replacement, rejected typing, Unicode, text replacement, cancellation, and actual fake `pair_pin`/`start` payloads are asserted. Actual single-line paste behavior is separately tested; newline suffixes are truncated by the WPF textbox before submission. Ordinary local harness calls never access the clipboard. Clipboard verification explicitly requires the own-fork GitHub-hosted runner.

The unchanged PIN return, with only a narrow modal test seam added, failed the normalized fake-command assertion. Its source/report remain under worktree `artifacts/group-e-2026-10-11/baseline-pin`. Fixed local checks: 19 normalizer cases; 184 full Core tests on the original base; warning-free Release build; 118 expanded narrow UI assertions; full catalog runs 468 English, 460 Simplified Chinese and 460 Traditional Chinese. Refreshed stable: 218 Core tests, warning-free build, 118 narrow UI assertions.

## Credential lookup

The accessory `/info` identity still keys the current-user DPAPI file with the existing normalization, magic and payload format. Loading opens one handle; only initial `NotFound` means absence. Permission/other open errors and all read errors, including read-stage `NotFound`, remain errors. Reads stop at 16,385 bytes and reject oversize. Deletion after opening reads the opened object. DPAPI, JSON, identity and verification errors cannot select another authentication method. Setup file-I/O classification remains unchanged.

Synthetic temporary-root/injected tests assert one authentication method, serialized contextual I/O errors, protected malformed JSON, tampering, empty identities, replacement and bounds. Existing localhost crypto/verification tests remain controls. A separate preserved validation branch `codex/verify-auth-absence-regression` at `a7c8646` emulates the old error-hiding lookup; hosted run `38162296485` fails the decisive initial-open classification test (35 pass, one fail). This is a source-equivalent injected regression mutation, not an execution of the literal unchanged loader.

## Executed hosted checks

Exact implementation `47d0823`, run `38162234703`: 57 native tests passed, one existing qualification test ignored; clippy with warnings denied and release/static-CRT build passed; 184 Core tests/build passed; 220 real modal typing/text/paste assertions passed. These use synthetic data and no receiver/capture/user credential store. The native qualification test remains ignored; no hardware result is claimed.

Exact refreshed candidate `f1e21ca`, run `38162432563`: all 57 native tests, clippy and compilation passed, then the newly required compatibility audit failed because the fork validation workflow lacked `uv`. Logs/artifacts are retained. Workflow `4227af1` adds official setup-uv and locked dependency sync; corrected exact-source run `38162746839` passed. Local Rust/MSVC/Windows SDK remain absent; verified local rustfmt is formatting evidence only.

Refreshed upstream logs two startup stages before the UI harness initializes its isolated data path. Initial refreshed UI trials therefore appended startup log lines to the default log directory; they did not read settings, credentials or endpoints. The shared diagnostic startup fix now chooses isolated data before either log, recognizes exactly the nine dispatched diagnostic modes, and preserves normal/unknown-argument startup. Five UI guards verify those mode decisions, the isolated directory and both actual log stages without reading default user logs. Independent review's broad-predicate finding was corrected; a source-equivalent mutation fails the unknown-switch guard. The implementation also retains the disposable-runner clipboard restriction and accurate workflow label.

Final exact head `fc8b919`, run `38164451047`: 57 native tests passed with one existing qualification test ignored; warnings-denied clippy, release/static-CRT build and compatibility audit passed; 218 Core tests and warning-free Release solution build passed; 225 real modal assertions passed. Local final narrow UI has 123 passing assertions; full refreshed catalogs before the final three pure mode guards passed 684 English / 676 Simplified Chinese / 676 Traditional Chinese. The final exact runner checks include all five startup guards. Source/assertion review is clear. Receipts and raw job log are in fork `artifacts/continuation-2026-10-11/ci-e-fc8b919/`, including a source receipt that explicitly identifies `fc8b919` rather than the workflow's main-branch SHA. Group E is locally complete on this feature branch. No real configuration, credential, endpoint, receiver or desktop installer operation occurred; no hardware qualification is claimed.
