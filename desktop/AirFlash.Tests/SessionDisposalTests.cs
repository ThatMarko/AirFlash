using AirFlash.Core;
using Xunit;
namespace AirFlash.Tests;

public sealed class SessionDisposalTests
{
    private static readonly TimeSpan Limit = SessionTestEngine.Limit;

    [Fact]
    public async Task Disposal_waits_for_an_in_flight_diagnostic_snapshot_and_later_callers_return()
    {
        var connection = new SessionTestEngine.Connection();
        var controller = TestSessionController.Create(new SessionTestEngine.Factory(connection), new SessionTestEngine.Audio(), timing: SessionTestEngine.Timing);
        await SessionTestEngine.StreamAsync(controller, connection);
        // The snapshot holds the diagnostic writer while it waits for the reply.
        var snapshot = controller.DiagnosticSnapshotAsync();
        await connection.WaitForAsync("diag_summary");
        // Ending the session answers the pending snapshot before disposal finishes.
        await controller.DisposeAsync().AsTask().WaitAsync(Limit);
        Assert.Null(await snapshot.WaitAsync(Limit));
        // Callers that arrive after disposal (a late export, the settings window's
        // fire-and-forget preview clear) return instead of throwing.
        Assert.Null(await controller.DiagnosticSnapshotAsync().WaitAsync(Limit));
        await controller.ClearEqualizerPreviewAsync(Guid.NewGuid()).WaitAsync(Limit);
        await controller.PreviewEqualizerAsync(Guid.NewGuid(), new EqualizerSettings()).WaitAsync(Limit);
    }

    [Fact]
    public async Task A_diagnostic_requested_after_disposal_begins_does_not_wait_for_the_current_export()
    {
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var connection = new SessionTestEngine.Connection
        {
            BeforeCommandAsync = command =>
            {
                if (command != "diag_summary") return Task.CompletedTask;
                entered.TrySetResult();
                return release.Task;
            }
        };
        var controller = TestSessionController.Create(new SessionTestEngine.Factory(connection), new SessionTestEngine.Audio(), timing: SessionTestEngine.Timing);
        Task? first = null;
        Task? disposal = null;
        try
        {
            await SessionTestEngine.StreamAsync(controller, connection);
            var snapshot = controller.DiagnosticSnapshotAsync();
            first = snapshot;
            await entered.Task.WaitAsync(Limit);
            disposal = controller.DisposeAsync().AsTask();

            var late = controller.DiagnosticSnapshotAsync();

            Assert.True(late.IsCompletedSuccessfully, "disposal must reject a late export without waiting on the held diagnostic writer");
            Assert.Null(await late);
            Assert.Equal(0, connection.Count("diag_summary"));

            release.TrySetResult();
            Assert.Null(await snapshot.WaitAsync(Limit));
            Assert.Equal(1, connection.Count("diag_summary"));
        }
        finally
        {
            release.TrySetResult();
            await (disposal ?? controller.DisposeAsync().AsTask()).WaitAsync(Limit);
            if (first is not null) await first.WaitAsync(Limit);
        }
    }
}
