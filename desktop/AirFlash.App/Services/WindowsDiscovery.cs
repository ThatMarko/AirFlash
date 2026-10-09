using System.ComponentModel;
using System.Net.NetworkInformation;
using AirFlash.Core;
namespace AirFlash.App.Services;

// Logical deadlines do not settle native owners. A late callback can release its owner,
// but only a current epoch/token may change advertised or cached receiver state.
public sealed partial class WindowsDiscovery : IDiscoveryService
{
    private const uint PendingStatus = 9506;
    private readonly object _sync = new();
    private readonly object _publication = new();
    private readonly IDnsServiceApi _dns;
    private readonly TimeProvider _clock;
    private readonly long _origin;
    private readonly Func<IReadOnlyList<DiscoveryAdapter>> _adapters;
    private readonly Action<string> _log;
    private readonly bool _enableInfrastructure;
    private readonly DiscoveryPolicy _policy;
    private readonly Dictionary<string, Advertisement> _advertised = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CachedRecord> _records = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Owner> _pending = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<long, Owner> _owners = [];
    private readonly Dictionary<string, Browser> _browsers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["_airplay._tcp.local"] = new("_airplay._tcp.local"),
        ["_raop._tcp.local"] = new("_raop._tcp.local")
    };
    private HashSet<string> _diagnostics = new(StringComparer.Ordinal);
    private ITimer? _timer;
    private string _interfaceId = "";
    private uint? _interfaceIndex;
    private TimeSpan _nextAdapterCheck;
    private long _epoch;
    private long _restartSequence;
    private bool _started;
    private bool _disposed;
    private bool _capacityReported;
    private Work? _deferredNativeWork;
    public event Action<IReadOnlyList<Receiver>>? Changed;
    public event Action<string>? Failed;

    internal WindowsDiscovery(IDnsServiceApi dns, TimeProvider clock,
        Func<IReadOnlyList<DiscoveryAdapter>> adapters, Action<string> log,
        bool enableInfrastructure = false, DiscoveryPolicy? policy = null)
    {
        _dns = dns; _clock = clock; _origin = clock.GetTimestamp(); _adapters = adapters;
        _log = log; _enableInfrastructure = enableInfrastructure; _policy = policy ?? new();
        if (_policy.ResolveTimeout <= TimeSpan.Zero || _policy.RefreshInterval <= TimeSpan.Zero
            || _policy.RecordTtl <= TimeSpan.Zero || _policy.RetryBase <= TimeSpan.Zero
            || _policy.RetryMaximum < _policy.RetryBase || _policy.MaxOwners < 2
            || _policy.MaxResolveOwnersPerName < 1) throw new ArgumentOutOfRangeException(nameof(policy));
    }

    private TimeSpan Now => _clock.GetElapsedTime(_origin, _clock.GetTimestamp());
    internal DiscoverySnapshot Snapshot
    {
        get { lock (_sync) return new(_epoch, _advertised.Count, _records.Count, _pending.Count, _owners.Count); }
    }

    public void SetInterface(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        lock (_sync)
        {
            if (_disposed || _interfaceId == id) return;
            _interfaceId = id;
            if (!_started) return;
        }
        Restart();
    }

    public void Start()
    {
        lock (_sync)
        {
            if (_disposed || _started) return;
            _started = true;
            _nextAdapterCheck = Now + _policy.RefreshInterval;
            if (_enableInfrastructure)
            {
                NetworkChange.NetworkAddressChanged += NetworkChanged;
                NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChanged;
                // The one-second maintenance tick bounds deadlines independently of the
                // less frequent adapter enumeration and successful resolve refreshes.
                _timer = _clock.CreateTimer(_ => Tick(), null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            }
        }
        Restart();
    }

    private void NetworkChanged(object? sender, EventArgs args) => Restart();
    private void NetworkAvailabilityChanged(object? sender, NetworkAvailabilityEventArgs args) => Restart();

    private void Restart()
    {
        string selected; long sequence;
        lock (_sync)
        {
            if (_disposed || !_started) return;
            selected = _interfaceId; sequence = ++_restartSequence;
        }
        uint? index; string? error = null;
        try { index = DiscoveryInterface.ResolveIndex(selected, _adapters()); }
        catch (NetworkInformationException exception) { error = exception.Message; index = null; }
        var work = new Work { Publish = true };
        lock (_sync)
        {
            if (_disposed || !_started || sequence != _restartSequence || selected != _interfaceId) return;
            ++_epoch; _interfaceIndex = index; _nextAdapterCheck = Now + _policy.RefreshInterval;
            foreach (var owner in _owners.Values) RetireLocked(owner, work);
            _pending.Clear(); _advertised.Clear(); _records.Clear();
            foreach (var browser in _browsers.Values)
            {
                browser.Owner = null; browser.Failures = 0; browser.NextAttempt = Now;
            }
            if (error is not null) work.Errors.Add(error);
            if (index is null) work.Errors.Add(L.Get("The selected network interface is unavailable. Discovery is paused."));
            else ScheduleLocked(Now, work);
        }
        Run(work);
    }

    internal void Tick()
    {
        string selected; uint? previous; long epoch; bool checkAdapter;
        lock (_sync)
        {
            if (_disposed || !_started) return;
            selected = _interfaceId; previous = _interfaceIndex; epoch = _epoch;
            checkAdapter = Now >= _nextAdapterCheck;
            if (checkAdapter) _nextAdapterCheck = Now + _policy.RefreshInterval;
        }
        if (checkAdapter)
        {
            try
            {
                var current = DiscoveryInterface.ResolveIndex(selected, _adapters());
                lock (_sync)
                {
                    if (_disposed || epoch != _epoch || selected != _interfaceId) return;
                }
                if (current != previous) { Restart(); return; }
            }
            catch (NetworkInformationException exception)
            {
                Report(exception.Message); Restart(); return;
            }
        }
        var work = new Work();
        lock (_sync)
        {
            if (_disposed || !_started || epoch != _epoch) return;
            var now = Now;
            foreach (var name in _advertised.Where(pair => pair.Value.Expires <= now).Select(pair => pair.Key).ToArray())
                RemoveAdvertisementLocked(name, work);
            foreach (var name in _records.Where(pair => pair.Value.Expires <= now).Select(pair => pair.Key).ToArray())
            {
                _records.Remove(name); work.Publish = true;
            }
            foreach (var owner in _pending.Values.Where(owner => owner.Deadline <= now).ToArray())
            {
                FailResolveLocked(owner, now, work);
                work.Errors.Add(L.Format("Receiver resolution timed out: {0}. Discovery will retry with backoff.", owner.Name));
            }
            ScheduleLocked(now, work);
        }
        Run(work);
    }

    private void ScheduleLocked(TimeSpan now, Work work)
    {
        if (_disposed || !_started || _interfaceIndex is not uint index) return;
        foreach (var browser in _browsers.Values)
        {
            if (browser.Owner is not null || browser.NextAttempt > now) continue;
            if (!HasCapacityLocked(null, work)) continue;
            var weak = new WeakReference<WindowsDiscovery>(this); Owner? owner = null;
            try
            {
                var operation = _dns.CreateBrowse(browser.Type, index, (status, announcements) =>
                {
                    if (owner is not null && weak.TryGetTarget(out var target)) target.Browse(owner, status, announcements);
                });
                owner = new(operation, browser.Type, browser.Type, _epoch, false);
                RegisterLocked(owner); browser.Owner = owner; work.Starts.Add(owner); _capacityReported = false;
            }
            catch (DnsServiceCapacityException) { CapacityLocked(work); }
        }
        foreach (var advertisement in _advertised.Values)
        {
            if (_pending.ContainsKey(advertisement.Name) || advertisement.NextAttempt > now || advertisement.Expires <= now) continue;
            if (!HasCapacityLocked(advertisement.Name, work)) continue;
            var weak = new WeakReference<WindowsDiscovery>(this); Owner? owner = null;
            try
            {
                var operation = _dns.CreateResolve(advertisement.Name, advertisement.Type, index, (status, record) =>
                {
                    if (owner is not null && weak.TryGetTarget(out var target)) target.Resolved(owner, status, record);
                });
                owner = new(operation, advertisement.Name, advertisement.Type, _epoch, true) { Deadline = now + _policy.ResolveTimeout };
                RegisterLocked(owner); _pending[advertisement.Name] = owner; work.Starts.Add(owner); _capacityReported = false;
            }
            catch (DnsServiceCapacityException) { CapacityLocked(work); }
        }
    }

    private bool HasCapacityLocked(string? name, Work work)
    {
        if (_owners.Count < _policy.MaxOwners && (name is null || _owners.Values.Count(owner => owner.Resolve
            && owner.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) < _policy.MaxResolveOwnersPerName)) return true;
        CapacityLocked(work); return false;
    }

    private void CapacityLocked(Work work)
    {
        if (_capacityReported) return;
        _capacityReported = true;
        work.Errors.Add(L.Get("Some discovery requests are paused because DNS operations have not finished cancellation. They will resume when those operations complete; restarting the app is the final recovery."));
    }

    private void RegisterLocked(Owner owner)
    {
        var weak = new WeakReference<WindowsDiscovery>(this); var token = owner.Operation.Token;
        owner.SettledHandler = () => { if (weak.TryGetTarget(out var target)) target.Settled(token); };
        owner.CancellationFailedHandler = message =>
        {
            if (weak.TryGetTarget(out var target)) target.CancellationFailed(token, message);
        };
        _owners.Add(token, owner);
        owner.Operation.Settled += owner.SettledHandler;
        owner.Operation.CancellationFailed += owner.CancellationFailedHandler;
    }

    private void Browse(Owner owner, uint status, IReadOnlyList<DnsAnnouncement> announcements)
    {
        var work = new Work();
        lock (_sync)
        {
            if (_disposed || owner.Retired || owner.Epoch != _epoch
                || !_browsers.TryGetValue(owner.Type, out var browser) || !ReferenceEquals(browser.Owner, owner)) return;
            var now = Now;
            if (status != 0)
            {
                FailBrowseLocked(owner, now, work);
                work.Errors.Add(L.Format("Receiver discovery failed: {0}. You can add a receiver manually.", new Win32Exception((int)status).Message));
            }
            else
            {
                browser.Failures = 0;
                foreach (var announcement in announcements)
                {
                    if (string.IsNullOrEmpty(announcement.Name)) continue;
                    if (announcement.Ttl == 0) { RemoveAdvertisementLocked(announcement.Name, work); continue; }
                    if (_advertised.TryGetValue(announcement.Name, out var advertised)) advertised.Expires = now + TimeSpan.FromSeconds(announcement.Ttl);
                    else _advertised[announcement.Name] = new(announcement.Name, owner.Type, now + TimeSpan.FromSeconds(announcement.Ttl), now);
                }
                ScheduleLocked(now, work);
            }
        }
        Run(work);
    }

    private void Resolved(Owner owner, uint status, ServiceRecord? record)
    {
        var work = new Work();
        lock (_sync)
        {
            // Resolve callbacks are terminal: the adapter settles the owner after this
            // handler returns, including ignored callbacks. No cancellation is needed.
            owner.TerminalCallback = true;
            if (_disposed || owner.Retired || owner.Epoch != _epoch || !_pending.TryGetValue(owner.Name, out var pending)
                || !ReferenceEquals(pending, owner) || !_advertised.TryGetValue(owner.Name, out var advertisement)) return;
            var now = Now;
            if (advertisement.Expires <= now) RemoveAdvertisementLocked(owner.Name, work);
            else if (owner.Deadline <= now)
            {
                FailResolveLocked(owner, now, work);
                work.Errors.Add(L.Format("Receiver resolution timed out: {0}. Discovery will retry with backoff.", owner.Name));
            }
            else if (status != 0 || record is null || record.Port is <= 0 or > 65535
                || string.IsNullOrWhiteSpace(record.Address) || !record.Instance.Equals(owner.Name, StringComparison.OrdinalIgnoreCase)
                || !record.ServiceType.Equals(owner.Type, StringComparison.OrdinalIgnoreCase))
            {
                FailResolveLocked(owner, now, work);
                work.Errors.Add(L.Format("Receiver resolution failed: {0}. Discovery will retry with backoff.", owner.Name));
            }
            else
            {
                _pending.Remove(owner.Name); owner.Retired = true;
                advertisement.Failures = 0; advertisement.NextAttempt = now + _policy.RefreshInterval;
                if (!_records.TryGetValue(owner.Name, out var previous) || Describe(previous.Record) != Describe(record))
                    work.Logs.Add("Discovery resolved: " + Describe(record));
                _records[owner.Name] = new(record, now + _policy.RecordTtl); work.Publish = true;
            }
        }
        Run(work);
    }

    private void RemoveAdvertisementLocked(string name, Work work)
    {
        _advertised.Remove(name);
        if (_records.Remove(name)) work.Publish = true;
        if (_pending.Remove(name, out var pending)) RetireLocked(pending, work);
    }

    private void FailResolveLocked(Owner owner, TimeSpan now, Work work)
    {
        if (_pending.TryGetValue(owner.Name, out var pending) && ReferenceEquals(pending, owner))
        {
            _pending.Remove(owner.Name);
            if (_advertised.TryGetValue(owner.Name, out var advertisement))
                advertisement.NextAttempt = now + Backoff(++advertisement.Failures);
        }
        RetireLocked(owner, work);
    }

    private void FailBrowseLocked(Owner owner, TimeSpan now, Work work)
    {
        if (_browsers.TryGetValue(owner.Type, out var browser) && ReferenceEquals(browser.Owner, owner))
        {
            browser.Owner = null; browser.NextAttempt = now + Backoff(++browser.Failures);
        }
        RetireLocked(owner, work);
    }

    private TimeSpan Backoff(int failures) => TimeSpan.FromTicks((long)Math.Min(_policy.RetryMaximum.Ticks,
        _policy.RetryBase.Ticks * Math.Pow(2, Math.Min(failures - 1, 30))));

    private void RetireLocked(Owner owner, Work work)
    {
        owner.Retired = true;
        if (owner.CancelRequested || owner.TerminalCallback || owner.Operation.IsSettled) return;
        owner.CancelRequested = true; work.Cancels.Add(owner.Operation);
    }

    private void Settled(long token)
    {
        var work = new Work();
        lock (_sync)
        {
            if (!_owners.Remove(token, out var owner)) return;
            owner.Operation.Settled -= owner.SettledHandler;
            owner.Operation.CancellationFailed -= owner.CancellationFailedHandler;
            if (_disposed || owner.Retired || owner.Epoch != _epoch) return;
            // A non-pending Start can settle the adapter before its return value is
            // observed. Retire now, but retain the reason so Run reports that result.
            owner.UnexpectedSettlement = true;
            if (owner.Resolve) FailResolveLocked(owner, Now, work);
            else FailBrowseLocked(owner, Now, work);
        }
        Run(work);
    }

    private void CancellationFailed(long token, string message)
    {
        lock (_sync) { if (_disposed || !_owners.ContainsKey(token)) return; }
        Report(L.Format("DNS cancellation failed: {0}. The operation remains safely owned until completion.", message));
    }

    private void Run(Work work)
    {
        if (Monitor.IsEntered(_publication))
        {
            // External subscribers may restart/dispose reentrantly. Native cancellation
            // can wait for a concurrent callback trying to publish, so defer only native
            // calls until the outermost publication releases its lock. Coalescing by token
            // and pruning settled owners keeps this queue bounded by native admission.
            if (work.Cancels.Count != 0 || work.Starts.Count != 0)
            {
                var deferred = _deferredNativeWork ??= new();
                deferred.Cancels.RemoveAll(operation => operation.IsSettled);
                deferred.Starts.RemoveAll(owner => owner.Operation.IsSettled);
                foreach (var operation in work.Cancels)
                    if (!operation.IsSettled && deferred.Cancels.All(existing => existing.Token != operation.Token))
                        deferred.Cancels.Add(operation);
                foreach (var owner in work.Starts)
                    if (!owner.Operation.IsSettled && deferred.Starts.All(existing => existing.Operation.Token != owner.Operation.Token))
                        deferred.Starts.Add(owner);
            }
        }
        else RunNative(work);
        foreach (var message in work.Logs) Log(message);
        foreach (var error in work.Errors) Report(error);
        if (work.Publish) Publish();
    }

    private void DrainDeferredNative()
    {
        if (Monitor.IsEntered(_publication)) return;
        while (true)
        {
            Work? work;
            lock (_publication) { work = _deferredNativeWork; _deferredNativeWork = null; }
            if (work is null) return;
            RunNative(work);
        }
    }

    private void RunNative(Work work)
    {
        foreach (var operation in work.Cancels)
        {
            try
            {
                var status = operation.Cancel();
                if (status != 0 && !operation.IsSettled) Report(L.Format("DNS cancellation failed: {0}. The operation remains safely owned until completion.", new Win32Exception((int)status).Message));
            }
            catch (Exception error) when (IsOperationFailure(error))
            {
                Report(L.Format("DNS cancellation failed: {0}. The operation remains safely owned until completion.", error.Message));
            }
        }
        foreach (var owner in work.Starts)
        {
            Work? abandoned = null;
            lock (_sync)
            {
                if (_disposed || owner.Retired || owner.Epoch != _epoch || owner.Operation.IsSettled)
                {
                    abandoned = new(); RetireLocked(owner, abandoned);
                }
            }
            if (abandoned is not null) { Run(abandoned); continue; }
            uint status;
            try { status = owner.Operation.Start(); }
            catch (Exception error) when (IsOperationFailure(error))
            {
                StartFailed(owner); Report(L.Format("Receiver discovery failed: {0}. You can add a receiver manually.", error.Message)); continue;
            }
            var followup = new Work();
            lock (_sync)
            {
                // Inline completion/retirement wins over Start's return value. Adapter
                // settlement before a failed Start returns has already applied backoff;
                // report its status without retiring or incrementing failures twice.
                if (owner.Retired || _disposed || owner.Epoch != _epoch)
                {
                    RetireLocked(owner, followup);
                    if (status != PendingStatus && owner.UnexpectedSettlement && !_disposed && owner.Epoch == _epoch)
                        followup.Errors.Add(L.Format("DNS request did not remain pending: {0}. Discovery will retry with backoff.", status));
                }
                else if (status != PendingStatus)
                {
                    if (owner.Resolve) FailResolveLocked(owner, Now, followup);
                    else FailBrowseLocked(owner, Now, followup);
                    followup.Errors.Add(L.Format("DNS request did not remain pending: {0}. Discovery will retry with backoff.", status));
                }
            }
            Run(followup);
        }
    }

    private void StartFailed(Owner owner)
    {
        var work = new Work();
        lock (_sync)
        {
            if (owner.Retired || _disposed || owner.Epoch != _epoch) RetireLocked(owner, work);
            else if (owner.Resolve) FailResolveLocked(owner, Now, work);
            else FailBrowseLocked(owner, Now, work);
        }
        Run(work);
    }

    private static bool IsOperationFailure(Exception error) => error is Win32Exception or InvalidOperationException
        or DllNotFoundException or EntryPointNotFoundException or BadImageFormatException;

    private void Report(string message)
    {
        try
        {
            lock (_publication)
            {
                lock (_sync) { if (_disposed) return; }
                Log(message); InvokeObservers(Failed, message, "failure");
            }
        }
        finally { DrainDeferredNative(); }
    }

    private void Publish()
    {
        try
        {
            lock (_publication)
            {
                IReadOnlyList<Receiver> receivers; string[] messages;
                lock (_sync)
                {
                    if (_disposed) return;
                    var diagnostics = new HashSet<string>(StringComparer.Ordinal);
                    receivers = ReceiverAggregator.Build(_records.Values.Select(record => record.Record), message => diagnostics.Add(message));
                    messages = diagnostics.Except(_diagnostics).ToArray(); _diagnostics = diagnostics;
                }
                foreach (var message in messages) Log(message);
                InvokeObservers(Changed, receivers, "catalog");
            }
        }
        finally { DrainDeferredNative(); }
    }

    private void Log(string message)
    {
        try { _log(message); }
        catch (Exception error) { System.Diagnostics.Trace.TraceError("Discovery log observer: {0}", error.Message); }
    }

    private static void InvokeObservers<T>(Action<T>? observers, T value, string channel)
    {
        if (observers is null) return;
        foreach (Action<T> observer in observers.GetInvocationList())
            try { observer(value); }
            catch (Exception error) { System.Diagnostics.Trace.TraceError("Discovery {0} observer: {1}", channel, error.Message); }
    }

    private static string Describe(ServiceRecord record) => System.Text.Json.JsonSerializer.Serialize(new
    {
        record.Instance, record.ServiceType, record.Address, record.Port,
        DeviceId = record.Txt.GetValueOrDefault("deviceid", ""),
        Model = record.Txt.GetValueOrDefault("model", record.Txt.GetValueOrDefault("am", "")),
        StereoId = record.Txt.GetValueOrDefault("tsid", "")
    });

    public void Dispose()
    {
        var work = new Work(); ITimer? timer;
        lock (_publication)
        {
            lock (_sync)
            {
                if (_disposed) return;
                _disposed = true; ++_epoch; ++_restartSequence; timer = _timer; _timer = null;
                foreach (var owner in _owners.Values) RetireLocked(owner, work);
                _pending.Clear(); _advertised.Clear(); _records.Clear();
                foreach (var browser in _browsers.Values) browser.Owner = null;
                if (_enableInfrastructure)
                {
                    NetworkChange.NetworkAddressChanged -= NetworkChanged;
                    NetworkChange.NetworkAvailabilityChanged -= NetworkAvailabilityChanged;
                }
            }
        }
        timer?.Dispose(); Run(work);
    }

    private sealed class Advertisement(string name, string type, TimeSpan expires, TimeSpan nextAttempt)
    {
        public string Name { get; } = name;
        public string Type { get; } = type;
        public TimeSpan Expires { get; set; } = expires;
        public TimeSpan NextAttempt { get; set; } = nextAttempt;
        public int Failures { get; set; }
    }
    private sealed record CachedRecord(ServiceRecord Record, TimeSpan Expires);
    private sealed class Browser(string type)
    {
        public string Type { get; } = type;
        public Owner? Owner { get; set; }
        public TimeSpan NextAttempt { get; set; }
        public int Failures { get; set; }
    }
    private sealed class Owner(IDnsServiceOperation operation, string name, string type, long epoch, bool resolve)
    {
        public IDnsServiceOperation Operation { get; } = operation;
        public string Name { get; } = name;
        public string Type { get; } = type;
        public long Epoch { get; } = epoch;
        public bool Resolve { get; } = resolve;
        public TimeSpan Deadline { get; init; }
        public bool Retired { get; set; }
        public bool TerminalCallback { get; set; }
        public bool UnexpectedSettlement { get; set; }
        public bool CancelRequested { get; set; }
        public Action? SettledHandler { get; set; }
        public Action<string>? CancellationFailedHandler { get; set; }
    }
    private sealed class Work
    {
        public List<Owner> Starts { get; } = [];
        public List<IDnsServiceOperation> Cancels { get; } = [];
        public List<string> Logs { get; } = [];
        public List<string> Errors { get; } = [];
        public bool Publish { get; set; }
    }
}
