using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using AirFlash.Core;
using Xunit;

namespace AirFlash.Tests;

public sealed class PairingPasswordTests
{
    private static readonly TimeSpan Limit = TimeSpan.FromSeconds(5);
    private static readonly SessionTiming Timing = new(Limit, Limit, Limit, TimeSpan.Zero);
    private static TaskCompletionSource Signal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static Receiver Receiver(bool password) => new("synthetic-password", "Synthetic receiver", "192.0.2.81", 7001) { RequiresPassword = password };
    private static AppSettings Settings() => new() { MuteWhileStreaming = true, ForceReconnect = true, MaxReconnectAttempts = 1 };

    [Theory]
    [InlineData(true, "password_required", "  raw-pässword-12-34 \t", true)]
    [InlineData(false, "password_required", "raw-password-12-34", true)]
    [InlineData(false, "pin_required", "1234", false)]
    public async Task PairingPreservesPasswordFlagPromptKindAndExactRawPayload(bool required, string prompt, string value, bool passwordPrompt)
    {
        var connection = new Connection(prompt); var factory = new Factory(connection); var audio = new Audio();
        await using var controller = new SessionController(factory, audio, timing: Timing);
        Receiver? requested = null;
        await controller.PairAsync(Receiver(required), Settings(), (member, _) => { requested = member; return Task.FromResult<string?>(value); });
        await connection.PinSent.Task.WaitAsync(Limit);
        var pair = connection.Commands.Single(c => c.Name == "pair").Parameters;
        Assert.Equal(required, pair.GetProperty("password_required").GetBoolean());
        Assert.Equal(7001, pair.GetProperty("peer").GetProperty("port").GetInt32());
        Assert.Equal("192.0.2.81", pair.GetProperty("peer").Text("host"));
        Assert.NotNull(requested); Assert.Equal(passwordPrompt, requested.RequiresPassword);
        Assert.Equal(value, connection.Commands.Single(c => c.Name == "pair_pin").Parameters.Text("pin"));
        Assert.Equal(PlaybackState.Pairing, controller.Snapshot.State);
        await controller.StopAsync();
        Assert.Equal(1, factory.OpenCount); Assert.Equal(0, audio.MuteCalls);
        Assert.DoesNotContain(connection.Commands, c => c.Name == "start");
        Assert.True(connection.Disposed.Task.IsCompletedSuccessfully);
    }

    [Theory]
    [InlineData(null)] [InlineData("")] [InlineData(" \t ")]
    public async Task CancelledPasswordPromptDoesNotSubmitOrStartPlayback(string? value)
    {
        var connection = new Connection("password_required"); var factory = new Factory(connection); var audio = new Audio();
        await using var controller = new SessionController(factory, audio, timing: Timing);
        await controller.PairAsync(Receiver(true), Settings(), (_, _) => Task.FromResult(value));
        await connection.Disposed.Task.WaitAsync(Limit);
        Assert.Equal(PlaybackState.Idle, controller.Snapshot.State);
        Assert.Equal(1, factory.OpenCount); Assert.Equal(0, audio.MuteCalls);
        Assert.DoesNotContain(connection.Commands, c => c.Name is "pair_pin" or "start");
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task StopAndPromptTimeoutCancelThePasswordWaitWithoutPlayback(bool timeout)
    {
        var connection = new Connection("password_required"); var factory = new Factory(connection); var audio = new Audio();
        var entered = Signal(); var cancelled = Signal();
        var pending = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var fault = Signal();
        await using var controller = new SessionController(factory, audio, timing: timeout ? Timing with { Pin = TimeSpan.FromMilliseconds(100) } : Timing);
        controller.Changed += snapshot => { if (snapshot.State == PlaybackState.Error) fault.TrySetResult(); };
        try
        {
            await controller.PairAsync(Receiver(true), Settings(), async (_, token) =>
            {
                entered.TrySetResult();
                try { return await pending.Task.WaitAsync(token); }
                finally { if (token.IsCancellationRequested) cancelled.TrySetResult(); }
            });
            await entered.Task.WaitAsync(Limit);
            if (timeout) await fault.Task.WaitAsync(Limit);
            else await controller.StopAsync();
            await cancelled.Task.WaitAsync(Limit);
            await connection.Disposed.Task.WaitAsync(Limit);
            Assert.Equal(timeout ? PlaybackState.Error : PlaybackState.Idle, controller.Snapshot.State);
            Assert.Equal(1, factory.OpenCount); Assert.Equal(0, audio.MuteCalls);
            Assert.DoesNotContain(connection.Commands, c => c.Name is "pair_pin" or "start");
        }
        finally { pending.TrySetResult(null); await controller.StopAsync(); }
    }

    [Fact]
    public async Task MissingPasswordCapabilityRejectsBeforePromptPairOrPlayback()
    {
        var connection = new Connection("password_required", passwordCapability: false);
        var factory = new Factory(connection); var audio = new Audio(); var fault = Signal(); var prompts = 0;
        await using var controller = new SessionController(factory, audio, timing: Timing);
        controller.Changed += snapshot => { if (snapshot.State == PlaybackState.Error) fault.TrySetResult(); };
        await controller.PairAsync(Receiver(true), Settings(), (_, _) => { ++prompts; return Task.FromResult<string?>("never submitted"); });
        await fault.Task.WaitAsync(Limit); await connection.Disposed.Task.WaitAsync(Limit);
        Assert.Equal(0, prompts); Assert.Equal(1, factory.OpenCount); Assert.Equal(0, audio.MuteCalls);
        Assert.DoesNotContain(connection.Commands, c => c.Name is "pair" or "pair_pin" or "start");
        Assert.Equal(PlaybackState.Error, controller.Snapshot.State);
    }

    private sealed class Factory(Connection connection) : IEngineFactory
    {
        private int _opens;
        public int OpenCount => Volatile.Read(ref _opens);
        public IEngineConnection Open()
        {
            if (Interlocked.Increment(ref _opens) != 1) throw new InvalidOperationException("No playback/retry process is scripted.");
            return connection;
        }
    }
    private sealed class Connection(string prompt, bool passwordCapability = true) : IEngineConnection
    {
        private readonly Channel<JsonElement> _events = Channel.CreateUnbounded<JsonElement>();
        public ConcurrentQueue<(string Name, JsonElement Parameters)> Commands { get; } = new();
        public TaskCompletionSource PinSent { get; } = Signal();
        public TaskCompletionSource Disposed { get; } = Signal();
        public Task SendAsync(string session, string command, object? parameters, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            Commands.Enqueue((command, JsonSerializer.SerializeToElement(parameters)));
            if (command == "hello") Emit(session, new { @event = "hello", capabilities = passwordCapability ? new[] { "receiver_password" } : [] });
            else if (command == "pair") Emit(session, new { @event = prompt });
            else if (command == "pair_pin") PinSent.TrySetResult();
            return Task.CompletedTask;
        }
        private void Emit(string session, object payload)
        {
            var item = JsonSerializer.SerializeToNode(payload)!.AsObject();
            item["version"] = 1; item["session_id"] = session;
            _events.Writer.TryWrite(JsonSerializer.SerializeToElement(item));
        }
        public async ValueTask<JsonElement?> ReadAsync(CancellationToken cancellation)
        {
            try { return await _events.Reader.ReadAsync(cancellation); }
            catch (ChannelClosedException) { return null; }
        }
        public ValueTask DisposeAsync() { _events.Writer.TryComplete(); Disposed.TrySetResult(); return ValueTask.CompletedTask; }
    }
    private sealed class Audio : IAudioService
    {
        public int MuteCalls;
        public event Action? EndpointsChanged { add { } remove { } }
        public Task<IReadOnlyList<AudioEndpoint>> GetEndpointsAsync() => Task.FromResult<IReadOnlyList<AudioEndpoint>>([]);
        public Task<string?> GetDefaultEndpointIdAsync() => Task.FromResult<string?>("synthetic-endpoint");
        public Task MuteAsync(string? endpointId) { Interlocked.Increment(ref MuteCalls); return Task.CompletedTask; }
        public Task RestoreAsync() => Task.CompletedTask;
    }
}
