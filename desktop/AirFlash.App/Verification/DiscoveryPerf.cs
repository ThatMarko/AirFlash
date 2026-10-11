using System.Diagnostics;
using AirFlash.App.Services;
namespace AirFlash.App.Verification;

// Finite, opt-in native discovery verification; no audio, saved settings or tray.
internal static class DiscoveryPerf
{
    public static async Task<int> RunAsync(string[] args)
    {
        using var process = Process.GetCurrentProcess();
        using var discovery = new WindowsDiscovery(log: AppPaths.Log);
        var failures = new List<string>(); var receiverCount = 0;
        discovery.Failed += message => { lock (failures) failures.Add(message); };
        discovery.Changed += items => Volatile.Write(ref receiverCount, items.Count);
        var samples = new List<object>();
        async Task Sample(string phase, int seconds)
        {
            var lastCpu = process.TotalProcessorTime.TotalMilliseconds; var lastAt = Stopwatch.GetTimestamp();
            for (var i = 0; i < seconds; i++)
            {
                await Task.Delay(1000); process.Refresh();
                var now = Stopwatch.GetTimestamp(); var cpu = process.TotalProcessorTime.TotalMilliseconds;
                samples.Add(new { phase, second = i + 1,
                    cpu_percent = 100 * (cpu - lastCpu) / (Stopwatch.GetElapsedTime(lastAt, now).TotalMilliseconds * Environment.ProcessorCount),
                    threads = process.Threads.Count, handles = process.HandleCount, private_bytes = process.PrivateMemorySize64,
                    logical_queries = discovery.Snapshot.ActiveQueries,
                    retained_native_owners = NativeDnsServiceApi.CurrentOwnerCount,
                    retained_resolve_owners = NativeDnsServiceApi.RetainedResolveOwnerCount,
                    resolve_helper_owners = ResolveProcessDnsServiceApi.ProcessOwnerCount,
                    receivers = Volatile.Read(ref receiverCount) });
                lastAt = now; lastCpu = cpu;
            }
        }
        try
        {
            discovery.Start(); await Sample("visible", 150); discovery.Stop();
            var stopped = discovery.Snapshot.ActiveQueries == 0;
            for (var i = 0; i < 25; i++)
            {
                discovery.Start(); await Task.Delay(400); discovery.Stop();
                stopped &= discovery.Snapshot.ActiveQueries == 0;
            }
            await Sample("hidden", 120);
            string[] errors; lock (failures) errors = failures.ToArray();
            App.WriteOutput(args, new { ok = stopped && errors.Length == 0, all_queries_stopped = stopped,
                retained_native_owners = NativeDnsServiceApi.CurrentOwnerCount,
                resolve_helper_owners = ResolveProcessDnsServiceApi.ProcessOwnerCount,
                resolve_settlement = "observed-helper-exit", cycles = 25, failures = errors, samples });
            return stopped && errors.Length == 0 ? 0 : 1;
        }
        catch (Exception error) { App.WriteOutput(args, new { ok = false, error = error.ToString(), samples }); return 1; }
    }
}
