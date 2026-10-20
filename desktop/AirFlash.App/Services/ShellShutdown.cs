using System.Runtime.ExceptionServices;
namespace AirFlash.App.Services;

internal static class ShellShutdown
{
    internal static async Task RunAsync(Action beginShutdown, Func<Task> waitForApply,
        Action closeWindows, Func<ValueTask> disposeEngine, Func<ValueTask> disposeViewModel)
    {
        // This runs synchronously, before any accepted save or probe cleanup can yield.
        beginShutdown();
        List<Exception> errors = [];
        try { await waitForApply(); } catch (Exception error) { errors.Add(error); }
        try { closeWindows(); } catch (Exception error) { errors.Add(error); }
        try { await disposeEngine(); } catch (Exception error) { errors.Add(error); }
        try { await disposeViewModel(); } catch (Exception error) { errors.Add(error); }
        if (errors.Count == 1) ExceptionDispatchInfo.Capture(errors[0]).Throw();
        if (errors.Count > 1) throw new AggregateException(errors);
    }
}
