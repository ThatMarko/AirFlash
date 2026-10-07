# Combined fork integration acceptance

Prepared on 2026-10-07 for the authorized Group A + Group B fork integration. This is unofficial validation of `ThatMarko/AirFlash`; original `Ding-Kyoma/AirFlash:main` remains `41190e0d13a63a714c08dffe73ababca1804875c`. A fork merge does not imply upstream acceptance.

## Source and branch boundaries

- Group A: `codex/fix-build-preflight`, unchanged head `de13b6d3e22a177537ed9d5977fe693a5b497fc1`; upstream [PR #11](https://github.com/Ding-Kyoma/AirFlash/pull/11) is still open.
- Group B: `codex/fix-session-lifecycle`, unchanged head `342aeb77cb85ff6f3f7f850161b3db79d8637945`, with its earlier Core contract commit `4662796`. Its [separate checklist](GROUP-B-ACCEPTANCE.md) remains the report-by-report acceptance record.
- The Group B feature branch was pushed to the fork and its remote head verified as `342aeb77cb85ff6f3f7f850161b3db79d8637945`. Group A's existing remote branch remains `de13b6d`; no PR branch rewrite was required.
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

Remote publication, run IDs, build results and downloaded-binary checks will be recorded here after execution. A planned check is not a passing result.

The fork-only `Fork integration validation` workflow checks out the triggering SHA with a read-only token, runs Python/native/managed checks, publishes directly, builds both installer-host variants, verifies the embedded native hello and performs installer lifecycle checks only on the disposable hosted runner. It never invokes the version-reserving production build or Release workflow.

The package's numeric version is an **unreserved test value `0.3.1`**, chosen above the then-current upstream `v0.3.0` solely for hosted MSI upgrade testing. Its informational version includes `fork-validation` and the exact source SHA. This does not alter the project version, reserve a tag, publish a release, or establish the next stable version.

Assets carry a source/run/toolchain manifest and SHA256 hashes. Desktop validation will select the exact successful run/artifact, check its source and hashes, and run the downloaded portable EXE's isolated English/Chinese mock harness. A separate downloaded native hello avoids changing the desktop's normal engine cache; bundled native extraction/hello is checked on the hosted runner.

## Limits and next action

No real configuration, credential store, receiver, capture or endpoint mutation is part of desktop validation. The mock UI does use isolated real tray/window registrations. Native localhost tests and mock tests do not establish hardware/acoustic reliability. MSI installation belongs exclusively to the disposable hosted runner.

Group B upstream issue/PR creation remains Task 3. Neither upstream PR merging nor public release publication is authorized by this fork-validation run.
