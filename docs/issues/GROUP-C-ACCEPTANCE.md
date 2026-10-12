# Group C feature acceptance

## Current stable-base qualification — 2026-10-11

Current public head `5d72da3b77b4ac72f858966baef04b8a4ad8b1ad` is pushed against stable `cf420d9`, with independent refresh review, 129 focused/300 owning Core tests and a zero-warning/error Release build. Source preservation/loaded validation is unchanged by the newer recording feature. Both startup loads share preservation state; raw bytes/BOM and unknown fields remain protected. Final integrated WPF checks include the corrected explicit browse demand after safe rejected-settings startup; earlier failed fixtures and receipts remain preserved. These results do not establish coordination with every external replacement after a valid Load.

These current sections supersede earlier pending-reboot, unpushed-feature and proposal wording. Combined fork results and publication limits are recorded in [continuation acceptance](CONTINUATION-ACCEPTANCE.md). The dated evidence below retains its original source boundary.

## Earlier implementation evidence

## Stable refresh — 2026-10-11

Feature `3055988f68e2b1387b125c9dde238bc8ec37609a` normally integrates verified
original stable `91698d8`, is pushed to the fork and integrated into fork main.
Its configuration production source remains unchanged by that refresh. Discovery
fixtures now implement scoped Start/Stop and request panel browsing after proving
safe recovered defaults leave hidden startup paused. The standalone feature
passes 172 focused and 297 owning Core tests, a warning-free Release build, and
70 isolated recovery UI checks in both English and Simplified Chinese. Independent
review is clear. Traditional Chinese recovery messages still have existing English
fallbacks. Integrated-only ownership/failure-notice fixtures separately pass 92
checks after the same explicit browse-demand correction; their earlier failed
report and source are retained. These are fork receipts, not original-main
integration or new upstream publication.

## Continuation on 2026-10-11

The [continuation acceptance](CONTINUATION-ACCEPTANCE.md) supersedes the earlier pending-reboot and unpushed-branch statements below. The manual reboot is verified; current B/D follow-ups are reviewed, pushed to the fork and integrated locally, with 437 Core tests and six crossed regional/catalog WPF runs passing. E is actively in progress; F/G/H remain unimplemented. Upstream publication gates remain unchanged. Earlier source audits and receipts retain their dated boundaries.

## Local main integration on 2026-10-10

Final C `c3a1d5e4bf2879054e729b77440f97aa259155e9` is now an ancestor of local main **`9e894ab133f8cbdb3b5f5ac1f411023b182e65d7`**, with final B/D, scrolling and updated upstream. Both original C commits and its branch are retained; C remains unpushed/unpublished and unmerged upstream. This source-only audit traced both normal startup loads, byte capture/BOM decoding, migration, collection normalization, semantic validation, future-schema classification, pending revision/backup retry and identity persistence into application notice/save/admission callers. Current Core/WPF assertions were inspected without executing them.

The results below belong to their recorded feature/composition source hashes. No app, build, test, configuration, registry, DNS or audio operation runs in this merge/audit; **new local main testing is deferred until after reboot**. See [integration acceptance](ABCD-INTEGRATION-ACCEPTANCE.md). Task 3 publication remains unactivated.

Initial verification completed on 2026-10-08; the additional testing below advanced local branch `codex/fix-settings-recovery` to **`c3a1d5e4bf2879054e729b77440f97aa259155e9`**. Reports 06 and 07 are implemented together in two commits based on original stable `41190e0d13a63a714c08dffe73ababca1804875c`. The initial implementation head was `e1294668161fb1967448a186e2e0263444d0d3bd`. Neither commit has been pushed or published. The new local integration above supersedes earlier statements that C was outside local main; any future publication still requires checking the standalone branch against actual updated upstream.

Worktree: `C:\Users\marko\.codex\worktrees\fix-settings-recovery\AirFlash`.

## Implemented behavior

- The store reads bytes through the guarded load, decodes with compatible BOM detection, and never rereads a failing path from its recovery catch. Only actual missing-file/directory results mean absence; access and I/O failures create a pending preservation obligation.
- One store lock coordinates both startup Load calls, saves and identity backups. Every distinct recovery revision captured by Load, pending Save or identity save stays available until verified backups and successful replacement. Failed backup/replacement remains retryable, including a source changed between attempts.
- Recovery and migration backups retain exact bytes, including UTF-8/16/32 BOMs. Existing `.pre-wpf.bak` contents are kept; differing sources receive unique `.pre-wpf.<guid>.bak` siblings. Cached backups are reverified, and deleted/corrupted paths are excluded from completed-preservation notices.
- An unreadable original cannot be silently replaced after defaults load. Later persistence reads and preserves it first or fails safely. Disappearance without any captured original still blocks replacement. Future schemas remain fatal, including case variants and duplicate fields containing a future value; a rejected store cannot save until a valid reread.
- Existing null-collection, null receiver-option and alias normalization precedes total semantic validation. Null manual entries, empty/duplicate ordinal IDs, blank hosts, invalid ports, sample rates, enums, ranges and EQ values recover before runtime consumes them. Loaded host validation does not perform DNS or adopt the dialog's stricter host-syntax, IPv6 and duplicate-endpoint rules. Null/blank manual names, nullable overrides, extension fields and offline selected adapter IDs retain compatibility.
- Application persistence refreshes an owned recovery notice after successful preservation, without replacing an unrelated notice. Normal production startup's read-only native `hello` is unchanged.

## Initial verification on 2026-10-08

| Check | Result |
| --- | --- |
| Original-source decisive Core probes | 8 desired assertions failed after successful build; relevant production blob hashes unchanged |
| Focused recovery/validation Core suite | 76 passed; zero failures/skips |
| Complete owning Core suite | 239 passed; zero failures/skips |
| Locked restore and final Debug/Release solution builds | SDK 10.0.401; zero build warnings/errors |
| Full isolated WPF suite | 208 English and 208 Chinese checks passed |
| Independent final source/assertion review | Clear after preservation-retry and stale-backup-notice findings were resolved |
| Complete diff and outgoing commit | Eight desktop runtime/test/localization files; no fork analysis/issues/planning |

The original probes executed locked-read recovery, null manual validation, three invalid/null rates, UTF-16 migration backup bytes, an unrelated pre-existing backup, and case-insensitive future-schema handling. They establish those original Core defects, not an executed original GUI startup failure.

Core regressions additionally cover access denial injection, multiple captured revisions, failed backups/replacements, identity-save retries, corrupted backup diagnostics, future-schema persistence guards and schema/field compatibility. Fixed-branch WPF fixtures use the same concrete temporary store for both startup loads, actual notice bindings, deterministic automatic-admission invocation, counted fake playback/mute/autostart services, an exclusive file lock and later panel persistence. Rejected input creates no mock playback connection or command, local mute, or unpreserved replacement.

The first full Core pass caught an identity-backup exception compatibility regression, which was corrected. Independent review found and fixed captured-revision loss on failure/retry, then stale paths in completed-backup notices. An initial UI fixture used a fake broadcast identity, which legitimately caused alias persistence after exact recovery backup; replacing it with a synthetic endpoint removed that unrelated save without weakening assertions. The final passes above include these corrections.

## Evidence and limits

Ignored implementation-worktree evidence is in `artifacts/group-c-tests/`, `artifacts/group-c-baseline/`, `artifacts/group-c-ui-en-final/` and `artifacts/group-c-ui-zh-final/`. Core/TRX and baseline receipts contain hashes, source boundaries and actual counts. The temporary original-base comparison worktree was archived after copying its evidence.

Tests use synthetic temporary files and simulated services; the WPF suite uses isolated real window/tray identities. No real settings, credential store, discovery, receivers, capture or endpoint mutation is involved. Store serialization and atomic replacement do not establish power-loss durability or coordination with arbitrary external writers. No native engine source, schema-2 format or JSONL v1 contract changed.

## Additional testing and fix on 2026-10-09

The initial future-schema guard depended on bounded numeric conversion. Positive integer schema tokens above that conversion's range could be treated as recoverable invalid input, allowing defaults persistence to replace a future configuration. Eight decisive cases ran against unchanged `e129466`; six failed and the two ordinary future-version controls passed. This is a defect in the initial Group C implementation, separate from the eight original-`41190e0` probes above.

Commit `c3a1d5e` classifies positive integer JSON tokens lexically before bounded parsing: every integer above the supported schema remains fatal, including oversized values, case variants and supported duplicates. Load refuses it and latches a block on later Save/identity Save; when preservation is pending, Save and identity Save also inspect the current source and refuse future-schema backup/replacement. An ordinary Save or identity Save after a valid Load does not inspect every later external schema change without a preservation obligation. Existing fractional, exponent, string and invalid negative spellings retain controlled recovery with exact-byte backup. No schema format changed.

| Additional check | Result |
| --- | --- |
| Decisive pre-fix cases on unchanged initial implementation | Six failed, two controls passed; successful build |
| Added Core cases | 22 covering oversized future integers, save/identity-save blocking and spelling compatibility |
| Focused settings suite | 126 passed; zero failures/skips |
| Complete Core suite, Debug and Release | 261 passed in each configuration; zero failures/skips |
| Debug/Release solution builds | SDK 10.0.401; zero warnings/errors |
| Fresh full isolated WPF checks | 208 English and 208 Chinese passed on the corrected source |
| Independent lexical-classification/assertion review | No actionable finding |
| Complete feature scope | Two commits, eight desktop runtime/test/localization files; fork-only planning remains excluded |

Evidence is retained in the implementation worktree's `artifacts/more-testing-2026-10-09/future-schema-after-receipt.json` and associated TRX files. The receipt was captured before committing, so its `committed: false` and `base_head: e129466` describe that test invocation. Its source hashes match the committed `c3a1d5e` files: SettingsStore SHA-256 `a9513055d64de09b8edab56a690de4d5fe0ce10e7421849524cd07c249be68a3`, SettingsRecoveryTests `80615499213bb953d089e05dc3cbc4d45737f7b90c812be490a0935ef01378b2`. Fresh UI reports are in the fork checkout's ignored `artifacts/abcd-extra-2026-10-09/c-ui-en-US/` and `c-ui-zh-CN/`.

Group D was verified separately; see [D acceptance](GROUP-D-ACCEPTANCE.md). Earlier local [A/B/C/D source](ABCD-INTEGRATION-ACCEPTANCE.md) `6fe467b` passed 422 Core per configuration, six 579/571-check portable runs including all 19 cross-group assertions and exact catalog restoration, and MSI payload/refresh checks. C production/standalone results are unchanged; those counts do not describe new main execution. Public hosted sources exclude C/D. C is now integrated into local main as recorded above, while its publication remains unactivated.
