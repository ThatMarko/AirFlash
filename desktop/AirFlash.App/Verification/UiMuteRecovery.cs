using System.Collections.Concurrent;
using AirFlash.App.Services;
using AirFlash.Core;

namespace AirFlash.App.Verification;

// Exercise the real audio service queue with a fake endpoint and a temporary journal.
internal static class UiMuteRecovery
{
    public static async Task RunAsync(List<string> checks, string directory)
    {
        var root = Path.Combine(directory, "mute-recovery", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        Check(AudioService.RecoveryDirectory == Path.Combine(AppPaths.DataDirectory, "mute-recovery"),
            "diagnostic mute journal follows isolated startup data", checks);
        await ReadOnlyAsync(checks, Path.Combine(root, "read-only"));
        foreach (var previous in new[] { false, true }) await ExactBitAsync(checks, Path.Combine(root, "bit-" + previous), previous);
        foreach (var previous in new[] { false, true }) await RecordingAsync(checks, Path.Combine(root, "recording-" + previous), previous);
        await JournalFailuresAsync(checks, Path.Combine(root, "journal-failures"));
        await MutatingFailureAsync(checks, Path.Combine(root, "setter-failure"));
        await DisposeFailureAsync(checks, Path.Combine(root, "dispose-failure"));
    }

    private static async Task ReadOnlyAsync(List<string> checks, string directory)
    {
        var backend = new Backend(false); var journal = new Journal(directory);
        using (var interrupted = new MuteRecoveryCoordinator(backend, journal)) { interrupted.Initialize(); interrupted.Mute(null); }
        backend.Calls.Clear(); backend.Opens = 0; journal.Leases = 0; journal.Reads = 0; journal.Writes = 0;
        var audio = Audio(backend, journal);
        Check(audio.Recovery.Status == MuteRecoveryStatus.NotInitialized, "audio constructor leaves recovery uninitialized", checks);
        await audio.GetEndpointsAsync(); await audio.GetDefaultEndpointIdAsync(); await audio.RestoreAsync(); await audio.DisposeAsync();
        Check(journal.Leases == 0 && journal.Reads == 0 && journal.Writes == 0 && backend.Opens == 0 && backend.Bit,
            "read-only audio catalog and disposal never read stale journal or endpoint mute", checks);
        Check(backend.Calls.All(call => call.Thread != Environment.CurrentManagedThreadId && call.Apartment == ApartmentState.MTA),
            "injected audio catalog executes on the MTA worker", checks);
    }

    private static async Task ExactBitAsync(List<string> checks, string directory, bool previous)
    {
        var backend = new Backend(previous); var journal = new Journal(directory);
        await using (var audio = Audio(backend, journal))
        {
            await audio.InitializeRecoveryAsync(); await audio.MuteAsync(null); await audio.MuteAsync(null);
            var owned = audio.Recovery.Entries.Single();
            Check(owned.EndpointId == Backend.EndpointId && owned.PreviousMute == previous && owned.Live,
                $"MTA journal owns actual default id and original {previous} bit without recapture", checks);
            await audio.RestoreAsync();
            Check(backend.Bit == previous && audio.Recovery.CanMute && audio.Recovery.Entries.Count == 0,
                $"MTA cleanup restores exact {previous} bit and resolves journal", checks);
        }
        using var lease = new FileMuteRecoveryJournal(directory).AcquireLease();
        Check(backend.Calls.All(call => call.Apartment == ApartmentState.MTA), "endpoint access remains on MTA including cleanup", checks);
        checks.Add("audio disposal releases recovery lease after queued cleanup");
    }

    private static async Task RecordingAsync(List<string> checks, string directory, bool previous)
    {
        var backend = new Backend(previous) { Kind = AudioEndpointKind.Recording, RejectMuteAccess = true };
        var journal = new Journal(directory);
        // Deliberately disagree with the catalog: only the opened endpoint's
        // direction authorizes mute access, even with identical friendly names.
        await using (var audio = new AudioService(backend, journal,
            () => [new(Backend.EndpointId, "Synthetic shared name", AudioEndpointKind.Playback),
                new("synthetic-recording-secondary", "Synthetic shared name", AudioEndpointKind.Recording)],
            () => Backend.EndpointId))
        {
            var catalog = await audio.GetEndpointsAsync();
            Check(catalog.Count == 2 && catalog.Select(item => item.Kind).Distinct().Count() == 2
                && catalog.Select(item => item.Id).Distinct().Count() == 2,
                "audio catalog preserves playback and recording identities with shared friendly names", checks);
            await audio.MuteAsync(Backend.EndpointId);
            Check(journal.Leases == 0 && journal.Reads == 0 && journal.Writes == 0 && backend.Getters == 0 && backend.Setters == 0,
                $"opened recording input with prior {previous} bypasses mute access and journal acquisition before initialization", checks);
            await audio.InitializeRecoveryAsync();
            await audio.MuteAsync(null); await audio.MuteAsync(Backend.EndpointId); await audio.RestoreAsync();
            Check(audio.Recovery.CanMute && audio.Recovery.Entries.Count == 0 && journal.Writes == 0
                && backend.Getters == 0 && backend.Setters == 0 && backend.Bit == previous,
                $"recording input with prior {previous} never creates recovery ownership despite playback catalog label", checks);
            backend.Kind = AudioEndpointKind.Playback; backend.RejectMuteAccess = false;
            await audio.MuteAsync(null); await audio.RestoreAsync();
            Check(backend.Bit == previous && backend.Setters == 2 && audio.Recovery.CanMute,
                $"switching to the actual playback endpoint retains exact prior {previous} cleanup", checks);
            var reads = backend.Getters; var writes = journal.Writes;
            backend.Kind = AudioEndpointKind.Recording; backend.RejectMuteAccess = true;
            await audio.MuteAsync(null); await audio.RestoreAsync();
            Check(backend.Getters == reads && backend.Setters == 2 && journal.Writes == writes,
                "switching back to recording neither recaptures nor restores its mute bit", checks);
        }

        var staleBackend = new Backend(previous); var staleJournal = new Journal(Path.Combine(directory, "stale"));
        byte[] raw;
        using (var interrupted = new MuteRecoveryCoordinator(staleBackend, staleJournal))
        { interrupted.Initialize(); interrupted.Mute(null); raw = staleJournal.Read()!; }
        var staleWrites = staleJournal.Writes; var staleReads = staleBackend.Getters;
        staleBackend.Calls.Clear();
        staleBackend.Kind = AudioEndpointKind.Recording; staleBackend.RejectMuteAccess = true;
        await using var recovering = Audio(staleBackend, staleJournal);
        await recovering.InitializeRecoveryAsync();
        var entry = recovering.Recovery.Entries.Single();
        Check(recovering.Recovery.Status == MuteRecoveryStatus.Pending && entry.CurrentMute is null
            && staleBackend.Getters == staleReads && staleBackend.Setters == 1 && staleJournal.Writes == staleWrites
            && raw.SequenceEqual(staleJournal.Read()!),
            "saved recovery for a recording input remains pending with exact journal bytes and no mute access", checks);
        var rejected = false;
        try
        {
            await recovering.ResolveRecoveryAsync(recovering.Recovery.Revision, entry.Owner,
                entry.Generation, entry.EndpointId, MuteRecoveryChoice.RestoreSaved);
        }
        catch (InvalidOperationException) { rejected = true; }
        await recovering.RestoreAsync();
        Check(rejected && staleBackend.Getters == staleReads && staleBackend.Setters == 1 && raw.SequenceEqual(staleJournal.Read()!),
            "explicit recovery and ordinary cleanup cannot write an unavailable playback mute obligation to a recording input", checks);
        staleBackend.Kind = AudioEndpointKind.Playback; staleBackend.RejectMuteAccess = false;
        await recovering.RefreshRecoveryAsync();
        if (!recovering.Recovery.CanMute)
        {
            entry = recovering.Recovery.Entries.Single();
            await recovering.ResolveRecoveryAsync(recovering.Recovery.Revision, entry.Owner,
                entry.Generation, entry.EndpointId, MuteRecoveryChoice.RestoreSaved);
        }
        Check(recovering.Recovery.CanMute && staleBackend.Bit == previous
            && staleBackend.Calls.All(call => call.Apartment == ApartmentState.MTA),
            $"actual playback return permits controlled exact prior {previous} recovery on the MTA", checks);
    }

    private static async Task JournalFailuresAsync(List<string> checks, string directory)
    {
        var backend = new Backend(false); var journal = new Journal(directory);
        await using var audio = Audio(backend, journal);
        await audio.InitializeRecoveryAsync(); journal.FailWrites = true;
        await ExpectFailure(audio.MuteAsync(null));
        Check(backend.Setters == 0 && !backend.Bit, "failed durable acquisition prevents every mute setter", checks);
        journal.FailWrites = false; await audio.RestoreAsync(); await audio.MuteAsync(null); journal.FailWrites = true;
        await ExpectFailure(audio.RestoreAsync());
        Check(!backend.Bit && audio.Recovery.Entries.Any(entry => entry.KnownRestored),
            "journal failure still attempts exact live restore and retains known-restored status", checks);
        var setters = backend.Setters; backend.ChangeExternally(true);
        await ExpectFailure(audio.RestoreAsync());
        Check(backend.Bit && backend.Setters == setters, "known-restored retry never overwrites a later external mute", checks);
        journal.FailWrites = false; await audio.RestoreAsync();
        Check(audio.Recovery.CanMute && backend.Setters == setters && backend.Bit,
            "resolution persistence retry settles without replaying endpoint restore", checks);
    }

    private static async Task MutatingFailureAsync(List<string> checks, string directory)
    {
        var backend = new Backend(false); var journal = new Journal(directory);
        await using var audio = Audio(backend, journal); await audio.InitializeRecoveryAsync();
        backend.ThrowAfterNextSet = true; await ExpectFailure(audio.MuteAsync(null));
        Check(!backend.Bit && backend.Setters >= 2, "setter mutation followed by exception receives tracked exact-bit rollback", checks);
        await audio.RestoreAsync();
        Check(audio.Recovery.Entries.Count == 0, "failed mute rollback settles its journal ownership", checks);
    }

    private static async Task DisposeFailureAsync(List<string> checks, string directory)
    {
        var backend = new Backend(false); var journal = new Journal(directory); var audio = Audio(backend, journal);
        await audio.InitializeRecoveryAsync(); await audio.MuteAsync(null); journal.FailWrites = true;
        await ExpectFailure(audio.DisposeAsync().AsTask());
        Check(!backend.Bit && audio.Recovery.Entries.Any(entry => entry.KnownRestored),
            "audio disposal reports journal failure after attempting exact live restoration", checks);
        var verifier = new FileMuteRecoveryJournal(directory); using var lease = verifier.AcquireLease();
        Check(verifier.Read() is { Length: > 0 },
            "failed disposal releases lease while retaining durable recovery evidence", checks);
    }

    private static AudioService Audio(Backend backend, Journal journal) => new(backend, journal,
        () => { backend.Track(); return [new(Backend.EndpointId, "Synthetic speakers")]; },
        () => { backend.Track(); return Backend.EndpointId; });
    private static TaskCompletionSource Signal() => new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static async Task ExpectFailure(Task task)
    {
        try { await task; }
        catch (IOException) { return; }
        catch (AggregateException error) when (error.Flatten().InnerExceptions.All(inner => inner is IOException)) { return; }
        throw new InvalidOperationException("Expected an injected endpoint or journal failure.");
    }
    private static void Check(bool condition, string name, List<string> checks)
    { if (!condition) throw new InvalidOperationException("Mute recovery: " + name); checks.Add(name); }

    private sealed class Backend(bool initial) : IMuteEndpointBackend
    {
        public const string EndpointId = "synthetic-render-endpoint";
        private readonly object _gate = new(); private bool _bit = initial;
        public readonly ConcurrentQueue<(int Thread, ApartmentState Apartment)> Calls = new();
        public readonly TaskCompletionSource OpenEntered = Signal();
        public ManualResetEventSlim? OpenGate;
        public AudioEndpointKind Kind = AudioEndpointKind.Playback;
        public bool ThrowAfterNextSet, RejectMuteAccess; public int Opens, Getters, Setters;
        public bool Bit { get { lock (_gate) return _bit; } }
        public void ChangeExternally(bool value) { lock (_gate) _bit = value; }
        public void Track() => Calls.Enqueue((Environment.CurrentManagedThreadId, Thread.CurrentThread.GetApartmentState()));
        public IMuteEndpoint Open(string? id)
        {
            Track(); Interlocked.Increment(ref Opens);
            if (id is not null && id != EndpointId) throw new IOException("Synthetic endpoint is unavailable.");
            if (Interlocked.Exchange(ref OpenGate, null) is { } gate)
            { OpenEntered.TrySetResult(); if (!gate.Wait(TimeSpan.FromSeconds(10))) throw new IOException("Synthetic endpoint gate was not released."); }
            return new Endpoint(this);
        }
        private sealed class Endpoint(Backend backend) : IMuteEndpoint
        {
            public string Id => EndpointId;
            public AudioEndpointKind Kind { get { backend.Track(); return backend.Kind; } }
            public bool Muted
            {
                get
                {
                    backend.Track(); Interlocked.Increment(ref backend.Getters);
                    if (backend.RejectMuteAccess) throw new IOException("Recording mute getter must not be used.");
                    return backend.Bit;
                }
                set
                {
                    backend.Track(); Interlocked.Increment(ref backend.Setters);
                    if (backend.RejectMuteAccess) throw new IOException("Recording mute setter must not be used.");
                    backend.ChangeExternally(value);
                    if (backend.ThrowAfterNextSet) { backend.ThrowAfterNextSet = false; throw new IOException("Synthetic setter changed state before failure."); }
                }
            }
            public void Dispose() => backend.Track();
        }
    }
    private sealed class Journal(string directory) : IMuteRecoveryJournal
    {
        private readonly FileMuteRecoveryJournal _inner = new(directory);
        public bool FailWrites; public int Leases, Reads, Writes;
        public IDisposable AcquireLease() { Interlocked.Increment(ref Leases); return _inner.AcquireLease(); }
        public byte[]? Read() { Interlocked.Increment(ref Reads); return _inner.Read(); }
        public void Write(byte[] bytes) { Interlocked.Increment(ref Writes); if (FailWrites) throw new IOException("Synthetic journal write failure."); _inner.Write(bytes); }
        public void Archive(byte[] bytes) => _inner.Archive(bytes);
    }
}
