# Retry DNS-SD resolution with bounded deadlines and operation ownership

A missing, failed or null DNS resolve completion leaves its slot occupied while renewed advertisements remain present, preventing another attempt. This change separates advertised names, cache TTL, request token/epoch/deadline and resource ownership, with five-second logical deadlines and bounded retry/backoff.

Each production resolve runs in a bounded helper that dispatches before WPF/application services. The parent roots its exact child before Start and retains capacity until exit is successfully observed. Browse uses its separate terminal 1223/tracked-section contract. Old tokens/epochs cannot publish or retire newer work; admission caps include unsettled owners. Selected unavailable interfaces continue to pause without fallback.

The implementation preserves full TXT-change catalog publication, native case-insensitive keys and exact values across the helper wire, and explicit LF framing at the existing reply-byte boundary. It adds gated process-cleanup and actual WPF blocked-Start/Cancel regressions, while retaining configuration schema 2 and playback JSONL v1 compatibility.

Validation on `b8ee79511e2fa41ee02651dad83d50e008f9f680`, original base `cf420d97700463c56adc31408510e8336e99e51c`:

- SDK 10.0.401/10.0.402: warning-free Release build, forced locked restore, 303 Core and 113 focused tests each; unchanged locks and consumed ILLink analyzer.
- Seven controls fail before correction; all eight corrected cases pass, including the unchanged/reordered control.
- WPF: focused 16, controlled invalid-culture refusal and six observed catalog/format combinations × 580; synchronous-DNS mutation fails bounded startup.
- Actual synthetic helper processes through apphost and dotnet/DLL; LF-only producer verified against old CRLF output.
- [Hosted Windows qualification](https://github.com/ThatMarko/AirFlash/actions/runs/38471035620): 49 native (two inherited ignores), 303 Core, native checks/build/compatibility and synthetic helpers.

Real DNS/receiver reliability, physical capture/endpoints and Windows 10 execution are not qualified. Browse tests observe protected managed sections/tracked calls, not arbitrary native return frames.
