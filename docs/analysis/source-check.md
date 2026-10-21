# Source check

## Current-source boundary — 2026-10-21

Current stable comparison is `439de626`, with schema 4 and the all-author features retained in combined `6c1e2ef`. The current readiness overlay maps all seventeen report contracts and separates already-upstream A/C/F/retention scope from remaining work. Earlier unexplained raster failures, physical-device/power-loss limits and unchanged-tree native provenance remain explicit. Controlled fixture evidence establishes its asserted behavior, not a universal HomePod cause or cure.

[Current source, acceptance and executed limits](../issues/PR-READINESS-2026-10-21.md) supersede older current/open/schema labels below. The remaining source links and test counts retain their recorded historical baselines; this documentation refresh claims no new test execution or upstream publication.

## Discovery preparation supplement — 2026-10-13

Current fork code `c5ddb894e8bf0f977a94920ff26878c3cf5e1004` integrates standalone D `b8ee79511e2fa41ee02651dad83d50e008f9f680`, based on original `cf420d97700463c56adc31408510e8336e99e51c`. The old unsupported claim that every TXT-only refresh notifies subscribers is superseded for current c5ddb89: full record/TXT equality now restores original stable publication. Helper key lookup remains case-insensitive through JSON with exact values. Meaningful consumer assertions and unchanged-source red controls support these corrections; neither real DNS callback reliability nor arbitrary native return-stack quiescence is established. Source, carrier workflow, compiled binary and actual execution stay separately identified.

[D acceptance](../issues/GROUP-D-ACCEPTANCE.md) records source, tests, reviews and limits. The earlier source map below retains its pinned historical baseline.

This page checks claims against **fork `main` at `06237873dcb6394eb35e97a017c536843bd7743d`**, reviewed on 2026-10-12. Its runtime matches `8989b36` and qualified `4ae1db4`; it includes original stable `cf420d97700463c56adc31408510e8336e99e51c` and the 17 reported fixes. The original `41190e0` audit is history, not a statement that its repaired defects persist. [Audit](audit.md) records independent review of every analysis page; [verification](verification.md) separates actual test layers and [current acceptance](../issues/CONTINUATION-ACCEPTANCE.md) binds executed evidence.

Deleted speculative reports, unrelated implementations, proposal text and local incident correlation are not source evidence. Development `67435a4` remains excluded. Current source facts, controlled regression observations, CI/artifact results and unmeasured hardware behavior stay distinct.

## Supported, with the stated bounds

| Claim | Current evidence and qualification |
| --- | --- |
| Discovery availability does not revoke an owned attempt | [AppViewModel](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/ViewModels/AppViewModel.cs#L301) retains Connecting/Pairing/Streaming/Standby, active controls and complete transport; new unavailable/incomplete starts remain rejected |
| Delayed work requires atomic authority | [SessionController](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/SessionController.cs#L181) captures owner/intent/snapshot together and revalidates under serialization, including after cleanup |
| Automatic candidate fallback precedes ranking | [ReceiverSelection](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/ReceiverSelection.cs) filters attempts/eligibility before last-used/name/id; only admitted work changes attempts/last-used |
| Explicit manual endpoints remain exact | The same resolver preserves manual/group selection, rejects ambiguous discovered ownership and has no address-only fallback; credential keys stay separate |
| Selected missing/down adapter pauses without fallback | [DiscoveryInterface](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/Receiver.cs) uses index zero only for empty selection |
| Logical resolve lifetime is finite | [WindowsDiscovery](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Services/WindowsDiscovery.cs) has five-second deadlines, bounded retry, current tokens/epochs and separate advertisement/cache/owner state |
| Cache uses PTR lifetime | Positive PTR TTL is capped at one day; renewed PTR extends cached expiry; resume/restart cache has ten seconds, not fixed ninety seconds |
| Resolve ownership needs process exit | [ResolveProcessDnsServiceApi](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Services/ResolveProcessDnsServiceApi.cs) roots exact children before Start; only observed exact exit releases started owners, maximum eight globally |
| Browse ownership has tracked settlement conditions | [NativeDnsServiceApi](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.App/Services/NativeDnsServiceApi.cs) requires terminal `1223` or proved nonpending admission, zero tracked Start/Cancel calls and zero protected managed callback sections; freeing can occur inside the reverse P/Invoke return frame, so full native-stack quiescence is not established |
| Settings recover without destructive defaults replacement | [SettingsStore](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/SettingsStore.cs) captures bytes once, keeps obligations through both Loads, validates loaded values and blocks future schema |
| Mute restores exact prior bit best effort | [MuteRecoveryCoordinator](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/MuteRecoveryCoordinator.cs) records owned playback intent before mutation; normal restore follows cleanup; conservative pending recovery survives interruption |
| Actual recording endpoints are excluded from mute access | Opened identity/direction governs; catalog label alone cannot authorize Get/Set/journal ownership |
| Authentication selection preserves storage errors | [credentials.rs](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/credentials.rs) opens once, only initial NotFound means absence, and bounds reads; DPAPI/parsing/identity/verify errors do not choose transient |
| JSONL remains the process boundary with asymmetric limits | [EngineConnection](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/EngineConnection.cs) and [main.rs](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/main.rs): native input bytes include newline; desktop output validation counts characters after allocation |
| Defined empty-correlated IPC error is terminal | [EngineEvents](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/desktop/AirFlash.Core/EngineEvents.cs) requires exact v1 `error`/`command_error`/`ipc`, `retryable: false`, null `host` and empty-string ids before ordinary session filtering |
| EQ outcome reflects retained preparation | [Equalizer mailbox](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/equalizer.rs) distinguishes accepted/idempotent/stale/conflict and returns stored preparation; full gain/bypass/coefficients/headroom participate, not only raw controls |
| Padding counts are local output-rate slots before EQ | [live.rs](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/live.rs) counts shared missing slots/wholly padded 352-frame pulls once before speaker fan-out; real captured silence is not missing |
| Worker and command process have different lifetimes | Native error/Stop can end worker without command-loop exit; desktop closes stdin/waits/kills during disposal. Retries create fresh transport identities |
| Feedback/volume serialize RTSP control | Feedback request timing begins after shared mutex acquisition; mutex wait can delay its next request without appearing in its measured 4/12-second interval |
| PTP and capture clocks differ | [clock.rs](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/native/airflash-engine/src/clock.rs) uses SystemTime origin plus Instant; capture/desktop silence use QPC |
| Build preflight precedes avoidable release effects | [dotnet-common.ps1](https://github.com/ThatMarko/AirFlash/blob/06237873dcb6394eb35e97a017c536843bd7743d/scripts/dotnet-common.ps1), build and release workflow check the selected host in desktop/global.json context before reservation/native/cleanup |

Dedicated [Discovery](discovery.md), [Session](session.md), [Settings](settings.md), [Authentication](auth.md), [Control and mute](control-and-mute.md), [Capture](capture-clocks.md), [Equalizer](equalizer.md), [Engine](engine.md), [Desktop shell](desktop-shell.md), [Build/release](build-and-release.md) and [Verification](verification.md) pages give callers/consumers and failure limits.

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
| Every worker completion emits `stopped` or clears the command slot | Pair success emits `paired`; handshake-only emits `handshake_complete`; early cancellation/error/panic differ, and retained command ownership can outlive a worker |
| An acknowledged master-gain command always changes subsequent audio | Live capture rereads gain while producing PCM; finite WAV probe PCM is loaded with its initial gain and does not reread later stored gain |
| A terminal Pair event interrupts an open PIN prompt | The sole Pair event reader awaits prompt handling. Existing IPC regression scripts rejection before `pin_required`; prompt-open timing remains source-derived |
| Multi-member Pair rolls back earlier credentials on failure | Successful member credentials save individually; a later failure/cancellation leaves earlier credentials persisted |
| Quit invalidates automatic intent before every await | Shell Quit first awaits Settings Apply/engine initialization; invalidation occurs at view-model disposal entry |
| The connect read deadline cancels the EQ lock/initial send | Its elapsed time includes those waits but enforcement occurs in the later reader; DNS/factory creation precede it |
| All diagnostics use only fake services and fully isolated caches | Audio/discovery/update checks use actual services; self/compat engine extraction uses the normal local engine cache |
| Performance `ok: true` means every timing target passed | UI performance reports target booleans separately and returns success without enforcing them |
| Every changed resolved TXT field immediately updates subscribers | `Describe` omits `gpn`, `igl`, `pk` and `cn`; those fields update cached records but alone do not request Changed/log publication |
| SDK preflight prevents every later reserved-build failure | It prevents selected-SDK failure effects; later native/Python/MSI work can fail after reservation |

## Reading a gap report

First establish the running build. A missing row followed by zero reconnects was compatible with historical `41190e0` discovery-driven Stop, but current B retains owned attempts. Neither observation identifies the snapshot cause or excludes an independent engine fault. Record epoch/restart reason, canonical id, desktop ownership/intent, engine session/fault sequence and exact binary before assigning cause.

High underruns with no drops can reflect idle loopback or another local shortage. Dropped frames include overflow and stale-frame discard during schedule recovery. Real captured silence produces neither padded-frame count nor whole-padding count. Optional absent metrics are unavailable, not zero. Neither metric diagnoses network loss.

A feedback_delayed warning is nonfatal but can precede hard timeout/status failure; mutex wait is outside its recorded request duration. Mute restoration/pending recovery also cannot be inferred from the receiver row alone.

## Completed integration qualification — 2026-10-12

Earlier `8989b36` local Release 667 / 124 Python / lint / build and normal CI are green. Exact reviewed `06237873` [normal CI](https://github.com/ThatMarko/AirFlash/actions/runs/38329156167) also succeeded with 667 Core, 124 Python/lint and 69 native passes/two ignored, discovery stability, bundle audit and packaged startup. Code-identical `4ae1db4` owns the full hosted package/UI/DPI/MSI qualification: six 1,059-assertion portable pairs plus three observed 125% DPI 40-assertion runs. Recording qualification/thirty-minute localhost soak remain excluded. No real receiver/endpoint/credential operation ran in this documentation review; [verification](verification.md) distinguishes source equality from a new execution.

## Additional packaged UI finding — 2026-10-12

Earlier `dc521ae` DLL matrix was green, but its later portable Monitor assertion failed after 818 checks. Saved glyph/viewport geometry showed fractional-DPI unrounded Border bounds, distinct from earlier real counter clipping. H `6855dfe` uses finite parent layout slots without widening tolerance. Exact hosted 125% controls pass; restored legacy bounds and minimum-width NoWrap mutations fail their intended assertions. Full `107174a`, then `4ae1db4`, portable matrices pass; [H acceptance](../issues/GROUP-H-ACCEPTANCE.md) preserves source/hash/negative evidence.

Normal CI `107174a` separately failed an inherited EQ rollback fixture. Its quiet-fake read deadline was reproduced on unchanged runtime through actual Error/disposal signals; original CI cause remains inferred without captured state. F `421e1ea` gates real send/cancellation/live-session premises and adds finite-deadline retained-preview coverage. `4ae1db4` and `8989b36` qualification pass; no runtime deadline/debounce change was made. [F acceptance](../issues/GROUP-F-ACCEPTANCE.md) preserves the failed controls.

## Historical source audits

The immutable [earlier source-check](https://github.com/ThatMarko/AirFlash/blob/8989b3609dd1294f4d1d6eb1502f345cc22bc382/docs/analysis/source-check.md) preserves `41190e0` and `9e894ab` tables plus dated continuation snapshots. Those old accepted passive-Stop/memory-only-mute/missing-tests claims describe their source, not current main. [Audit](audit.md#original-audit--2026-10-06) retains the initial source-only/toolchain boundaries. No deleted speculative note, private household log or unrelated upstream issue is imported as evidence.
