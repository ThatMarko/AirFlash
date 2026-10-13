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
- Cover inline/missing/late callbacks, failed Start/Cancel/Kill/exit observation, both timeout/completion orders, expiry/removal/new PTR, old tokens/epochs, Restart/Dispose and repeated bounded attempts. Assert exact settlement and that observer failures or reentrant publication do not abandon detached work.

## Verification and limits

A historical [cancellation/publication fixture failure](https://github.com/ThatMarko/AirFlash/actions/runs/38134996290) was investigated separately from its [later documentation-only green run](https://github.com/ThatMarko/AirFlash/actions/runs/38135036967). Controlled scheduling exposed a fake cancellation drainer synchronously joining its own callback; [the fixture correction](https://github.com/ThatMarko/AirFlash/commit/68cae6f) exercises both deferred-drain winners. That establishes a fixture defect, not a production deadlock or a Windows callback-loss diagnosis.

Logical-state and resource fixtures, plus actual synthetic helper-process checks, provide the relevant verification paths. Browse fixtures establish settlement of tracked native calls and managed callback sections; they do not independently establish native callback return-stack quiescence beyond the documented terminal callback contract. Earlier in-process resolve reclamation and a bounded retained-owner prototype were superseded; their apparent zero-owner results do not establish the helper design's lifetime safety. Final standalone branch qualification must bind results to its exact commit. Combined-fork test/package results are not standalone branch proof. Real-network reliability, receiver behavior and helper startup cost on every machine remain unqualified.

## Prepared implementation and executed verification

The prepared implementation is [codex/fix-discovery-resolve](https://github.com/ThatMarko/AirFlash/tree/277efc5b389acd2b9699c9b25baf83450274096f), commit `277efc5b389acd2b9699c9b25baf83450274096f`, independently based on the original stable source above. Independent final source/regression review found no unresolved findings.

Fresh standalone Release build passed with zero warnings/errors; all **295 Core tests** passed with no skips. Separate isolated WPF runs passed **568 assertions each in 3 catalog/region combinations**, covering English, Simplified Chinese and Traditional Chinese. These are this branch’s results, separate from combined-fork qualification.

Six actual synthetic DNS helper processes also passed reply/watchdog/kill/parent-settlement checks with zero parent native owners. No live DNS or receiver incident was used.
