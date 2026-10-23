using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using AirFlash.Core;
using Xunit;
namespace AirFlash.Tests;

public sealed class IpcFailureTests
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(4);
    // These fixtures assert event classification, not scheduler speed. The
    // finite read watchdog has its own explicitly quiet playback control.
    private static readonly SessionTiming Timing = SessionTiming.Default with { Pin = Limit, Retry = TimeSpan.Zero };
    private const string Rejection = "{\"version\":1,\"id\":\"\",\"session_id\":\"\",\"event\":\"error\",\"code\":\"command_error\",\"channel\":\"ipc\",\"host\":null,\"retryable\":false,\"message\":\"synthetic command rejection\"}";
    private static AppSettings Settings() => new() { ForceReconnect = true, MaxReconnectAttempts = 1, MuteWhileStreaming = true };
    private static Receiver Receiver() => new("synthetic-ipc", "Synthetic IPC", "192.0.2.10", 7000);
    private static TaskCompletionSource Signal() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    [Theory]
    [InlineData("connecting", false)] [InlineData("connecting", true)]
    [InlineData("streaming", false)] [InlineData("streaming", true)]
    [InlineData("pairing", false)] [InlineData("pairing", true)]
    public async Task ExactProcessRejectionKeepsItsFaultAndTerminatesTheOwnedAttempt(string phase, bool originalMute)
    {
        var connection = new Connection(); var factory = new Factory(connection); var audio = new Audio(originalMute);
        await using var controller = new SessionController(factory, audio, timing: Timing);
        var fault = Signal(); var streaming = Signal();
        controller.Changed += snapshot => { if (snapshot.State == PlaybackState.Error) fault.TrySetResult(); if (snapshot.State == PlaybackState.Streaming) streaming.TrySetResult(); };
        if (phase == "pairing") await controller.PairAsync(Receiver(), Settings(), (_, _) => throw new InvalidOperationException("No PIN request is scripted"));
        else await controller.StartAsync(Receiver(), Settings());
        await connection.Started.Task.WaitAsync(Limit);
        await connection.Starting.Task.WaitAsync(Limit);
        if (phase == "streaming") { connection.Current("streaming"); await streaming.Task.WaitAsync(Limit); Assert.True(audio.Muted); }
        var restores = audio.Restores;
        connection.Raw(Rejection);
        await fault.Task.WaitAsync(Limit);
        var failed = controller.Snapshot;
        Assert.NotNull(failed.Diagnostics?.LastFault);
        var notice = failed.Diagnostics!.LastFault!;
        Assert.Equal("command_error", notice.Code); Assert.Equal("ipc", notice.Channel);
        Assert.Equal("", notice.Host); Assert.Equal(false, notice.Retryable);
        Assert.Equal("synthetic command rejection", notice.Message);
        Assert.Equal(notice.Detail, failed.Message);
        // Playback restores in its catch and outer finally; pairing has only the outer finally.
        await audio.WhenRestoredAtLeastAsync(restores + (phase == "pairing" ? 1 : 2)).WaitAsync(Limit);
        Assert.True(connection.Disposed); Assert.Contains("stop", connection.Commands);
        Assert.Equal(originalMute, audio.Muted); Assert.Equal(1, factory.OpenCount);
        Assert.Equal(0, failed.Diagnostics.ReconnectCount);
    }

    public static IEnumerable<object[]> NearMatches()
    {
        foreach (var (key, value) in new (string, object?)[]
        {
            ("version", 2), ("version", "1"), ("event", "equalizer_error"), ("code", "engine_error"),
            ("channel", "setup"), ("host", "192.0.2.11"), ("retryable", true), ("retryable", "false"),
            ("id", "other-request"), ("id", null), ("session_id", "stale-attempt"), ("session_id", null)
        })
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object?>>(Rejection)!; data[key] = value;
            yield return [JsonSerializer.Serialize(data)];
        }
        foreach (var key in new[] { "version", "event", "code", "channel", "host", "retryable", "id", "session_id" })
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, object?>>(Rejection)!; data.Remove(key);
            yield return [JsonSerializer.Serialize(data)];
        }
    }
    [Theory] [MemberData(nameof(NearMatches))]
    public async Task NearMatchGlobalAndStaleEnvelopesRemainFiltered(string envelope)
    {
        var connection = new Connection(); var factory = new Factory(connection);
        await using var controller = new SessionController(factory, new Audio(false), timing: Timing);
        var streaming = Signal(); var fault = new TaskCompletionSource<SessionSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        controller.Changed += snapshot => { if (snapshot.State == PlaybackState.Streaming) streaming.TrySetResult(); if (snapshot.State == PlaybackState.Error) fault.TrySetResult(snapshot); };
        await controller.StartAsync(Receiver(), Settings()); await AwaitPhaseAsync(connection.Starting.Task, fault.Task, "the Start command");
        connection.Raw(envelope); connection.Current("streaming");
        await AwaitPhaseAsync(streaming.Task, fault.Task, "Streaming");
        Assert.Null(controller.Snapshot.Diagnostics?.LastFault); Assert.Equal(1, factory.OpenCount);
        await controller.StopAsync(); Assert.True(connection.Disposed);
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task NearMatchBeforeHelloOnlyFailsWhenPlaybackArrivesPrematurely(bool prematureStreaming)
    {
        var connection = new Connection(); var factory = new Factory(connection); var audio = new Audio(false);
        await using var controller = new SessionController(factory, audio, timing: Timing);
        var helloEntered = Signal(); var releaseHello = Signal(); var streaming = Signal();
        var fault = new TaskCompletionSource<SessionSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.BeforeHelloReply = async cancellation => { helloEntered.TrySetResult(); await releaseHello.Task.WaitAsync(cancellation); };
        controller.Changed += snapshot => { if (snapshot.State == PlaybackState.Streaming) streaming.TrySetResult(); if (snapshot.State == PlaybackState.Error) fault.TrySetResult(snapshot); };
        try
        {
            var settings = Settings(); settings.ForceReconnect = false;
            await controller.StartAsync(Receiver(), settings); await helloEntered.Task.WaitAsync(Limit);
            var restores = audio.Restores;
            // This is the old producer order: Started does not establish that
            // hello has been published, let alone accepted by the controller.
            await connection.Started.Task.WaitAsync(Limit);
            var envelope = JsonSerializer.Deserialize<Dictionary<string, object?>>(Rejection)!;
            envelope.Remove("event"); connection.Raw(JsonSerializer.Serialize(envelope));
            if (prematureStreaming) connection.Current("streaming");
            releaseHello.TrySetResult();
            if (prematureStreaming)
            {
                var failed = await fault.Task.WaitAsync(Limit);
                Assert.Equal(PlaybackState.Error, failed.State);
                Assert.Equal("host_error", failed.Diagnostics?.LastFault?.Code); Assert.Equal("desktop", failed.Diagnostics?.LastFault?.Channel);
                Assert.Null(failed.Diagnostics?.LastFault?.Retryable);
                Assert.Equal(L.Get("Incompatible engine protocol version."), failed.Diagnostics?.LastFault?.Message);
                Assert.Equal(failed.Diagnostics?.LastFault?.Detail, failed.Message);
                await audio.WhenRestoredAtLeastAsync(restores + 2).WaitAsync(Limit);
                Assert.False(streaming.Task.IsCompleted); Assert.False(connection.Starting.Task.IsCompleted);
                Assert.DoesNotContain("start", connection.Commands); Assert.Contains("stop", connection.Commands);
            }
            else
            {
                await AwaitPhaseAsync(connection.Starting.Task, fault.Task, "the Start command");
                connection.Current("streaming"); await AwaitPhaseAsync(streaming.Task, fault.Task, "Streaming");
                Assert.Null(controller.Snapshot.Diagnostics?.LastFault); Assert.True(audio.Muted);
                await controller.StopAsync(); Assert.Equal(PlaybackState.Idle, controller.Snapshot.State);
            }
            Assert.True(connection.Disposed); Assert.False(audio.Muted); Assert.Equal(1, factory.OpenCount);
        }
        finally { releaseHello.TrySetResult(); }
    }

    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task QuietPlaybackStillExpiresTheFiniteReadWatchdogAndRestoresMute(bool originalMute)
    {
        var connection = new Connection(); var factory = new Factory(connection); var audio = new Audio(originalMute);
        await using var controller = new SessionController(factory, audio, timing: Timing with { Read = TimeSpan.FromMilliseconds(150) });
        var streaming = new TaskCompletionSource<(bool Muted, int Restores)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var fault = new TaskCompletionSource<SessionSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        var silentRead = Signal(); var releaseRead = Signal();
        controller.Changed += snapshot =>
        {
            if (snapshot.State == PlaybackState.Streaming) streaming.TrySetResult((audio.Muted, audio.Restores));
            if (snapshot.State == PlaybackState.Error) fault.TrySetResult(snapshot);
        };
        connection.BeforeRead = async (read, cancellation) =>
        {
            // Hello and Streaming occupy reads one and two. Hold only the
            // subsequent idle playback read until its own deadline cancels it.
            if (read != 3) return;
            silentRead.TrySetResult(); await releaseRead.Task.WaitAsync(cancellation);
        };
        try
        {
            var settings = Settings(); settings.ForceReconnect = false;
            await controller.StartAsync(Receiver(), settings); await AwaitPhaseAsync(connection.Starting.Task, fault.Task, "the Start command");
            connection.Current("streaming"); await AwaitPhaseAsync(streaming.Task, fault.Task, "Streaming");
            var started = await streaming.Task;
            Assert.True(started.Muted);
            await silentRead.Task.WaitAsync(Limit);
            var failed = await fault.Task.WaitAsync(Limit);
            Assert.Equal("host_error", failed.Diagnostics?.LastFault?.Code); Assert.Equal("desktop", failed.Diagnostics?.LastFault?.Channel);
            Assert.Equal(L.Get("The audio engine response timed out."), failed.Diagnostics?.LastFault?.Message);
            await audio.WhenRestoredAtLeastAsync(started.Restores + 2).WaitAsync(Limit);
            Assert.True(connection.Disposed); Assert.Contains("stop", connection.Commands);
            Assert.Equal(originalMute, audio.Muted); Assert.Equal(1, factory.OpenCount);
            await controller.StopAsync(); Assert.Equal(PlaybackState.Idle, controller.Snapshot.State);
        }
        finally { releaseRead.TrySetResult(); }
    }

    private static async Task AwaitPhaseAsync(Task phase, Task<SessionSnapshot> fault, string expected)
    {
        if (await Task.WhenAny(phase, fault).WaitAsync(Limit) == fault)
        {
            var failed = await fault;
            Assert.Fail($"Expected {expected}; observed {failed.State}: {failed.Diagnostics?.LastFault?.Detail ?? failed.Message}");
        }
        await phase;
    }

    [Theory] [InlineData("[]")] [InlineData("null")] [InlineData("42")] [InlineData("\"text\"")] [InlineData("{")]
    public async Task MalformedOutputIsAControlledFinalIpcFailure(string raw)
    {
        var connection = new Connection(); var factory = new Factory(connection);
        await using var controller = new SessionController(factory, new Audio(false), timing: Timing);
        var fault = Signal(); controller.Changed += snapshot => { if (snapshot.State == PlaybackState.Error) fault.TrySetResult(); };
        await controller.StartAsync(Receiver(), Settings()); await connection.Starting.Task.WaitAsync(Limit); connection.Raw(raw);
        await fault.Task.WaitAsync(Limit);
        var notice = controller.Snapshot.Diagnostics!.LastFault!;
        Assert.Equal("protocol_error", notice.Code); Assert.Equal("ipc", notice.Channel); Assert.Equal(false, notice.Retryable);
        Assert.True(connection.Disposed); Assert.Equal(1, factory.OpenCount);
    }

    [Fact]
    public async Task CurrentStructuredPairingFailureKeepsClassificationWithoutPlayback()
    {
        var connection = new Connection(); var factory = new Factory(connection);
        await using var controller = new SessionController(factory, new Audio(false), timing: Timing);
        var fault = Signal(); controller.Changed += snapshot => { if (snapshot.State == PlaybackState.Error) fault.TrySetResult(); };
        await controller.PairAsync(Receiver(), Settings(), (_, _) => throw new InvalidOperationException("No PIN requested"));
        await connection.Starting.Task.WaitAsync(Limit);
        connection.Current("error", new { code = "authentication_failed", channel = "authentication", host = "192.0.2.10", retryable = false, message = "synthetic verify rejected" });
        await fault.Task.WaitAsync(Limit);
        Assert.Equal("authentication_failed", controller.Snapshot.Diagnostics?.LastFault?.Code);
        Assert.Equal(false, controller.Snapshot.Diagnostics?.LastFault?.Retryable);
        Assert.DoesNotContain("start", connection.Commands); Assert.True(connection.Disposed);
    }

    [Fact]
    public async Task LifetimeCancellationRemainsIdleAndDisposesItsConnection()
    {
        var connection = new Connection(); var factory = new Factory(connection);
        await using var controller = new SessionController(factory, new Audio(true), timing: Timing);
        await controller.StartAsync(Receiver(), Settings()); await connection.Started.Task.WaitAsync(Limit);
        await connection.Reading.Task.WaitAsync(Limit);
        await controller.StopAsync().WaitAsync(Limit);
        connection.Raw(Rejection);
        Assert.Equal(PlaybackState.Idle, controller.Snapshot.State); Assert.Null(controller.Snapshot.Diagnostics?.LastFault);
        Assert.True(connection.Disposed); Assert.Equal(1, factory.OpenCount);
    }

    [Theory] [InlineData(Rejection)] [InlineData("[]")] [InlineData("{")]
    public async Task StartupKeepsTheIpcCauseAndDisposesTheProbe(string raw)
    {
        var connection = new Connection(); connection.Raw(raw);
        var error = await Assert.ThrowsAnyAsync<IOException>(() => EngineHandshake.HelloAsync(new Factory(connection), limit: Limit));
        Assert.Contains("ipc", error.Message);
        if (raw == Rejection) Assert.Contains("synthetic command rejection", error.Message);
        Assert.True(connection.Disposed);
    }

    private sealed class Factory(Connection connection) : IEngineFactory
    {
        public int OpenCount;
        public IEngineConnection Open() { if (Interlocked.Increment(ref OpenCount) != 1) throw new IOException("unexpected retry"); return connection; }
    }
    private sealed class Connection : IEngineConnection
    {
        private readonly Channel<string> _events = Channel.CreateUnbounded<string>();
        public readonly ConcurrentQueue<string> Commands = new();
        public readonly TaskCompletionSource Started = Signal(), Starting = Signal(), Reading = Signal();
        public Func<CancellationToken, Task>? BeforeHelloReply { get; set; }
        public Func<int, CancellationToken, Task>? BeforeRead { get; set; }
        public bool Disposed;
        private string _session = "";
        private int _reads;
        public async Task SendAsync(string session, string command, object? parameters, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested(); Commands.Enqueue(command);
            if (command == "hello")
            {
                _session = session;
                Started.TrySetResult();
                if (BeforeHelloReply is { } beforeHelloReply) await beforeHelloReply(cancellation);
                cancellation.ThrowIfCancellationRequested();
                Current("hello", new { capabilities = new[] { "receiver_profiles", "software_stereo", "continuity", "buffered_audio", "standby_release", "member_volume", "receiver_password", "application_capture" } });
            }
            if (command is "start" or "pair") { _session = session; Starting.TrySetResult(); }
            if (command == "stop") _events.Writer.TryComplete();
        }
        public void Raw(string raw) => _events.Writer.TryWrite(raw);
        public void Current(string name, object? fields = null)
        {
            var data = fields is null ? new Dictionary<string, object?>() : JsonSerializer.Deserialize<Dictionary<string, object?>>(JsonSerializer.Serialize(fields))!;
            data["version"] = 1; data["session_id"] = _session; data["event"] = name; Raw(JsonSerializer.Serialize(data));
        }
        public async ValueTask<JsonElement?> ReadAsync(CancellationToken cancellation)
        {
            Reading.TrySetResult();
            var read = Interlocked.Increment(ref _reads);
            if (BeforeRead is { } beforeRead) await beforeRead(read, cancellation);
            try { using var json = JsonDocument.Parse(await _events.Reader.ReadAsync(cancellation)); return json.RootElement.Clone(); }
            catch (ChannelClosedException) { return null; }
        }
        public ValueTask DisposeAsync() { Disposed = true; _events.Writer.TryComplete(); return ValueTask.CompletedTask; }
    }
    private sealed class Audio(bool original) : IAudioService
    {
        private readonly bool _original = original;
        private bool _owned;
        private readonly object _sync = new();
        private readonly List<(int Count, TaskCompletionSource Signal)> _waiters = [];
        public bool Muted = original;
        public int Restores;
        public event Action? EndpointsChanged { add { } remove { } }
        public Task<IReadOnlyList<AudioEndpoint>> GetEndpointsAsync() => Task.FromResult<IReadOnlyList<AudioEndpoint>>([]);
        public Task<string?> GetDefaultEndpointIdAsync() => Task.FromResult<string?>("synthetic-endpoint");
        public Task MuteAsync(string? endpointId) { _owned = true; Muted = true; return Task.CompletedTask; }
        public Task RestoreAsync()
        {
            lock (_sync)
            {
                if (_owned) { Muted = _original; _owned = false; }
                ++Restores;
                foreach (var waiter in _waiters.Where(w => w.Count <= Restores).ToArray()) { waiter.Signal.TrySetResult(); _waiters.Remove(waiter); }
            }
            return Task.CompletedTask;
        }
        public Task WhenRestoredAtLeastAsync(int count)
        {
            lock (_sync) { if (Restores >= count) return Task.CompletedTask; var signal = Signal(); _waiters.Add((count, signal)); return signal.Task; }
        }
    }
}
