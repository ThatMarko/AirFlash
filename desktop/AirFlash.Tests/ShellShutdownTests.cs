using AirFlash.App.Services;
using Xunit;
namespace AirFlash.Tests;

public sealed class ShellShutdownTests
{
    [Fact]
    public async Task AdmissionClosesSynchronouslyBeforeAcceptedSaveAndCleanupSettle()
    {
        List<string> calls = [];
        var save = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var cleanup = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var operation = ShellShutdown.RunAsync(() => calls.Add("close admission"),
            () => { calls.Add("save"); return save.Task; }, () => calls.Add("windows"),
            async () => { calls.Add("engine"); entered.SetResult(); await cleanup.Task; },
            () => { calls.Add("view model"); return ValueTask.CompletedTask; });
        try
        {
            Assert.Equal(new[] { "close admission", "save" }, calls);
            Assert.False(operation.IsCompleted);
            save.SetResult(); await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(new[] { "close admission", "save", "windows", "engine" }, calls);
            Assert.False(operation.IsCompleted);
        }
        finally { save.TrySetResult(); cleanup.TrySetResult(); await operation; }
        Assert.Equal("view model", calls[^1]);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public async Task AdmissionNotificationFailureStillAttemptsEveryCleanup(bool engineAlsoFails)
    {
        List<string> calls = [];
        var admissionError = new IOException("synthetic admission notification failure");
        var engineError = new IOException("synthetic engine cleanup failure");
        var operation = ShellShutdown.RunAsync(() => { calls.Add("admission"); throw admissionError; },
            () => { calls.Add("save"); return Task.CompletedTask; }, () => calls.Add("windows"),
            () => { calls.Add("engine"); if (engineAlsoFails) throw engineError; return ValueTask.CompletedTask; },
            () => { calls.Add("view model"); return ValueTask.CompletedTask; });
        var error = await Record.ExceptionAsync(() => operation);
        Assert.Equal(new[] { "admission", "save", "windows", "engine", "view model" }, calls);
        if (engineAlsoFails)
            Assert.Equal(new[] { admissionError, engineError }, Assert.IsType<AggregateException>(error).InnerExceptions);
        else Assert.Same(admissionError, error);
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)] [InlineData(15)]
    public async Task EachCleanupRunsAfterEarlierFailuresAndOriginalErrorsArePreserved(int failures)
    {
        List<int> calls = [];
        var errors = Enumerable.Range(0, 4).Select(i => new IOException($"synthetic stage {i}")).ToArray();
        void Step(int i) { calls.Add(i); if (failures == 15 || failures == i) throw errors[i]; }
        var operation = ShellShutdown.RunAsync(() => calls.Add(-1),
            () => { Step(0); return Task.CompletedTask; }, () => Step(1),
            () => { Step(2); return ValueTask.CompletedTask; },
            () => { Step(3); return ValueTask.CompletedTask; });
        var error = await Record.ExceptionAsync(() => operation);
        Assert.Equal(new[] { -1, 0, 1, 2, 3 }, calls);
        if (failures == 15) Assert.Equal(errors, Assert.IsType<AggregateException>(error).InnerExceptions);
        else Assert.Same(errors[failures], error);
    }
}
