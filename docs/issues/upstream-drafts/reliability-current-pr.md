# Complete session, discovery and recovery safeguards

Queued receiver work can outlive user intent, missing DNS completions can suppress retries, and terminal engine events can leave pairing input open. This change addresses those paths alongside owned mute recovery, exact input handling and bounded diagnostics.

Reviewed base: `439de6260e01efbfca528a12cf4d4c1010b3583e`. Candidate: `0533cbff37cb7e0a2d0b80cee0d1aed48737ed15`, `ThatMarko:codex/integrate-current-stable`. Related issues: #14 and #24; this includes the updated authentication scope originally proposed in #25. Issues #6/#7 remain separate incidents whose causes and resolution are not established by this work.

Schema 4, software stereo, application capture/standby, per-receiver formats/profiles, password pairing, localization and SDK/ILLink changes remain intact. SDK preflight, scrolling, installer refresh, configuration recovery, passive transport retention and native IPC/EQ results already integrated into Main are retained. The sections below describe the remaining work.

## Review guide

The [complete diff](https://github.com/ThatMarko/AirFlash/compare/439de6260e01efbfca528a12cf4d4c1010b3583e...0533cbff37cb7e0a2d0b80cee0d1aed48737ed15) is 78 files (+14,117/-758). It includes 31 test/fixture files (including the test project), with 9,940 added lines. The existing 32 commits comprise 21 nonmerge commits and 11 integration merges. Use these eight sections to review behavior, then check the shared composition below; the sections are not eight independent linear commits.

### 1. Package cleanup and host selection

Reject cleanup through linked ancestors/contents before deletion, align stable/preview artifact consumers, and select the first actual PATH host. Preserve SDK `10.0.100/latestFeature`, working-directory resolution, early failure ordering and release reservation policy.

Review [cleanup and naming](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/scripts/build-package.ps1#L2) and [host selection](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/scripts/dotnet-common.ps1#L24). Regressions cover [linked paths without destructive operations](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/tests/test_build_package.py#L297), preview agreement and [ordered PATH hosts](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/tests/test_dotnet_preflight.py#L341). Commits: `121b9fd`, `81948f2`.

### 2. Session ownership and receiver intent

Capture owner/intent atomically and serialize delayed mutations and current automatic eligibility with admission. Filter attempted candidates before ranking and preserve exact manual endpoints. Shell shutdown closes admission before waits. Publish the selected-rate Connecting snapshot so dispatcher filtering retains active Stop presentation.

Review [admission and delayed settings](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Core/SessionController.cs#L286) and [routing](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Core/ReceiverSelection.cs#L7). Regressions cover [Stop while serialized](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Tests/SessionLifecycleTests.cs#L315), [current snapshot publication](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Tests/SessionSnapshotPublicationTests.cs#L12) and [actual dispatcher behavior](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.App/Verification/UiSessionLifecycle.cs#L75). Commits: `8337ae2`, `5670710`, `39198ef`, `9b96f10`.

### 3. Null software-group identity recovery

Reject a null ID before string operations so the existing settings loader handles the validation error and preserves original bytes. Existing blank-ID rejection, schema-4 migration and unknown fields remain intact.

Review [the validation guard](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Core/Settings.cs#L157) and [repeated-Load/raw-byte recovery controls](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Tests/SoftwareGroupSettingsTests.cs#L29). Commit: `95051e0`.

### 4. DNS deadlines and resource ownership

Separate advertisements, cached TTL, deadlines, request tokens and epochs. Bound retry/backoff and unsettled owners. Native resolves use a finite helper address space: once a child may have started, capacity stays held until its exact exit is observed, including uncertain kill/wait outcomes. Proven unstarted owners may settle directly. Browse ownership has a separate terminal-callback contract; a cancel return does not prove quiescence.

Review [result/token guards](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L311) and [helper exit settlement](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.App/Services/ResolveProcessDnsServiceApi.cs#L130). Regressions explicitly schedule [cancellation/publication races](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Tests/DiscoveryLifecycleTests.cs#L494) and [result-before-exit ownership](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Tests/ResolveProcessTests.cs#L18). Commit: `da3649d`.

### 5. Authentication errors and exact input

Open each selected credential/password file once and read within bounds. Only defined initial-open absence selects another eligible method; other failures remain errors. Retain accessory-keyed current-user DPAPI and mutually exclusive authentication. Keep one event reader while input is open and cancel/join prompts on terminal events. Enablement and final submission share the accepted PIN/password contracts, preserving raw whitespace passwords, strict UTF-8 limits and the complete WPF secret value.

Review [storage reads](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/native/airflash-engine/src/credentials.rs#L116), [prompt/event ownership](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Core/SessionController.cs#L837) and [the password control guard](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.App/Ui/PinDialog.cs#L48). Regressions cover [terminal prompt cleanup](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Tests/PairingPromptLifecycleTests.cs#L27), [exact whitespace payloads](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Tests/PairingPasswordTests.cs#L51) and [actual modal controls](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.App/Verification/UiPairing.cs#L75). Commits: `932f7a5`, `1498a94`, `32c5b14`, `b76472d`, `2239575`, `b7bbc33`.

### 6. Remaining terminal IPC consumers

Preserve the defined process-owned terminal rejection through hello, capabilities and application-catalog consumers, including detail, nonretryability and cleanup. Ordinary session filtering and native EQ preparation results retain their existing semantics.

Review [the response reader](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Core/EngineEvents.cs#L6) and [application queries](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.App/Services/ApplicationCatalog.cs#L16). Regressions exercise [capability rejection](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Tests/IpcConsumerPhaseTests.cs#L84), [application rejection and connection cleanup](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Tests/IpcQueryConsumerTests.cs#L20). Commit: `d5bd126`.

### 7. Durable owned mute recovery

Persist exact endpoint/prior-bit intent before mutation, with an exclusive lease and GUID/generation journal ownership. Preserve unresolved or ambiguous records and block new muting from overwriting them. Normal cleanup still attempts exact-bit restoration after journal failure; application capture remains nonmuting. Persisted state alone cannot prove that no later user/application change occurred.

Review [intent and recovery transitions](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Core/MuteRecoveryCoordinator.cs#L88) and [the exclusive lease](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Core/FileMuteRecoveryJournal.cs#L15). Regressions cover [both prior bits and interruption/failure states](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Tests/MuteRecoveryTests.cs#L15) and [blocked admission](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Tests/MuteRecoveryAdmissionTests.cs#L16). Commit: `a284566`.

### 8. Diagnostics and capture padding

Expose the existing combined diagnostics-copy command from Monitor. Count optional padded output frames and wholly padded pulls before EQ, once per shared pull, while preserving underruns and distinguishing missing capture from captured/scheduled silence. Reset per worker attempt and retain pre-fault values. Remove only unused translation keys after final dynamic-consumer review.

Review [the shared capture pull](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/native/airflash-engine/src/live.rs#L177) and [Monitor action](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.App/Ui/SettingsWindow.xaml#L280). Regressions cover [both output rates and padding](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/native/airflash-engine/src/live.rs#L711), [retry/pre-fault snapshots](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.Tests/CapturePaddingMetricsTests.cs#L208) and [realized Monitor/copy behavior](https://github.com/ThatMarko/AirFlash/blob/0533cbff37cb7e0a2d0b80cee0d1aed48737ed15/desktop/AirFlash.App/Verification/UiDiagnostics.cs#L30). Commit: `1e19579`. These metrics describe local padding, not network loss or acoustic measurements.

**Shared composition to review:** merges `be7f993` → `6a0f9ae` → `7a61145` combine metrics, recovery and admission in the controller/view model; `6eabb18` combines PIN normalization with the existing password dialog. Inspect those resolved hunks as well as originating commits. `5d43b04` adds actual modal fixtures and strict verification-route parsing, and corrects the broad hidden-active-row assertion. `5d9771e` observes standby state/metrics publications separately; `4353635` and `0533cbf` strengthen actual input fixtures. All existing commits remain intact; DNS and durable recovery need individual review within the combined diff.

## Executed validation

| Area | Source and executed result |
| --- | --- |
| Managed | Exact `0533cbf`: **965 Core passed, zero skips**; locked restore and Release build using SDK 10.0.402, zero warnings/errors. |
| Local focused desktop | Exact `0533cbf`: six cells / **1,394 assertions**, including actual modal input, terminal cleanup, session controls and Monitor. |
| Hosted desktop | Exact-source [run 39517562675](https://github.com/ThatMarko/AirFlash/actions/runs/39517562675): **18 cells / 10,914 assertions**, including seven full regional cells and actual modal/clipboard tests. All 247 initial/final source files stayed unchanged; archive digest, binary identity, modes and observed cultures were independently checked. |
| Native and Python | Native/scripts/Python/installer trees match `6eabb1864d32256283ec520962380b87e8fcbe86`. Its [run 39484878539](https://github.com/ThatMarko/AirFlash/actions/runs/39484878539) passed **174 Python / 105 native**, with two intentional native device/soak ignores, Ruff/Clippy and native build/static checks. These results retain that source provenance; native tests were not newly executed at `0533cbf`. |

Pre-fix controls and healthy controls are retained for the repaired failure paths. DNS tests explicitly choose cancellation/publication schedules and assert ownership through observed process exit. Fixtures use fake services, synthetic identities, temporary files/journals and isolated output, without personal configuration/credentials or real receivers/capture/endpoints.

## Qualification limits

Local full raster smoke still fails the unchanged SettingsIcon alpha assertion after 38 checks, including same-binary Normal/Hidden launch contrasts. Hosted full smoke passes; the cause of the local/hosted difference remains unknown. Hosted builds set `AirFlashConsole=true` and observed DPI 96/scale 1; default local Release uses GUI output. No hosted PE subsystem inspection is claimed.

Physical DPI/multimonitor, Windows 10 execution, real receiver/audio/network reliability and physical power-loss durability remain unqualified. Four unchanged inherited formatting files and 152 unconfirmed static imports remain explicit. Tests and mocks do not establish physical reboot durability or incident resolution.
