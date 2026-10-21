# [MEDIUM] Credential lookup error preservation — resolved in fork main

## Current-source boundary — 2026-10-21

Combined `6c1e2ef`, compared with stable `439de626`, uses bounded single-open credential/password reads and preserves lookup, decryption, parsing and identity errors. Only defined initial-open absence selects another eligible authentication path. Accessory-keyed current-user DPAPI and mutually exclusive authentication remain unchanged; #24/#25 remain open.

[Current source, acceptance and executed limits](PR-READINESS-2026-10-21.md) supersede older current/open/schema labels below. The remaining source links and test counts retain their recorded historical baselines; this documentation refresh claims no new test execution or upstream publication.

**Status:** Implemented and integrated in own-fork main `8989b3609dd1294f4d1d6eb1502f345cc22bc382`, reviewed 2026-10-12. Original stable base is `cf420d97700463c56adc31408510e8336e99e51c`; this implementation is not yet integrated there. Severity is the original triage priority, not an unqualified current hardware failure.

**Group:** E — Authentication inputs. **Feature head:** `0ebc59bd99e5ca9d1c7219a7b57bc9bb2b237c5a`. [Plan](IMPLEMENTATION_PLAN.md#issue-09), [acceptance](GROUP-E-ACCEPTANCE.md), [credentials/IPC](../analysis/credentials-and-ipc.md), [authentication](../analysis/auth.md).

## Publication status — 2026-10-18

Published with PIN normalization as [issue #24](https://github.com/Ding-Kyoma/AirFlash/issues/24) and [PR #25](https://github.com/Ding-Kyoma/AirFlash/pull/25). Both remain open; the PR is unmerged at current standalone head `234167b31092339b2e909bfa2cbee5748b6006e1`. Earlier feature/main hashes, source links and verification counts below retain their recorded provenance; [E acceptance](GROUP-E-ACCEPTANCE.md) records later qualification separately.

## Current behavior and source trace

[load/load_with](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/credentials.rs#L106) opens the accessory-keyed file once and maps **only initial open NotFound** to absence. It reads the opened object with a 16 KiB+1 bound; later NotFound/read/permission/I/O, oversize, magic, DPAPI, JSON and empty-identity failures remain errors.

[authenticate_with](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/session.rs#L200) propagates storage errors before either auth exchange. Loaded credentials select verify once; genuine absence selects transient once. Failed verify never selects transient. Current-user DPAPI, W2AP version-1 format, accessory /info hash normalization, atomic replacement and zeroization remain.

Missing/non-string/empty playback /info deviceID still skips file lookup and uses the inherited transient path. That separate identity policy was not expanded by this fix. File I/O can retain setup/socket_error/retryable classification; preservation does not promise authentication_failed for every storage failure.

## Acceptance and evidence

- Initial absence differs from PermissionDenied/other error and read-stage disappearance.
- Valid saved credential selects verify exactly once; absent selects transient exactly once.
- Failed storage/decrypt/parse/identity/verify initiates no alternate auth method.
- Normalized accessory key, replacement, opened-object read after path removal and bounded data are verified using synthetic temporary roots.

[loader fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/credentials.rs#L203) and [selection fixtures](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/session.rs#L793) check result plus method calls/classified serialized faults. Acceptance preserves the failing error-hiding mutation separately from healthy execution. No real credential store was inspected.

Exact-main CI passed 667 Core, 124 Python and 69 native tests (recording-device qualification and thirty-minute localhost soak ignored). Packaged integration at `4ae1db4` passed six 1,059-assertion UI runs, three actual 125% DPI runs with 40 each and disposable MSI lifecycle; current runtime/package inputs are identical, with main's last commit documentation only. This refresh reviewed sources/records; it did not rerun suites or use real user data/devices.

## Remaining work

The published upstream PR awaits maintainer review and integration; real-device qualification remains separate. No plaintext backup, catalog-key migration or verify-to-transient fallback was introduced.

## Historical finding

Original `41190e0` used `Path::exists()`, which could hide metadata/access failure as false and select transient. Later errors after a true probe already propagated. Original analysis predicted the uncertain-presence path without claiming a particular ACL/HomePod reproduction. The [archival report at the prior documentation commit](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/issues/ISSUE-09-MEDIUM-credential-probe-errors-look-like-absence.md) retains the original `41190e0` source links, source-derived reproduction/proposal and separately dated implementation updates. Its open/proposal-only/pending-reboot labels are historical; current acceptance is above.