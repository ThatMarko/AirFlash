using AirFlash.App.ViewModels;
using AirFlash.Core;
using Xunit;

namespace AirFlash.Tests;

public sealed class ErrorDetailsTextTests
{
    private const string Warning = "Synthetic discovery warning";
    private static readonly EngineNotice Fault = new("clock_lost", "", "clock", "Synthetic clock failure", true);

    [Fact]
    public void ReconnectRetainsThePlaybackCauseAlongsideDiscoveryWarnings()
    {
        var snapshot = new SessionSnapshot(PlaybackState.Connecting, Message: "Reconnecting (3/5)",
            Diagnostics: new(LastFault: Fault));

        Assert.Equal(Fault.Detail + Environment.NewLine + Environment.NewLine + Warning,
            ErrorDetailsText.For(snapshot, Warning));
        Assert.Same(Fault, snapshot.Diagnostics!.LastFault);
    }

    [Fact]
    public void ReconnectWithoutDiscoveryWarningStillExposesThePlaybackCause()
    {
        var snapshot = new SessionSnapshot(PlaybackState.Connecting, Diagnostics: new(LastFault: Fault));
        Assert.Equal(Fault.Detail, ErrorDetailsText.For(snapshot, ""));
    }

    [Fact]
    public void TerminalErrorRetainsBothIndependentFailures()
    {
        var snapshot = new SessionSnapshot(PlaybackState.Error, Message: Fault.Detail,
            Diagnostics: new(LastFault: Fault));
        Assert.Equal(Fault.Detail + Environment.NewLine + Environment.NewLine + Warning,
            ErrorDetailsText.For(snapshot, Warning));
    }

    [Fact]
    public void EmptyTerminalMessageFallsBackToTheRecordedFault()
    {
        var snapshot = new SessionSnapshot(PlaybackState.Error, Diagnostics: new(LastFault: Fault));
        Assert.Equal(Fault.Detail, ErrorDetailsText.For(snapshot, ""));
    }

    [Theory]
    [InlineData(PlaybackState.Idle)]
    [InlineData(PlaybackState.Streaming)]
    [InlineData(PlaybackState.Standby)]
    [InlineData(PlaybackState.Pairing)]
    public void CompletedRecoveryOrDifferentActivityDoesNotDisplayTheOldFault(PlaybackState state)
    {
        var snapshot = new SessionSnapshot(state, Diagnostics: new(LastFault: Fault));
        Assert.Equal(Warning, ErrorDetailsText.For(snapshot, Warning));
    }

    [Fact]
    public void FreshConnectionDoesNotTreatProgressTextAsAnError()
        => Assert.Equal(Warning, ErrorDetailsText.For(new(PlaybackState.Connecting, Message: "Connecting…"), Warning));

    [Fact]
    public void IdenticalPlaybackAndNoticeTextIsNotRepeated()
    {
        var snapshot = new SessionSnapshot(PlaybackState.Connecting, Diagnostics: new(LastFault: Fault));
        Assert.Equal(Fault.Detail, ErrorDetailsText.For(snapshot, Fault.Detail));
    }

    [Fact]
    public void AnEmptyIdlePanelHasNoErrorDetails()
        => Assert.Equal("", ErrorDetailsText.For(new(PlaybackState.Idle), ""));
}
