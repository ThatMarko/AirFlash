using AirFlash.Core;
using Xunit;

namespace AirFlash.Tests;

public sealed class ReceiverClockRoleTests
{
    private const string Identity = "synthetic-accessory";
    private const string AccessoryModel = "AudioAccessory5,1";

    private static ServiceRecord Raop() => new("Synthetic._raop._tcp.local", "_raop._tcp.local", "192.0.2.10", 7000,
        new Dictionary<string, string>
        {
            ["deviceid"] = Identity, ["am"] = AccessoryModel, ["igl"] = "1", ["osvers"] = "27.0"
        });

    private static ServiceRecord Airplay(string? model = null)
    {
        var txt = new Dictionary<string, string> { ["deviceid"] = Identity };
        if (model is not null) txt["model"] = model;
        return new("Synthetic._airplay._tcp.local", "_airplay._tcp.local", "192.0.2.10", 7000, txt);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AddingAPreferredServiceWithoutModelPreservesTheResolvedClockRole(bool reverse)
    {
        var raop = Raop();
        var original = Assert.Single(ReceiverAggregator.Build([raop]));
        Assert.True(original.FollowReceiverClock);

        var airplay = Airplay();
        var merged = Assert.Single(ReceiverAggregator.Build(reverse ? [raop, airplay] : [airplay, raop]));
        Assert.Equal(original.Id, merged.Id);
        Assert.Equal(AccessoryModel, merged.Model);
        Assert.True(merged.FollowReceiverClock);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AConflictingPreferredModelRetainsItsExistingPriority(bool reverse)
    {
        var raop = Raop();
        var airplay = Airplay("AppleTV6,2");
        var merged = Assert.Single(ReceiverAggregator.Build(reverse ? [raop, airplay] : [airplay, raop]));
        Assert.Equal("AppleTV6,2", merged.Model);
        Assert.False(merged.FollowReceiverClock);
    }

    [Theory]
    [InlineData("osvers", "26.9")]
    [InlineData("osvers", "")]
    [InlineData("osvers", "invalid")]
    [InlineData("igl", "0")]
    [InlineData("igl", "")]
    [InlineData("tsid", "synthetic-stereo")]
    [InlineData("pgid", "synthetic-group")]
    public void AResolvedAccessoryModelStillRequiresTheExistingStandaloneClockEvidence(string key, string value)
    {
        var raop = Raop();
        raop = raop with { Txt = new Dictionary<string, string>(raop.Txt) { [key] = value } };
        var receiver = Assert.Single(ReceiverAggregator.Build([Airplay(), raop]));
        var peer = Assert.Single(receiver.Peers);
        Assert.Equal(AccessoryModel, peer.Model);
        Assert.False(peer.FollowReceiverClock);
    }
}
