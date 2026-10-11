using AirFlash.Core;
using System.Text.Json;
using System.Threading.Channels;
using Xunit;
namespace AirFlash.Tests;

public sealed class MuteRecoveryAdmissionTests
{
    [Theory]
    [InlineData(MuteRecoveryStatus.NotInitialized)]
    [InlineData(MuteRecoveryStatus.Pending)]
    [InlineData(MuteRecoveryStatus.Busy)]
    [InlineData(MuteRecoveryStatus.Unreadable)]
    [InlineData(MuteRecoveryStatus.Unrecognized)]
    public async Task Unresolved_recovery_rejects_start_and_pair_before_opening_an_engine(MuteRecoveryStatus status)
    {
        var audio = new RecoveryAudio(status); var factory = new CountingFactory();
        await using var controller = new SessionController(factory, audio);
        var receiver = new Receiver("synthetic", "Synthetic", "192.0.2.1");
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.StartAsync(receiver, new()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.PairAsync(receiver, new(), (_, _) => throw new InvalidOperationException("PIN must not open")));
        Assert.Equal(0, factory.Count); Assert.Equal(0, audio.Mutes); Assert.Equal(PlaybackState.Idle, controller.Snapshot.State);
    }

    [Fact]
    public async Task Cleanup_can_expose_pending_recovery_and_admission_rechecks_before_open()
    {
        var audio = new RecoveryAudio(MuteRecoveryStatus.Ready)
        { OnRestore = (value, _) => value.Status = MuteRecoveryStatus.Pending };
        var factory = new CountingFactory();
        await using var controller = new SessionController(factory, audio);
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.StartAsync(new("synthetic", "Synthetic", "192.0.2.1"), new()));
        Assert.Equal(0, factory.Count); Assert.Equal(0, audio.Mutes);
    }

    [Fact]
    public async Task Rejected_replacement_publishes_idle_after_the_old_worker_has_stopped()
    {
        var audio = new RecoveryAudio(MuteRecoveryStatus.Ready); var factory = new StreamingFactory();
        await using var controller = new SessionController(factory, audio);
        var streaming = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        controller.Changed += value => { if (value.State == PlaybackState.Streaming) streaming.TrySetResult(); };
        await controller.StartAsync(new("first", "First", "192.0.2.1"), new());
        await streaming.Task.WaitAsync(TimeSpan.FromSeconds(3));
        audio.OnRestore = (value, _) => value.Status = MuteRecoveryStatus.Pending;
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.StartAsync(new("second", "Second", "192.0.2.2"), new()));
        Assert.Equal(1, factory.Count); Assert.Equal(1, factory.Disposed);
        Assert.Equal(PlaybackState.Idle, controller.Snapshot.State); Assert.Null(controller.Snapshot.Receiver);
    }

    [Fact]
    public async Task Playback_without_local_muting_does_not_require_a_recovery_decision()
    {
        var audio = new RecoveryAudio(MuteRecoveryStatus.Pending); var factory = new CountingFactory();
        await using var controller = new SessionController(factory, audio);
        await controller.StartAsync(new("synthetic", "Synthetic", "192.0.2.1"), new() { MuteWhileStreaming = false });
        await factory.Opened.Task.WaitAsync(TimeSpan.FromSeconds(3));
        await controller.StopAsync();
        Assert.Equal(1, factory.Count); Assert.Equal(0, audio.Mutes); Assert.Equal(PlaybackState.Idle, controller.Snapshot.State);
    }

    [Fact]
    public async Task Failed_cleanup_blocks_a_new_retry_before_another_process()
    {
        var audio = new RecoveryAudio(MuteRecoveryStatus.Ready)
        { OnRestore = (value, count) => { if (count >= 2) value.Status = MuteRecoveryStatus.Pending; } };
        var factory = new CountingFactory();
        await using var controller = new SessionController(factory, audio);
        var rejectedRetry = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        controller.Changed += value => { if (value.State == PlaybackState.Error && value.Message.Contains("recovery", StringComparison.OrdinalIgnoreCase)) rejectedRetry.TrySetResult(); };
        await controller.StartAsync(new("synthetic", "Synthetic", "192.0.2.1"), new() { ForceReconnect = true });
        await rejectedRetry.Task.WaitAsync(TimeSpan.FromSeconds(4));
        Assert.Equal(1, factory.Count); Assert.Equal(0, audio.Mutes);
    }

    private sealed class CountingFactory : IEngineFactory
    {
        public int Count;
        public TaskCompletionSource Opened { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public IEngineConnection Open()
        {
            Interlocked.Increment(ref Count); Opened.TrySetResult();
            throw new IOException("Synthetic engine open failure");
        }
    }
    private sealed class StreamingFactory : IEngineFactory
    {
        public int Count, Disposed;
        public IEngineConnection Open() { ++Count; return new Connection(this); }
        private sealed class Connection(StreamingFactory factory) : IEngineConnection
        {
            private readonly Channel<JsonElement> _events = Channel.CreateUnbounded<JsonElement>();
            public Task SendAsync(string session, string command, object? parameters, CancellationToken cancellation)
            {
                if (command == "start") _events.Writer.TryWrite(JsonSerializer.SerializeToElement(new { version = 1, id = "synthetic", session_id = session, @event = "streaming" }));
                return Task.CompletedTask;
            }
            public async ValueTask<JsonElement?> ReadAsync(CancellationToken cancellation) => await _events.Reader.ReadAsync(cancellation);
            public ValueTask DisposeAsync() { ++factory.Disposed; _events.Writer.TryComplete(); return ValueTask.CompletedTask; }
        }
    }
    private sealed class RecoveryAudio(MuteRecoveryStatus status) : IAudioService, IMuteRecoveryService
    {
        public MuteRecoveryStatus Status { get; set; } = status;
        public MuteRecoverySnapshot Recovery => new(1, Status, []);
        public int Mutes, Restores;
        public Action<RecoveryAudio, int>? OnRestore;
        public event Action? EndpointsChanged { add { } remove { } }
        public event Action? RecoveryChanged { add { } remove { } }
        public Task<IReadOnlyList<AudioEndpoint>> GetEndpointsAsync() => Task.FromResult<IReadOnlyList<AudioEndpoint>>([]);
        public Task<string?> GetDefaultEndpointIdAsync() => Task.FromResult<string?>("synthetic-endpoint");
        public Task MuteAsync(string? endpointId) { ++Mutes; return Task.CompletedTask; }
        public Task RestoreAsync() { OnRestore?.Invoke(this, ++Restores); return Task.CompletedTask; }
        public Task InitializeRecoveryAsync() => Task.CompletedTask;
        public Task RefreshRecoveryAsync() => Task.CompletedTask;
        public Task ResolveRecoveryAsync(long revision, Guid owner, long generation, string endpointId, MuteRecoveryChoice choice) => Task.CompletedTask;
        public Task ArchiveUnrecognizedRecoveryAsync(long revision) => Task.CompletedTask;
    }
}
