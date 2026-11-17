# Improve AirFlash reliability, pairing and diagnostics

Delayed receiver work can outlive explicit user actions. A missing DNS completion can keep a resolve slot occupied while fresh advertisements extend the record lifetime, and a terminal engine event can leave a pairing prompt waiting for input. This tracks the remaining reliability work against upstream Main at `60e3390`, including recovery from interrupted local mute changes and more useful diagnostics.

## Reproduction and expected behavior

The upstream failure paths below are inferred from source. Isolated fixtures exercise the expected behavior on the proposed branch.

- Hold a settings save after discovery reconciles a receiver identity. Start a newer Pair, then release the first save. The new attempt should retain its exact endpoint, settings and controls. An older automatic request should also yield to a queued Stop or a newer eligibility change. [Session controller](https://github.com/Ding-Kyoma/AirFlash/blob/60e3390c8e171a49ba842cd8ccc957d648e3823b/desktop/AirFlash.Core/SessionController.cs)
- Suppress a DNS completion and advance the fixture clock. A logical resolve should expire and retry within its bounds. A stale callback should neither publish its result nor release the next request's ownership. [Discovery service](https://github.com/Ding-Kyoma/AirFlash/blob/60e3390c8e171a49ba842cd8ccc957d648e3823b/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L122-L199)
- Open a fake receiver's pairing prompt, then send a terminal process event. The prompt should close before the error is presented. If the prompt succeeds, the submitted password should match the accepted input byte for byte, including whitespace. [Pairing event loop](https://github.com/Ding-Kyoma/AirFlash/blob/60e3390c8e171a49ba842cd8ccc957d648e3823b/desktop/AirFlash.Core/SessionController.cs#L1039-L1072)
- Load a software group with a null ID. Validation should use the existing recovery path and preserve the original config bytes. [Settings validator](https://github.com/Ding-Kyoma/AirFlash/blob/60e3390c8e171a49ba842cd8ccc957d648e3823b/desktop/AirFlash.Core/Settings.cs#L207-L213)

Isolated fixtures exercise these failures, including synthetic PTP packets for Grant handling. Upstream has closed the PTP and after-sleep reports and integrated the wake retry and PTP exchange recovery fixes. This work covers the remaining behavior listed below. [PTP report #38](https://github.com/Ding-Kyoma/AirFlash/issues/38) · [After-sleep report #42](https://github.com/Ding-Kyoma/AirFlash/issues/42) · [Wake retry PR #45](https://github.com/Ding-Kyoma/AirFlash/pull/45) · [PTP exchange PR #51](https://github.com/Ding-Kyoma/AirFlash/pull/51)

## Acceptance checklist

- [ ] Reject unsafe package cleanup paths and unbuildable release versions before side effects. Use the selected .NET host from the same working directory as the build and bound diagnostic child processes.
- [ ] Keep explicit Play, Pair, Stop and shutdown authoritative. Revalidate automatic admission, consider eligible alternatives, honor manual endpoint/port, validate that a native stereo pair has two distinct usable peers, and retain usable controls through passive discovery gaps and Quit. Preserve saved panel gain until a current guarded update applies it; do not repeat the save on retry. Ignore obsolete endpoint work while retaining fresh changes for the current owner. If engine disposal fails, keep Stop available through passive absence, block replacement playback, and join the worker’s exact-bit restoration attempt before disposal retries.
- [ ] Reject a null group ID through validated recovery while preserving original bytes, supported schema migration and unknown settings.
- [ ] Separate DNS advertisements, cached data, logical deadlines and native owners. Bound retries and keep an uncertain helper owned until its exact exit is observed.
- [ ] Preserve password lookup errors, complete accepted input and one reader across the prompt. Cancel and join input on terminal failure.
- [ ] Surface process-owned terminal IPC failures through all affected hello and catalog consumers while preserving ordinary session filtering.
- [ ] Record durable intent with the exact local endpoint and prior mute bit under an exclusive owner lease before muting. Keep unresolved obligations and attempt exact restoration after normal cleanup.
- [ ] Expose diagnostics copy and distinguish padded capture slots from real silence. Keep retry faults accessible, allow dismissal of repeated notices, and keep receiver controls clear of the scrollbar. Validate PTP Grant fields, current pending requests, independently reordered messages and same-identity contract renewal with synthetic packets. Do not treat the remote sequence counter as a token for the local request. Honor same-clock holdover, preserve upstream ownership checks during clock recovery, and retain the capture probe cooldown through empty polls. Keep Settings navigation and capture choices clear of their scrollbars.

The proposed implementation and test guide are in the [refresh comparison](https://github.com/Ding-Kyoma/AirFlash/compare/60e3390c8e171a49ba842cd8ccc957d648e3823b...ThatMarko:codex/reliability-refresh-v062).
