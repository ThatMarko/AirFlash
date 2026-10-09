using AirFlash.Core;
namespace AirFlash.App.Services;

public sealed partial class WindowsDiscovery
{
    public WindowsDiscovery(Func<IReadOnlyList<DiscoveryAdapter>>? adapters = null)
        : this(new NativeDnsServiceApi(), TimeProvider.System, adapters ?? NetworkAdapterCatalog.List, AppPaths.Log, enableInfrastructure: true) { }
}
