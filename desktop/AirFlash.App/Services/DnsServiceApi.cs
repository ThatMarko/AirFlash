using AirFlash.Core;
namespace AirFlash.App.Services;

internal sealed record DnsAnnouncement(string Name, uint Ttl);
internal interface IDnsServiceOperation
{
    long Token { get; }
    string Name { get; }
    bool IsResolve { get; }
    bool IsSettled { get; }
    event Action? Settled;
    // Only asynchronously queued native cancellation failures use this channel.
    // A synchronous Cancel returns its status or throws to its caller instead.
    event Action<string>? CancellationFailed;
    uint Start();
    uint Cancel();
}
internal interface IDnsServiceApi
{
    // Creation registers an owner but must not invoke callbacks until Start.
    IDnsServiceOperation CreateBrowse(string type, uint interfaceIndex, Action<uint, IReadOnlyList<DnsAnnouncement>> callback);
    IDnsServiceOperation CreateResolve(string name, string type, uint interfaceIndex, Action<uint, ServiceRecord?> callback);
}
internal sealed class DnsServiceCapacityException : InvalidOperationException
{
    public DnsServiceCapacityException() : base(L.Get("Some discovery requests are paused because DNS operations have not finished cancellation. They will resume when those operations complete; restarting the app is the final recovery.")) { }
}
internal sealed record DiscoveryPolicy
{
    public TimeSpan ResolveTimeout { get; init; } = TimeSpan.FromSeconds(5);
    public TimeSpan RefreshInterval { get; init; } = TimeSpan.FromSeconds(20);
    public TimeSpan RecordTtl { get; init; } = TimeSpan.FromSeconds(90);
    public TimeSpan RetryBase { get; init; } = TimeSpan.FromSeconds(1);
    public TimeSpan RetryMaximum { get; init; } = TimeSpan.FromSeconds(30);
    public int MaxOwners { get; init; } = 64;
    public int MaxResolveOwnersPerName { get; init; } = 2;
}
internal sealed record DiscoverySnapshot(long Epoch, int AdvertisedNames, int CachedRecords, int PendingResolves, int NativeOwners);
