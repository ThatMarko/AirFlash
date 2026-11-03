using System.Windows;
using System.Windows.Threading;
using AirFlash.App.Services;
using AirFlash.App.ViewModels;
using AirFlash.Core;

namespace AirFlash.App.Verification;

internal static class UiDiscoveryLifecycle
{
    internal static async Task RunAsync(List<string> checks)
    {
        var dispatcher = Application.Current.Dispatcher;
        var dispatcherThread = Environment.CurrentManagedThreadId;
        Check(dispatcher.CheckAccess(), "discovery lifecycle runs on the WPF dispatcher", checks);
        using var dns = new GatedDns();
        using var watchdog = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        var expired = 0;
        using var registration = watchdog.Token.Register(() =>
        {
            Interlocked.Exchange(ref expired, 1);
            dns.ReleaseStart.Set(); dns.ReleaseCancel.Set();
        });
        using var discovery = new WindowsDiscovery(dns, TimeProvider.System, () => [], _ => { }, queueNative: true);
        var engine = new UiSmoke.MockFactory();
        var app = new AppViewModel(new UiSmoke.MemoryStore(), discovery, new UiSmoke.MockAutostart(),
            new UiSmoke.MockAudio(), engine, dispatcher);
        try
        {
            app.SetPanelVisible(true); app.Start(); await app.Startup.WaitAsync(TimeSpan.FromSeconds(10));
            await dns.StartEntered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Check(dns.StartThread != dispatcherThread, "blocking DNS Start executes off the dispatcher", checks);
            var startPulse = false;
            await dispatcher.InvokeAsync(() => startPulse = true, DispatcherPriority.Background);
            Check(startPulse && !dns.ReleaseStart.IsSet, "dispatcher processes work while DNS Start is held", checks);
            Check(engine.CreatedCount == 0, "blocked discovery Start opens no engine", checks);
            dns.ReleaseStart.Set();
            await Task.WhenAll(dns.Operations.Select(operation => operation.StartReturned.Task)).WaitAsync(TimeSpan.FromSeconds(10));
            Check(discovery.Snapshot.ActiveQueries == 2, "both browse requests remain owned after Start", checks);
            app.SetPanelVisible(false);
            await dns.CancelEntered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Check(dns.CancelThread != dispatcherThread, "blocking DNS Cancel executes off the dispatcher", checks);
            var cancelPulse = false;
            await dispatcher.InvokeAsync(() => cancelPulse = true, DispatcherPriority.Background);
            Check(cancelPulse && !dns.ReleaseCancel.IsSet, "dispatcher processes work while DNS Cancel is held", checks);
            Check(discovery.Snapshot.ActiveQueries == 0 && discovery.Snapshot.NativeOwners == 2,
                "suspension retires logical requests while cancellation still owns inputs", checks);
            Check(engine.CreatedCount == 0 && app.Snapshot.State == PlaybackState.Idle,
                "discovery cancellation causes no playback or engine operation", checks);
        }
        finally
        {
            dns.ReleaseStart.Set(); dns.ReleaseCancel.Set();
            await app.DisposeAsync();
            await Task.WhenAll(dns.Operations.Select(operation => operation.SettledCompletion.Task)).WaitAsync(TimeSpan.FromSeconds(10));
            await Task.WhenAll(dns.Operations.Select(operation => operation.StartReturned.Task)).WaitAsync(TimeSpan.FromSeconds(10));
        }
        Check(expired == 0, "discovery barriers complete without watchdog release", checks);
        Check(discovery.Snapshot.NativeOwners == 0 && dns.Operations.All(operation => operation.CancelCalls == 1),
            "shutdown joins exact browse settlement and cancels each request once", checks);
    }

    private static void Check(bool condition, string message, List<string> checks)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks.Add(message);
    }

    private sealed class GatedDns : IDnsServiceApi, IDisposable
    {
        internal readonly ManualResetEventSlim ReleaseStart = new(), ReleaseCancel = new();
        internal readonly TaskCompletionSource StartEntered = new(TaskCreationOptions.RunContinuationsAsynchronously),
            CancelEntered = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal readonly List<Operation> Operations = [];
        internal int StartThread, CancelThread;
        public IDnsServiceOperation CreateBrowse(string type, uint index, Action<uint, IReadOnlyList<DnsAnnouncement>> callback)
        {
            var operation = new Operation(this, Operations.Count + 1, type);
            Operations.Add(operation);
            return operation;
        }
        public IDnsServiceOperation CreateResolve(string name, string type, uint index, Action<uint, ServiceRecord?> callback)
            => throw new InvalidOperationException("The dispatcher fixture advertises no receivers.");
        public void Dispose() { ReleaseStart.Dispose(); ReleaseCancel.Dispose(); }

        internal sealed class Operation(GatedDns dns, long token, string name) : IDnsServiceOperation
        {
            private readonly object _gate = new();
            private bool _started, _cancelRequested, _settled;
            internal readonly TaskCompletionSource StartReturned = new(TaskCreationOptions.RunContinuationsAsynchronously),
                SettledCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
            public long Token => token;
            public string Name => name;
            public bool IsResolve => false;
            public bool IsSettled { get { lock (_gate) return _settled; } }
            public event Action? Settled;
            public event Action<string>? CancellationFailed { add { } remove { } }
            public event Action<string>? CallbackFailed { add { } remove { } }
            internal int CancelCalls;
            public uint Start()
            {
                lock (_gate)
                {
                    if (_cancelRequested) { StartReturned.TrySetResult(); return 1223; }
                    _started = true;
                }
                dns.StartThread = Environment.CurrentManagedThreadId; dns.StartEntered.TrySetResult();
                try { dns.ReleaseStart.Wait(); return 9506; }
                finally { StartReturned.TrySetResult(); }
            }
            public uint Cancel()
            {
                Interlocked.Increment(ref CancelCalls);
                bool started;
                lock (_gate) { _cancelRequested = true; started = _started; }
                dns.CancelThread = Environment.CurrentManagedThreadId; dns.CancelEntered.TrySetResult();
                dns.ReleaseCancel.Wait();
                if (started) StartReturned.Task.GetAwaiter().GetResult();
                else StartReturned.TrySetResult();
                Action? settled = null;
                lock (_gate) { if (!_settled) { _settled = true; settled = Settled; } }
                settled?.Invoke();
                SettledCompletion.TrySetResult(); return 0;
            }
        }
    }
}
