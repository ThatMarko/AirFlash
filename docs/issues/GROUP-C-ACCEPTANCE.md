# Group C feature acceptance

Verified on 2026-10-08. Reports 06 and 07 are implemented together on local branch `codex/fix-settings-recovery`, head `e1294668161fb1967448a186e2e0263444d0d3bd`, directly based on refreshed original stable `41190e0d13a63a714c08dffe73ababca1804875c`. This branch has not been pushed, published, or merged into either fork or upstream main.

## Implemented behavior

- The store reads bytes through the guarded load, decodes with compatible BOM detection, and never rereads a failing path from its recovery catch. Only actual missing-file/directory results mean absence; access and I/O failures create a pending preservation obligation.
- One store lock coordinates both startup Load calls, saves and identity backups. Every distinct recovery revision captured by Load, pending Save or identity save stays available until verified backups and successful replacement. Failed backup/replacement remains retryable, including a source changed between attempts.
- Recovery and migration backups retain exact bytes, including UTF-8/16/32 BOMs. Existing `.pre-wpf.bak` contents are kept; differing sources receive unique `.pre-wpf.<guid>.bak` siblings. Cached backups are reverified, and deleted/corrupted paths are excluded from completed-preservation notices.
- An unreadable original cannot be silently replaced after defaults load. Later persistence reads and preserves it first or fails safely. Disappearance without any captured original still blocks replacement. Future schemas remain fatal, including case variants and duplicate fields containing a future value; a rejected store cannot save until a valid reread.
- Existing null-collection, null receiver-option and alias normalization precedes total semantic validation. Null manual entries, empty/duplicate ordinal IDs, invalid addresses/ports, sample rates, enums, ranges and EQ values recover before runtime consumes them. Null/blank manual names, nullable overrides, extension fields and offline selected adapter IDs retain compatibility.
- Application persistence refreshes an owned recovery notice after successful preservation, without replacing an unrelated notice. Normal production startup's read-only native `hello` is unchanged.

## Executed verification

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

Next: implement Group D separately. Group C remains a local original-base implementation, ready for a later explicitly authorized Task 3 publication.
