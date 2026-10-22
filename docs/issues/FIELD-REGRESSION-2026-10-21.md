# Preview connection regression — 2026-10-21

The owner reports that 6.0.0-rc.1 worked and 6.0.1-rc.1 fails to connect; fork v0.4.1 (`fb6721d`) remains their best working reference. **Publication and reliability acceptance are on hold pending repair and field verification.** Earlier automated results remain valid for their recorded fake-service/build checks; they do not override this field failure.

## Executed observations

The owner supplied the final error `PTP exchange did not become ready · clock · clock_not_ready` and authorized reading one dated WPF log. That read showed eight `clock_not_ready` failures across two 6.0.1 launches. Each recorded attempt completed stream negotiation with realtime ALAC, 44.1 kHz, PTP and receiver-follow clock mode, then failed before Streaming. Engine processes exited with code zero. The log contains earlier 6.0.0 Streaming starts. Version labels alone do not independently identify executable hashes or source commits.

Every failed attempt lacked a recorded clock-status snapshot. No raw log, personal address, receiver identifier, credential or configuration is copied into this report. No agent-run application, receiver, capture, endpoint, firewall or process inspection was performed. The log read is the owner's explicit exception to the earlier personal-data boundary.

## Source-confirmed findings

- Playback, discovery and native sources match between delivered 6.0.0's expected source `32b49cb` and 6.0.1's qualified source `8b94f3b`/public `07dd238`. The runtime follow-up changes notice dismissal. Distinct builds still produce distinct binaries; source identity does not explain or dismiss the observed regression.
- The current clock-following path starts PTP before authentication and AirPlay SETUP. Initial unicast subscription requests can therefore precede session establishment. Subscription renewal waits ten seconds, while startup readiness fails after eight seconds following setup. A missing early request/grant can leave no retry within that budget. This scheduling gap is confirmed in source; the log does not prove why the receiver's exchange failed.
- Grants require the pending sequence, matching source/target and a two-second response window. A repair must retain those checks and accept still-valid delayed replies across retransmission. Readiness thresholds, clock roles and the eight-second startup limit must remain intact.
- The generic unsafe-owner/restart warning also covers ordinary eight-child DNS concurrency. A ninth healthy advertised name reproduces it with no unsafe native owner. This warning does not independently cause playback reconnects.
- During reconnect, ordinary error details select the discovery notice instead of the retained playback fault. The fault remains in diagnostics. A pure headless regression reproduces this presentation loss; four cases fail against the original expression, while seven controls pass.

## Reviewed repairs and verification in progress

Public candidate `ac145a8c1d342b239a398ab1eb7c605e3a15dc1e` adds three reviewed commits on the existing branch: `9e37ca5` retains playback faults during retries; `42baed8` separates normal DNS backpressure from retained ownership and reports prolonged waiting truthfully; `ac145a8` retries unsettled follower subscriptions every second and emits clock status before startup failure. Healthy, fully granted followers retain ten-second renewal. Retransmission keeps the pending sequence and original two-second reply window. Clock/source validation, readiness thresholds, the eight-second startup gate and exact DNS exit ownership remain intact.

Executed local checks: **1,029 Core tests passed, zero skipped**, and a Release solution build with zero warnings/errors. DNS/localization checks pass **141/141**; the healthy-nine-name regression fails against unchanged production code and passes after correction. The details regression's original expression fails four cases with seven healthy controls; the repair passes all eleven. A repository-local formatter obtained from hash-verified official Rust components parses/formats only the two changed native files; independent review confirms formatting-only deltas and a passing direct check. No compiler, toolchain or global settings were installed.

Six new native packet/state tests cover initial loss, grant-before-Announce, delayed/stale replies, source/port changes, leases, healthy cadence and sender behavior using injected time and synthetic packets. Native compilation/tests remain pending [exact-source Windows headless run 39702875282](https://github.com/ThatMarko/AirFlash/actions/runs/39702875282), carried by fork Main `ca0672c`. No new corrective preview has been delivered. Do not claim the field failure resolved solely from mock passes; successful field retest remains required.

Keep both previews, v0.4.1, every existing branch and the original automated receipts intact. Do not change authentication method, codec, clock role, firewall rules, settings or credentials as an unverified workaround. Upstream publication and #25 replacement remain separately gated.
