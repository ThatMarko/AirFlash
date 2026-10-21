# Improve AirFlash reliability, pairing and diagnostics

Queued receiver work can outlive user intent, missing DNS completions can suppress retries, and terminal engine events can leave pairing input open. This change addresses those paths alongside owned mute recovery, exact input handling and bounded diagnostics.

Base: `Ding-Kyoma:main` at `439de6260e01efbfca528a12cf4d4c1010b3583e`. Head: `ThatMarko:codex/reliability-review` at `bbe283c8e641b059370d07033622df65522c9d61`.

Fixes #14.
Fixes #24.

This includes the updated authentication scope originally proposed in #25, which remains open. The author reports that #6/#7 are now fixed in manual testing; the tested build and conditions are being confirmed before attributing their resolution to this PR. Incidents #16/#35 have no established root-cause relationship to these changes. Configured-pair completeness (#28) and media-key volume (#30) are separate scope.

Upstream contributors' software stereo, application capture/standby, schema-4 settings, receiver profiles/rates, password pairing and language catalogs are retained. Already-integrated SDK preflight, scrolling, installer refresh, configuration recovery, passive transport retention and native IPC/EQ results remain intact. The sections below describe the remaining fixes and additions.

## Review guide

The [complete diff](https://github.com/ThatMarko/AirFlash/compare/439de6260e01efbfca528a12cf4d4c1010b3583e...bbe283c8e641b059370d07033622df65522c9d61) contains **75 files (+15,047/-755)** in **nine linear commits**: eight behavior commits with their regressions, followed by shared desktop verification. Each commit builds and passes its available Core suite. Existing development branches and their original commits are preserved; this branch starts from reviewed upstream Main. Documentation changes remain separately in the fork; this PR retains upstream documentation unchanged.

### 1. Package cleanup and host selection — `a30c050`

Reject cleanup through linked ancestors or contents before deletion, align stable/preview artifact consumers, and select the first actual PATH host. Preserve SDK `10.0.100/latestFeature`, working-directory resolution, failure-before-side-effects ordering and release reservation policy.

Review [cleanup and naming](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/scripts/build-package.ps1#L2) and [host selection](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/scripts/dotnet-common.ps1#L24). Tests cover [linked paths using fake operations](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/tests/test_build_package.py#L297), preview agreement and [ordered PATH hosts](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/tests/test_dotnet_preflight.py#L341).

### 2. Session ownership and receiver intent — `039dd84`

Capture owner/intent atomically and serialize delayed mutations and current automatic eligibility with admission. Filter attempted candidates before ranking and preserve exact manual endpoints. Shell shutdown closes admission before waits. Publish the selected-rate Connecting snapshot so the UI retains active Stop controls.

Keep owned transport settings separate from canonical persisted identities. An unrelated flush after a newer Pair preserves its exact transport and overrides, including while input is open. Canonical imports or clears require a unique current transport binding; preserve intentional raw-key resets, present defaults, software membership edits and endpoint restarts. The latency hint reads effective owned overrides with the current panel mode.

Review [admission](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Core/SessionController.cs#L290), [settings projection](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Core/SessionSettingsContext.cs#L13) and [routing](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Core/ReceiverSelection.cs#L7). Regressions cover [serialized Stop](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/SessionLifecycleTests.cs#L315), [Connecting publication](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/SessionSnapshotPublicationTests.cs#L12), [40 settings-context cases](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/SessionSettingsContextTests.cs#L37) and [actual PIN-to-playback continuation](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.App/Verification/UiSessionLifecycle.cs#L141).

### 3. Null software-group identity recovery — `46f4f8d`

Reject a null ID before string operations so the existing settings loader handles the validation error and preserves original bytes. Preserve existing blank-ID rejection, schema-4 migration and unknown fields.

Review [the guard](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Core/Settings.cs#L157) and [repeated-Load/raw-byte recovery controls](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/SoftwareGroupSettingsTests.cs#L29).

### 4. DNS deadlines and resource ownership — `c4c292a`

Separate advertisements, cached TTL, deadlines, request tokens and epochs. Bound retry/backoff and unsettled owners. Native resolves use a finite helper address space: once a child may have started, capacity remains held until that exact child's exit is observed, including uncertain kill/wait outcomes. Proven unstarted owners may settle directly. Browse ownership has a separate terminal-callback contract; a cancel return does not prove quiescence.

Contain observer and diagnostic fallback failures so a throwing Trace sink cannot interrupt publication or abandon deferred native work.

Review [token guards](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L311), [fallback containment](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.App/Services/WindowsDiscovery.cs#L605) and [helper settlement](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.App/Services/ResolveProcessDnsServiceApi.cs#L130). Tests explicitly schedule [publication/cancellation races](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/DiscoveryLifecycleTests.cs#L495), [throwing fallback sinks](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/DiscoveryLifecycleTests.cs#L731) and [result-before-exit ownership](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/ResolveProcessTests.cs#L18).

### 5. Authentication errors and complete input — `4334e6e`

Open each selected credential/password file once and read within bounds. Only defined initial-open absence selects another eligible method; other failures remain errors. Retain accessory-keyed current-user DPAPI and mutually exclusive authentication. Keep one event reader while input is open and cancel/join prompts on terminal events. Enablement and final submission share PIN/password validation, preserving accepted whitespace passwords, strict UTF-8 limits and the complete WPF secret value.

Review [storage reads](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/native/airflash-engine/src/credentials.rs#L116), [prompt/event ownership](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Core/SessionController.cs#L845) and [the password control guard](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.App/Ui/PinDialog.cs#L48). Tests cover [terminal prompt cleanup](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/PairingPromptLifecycleTests.cs#L27), [exact whitespace payloads](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/PairingPasswordTests.cs#L51) and [actual modal controls](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.App/Verification/UiPairing.cs#L75).

### 6. Remaining terminal IPC consumers — `3be9fbb`

Preserve the defined process-owned terminal rejection through hello, capabilities and application-catalog consumers, including original detail, nonretryability and cleanup. Keep ordinary session filtering and native EQ preparation semantics.

Review [the response reader](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Core/EngineEvents.cs#L6) and [application queries](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.App/Services/ApplicationCatalog.cs#L16), with [capability](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/IpcConsumerPhaseTests.cs#L84) and [application/cleanup regressions](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/IpcQueryConsumerTests.cs#L20).

### 7. Durable owned mute recovery — `9cd4338`

Persist exact endpoint/prior-bit intent before mutation, with an exclusive lease and GUID/generation ownership. Preserve unresolved or ambiguous records and block new muting from overwriting them. Normal cleanup still attempts exact-bit restoration after journal failure; application capture remains nonmuting. A saved bit cannot prove that no later user/application change occurred.

Review [intent/recovery transitions](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Core/MuteRecoveryCoordinator.cs#L88) and [the lease](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Core/FileMuteRecoveryJournal.cs#L15). Tests cover [both prior bits and failure states](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/MuteRecoveryTests.cs#L15), [wrong returned endpoint identities](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/MuteRecoveryTests.cs#L248) and [blocked admission](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/MuteRecoveryAdmissionTests.cs#L16).

### 8. Diagnostics and capture padding — `7e7ecb8`

Expose the existing combined diagnostics-copy command from Monitor. Count optional padded output frames and wholly padded pulls before EQ, once per shared pull, while preserving underruns and distinguishing missing capture from captured/scheduled silence. Reset each worker attempt and retain pre-fault values. Remove only unused translation keys after dynamic-consumer review, preserving the upstream language catalogs.

Review [the shared pull](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/native/airflash-engine/src/live.rs#L177) and [Monitor action](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.App/Ui/SettingsWindow.xaml#L280). Tests cover [both output rates and padding](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/native/airflash-engine/src/live.rs#L711), [retry/pre-fault snapshots](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.Tests/CapturePaddingMetricsTests.cs#L208) and [realized Monitor/copy behavior](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.App/Verification/UiDiagnostics.cs#L30). These metrics describe local capture padding, not network loss or acoustic measurements.

### 9. Shared desktop verification — `bbe283c`

Reject ambiguous, orphaned and mixed tray/focused verification modes before WPF initialization. Exercise actual PIN/password controls and the complete submitted value. Check final Settings controls within the real viewport when a page fits, while retaining dedicated overflow, wheel and raster assertions. Shared fixtures follow the behavior commits so each intermediate implementation remains buildable and testable.

Review [argument dispatch](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.App/Program.cs#L15) and [measured page-end access](https://github.com/ThatMarko/AirFlash/blob/bbe283c8e641b059370d07033622df65522c9d61/desktop/AirFlash.App/Verification/UiSmoke.cs#L475).

## Validation

| Scope | Executed result |
| --- | --- |
| Every new commit | Clean Release solution build, zero warnings/errors; available Core suites pass **403 / 422 / 472 / 587 / 621 / 726 / 884 / 1,016 / 1,016**, in commit order. |
| Final local head | **1,016 Core passed**; **173 Python passed**, one Windows symlink-privilege skip. |
| Fresh exact-head desktop CI | [Run 39630589971](https://github.com/ThatMarko/AirFlash/actions/runs/39630589971) passed **18 cells / 11,386 assertions**, including seven full regional cells and clipboard controls. The artifact digest, reports, modes/cultures, stable DLL/apphost hashes and all 248 before/after source-file hashes were verified against the frozen checkout. |
| Fresh exact-head headless/native CI | [Run 39630594304](https://github.com/ThatMarko/AirFlash/actions/runs/39630594304) passed **1,016 Core / 174 Python / 105 native**, with zero managed/Python skips and two intentional native device/soak ignores. Locked managed/native Release builds, Ruff, Clippy and static checks passed; four unchanged inherited formatting files and 152 unconfirmed static imports remain explicit limits. |
| Source and history preservation | Every non-documentation blob matches qualified `6c1e2ef` exactly. All original development commits and upstream contributors remain preserved; the new branch presents their resolved implementation in nine reviewable commits. |

Both successful workflows were carried by fork Main `abb0883`; their selected source was exact `bbe283c`. Headless qualification verifies its initial full source-byte map and final HEAD/tracked status; desktop additionally verifies a complete final byte map. Tests use fake services, synthetic identities and temporary isolated files. No personal configuration/credentials, real receiver/capture/endpoint mutation, release reservation or release dispatch is required.

Earlier local runs intermittently failed the SettingsIcon alpha check. Later full runs pass without a rendering-code repair; the cause remains unexplained. Physical DPI/multimonitor, Windows 10, real receiver/audio/network reliability and physical power-loss durability remain unqualified. Passing mocks or CI does not establish incident resolution.
