# Reliability review against original Main — November 5

This preserves the November 5 review snapshot. The November 7 addendum below records a later field follow-up on the implementation branch; it does not turn historical qualification or publication state into current evidence.

The adversarial review covered every changed hunk and affected assertion in all **103 changed paths** between original Main `11b7d18ad89fe2bbab43dfbea1191609e9457177` and the prior candidate `66b32942b0a1a42c8fd0457217ca2e9534f21bc6`. It included deletions and replacement consumers, rather than selecting only recent fork commits. The original v0.6.2 release points to `1717dab91a4091717fe370d188362d3a05202957`; the subsequent Main commit changes only the compact panel and its UI fixture. No native/audio difference separates those two original refs.

The fixes reached `270453895b583b55b4422a34c74aec7cdd76daa9`. Original Main then advanced to `a52803dac1696a56b7067356e09bd7e7ebb9dab6` during final verification: one four-line tray transparency/corner change and original 0.6.3 release notes. Those changes were reviewed and cleanly merged as `1cee7555ca298f01da58bd085c23a99546b989fe`. A subsequent fixture-publication correction produces current candidate `367dbb3dc6163b2bb9e3772a267d33314a05a4f3` on `codex/reliability-refresh-v062`. The correction commits and merge received independent source/assertion review. The final diff against current original Main still has 103 paths and 38 outgoing commits. Every outgoing tree excludes fork-only documents; its documentation is either the prior original tree or the exact newly integrated original release notes. The final documentation tree matches current original Main. This report and the unsent publication drafts stay on fork Main.

## Findings and corrections

1. **Fork regression: Grant packet ordering.** An accepted Announce Grant with sender sequence 12 prevented otherwise valid, independently pending Sync 10 and DelayResp 11 Grants from being adopted. A test-only child of `66b3294` reproduced exactly one adopted Grant instead of three, with 313 other library tests passing and the device qualification ignored. [Before control](https://github.com/ThatMarko/AirFlash/actions/runs/42000664181), source `ac122b0ffccdba770b71ddfe1f4c79a8df8bcbd3`.

   `2704538` removes the fork's receive serial-order gate. A sender sequence is not the local request token, and continuous ordering across independent contracts or receiver state changes was not established. Acceptance still requires the known peer/port, exact target, complete TLV framing, valid fields and duration, and an unexpired request of the matching type. Accepting a Grant consumes pending work for that type. New assertions cover reordered types, duplicates without pending requests, multiple TLVs, wrap, and same-identity renewal with an unchanged or restarted sender counter. The serialized replay-counter key remains present as zero for compatibility.

   These guards validate packet and state consistency. An old valid datagram can match a newly opened pending window; this is not cryptographic replay protection. Grants affect subscription bookkeeping and retry traffic, while readiness depends on actual clock exchanges. The controlled failure does not establish the cause of reported HomePod clock or acoustic failures.

2. **Fork regression: persisted gain lost after intent invalidation.** While A streamed, a held save persisted master volume 37 (gain .37). A newer Play for B invalidated the saved snapshot's authority; queued discovery made B unavailable, so Play was rejected. A retained its worker and ownership, but the successful save cleared dirty work and no later gain command was sent. A test-only child of `66b3294` passed the four decisive setup/ownership/save assertions, then failed the actual .37 gain assertion. [Before control](https://github.com/ThatMarko/AirFlash/actions/runs/42000856056), source `c65efe1aa44afc99de809634f7fa206252837021`.

   `0439a86` separates unsaved edits from saved settings awaiting live application. A retry captures current settings and authority without repeating persistence. The original decisive method remains byte-identical after newline normalization. Additional controls cover the adjacent Apply consumer, exact save counts, canonical identity migration and edits made while live application is held. Discovery consuming a dirty panel gain was already possible on original Main; that adjacent correction is not attributed to the fork. No ownership guard was weakened, and no new start is admitted by a gain retry.

3. **Verification settlement.** The queued native-start fixture signaled inside Start before the call actually returned. `61f76f2` models cancellation joining that return and waits for all settlement subscribers after coordinator removal, then asserts zero retained owners and exact-once release. The local 114-test DNS subset passes. This is fake-boundary evidence, not a claim about native callback quiescence.

4. **Verification isolation and resource scope.** `61f76f2` supplies the missing isolated-verification argument in settings recovery. Discovery performance output now labels both successful and failed resource reports as desktop-parent-only and explicitly excludes helper resource usage. Existing numeric fields are retained. Owner counts do not establish total helper CPU/memory cost; no performance-equivalence claim or live network performance run is made.

## Verification

The corrected managed solution builds locally in Release with SDK 10.0.402, zero warnings/errors, and all 1,247 Core tests passing. Both failed controls above verify the complete source manifest against the corresponding Git archive, unchanged runtime production files, artifact digest, and extracted evidence bytes. Neither is dismissed because an older broad run was green.

The pre-merge corrected source `2704538` passed [headless run 42003863113](https://github.com/ThatMarko/AirFlash/actions/runs/42003863113): 1,247 Core, 252 Python and 362 Rust tests, with two intentional native ignores. Archive/source verification confirms all 305 tracked source entries and all 19 extracted evidence files. Its full desktop run [42003863282](https://github.com/ThatMarko/AirFlash/actions/runs/42003863282) failed eight cells at the same new canonical-identity UI assertion. The other 14 cells passed 2,835 assertions. All 1,125 extracted files and all source entries were verified. The actual-gain regression and identity-consumer gain/save/worker controls passed before that failure. This failure remains distinct from the earlier lost-gain control.

The failed assertion combined the panel identity, saved sample rate and removed legacy key. Source tracing establishes legitimate same-owner promotion in the controller and that the view model queues snapshot publication. The fixture immediately read the panel snapshot without joining that dispatcher work. `367dbb3` adds the existing dispatcher drain and keeps every canonical identity/preference assertion. Production code and the original lost-gain method are unchanged. The focused and full runs below execute that exact correction; an unrelated later green run would not establish it.

Merged-source headless run [42005061339](https://github.com/ThatMarko/AirFlash/actions/runs/42005061339) passed; its full isolated desktop run [42005064703](https://github.com/ThatMarko/AirFlash/actions/runs/42005064703) reproduced the same eight canonical-publication failures, with the other 14 cells passing. Both archives and complete source manifests were verified. Those runs use `1cee755` and precede the fixture correction.

Final exact-source results on `367dbb3`:

- [Headless run 42006160255](https://github.com/ThatMarko/AirFlash/actions/runs/42006160255): 1,247 Core, 252 Python and 362 Rust tests pass, with two intentional native hardware/soak ignores. The Release managed solution builds with zero warnings/errors; native lint, release build and compatibility inspection pass. Changed Rust files are formatted; eight unchanged baseline files retain inherited formatting drift. All 305 source entries and 19 extracted evidence files were verified.
- [Focused lifecycle run 42006155445](https://github.com/ThatMarko/AirFlash/actions/runs/42006155445): all 258 assertions pass, including both additional save consumers, canonical identity, exact persistence counts and edits made during application. The only change from `1cee755` is the three-line fixture-publication join; no expectation or runtime implementation was weakened.
- [Full desktop run 42006699186](https://github.com/ThatMarko/AirFlash/actions/runs/42006699186): all 22 cells and 15,189 assertions pass. Verification requires the exact unique routes, requested/observed cultures and languages, both identity-discovery and Apply gain markers, and four measured synthetic 125% DPI catalogs. Five startup guards and six synthetic DNS-helper settlement checks pass. Both source manifests match all 305 entries, binary/assembly receipts remain identical across cells, and all 1,293 extracted files match the verified archive.

The workflow carrier is separately identified as `0922d6bc82ff5349ed5b72c5da9f8e532bd5ce85`; each run requests exact source `367dbb3`. Raw artifacts, source comparisons, failed controls and verified receipts remain in ignored `artifacts/code-review-2026-11-05`. Packaging/MSI lifecycle was not rerun on this corrected head; the earlier `66b3294` package remains historical.

## Limits and publication state

This is changed-source review with deterministic and synthetic qualification, not proof that every source line is correct or that physical playback is better than original v0.6.2. Native hardware qualification and the soak remain intentionally excluded. No real receiver, capture endpoint, personal configuration or credential store was mutated.

The latest authorized local log references an earlier preview run `41940619983`; it does not qualify the corrected candidate. Previous `6.0.x` unofficial preview numbering also sorts above original `0.6.2` for update checks and MSI upgrade ordering. A future official lower-numbered MSI cannot be treated as a direct upgrade from that preview. No new preview/version or release is created by this review.

Original Main is now `a52803d` and is an ancestor of the final candidate. PRs [45](https://github.com/Ding-Kyoma/AirFlash/pull/45), [44](https://github.com/Ding-Kyoma/AirFlash/pull/44), and [41](https://github.com/Ding-Kyoma/AirFlash/pull/41) are open; their proposed changes are not integrated evidence. In particular, #45 concerns the ownership/retry path and needs reconciliation if merged. The umbrella issue and PR remain unsent. Field stability/audio comparison and explicit upstream publication authorization remain required.

## Changed-file coverage

Each row records the reviewer responsible for all changed hunks, including fixture assertions. Direct callers and consumers were traced where relevant. The correction commits were separately reviewed by their author, the lead and an independent specialist. This inventory describes the original 103-path review and remains separate from its executed test results.

<!-- coverage rows are generated from the checked diff inventory -->
| Changed path | Changed-hunk reviewer |
| --- | --- |
| `.github/workflows/release.yml` | Lead integrator |
| `desktop/AirFlash.App/App.xaml` | Lead integrator |
| `desktop/AirFlash.App/App.xaml.cs` | Lead integrator |
| `desktop/AirFlash.App/Program.cs` | Lead integrator |
| `desktop/AirFlash.App/Services/ApplicationCatalog.cs` | Native and IPC specialist |
| `desktop/AirFlash.App/Services/AudioService.cs` | DNS and mute specialist |
| `desktop/AirFlash.App/Services/DnsResolveHelper.cs` | DNS and mute specialist |
| `desktop/AirFlash.App/Services/DnsServiceApi.cs` | DNS and mute specialist |
| `desktop/AirFlash.App/Services/NativeDnsServiceApi.cs` | DNS and mute specialist |
| `desktop/AirFlash.App/Services/ResolveProcessDnsServiceApi.cs` | DNS and mute specialist |
| `desktop/AirFlash.App/Services/ShellShutdown.cs` | Lead integrator |
| `desktop/AirFlash.App/Services/WindowsDiscovery.Windows.cs` | DNS and mute specialist |
| `desktop/AirFlash.App/Services/WindowsDiscovery.cs` | DNS and mute specialist |
| `desktop/AirFlash.App/Services/WindowsDnsSdApi.cs` | DNS and mute specialist |
| `desktop/AirFlash.App/Ui/ControlPanel.xaml` | Lead integrator |
| `desktop/AirFlash.App/Ui/ControlPanel.xaml.cs` | Lead integrator |
| `desktop/AirFlash.App/Ui/MuteRecoveryWindow.cs` | DNS and mute specialist |
| `desktop/AirFlash.App/Ui/PinDialog.cs` | Native and IPC specialist |
| `desktop/AirFlash.App/Ui/SettingsWindow.xaml` | Lead integrator |
| `desktop/AirFlash.App/Ui/SettingsWindow.xaml.cs` | Lead integrator |
| `desktop/AirFlash.App/Verification/AdaptiveLatencyRegression.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.App/Verification/DiscoveryPerf.cs` | DNS and mute specialist |
| `desktop/AirFlash.App/Verification/ForcedLatencyRegression.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.App/Verification/LatencyEntryRegression.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.App/Verification/PairingHintRegression.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.App/Verification/ReceiverSettingsRegression.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.App/Verification/StabilityCheck.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.App/Verification/StopRegression.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.App/Verification/UiDiagnostics.cs` | Lead integrator |
| `desktop/AirFlash.App/Verification/UiDiscoveryLifecycle.cs` | DNS and mute specialist |
| `desktop/AirFlash.App/Verification/UiEqualizer.cs` | Lead integrator |
| `desktop/AirFlash.App/Verification/UiMuteRecovery.cs` | DNS and mute specialist |
| `desktop/AirFlash.App/Verification/UiOwnershipRecovery.cs` | Lead integrator |
| `desktop/AirFlash.App/Verification/UiPairing.cs` | Native and IPC specialist |
| `desktop/AirFlash.App/Verification/UiPairingInputs.cs` | Native and IPC specialist |
| `desktop/AirFlash.App/Verification/UiPerf.cs` | Lead integrator |
| `desktop/AirFlash.App/Verification/UiRegression.cs` | Lead integrator |
| `desktop/AirFlash.App/Verification/UiScrolling.cs` | Lead integrator |
| `desktop/AirFlash.App/Verification/UiSessionLifecycle.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.App/Verification/UiSettingsRecovery.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.App/Verification/UiSmoke.cs` | Lead integrator |
| `desktop/AirFlash.App/ViewModels/AppViewModel.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.App/ViewModels/ErrorDetailsText.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.App/ViewModels/SettingsViewModel.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Core/EngineConnection.cs` | Native and IPC specialist |
| `desktop/AirFlash.Core/EngineEvents.cs` | Native and IPC specialist |
| `desktop/AirFlash.Core/EngineHandshake.cs` | Native and IPC specialist |
| `desktop/AirFlash.Core/FileMuteRecoveryJournal.cs` | DNS and mute specialist |
| `desktop/AirFlash.Core/MuteRecovery.cs` | DNS and mute specialist |
| `desktop/AirFlash.Core/MuteRecoveryCoordinator.cs` | DNS and mute specialist |
| `desktop/AirFlash.Core/PasswordInput.cs` | Native and IPC specialist |
| `desktop/AirFlash.Core/Properties/AssemblyInfo.cs` | Lead integrator |
| `desktop/AirFlash.Core/Receiver.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Core/ReceiverCapabilities.cs` | Native and IPC specialist |
| `desktop/AirFlash.Core/ReceiverSelection.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Core/SessionController.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Core/SessionSettingsContext.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Core/Settings.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Core/Strings.de.json` | Lead integrator |
| `desktop/AirFlash.Core/Strings.zh-TW.json` | Lead integrator |
| `desktop/AirFlash.Core/Strings.zh.json` | Lead integrator |
| `desktop/AirFlash.Tests/AirFlash.Tests.csproj` | Lead integrator |
| `desktop/AirFlash.Tests/CapturePaddingMetricsTests.cs` | DNS and mute specialist |
| `desktop/AirFlash.Tests/DiscoveryLifecycleTests.cs` | DNS and mute specialist |
| `desktop/AirFlash.Tests/ErrorDetailsTextTests.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Tests/IpcConsumerPhaseTests.cs` | Native and IPC specialist |
| `desktop/AirFlash.Tests/IpcFailureTests.cs` | Native and IPC specialist |
| `desktop/AirFlash.Tests/IpcQueryConsumerTests.cs` | Native and IPC specialist |
| `desktop/AirFlash.Tests/ManualDeadlineClock.cs` | Lead integrator |
| `desktop/AirFlash.Tests/MuteRecoveryAdmissionTests.cs` | DNS and mute specialist |
| `desktop/AirFlash.Tests/MuteRecoveryTests.cs` | DNS and mute specialist |
| `desktop/AirFlash.Tests/NativeDnsOperationTests.cs` | DNS and mute specialist |
| `desktop/AirFlash.Tests/PairingPasswordTests.cs` | Native and IPC specialist |
| `desktop/AirFlash.Tests/PairingPromptLifecycleTests.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Tests/PasswordInputTests.cs` | Native and IPC specialist |
| `desktop/AirFlash.Tests/ReceiverIdentityTests.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Tests/ReceiverSelectionTests.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Tests/ResolveProcessTests.cs` | DNS and mute specialist |
| `desktop/AirFlash.Tests/ResolveWireTests.cs` | DNS and mute specialist |
| `desktop/AirFlash.Tests/SessionLifecycleTests.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Tests/SessionSettingsContextTests.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Tests/SessionSnapshotPublicationTests.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Tests/SessionTests.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Tests/ShellShutdownTests.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Tests/SoftwareGroupSettingsTests.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Tests/TestSessionController.cs` | Managed lifecycle specialist |
| `desktop/AirFlash.Tests/WindowsDiscoveryTests.cs` | DNS and mute specialist |
| `native/airflash-engine/src/clock.rs` | Native and IPC specialist |
| `native/airflash-engine/src/clock_tests.rs` | Native and IPC specialist |
| `native/airflash-engine/src/credentials.rs` | Native and IPC specialist |
| `native/airflash-engine/src/live.rs` | Native and IPC specialist |
| `native/airflash-engine/src/session.rs` | Native and IPC specialist |
| `scripts/build-installer.ps1` | Lead integrator |
| `scripts/build-package.ps1` | Lead integrator |
| `scripts/build-release.ps1` | Lead integrator |
| `scripts/dotnet-common.ps1` | Lead integrator |
| `scripts/release-version.ps1` | Lead integrator |
| `scripts/verify-release.ps1` | Lead integrator |
| `tests/test_build_package.py` | Lead integrator |
| `tests/test_dotnet_preflight.py` | Lead integrator |
| `tests/test_release_version.py` | Lead integrator |
| `tests/test_startup_diagnostics.py` | Lead integrator |
| `tests/test_verify_release.py` | Lead integrator |

## November 7 field follow-up — feature branch only

The user reports that preview source `2c6a234c48c84fcd0e986454053cb1a534146b23` works very well at Custom 150 ms with forced latency enabled. Preview `0.3.3-rc.1`, source `644a7d82511d05265ffdf91759c8a03e1756bfee`, is also reported clean at Custom 150 ms with forcing either enabled or disabled; Real-time 120 ms still crackles in both forcing states. These are user listening observations, not measured acoustic results. Source maps both presets to the fixed-target path. Nonforced negotiation may adopt the receiver's returned minimum, so requested labels alone do not establish effective buffering or the crackling's cause.

The same field session reports a native stereo pair intermittently appearing as a standalone member or a 1/2 group. Authorized log inspection finds two distinct device identities and endpoints initially reporting the same nonempty stereo identifier, followed by resolve timeouts for one member. There are no identity-conflict or capacity warning lines in that snapshot. Existing logs do not record cache removals or demand restarts, so they cannot distinguish PTR withdrawal/expiry, the ten-second resumed-cache grace, and missing stereo metadata. Timing relative to panel reopening remains uncertain. Actual device unpairing is not established.

Follow-up source `dd0c2a3f55fcff17bbe7011756ec0d13c048cb65` adds an optional discovery diagnostic provider and a 128-entry memory-only history. Entries capture demand flags, epoch, monotonic/UTC receipt times, accepted metadata, and removal provenance. Correlations use instance-local keyed hashes; raw service names, device identifiers, addresses, TXT values and adapter identifiers are excluded. Overwrite counts make the bounded evidence window explicit. Saved JSON includes the snapshot even without an active engine. No new synchronous disk writes, discovery timers, retry/TTL policies, native ownership rules, stereo completeness rules, audio engine, PTP or latency changes are introduced.

Eleven fake-DNS stereo cells distinguish Stop/resume grace, continuous PTR expiry/goodbye, explicit successful TXT clearing, stale callbacks, reappearance, incomplete admission and concurrent/reentrant snapshots. Seven Core diagnostic cells cover bounds, normalization, correlation, redaction and optional-provider input validation. Full WPF runs also check demand changes without restarting an existing browse and the actual saved export.

Exact-source [Headless run 42411704544](https://github.com/ThatMarko/AirFlash/actions/runs/42411704544) passes 1,338 Core, 372 Rust and 252 Python tests, with two native ignores, plus Clippy, native release/compatibility inspection and a managed build with zero warnings/errors. [Desktop run 42411707184](https://github.com/ThatMarko/AirFlash/actions/runs/42411707184) passes all 18 isolated cells; the seven full regional runs each contain 1,744 checks, including the three discovery export/demand assertions. Downloaded receipts identify exact source `dd0c2a3` rather than the workflow carrier. Independent final source/assertion review found no remaining blocker.

The first desktop build, [42411391621](https://github.com/ThatMarko/AirFlash/actions/runs/42411391621), rejected the new fixture's cleanup await under xUnit1030. The final one-line correction retains the captured xUnit context while suppressing secondary cleanup exceptions. The corresponding old headless run was cancelled after that known compile error. Final green evidence executes the corrected source; it is not an unchanged rerun used to dismiss the failure.

Unofficial preview `0.3.4-rc.1` from exact source `dd0c2a3` passes [packaging run 42412516148](https://github.com/ThatMarko/AirFlash/actions/runs/42412516148), including 1,338 Core tests, six full portable UI cells, three fractional-DPI cells, embedded-engine/package checks, repeated packaging and disposable installer lifecycle. The downloaded archive matches GitHub's digest `6b7b166b6efdc494a4b7b4dafa9dab81c6e3447b2e9d33492012eadab7762b85`; all three binary lengths/hashes match the source-bound manifest. The downloaded app's hash also matches the executable exercised by the portable UI matrix. No release tag or version reservation was created.

These changes remain on `codex/reliability-refresh-v062`. They are diagnostic follow-up and controlled regression coverage, not a demonstrated fix for the physical pair's disappearance or audible crackling. They have not been merged into fork or original Main. No local builds, tests, application launches, real receiver probes or endpoint/settings mutations were run during the user's listening comparison. Upstream publication remains gated.

## November 9 diagnostics follow-up

The user reported clean gameplay and video playback on preview `0.3.4-rc.1` (`dd0c2a3`). The matching export covers 34 minutes 53 seconds at a reported 150 ms engine target. The log records zero session failures or reconnects in that window. The engine reports zero media send errors, sender recoveries or skipped packets. All 163 retransmission requests were served. The log also records 20 recovered clock holdovers and 14 successful silence-probe capture reopens. The retained discovery history includes successful browse resumes and zero record removals. The bounded history had already overwritten 120 entries at export time.

Feature commit `9c48646b73e8624e248ac74a3b7210886e73bb8b` reports adapter availability as unknown while discovery is stopped or a current lookup is outstanding or failed. A successful periodic lookup updates the observation even when the selected interface remains absent. A PTR refresh reports cached lifetime renewal without claiming a fresh endpoint or TXT resolution. DNS history records the first observed failure when an active query fails to start or settles unexpectedly, while keeping timeouts distinct. The native silence-probe comment and test name now describe how nonsilent audio resets the cooldown.

Isolated controls using `dd0c2a3` reproduced stale adapter availability, the overstated PTR event and missing failed-start traces. The fixed source passed 94 focused discovery tests, a Release solution build with zero warnings and errors, and 1,342 Core tests under SDK 10.0.401. Independent review checked the periodic lookup guard and final regression assertions. Original Main `a52803d` was verified as the stable ancestor. The implementation and its verification remain on the feature branch.

Unofficial preview `0.3.5-rc.1` from `9c48646` passed [packaging run 42685707648](https://github.com/ThatMarko/AirFlash/actions/runs/42685707648) with 1,342 Core, 372 Rust and 252 Python tests, two Rust ignores, Clippy, six full portable UI cells, three fractional-DPI cells, embedded-engine checks and disposable installer lifecycle. The downloaded ZIP matches GitHub's digest `46deb1db2529dcbff2c4c4c72fd8839efa578f1ecf14e7e6c2535c5c1bc3bd95`. All three binary hashes match the manifest, and the app hash matches the portable UI receipt.

## November 9 capture reopen follow-up

On a successful capture reopen, queued samples from the prior input can still be waiting for output. The former implementation began its fade at the next packet boundary. With enough queued audio, the fade finished before the first sample from the reopened input, leaving a hard PCM jump inside a later packet. `181159b` moves the splice to that exact queued frame. It preserves the capture reopen policy, queue capacity, forced-silence handling and latency selection.

The waveform regression ran against the unchanged runtime at `9c48646` with only the fixture added. It reproduced a 16,384-LSB adjacent-sample jump on Windows at 44.1 kHz. The corrected fixture passes at 44.1 and 48 kHz. Two additional cases cover queued reopen markers across forced silence and a reopen that coincides with an existing splice. Independent review checked the output assertions and queue transitions. [Before control](https://github.com/ThatMarko/AirFlash/actions/runs/42744556820)

The first fix run passed the behavior tests and identified two Rust formatting differences. The formatted head `3adf83c` passes Windows Headless CI with 375 Rust tests, two Rust ignores, 1,342 Core tests and 252 Python tests. Clippy, formatting against original Main `a52803d`, native release compatibility and the managed build also pass. [Final Headless run](https://github.com/ThatMarko/AirFlash/actions/runs/42745587467)

The fixture proves the engine now smooths this PCM transition. The cause of the intermittent crackle heard during real playback remains unconfirmed. The implementation is on `codex/reliability-refresh-v062`.

Unofficial preview `0.3.6-rc.1` from `3adf83c` passed six full portable UI cells (1,744 checks each), three fractional-DPI cells (92 checks each), embedded-engine checks, repeated packaging and the disposable installer lifecycle. The archive matches GitHub's SHA-256 digest `664fbc37ee153b946b26da3755b270237626e1dc5c0cbe19b03292f5e654b7ee`. Each binary matches the package manifest, and the portable app hash matches the UI test receipt. [Packaging run](https://github.com/ThatMarko/AirFlash/actions/runs/42745591388)\n