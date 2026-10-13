# Retry stalled DNS-SD resolution with finite deadlines and explicit operation ownership

## Current behavior

Reviewed original stable source: `cf420d97700463c56adc31408510e8336e99e51c`.

Discovery starts one resolve for each advertised instance and keeps its slot while that advertisement remains present. A missing completion, an error completion, or a null result does not schedule another attempt. Renewed positive PTR records extend the advertisement expiry while the existing slot suppresses another resolve. A receiver can therefore remain unresolved despite continuing advertisements.

This is a source-derived failure path, not a claim that Windows drops callbacks in a particular network incident. Current stable already removes resolve slots on PTR removal, advertisement expiry and browse restart; it should not be described as unconditionally stuck until application restart.

Relevant original source:

- [PTR handling and existing-slot suppression](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L129).
- [Error/null completion and retained slot](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L166).
- [Advertisement expiry and operation disposal](https://github.com/Ding-Kyoma/AirFlash/blob/cf420d97700463c56adc31408510e8336e99e51c/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L187).

## Reproduction and expected behavior

Use an injected DNS-SD boundary and monotonic clock: advertise a synthetic instance, withhold its resolve completion, then renew the PTR before its TTL expires. In current stable, there is no resolve deadline or retry while the slot survives. Repeat with an error or null completion. No receiver or live network is needed to exercise this control flow.

Expected behavior is a finite logical attempt followed by bounded retry/backoff while the name remains advertised. Cache TTL, advertisement state, pending request token, browse epoch and resource ownership must remain separate. An obsolete callback must neither publish stale records nor clear a newer attempt, including within the same epoch.

## Bounded fix and acceptance

- Add finite initial/refresh deadlines, bounded backoff and coordinator/per-name/shared owner limits. Preserve selected-interface pause without fallback, manual receiver persistence and existing catalog identities.
- Run each native resolve in a finite helper that dispatches before WPF, logging, settings or audio initialization. Root its owner before starting it. An unstarted owner with no associated process may settle directly; once a child may have started, release capacity only after successfully observing that exact child's exit. A reply, deadline, cancel/kill result or failed exit wait is insufficient. Retain uncertain owners within the shared bound.
- Keep browse cancellation separate. [Microsoft documents a final `ERROR_CANCELLED` browse callback](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsservicebrowsecancel); settle its inputs only after the terminal indication and the implementation's tracked native calls/managed callback sections have ended. [ResolveCancel](https://learn.microsoft.com/en-us/windows/win32/api/windns/nf-windns-dnsserviceresolvecancel) provides no equivalent callback-quiescence guarantee, so a successful-looking return alone must not justify reclaiming resolve inputs.
- Preserve full record/TXT change notifications and case-insensitive TXT-key lookup across the helper wire without changing values. Unchanged/reordered metadata must not republish; ambiguous case-folded wire keys are rejected as invalid replies. Match the private helper writer and bounded reader with explicit LF framing.
- Cover inline/missing/late callbacks, failed Start/Cancel/Kill/exit observation, both timeout/completion orders, expiry/removal/new PTR, old tokens/epochs, Restart/Dispose and repeated bounded attempts. Assert exact settlement and that observer failures or reentrant publication do not abandon detached work.

## Verification and limits

A historical [cancellation/publication fixture failure](https://github.com/ThatMarko/AirFlash/actions/runs/38134996290) was investigated separately from its [later documentation-only green run](https://github.com/ThatMarko/AirFlash/actions/runs/38135036967). Controlled scheduling exposed a fake cancellation drainer synchronously joining its own callback; [the fixture correction](https://github.com/ThatMarko/AirFlash/commit/68cae6f) exercises both deferred-drain winners. That establishes a fixture defect, not a production deadlock or a Windows callback-loss diagnosis.

Logical-state and resource fixtures, plus actual synthetic helper-process checks, provide the relevant verification paths. Browse fixtures establish settlement of tracked native calls and managed callback sections; they do not independently establish native callback return-stack quiescence beyond the documented terminal callback contract. Earlier in-process resolve reclamation and a bounded retained-owner prototype were superseded; their apparent zero-owner results do not establish the helper design's lifetime safety. Final standalone branch qualification must bind results to its exact commit. Combined-fork test/package results are not standalone branch proof. Real-network reliability, receiver behavior and helper startup cost on every machine remain unqualified.

## Prepared implementation and executed verification

The prepared implementation is [codex/fix-discovery-resolve](https://github.com/ThatMarko/AirFlash/tree/b8ee79511e2fa41ee02651dad83d50e008f9f680), commit `b8ee79511e2fa41ee02651dad83d50e008f9f680`, independently based on original `cf420d97700463c56adc31408510e8336e99e51c`. Independent final source, regression, API-contract and integration reviews are clear. The change preserves the original full TXT notification/key semantics while adding bounded resolve lifecycle; it changes neither playback JSONL v1 nor configuration schema 2.

Exact final-head Release solution builds pass with zero warnings/errors on **SDK 10.0.401 and 10.0.402**. Each passes forced locked restore, **303 owning Core / 113 focused discovery tests**, unchanged lock hashes and an active single-file analyzer. The branch explicitly references the already-locked ILLink 10.0.12 build dependency, preserving SDK roll-forward; before that shared correction, its 10.0.402 locked restore failed NU1004.

Controlled unchanged-feature runtime assertions fail **seven cases** for TXT-only notification, helper key semantics and premature fixture cleanup; the unchanged/reordered metadata control passes. All eight pass after correction. A compiled synchronous-DNS mutation fails the actual WPF startup wait while a Start barrier is held; the normal queued path passes dispatcher/thread/ownership checks and joins cleanup. Earlier cancellation fixture failures remain separate from these controls.

Fresh isolated WPF passes **16 focused dispatcher assertions**, controlled invalid-culture refusal and **580 assertions in each of six combinations**: en-US/zh-CN/zh-TW UI catalogs crossed with en-US/zh-CN formatting cultures. Reports verify observed cultures and exact catalog restoration. Earlier three-catalog results used default formatting culture and do not establish this matrix.

Six actual synthetic DNS-helper children pass through each apphost and dotnet/DLL host style, covering replies, watchdog, kill and observed parent settlement. Raw output proves LF-only framing; actual old-helper output used CRLF. [Exact-source hosted Windows qualification](https://github.com/ThatMarko/AirFlash/actions/runs/38471035620) independently checks out this commit and passes **49 native / 303 Core**, native lint/build/compatibility and six synthetic helper checks. Two inherited native tests remain ignored: recording-endpoint qualification and the thirty-minute localhost soak. These are this standalone branch's results; combined-fork qualification is separate.

No live DNS, physical receiver, capture, local endpoint, personal configuration or credential store is used. Real DNS delivery, helper startup cost on every machine, arbitrary native return-stack quiescence and actual Windows 10 execution remain unqualified. No release or upstream publication is performed by preparation.
