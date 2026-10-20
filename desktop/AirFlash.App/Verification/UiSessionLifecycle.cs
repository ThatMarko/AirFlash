using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Threading;
using AirFlash.App.Services;
using AirFlash.App.ViewModels;
using AirFlash.Core;
namespace AirFlash.App.Verification;

// Opt-in dispatcher fixtures for a future explicitly authorized UI qualification.
// Every application boundary is fake; this harness never constructs platform services.
internal static class UiSessionLifecycle
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(6);
    public static async Task RunAsync(List<string> checks, string directory)
    {
        _ = directory;
        await IdentityOwnershipAsync(checks);
        await AutomaticOwnershipAsync(checks);
        await AutomaticEligibilityAsync(checks);
        await AutomaticSaveFailureAsync(checks);
        await StopWaitingForAdmissionAsync(checks);
        await EligibilityArrivalAsync(checks);
        await ManualSelectionAsync(checks);
        await RetainedStatesAsync(checks);
        await ResetDuringAutomaticSaveAsync(checks);
        await ShutdownDuringAutomaticSaveAsync(checks);
    }
    private static Receiver Pod(string id, string address = "192.0.2.10", int port = 7000) => new(id, id, address, port);
    private static Receiver Pair(string id = "stereo:fixture") => Pod(id) with
    {
        StereoId = "fixture", Members = [Pod("left") with { IsLeader = true }, Pod("right", "192.0.2.11")]
    };
    private static AppSettings Settings(bool automatic = false) => new()
    {
        AutoConnectOnDiscover = automatic, ForceReconnect = false, MuteWhileStreaming = false, LastReceiverId = "previous"
    };
    private static void SeedAliases(AppSettings settings, params Receiver[] receivers)
    {
        foreach (var receiver in receivers.SelectMany(r => r.Peers))
            if (ReceiverIdentity.IsBroadcast(receiver.Id)) settings.ReceiverAliases[receiver.Id] = receiver.Id;
    }
    private static void Check(bool condition, string message, List<string> checks)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks.Add(message);
    }
    private static Task DrainAsync() => Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle).Task;
    private static async Task StateAsync(Fixture fixture, Func<SessionSnapshot, bool> predicate)
    {
        var arrived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void Changed(SessionSnapshot snapshot) { if (predicate(snapshot)) arrived.TrySetResult(); }
        fixture.App.Session.Changed += Changed;
        try
        {
            if (predicate(fixture.App.Session.Snapshot)) arrived.TrySetResult();
            await arrived.Task.WaitAsync(Limit);
            await DrainAsync();
        }
        finally { fixture.App.Session.Changed -= Changed; }
    }
    private static Task StateAsync(Fixture fixture, PlaybackState state) => StateAsync(fixture, snapshot => snapshot.State == state);
    private static async Task CatalogAsync(Fixture fixture, Func<bool> predicate)
    {
        var arrived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void Changed() { if (predicate()) arrived.TrySetResult(); }
        fixture.App.CatalogChanged += Changed;
        try
        {
            Changed(); await arrived.Task.WaitAsync(Limit);
        }
        finally { fixture.App.CatalogChanged -= Changed; }
    }
    private static async Task IdentityOwnershipAsync(List<string> checks)
    {
        foreach (var scenario in new[] { "absent", "changed", "same id", "save failure" })
        {
            var a = Pod("old-a"); var b = Pod("manual-b", "192.0.2.20", 7001) with { IsManual = true };
            var settings = Settings(); settings.ManualReceivers.Add(new(b.Id, b.Name, b.Address, b.Port));
            settings.Options(b.Id).LatencyMode = "custom"; settings.Options(b.Id).CustomBufferMs = 333;
            SeedAliases(settings, a);
            await using var fixture = await Fixture.CreateAsync(settings, [a]);
            await fixture.App.ToggleAsync(a); await StateAsync(fixture, PlaybackState.Streaming);
            var old = fixture.Engine.Connections.Single();
            var pin = new PendingPin(); var pairing = new ScriptedConnection(false);
            fixture.Engine.Enqueue(pairing);
            var gate = fixture.Store.Hold();
            var c = Pod("new-c", "192.0.2.30");
            var next = scenario is "changed" or "same id" ? new[] { a with { Address = "192.0.2.12", Port = 7009 }, c } : new[] { c };
            var reconcile = fixture.App.ApplyDiscoveryAsync(next);
            try
            {
                await gate.Entered.Task.WaitAsync(Limit);
                old.Emit(new { @event = "capture_metrics", metrics = new { underrun_packets = 7 } });
                await StateAsync(fixture, snapshot => snapshot.Metrics?.Underruns == 7);
                var selected = scenario == "same id" ? a with { Address = "192.0.2.25", Port = 7010 } : b;
                await fixture.App.PairAsync(selected, pin.RequestAsync);
                await pairing.PairSent.Task.WaitAsync(Limit);
                pairing.Emit(new { @event = "pin_required" });
                await pin.Entered.Task.WaitAsync(Limit);
                await StateAsync(fixture, PlaybackState.Pairing);
                var owned = fixture.App.Session.Capture();
                fixture.Store.Fail = scenario == "save failure";
                gate.Release.TrySetResult();
                try { await reconcile; } catch (IOException) when (fixture.Store.Fail) { }
                await DrainAsync();
                var current = fixture.App.Session.Capture();
                Check(current.Owner == owned.Owner && current.Snapshot.State == PlaybackState.Pairing &&
                    current.Snapshot.Receiver?.Id == selected.Id && current.Snapshot.Receiver.Address == selected.Address &&
                    ReferenceEquals(current.Snapshot.Diagnostics, owned.Snapshot.Diagnostics) && !pairing.Disposed &&
                    !pairing.Commands.Any(command => command.Name == "stop") && !pin.Cancelled,
                    "delayed identity reconciliation preserves the new pairing owner: " + scenario, checks);
                if (scenario != "same id")
                    Check(current.Snapshot.TargetLatency == 333, "stale identity work preserves manual pairing settings: " + scenario, checks);
                if (fixture.Store.Fail)
                    Check(fixture.App.AllReceivers.Any(receiver => receiver.Id == a.Id) &&
                        !fixture.App.Settings.ReceiverAliases.ContainsKey(c.Id), "failed identity save leaves the catalog and settings unchanged", checks);
            }
            finally
            {
                gate.Release.TrySetResult(); fixture.Store.Fail = false;
                await reconcile.WaitAsync(Limit);
                pin.Release.TrySetResult(null); await fixture.App.StopAsync();
            }
        }
    }
    private static async Task AutomaticOwnershipAsync(List<string> checks)
    {
        foreach (var scenario in new[] { "pairing", "streaming", "user stop", "same id" })
        {
            var a = Pod("auto-a"); var b = Pod("manual-b", "192.0.2.20", 7001) with { IsManual = true };
            var settings = Settings(true); settings.ManualReceivers.Add(new(b.Id, b.Name, b.Address, b.Port));
            settings.Options(b.Id).AutoConnect = false; SeedAliases(settings, a);
            await using var fixture = await Fixture.CreateAsync(settings, [a]);
            var gate = fixture.Store.Hold(); fixture.App.MasterVolume = 37;
            var automatic = fixture.App.TryAutoConnectAsync(); var pin = new PendingPin();
            var pairing = new ScriptedConnection(false) { CompletePair = scenario == "streaming" };
            fixture.Engine.Enqueue(pairing);
            try
            {
                await gate.Entered.Task.WaitAsync(Limit);
                Check(fixture.Engine.OpenCount == 0 && !automatic.IsCompleted, "automatic flush is held before opening a connection: " + scenario, checks);
                var selected = scenario == "same id" ? a : b;
                await fixture.App.PairAsync(selected, scenario == "streaming" ? (_, _) => Task.FromResult<string?>("1234") : pin.RequestAsync);
                await pairing.PairSent.Task.WaitAsync(Limit);
                if (scenario is "pairing" or "same id")
                {
                    pairing.Emit(new { @event = "pin_required" }); await pin.Entered.Task.WaitAsync(Limit);
                }
                await StateAsync(fixture, scenario == "streaming" ? PlaybackState.Streaming : PlaybackState.Pairing);
                var owned = fixture.App.Session.Capture(); var opens = fixture.Engine.OpenCount;
                if (scenario == "user stop")
                {
                    Check(!pin.Entered.Task.IsCompleted && fixture.App.StopCommand.CanExecute(null), "ordinary Stop is available before a PIN prompt opens", checks);
                    await fixture.App.StopAsync(); await StateAsync(fixture, PlaybackState.Idle);
                }
                gate.Release.TrySetResult(); await automatic; await DrainAsync();
                Check(fixture.Engine.OpenCount == opens && fixture.Store.Saved.MasterVolume == 37 && fixture.App.Settings.LastReceiverId == "previous",
                    "rejected automatic work preserves valid persistence without recording an attempt: " + scenario, checks);
                if (scenario == "user stop")
                {
                    await fixture.App.TryAutoConnectAsync();
                    Check(fixture.Engine.OpenCount == opens && fixture.App.Snapshot.State == PlaybackState.Idle,
                        "releasing a delayed automatic flush cannot undo user Stop suppression", checks);
                }
                else
                {
                    var current = fixture.App.Session.Capture();
                    Check(current.Owner == owned.Owner && current.Snapshot.Receiver?.Id == selected.Id &&
                        current.Snapshot.State == owned.Snapshot.State && !pin.Cancelled,
                        "delayed automatic admission cannot replace explicit ownership: " + scenario, checks);
                    await fixture.App.StopAsync(false); pin.Release.TrySetResult(null);
                    await fixture.App.TryAutoConnectAsync(); await StateAsync(fixture, PlaybackState.Streaming);
                    Check(fixture.App.Snapshot.Receiver?.Id == a.Id && fixture.Engine.OpenCount == opens + 1,
                        "a rejected automatic request leaves its candidate available for later valid admission: " + scenario, checks);
                }
            }
            finally { gate.Release.TrySetResult(); pin.Release.TrySetResult(null); await automatic.WaitAsync(Limit); }
        }
    }
    private static async Task AutomaticEligibilityAsync(List<string> checks)
    {
        foreach (var change in new[] { "removed", "incomplete", "hidden", "disabled" })
        {
            var group = Pair("stereo:eligible"); var settings = Settings(true); SeedAliases(settings, group);
            await using var fixture = await Fixture.CreateAsync(settings, [group]);
            var owner = fixture.App.Session.Capture().Owner;
            var gate = fixture.Store.Hold(); fixture.App.MasterVolume = 37;
            Task transaction = Task.CompletedTask;
            fixture.App.BeforeAutomaticAdmission = () => transaction;
            var automatic = fixture.App.TryAutoConnectAsync();
            try
            {
                await gate.Entered.Task.WaitAsync(Limit);
                if (change == "removed") transaction = fixture.App.ApplyDiscoveryAsync([]);
                else if (change == "incomplete") transaction = fixture.App.ApplyDiscoveryAsync([group with { Members = [group.Members[0]] }]);
                else
                {
                    var baseline = fixture.App.Settings.Clone(); var draft = baseline.Clone();
                    if (change == "hidden") draft.Options(group.Id).Hidden = true;
                    else draft.Options(group.Id).AutoConnect = false;
                    transaction = fixture.App.ApplyAsync(baseline, draft);
                }
                gate.Release.TrySetResult(); await transaction; await automatic;
                Check(fixture.App.Session.Capture().Owner == owner && fixture.Engine.OpenCount == 0 &&
                    fixture.Store.Saved.MasterVolume == 37 && fixture.App.Settings.LastReceiverId == "previous",
                    "automatic admission rechecks eligibility after persistence with the same lifecycle owner: " + change, checks);
                fixture.App.BeforeAutomaticAdmission = null;
                if (change is "removed" or "incomplete") await fixture.App.ApplyDiscoveryAsync([group]);
                else
                {
                    var baseline = fixture.App.Settings.Clone(); var draft = baseline.Clone();
                    draft.Options(group.Id).Hidden = false; draft.Options(group.Id).AutoConnect = null;
                    await fixture.App.ApplyAsync(baseline, draft);
                }
                await fixture.App.TryAutoConnectAsync(); await StateAsync(fixture, PlaybackState.Streaming);
                Check(fixture.Engine.OpenCount == 1, "rejected eligibility changes do not consume the receiver's automatic attempt: " + change, checks);
            }
            finally
            {
                gate.Release.TrySetResult(); fixture.App.BeforeAutomaticAdmission = null;
                await Task.WhenAll(transaction, automatic).WaitAsync(Limit);
            }
        }
    }
    private static async Task StopWaitingForAdmissionAsync(List<string> checks)
    {
        var a = Pod("auto-a"); var settings = Settings(true); SeedAliases(settings, a);
        await using var fixture = await Fixture.CreateAsync(settings, [a]);
        var restore = fixture.Audio.HoldRestore();
        var updating = fixture.App.Session.UpdateSettingsAsync(settings);
        Task automatic = Task.CompletedTask, stopping = Task.CompletedTask;
        try
        {
            await restore.Entered.Task.WaitAsync(Limit);
            // This call reaches the held controller semaphore synchronously: no dirty save or admission hook intervenes.
            automatic = fixture.App.TryAutoConnectAsync();
            Check(!automatic.IsCompleted, "automatic admission waits behind serialized controller work", checks);
            stopping = fixture.App.StopAsync();
            Check(!stopping.IsCompleted, "user Stop invalidates automatic intent before its controller wait finishes", checks);
            restore.Release.TrySetResult(); await Task.WhenAll(updating, automatic, stopping);
            await fixture.App.TryAutoConnectAsync();
            Check(fixture.Engine.OpenCount == 0 && fixture.App.Session.Snapshot.State == PlaybackState.Idle &&
                fixture.App.Settings.LastReceiverId == "previous", "Stop while automatic admission waits for serialization prevents Start and preserves suppression", checks);
        }
        finally { restore.Release.TrySetResult(); await Task.WhenAll(updating, automatic, stopping).WaitAsync(Limit); }
    }
    private static async Task AutomaticSaveFailureAsync(List<string> checks)
    {
        var a = Pod("save-failure-a"); var settings = Settings(true); SeedAliases(settings, a);
        await using var fixture = await Fixture.CreateAsync(settings, [a]);
        var gate = fixture.Store.Hold(); fixture.App.MasterVolume = 37;
        var automatic = fixture.App.TryAutoConnectAsync();
        var failed = false;
        try
        {
            await gate.Entered.Task.WaitAsync(Limit); fixture.Store.Fail = true; gate.Release.TrySetResult();
            try { await automatic; } catch (IOException) { failed = true; }
        }
        finally
        {
            fixture.Store.Fail = false; gate.Release.TrySetResult();
            try { await automatic.WaitAsync(Limit); } catch (IOException) when (failed) { }
        }
        Check(failed && fixture.Engine.OpenCount == 0 && fixture.App.Settings.LastReceiverId == "previous" && fixture.Store.Saved.MasterVolume == 100,
            "failed automatic persistence prevents admission and leaves attempts and last-used untouched", checks);
        await fixture.App.TryAutoConnectAsync(); await StateAsync(fixture, PlaybackState.Streaming);
        Check(fixture.Engine.OpenCount == 1 && fixture.Store.Saved.MasterVolume == 37 && fixture.App.Settings.LastReceiverId == a.Id,
            "a candidate rejected by save failure can make a later valid automatic attempt", checks);
    }
    private static async Task ShutdownDuringAutomaticSaveAsync(List<string> checks)
    {
        var a = Pod("shutdown-a"); var settings = Settings(true); SeedAliases(settings, a);
        await using var fixture = await Fixture.CreateAsync(settings, [a]);
        var gate = fixture.Store.Hold(); fixture.App.MasterVolume = 29;
        var automatic = fixture.App.TryAutoConnectAsync();
        Task shutdown = Task.CompletedTask;
        try
        {
            await gate.Entered.Task.WaitAsync(Limit);
            shutdown = fixture.App.DisposeAsync().AsTask();
            var responsive = false;
            await Application.Current.Dispatcher.InvokeAsync(() => responsive = true, DispatcherPriority.Input);
            Check(responsive && !shutdown.IsCompleted, "shutdown waits asynchronously for held automatic persistence while the dispatcher stays responsive", checks);
            gate.Release.TrySetResult(); await Task.WhenAll(automatic, shutdown);
            Check(fixture.Engine.OpenCount == 0 && fixture.Store.Saved.MasterVolume == 29 && fixture.Store.Saved.LastReceiverId == "previous",
                "shutdown preserves valid pending edits and rejects delayed automatic admission", checks);
        }
        finally { gate.Release.TrySetResult(); await Task.WhenAll(automatic, shutdown).WaitAsync(Limit); }
    }

    private static async Task EligibilityArrivalAsync(List<string> checks)
    {
        foreach (var change in new[] { "discovery", "apply" })
        {
            var receiver = Pod("arrival-a"); var settings = Settings(true); SeedAliases(settings, receiver);
            await using var fixture = await Fixture.CreateAsync(settings, [receiver]);
            var restore = fixture.Audio.HoldRestore(); Task automatic = Task.CompletedTask, transaction = Task.CompletedTask;
            var holding = fixture.App.Session.UpdateSettingsAsync(settings);
            try
            {
                await restore.Entered.Task.WaitAsync(Limit);
                var captured = fixture.App.Session.Capture();
                automatic = fixture.App.TryAutoConnectAsync();
                Check(!automatic.IsCompleted, "automatic request holds the settings gate while waiting for controller serialization: " + change, checks);
                if (change == "discovery")
                {
                    // Publish invokes the real AppViewModel subscription immediately; reconciliation
                    // itself is still queued behind the settings gate held by automatic admission.
                    transaction = CatalogAsync(fixture, () => fixture.App.AllReceivers.All(candidate => !candidate.Online));
                    fixture.Discovery.Items = []; fixture.Discovery.Publish();
                }
                else
                {
                    var baseline = fixture.App.Settings.Clone(); var draft = baseline.Clone();
                    draft.Options(receiver.Id).Hidden = true;
                    transaction = fixture.App.ApplyAsync(baseline, draft);
                    Check(!transaction.IsCompleted, "settings Apply is queued behind automatic admission", checks);
                }
                Check(fixture.App.Session.Capture().Owner == captured.Owner &&
                    fixture.App.Session.Capture().AutomaticIntent == captured.AutomaticIntent,
                    "queued eligibility arrivals do not invalidate explicit intent or replace its owner: " + change, checks);
                restore.Release.TrySetResult();
                await Task.WhenAll(holding, automatic, transaction).WaitAsync(Limit);
                await DrainAsync();
                Check(fixture.Engine.OpenCount == 0 && fixture.App.Settings.LastReceiverId == "previous",
                    "arrival fence rejects stale automatic admission before engine startup: " + change, checks);
                if (change == "discovery") await fixture.App.ApplyDiscoveryAsync([receiver]);
                else
                {
                    var baseline = fixture.App.Settings.Clone(); var draft = baseline.Clone();
                    draft.Options(receiver.Id).Hidden = false;
                    await fixture.App.ApplyAsync(baseline, draft);
                }
                await fixture.App.TryAutoConnectAsync(); await StateAsync(fixture, PlaybackState.Streaming);
                Check(fixture.Engine.OpenCount == 1, "arrival rejection leaves the receiver available for a later valid attempt: " + change, checks);
            }
            finally { restore.Release.TrySetResult(); await Task.WhenAll(holding, automatic, transaction).WaitAsync(Limit); }
        }
    }
    private static async Task ManualSelectionAsync(List<string> checks)
    {
        foreach (var port in new[] { 7000, 7001 })
        foreach (var condition in new[] { "complete", "incomplete", "hidden" })
        {
            var group = Pair(); if (condition == "incomplete") group = group with { Members = [group.Members[0]] };
            var manual = Pod("manual-m", "192.0.2.10", port) with { IsManual = true };
            var settings = Settings(); settings.MasterVolume = 10;
            settings.ManualReceivers.Add(new(manual.Id, manual.Name, manual.Address, port));
            settings.Options(manual.Id).LatencyMode = "custom"; settings.Options(manual.Id).CustomBufferMs = 333;
            settings.Options(group.Id).LatencyMode = "buffered";
            settings.Options(group.Id).Hidden = condition == "hidden"; SeedAliases(settings, group);
            await using var fixture = await Fixture.CreateAsync(settings, [group]);
            await fixture.App.ToggleAsync(fixture.App.AllReceivers.Single(receiver => receiver.Id == manual.Id));
            await StateAsync(fixture, PlaybackState.Streaming);
            var command = fixture.Engine.Connections.Single().Commands.Single(command => command.Name == "start");
            var peers = command.Parameters.GetProperty("peers");
            Check(peers.GetArrayLength() == 1 && peers[0].Text("host") == manual.Address && peers[0].Integer("port") == port &&
                command.Parameters.Integer("latency_ms") == 333 && command.Parameters.Number("gain") == .1 &&
                fixture.App.Snapshot.Receiver?.Id == manual.Id && fixture.App.Settings.LastReceiverId == manual.Id,
                $"manual route preserves its exact endpoint, identity and options beside a {condition} native group at port {port}", checks);
            await fixture.App.ApplyDiscoveryAsync([]);
            Check(fixture.App.Snapshot.IsActive && fixture.App.AllReceivers.Single(receiver => receiver.IsManual).Id == manual.Id,
                "manual persistence survives empty discovery: " + condition + "/" + port, checks);
        }
    }
    private static async Task RetainedStatesAsync(List<string> checks)
    {
        foreach (var original in new[] { false, true })
        foreach (var state in new[] { PlaybackState.Connecting, PlaybackState.Pairing, PlaybackState.Streaming, PlaybackState.Standby })
        {
            var group = Pair(); var settings = Settings(); settings.MuteWhileStreaming = true;
            settings.StandbyEnabled = true; SeedAliases(settings, group);
            await using var fixture = await Fixture.CreateAsync(settings, [group], original);
            var process = new ScriptedConnection(state is PlaybackState.Streaming or PlaybackState.Standby);
            fixture.Engine.Enqueue(process);
            if (state == PlaybackState.Pairing)
                await fixture.App.PairAsync(group, (_, _) => throw new InvalidOperationException("This fixture holds Pair before pin_required."));
            else await fixture.App.ToggleAsync(group);
            await (state == PlaybackState.Pairing ? process.PairSent.Task : process.StartSent.Task).WaitAsync(Limit);
            if (state == PlaybackState.Standby)
            {
                await StateAsync(fixture, PlaybackState.Streaming);
                process.Emit(new { @event = "standby", reason = "silence" });
            }
            await StateAsync(fixture, state);
            var owner = fixture.App.Session.Capture().Owner;
            var snapshots = new IReadOnlyList<Receiver>[] { [], [group with { Members = [group.Members[0]] }], [], [] };
            foreach (var snapshot in snapshots)
            {
                await fixture.App.ApplyDiscoveryAsync(snapshot); await DrainAsync();
                var current = fixture.App.Session.Capture();
                var row = fixture.App.Receivers.Single(receiver => receiver.Receiver.Id == group.Id);
                var catalog = fixture.App.AllReceivers.Single(receiver => receiver.Id == group.Id);
                Check(current.Owner == owner && current.Snapshot.State == state && current.Snapshot.Receiver?.Members.Length == 2 &&
                    fixture.Engine.OpenCount == 1 && !process.Disposed && !process.Commands.Any(command => command.Name == "stop") &&
                    (!catalog.Online || !catalog.Complete) && row.Active && row.CanPlay && row.ToggleCommand.CanExecute(null) &&
                    row.StateText.Contains(L.Get("Discovery unavailable"), StringComparison.Ordinal),
                    "passive absence retains transport and the Stop command in " + state + " (prior mute " + original + ")", checks);
            }
            await fixture.App.ApplyDiscoveryAsync([group]); await DrainAsync();
            Check(fixture.Engine.OpenCount == 1 && fixture.App.Session.Capture().Owner == owner &&
                !fixture.App.Receivers.Single().StateText.Contains(L.Get("Discovery unavailable"), StringComparison.Ordinal),
                "reappearance restores catalog presentation without replacing " + state, checks);
            await fixture.App.ApplyDiscoveryAsync([]); await DrainAsync();
            // Pairing remains before pin_required: no modal PIN dialog hides this command.
            await fixture.App.ToggleAsync(fixture.App.Receivers.Single().Receiver);
            await StateAsync(fixture, PlaybackState.Idle); await process.DisposalCompleted.Task.WaitAsync(Limit);
            Check(fixture.App.Receivers.Count == 0 && fixture.Engine.OpenCount == 1 &&
                fixture.Audio.MuteBit == original && !fixture.Audio.HasSavedBit,
                "explicit Stop removes the inactive overlay and restores the exact prior mute bit from " + state + "/" + original, checks);
        }
    }
    private static async Task ResetDuringAutomaticSaveAsync(List<string> checks)
    {
        var receiver = Pod("reset-a"); var settings = Settings(true); SeedAliases(settings, receiver);
        await using var fixture = await Fixture.CreateAsync(settings, [receiver]);
        var save = fixture.Store.Hold(); fixture.App.MasterVolume = 37;
        var automatic = fixture.App.TryAutoConnectAsync(); Task reset = Task.CompletedTask;
        try
        {
            await save.Entered.Task.WaitAsync(Limit);
            var captured = fixture.App.Session.Capture(); reset = fixture.App.ResetAsync();
            Check(!reset.IsCompleted && fixture.App.Session.Capture().AutomaticIntent != captured.AutomaticIntent,
                "Reset invalidates delayed automatic intent synchronously before its settings gate wait", checks);
            save.Release.TrySetResult(); await Task.WhenAll(automatic, reset).WaitAsync(Limit); await DrainAsync();
            Check(fixture.Engine.OpenCount == 0 && fixture.Store.ResetCalls == 1 && fixture.Restart.Calls == 1 &&
                fixture.App.IsClosing && fixture.App.Session.Snapshot.State == PlaybackState.Idle &&
                fixture.Store.Saved.LastReceiverId is null && !fixture.Audio.HasSavedBit,
                "Reset rejects stale Start, clears the synthetic store and schedules only the fake restart", checks);
        }
        finally { save.Release.TrySetResult(); await Task.WhenAll(automatic, reset).WaitAsync(Limit); }
    }
    private sealed class Gate
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async Task WaitAsync() { Entered.TrySetResult(); await Release.Task; }
    }
    private sealed class Store(AppSettings initial) : ISettingsStore
    {
        private AppSettings _saved = initial.Clone();
        public bool Fail { get; set; }
        public Gate? SaveGate { get; private set; }
        public AppSettings Saved => _saved.Clone();
        public AppSettings Load() => _saved.Clone();
        public Gate Hold() => SaveGate = new();
        public int ResetCalls { get; private set; }
        public Task ResetAsync() { ResetCalls++; _saved = new(); return Task.CompletedTask; }
        public void Save(AppSettings settings)
        {
            if (Fail) throw new IOException("synthetic save failure");
            _saved = settings.Clone();
        }
        public async Task SaveAsync(AppSettings settings)
        {
            if (SaveGate is { } gate) await gate.WaitAsync();
            Save(settings);
        }
    }
    private sealed class Audio(bool original) : IAudioService
    {
        public bool OriginalBit { get; } = original;
        public bool MuteBit { get; private set; } = original;
        public bool HasSavedBit => _saved is not null;
        private bool? _saved;
        public int RestoreCalls { get; private set; }
        public List<bool> RestoredBits { get; } = [];
        private Gate? _restoreGate;
        private readonly ConcurrentQueue<Gate> _nextRestores = new();
        private Gate? _defaultEndpointGate;
        public string? DefaultEndpoint { get; set; } = "synthetic-endpoint";
        public bool FailRestore { get; set; }
        public Gate HoldRestore() => _restoreGate = new();
        public Gate HoldNextRestore() { var gate = new Gate(); _nextRestores.Enqueue(gate); return gate; }
        public Gate HoldDefaultEndpoint() => _defaultEndpointGate = new();
        public void Release() { _restoreGate?.Release.TrySetResult(); _defaultEndpointGate?.Release.TrySetResult(); }
        public event Action? EndpointsChanged { add { } remove { } }
        public Task<IReadOnlyList<AudioEndpoint>> GetEndpointsAsync() => Task.FromResult<IReadOnlyList<AudioEndpoint>>([]);
        public async Task<string?> GetDefaultEndpointIdAsync()
        {
            if (_defaultEndpointGate is { } gate) await gate.WaitAsync();
            return DefaultEndpoint;
        }
        public Task MuteAsync(string? endpointId)
        {
            _saved ??= MuteBit; MuteBit = true; return Task.CompletedTask;
        }
        public async Task RestoreAsync()
        {
            RestoreCalls++;
            if (_nextRestores.TryDequeue(out var next)) await next.WaitAsync();
            else if (_restoreGate is { } gate) await gate.WaitAsync();
            if (FailRestore) throw new IOException("synthetic endpoint restore failure");
            if (_saved is { } saved) { MuteBit = saved; RestoredBits.Add(saved); _saved = null; }
        }
    }
    private sealed class PendingPin
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<string?> Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool Cancelled { get; private set; }
        public async Task<string?> RequestAsync(Receiver member, CancellationToken cancellation)
        {
            Entered.TrySetResult();
            try { return await Release.Task.WaitAsync(cancellation); }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { Cancelled = true; throw; }
        }
    }
    private sealed record Command(string Session, string Name, JsonElement Parameters);
    private sealed class ScriptedConnection(bool streaming = true) : IEngineConnection
    {
        private readonly Channel<JsonElement> _events = Channel.CreateUnbounded<JsonElement>();
        public ConcurrentQueue<Command> Commands { get; } = new();
        public TaskCompletionSource StartSent { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource PairSent { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource DisposalCompleted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool CompletePair { get; init; }
        public bool Disposed { get; private set; }
        public string Session { get; private set; } = "";
        public Task SendAsync(string session, string command, object? parameters, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            Commands.Enqueue(new(session, command, JsonSerializer.SerializeToElement(parameters ?? new { })));
            if (command == "hello") { Session = session; Emit(new { @event = "hello", capabilities = new[] { "receiver_profiles", "software_stereo", "continuity", "buffered_audio", "standby_release", "member_volume", "receiver_password", "application_capture" } }); }
            if (command == "start") { Session = session; StartSent.TrySetResult(); if (streaming) Emit(new { @event = "streaming" }); }
            if (command == "pair") { Session = session; PairSent.TrySetResult(); if (CompletePair) Emit(new { @event = "pin_required" }); }
            if (command == "pair_pin" && CompletePair) Emit(new { @event = "paired" });
            if (command == "stop") _events.Writer.TryComplete();
            return Task.CompletedTask;
        }
        public void Emit(object payload)
        {
            var value = JsonSerializer.SerializeToNode(payload)!.AsObject();
            value["version"] = 1; value["session_id"] = Session;
            if (!_events.Writer.TryWrite(JsonSerializer.SerializeToElement(value))) throw new InvalidOperationException("Fixture event emitted after cleanup.");
        }
        public async ValueTask<JsonElement?> ReadAsync(CancellationToken cancellation)
        {
            try { return await _events.Reader.ReadAsync(cancellation); }
            catch (ChannelClosedException) { return null; }
        }
        public ValueTask DisposeAsync()
        {
            Disposed = true; _events.Writer.TryComplete(); DisposalCompleted.TrySetResult(); return ValueTask.CompletedTask;
        }
    }
    private sealed class Engine : IEngineFactory
    {
        private readonly ConcurrentQueue<ScriptedConnection> _next = new();
        public ConcurrentQueue<ScriptedConnection> Connections { get; } = new();
        public int OpenCount;
        public bool FailNext { get; set; }
        public void Enqueue(ScriptedConnection connection) => _next.Enqueue(connection);
        public IEngineConnection Open()
        {
            Interlocked.Increment(ref OpenCount);
            if (FailNext) { FailNext = false; throw new IOException("synthetic open failure"); }
            var connection = _next.TryDequeue(out var scripted) ? scripted : new ScriptedConnection();
            Connections.Enqueue(connection); return connection;
        }
    }
    private sealed class Restart : IRestartService
    {
        public int Calls { get; private set; }
        public void Schedule() => Calls++;
    }
    private sealed class Fixture : IAsyncDisposable
    {
        public Store Store { get; }
        public Audio Audio { get; }
        public Engine Engine { get; } = new();
        public UiSmoke.MockDiscovery Discovery { get; } = new() { Items = [] };
        public Restart Restart { get; } = new();
        public AppViewModel App { get; }
        private Fixture(AppSettings settings, bool original, EngineInitialization? initialization)
        {
            Store = new(settings); Audio = new(original);
            App = new(Store, Discovery, new UiSmoke.MockAutostart(), Audio, Engine, Application.Current.Dispatcher, initialization, restart: Restart);
            App.UseControlledScheduling();
        }
        public static async Task<Fixture> CreateAsync(AppSettings settings, IReadOnlyList<Receiver> receivers, bool original = false, EngineInitialization? initialization = null)
        {
            var fixture = new Fixture(settings, original, initialization);
            try
            {
                fixture.App.Start(); await fixture.App.Startup; await DrainAsync();
                await fixture.App.ApplyDiscoveryAsync(receivers); await DrainAsync(); return fixture;
            }
            catch { await fixture.DisposeAsync(); throw; }
        }
        public async ValueTask DisposeAsync()
        {
            Store.SaveGate?.Release.TrySetResult(); Audio.Release(); await App.DisposeAsync();
        }
    }
}
