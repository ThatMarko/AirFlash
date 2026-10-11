using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AirFlash.App.Services;
using AirFlash.App.Ui;
using AirFlash.App.ViewModels;
using AirFlash.Core;

namespace AirFlash.App.Verification;

// Exercises actual Monitor bindings and serialized session events. Clipboard writes
// are injected; no production endpoint, receiver, engine or settings store is used.
internal static class UiDiagnostics
{
    public static async Task RunAsync(List<string> checks, string directory)
    {
        var store = new Store(); var audio = new Audio(); var factory = new Factory();
        await using var app = new AppViewModel(store, new UiSmoke.MockDiscovery { Items = [] }, new UiSmoke.MockAutostart(), audio, factory,
            Application.Current.Dispatcher) { EngineVersion = "synthetic-engine" };
        var writer = new Writer(); SettingsWindow? window = null;
        try
        {
            app.Start(); await app.Startup;
            window = Open(app, writer); await Pump();
            await CopyAsync(window, app, writer, store, factory, audio, checks, "Idle");
            Check(Values(window) == ("—", "—"), "Idle Monitor shows unavailable padding fields", checks);
            writer.Fail = true;
            await CopyAsync(window, app, writer, store, factory, audio, checks, "controlled clipboard error");
            Check(window.ViewModel.Error == Writer.Failure && Descendants(window).OfType<TextBlock>().Any(block => block.IsVisible && block.Text == Writer.Failure),
                "clipboard failure appears in actual Settings error surface without escaping", checks);
            writer.Fail = false; Render(window, Path.Combine(directory, "diagnostics-idle.png"));

            var receiver = app.Receivers.Single().Receiver;
            var streaming = Snapshot(app.Session, item => item.State == PlaybackState.Streaming);
            await app.ToggleAsync(receiver); await streaming; await Pump();
            await MetricsAsync(app, factory, 7); await Pump();
            Check(Values(window) == ("—", "—") && app.Snapshot.Metrics is { PaddedFrames: null, FullyPaddedPackets: null },
                "older capture events retain underruns and leave both new Monitor counters unavailable", checks);
            await CopyAsync(window, app, writer, store, factory, audio, checks, "Streaming with old producer");

            await MetricsAsync(app, factory, 8, ("padded_frames", 353UL)); await Pump();
            Check(Values(window) == ("353", "—"), "live Monitor independently updates padded output frames when whole-pull count is absent", checks);
            await MetricsAsync(app, factory, 9, ("fully_padded_packets", 2UL)); await Pump();
            Check(Values(window) == ("—", "2"), "live Monitor independently updates wholly padded pulls when frame count is absent", checks);
            await MetricsAsync(app, factory, 10, ("padded_frames", ulong.MaxValue), ("fully_padded_packets", ulong.MaxValue)); await Pump();
            var maximum = ulong.MaxValue.ToString();
            Check(Values(window) == (maximum, maximum), "actual Monitor preserves both full UInt64 counter values without floating-point loss", checks);
            await CopyAsync(window, app, writer, store, factory, audio, checks, "Streaming with new producer");
            Check(writer.Last.Contains(maximum, StringComparison.Ordinal), "existing diagnostics serializer preserves UInt64 padding values", checks);
            await ShowPaddingAsync(window);
            var normal = PaddingLayout(window);
            Render(window, Path.Combine(directory, "diagnostics-padding-max.png"));
            var width = window.Width; window.Width = window.MinWidth; await Pump(); await ShowPaddingAsync(window);
            var minimum = PaddingLayout(window);
            Render(window, Path.Combine(directory, "diagnostics-padding-minimum.png"));
            File.WriteAllText(Path.Combine(directory, "diagnostics-padding-layout.json"), JsonSerializer.Serialize(new { normal, minimum }, new JsonSerializerOptions { WriteIndented = true }));
            Check(normal.Length == 2 && normal.All(item => item.Fits && item.Visible), "full UInt64 Monitor counters fit or wrap inside their actual visible metric cards", checks);
            Check(minimum.Length == 2 && minimum.All(item => item.Fits && item.Visible), "full UInt64 Monitor counters fit or wrap in fully visible cards at minimum Settings width", checks);
            window.Width = width; await Pump();

            await MetricsAsync(app, factory, 11, ("padded_frames", -1), ("fully_padded_packets", "2")); await Pump();
            Check(Values(window) == ("—", "—") && app.Snapshot.Metrics is { Underruns: 11 },
                "invalid optional padding fields display unavailable while existing metrics remain usable", checks);
            window.Close(); window = null;
            await MetricsAsync(app, factory, 12, ("padded_frames", 705UL), ("fully_padded_packets", 2UL));
            var error = Snapshot(app.Session, item => item.State == PlaybackState.Error);
            factory.Latest.Emit(new { @event = "error", code = "synthetic_error", kind = "rtsp", retryable = false, message = "synthetic receiver fault", host = "192.0.2.30" });
            await error; await Pump();
            window = Open(app, writer); await Pump();
            Check(Values(window) == ("705", "2"), "reopened Monitor refreshes the latest capture counters after hidden updates", checks);
            var expander = Descendants(window).OfType<Expander>().Single(item => Equals(item.Header, L.Get("Statistics before the last failure")));
            expander.IsExpanded = true; expander.BringIntoView(); await Pump();
            var before = Descendants(window).OfType<TextBlock>().Single(block => block.Name == "MonitorBeforeFaultValue");
            Check(app.Snapshot.Diagnostics?.CaptureBeforeFault is { PaddedFrames: 705, FullyPaddedPackets: 2 }
                && before.Text == app.MonitorBeforeFault && before.Text.Contains("705", StringComparison.Ordinal),
                "actual pre-fault Monitor text retains its own capture-padding snapshot", checks);
            await CopyAsync(window, app, writer, store, factory, audio, checks, "Error");
            Check(writer.Last.Contains("synthetic receiver fault", StringComparison.Ordinal) && writer.Last.Contains("705", StringComparison.Ordinal),
                "Error diagnostics include the original failure and retained padding metrics", checks);
            before.BringIntoView(); await Pump();
            Render(window, Path.Combine(directory, "diagnostics-before-fault.png"));

            streaming = Snapshot(app.Session, item => item.State == PlaybackState.Streaming && item.Metrics is null);
            await app.ToggleAsync(receiver); await streaming; await Pump();
            Check(Values(window) == ("—", "—") && app.MonitorBeforeFault == "—",
                "new playback attempt clears live and previous-session padding displays", checks);
            await MetricsAsync(app, factory, 1, ("padded_frames", 0UL), ("fully_padded_packets", 0UL)); await Pump();
            Check(Values(window) == ("0", "0"), "fresh worker zero counters display as measured zeros rather than unavailable", checks);
            await CopyAsync(window, app, writer, store, factory, audio, checks, "fresh Streaming attempt");
        }
        finally { window?.Close(); await app.StopAsync(); }
    }

    private static SettingsWindow Open(AppViewModel app, Writer writer)
    {
        var window = new SettingsWindow(app, writer.Write); window.ViewModel.SelectedPage = SettingsViewModel.MonitorPage;
        window.Show(); window.Activate(); return window;
    }
    private static async Task CopyAsync(SettingsWindow window, AppViewModel app, Writer writer, Store store, Factory factory, Audio audio,
        List<string> checks, string state)
    {
        var button = Descendants(window).OfType<Button>().Single(item => item.Name == "CopyDiagnosticsButton");
        button.BringIntoView(); await Pump(); window.Activate();
        Check(ReferenceEquals(button.Command, window.ViewModel.CopyDiagnosticsCommand) && button.IsVisible && button.IsEnabled
            && AutomationProperties.GetName(button) == L.Get("Copy diagnostics") && button.Focus() && button.IsKeyboardFocused,
            $"{state} exposes the localized keyboard-accessible existing diagnostics command", checks);
        var expected = app.Diagnostics(); var saves = store.Saves; var commands = factory.Commands.Count;
        var mutes = audio.Mutes; var restores = audio.Restores; var calls = writer.Calls;
        var copied = writer.Next = new(TaskCreationOptions.RunContinuationsAsynchronously);
        ((IInvokeProvider)new ButtonAutomationPeer(button).GetPattern(PatternInterface.Invoke)).Invoke();
        var actual = await copied.Task.WaitAsync(TimeSpan.FromSeconds(10)); await Pump();
        Check(actual == expected && writer.Calls == calls + 1 && writer.OnDispatcher && writer.Apartment == ApartmentState.STA,
            $"{state} actual button copies the existing serializer snapshot on the STA dispatcher", checks);
        Check(store.Saves == saves && factory.Commands.Count == commands && audio.Mutes == mutes && audio.Restores == restores,
            $"{state} diagnostics copy causes no save, engine command or local audio mutation", checks);
    }
    private static (string, string) Values(SettingsWindow window)
    {
        var blocks = Descendants(window).OfType<TextBlock>().ToArray();
        return (blocks.Single(block => block.Name == "MonitorPaddedFramesValue").Text,
            blocks.Single(block => block.Name == "MonitorFullyPaddedPacketsValue").Text);
    }
    private sealed record PaddingGeometry(string Name, double CardWidth, double InnerWidth, double GlyphWidth,
        double ValueWidth, double ValueHeight, double RequiredHeight, int RenderedCharacters, bool AllGlyphsInsideValue, double CardViewportTop, double CardHeight,
        double ViewportHeight, bool Fits, bool Visible);
    private static PaddingGeometry[] PaddingLayout(SettingsWindow window) => Descendants(window).OfType<TextBlock>()
        .Where(block => block.Name is "MonitorPaddedFramesValue" or "MonitorFullyPaddedPacketsValue").Select(block =>
        {
            var card = Ancestor<Border>(block);
            var viewport = Ancestor<ScrollContentPresenter>(card);
            var inner = new Rect(card.BorderThickness.Left + card.Padding.Left, card.BorderThickness.Top + card.Padding.Top,
                Math.Max(0, card.ActualWidth - card.BorderThickness.Left - card.BorderThickness.Right - card.Padding.Left - card.Padding.Right),
                Math.Max(0, card.ActualHeight - card.BorderThickness.Top - card.BorderThickness.Bottom - card.Padding.Top - card.Padding.Bottom));
            var origin = block.TranslatePoint(new(), card);
            var text = new FormattedText(block.Text, CultureInfo.CurrentCulture, block.FlowDirection,
                new Typeface(block.FontFamily, block.FontStyle, block.FontWeight, block.FontStretch), block.FontSize,
                Brushes.Black, VisualTreeHelper.GetDpi(block).PixelsPerDip);
            var glyphWidth = text.WidthIncludingTrailingWhitespace;
            var run = block.Inlines.OfType<Run>().Single();
            // WPF's public pointer rectangles describe caret positions. Join both
            // boundaries of each digit, preserving the preceding line at a wrap.
            var characters = Enumerable.Range(0, run.Text.Length).Select(index => Rect.Union(
                run.ContentStart.GetPositionAtOffset(index, LogicalDirection.Forward)!.GetCharacterRect(LogicalDirection.Forward),
                run.ContentStart.GetPositionAtOffset(index + 1, LogicalDirection.Backward)!.GetCharacterRect(LogicalDirection.Backward))).ToArray();
            var allGlyphsInside = run.Text == block.Text && characters.Length == block.Text.Length
                && characters.All(rect => !rect.IsEmpty && rect.Width > 0 && rect.Left >= -.5 && rect.Top >= -.5
                    && rect.Right <= block.ActualWidth + .5 && rect.Bottom <= block.ActualHeight + .5);
            var requiredHeight = characters.Max(rect => rect.Bottom);
            var top = card.TranslatePoint(new(), viewport).Y;
            var fits = origin.X >= inner.Left - .5 && origin.Y >= inner.Top - .5
                && origin.X + block.ActualWidth <= inner.Right + .5 && origin.Y + block.ActualHeight <= inner.Bottom + .5
                && allGlyphsInside && (block.TextWrapping != TextWrapping.NoWrap || glyphWidth <= inner.Width + .5);
            return new PaddingGeometry(block.Name, card.ActualWidth, inner.Width, glyphWidth, block.ActualWidth,
                block.ActualHeight, requiredHeight, characters.Length, allGlyphsInside, top, card.ActualHeight, viewport.ActualHeight, fits,
                top >= -.5 && top + card.ActualHeight <= viewport.ActualHeight + .5);
        }).ToArray();
    private static async Task ShowPaddingAsync(SettingsWindow window)
    {
        var value = Descendants(window).OfType<TextBlock>().Single(block => block.Name == "MonitorPaddedFramesValue");
        var card = Ancestor<Border>(value);
        var scroll = (ScrollViewer)window.FindName("PageScroll");
        var content = (FrameworkElement)window.FindName("PageContainer");
        scroll.ScrollToVerticalOffset(Math.Max(0, card.TranslatePoint(new(), content).Y - 8));
        await Pump(); window.UpdateLayout(); await Pump();
    }
    private static T Ancestor<T>(DependencyObject element) where T : DependencyObject
    {
        for (var parent = VisualTreeHelper.GetParent(element); parent is not null; parent = VisualTreeHelper.GetParent(parent))
            if (parent is T result) return result;
        throw new InvalidOperationException($"Missing visual ancestor {typeof(T).Name}.");
    }
    private static async Task MetricsAsync(AppViewModel app, Factory factory, long marker, params (string Key, object Value)[] optional)
    {
        var received = Snapshot(app.Session, item => item.Metrics?.Underruns == marker);
        var metrics = new Dictionary<string, object>
        {
            ["underrun_packets"] = marker, ["dropped_frames"] = 4L, ["input_rate"] = 48000,
            ["capture_to_send_p95_ms"] = 3.5, ["max_queue_age_ms"] = 4.5,
            ["last_audio_qpc_ns"] = (double)Stopwatch.GetTimestamp() * 1_000_000_000 / Stopwatch.Frequency
        };
        foreach (var (key, value) in optional) metrics[key] = value;
        factory.Latest.Emit(new { @event = "capture_metrics", metrics }); await received;
    }
    private static async Task Snapshot(SessionController session, Func<SessionSnapshot, bool> predicate)
    {
        var signal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void Changed(SessionSnapshot snapshot) { if (predicate(snapshot)) signal.TrySetResult(); }
        session.Changed += Changed;
        try { Changed(session.Snapshot); await signal.Task.WaitAsync(TimeSpan.FromSeconds(10)); }
        finally { session.Changed -= Changed; }
    }
    private static async Task Pump() => await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);
    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); ++i)
        {
            var child = VisualTreeHelper.GetChild(root, i); yield return child;
            foreach (var nested in Descendants(child)) yield return nested;
        }
    }
    private static void Check(bool condition, string name, List<string> checks)
    { if (!condition) throw new InvalidOperationException("Diagnostics: " + name); checks.Add(name); }
    private static void Render(Window window, string path)
    {
        window.UpdateLayout();
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(window.ActualWidth), (int)Math.Ceiling(window.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window); var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(bitmap));
        using var output = File.Create(path); png.Save(output);
    }
    private sealed class Writer
    {
        public const string Failure = "synthetic clipboard failure";
        public TaskCompletionSource<string>? Next; public string Last = ""; public bool Fail, OnDispatcher;
        public ApartmentState Apartment; public int Calls;
        public void Write(string text)
        {
            Calls++; Last = text; OnDispatcher = Application.Current.Dispatcher.CheckAccess(); Apartment = Thread.CurrentThread.GetApartmentState();
            Next?.TrySetResult(text); if (Fail) throw new IOException(Failure);
        }
    }
    private sealed class Store : ISettingsStore
    {
        private AppSettings _settings = new() { AutoConnectOnDiscover = false, ForceReconnect = false, MuteWhileStreaming = false,
            ManualReceivers = [new("synthetic-manual", "Synthetic receiver", "192.0.2.30", 7000)] };
        public int Saves;
        public AppSettings Load() => _settings.Clone();
        public void Save(AppSettings settings) { Saves++; _settings = settings.Clone(); }
    }
    private sealed class Audio : IAudioService
    {
        public int Mutes, Restores;
        public event Action? EndpointsChanged { add { } remove { } }
        public Task<IReadOnlyList<AudioEndpoint>> GetEndpointsAsync() => Task.FromResult<IReadOnlyList<AudioEndpoint>>([new("synthetic-render", "Synthetic output")]);
        public Task<string?> GetDefaultEndpointIdAsync() => Task.FromResult<string?>("synthetic-render");
        public Task MuteAsync(string? endpointId) { Mutes++; return Task.CompletedTask; }
        public Task RestoreAsync() { Restores++; return Task.CompletedTask; }
    }
    private sealed class Factory : IEngineFactory
    {
        public readonly ConcurrentQueue<string> Commands = new();
        private Connection? _latest;
        public Connection Latest => Volatile.Read(ref _latest) ?? throw new InvalidOperationException("Synthetic engine has not started.");
        public IEngineConnection Open() { var connection = new Connection(this); Volatile.Write(ref _latest, connection); return connection; }
        public sealed class Connection(Factory factory) : IEngineConnection
        {
            private readonly Channel<JsonElement> _events = Channel.CreateUnbounded<JsonElement>(); private string _session = "";
            public Task SendAsync(string session, string command, object? parameters, CancellationToken cancellation)
            {
                factory.Commands.Enqueue(command);
                if (command == "start") { _session = session; Emit(new { @event = "streaming" }); }
                if (command == "stop") _events.Writer.TryComplete();
                return Task.CompletedTask;
            }
            public void Emit(object payload)
            {
                var item = JsonSerializer.SerializeToNode(payload)!.AsObject(); item["version"] = 1; item["session_id"] = _session;
                _events.Writer.TryWrite(JsonSerializer.SerializeToElement(item));
            }
            public async ValueTask<JsonElement?> ReadAsync(CancellationToken cancellation)
            { try { return await _events.Reader.ReadAsync(cancellation); } catch (ChannelClosedException) { return null; } }
            public ValueTask DisposeAsync() { _events.Writer.TryComplete(); return ValueTask.CompletedTask; }
        }
    }
}
