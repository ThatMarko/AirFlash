# Improve AirFlash reliability, pairing and diagnostics

Delayed receiver work can outlive explicit user actions. A missing DNS completion can occupy a resolve slot indefinitely, and a terminal engine event can leave a pairing prompt waiting for input. This tracks the remaining reliability work against upstream Main (11b7d18, after v0.6.2), including recovery from interrupted local mute changes and more useful diagnostics.

## Reproduction and expected behavior

- Hold a settings save after discovery reconciles a receiver identity. Start a newer Pair, then release the first save. The new attempt should retain its exact endpoint, settings and controls. An older automatic request should also yield to a queued Stop or a newer eligibility change. [Session controller](https://github.com/Ding-Kyoma/AirFlash/blob/11b7d18ad89fe2bbab43dfbea1191609e9457177/desktop/AirFlash.Core/SessionController.cs)
- Suppress a DNS completion and advance the fixture clock. A logical resolve should expire and retry within its bounds. A stale callback should neither publish its result nor release the next request's ownership. [Discovery service](https://github.com/Ding-Kyoma/AirFlash/blob/11b7d18ad89fe2bbab43dfbea1191609e9457177/desktop/AirFlash.App/Services/WindowsDiscovery.cs)
- Open a fake receiver's pairing prompt, then send a terminal process event. The prompt should close before the error is presented. If the prompt succeeds, the submitted password should match the accepted input byte for byte, including whitespace. [Pairing dialog](https://github.com/Ding-Kyoma/AirFlash/blob/11b7d18ad89fe2bbab43dfbea1191609e9457177/desktop/AirFlash.App/Ui/PinDialog.cs)
- Load a software group with a null ID. Validation should use the existing recovery path and preserve the original config bytes. [Settings validator](https://github.com/Ding-Kyoma/AirFlash/blob/11b7d18ad89fe2bbab43dfbea1191609e9457177/desktop/AirFlash.Core/Settings.cs)

The failure paths are grounded in source and isolated fixtures. The PTP Grant checks use synthetic packets. The field reports for clock readiness and discovery after sleep remain separate observations. [PTP report #38](https://github.com/Ding-Kyoma/AirFlash/issues/38) · [Discovery report #42](https://github.com/Ding-Kyoma/AirFlash/issues/42)

## Acceptance checklist

- [ ] Reject unsafe package cleanup paths and unbuildable release versions before side effects. Use the selected .NET host from the same working directory as the build and bound diagnostic child processes.
- [ ] Keep explicit Play, Pair, Stop and shutdown authoritative. Revalidate automatic admission, consider eligible alternatives, honor manual endpoint/port, and retain usable controls through passive discovery gaps.
- [ ] Reject a null group ID through validated recovery while preserving original bytes, supported schema migration and unknown settings.
- [ ] Separate DNS advertisements, cached data, logical deadlines and native owners. Bound retries and keep an uncertain helper owned until its exact exit is observed.
- [ ] Preserve password lookup errors, complete accepted input and one reader across the prompt. Cancel and join input on terminal failure.
- [ ] Surface process-owned terminal IPC failures through all affected hello and catalog consumers while preserving ordinary session filtering.
- [ ] Save the exact local endpoint mute bit under an exclusive durable lease before muting. Keep unresolved obligations and attempt exact restoration after normal cleanup.
- [ ] Expose diagnostics copy and distinguish padded capture slots from real silence. Keep retry faults accessible, allow dismissal of repeated notices, and keep receiver controls clear of the scrollbar. Validate PTP Grant fields and sequence handling with synthetic packets.

The proposed implementation and test guide are in the [refresh comparison](https://github.com/Ding-Kyoma/AirFlash/compare/11b7d18ad89fe2bbab43dfbea1191609e9457177...ThatMarko:codex/reliability-refresh-v062).
