using AirFlash.Core;
using Xunit;
namespace AirFlash.Tests;

public sealed class NowPlayingSessionTests
{
    private static readonly TimeSpan Limit = SessionTestEngine.Limit;
    private static readonly SessionTiming Timing = SessionTestEngine.Timing;
    private static NowPlaying Track(string title) => new(title, "Artist", "Album", "", false, 240_000, 30_000);

    [Fact]
    public async Task A_value_published_before_the_stream_is_sent_when_it_starts()
    {
        var connection = new SessionTestEngine.Connection();
        await using var controller = TestSessionController.Create(new SessionTestEngine.Factory(connection), new SessionTestEngine.Audio(), timing: Timing);
        await controller.PublishNowPlayingAsync(Track("Before"));
        await SessionTestEngine.StreamAsync(controller, connection);
        Assert.Equal(["Before"], connection.NowPlayingTitles());
    }

    [Fact]
    public async Task A_cleared_source_empties_the_live_engine_and_is_not_shown_by_a_later_session()
    {
        var first = new SessionTestEngine.Connection(); var second = new SessionTestEngine.Connection();
        await using var controller = TestSessionController.Create(new SessionTestEngine.Factory(first, second), new SessionTestEngine.Audio(), timing: Timing);
        await SessionTestEngine.StreamAsync(controller, first);
        await controller.PublishNowPlayingAsync(Track("Old"));
        // The engine sends nothing to the receiver for an empty value, so the
        // display is kept while the engine's copy is dropped.
        await controller.ClearNowPlayingAsync();
        Assert.Equal(["Old", ""], first.NowPlayingTitles());
        await controller.ClearNowPlayingAsync();
        Assert.Equal(2, first.NowPlayingTitles().Length);

        await controller.StopAsync().WaitAsync(Limit);
        await SessionTestEngine.StreamAsync(controller, second);
        Assert.Empty(second.NowPlayingTitles());
    }

}
