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
        // Disposal ends the session, which answers the pending snapshot, and
        // only then disposes the writer it held.
        await controller.DisposeAsync().AsTask().WaitAsync(Limit);
        Assert.Null(await snapshot.WaitAsync(Limit));
        // Callers that arrive after disposal (a late export, the settings window's
        // fire-and-forget preview clear) return instead of throwing.
        Assert.Null(await controller.DiagnosticSnapshotAsync().WaitAsync(Limit));
        await controller.ClearEqualizerPreviewAsync(Guid.NewGuid()).WaitAsync(Limit);
        await controller.PreviewEqualizerAsync(Guid.NewGuid(), new EqualizerSettings()).WaitAsync(Limit);
    }
}
