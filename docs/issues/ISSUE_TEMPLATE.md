# [SEVERITY] Concrete behavior and impact

- **Issue ID**: ISSUE-NN; next retained id is ISSUE-18.
- **Group**: Existing coherent group, or a justified new package.
- **Status**: Open proposal / Implemented in fork / Integrated upstream. Keep status consistent with Main and executed evidence.
- **Kind / original severity**: Defect / Recovery gap / Enhancement; HIGH / MEDIUM / LOW supported by concrete impact. Hypothetical hardware causes are not defects.
- **Current source**: Exact checkout and repository. Last verified fork Main is `8989b3609dd1294f4d1d6eb1502f345cc22bc382`; original stable is `cf420d97700463c56adc31408510e8336e99e51c`. Recheck these before a future report.
- **Original defective source**: Separate from fixed source. Existing reports retain `41190e0`; documentation `c077a05` is not a runtime revision.
- **Implemented source / branch**: Exact head, Main ancestry and bounded scope.
- **Evidence**: Source derived / Controlled regression executed / Fixed test executed / Real-device observation. State what ran.
- **Analysis / plan**: Dedicated [analysis](../analysis/README.md) and [work package](IMPLEMENTATION_PLAN.md).
- **Publication**: Local ids differ from GitHub numbers; issue/PR status differs from implementation readiness.

## 1. Current Main behavior and impact

Describe the current source for the concrete trigger. For an implemented report, lead with corrected observable behavior, then separate original failure history. For a proposal, distinguish predictions from observations.

Trace callers and consumers through analysis and source. Never infer an incident cause from symptoms, import excluded development or treat an external API contract as repository behavior.

## 2. Source and contract trace

Pin exact source to the owning repository/full SHA:

```markdown
[SessionController.cs](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs)
```

Use verified line anchors where useful. Trace input, ownership/state transition, result and consumer, including protocol, persistence, threads, locks and cleanup. Quote short excerpts; identify unverified assumptions and related mechanisms already present.

## 3. Original failure and controlled evidence

Record defective source, unchanged production blobs, fixture scaffolding, input/order, expected assertion and observed failure. A source-equivalent mutation is not literal unchanged original execution; label it accordingly. A green rerun with unchanged runtime does not explain an intermittent failure.

Use temporary files, synthetic identities, fakes or receiver-free localhost protocol fixtures. Exclude real addresses, MACs, PINs, credentials, private logs and machine-specific paths from public reports. IPv4 examples use loopback or TEST-NET. Never use real configuration/endpoints/credential stores for fixtures.

## 4. Implemented change or proposed work

For implemented scope, describe the final change/error/cancellation behavior, integrated Main and public feature head, with executed acceptance. Do not leave an obsolete proposed-fix body under a current addendum.

For a new proposal, mark it unimplemented, define a bounded contract and test gap. An open related PR is not an integrated prerequisite. Preserve:

- Schema 2, migration/unknown fields and JSONL v1.
- Nonblocking dispatcher and selected-offline-adapter no-fallback.
- Manual persistence and exact explicit endpoint selection.
- Authentication exclusivity; current-user DPAPI keyed by accessory `deviceID`.
- Exact prior mute bit, including true; receiver volume independent of PCM gain.
- Counter units/missing-data semantics without network/acoustic claims.

## 5. Executed acceptance and limitations

Separate regression, owning suite/build, WPF, native, integration and artifact identity. Record actual source/run/configuration, counts, ignored tests and differing binary inputs. Reading test source, commands or prior receipts is not fresh execution.

Relevant root entry points:

```powershell
# Wrapper enters desktop/; project paths are relative to that directory.
./scripts/dotnet.ps1 build AirFlash.sln '-p:RestoreLockedMode=true'
./scripts/dotnet.ps1 test AirFlash.Tests/AirFlash.Tests.csproj '-p:RestoreLockedMode=true'
cargo test --manifest-path native/airflash-engine/Cargo.toml
uv run --locked pytest -q -ra
```

Core xUnit does not execute WPF bindings. Use the separate built `--ui-smoke` harness with isolated output; preserve exact regional culture/catalog and release gates in finally. There is no `scripts/test.ps1`. Run appropriate checks without inflating counts through unrelated repetition.

Historical reboot deferral is satisfied. Local native tooling is unavailable; qualification is hosted. The ignored localhost soak is not a hardware test or default prerequisite. Production release/build entry points can reserve versions and clean output; use fake operations for reproductions. Separately authorized real-device probes require gain at most 0.1, duration at most five seconds and the quiet-WAV limit.

Mocks do not establish real playback, acoustic latency, PTP lock, every OS cancellation schedule or physical mute/power-loss durability. [Continuation acceptance](CONTINUATION-ACCEPTANCE.md) defines current provenance and limits.

## 6. Publication handoff

Keep one coherent issue/PR per group. Verify private-document exclusion in final diff and every outgoing commit, actual upstream scope and executed checks before selected Task 3 publication. Reporting does not authorize upstream merge, release dispatch or branch deletion.
