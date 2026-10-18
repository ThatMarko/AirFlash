using AirFlash.App.ViewModels;
using System.Runtime.ExceptionServices;

namespace AirFlash.App.Services;

// Shared by the shell's Quit action and isolated dispatcher regression fixtures.
internal static class ShellShutdown
{
    internal static async Task RunAsync(AppViewModel? viewModel, Func<Task> waitForApply,
        Action closeWindows, Func<ValueTask> disposeEngine)
    {
        // Close admission before the first await, while accepted saves can still settle.
        viewModel?.BeginShutdown();
        List<Exception> errors = [];
        try { await waitForApply(); } catch (Exception error) { errors.Add(error); }
        try { closeWindows(); } catch (Exception error) { errors.Add(error); }
        try { await disposeEngine(); } catch (Exception error) { errors.Add(error); }
        try { if (viewModel is not null) await viewModel.DisposeAsync(); }
        catch (Exception error) { errors.Add(error); }
        if (errors.Count == 1) ExceptionDispatchInfo.Capture(errors[0]).Throw();
        if (errors.Count > 1) throw new AggregateException(errors);
    }
}
