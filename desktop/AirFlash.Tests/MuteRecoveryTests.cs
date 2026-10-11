using System.Text;
using System.Text.Json;
using AirFlash.Core;
using Xunit;
namespace AirFlash.Tests;

public sealed class MuteRecoveryTests
{
    private static readonly Guid OldOwner = Guid.Parse("3f23b051-0729-4890-bdaf-e19be3422945");
    private const string EndpointId = "synthetic-render-endpoint";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IntentPrecedesMutationRepeatedMuteDoesNotRecaptureAndRestoreIsExact(bool previous)
    {
        var backend = new Backend(previous);
        var journal = new Journal();
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        Assert.Equal(MuteRecoveryStatus.NotInitialized, recovery.Snapshot.Status);
        Assert.Equal(0, journal.Reads);
        recovery.Initialize();
        journal.BeforeWrite = bytes =>
        {
            if (State(bytes) == "Intent")
            {
                Assert.Equal(previous, backend.Bit);
                Assert.Empty(backend.Writes);
            }
        };
        recovery.Mute(null);
        var owned = Assert.Single(recovery.Snapshot.Entries);
        Assert.Equal(EndpointId, owned.EndpointId);
        Assert.Equal(previous, owned.PreviousMute);
        Assert.True(owned.Live);
        Assert.Equal(MuteJournalState.Owned, owned.State);
        Assert.Equal(MuteRecoveryStatus.Ready, recovery.Snapshot.Status);
        recovery.Mute(null);
        Assert.Equal([true], backend.Writes);
        recovery.Restore();
        Assert.Equal(previous, backend.Bit);
        Assert.Equal(new[] { true, previous }, backend.Writes);
        Assert.Empty(recovery.Snapshot.Entries);
        Assert.Equal("Resolved", State(journal.Bytes!));
        recovery.Restore();
        Assert.Equal(2, backend.Writes.Count);
        Assert.All(backend.Handles, handle => Assert.True(handle.Disposed));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedIntentNeverMutatesAndBlocksAcquisitionUntilPersistenceRecovers(bool previous)
    {
        var backend = new Backend(previous);
        var journal = new Journal { FailWrites = true };
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize();
        Assert.ThrowsAny<Exception>(() => recovery.Mute(null));
        Assert.Empty(backend.Writes);
        Assert.Equal(previous, backend.Bit);
        Assert.False(recovery.Snapshot.CanMute);
        Assert.ThrowsAny<Exception>(() => recovery.Mute(null));
        journal.FailWrites = false;
        recovery.Restore();
        Assert.Empty(backend.Writes);
        Assert.True(recovery.Snapshot.CanMute);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SetterThatMutatesThenThrowsStillHasOwnedMemoryAndRollsBackExactBit(bool previous)
    {
        var backend = new Backend(previous) { ThrowAfterSet = true };
        var journal = new Journal();
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize();
        Assert.ThrowsAny<Exception>(() => recovery.Mute(null));
        Assert.Equal(new[] { true, previous }, backend.Writes);
        Assert.Equal(previous, backend.Bit);
        Assert.True(recovery.Snapshot.CanMute);
        Assert.Equal("Resolved", State(journal.Bytes!));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedOwnedCommitRollsBackAndPreservesPriorBit(bool previous)
    {
        var backend = new Backend(previous);
        var journal = new Journal { FailState = "Owned" };
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize();
        Assert.ThrowsAny<Exception>(() => recovery.Mute(null));
        Assert.Equal(new[] { true, previous }, backend.Writes);
        Assert.Equal(previous, backend.Bit);
        Assert.True(recovery.Snapshot.CanMute);
    }

    [Fact]
    public void SuccessfulReplacementFollowedByFlushFailureCanSettleWithoutOverwritingLaterChange()
    {
        var backend = new Backend(false);
        var journal = new Journal();
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize(); recovery.Mute(null);
        journal.FailAfterState = "Resolved";
        Assert.ThrowsAny<Exception>(recovery.Restore);
        Assert.False(backend.Bit);
        Assert.True(Assert.Single(recovery.Snapshot.Entries).KnownRestored);
        backend.Bit = true;
        journal.FailAfterState = null;
        recovery.Restore();
        Assert.True(backend.Bit);
        Assert.Equal(new[] { true, false }, backend.Writes);
        Assert.True(recovery.Snapshot.CanMute);
    }

    [Fact]
    public void MultipleEndpointsPreserveIndependentBitsAndDefaultChangeDoesNotRecaptureOwnedMute()
    {
        var backend = new MultipleEndpoints();
        var journal = new Journal();
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize(); recovery.Mute(null);
        backend.Default = "synthetic-second";
        recovery.Mute(null);
        Assert.Equal(2, recovery.Snapshot.Entries.Count);
        Assert.False(recovery.Snapshot.Entries[0].PreviousMute);
        Assert.True(recovery.Snapshot.Entries[1].PreviousMute);
        Assert.NotEqual(recovery.Snapshot.Entries[0].Generation, recovery.Snapshot.Entries[1].Generation);
        backend.Unavailable.Add("synthetic-second");
        Assert.ThrowsAny<Exception>(recovery.Restore);
        Assert.False(backend.Bits[EndpointId]);
        Assert.Equal(MuteRecoveryStatus.Pending, recovery.Snapshot.Status);
        backend.Unavailable.Clear();
        recovery.Restore();
        Assert.True(backend.Bits["synthetic-second"]);
        Assert.True(recovery.Snapshot.CanMute);
        Assert.All(backend.Handles, handle => Assert.True(handle.Disposed));
    }

    [Fact]
    public void RepeatedLiveOwnedMuteRemainsHarmlessWhenRefreshCannotReadEndpoint()
    {
        var backend = new Backend(false);
        var journal = new Journal();
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize(); recovery.Mute(null);
        backend.FailGet = true;
        recovery.Refresh();
        Assert.Equal(MuteRecoveryStatus.Pending, recovery.Snapshot.Status);
        recovery.Mute(null);
        Assert.Equal([true], backend.Writes);
        Assert.ThrowsAny<Exception>(() => recovery.Mute("synthetic-other"));
        backend.FailGet = false;
        recovery.Restore();
        Assert.False(backend.Bit);
    }

    [Fact]
    public void PreInitializeFailureDoesNotEnableMuteOrTouchEndpoint()
    {
        var backend = new Backend(false);
        var journal = new Journal();
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        Assert.ThrowsAny<Exception>(() => recovery.Mute(null));
        Assert.Equal(MuteRecoveryStatus.NotInitialized, recovery.Snapshot.Status);
        Assert.Empty(backend.Handles);
        Assert.Equal(0, journal.Reads);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NormalRestoreContinuesAfterJournalFailureAndKnownRestoreNeverRewritesExternalChange(bool previous)
    {
        var backend = new Backend(previous);
        var journal = new Journal();
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize(); recovery.Mute(null);
        journal.FailWrites = true;
        Assert.ThrowsAny<Exception>(recovery.Restore);
        Assert.Equal(previous, backend.Bit);
        Assert.True(Assert.Single(recovery.Snapshot.Entries).KnownRestored);
        Assert.Equal(MuteRecoveryStatus.Pending, recovery.Snapshot.Status);
        backend.Bit = !previous;
        journal.FailWrites = false;
        recovery.Refresh(); recovery.Restore();
        Assert.Equal(!previous, backend.Bit);
        Assert.Equal(new[] { true, previous }, backend.Writes);
        Assert.True(recovery.Snapshot.CanMute);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FailedRestoreKeepsOwnershipAndReturningEndpointCanBeRetried(bool previous)
    {
        var backend = new Backend(previous);
        var journal = new Journal();
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize(); recovery.Mute(null);
        backend.Unavailable = true;
        Assert.ThrowsAny<Exception>(recovery.Restore);
        Assert.Equal(MuteRecoveryStatus.Pending, recovery.Snapshot.Status);
        Assert.False(Assert.Single(recovery.Snapshot.Entries).KnownRestored);
        Assert.ThrowsAny<Exception>(() => recovery.Mute("another-synthetic-endpoint"));
        backend.Unavailable = false;
        recovery.Restore();
        Assert.Equal(previous, backend.Bit);
        Assert.True(recovery.Snapshot.CanMute);
    }

    [Theory]
    [InlineData(false, "Intent")]
    [InlineData(false, "Owned")]
    [InlineData(false, "Restoring")]
    [InlineData(true, "Intent")]
    [InlineData(true, "Owned")]
    [InlineData(true, "Restoring")]
    public void RestartedInterruptionIsConservativeAndMatchingPriorBitNeedsNoSetter(bool previous, string state)
    {
        var backend = new Backend(true);
        var journal = new Journal { Bytes = Saved(previous, state) };
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize();
        Assert.Empty(backend.Writes);
        if (previous)
        {
            Assert.True(recovery.Snapshot.CanMute);
            Assert.Empty(recovery.Snapshot.Entries);
        }
        else
        {
            Assert.Equal(MuteRecoveryStatus.Pending, recovery.Snapshot.Status);
            var entry = Assert.Single(recovery.Snapshot.Entries);
            Assert.False(entry.Live);
            Assert.False(entry.PreviousMute);
            Assert.True(entry.CurrentMute);
            Assert.ThrowsAny<Exception>(() => recovery.Mute(null));
            recovery.Resolve(recovery.Snapshot.Revision, entry.Owner, entry.Generation, entry.EndpointId, MuteRecoveryChoice.RestoreSaved);
            Assert.Equal([false], backend.Writes);
            Assert.True(recovery.Snapshot.CanMute);
        }
    }

    [Fact]
    public void DisposeModelsProcessLossByReleasingOnlyLeaseAndRestartDoesNotRecapture()
    {
        var backend = new Backend(false);
        var journal = new Journal();
        var first = new MuteRecoveryCoordinator(backend, journal);
        first.Initialize(); first.Mute(null); first.Dispose();
        Assert.True(backend.Bit);
        Assert.Equal([true], backend.Writes);
        using var restarted = new MuteRecoveryCoordinator(backend, journal);
        restarted.Initialize();
        var entry = Assert.Single(restarted.Snapshot.Entries);
        Assert.False(entry.PreviousMute);
        Assert.False(entry.Live);
        Assert.False(restarted.Snapshot.CanMute);
    }

    [Fact]
    public void ChangedBitAndStaleTicketRejectExplicitRecoveryWithoutMutation()
    {
        var backend = new Backend(true);
        var journal = new Journal { Bytes = Saved(false, "Owned") };
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize();
        var ticket = recovery.Snapshot;
        var entry = Assert.Single(ticket.Entries);
        backend.Bit = false;
        Assert.ThrowsAny<Exception>(() => recovery.Resolve(ticket.Revision, entry.Owner, entry.Generation, entry.EndpointId, MuteRecoveryChoice.RestoreSaved));
        Assert.Empty(backend.Writes);
        Assert.ThrowsAny<Exception>(() => recovery.Resolve(ticket.Revision, entry.Owner, entry.Generation, entry.EndpointId, MuteRecoveryChoice.KeepCurrent));
        recovery.Refresh();
        Assert.True(recovery.Snapshot.CanMute);
    }

    [Fact]
    public void ExplicitRecoveryFailsBeforeMutationOnWriteErrorAndKnownSuccessfulChoiceOnlyRetriesPersistence()
    {
        var backend = new Backend(true);
        var journal = new Journal { Bytes = Saved(false, "Owned") };
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize();
        var entry = Assert.Single(recovery.Snapshot.Entries);
        journal.FailWrites = true;
        Assert.ThrowsAny<Exception>(() => recovery.Resolve(recovery.Snapshot.Revision, entry.Owner, entry.Generation, entry.EndpointId, MuteRecoveryChoice.RestoreSaved));
        Assert.Empty(backend.Writes);
        journal.FailWrites = false; journal.FailState = "Resolved";
        Assert.ThrowsAny<Exception>(() => recovery.Resolve(recovery.Snapshot.Revision, entry.Owner, entry.Generation, entry.EndpointId, MuteRecoveryChoice.RestoreSaved));
        Assert.Equal([false], backend.Writes);
        Assert.True(Assert.Single(recovery.Snapshot.Entries).KnownRestored);
        backend.Bit = true; journal.FailState = null;
        recovery.Resolve(recovery.Snapshot.Revision, entry.Owner, entry.Generation, entry.EndpointId, MuteRecoveryChoice.RestoreSaved);
        Assert.Equal([false], backend.Writes);
        Assert.True(backend.Bit);
        Assert.True(recovery.Snapshot.CanMute);
    }

    [Fact]
    public void FailedNormalSetterRetainsAmbiguousOwnershipUntilExactRetry()
    {
        var backend = new Backend(false);
        var journal = new Journal();
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize(); recovery.Mute(null);
        backend.ThrowAfterSet = true;
        Assert.ThrowsAny<Exception>(recovery.Restore);
        var pending = Assert.Single(recovery.Snapshot.Entries);
        Assert.False(pending.KnownRestored);
        Assert.Null(pending.CurrentMute);
        backend.Bit = true;
        recovery.Restore();
        Assert.False(backend.Bit);
        Assert.Equal(new[] { true, false, false }, backend.Writes);
        Assert.True(recovery.Snapshot.CanMute);
    }

    [Fact]
    public void EscapedIdentitiesCannotExceedBoundByTakingUnresolvableOwnership()
    {
        var backend = new Backend(false);
        var journal = new Journal();
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize();
        var admitted = 0;
        for (var index = 0; index < 32; index++)
        {
            var id = index.ToString() + new string('界', 1020);
            try { recovery.Mute(id); admitted++; }
            catch (InvalidDataException) { break; }
        }
        Assert.InRange(admitted, 1, 31);
        Assert.Equal(admitted, backend.Writes.Count);
        Assert.Equal(admitted, recovery.Snapshot.Entries.Count);
        Assert.True(journal.Bytes!.Length <= 65_536);
        recovery.Restore();
        Assert.True(recovery.Snapshot.CanMute);
    }

    [Fact]
    public void WrongIdentityOrGenerationCannotResolveAndKeepCurrentIsExplicitAbandonment()
    {
        var backend = new Backend(true);
        var journal = new Journal { Bytes = Saved(false, "Owned") };
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize();
        var entry = Assert.Single(recovery.Snapshot.Entries);
        Assert.ThrowsAny<Exception>(() => recovery.Resolve(recovery.Snapshot.Revision, Guid.NewGuid(), entry.Generation, entry.EndpointId, MuteRecoveryChoice.RestoreSaved));
        Assert.ThrowsAny<Exception>(() => recovery.Resolve(recovery.Snapshot.Revision, entry.Owner, entry.Generation + 1, entry.EndpointId, MuteRecoveryChoice.RestoreSaved));
        Assert.ThrowsAny<Exception>(() => recovery.Resolve(recovery.Snapshot.Revision, entry.Owner, entry.Generation, "other-synthetic-id", MuteRecoveryChoice.RestoreSaved));
        recovery.Resolve(recovery.Snapshot.Revision, entry.Owner, entry.Generation, entry.EndpointId, MuteRecoveryChoice.KeepCurrent);
        Assert.Empty(backend.Writes);
        Assert.True(backend.Bit);
        Assert.Equal("Abandoned", State(journal.Bytes!));
        Assert.True(recovery.Snapshot.CanMute);
    }

    [Fact]
    public void ExternalJournalReplacementAndUnavailableRecoveryNeverOverwriteOrSetEndpoint()
    {
        var backend = new Backend(true) { Unavailable = true };
        var journal = new Journal { Bytes = Saved(false, "Owned") };
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize();
        var entry = Assert.Single(recovery.Snapshot.Entries);
        Assert.Null(entry.CurrentMute);
        Assert.ThrowsAny<Exception>(() => recovery.Resolve(recovery.Snapshot.Revision, entry.Owner, entry.Generation, entry.EndpointId, MuteRecoveryChoice.RestoreSaved));
        backend.Unavailable = false; recovery.Refresh();
        var ticket = recovery.Snapshot;
        var unknown = Encoding.UTF8.GetBytes("future external bytes");
        journal.Bytes = unknown;
        Assert.ThrowsAny<Exception>(() => recovery.Resolve(ticket.Revision, entry.Owner, entry.Generation, entry.EndpointId, MuteRecoveryChoice.RestoreSaved));
        Assert.Equal(unknown, journal.Bytes);
        Assert.Empty(backend.Writes);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("{\"version\":2,\"entries\":[]}")]
    [InlineData("{\"version\":1,\"version\":1,\"entries\":[]}")]
    [InlineData("{\"version\":1,\"entries\":[null]}")]
    [InlineData("{\"version\":1,\"entries\":null}")]
    [InlineData("{\"version\":\"1\",\"entries\":[]}")]
    [InlineData("{\"version\":1.5,\"entries\":[]}")]
    [InlineData("not-json")]
    public void UnrecognizedBytesArePreservedAndBlockNewMute(string json)
    {
        var bytes = Encoding.UTF8.GetBytes(json);
        var backend = new Backend(false);
        var journal = new Journal { Bytes = bytes };
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize(); recovery.Refresh();
        Assert.Equal(MuteRecoveryStatus.Unrecognized, recovery.Snapshot.Status);
        Assert.ThrowsAny<Exception>(() => recovery.Mute(null));
        Assert.Equal(bytes, journal.Bytes);
        Assert.Empty(backend.Writes);
        Assert.Empty(journal.Written);
    }

    [Fact]
    public void DuplicateAndBoundedRecordsAreRejectedWithoutPartialRecovery()
    {
        var one = JsonDocument.Parse(Saved(false, "Owned")).RootElement.GetProperty("entries")[0].GetRawText();
        foreach (var json in new[]
        {
            "{\"version\":1,\"entries\":[" + one + "," + one + "]}",
            "{\"version\":1,\"entries\":[" + string.Join(',', Enumerable.Repeat(one, 33)) + "]}",
            Encoding.UTF8.GetString(Saved(false, "Owned", new string('x', 1025))),
            new string(' ', 65_537)
        })
        {
            var backend = new Backend(false);
            var journal = new Journal { Bytes = Encoding.UTF8.GetBytes(json) };
            using var recovery = new MuteRecoveryCoordinator(backend, journal);
            recovery.Initialize();
            Assert.Equal(MuteRecoveryStatus.Unrecognized, recovery.Snapshot.Status);
            Assert.Empty(backend.Writes);
            Assert.Empty(journal.Written);
        }
    }

    [Fact]
    public void BusyAndUncertainReadCannotBecomeAbsenceAndRefreshCanAcquireReleasedLease()
    {
        var backend = new Backend(false);
        var journal = new Journal();
        using var owner = journal.AcquireLease();
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize();
        Assert.Equal(MuteRecoveryStatus.Busy, recovery.Snapshot.Status);
        Assert.Equal(0, journal.Reads);
        Assert.ThrowsAny<Exception>(() => recovery.Mute(null));
        owner.Dispose();
        journal.FailReads = true;
        recovery.Refresh();
        Assert.Equal(MuteRecoveryStatus.Unreadable, recovery.Snapshot.Status);
        Assert.ThrowsAny<Exception>(() => recovery.Mute(null));
        journal.FailReads = false;
        recovery.Refresh();
        Assert.True(recovery.Snapshot.CanMute);
        Assert.Empty(backend.Writes);
    }

    [Theory]
    [InlineData("backup")]
    [InlineData("delete")]
    [InlineData("after-delete")]
    [InlineData("empty-write")]
    public void ArchiveFailuresPreserveBytesOrBackupAndRemainBlockedAcrossRefresh(string failure)
    {
        var bytes = Encoding.UTF8.GetBytes("unrecognized synthetic journal");
        var backend = new Backend(false);
        var journal = new Journal { Bytes = bytes, ArchiveFailure = failure };
        if (failure == "empty-write") journal.FailWrites = true;
        using var recovery = new MuteRecoveryCoordinator(backend, journal);
        recovery.Initialize();
        Assert.ThrowsAny<Exception>(() => recovery.ArchiveUnrecognized(recovery.Snapshot.Revision));
        recovery.Refresh();
        Assert.False(recovery.Snapshot.CanMute);
        Assert.Empty(backend.Writes);
        Assert.True(journal.Bytes?.SequenceEqual(bytes) == true || journal.Archived.Any(copy => copy.SequenceEqual(bytes)));
        Assert.ThrowsAny<Exception>(() => recovery.Mute(null));
        if (failure == "empty-write")
        {
            journal.FailWrites = false;
            recovery.ArchiveUnrecognized(recovery.Snapshot.Revision);
            Assert.True(recovery.Snapshot.CanMute);
        }
    }

    [Fact]
    public void ArchiveRequiresCurrentRevisionAndExactBytesBeforeEnablingNewAcquisition()
    {
        var original = Encoding.UTF8.GetBytes("unknown synthetic bytes");
        var journal = new Journal { Bytes = original };
        using var recovery = new MuteRecoveryCoordinator(new Backend(false), journal);
        recovery.Initialize();
        var revision = recovery.Snapshot.Revision;
        recovery.Refresh();
        Assert.ThrowsAny<Exception>(() => recovery.ArchiveUnrecognized(revision));
        recovery.ArchiveUnrecognized(recovery.Snapshot.Revision);
        Assert.Equal(original, Assert.Single(journal.Archived));
        Assert.True(recovery.Snapshot.CanMute);
    }

    [Fact]
    public void FileLeaseIsExclusiveConstantAndReleasedWithoutReadingOrDeletingJournal()
    {
        using var root = new TemporaryRoot();
        var first = new FileMuteRecoveryJournal(root.Path);
        var second = new FileMuteRecoveryJournal(root.Path);
        var lease = first.AcquireLease();
        try
        {
            Assert.Throws<MuteRecoveryBusyException>(() => second.AcquireLease());
            Assert.Null(first.Read());
            first.Write(Saved(false, "Owned"));
            first.Write(Saved(true, "Intent"));
            Assert.Equal(Saved(true, "Intent"), first.Read());
            Assert.True(File.Exists(System.IO.Path.Combine(root.Path, "mute-recovery.lease")));
        }
        finally { lease.Dispose(); }
        using var reopened = second.AcquireLease();
        Assert.Equal(Saved(true, "Intent"), second.Read());
        Assert.DoesNotContain(Directory.EnumerateFiles(root.Path), file => file.EndsWith(".tmp", StringComparison.Ordinal));
    }

    [Fact]
    public void FileArchivePreservesExactRawBytesAndRejectsChangedSource()
    {
        using var root = new TemporaryRoot();
        var journal = new FileMuteRecoveryJournal(root.Path);
        using var lease = journal.AcquireLease();
        var bytes = new byte[] { 0xef, 0xbb, 0xbf, 0xff, 0, 27 };
        journal.Write(bytes);
        Assert.ThrowsAny<Exception>(() => journal.Archive([1, 2, 3]));
        Assert.Equal(bytes, journal.Read());
        journal.Archive(bytes);
        Assert.Null(journal.Read());
        var backup = Assert.Single(Directory.EnumerateFiles(root.Path), file => file.Contains(".archive.", StringComparison.Ordinal));
        Assert.Equal(bytes, File.ReadAllBytes(backup));
    }

    [Fact]
    public void FileReadAndWriteStayBoundedAndDirectoryIsNotAbsence()
    {
        using var root = new TemporaryRoot();
        var journal = new FileMuteRecoveryJournal(root.Path);
        using var lease = journal.AcquireLease();
        Assert.ThrowsAny<Exception>(() => journal.Write(new byte[65_537]));
        var path = System.IO.Path.Combine(root.Path, "mute-recovery.json");
        File.WriteAllBytes(path, new byte[65_537]);
        Assert.ThrowsAny<Exception>(() => journal.Read());
        File.Delete(path); Directory.CreateDirectory(path);
        Assert.ThrowsAny<Exception>(() => journal.Read());
    }

    [Fact]
    public void WindowsLockedReplacementAndArchiveDeletionRetainOriginalAndDurableBackup()
    {
        // Windows sharing flags define this failure boundary; the application
        // and its hosted validation use Windows. Other OS unlink rules differ.
        if (!OperatingSystem.IsWindows()) return;
        using var root = new TemporaryRoot();
        var journal = new FileMuteRecoveryJournal(root.Path);
        using var lease = journal.AcquireLease();
        var bytes = Encoding.UTF8.GetBytes("synthetic unrecognized original");
        journal.Write(bytes);
        using (var locked = new FileStream(System.IO.Path.Combine(root.Path, "mute-recovery.json"),
            FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            Assert.ThrowsAny<Exception>(() => journal.Write([1, 2, 3]));
            Assert.Equal(bytes, journal.Read());
            Assert.ThrowsAny<Exception>(() => journal.Archive(bytes));
            Assert.Equal(bytes, journal.Read());
            var backup = Assert.Single(Directory.EnumerateFiles(root.Path), file => file.Contains(".archive.", StringComparison.Ordinal));
            Assert.Equal(bytes, File.ReadAllBytes(backup));
        }
        journal.Write([1, 2, 3]);
        Assert.Equal(new byte[] { 1, 2, 3 }, journal.Read());
        Assert.DoesNotContain(Directory.EnumerateFiles(root.Path), file => file.EndsWith(".tmp", StringComparison.Ordinal));
    }

    private static byte[] Saved(bool previous, string state, string endpointId = EndpointId)
        => JsonSerializer.SerializeToUtf8Bytes(new { version = 1, entries = new[] { new { owner = OldOwner, generation = 7, endpoint_id = endpointId, previous_mute = previous, state } } });
    private static string State(byte[] bytes)
    {
        using var json = JsonDocument.Parse(bytes);
        return json.RootElement.GetProperty("entries")[0].GetProperty("state").GetString()!;
    }

    private sealed class Backend(bool previous) : IMuteEndpointBackend
    {
        public bool Bit = previous, ThrowAfterSet, Unavailable, FailGet;
        public readonly List<bool> Writes = [];
        public readonly List<Endpoint> Handles = [];
        public IMuteEndpoint Open(string? endpointId)
        {
            if (Unavailable) throw new IOException("Synthetic endpoint unavailable");
            var handle = new Endpoint(this, endpointId ?? EndpointId);
            Handles.Add(handle); return handle;
        }
        public sealed class Endpoint(Backend backend, string id) : IMuteEndpoint
        {
            public string Id => id;
            public bool Disposed;
            public bool Muted
            {
                get => backend.FailGet ? throw new IOException("Synthetic endpoint read failure") : backend.Bit;
                set
                {
                    backend.Bit = value; backend.Writes.Add(value);
                    if (backend.ThrowAfterSet) { backend.ThrowAfterSet = false; throw new IOException("Synthetic setter mutated before failure"); }
                }
            }
            public void Dispose() => Disposed = true;
        }
    }

    private sealed class MultipleEndpoints : IMuteEndpointBackend
    {
        public string Default = EndpointId;
        public readonly Dictionary<string, bool> Bits = new(StringComparer.Ordinal) { [EndpointId] = false, ["synthetic-second"] = true };
        public readonly HashSet<string> Unavailable = new(StringComparer.Ordinal);
        public readonly List<Endpoint> Handles = [];
        public IMuteEndpoint Open(string? endpointId)
        {
            var id = endpointId ?? Default;
            if (Unavailable.Contains(id)) throw new IOException("Synthetic endpoint is unavailable");
            var handle = new Endpoint(this, id); Handles.Add(handle); return handle;
        }
        public sealed class Endpoint(MultipleEndpoints backend, string id) : IMuteEndpoint
        {
            public string Id => id;
            public bool Disposed;
            public bool Muted { get => backend.Bits[id]; set => backend.Bits[id] = value; }
            public void Dispose() => Disposed = true;
        }
    }

    private sealed class Journal : IMuteRecoveryJournal
    {
        public byte[]? Bytes;
        public bool FailWrites, FailReads;
        public string? FailState, FailAfterState, ArchiveFailure;
        public Action<byte[]>? BeforeWrite;
        public int Reads;
        public readonly List<byte[]> Written = [], Archived = [];
        private bool _leased;
        public IDisposable AcquireLease()
        {
            if (_leased) throw new MuteRecoveryBusyException("Synthetic lease is already owned");
            _leased = true; return new Lease(() => _leased = false);
        }
        public byte[]? Read()
        {
            Reads++;
            if (FailReads) throw new IOException("Synthetic read failure");
            return Bytes?.ToArray();
        }
        public void Write(byte[] bytes)
        {
            BeforeWrite?.Invoke(bytes);
            if (FailWrites || FailState is not null && State(bytes) == FailState) throw new IOException("Synthetic journal write failure");
            Bytes = bytes.ToArray(); Written.Add(bytes.ToArray());
            if (FailAfterState is not null && State(bytes) == FailAfterState) throw new IOException("Synthetic final flush failure after replacement");
        }
        public void Archive(byte[] expectedBytes)
        {
            if (Bytes is null || !Bytes.SequenceEqual(expectedBytes)) throw new IOException("Synthetic archive source changed");
            if (ArchiveFailure == "backup") throw new IOException("Synthetic backup write failure");
            Archived.Add(expectedBytes.ToArray());
            if (ArchiveFailure == "delete") throw new IOException("Synthetic original deletion failure");
            Bytes = null;
            if (ArchiveFailure == "after-delete") throw new IOException("Synthetic deletion completion failure");
        }
        private sealed class Lease(Action release) : IDisposable
        {
            private Action? _release = release;
            public void Dispose() => Interlocked.Exchange(ref _release, null)?.Invoke();
        }
    }
    private sealed class TemporaryRoot : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "airflash-mute-tests-" + Guid.NewGuid().ToString("N"));
        public TemporaryRoot() => Directory.CreateDirectory(Path);
        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
