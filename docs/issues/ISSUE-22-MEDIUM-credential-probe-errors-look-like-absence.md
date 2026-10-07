# [MEDIUM] Credential existence errors can select transient authentication

- **Issue ID**: ISSUE-22
- **Severity**: MEDIUM
- **Kind**: Defect — credential error classification
- **Status**: Open; proposal only, not implemented
- **Evidence status**: Source and standard-library contract verified; fault-injection reproduction is specified but not executed
- **Implementation status**: Proposal only; no runtime change applied
- **Runtime baseline**: `41190e0`; documentation baseline `c077a05` with the audited [credentials/IPC](../analysis/credentials-and-ipc.md) and [authentication](../analysis/auth.md) pages
- **Target files**: [credentials.rs:105–121](../../native/airflash-engine/src/credentials.rs#L105-L121), [session.rs:232–251](../../native/airflash-engine/src/session.rs#L232-L251)
- **Analysis context**: [Credentials and IPC](../analysis/credentials-and-ipc.md), [Authentication](../analysis/auth.md)

## 1. Current behavior and impact

`credentials::load` returns `Ok(None)` whenever `target.exists()` is false. Playback interprets `None` as no saved credential and chooses transient pair-setup using PIN `3939`. The two authentication paths remain mutually exclusive, but the choice can be based on an unsuccessful existence probe rather than established file absence.

Rust documents that [Path::exists](https://doc.rust-lang.org/std/path/struct.Path.html#method.exists) returns false when file metadata cannot be accessed, including permission errors. Thus an inaccessible credential path can be classified as absent. Errors from later metadata/read/DPAPI/JSON checks do propagate when the initial probe succeeds. This report does not claim that every unreadable file takes the transient branch or that HomePods accept that branch under those conditions.

The concrete defect is loss of the storage error and selection of a different authentication method under uncertain credential presence. Hardware acceptance, user-visible error text, and frequency are unmeasured.

## 2. Source trace

| Step | Evidence |
| --- | --- |
| Filename hashes normalized accessory `/info` id | [credentials.rs:25–32](../../native/airflash-engine/src/credentials.rs#L25-L32) |
| False existence probe returns no credential | [credentials.rs:105–109](../../native/airflash-engine/src/credentials.rs#L105-L109) |
| Metadata/read/decrypt validation follows only a true probe | [credentials.rs:110–121](../../native/airflash-engine/src/credentials.rs#L110-L121) |
| Nonempty `/info` identity calls this loader; `Some` verifies and `None` uses transient setup | [session.rs:232–251](../../native/airflash-engine/src/session.rs#L232-L251) |

There is no fallback after an actual pair-verify failure. This issue concerns the earlier absence decision. It does not change credential encryption, accessory identity checks, or pairing protocols.

## 3. Deterministic reproduction to implement

Use synthetic identities and an injected filesystem result; do not inspect or change `%APPDATA%` credential files.

1. Exercise the loader's first filesystem operation with an injected metadata error such as `PermissionDenied`.
2. Model the present `Path::exists` behavior, which converts that error to false.
3. Feed the resulting `Ok(None)` into the same saved-versus-transient selection used by playback.
4. The source-derived current result is the transient branch, with the metadata error unavailable to the caller. The intended result is a storage error before either authentication exchange begins.
5. Separately exercise verified `NotFound`, a valid saved credential, and a decrypt/parse failure after successful metadata lookup.

This is a proposed fault-injection fixture, not a completed integration test or a claim about a particular Windows ACL configuration. An optional temporary-directory ACL test needs independent cleanup and must use synthetic encrypted test data only.

## 4. Proposed architectural fix

Replace the Boolean precheck with an error-preserving operation. For example, use one metadata/open operation and map only the chosen genuine-absence error (`NotFound`) to `None`; propagate other errors. Keep later read/decrypt/identity validation errors as errors. Define how a file disappearing between lookup and read is handled rather than silently changing authentication paths.

The loader already returns `Result<Option<Credentials>>`, so this can be corrected without changing JSONL v1, `config.json` schema 2, DPAPI format, filename normalization, or the single-method-per-connection invariant. Do not add plaintext backups, migrate credentials by desktop aliases, or retry transient setup after verification fails.

## 5. Acceptance criteria and coverage

- A verified missing file can return `None`; a permission/metadata error returns an error with no authentication exchange initiated.
- A successfully loaded file selects pair-verify exactly once; absence selects transient exactly once.
- Read/decrypt/parse failures and a failed pair-verify never become transient fallback.
- Synthetic tests distinguish `NotFound`, `PermissionDenied`, other I/O failures, disappearance during read, and valid/tampered payloads without logging secrets.
- Existing DPAPI magic, current-user encryption, normalized accessory filename key, and atomic replacement remain unchanged.

[credentials.rs:127–134](../../native/airflash-engine/src/credentials.rs#L127-L134) tests protect/unprotect and tamper rejection, not loader absence/error classification. [auth.rs:454–540](../../native/airflash-engine/src/auth.rs#L454-L540) covers synthetic pair-verify identity/proof handling. Neither is evidence of an executed filesystem fault test here. A later implementation should run the Rust suite; it was unavailable during the preceding audit. No receiver probe is needed. If later hardware qualification is authorized, finite probes remain gain at most 0.1 and duration at most five seconds.
