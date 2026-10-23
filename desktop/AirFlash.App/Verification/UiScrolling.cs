using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using AirFlash.App.Services;
using AirFlash.App.Ui;
using AirFlash.App.ViewModels;
using AirFlash.Core;
namespace AirFlash.App.Verification;

// Uses only in-memory settings and simulated discovery/audio/engine services.
internal static class UiScrolling
{
    public static async Task RunAsync(List<string> checks, string directory, bool interactive = false)
    {
        Directory.CreateDirectory(directory);
        await VerifyPanelAsync(checks, directory);
        var failures = new List<string>();
        var measurements = new List<object>();
        var store = new UiSmoke.MemoryStore();
        var engine = new UiSmoke.MockFactory();
        var discovery = new UiSmoke.MockDiscovery
        {
            Items = Enumerable.Range(1, 6).Select(i => new Receiver($"scroll-{i}", $"Scroll receiver {i}", $"127.0.0.{i}") { Model = "HomePod mini" }).ToArray()
        };
        await using var app = new AppViewModel(store, discovery, new UiSmoke.MockAutostart(), new UiSmoke.MockAudio(), engine, Application.Current.Dispatcher);
        // Finish receiver initialization before measuring scrolling-only settings changes.
        app.SetPanelVisible(true);
        app.Start(); await app.Startup; await Pump();
        app.SetPanelVisible(false);
        var window = new SettingsWindow(app); window.Show(); await Pump();
        try
        {
            // Keep an edited curve so accidental slider changes and persistence are observable.
            window.ViewModel.Equalizer.Enabled = true;
            window.ViewModel.Equalizer.Bands[0].Gain = 7.5;
            await Pump();
            var draftBefore = Fingerprint(window.ViewModel.Draft);
            var savedBefore = Fingerprint(store.Saved);
            var savesBefore = store.Saves;
            var startsBefore = engine.Commands.Count(c => c.Text("command") == "start");

            foreach (var dark in new[] { false, true })
            {
                ThemeService.SetForVerification(dark); await Pump();
                foreach (var (width, height, size) in new[] { (880d, 640d, "default"), (640d, 440d, "minimum") })
                {
                    window.Width = width; window.Height = height; await Pump();
                    for (var pageIndex = 0; pageIndex < window.ViewModel.Pages.Length; pageIndex++)
                    {
                        window.ViewModel.SelectedPage = pageIndex;
                        window.PageScroll.ScrollToTop(); await Pump();
                        var scroll = window.PageScroll;
                        var page = window.PageContainer.Children.OfType<FrameworkElement>().Single(p => p.Visibility == Visibility.Visible);
                        var label = $"scroll {(dark ? "dark" : "light")} {size} page {pageIndex}";
                        var overflowing = scroll.ScrollableHeight > 1;
                        var vertical = scroll.Template.FindName("PART_VerticalScrollBar", scroll) as ScrollBar;
                        var contentBounds = Bounds(page, scroll);
                        double? gutter = null;
                        if (overflowing && vertical is { IsVisible: true })
                        {
                            gutter = Bounds(vertical, scroll).Left - contentBounds.Right;
                            Observe(gutter >= 8 - .5, $"{label}: content clears vertical scrollbar by at least 8 DIP (actual {gutter:F2})", checks, failures);
                        }
                        Observe(overflowing ? vertical is { IsVisible: true, ActualWidth: > 0 } : vertical is { IsVisible: false }, $"{label}: scrollbar visibility matches page overflow", checks, failures);
                        if (pageIndex is SettingsViewModel.EqualizerPage or SettingsViewModel.MonitorPage)
                            UiSmoke.Render(window, Path.Combine(directory, $"settings-scroll-{(dark ? "dark" : "light")}-{size}-page-{pageIndex}-top.png"), 1);

                        var referenceWheelOffset = 0d;
                        if (overflowing)
                        {
                            Wheel(page, -120); await Pump();
                            referenceWheelOffset = scroll.VerticalOffset;
                            Observe(scroll.VerticalOffset > 0, $"{label}: ordinary wheel moves page content downward", checks, failures);
                            scroll.ScrollToBottom(); await Pump();
                            var bottom = Bounds(page, scroll).Bottom;
                            Observe(Math.Abs(scroll.VerticalOffset - scroll.ScrollableHeight) <= 1, $"{label}: final scroll position reaches content end", checks, failures);
                            Observe(bottom <= scroll.ActualHeight - scroll.Padding.Bottom + 1 && bottom > 0, $"{label}: final content is inside the viewport", checks, failures);
                            if (pageIndex == SettingsViewModel.MonitorPage)
                                UiSmoke.Render(window, Path.Combine(directory, $"settings-scroll-{(dark ? "dark" : "light")}-{size}-monitor-bottom.png"), 1);
                            Wheel(page, 120); await Pump();
                            Observe(scroll.VerticalOffset < scroll.ScrollableHeight, $"{label}: ordinary wheel returns from page bottom", checks, failures);
                        }
                        else
                        {
                            Wheel(page, -120); await Pump();
                            Observe(scroll.VerticalOffset == 0 && contentBounds.Bottom <= scroll.ActualHeight - scroll.Padding.Bottom + 1, $"{label}: fitting content stays fully accessible", checks, failures);
                        }

                        measurements.Add(new
                        {
                            theme = dark ? "dark" : "light", size, page = pageIndex,
                            window_width = window.ActualWidth, window_height = window.ActualHeight,
                            extent_height = scroll.ExtentHeight, viewport_height = scroll.ViewportHeight,
                            scrollable_height = scroll.ScrollableHeight, gutter_dip = gutter
                        });

                        if (pageIndex == SettingsViewModel.EqualizerPage)
                        {
                            var bands = UiSmoke.Descendants(page).OfType<ScrollViewer>().Single(s => s.Name == "EqualizerBandScroll");
                            bands.ScrollToHorizontalOffset(Math.Min(72, bands.ScrollableWidth)); await Pump();
                            var horizontalBeforeWheel = bands.HorizontalOffset;
                            var curveBeforeWheel = JsonSerializer.Serialize(window.ViewModel.Draft.Equalizer);
                            var slider = UiSmoke.Descendants(bands).OfType<Slider>().First(s => IsHorizontallyVisible(s, bands));
                            var bandLabel = UiSmoke.Descendants(bands).OfType<TextBlock>().First(t => IsHorizontallyVisible(t, bands));
                            foreach (var (source, name) in new (UIElement, string)[] { (bandLabel, "band label"), (slider, "band slider") })
                            {
                                scroll.ScrollToTop(); await Pump();
                                Wheel(source, -120); await Pump();
                                Observe(referenceWheelOffset > 0 && Math.Abs(scroll.VerticalOffset - referenceWheelOffset) <= 1, $"{label}: wheel over {name} scrolls surrounding page exactly once", checks, failures);
                                // Establish a positive offset independently of the downward assertion.
                                scroll.ScrollToVerticalOffset(referenceWheelOffset); await Pump();
                                var beforeUpwardWheel = scroll.VerticalOffset;
                                Wheel(source, 120); await Pump();
                                Observe(beforeUpwardWheel > 0 && scroll.VerticalOffset <= 1, $"{label}: upward wheel over {name} returns surrounding page to top", checks, failures);
                                Observe(Math.Abs(bands.HorizontalOffset - horizontalBeforeWheel) <= 1 && JsonSerializer.Serialize(window.ViewModel.Draft.Equalizer) == curveBeforeWheel, $"{label}: wheel over {name} preserves band offset and gains", checks, failures);
                            }
                            if (size == "minimum")
                            {
                                Observe(bands.ScrollableWidth > 0, $"{label}: frequency bands expose horizontal overflow", checks, failures);
                                bands.ScrollToRightEnd(); await Pump();
                                var lastBand = UiSmoke.Descendants(bands).OfType<Slider>().Last();
                                var lastBounds = Bounds(lastBand, bands);
                                Observe(bands.HorizontalOffset > 0 && lastBounds.Left >= 0 && lastBounds.Right <= bands.ActualWidth + 1, $"{label}: final frequency slider is reachable horizontally", checks, failures);
                                var horizontalBar = bands.Template.FindName("PART_HorizontalScrollBar", bands) as ScrollBar;
                                var bandContent = (ItemsControl)bands.Content;
                                // Scrollbar arrow glyphs are also TextBlocks; select the real frequency label.
                                var lastLabel = UiSmoke.Descendants(bandContent).OfType<TextBlock>().Single(t => t.Text == window.ViewModel.Equalizer.Bands.Last().Label);
                                var horizontalGutter = horizontalBar is { IsVisible: true } ? Bounds(horizontalBar, bands).Top - Bounds(lastLabel, bands).Bottom : double.NaN;
                                Observe(horizontalGutter >= 8 - .5, $"{label}: frequency labels clear horizontal scrollbar by at least 8 DIP (actual {horizontalGutter:F2})", checks, failures);
                                measurements.Add(new
                                {
                                    theme = dark ? "dark" : "light", size, page = pageIndex, kind = "frequency labels",
                                    horizontal_gutter_dip = double.IsFinite(horizontalGutter) ? horizontalGutter : (double?)null,
                                    padding = bands.Padding.ToString(), content_margin = bandContent.Margin.ToString(),
                                    viewer_height = bands.ActualHeight, content_bounds = Coordinates(Bounds(bandContent, bands)),
                                    label_text = lastLabel.Text, label_bounds = Coordinates(Bounds(lastLabel, bands)),
                                    scrollbar_bounds = horizontalBar is null ? null : Coordinates(Bounds(horizontalBar, bands))
                                });
                                scroll.ScrollToBottom(); await Pump();
                                UiSmoke.Render(window, Path.Combine(directory, $"settings-scroll-{(dark ? "dark" : "light")}-{size}-equalizer-bottom.png"), 1);
                                bands.ScrollToLeftEnd(); await Pump();
                            }
                        }
                    }
                }
            }

            if (interactive)
            {
                ThemeService.SetForVerification(false);
                window.Width = 880; window.Height = 640;
                window.ViewModel.SelectedPage = SettingsViewModel.EqualizerPage;
                window.PageScroll.ScrollToTop(); await Pump();
                await Task.Delay(TimeSpan.FromSeconds(60));
            }

            Observe(Fingerprint(window.ViewModel.Draft) == draftBefore, "scrolling preserves the edited equalizer curve and settings draft", checks, failures);
            Observe(Fingerprint(store.Saved) == savedBefore && store.Saves == savesBefore, "scrolling never saves settings", checks, failures);
            Observe(engine.Commands.Count(c => c.Text("command") == "start") == startsBefore && app.Snapshot.State == PlaybackState.Idle, "scrolling never starts receiver playback", checks, failures);

            var report = new
            {
                ok = failures.Count == 0, failures, measurements,
                note = "Mock-only scroll layout and routed wheel verification; interactive mode temporarily exposes the same isolated Settings window. No real receiver or settings store is used."
            };
            File.WriteAllText(Path.Combine(directory, "ui-scrolling-report.json"), JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            if (failures.Count > 0) throw new InvalidOperationException($"Settings scrolling regression: {failures.Count} failed checks; first: {failures[0]}. See ui-scrolling-report.json.");
        }
        finally { window.Close(); }
    }

    private static async Task VerifyPanelAsync(List<string> checks, string directory)
    {
        var failures = new List<string>();
        var measurements = new List<object>();
        var store = new UiSmoke.MemoryStore();
        var engine = new UiSmoke.MockFactory();
        var audio = new UiSmoke.MockAudio();
        var receivers = Enumerable.Range(1, 12).Select(i => new Receiver($"panel-scroll-{i}", $"Panel scroll receiver {i}", $"127.0.0.{i}")
            { Model = "HomePod mini" }).ToArray();
        var discovery = new UiSmoke.MockDiscovery { Items = receivers };
        await using var app = new AppViewModel(store, discovery, new UiSmoke.MockAutostart(), audio, engine, Application.Current.Dispatcher);
        var panel = new ControlPanel(app) { SizeToContent = SizeToContent.Manual, Height = 600 };
        try
        {
            app.Start(); await app.Startup;
            panel.ShowPanel(); await Pump();
            var savesBefore = store.Saves;
            var commandsBefore = engine.Commands.Count;
            var mutesBefore = audio.Mutes; var restoresBefore = audio.Restores;
            var savedBefore = Fingerprint(store.Saved);
            foreach (var dark in new[] { false, true })
            {
                ThemeService.SetForVerification(dark); await Pump();
                foreach (var dpi in new[] { 96, 120 })
                {
                    await UseDpiAsync(panel, dpi);
                    foreach (var width in new[] { 440d, 520d })
                    {
                        panel.Width = width;
                        panel.Height = Math.Min(600, panel.MaxHeight);
                        await ReceiversAsync(app, discovery, receivers);
                        var scroll = panel.DeviceScroll;
                        scroll.ScrollToTop(); await Pump();
                        var label = $"receiver scroll {(dark ? "dark" : "light")} {width:F0} DIP {dpi} DPI";
                        var vertical = scroll.Template.FindName("PART_VerticalScrollBar", scroll) as ScrollBar
                            ?? throw new InvalidOperationException("The receiver vertical scrollbar is unavailable.");
                        var viewport = scroll.Template.FindName("PART_ScrollContentPresenter", scroll) as ScrollContentPresenter
                            ?? throw new InvalidOperationException("The receiver scroll viewport is unavailable.");
                        var cards = ReceiverCards(scroll, app).ToArray();
                        var barBounds = Bounds(vertical, scroll);
                        var gutters = cards.Select(card => barBounds.Left - Bounds(card.Card, scroll).Right).ToArray();
                        var controlsFit = cards.All(card =>
                        {
                            var controls = UiSmoke.Descendants(card.Card).OfType<FrameworkElement>()
                                .Where(element => element is Button or Slider).ToArray();
                            return controls.OfType<Button>().Any(button => ReferenceEquals(button.Command, card.Row.ToggleCommand))
                                && controls.OfType<Button>().Any(button => ReferenceEquals(button.Command, card.Row.MuteCommand))
                                && controls.OfType<Slider>().Any() && controls.All(control =>
                            {
                                var bounds = Bounds(control, scroll);
                                var cardBounds = Bounds(card.Card, scroll);
                                return control.IsVisible && control.ActualWidth > 0 && control.ActualHeight > 0
                                    && bounds.Left >= cardBounds.Left - .5 && bounds.Right <= cardBounds.Right + .5
                                    && bounds.Right <= barBounds.Left - 8 + .5;
                            });
                        });
                        Observe(cards.Length == receivers.Length && scroll.ScrollableHeight > 1
                            && vertical.IsVisible && vertical.ActualWidth > 0, $"{label}: all receiver cards render with a visible overflow scrollbar", checks, failures);
                        Observe(gutters.All(gap => gap >= 8 - .5), $"{label}: every receiver card clears the actual scrollbar by at least 8 DIP (minimum {gutters.Min():F2})", checks, failures);
                        Observe(controlsFit, $"{label}: playback, mute and volume controls stay inside their cards and clear the scrollbar", checks, failures);
                        Observe(scroll.ScrollableWidth == 0, $"{label}: receiver controls need no horizontal scrolling", checks, failures);
                        UiSmoke.Render(panel, Path.Combine(directory, $"receiver-scroll-{(dark ? "dark" : "light")}-{width:F0}-{dpi}-top.png"), dpi / 96d);

                        Wheel(cards[0].Card, -120); await Pump();
                        Observe(scroll.VerticalOffset > 0, $"{label}: ordinary wheel over a card scrolls receivers downward", checks, failures);
                        scroll.ScrollToBottom(); await Pump();
                        var last = cards[^1]; var lastBounds = Bounds(last.Card, viewport);
                        var play = UiSmoke.Descendants(last.Card).OfType<Button>().Single(button => ReferenceEquals(button.Command, last.Row.ToggleCommand));
                        panel.Activate();
                        Observe(Math.Abs(scroll.VerticalOffset - scroll.ScrollableHeight) <= 1,
                            $"{label}: final scroll position reaches the last receiver", checks, failures);
                        Observe(lastBounds.Top >= -.5 && lastBounds.Bottom <= viewport.ActualHeight + .5,
                            $"{label}: the last receiver card is fully inside the viewport", checks, failures);
                        Observe(play.IsVisible && play.IsEnabled && play.Command!.CanExecute(play.CommandParameter)
                            && play.Focus() && play.IsKeyboardFocused,
                            $"{label}: the final receiver Play control remains accessible by keyboard", checks, failures);
                        UiSmoke.Render(panel, Path.Combine(directory, $"receiver-scroll-{(dark ? "dark" : "light")}-{width:F0}-{dpi}-bottom.png"), dpi / 96d);
                        measurements.Add(new
                        {
                            theme = dark ? "dark" : "light", requested_width = width, requested_dpi = dpi,
                            actual_width = panel.ActualWidth, actual_height = panel.ActualHeight,
                            actual_dpi_x = VisualTreeHelper.GetDpi(panel).DpiScaleX,
                            actual_dpi_y = VisualTreeHelper.GetDpi(panel).DpiScaleY,
                            scrollbar_bounds = Coordinates(barBounds), minimum_card_gutter_dip = gutters.Min(),
                            controls_fit = controlsFit, viewport_height = viewport.ActualHeight,
                            scrollable_height = scroll.ScrollableHeight, final_offset = scroll.VerticalOffset,
                            last_card_bounds = Coordinates(lastBounds)
                        });

                        await ReceiversAsync(app, discovery, [receivers[^1]]);
                        scroll.ScrollToTop(); await Pump();
                        var fitting = ReceiverCards(scroll, app).Single();
                        var fittingBounds = Bounds(fitting.Card, viewport);
                        Observe(scroll.ScrollableHeight <= 1 && !vertical.IsVisible,
                            $"{label}: a fitting receiver list hides its vertical scrollbar", checks, failures);
                        Observe(fittingBounds.Left >= -.5 && fittingBounds.Top >= -.5
                            && fittingBounds.Right <= viewport.ActualWidth + .5 && fittingBounds.Bottom <= viewport.ActualHeight + .5,
                            $"{label}: a fitting receiver card stays fully accessible", checks, failures);
                        Wheel(fitting.Card, -120); await Pump();
                        Observe(scroll.VerticalOffset == 0, $"{label}: wheel leaves a fitting receiver list at its accessible origin", checks, failures);
                    }
                }
            }
            Observe(store.Saves == savesBefore && Fingerprint(store.Saved) == savedBefore,
                "receiver scrolling, resizing and synthetic DPI changes never save settings", checks, failures);
            Observe(engine.Commands.Count == commandsBefore && audio.Mutes == mutesBefore && audio.Restores == restoresBefore
                && app.Snapshot.State == PlaybackState.Idle,
                "receiver scrolling never starts playback or mutates local audio", checks, failures);
            File.WriteAllText(Path.Combine(directory, "ui-panel-scrolling-report.json"), JsonSerializer.Serialize(new
            {
                ok = failures.Count == 0, failures, measurements,
                note = "Actual mock ControlPanel geometry, scroll input and keyboard access at 440/520 DIP and synthetic 96/120 DPI. Synthetic window DPI does not replace physical display testing."
            }, new JsonSerializerOptions { WriteIndented = true }));
            if (failures.Count > 0) throw new InvalidOperationException($"Receiver scrolling regression: {failures.Count} failed checks; first: {failures[0]}. See ui-panel-scrolling-report.json.");
        }
        finally { panel.ShutdownPanel(); }
    }

    private static IEnumerable<(Border Card, ReceiverViewModel Row)> ReceiverCards(ScrollViewer scroll, AppViewModel app)
    {
        var items = UiSmoke.Descendants(scroll).OfType<ItemsControl>().Single(item => ReferenceEquals(item.ItemsSource, app.Receivers));
        foreach (var row in app.Receivers)
        {
            var container = items.ItemContainerGenerator.ContainerFromItem(row) as FrameworkElement
                ?? throw new InvalidOperationException("A receiver item container is unavailable.");
            yield return (UiSmoke.Descendants(container).OfType<Border>().First(), row);
        }
    }

    private static async Task ReceiversAsync(AppViewModel app, UiSmoke.MockDiscovery discovery, Receiver[] receivers)
    {
        var published = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        void Changed(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        {
            if (app.Receivers.Select(row => row.Receiver.Id).ToHashSet(StringComparer.Ordinal)
                .SetEquals(receivers.Select(receiver => receiver.Id))) published.TrySetResult();
        }
        app.Receivers.CollectionChanged += Changed;
        try
        {
            discovery.Items = receivers; discovery.Publish();
            Changed(null, new(System.Collections.Specialized.NotifyCollectionChangedAction.Reset));
            await published.Task.WaitAsync(TimeSpan.FromSeconds(10)); await Pump();
        }
        finally { app.Receivers.CollectionChanged -= Changed; }
    }

    // Notify only the isolated fake-services window, preserving its logical size.
    // This neither changes Windows display preferences nor messages another app.
    private static async Task UseDpiAsync(Window window, int dpi)
    {
        var handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;
        if (handle == IntPtr.Zero || !GetWindowRect(handle, out var bounds))
            throw new InvalidOperationException("The synthetic receiver panel is unavailable.");
        var previous = VisualTreeHelper.GetDpi(window);
        bounds.Right = bounds.Left + (int)Math.Round((bounds.Right - bounds.Left) * dpi / 96d / previous.DpiScaleX);
        bounds.Bottom = bounds.Top + (int)Math.Round((bounds.Bottom - bounds.Top) * dpi / 96d / previous.DpiScaleY);
        var memory = Marshal.AllocHGlobal(Marshal.SizeOf<NativeRect>());
        try
        {
            Marshal.StructureToPtr(bounds, memory, false);
            SendMessage(handle, 0x02E0 /* WM_DPICHANGED */, new IntPtr((dpi << 16) | dpi), memory);
        }
        finally { Marshal.FreeHGlobal(memory); }
        await window.Dispatcher.InvokeAsync(window.UpdateLayout, DispatcherPriority.Render); await Pump();
        var observed = VisualTreeHelper.GetDpi(window);
        if (observed.DpiScaleX != dpi / 96d || observed.DpiScaleY != dpi / 96d)
            throw new InvalidOperationException($"Synthetic {dpi} DPI was not adopted: {observed.DpiScaleX}, {observed.DpiScaleY}.");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll", EntryPoint = "SendMessageW", ExactSpelling = true)]
    private static extern IntPtr SendMessage(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr handle, out NativeRect rectangle);

    private static Rect Bounds(FrameworkElement element, Visual ancestor)
        => element.TransformToAncestor(ancestor).TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));

    private static object Coordinates(Rect rect) => new { left = rect.Left, top = rect.Top, right = rect.Right, bottom = rect.Bottom };

    private static bool IsHorizontallyVisible(FrameworkElement element, ScrollViewer scroll)
    {
        var bounds = Bounds(element, scroll);
        return bounds.Left >= 0 && bounds.Right <= scroll.ActualWidth;
    }

    private static void Wheel(UIElement source, int delta)
    {
        // Preview and bubbling input share handled state, as real WPF mouse input does.
        var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, delta) { RoutedEvent = Mouse.PreviewMouseWheelEvent };
        source.RaiseEvent(args);
        args.RoutedEvent = Mouse.MouseWheelEvent;
        source.RaiseEvent(args);
    }

    private static string Fingerprint(AppSettings settings) => JsonSerializer.Serialize(settings);
    private static void Observe(bool value, string name, List<string> checks, List<string> failures)
    {
        if (value) checks.Add(name); else failures.Add(name);
    }
    private static async Task Pump()
    {
        await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
        await Task.Delay(40);
    }
}
