# [MEDIUM] Prior endpoint mute bits cannot be recovered after forced desktop termination

- **Issue ID**: ISSUE-13
- **Implementation plan**: [Work package ISSUE-13](IMPLEMENTATION_PLAN.md#issue-13)
- **PR group**: G — Owned mute recovery
- **Severity**: MEDIUM
- **Kind**: Recovery gap
- **Type / Status**: Open recovery gap — source-verified missing recovery mechanism; endpoint outcome requires separate validation
- **Subsystem**: Desktop audio endpoint ownership and startup recovery
- **Runtime baseline**: `41190e0d13a63a714c08dffe73ababca1804875c`, unchanged by documentation baseline `c077a0577de26ad7e44b8fd50f62456e12965fb0`
- **Evidence status**: Implementation and existing test sources inspected on 2026-10-07. No forced termination, OS reboot, real endpoint mutation, or reproduction harness was executed. Source establishes loss of the saved prior bit, not that every crash leaves speakers muted across reboot.
- **Implementation status**: Proposal only; no runtime change applied
- **Target files**: [AudioService.cs](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/AudioService.cs#L7-L31), [App.xaml.cs](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/App.xaml.cs#L45-L58), [SessionController.cs](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SessionController.cs#L218-L224)
- **Analysis context**: [Control and mute](../analysis/control-and-mute.md), [Session](../analysis/session.md), [Failures](../analysis/failures.md)

## 1. Current behavior and bounded impact

`AudioService` records a render endpoint's prior mute bit only in `_originalMute`, an in-memory dictionary. `MuteAsync` reads that bit, writes mute true, then records the prior bit after the setter succeeds. A graceful restore writes the exact saved bit, removing the entry only after success. Individual COM failures retain entries for later restoration. ([AudioService.cs:7–31](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/AudioService.cs#L7-L31), [63–80](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/AudioService.cs#L63-L80).)

If the desktop is forcibly terminated after muting, this dictionary cannot survive. A new `AudioService` has no record of whether the endpoint was muted before AirFlash took ownership. If the endpoint still reports mute true, a later streaming attempt captures true as its new prior bit and restores true on clean stop. The previously unmuted state is then unrecoverable from AirFlash's own data. This is a source-visible missing recovery mechanism; persistence of the OS endpoint bit for a particular kill/reboot scenario and its audible impact have not been tested.

The issue does not establish permanent muting, full-volume output, a receiver disconnect cause, or missing normal cleanup. Normal quit awaits view-model disposal, controller stop restores after cleanup, and audio-service disposal restores again. The dispatcher exception handler logs/shows an error and marks that exception handled; not every exception is a process exit. Adding an exit handler would not cover a forced kill or power loss. ([App.xaml.cs:63–74](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/App.xaml.cs#L63-L74), [110–123](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/App.xaml.cs#L110-L123), [SessionController.cs:199–224](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Core/SessionController.cs#L199-L224), [AudioService.cs:87–106](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/AudioService.cs#L87-L106).)

## 2. Bounded root cause

1. Endpoint state is changed through Core Audio, while the original bit is held only in the service instance's dictionary.
2. Constructor initialization registers endpoint notifications but reads no recovery record. A subsequent instance therefore starts without prior ownership information.
3. The existing restoration code cannot distinguish an AirFlash-owned mute left by a terminated instance from a mute intentionally set by the user or another application afterward. Blindly writing false on startup would violate the exact-bit restoration invariant, especially for an endpoint originally muted before streaming.

These conclusions follow from [AudioService.cs:11,17–31,63–80](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/Services/AudioService.cs#L17-L31). The current single-instance ownership check precedes creation of `AudioService`, but it does not reconstruct the previous process's mute dictionary. ([App.xaml.cs:45–55](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.App/App.xaml.cs#L45-L55).)

## 3. Proposed reproduction — not executed

Start with an isolated endpoint backend whose mute property outlives a service instance, plus a disposable temporary recovery-store directory. No real endpoint, credentials, receiver, or production settings are needed.

1. Initialize the fake endpoint as unmuted, capture its bit, and perform the normal mute operation.
2. Simulate process loss by discarding service memory without running `Restore` or `DisposeAsync`.
3. Construct a new service over the same fake endpoint state.
4. Start and cleanly stop another fake streaming attempt.
5. Repeat with an endpoint originally muted.

**Source-predicted current result:** there is no durable prior bit to recover. If the backend remains muted, the new attempt preserves/restores that true bit. This model reproduces ownership-information loss; it cannot certify actual Windows persistence across termination or reboot. No observed production log is asserted here.

## 4. Proposed change — unimplemented design

Introduce a small, versioned recovery journal separate from schema 2 user preferences. Each entry must retain the exact original boolean, endpoint identity, and process/ownership generation. Durably record recovery intent **before** changing the endpoint, using atomic replacement and an explicit state transition so crash windows around the mute write remain representable. If durable acquisition fails, do not silently apply an untracked mute.

On graceful restore, continue writing the saved bit on the audio MTA worker and remove/resolve a record only after the corresponding restore succeeds. An unavailable endpoint or failed restore must leave a retryable entry. A restart must not consume a journal owned by another still-active instance.

Startup recovery must not blindly unmute every listed endpoint. A stale record cannot prove that the user has not changed mute since the crash. Automatically restore only when the implementation can establish valid ownership under its chosen model; otherwise present the unresolved endpoint and its saved bit for an explicit recovery decision before a new session replaces that information. Missing, corrupt, ambiguous, or unsupported records must not cause an unconditional false write. The policy for abandoning an entry without restoration must be explicit, rather than silently treating it as successfully restored.

Do not introduce a reconnect grace period into normal stop/failure restoration. Preserve the prior true bit as carefully as the prior false bit. Disk and endpoint calls must stay off the UI thread. This journal must contain no pairing credentials, and it must not change pair-verify/transient mutual exclusion, DPAPI storage, authentication, or the JSONL process boundary. An exit callback can be supplementary cleanup only; it is not the recovery mechanism.

## 5. Testable acceptance criteria

- Under a fake process-loss model, recovery retains the original true/false bit and endpoint id instead of recapturing an AirFlash-applied true bit as the original.
- Each interruption point before/after durable intent, endpoint mutation, restore, and journal removal has defined recovery behavior. A failure to persist acquisition never leaves an intentionally untracked mute.
- A successful normal stop writes the exact bit after the existing attempt cleanup, without an added reconnect delay, and clears/resolves that endpoint's journal entry. This is not a new hard restoration deadline.
- COM failure or endpoint absence retains ownership/recovery data; a returning endpoint can be retried without inventing a prior bit.
- A still-active owner, corrupt record, unknown endpoint, unsupported version, or ambiguous external/user change never causes blanket unmute. Recovery ambiguity remains visible and does not overwrite the saved prior bit.
- Multiple endpoint entries, repeated restore, originally muted endpoints, and failed journal writes are covered. Startup/restore operations remain asynchronous to the dispatcher.
- Current graceful session and pairing behavior stays unchanged; no credentials or real network identifiers are placed in journal fixtures or diagnostics.

## 6. Existing coverage and verification plan

[SessionTests.cs:199–208](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/SessionTests.cs#L199-L208) checks restoration requests after a fake engine failure and disabling mute. Its [FakeAudio at lines 400–406](https://github.com/Ding-Kyoma/AirFlash/blob/41190e0d13a63a714c08dffe73ababca1804875c/desktop/AirFlash.Tests/SessionTests.cs#L400-L406) simply toggles a boolean and does not model exact-bit ownership, persistent endpoint state, COM failures, or forced termination. These test sources were read, not executed for this issue review. No dedicated existing concrete `AudioService` crash-recovery/journal test was found.

Add injectable endpoint/recovery-store boundaries and deterministic interruption tests first. Any later real Windows qualification must explicitly record initial mute state, process-loss method, observed endpoint state, and cleanup outcome; do not claim reboot persistence from an in-memory mock. This review authorizes no hardware action. If a receiver is required in separately authorized qualification, finite probes remain limited to sender gain 0.1 and five seconds, with existing disabled-EQ constraints; no hardware soak is required for journal validation.
