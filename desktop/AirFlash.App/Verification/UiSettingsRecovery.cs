using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AirFlash.App.Services;
using AirFlash.App.Ui;
using AirFlash.App.ViewModels;
using AirFlash.Core;
namespace AirFlash.App.Verification;

// The concrete store is intentional: production loads it once for shell preferences,
// then again in AppViewModel. All paths and services here are disposable fixtures.
internal static class UiSettingsRecovery
{
    public static async Task RunAsync(List<string> checks, string directory)
    {
        var fixtures = Path.Combine(directory, "settings-recovery-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(fixtures);
        foreach (var (name, invalid) in new[]
        {
            ("null-manual", "\"manual_receivers\":[null]"),
            ("invalid-rate", "\"stream_sample_rate\":\"invalid\""),
            ("null-rate", "\"stream_sample_rate\":null")
        })
        {
            var original = Encoding.UTF8.GetBytes("{\"schema_version\":2,\"auto_connect_on_discover\":true,\"start_at_login\":true,\"mute_while_streaming\":true," + invalid + "}");
            await VerifyReadableRecoveryAsync(name, original, checks, fixtures);
        }
        var malformed = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("{\"schema_version\":2,\"manual_receivers\":[")).ToArray();
        await VerifyReadableRecoveryAsync("malformed-utf16-bom", malformed, checks, fixtures);
        await VerifyLockedRecoveryAsync(checks, fixtures);
        await VerifyCompatibleSettingsAsync(checks, fixtures);
        await VerifyAutomaticRecoveryOwnershipAsync(checks, fixtures);
        await VerifyDeferredFailureNoticeAsync(checks, fixtures);
        VerifyFutureSchema(checks, fixtures);
        File.WriteAllText(Path.Combine(directory, "settings-recovery-report.json"), JsonSerializer.Serialize(new
        {
            ok = true,
            cases = new[] { "null-manual", "invalid-rate", "null-rate", "malformed-utf16-bom", "locked-double-load-save-retry", "legacy-compatible-offline-interface", "automatic-recovery-lock-and-new-intent", "deferred-endpoint-error-notice-precedence", "future-schema" },
            scope = "Synthetic files, concrete SettingsStore, the two production Load calls, and mock WPF startup. No production configuration, registry, discovery, audio, native engine, or receivers are used. Normal production startup's read-only native hello is outside this fixture."
        }, AppSettings.JsonOptions));
    }

    private static async Task VerifyReadableRecoveryAsync(string name, byte[] original, List<string> checks, string directory)
    {
        var path = Path.Combine(directory, name + ".json"); File.WriteAllBytes(path, original);
        var store = new SettingsStore(path);
        var initial = store.Load();
        Check(initial.Validate() is null && !initial.StartAtLogin && !initial.AutoConnectOnDiscover, name + ": first shell load returns valid safe defaults", checks);
        Check(!string.IsNullOrEmpty(store.LoadWarning), name + ": first load reports recovery", checks);
        var discovery = new FixtureDiscovery(); var autostart = new FixtureAutostart(); var audio = new FixtureAudio(); var engine = new UiSmoke.MockFactory();
        await using var app = new AppViewModel(store, discovery, autostart, audio, engine, Application.Current.Dispatcher);
        var panel = new ControlPanel(app);
        try
        {
            Check(app.Settings.Validate() is null && app.Settings.ManualReceivers.Count == 0 && app.Settings.StreamSampleRate == "44100", name + ": second constructor load cannot expose invalid manual entries or rates", checks);
            app.Start(); await app.Startup.WaitAsync(TimeSpan.FromSeconds(5));
            Check(discovery.StartCount == 0, name + ": recovered defaults do not request a background startup browse", checks);
            panel.ShowPanel();
            await Until(() => app.AllReceivers.Any(r => r.Id == FixtureDiscovery.ReceiverId));
            await Pump();
            Check(!string.IsNullOrEmpty(app.Notice) && app.Notice == store.LoadWarning, name + ": startup retains the recovery notice across both loads", checks);
            Check(UiSmoke.Descendants(panel).OfType<TextBlock>().Any(t => t.IsVisible && t.Text == app.Notice), name + ": recovery notice reaches the panel text binding", checks);
            await app.TryAutoConnectAsync(); await Pump();
            Check(autostart.Values.SequenceEqual(new[] { false }), name + ": rejected startup flags cannot enable autostart", checks);
            Check(discovery.StartCount == 1 && discovery.Publications == 1 && app.Snapshot.State == PlaybackState.Idle, name + ": mock discovery completes without automatic playback", checks);
            Check(engine.CreatedCount == 0 && engine.Commands.IsEmpty && audio.MuteCount == 0, name + ": rejected data cannot open a playback engine, send commands, or mute", checks);
            Check(File.ReadAllBytes(path).SequenceEqual(original) && !File.Exists(path + ".pre-wpf.bak"), name + ": repeated load and startup neither replace the original nor claim a completed backup", checks);
        }
        finally { panel.ShutdownPanel(); }
        await app.DisposeAsync();
        Check(File.ReadAllBytes(path).SequenceEqual(original) && engine.CreatedCount == 0 && engine.Commands.IsEmpty && audio.MuteCount == 0, name + ": isolated shutdown keeps the rejected original unchanged", checks);
    }

    private static async Task VerifyLockedRecoveryAsync(List<string> checks, string directory)
    {
        var path = Path.Combine(directory, "locked-double-load.json");
        var original = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes("{\"schema_version\":2,\"master_volume\":73,\"auto_connect_on_discover\":true,\"start_at_login\":true}")).ToArray();
        File.WriteAllBytes(path, original);
        var store = new SettingsStore(path);
        var discovery = new FixtureDiscovery(); var autostart = new FixtureAutostart(); var audio = new FixtureAudio(); var engine = new UiSmoke.MockFactory();
        AppViewModel? app = null;
        try
        {
            using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var initial = store.Load();
                Check(initial.Validate() is null && !initial.AutoConnectOnDiscover && !initial.StartAtLogin && !string.IsNullOrEmpty(store.LoadWarning), "locked settings: shell load recovers without an escaping reread", checks);
                app = new(store, discovery, autostart, audio, engine, Application.Current.Dispatcher);
                app.Start(); await app.Startup.WaitAsync(TimeSpan.FromSeconds(5));
                Check(discovery.StartCount == 0, "locked settings: recovered defaults do not request a background startup browse", checks);
                app.SetPanelVisible(true);
                await Until(() => app.AllReceivers.Any(r => r.Id == FixtureDiscovery.ReceiverId));
                await app.TryAutoConnectAsync(); await Pump();
                Check(app.Settings.Validate() is null && !string.IsNullOrEmpty(app.Notice) && app.Notice == store.LoadWarning && autostart.Values.SequenceEqual(new[] { false }), "locked settings: second load and startup retain safe defaults and a controlled warning", checks);
                Check(engine.CreatedCount == 0 && engine.Commands.IsEmpty && audio.MuteCount == 0, "locked settings: automatic discovery never starts or mutes playback", checks);
                app.MasterVolume = 37;
                var failed = false;
                try { await app.FlushVolumeAsync().WaitAsync(TimeSpan.FromSeconds(5)); }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException) { failed = true; }
                Check(failed && !File.Exists(path + ".pre-wpf.bak"), "locked settings: panel flush cannot replace an unpreserved unreadable original", checks);
                var captured = new byte[original.Length]; locked.Position = 0; locked.ReadExactly(captured);
                Check(locked.Length == original.Length && captured.SequenceEqual(original), "locked settings: failed flush leaves the exact original bytes intact", checks);
            }
            Check(File.ReadAllBytes(path).SequenceEqual(original), "locked settings: releasing the fixture lock leaves the original intact", checks);
            await app!.FlushVolumeAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Check(File.ReadAllBytes(path + ".pre-wpf.bak").SequenceEqual(original), "locked settings: later panel flush preserves the original BOM and bytes before replacement", checks);
            Check(app.Notice == (store.LoadWarning ?? ""), "locked settings: a successful preservation save updates the recovery notice", checks);
            var saved = new SettingsStore(path).Load();
            Check(saved.Validate() is null && saved.MasterVolume == 37 && !saved.AutoConnectOnDiscover && !saved.StartAtLogin, "locked settings: the failed panel edit remains dirty and retries successfully after preservation", checks);
            Check(engine.CreatedCount == 0 && engine.Commands.IsEmpty && audio.MuteCount == 0, "locked settings: successful defaults persistence still has no playback side effects", checks);
        }
        finally { if (app is not null) await app.DisposeAsync(); }
    }

    private static async Task VerifyCompatibleSettingsAsync(List<string> checks, string directory)
    {
        const string offlineInterface = "01db7c77-1016-49cb-8f5c-f5c194425d15";
        var path = Path.Combine(directory, "legacy-compatible.json");
        var original = Encoding.UTF8.GetBytes("""
            {"schema_version":1,"discovery_interface_id":"01db7c77-1016-49cb-8f5c-f5c194425d15",
             "custom_buffer_ms":5000,"stream_sample_rate":"48000","unknown_root":{"keep":"root"},
             "manual_receivers":[{"id":"manual:fixture","name":"Fixture manual","host":"192.0.2.40","port":7001}],
             "receivers":{"manual:fixture":{"auto_connect":false,"custom_buffer_ms":null,"latency_mode":null}},
             "equalizer":{"enabled":true,"preamp_db":-3.5,"band_gains_db":[7.5,0,0,0,0,0,0,0,0,0],"unknown_eq":{"keep":"eq"}}}
            """);
        File.WriteAllBytes(path, original); var store = new SettingsStore(path); var initial = store.Load();
        Check(initial.Validate() is null && initial.SchemaVersion == 2 && initial.CustomBufferMs == 2000 && initial.StreamSampleRate == "48000", "valid settings: legacy migration validates while retaining supported playback values", checks);
        var discovery = new FixtureDiscovery(); var audio = new FixtureAudio(); var engine = new UiSmoke.MockFactory();
        await using var app = new AppViewModel(store, discovery, new FixtureAutostart(), audio, engine, Application.Current.Dispatcher);
        app.Start(); await app.Startup.WaitAsync(TimeSpan.FromSeconds(5)); await Pump();
        Check(discovery.StartCount == 0, "valid settings: an inactive hidden panel does not browse without automatic demand", checks);
        app.SetPanelVisible(true); await Pump();
        Check(app.Notice.Length == 0 && store.LoadWarning is null, "valid settings: repeated migration load does not produce a recovery warning", checks);
        Check(discovery.SelectedInterface == offlineInterface && app.Settings.DiscoveryInterfaceId == offlineInterface && discovery.Publications == 1 && app.AllReceivers.Count == 1 && app.AllReceivers[0].IsManual, "valid settings: an unavailable selected interface is retained without fallback while manual receivers remain", checks);
        Check(app.Settings.ManualReceivers.Single() == initial.ManualReceivers.Single() && app.Settings.ReadOptions("manual:fixture").AutoConnect is null && app.Settings.ReadOptions("manual:fixture").CustomBufferMs is null && app.Settings.ReadOptions("manual:fixture").LatencyMode is null, "valid settings: manual records and nullable legacy receiver overrides survive both loads", checks);
        Check(app.Settings.Extra!["unknown_root"].GetProperty("keep").GetString() == "root" && app.Settings.Equalizer.Extra!["unknown_eq"].GetProperty("keep").GetString() == "eq" && app.Settings.Equalizer.Enabled && app.Settings.Equalizer.PreampDb == -3.5 && app.Settings.Equalizer.BandGainsDb[0] == 7.5, "valid settings: unknown root/equalizer fields and the curve survive construction", checks);
        Check(File.ReadAllBytes(path).SequenceEqual(original) && !File.Exists(path + ".pre-wpf.bak"), "valid settings: migration and startup defer persistence until a real edit", checks);
        app.MasterVolume = 41; await app.FlushVolumeAsync().WaitAsync(TimeSpan.FromSeconds(5));
        var saved = new SettingsStore(path).Load();
        Check(File.ReadAllBytes(path + ".pre-wpf.bak").SequenceEqual(original) && saved.Validate() is null && saved.MasterVolume == 41 && saved.DiscoveryInterfaceId == offlineInterface && saved.ManualReceivers.Single() == initial.ManualReceivers.Single(), "valid settings: a panel save retains compatibility fields and backs up the exact pre-migration file", checks);
        Check(saved.Extra!["unknown_root"].GetProperty("keep").GetString() == "root" && saved.Equalizer.Extra!["unknown_eq"].GetProperty("keep").GetString() == "eq" && saved.Equalizer.BandGainsDb[0] == 7.5 && engine.CreatedCount == 0 && engine.Commands.IsEmpty && audio.MuteCount == 0, "valid settings: extensions and equalizer survive persistence without playback", checks);
    }

    private static async Task VerifyAutomaticRecoveryOwnershipAsync(List<string> checks, string directory)
    {
        foreach (var action in new[] { "pair", "stop" })
        {
            var path = Path.Combine(directory, "automatic-recovery-" + action + ".json");
            var original = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("{ malformed automatic recovery source }")).ToArray();
            var changed = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes("{\"schema_version\":2,\"auto_connect_on_discover\":true,\"start_at_login\":true,\"master_volume\":73}")).ToArray();
            File.WriteAllBytes(path, original); var store = new SettingsStore(path); _ = store.Load();
            var discovery = new FixtureDiscovery(); var autostart = new FixtureAutostart(); var audio = new FixtureAudio(); var engine = new UiSmoke.MockFactory();
            await using var app = new AppViewModel(store, discovery, autostart, audio, engine, Application.Current.Dispatcher);
            app.UseControlledScheduling(); app.Start(); await app.Startup.WaitAsync(TimeSpan.FromSeconds(5));
            await Until(() => app.AllReceivers.Count == 1);
            var receiver = app.AllReceivers.Single(); var before = app.Session.Capture();
            // An explicit per-receiver edit makes this candidate eligible; rejected source flags never do.
            app.Settings.Options(receiver.Id).AutoConnect = true; app.MasterVolume = 29;
            File.WriteAllBytes(path, changed);
            using (var locked = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                var failed = false;
                try { await app.TryAutoConnectAsync().WaitAsync(TimeSpan.FromSeconds(5)); }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException) { failed = true; }
                var captured = new byte[changed.Length]; locked.ReadExactly(captured);
                Check(failed && engine.CreatedCount == 0 && app.Settings.LastReceiverId is null && app.Session.Capture().Owner == before.Owner && audio.MuteCount == 0,
                    "cross-group recovery: locked automatic preflush fails before engine admission, mute, last-used or ownership changes: " + action, checks);
                Check(captured.SequenceEqual(changed) && !File.Exists(path + ".pre-wpf.bak") && app.Notice == store.LoadWarning,
                    "cross-group recovery: a changed and relocked source stays intact with the pending recovery notice: " + action, checks);
            }
            var admission = new Gate(); app.BeforeAutomaticAdmission = admission.WaitAsync;
            var automatic = app.TryAutoConnectAsync();
            try
            {
                await admission.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
                var saved = new SettingsStore(path).Load();
                Check(saved.MasterVolume == 29 && !saved.AutoConnectOnDiscover && !saved.StartAtLogin && saved.LastReceiverId is null && saved.ReadOptions(receiver.Id).AutoConnect == true,
                    "cross-group recovery: retry persists the real edit without importing external startup flags or claiming an automatic attempt: " + action, checks);
                var backups = Directory.GetFiles(directory, Path.GetFileName(path) + ".pre-wpf*.bak");
                Check(backups.Length == 2 && backups.Any(backup => File.ReadAllBytes(backup).SequenceEqual(original)) && backups.Any(backup => File.ReadAllBytes(backup).SequenceEqual(changed)) && app.Notice == store.LoadWarning,
                    "cross-group recovery: automatic preflush preserves both exact encodings and updates the completed recovery notice: " + action, checks);
                if (action == "pair")
                {
                    await app.PairAsync(receiver, (_, cancellation) => Task.FromResult<string?>(null));
                    await Until(() => app.Session.Snapshot.State == PlaybackState.Pairing && engine.Commands.Any(command => command.Text("command") == "pair"));
                }
                else await app.StopAsync();
                var owned = app.Session.Capture(); var opens = engine.CreatedCount; var commands = engine.Commands.Count;
                admission.Release.TrySetResult(); await automatic.WaitAsync(TimeSpan.FromSeconds(5)); await Pump();
                Check(app.Session.Capture().Owner == owned.Owner && app.Session.Capture().AutomaticIntent == owned.AutomaticIntent && engine.CreatedCount == opens && engine.Commands.Count == commands && app.Settings.LastReceiverId is null && new SettingsStore(path).Load().LastReceiverId is null,
                    "cross-group recovery: a delayed eligible automatic candidate cannot override newer " + action + " or mark it last used", checks);
                app.BeforeAutomaticAdmission = null; await app.StopAsync(false);
                if (action == "pair")
                {
                    await app.TryAutoConnectAsync().WaitAsync(TimeSpan.FromSeconds(5)); await Until(() => app.Session.Snapshot.State == PlaybackState.Streaming);
                    Check(engine.CreatedCount == opens + 1 && app.Settings.LastReceiverId == receiver.Id && new SettingsStore(path).Load().LastReceiverId == receiver.Id,
                        "cross-group recovery: a rejected stale admission did not mark the receiver attempted and a fresh automatic request still starts once", checks);
                }
                else
                {
                    await app.TryAutoConnectAsync().WaitAsync(TimeSpan.FromSeconds(5));
                    Check(engine.CreatedCount == 0 && app.Settings.LastReceiverId is null,
                        "cross-group recovery: user Stop remains automatically suppressed after the real preservation save", checks);
                }
                Check(autostart.Values.SequenceEqual(new[] { false }), "cross-group recovery: external file changes never enable autostart: " + action, checks);
            }
            finally { admission.Release.TrySetResult(); app.BeforeAutomaticAdmission = null; await automatic.WaitAsync(TimeSpan.FromSeconds(5)); }
        }
    }

    private static async Task VerifyDeferredFailureNoticeAsync(List<string> checks, string directory)
    {
        var path = Path.Combine(directory, "deferred-failure-recovery.json"); var original = Encoding.UTF8.GetBytes("{ malformed before deferred endpoint failure }");
        File.WriteAllBytes(path, original); var store = new SettingsStore(path); _ = store.Load();
        var audio = new FixtureAudio(); var engine = new FaultEngine();
        await using var app = new AppViewModel(store, new FixtureDiscovery(), new FixtureAutostart(), audio, engine, Application.Current.Dispatcher);
        app.UseControlledScheduling(); app.Start(); await app.Startup.WaitAsync(TimeSpan.FromSeconds(5)); await Until(() => app.AllReceivers.Count == 1);
        var recoveryWarning = app.Notice;
        // Start the owned synthetic transport directly so no explicit panel action clears the recovery notice.
        await app.Session.StartAsync(app.AllReceivers.Single(), app.Settings.Clone());
        await Until(() => app.Session.Snapshot.State == PlaybackState.Streaming);
        var first = audio.HoldNextRestore(); Gate? final = null;
        try
        {
            audio.FailRestore = true; engine.Connection.Emit(new { @event = "error", message = "synthetic receiver fault", retryable = false });
            await first.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); final = audio.HoldNextRestore(); first.Release.TrySetResult();
            await final.Entered.Task.WaitAsync(TimeSpan.FromSeconds(5)); await Until(() => app.Session.Snapshot.State == PlaybackState.Error);
            var owned = app.Session.Capture(); var restores = audio.RestoreCount;
            await EndpointChangedAsync(app);
            Check(audio.RestoreCount == restores && app.Notice == recoveryWarning && File.ReadAllBytes(path).SequenceEqual(original),
                "cross-group recovery: endpoint return queues behind worker cleanup while pending configuration recovery remains untouched", checks);
            var failed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            app.Session.EndpointRecoveryFailed += _ => failed.TrySetResult(); final.Release.TrySetResult();
            await failed.Task.WaitAsync(TimeSpan.FromSeconds(5)); await Pump();
            Check(app.Notice == "synthetic recovery endpoint restore failure" && app.Session.Capture().Owner == owned.Owner && audio.HasSavedBit && engine.CreatedCount == 1,
                "cross-group recovery: deferred endpoint failure replaces the older recovery notice without replacing its session owner or saved mute state", checks);
            audio.FailRestore = false; app.MasterVolume = 31; await app.FlushVolumeAsync().WaitAsync(TimeSpan.FromSeconds(5)); await Pump();
            Check(File.ReadAllBytes(path + ".pre-wpf.bak").SequenceEqual(original) && new SettingsStore(path).Load().MasterVolume == 31 && store.LoadWarning != recoveryWarning,
                "cross-group recovery: a later real-store flush preserves the exact original and commits the panel edit after deferred audio failure", checks);
            Check(app.Notice == "synthetic recovery endpoint restore failure" && app.Session.Capture().Owner == owned.Owner && app.Session.Snapshot.State == PlaybackState.Error && engine.CreatedCount == 1,
                "cross-group recovery: completed backup notice cannot erase the newer deferred endpoint error or restart the inactive transport", checks);
            await EndpointChangedAsync(app); await Pump();
            Check(!audio.HasSavedBit && !audio.MuteBit && engine.CreatedCount == 1,
                "cross-group recovery: later endpoint cleanup restores the original false mute bit without admitting playback", checks);
        }
        finally { audio.FailRestore = false; first.Release.TrySetResult(); final?.Release.TrySetResult(); }
    }

    private static Task EndpointChangedAsync(AppViewModel app)
        => (Task)typeof(AppViewModel).GetMethod("OnEndpointsChangedAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.Invoke(app, null)!;

    private static void VerifyFutureSchema(List<string> checks, string directory)
    {
        var path = Path.Combine(directory, "future-schema.json"); var original = Encoding.UTF8.GetBytes("{\"schema_version\":3,\"auto_connect_on_discover\":true,\"start_at_login\":true}");
        File.WriteAllBytes(path, original); var rejected = false;
        try { _ = new SettingsStore(path).Load(); }
        catch (NotSupportedException) { rejected = true; }
        Check(rejected && File.ReadAllBytes(path).SequenceEqual(original) && !File.Exists(path + ".pre-wpf.bak"), "future settings: the first shell load remains fatal before constructing a view model or services", checks);
    }

    private static void Check(bool value, string name, List<string> checks) { if (!value) throw new InvalidOperationException(name); checks.Add(name); }
    private static Task Pump() => Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;
    private static async Task Until(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!condition()) await Task.Delay(20, timeout.Token);
        await Pump();
    }
    private sealed class FixtureAutostart : IAutostart
    {
        public List<bool> Values { get; } = [];
        public void Set(bool enabled) => Values.Add(enabled);
        public Task SetAsync(bool enabled) { Set(enabled); return Task.CompletedTask; }
    }
    private sealed class FixtureAudio : IAudioService
    {
        private readonly ConcurrentQueue<Gate> _restores = new();
        private bool? _savedBit;
        public int MuteCount { get; private set; }
        public int RestoreCount { get; private set; }
        public bool MuteBit { get; private set; }
        public bool HasSavedBit => _savedBit is not null;
        public bool FailRestore { get; set; }
        public Gate HoldNextRestore() { var gate = new Gate(); _restores.Enqueue(gate); return gate; }
        public event Action? EndpointsChanged { add { } remove { } }
        public Task<IReadOnlyList<AudioEndpoint>> GetEndpointsAsync() => Task.FromResult<IReadOnlyList<AudioEndpoint>>([new("fixture-endpoint", "Fixture output")]);
        public Task<string?> GetDefaultEndpointIdAsync() => Task.FromResult<string?>("fixture-endpoint");
        public Task MuteAsync(string? endpointId) { MuteCount++; _savedBit ??= MuteBit; MuteBit = true; return Task.CompletedTask; }
        public async Task RestoreAsync()
        {
            RestoreCount++;
            if (_restores.TryDequeue(out var gate)) await gate.WaitAsync();
            if (FailRestore) throw new IOException("synthetic recovery endpoint restore failure");
            if (_savedBit is { } bit) { MuteBit = bit; _savedBit = null; }
        }
    }
    private sealed class Gate
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async Task WaitAsync() { Entered.TrySetResult(); await Release.Task; }
    }
    private sealed class FaultEngine : IEngineFactory
    {
        public FaultConnection Connection { get; } = new();
        public int CreatedCount { get; private set; }
        public IEngineConnection Open() { CreatedCount++; return Connection; }
    }
    private sealed class FaultConnection : IEngineConnection
    {
        private readonly Channel<JsonElement> _events = Channel.CreateUnbounded<JsonElement>();
        private string _session = "";
        public Task SendAsync(string session, string command, object? parameters, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            if (command == "start") { _session = session; Emit(new { @event = "streaming" }); }
            if (command == "stop") _events.Writer.TryComplete();
            return Task.CompletedTask;
        }
        public void Emit(object payload)
        {
            var value = JsonSerializer.SerializeToNode(payload)!.AsObject(); value["version"] = 1; value["session_id"] = _session;
            if (!_events.Writer.TryWrite(JsonSerializer.SerializeToElement(value))) throw new InvalidOperationException("Recovery fixture emitted after disposal.");
        }
        public async ValueTask<JsonElement?> ReadAsync(CancellationToken cancellation)
        {
            try { return await _events.Reader.ReadAsync(cancellation); }
            catch (ChannelClosedException) { return null; }
        }
        public ValueTask DisposeAsync() { _events.Writer.TryComplete(); return ValueTask.CompletedTask; }
    }
    private sealed class FixtureDiscovery : IDiscoveryService
    {
        // Endpoint identities do not add durable broadcast aliases. This keeps the
        // startup-preservation assertions independent of identity-migration saves.
        public const string ReceiverId = "192.0.2.10:7000";
        public int StartCount { get; private set; }
        public int Publications { get; private set; }
        private bool _requested;
        public string SelectedInterface { get; private set; } = "";
        public event Action<IReadOnlyList<Receiver>>? Changed;
        public event Action<string>? Failed { add { } remove { } }
        public event Action<bool>? RunningChanged;
        public void SetInterface(string id) => SelectedInterface = id;
        public void Start()
        {
            if (_requested) return;
            _requested = true;
            StartCount++; Publications++;
            RunningChanged?.Invoke(SelectedInterface.Length == 0);
            Changed?.Invoke(SelectedInterface.Length == 0 ? [new(ReceiverId, "Fixture receiver", "192.0.2.10")] : []);
        }
        public void Stop() { if (!_requested) return; _requested = false; RunningChanged?.Invoke(false); }
        public void Dispose() => Stop();
    }
}
