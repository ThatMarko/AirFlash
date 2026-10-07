# [SEVERITY] Concrete behavior and impact

- **Issue ID**: ISSUE-NN (allocate the next unused id; never reuse a retired id)
- **Severity**: HIGH / MEDIUM / LOW; use CRITICAL only with evidence of the impact defined in [the index](README.md)
- **Kind**: Defect / Recovery gap / Enhancement
- **Status**: Open — proposal not implemented
- **Evidence status**: Source verified / Mock reproduced / Hardware reproduced; name what was actually executed
- **Runtime baseline**: Runtime commit and documentation baseline; current reports use `41190e0` and `c077a05` plus the audited [analysis index](../analysis/README.md)
- **Implementation status**: Proposal only; no runtime change applied
- **Analysis context**: Portable links to the dedicated audited analysis pages
- **Target files**: Portable links to the exact implementation lines and relevant tests
- **Related items**: Link active reports; refer to [the disposition ledger](TRIAGE.md) for retired ids

## 1. Current behavior and impact

Describe the concrete input/trigger, what this checkout does, and why that behavior is a defect, recovery gap, or requested enhancement. Explain user impact at the confidence supported by the evidence. A missing feature, documented platform limit, intentional invariant, or unmeasured hardware hypothesis is not automatically a defect.

Read the area's dedicated `docs/analysis/` page first, then trace the actual callers and implementation. Analysis is a verified map to source; citing a report or issue proposal alone does not prove a behavior. Separate source-derived results from observations made during a reproduction.

Do not import excluded `upstream/dev` behavior, infer firmware timing, or claim that the issue explains a particular incident from matching symptoms alone. External issue relationships require a verified primary report and must still distinguish correlation from cause.

## 2. Source trace and bounded root cause

Provide exact code anchors for the trigger, state/ownership transition, erroneous result, and consumer. Include the relevant JSONL boundary, persistence transaction, or thread/lock boundary when one participates. Check the source rather than copying an old snippet.

Use portable paths, for example:

```markdown
[AppViewModel.cs:211–219](../../desktop/AirFlash.App/ViewModels/AppViewModel.cs#L211-L219)
```

Use a short excerpt only when it improves understanding. Explain which assumptions are unverified and which existing mechanism already handles a related case. Do not substitute absence of a preferred architecture for proof of a bug.

## 3. Reproduction and evidence status

Prefer a deterministic source-level/mock scenario using temporary configuration, fake services, or a localhost protocol fixture. Record prerequisites, fixture input, sequence/order, expected current assertion, and expected fixed assertion.

State explicitly whether these steps were executed. Proposed test steps belong under “planned reproduction”; do not label them “observed result.” Include actual logs/results only when available, sanitized, and identified by baseline/environment.

Avoid real receiver addresses, MAC addresses, PINs, credential files, user paths, and secrets. Documentation IPv4 examples must use loopback or reserved TEST-NET ranges. Do not edit the real user's configuration or credential store for a reproduction.

## 4. Proposed fix and compatibility

Mark the fix unimplemented. Explain the minimum ownership/state/interface change, why it addresses this trigger, and how cancellation and error paths remain bounded. Avoid a speculative patch unless verified against the actual architecture.

State impacts on `config.json` schema 2 and JSONL v1. Preserve:

- No blocking RTSP/DNS/capture work on the UI dispatcher.
- No selected-offline-adapter fallback to all interfaces.
- Persistent manual receivers and protection from discovery loss.
- One authentication method per connection; current-user DPAPI and accessory `deviceID` filename key.
- Exact prior mute-bit restoration, including an originally muted endpoint.
- Receiver volume independent of PCM master gain.

Cross-reference related work that must be coordinated. Do not implement a second competing policy for the same state transition or present another proposed issue's fix as already applied.

## 5. Acceptance criteria and verification

List observable pass/fail criteria covering the reported case, normal behavior, cancellation/stale events, and compatibility. Identify existing tests and the precise missing assertions; source inspection does not establish that tests passed.

Select the relevant checks for the later implementation:

```powershell
dotnet test desktop/AirFlash.Tests/AirFlash.Tests.csproj
cargo test --manifest-path native/airflash-engine/Cargo.toml
uv run --locked pytest -q -ra
```

The WPF mock verification entry point is `--ui-smoke` on a correctly built app; it is separate from the core xUnit assembly. `scripts/dotnet.ps1` accepts paths relative to `desktop/`. There is no `scripts/test.ps1` in this baseline. An ignored localhost soak is not an automatic hardware test or a prerequisite for every change.

Declare runtime/hardware/toolchain limits honestly. Default to fixtures that send no audio. Any separately authorized real-device finite probe must remain at gain at most **0.1**, duration at most **five seconds**, and the repository's quiet WAV limit; never run a hardware soak. Do not claim acoustic latency, PTP lock, successful playback, or physical mute persistence from mock tests.
