# AirFlash implementation backlog

This backlog was re-triaged on **2026-10-07** using the freshly audited [analysis](../analysis/README.md) and the actual runtime source at `41190e0`, unchanged by documentation baseline `c077a05`. It contains **17 active reports: 14 source-supported defects, one recovery gap, and two enhancements**. None of the proposed runtime fixes has been implemented by this documentation pass.

Of the fourteen original reports, seven were retained and rewritten, and seven were retired. Ten new reports were added. [TRIAGE.md](TRIAGE.md) records every original disposition, rejected claim, new item, and verification limit. Retired ids remain reserved; **the next new id is ISSUE-25**.

Source-supported means the reported control flow, contract mismatch, or missing recovery/validation mechanism is visible in this baseline. It does not mean a mock reproduction was executed or a HomePod incident was attributed to it. Every report separates that evidence from planned tests and unmeasured hardware behavior. A proposal is not an applied fix.

## Severity and kind

| Severity | Use |
| --- | --- |
| CRITICAL | Demonstrated severe data loss, security impact, or similarly broad unrecoverable failure. No current report establishes this level |
| HIGH | Substantial playback/session/discovery disruption, including concrete ownership races or indefinitely blocked resolution |
| MEDIUM | Scoped correctness, validation, recovery, or tooling failure with a specific trigger |
| LOW | Limited contract/diagnostics/UI impact, observability improvement, or maintenance cleanup |

Defects describe incorrect behavior with a traceable trigger. Recovery gaps describe a missing recovery mechanism while qualifying OS outcomes. Enhancements improve an intentionally limited behavior and must not be presented as proven playback failures. Severity expresses priority/impact; it does not replace evidence confidence.

## Active reports

| ID | Severity | Kind | Concrete scope |
| --- | --- | --- | --- |
| [ISSUE-01](ISSUE-01-HIGH-discovery-disconnects-active-playback.md) | HIGH | Defect | Passive absence/incompleteness stops discovered playback; missing rows also lose panel controls |
| [ISSUE-14](ISSUE-14-HIGH-dns-resolve-without-deadline-stalls-discovery.md) | HIGH | Defect | A non-completing resolve has no deadline, suppresses new queries, and retains operation ownership |
| [ISSUE-15](ISSUE-15-HIGH-stale-discovery-snapshot-controls-new-session.md) | HIGH | Defect | A delayed discovery identity save can stop/update a separately started session using an old snapshot |
| [ISSUE-24](ISSUE-24-HIGH-delayed-autoconnect-replaces-new-session.md) | HIGH | Defect | Delayed auto-connect can replace a new pairing session or undo a newer user Stop |
| [ISSUE-05](ISSUE-05-MEDIUM-build-pipeline-missing-dotnet-sdk-check.md) | MEDIUM | Tooling defect | Selected SDK compatibility is not checked before conditional release reservation, cleanup, and native build |
| [ISSUE-11](ISSUE-11-MEDIUM-endpoint-mute-crash-recovery-gap.md) | MEDIUM | Recovery gap | Exact prior endpoint mute bits are memory-only and cannot be recovered after abrupt app death |
| [ISSUE-16](ISSUE-16-MEDIUM-attempted-preferred-receiver-blocks-autoconnect.md) | MEDIUM | Defect | An already-attempted preferred candidate blocks another eligible auto-connect target |
| [ISSUE-17](ISSUE-17-MEDIUM-manual-play-redirected-to-stereo-by-address.md) | MEDIUM | Defect | A manual row can be redirected to a discovered stereo group by address alone, ignoring its port/identity |
| [ISSUE-18](ISSUE-18-MEDIUM-pairing-pin-format-mismatch.md) | MEDIUM | Defect | The PIN dialog enables formatted values that native PIN validation rejects |
| [ISSUE-19](ISSUE-19-MEDIUM-settings-recovery-reread-escapes.md) | MEDIUM | Defect | Recovery rereads a failing configuration file inside the catch and can throw instead of recovering |
| [ISSUE-20](ISSUE-20-MEDIUM-uncorrelated-ipc-errors-become-timeouts.md) | MEDIUM | Contract defect | Empty-correlated terminal IPC rejections are discarded until a host timeout, losing the original final fault |
| [ISSUE-22](ISSUE-22-MEDIUM-credential-probe-errors-look-like-absence.md) | MEDIUM | Defect | Credential existence errors can look like absence and select transient authentication |
| [ISSUE-23](ISSUE-23-MEDIUM-invalid-loaded-settings-reach-runtime.md) | MEDIUM | Defect | Valid JSON with null manual entries or invalid settings reaches unchecked startup/playback assumptions |
| [ISSUE-03](ISSUE-03-LOW-capture-padding-metrics-lack-frame-accounting.md) | LOW | Enhancement | Add missing-frame/whole-packet accounting and precise local timing labels without inventing a network/idle diagnosis |
| [ISSUE-04](ISSUE-04-LOW-unbound-copy-diagnostics-command.md) | LOW | UI binding defect | The existing diagnostics-copy command has no Settings action |
| [ISSUE-07](ISSUE-07-LOW-orphaned-translation-keys-cleanup.md) | LOW | Enhancement | Review ten statically unused catalog candidates, preserving dynamic and intentional entries |
| [ISSUE-21](ISSUE-21-LOW-equalizer-ignored-update-success-metadata.md) | LOW | Contract defect | Ignored stale/conflicting EQ preparations still return success metadata for the submitted curve |

There are four HIGH, nine MEDIUM, and four LOW items. ISSUE-02, ISSUE-06, ISSUE-08, ISSUE-09, ISSUE-10, ISSUE-12, and ISSUE-13 have no active report. Their source facts, withdrawal reasons, and scope distinctions are preserved in the disposition ledger rather than in misleading standalone defect reports.

## Implementation order and dependencies

1. **Establish executable regression fixtures.** Resolve the selected-SDK/preflight policy in ISSUE-05 and make the .NET/Rust tools available for implementation verification. Existing tests and proposed fixtures are different evidence; do not claim the latter pass before adding/running them. Use mocks/localhost; no hardware soak is needed.
2. **Protect session ownership, then remove passive discovery stop coupling.** Coordinate ISSUE-15's atomic expected-generation guard and ISSUE-24's conditional auto-start admission before or with ISSUE-01. A delayed automatic action must not replace a newer session or negate user Stop. Retain last-known active controls without enabling a new incomplete stereo start. Address ISSUE-14 independently with safe DNS-SD operation/cancellation ownership and bounded resolve deadlines; clearing a token alone is insufficient.
3. **Correct receiver-selection paths.** ISSUE-16 filters attempted candidates before choosing a preferred target; ISSUE-17 preserves explicit manual endpoint selection. Preserve user-stop suppression, canonical identity migration, selected-NIC no-fallback, and existing stereo completeness rules.
4. **Validate startup/authentication inputs.** Coordinate ISSUE-19 and ISSUE-23 around one readable-original/backup/recovery policy. Fix ISSUE-18's PIN contract and ISSUE-22's error-preserving credential absence decision. Keep schema 2/JSONL v1, unknown settings fields, accessory-keyed DPAPI, and mutually exclusive authentication paths.
5. **Make error/results truthful and recovery safe.** ISSUE-20 surfaces terminal process-owned IPC failures with their original nonretryable status. ISSUE-21 separates accepted preparation from stale/conflicting/idempotent replay; acknowledgements still do not claim DSP completion. Design ISSUE-11's crash-recovery ownership journal conservatively before writing any stale mute bit back; normal exact-bit restoration retains its existing best-effort cleanup path, with no added reconnect grace.
6. **Finish diagnostics and maintenance.** Expose ISSUE-04's existing command, implement ISSUE-03's bounded observability additions, and perform ISSUE-07's reviewed cleanup. Do not use diagnostic ambiguity as evidence for a firmware buffer change or WLAN API integration.

These are proposed priorities, not proof that a runtime repair has occurred. Each implementation needs its own focused diff, corresponding analysis update, and relevant executed tests. Hardware qualification, if separately performed, stays within the repository's finite limits.

## Verification and reporting rules

Use [ISSUE_TEMPLATE.md](ISSUE_TEMPLATE.md). Read the dedicated analysis page, verify the actual source/callers, cite portable relative file links with exact line anchors, state the baseline, and name whether reproduction was executed. Include a bounded proposal and testable acceptance criteria. Preserve prior ids across rewrites and never recycle retired ids.

The earlier [analysis audit](../analysis/audit.md) ran **49 Python tests with no skips**; it could not run .NET/Rust builds/tests because the required toolchains were unavailable. This triage pass inspected additional source/test evidence and checked the documents; it did not implement or test the proposed runtime fixes or open receiver connections. Current-check details are in [TRIAGE.md](TRIAGE.md).

Do not assert that these items explain upstream issues #6/#7, current release behavior, a real household log, or excluded `upstream/dev` without new primary evidence. Packet send success is not receiver acknowledgement/audibility; underrun count is not network loss; source or mock tests are not acoustic measurements. Never add actual IP/MAC addresses, PINs, credentials, or private logs to issue documents. Any authorized real-device finite probe remains gain at most **0.1**, duration at most **five seconds**; no hardware soak.
