# Source check

This page checks claims against **fork `main` at `8989b3609dd1294f4d1d6eb1502f345cc22bc382`**, reviewed on 2026-10-12. It includes original stable `cf420d97700463c56adc31408510e8336e99e51c` and all 17 report fixes. The original `41190e0` audit is history, not a statement that its repaired defects persist. [Audit](audit.md) gives recursive coverage and [current acceptance](../issues/CONTINUATION-ACCEPTANCE.md) binds executed evidence.

Deleted speculative reports, unrelated implementations, proposal text and local incident correlation are not source evidence. Development `67435a4` remains excluded. Current source facts, controlled regression observations, CI/artifact results and unmeasured hardware behavior stay distinct.

## Supported, with the stated bounds

| Claim | Current evidence and qualification |
| --- | --- |
| Discovery availability does not revoke an owned attempt | [AppViewModel](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L301) retains Connecting/Pairing/Streaming/Standby, active controls and complete transport; new unavailable/incomplete starts remain rejected |
| Delayed work requires atomic authority | [SessionController](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SessionController.cs#L181) captures owner/intent/snapshot together and revalidates under serialization, including after cleanup |
| Automatic candidate fallback precedes ranking | [ReceiverSelection](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/ReceiverSelection.cs) filters attempts/eligibility before last-used/name/id; only admitted work changes attempts/last-used |
| Explicit manual endpoints remain exact | The same resolver preserves manual/group selection, rejects ambiguous discovered ownership and has no address-only fallback; credential keys stay separate |
| Selected missing/down adapter pauses without fallback | [DiscoveryInterface](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/Receiver.cs) uses index zero only for empty selection |
| Logical resolve lifetime is finite | [WindowsDiscovery](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Services/WindowsDiscovery.cs) has five-second deadlines, bounded retry, current tokens/epochs and separate advertisement/cache/owner state |
| Cache uses PTR lifetime | Positive PTR TTL is capped at one day; renewed PTR extends cached expiry; resume/restart cache has ten seconds, not fixed ninety seconds |
| Resolve ownership needs process exit | [ResolveProcessDnsServiceApi](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Services/ResolveProcessDnsServiceApi.cs) roots exact children before Start; only observed exact exit releases started owners, maximum eight globally |
| Browse ownership needs terminal/frame completion | [NativeDnsServiceApi](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.App/Services/NativeDnsServiceApi.cs) retains pending inputs until documented terminal status `1223` and ended native/callback frames; result payload freeing is separate |
| Settings recover without destructive defaults replacement | [SettingsStore](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/SettingsStore.cs) captures bytes once, keeps obligations through both Loads, validates loaded values and blocks future schema |
| Mute restores exact prior bit best effort | [MuteRecoveryCoordinator](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/MuteRecoveryCoordinator.cs) records owned playback intent before mutation; normal restore follows cleanup; conservative pending recovery survives interruption |
| Actual recording endpoints are excluded from mute access | Opened identity/direction governs; catalog label alone cannot authorize Get/Set/journal ownership |
| Authentication selection preserves storage errors | [credentials.rs](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/credentials.rs) opens once, only initial NotFound means absence, and bounds reads; DPAPI/parsing/identity/verify errors do not choose transient |
| JSONL remains the process boundary with asymmetric limits | [EngineConnection](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/EngineConnection.cs) and [main.rs](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/main.rs): native input bytes include newline; desktop output validation counts characters after allocation |
| Defined empty-correlated IPC error is terminal | [EngineEvents](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/desktop/AirFlash.Core/EngineEvents.cs) requires exact v1 `error`/`command_error`/`ipc`, `retryable: false`, null `host` and empty-string ids before ordinary session filtering |
| EQ outcome reflects retained preparation | [Equalizer mailbox](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/equalizer.rs) distinguishes accepted/idempotent/stale/conflict and returns stored preparation; full gain/bypass/coefficients/headroom participate, not only raw controls |
| Padding counts are local output-rate slots before EQ | [live.rs](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/live.rs) counts shared missing slots/wholly padded 352-frame pulls once before speaker fan-out; real captured silence is not missing |
| Worker and command process have different lifetimes | Native error/Stop can end worker without command-loop exit; desktop closes stdin/waits/kills during disposal. Retries create fresh transport identities |
| Feedback/volume serialize RTSP control | Feedback request timing begins after shared mutex acquisition; mutex wait can delay its next request without appearing in its measured 4/12-second interval |
| PTP and capture clocks differ | [clock.rs](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/native/airflash-engine/src/clock.rs) uses SystemTime origin plus Instant; capture/desktop silence use QPC |
| Build preflight precedes avoidable release effects | [dotnet-common.ps1](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/scripts/dotnet-common.ps1), build and release workflow check the selected host in desktop/global.json context before reservation/native/cleanup |

Dedicated [Discovery](discovery.md), [Session](session.md), [Settings](settings.md), [Authentication](auth.md), [Control and mute](control-and-mute.md), [Capture](capture-clocks.md), [Equalizer](equalizer.md) and [Engine](engine.md) pages give callers/consumers and failure limits.

## Rejected or not established by this tree

| Claim | Verified limit |
| --- | --- |
| Repeated resolve logs prove a network event or receiver reboot | Cache absence **or** changed description can log; no event cause/transport condition follows |
| Normal latency is 2000 ms, or 150 ms is a tested firmware-safe margin | Settings presets remain 120/200/500 ms, custom 0–2000 ms; separate finite-probe default is 150 ms. No firmware jitter-margin measurements |
| Shared address proves stereo ownership | Explicit manual endpoint remains exact; discovered redirection requires unique canonical/broadcast ownership |
| A fresh snapshot read alone fixes stale caller control | It must retain original owner/intent and pass serialized validation; snapshot-reference changes from metrics are not ownership changes |
| Restart always publishes empty before any new result | Current scheduling/cache retention permits inline replacement results; explicit interface clearing is separate |
| Timeout/result/Cancel success safely frees resolve inputs | It does not establish native quiescence. Child address-space exit supplies current production boundary |
| Recorded prior mute proves no later user/application change | It cannot; recovery states/choices remain conservative, with durability and ambiguity limits |
| Recording endpoint metadata alone authorizes muting | Actual opened endpoint identity/direction is authoritative; recording/unknown eligibility remains protected |
| Store lock coordinates all file editors | It covers this store only; ordinary Save after valid Load does not catch every external future-schema change |
| Authentication faults are all final | Structured auth crypto/protocol failures are final; auth-phase network failures can remain retryable |
| Credential read/verify failure can select transient | Current connection branches once on preserved lookup outcome; only genuine absence/no accessory key takes transient |
| An EQ reply confirms DSP completion | It confirms preparation acceptance/idempotence; audio-thread adoption and 20 ms crossfade completion remain unacknowledged |
| Positive EQ sequence always means success | Older/conflicting sequences return a compatible nonfatal error while retaining mailbox state; preparation metadata belongs to accepted/idempotent success replies |
| Capture queue is lock-free or padding is network loss | Queue is mutex-protected; missing output frames are local substitution, not network/acoustic observations |
| Underrun count measures exact silence seconds | Count×352/rate describes packet slots containing some fill, not exact padded slots; use optional padded-frame accounting |
| Input capture counts give an output fill percentage | Input and output rates/resampling differ; no such ratio is exposed |
| PTP bind proves default-route egress or clock lock | Wildcard bind/send_to depends on Windows routing; successful bind/send is not synchronization/acoustic evidence |
| Feedback/retransmits are one UDP RTCP flow on 7000 | Feedback is encrypted RTSP POST; data/control use negotiated UDP ports; RTSP port can differ |
| History/pending limit 512 means four seconds guaranteed recovery | History age/local resend deadlines also apply; no fixed recoverable-audio guarantee |
| LATE_LIMIT is the speaker playout deadline | It is sender schedule lateness; live recovery at 60 ms, finite probe failure |
| Missing timeBeginPeriod proves stutter/scheduling quantum | Actual effective timer resolution is not measured; sleep/MMCSS source alone establishes no cause |
| HomePod OS 27 has a proved tolerance change | Source records capabilities/version but has no measured firmware branch/table |
| One green rerun resolves an intermittent failure | D and EQ failures retain raw evidence and controlled mechanism; unchanged-runtime green is not repair |
| Green fake/native/UI CI establishes real-device reliability | Hardware/network/acoustic and physical power-loss qualification remain separate |

## Reading a gap report

First establish the running build. A missing row followed by zero reconnects was compatible with historical `41190e0` discovery-driven Stop, but current B retains owned attempts. Neither observation identifies the snapshot cause or excludes an independent engine fault. Record epoch/restart reason, canonical id, desktop ownership/intent, engine session/fault sequence and exact binary before assigning cause.

High underruns with no drops can reflect idle loopback or another local shortage. Dropped frames include overflow and stale-frame discard during schedule recovery. Real captured silence produces neither padded-frame count nor whole-padding count. Optional absent metrics are unavailable, not zero. Neither metric diagnoses network loss.

A feedback_delayed warning is nonfatal but can precede hard timeout/status failure; mutex wait is outside its recorded request duration. Mute restoration/pending recovery also cannot be inferred from the receiver row alone.

## Completed integration qualification — 2026-10-12

Current `8989b36` local Release 667 / 124 Python / lint / build and exact-main normal CI are green. Code-identical `4ae1db4` passes 667 Core in Debug/Release and full hosted native/package/UI/DPI/MSI qualification: six 1,059-assertion portable pairs plus three observed 125% DPI 40-assertion runs. 69 native tests pass with recording qualification/thirty-minute localhost soak excluded. No real receiver/endpoint/credential operation ran in this documentation review.

## Additional packaged UI finding — 2026-10-12

Earlier `dc521ae` DLL matrix was green, but its later portable Monitor assertion failed after 818 checks. Saved glyph/viewport geometry showed fractional-DPI unrounded Border bounds, distinct from earlier real counter clipping. H `6855dfe` uses finite parent layout slots without widening tolerance. Exact hosted 125% controls pass; restored legacy bounds and minimum-width NoWrap mutations fail their intended assertions. Full `107174a`, then `4ae1db4`, portable matrices pass; [H acceptance](../issues/GROUP-H-ACCEPTANCE.md) preserves source/hash/negative evidence.

Normal CI `107174a` separately failed an inherited EQ rollback fixture. Its quiet-fake read deadline was reproduced on unchanged runtime through actual Error/disposal signals; original CI cause remains inferred without captured state. F `421e1ea` gates real send/cancellation/live-session premises and adds finite-deadline retained-preview coverage. `4ae1db4` and `8989b36` qualification pass; no runtime deadline/debounce change was made. [F acceptance](../issues/GROUP-F-ACCEPTANCE.md) preserves the failed controls.

## Historical source audits

The immutable [earlier source-check](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/analysis/source-check.md) preserves `41190e0` and `9e894ab` tables plus dated continuation snapshots. Those old accepted passive-Stop/memory-only-mute/missing-tests claims describe their source, not current main. [Audit](audit.md#original-audit--2026-10-06) retains the initial source-only/toolchain boundaries. No deleted speculative note, private household log or unrelated upstream issue is imported as evidence.
