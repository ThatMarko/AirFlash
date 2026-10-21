using System.Runtime.InteropServices;
using AirFlash.App.Services;
using AirFlash.Core;
namespace AirFlash.App;

internal static class Program
{
    internal static string? DiagnosticStartupDataDirectory { get; private set; }
    internal static bool IsDiagnostic(string[] args) => args.Any(a => a.EndsWith("-check", StringComparison.Ordinal)
        || a == "--discovery-perf") || StartupOptions(args).Any(IsUiVerificationOption);
    private static bool IsUiVerificationOption(string option) => option.StartsWith("--ui-", StringComparison.Ordinal)
        || option is "--tray-smoke" or "--receiver-settings-regression";
    private static bool IsOtherDiagnosticMode(string option) => option is
        "--discovery-perf" or "--update-check" or "--self-check" or "--stability-check" or "--compat-check" or
        "--discovery-check" or "--audio-check";
    // Values such as an output filename must not become verification switches.
    // Keep this list aligned with the existing valued startup/verification options.
    internal static IEnumerable<string> StartupOptions(string[] args)
    {
        for (var index = 0; index < args.Length; index++)
        {
            var option = args[index];
            yield return option;
            if (option is "--output" or "--self-check-output" or "--ui-language" or "--ui-culture" or "--tray-guid") index++;
        }
    }
    internal static bool NeedsDiagnosticIsolation(string[] args)
        => StartupOptions(args).Any(option => IsUiVerificationOption(option) || IsOtherDiagnosticMode(option));
    internal static void ValidateUiVerificationArguments(string[] args)
    {
        var options = StartupOptions(args).ToArray();
        if (!options.Any(IsUiVerificationOption)) return;
        if (options.Count(option => option is "--ui-smoke" or "--tray-smoke" or "--ui-perf") != 1)
            throw new ArgumentException("UI verification requires exactly one primary mode: --ui-smoke, --tray-smoke or --ui-perf.");
        // App dispatches these modes before UI verification; combining them could
        // otherwise run a real audio/discovery diagnostic instead of the fixture.
        if (options.Any(IsOtherDiagnosticMode))
            throw new ArgumentException("UI verification cannot be combined with another diagnostic mode.");
        if (options.Contains("--ui-perf") && options.Any(option => IsUiVerificationOption(option)
            && option is not ("--ui-perf" or "--ui-language" or "--ui-culture")))
            throw new ArgumentException("UI performance verification accepts only --ui-language and --ui-culture UI options.");
        if (options.Contains("--tray-smoke") && options.Any(option => IsUiVerificationOption(option)
            && option is not ("--tray-smoke" or "--ui-language" or "--ui-culture")))
            throw new ArgumentException("Tray verification accepts only --ui-language and --ui-culture UI options.");
    }
    [STAThread]
    public static int Main(string[] args)
    {
        // DNS children must not initialize WPF or touch application data. Keep
        // this dispatch before restart coordination, logging or App construction.
        if (args.Length > 0 && args[0] == "--dns-resolve-helper") return DnsResolveHelper.Run(args);
        if (args.Length > 0 && args[0] == "--dns-resolve-helper-check") return DnsResolveHelperCheck.Run(args);
        var diagnostic = NeedsDiagnosticIsolation(args);
        if (diagnostic)
        {
            // Establish a safe fallback before even diagnostic argument parsing can fail.
            AppPaths.DataDirectory = StartupOptions(args).Any(a => a is "--ui-smoke" or "--tray-smoke")
                ? Path.Combine(Path.GetTempPath(), "airplay-ui-smoke", "isolated-data")
                : Path.Combine(Path.GetTempPath(), "airflash-diagnostics", Guid.NewGuid().ToString("N"), "isolated-data");
            DiagnosticStartupDataDirectory = AppPaths.DataDirectory;
        }
        // Also works when the WPF resource dictionary or localization cannot initialize.
        AppDomain.CurrentDomain.UnhandledException += (_, e) => AppPaths.Log($"Fatal stage=unhandled {e.ExceptionObject}");
        TaskScheduler.UnobservedTaskException += (_, e) => AppPaths.Log($"Background task failed {e.Exception}");
        var stage = "arguments";
        try
        {
            // All diagnostics keep startup failures alongside their isolated output.
            if (diagnostic)
            {
                var index = Array.FindIndex(args, a => a is "--output" or "--self-check-output");
                if (index >= 0 && index + 1 < args.Length)
                    AppPaths.DataDirectory = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(args[index + 1]))!,
                        args.Contains("--compat-check") ? "compat-logs" : "isolated-data");
                DiagnosticStartupDataDirectory = AppPaths.DataDirectory;
            }
            ValidateUiVerificationArguments(args);
            RestartService.WaitForPreviousProcess(args);
            stage = "platform";
            AppPaths.Log($"Startup stage={stage} version={AppPaths.DisplayVersion} os={Environment.OSVersion.Version} architecture={RuntimeInformation.ProcessArchitecture}");
            if (!WindowsCapabilities.Current.Supported) throw new PlatformNotSupportedException(WindowsCompatibility.Requirement);
            stage = "wpf-resources";
            var app = new App();
            app.InitializeComponent();
            AppPaths.Log($"Startup stage={stage} ready");
            return app.Run();
        }
        catch (Exception error)
        {
            AppPaths.Log($"Startup stage={stage} failed {error}");
            try { App.WriteOutput(args, new { ok = false, stage, error = error.ToString(), hresult = $"0x{error.HResult:X8}" }); }
            catch (Exception reportError) { AppPaths.Log($"Diagnostic output failed: {reportError.Message}"); }
            if (!IsDiagnostic(args))
                MessageBox(IntPtr.Zero, $"{error.Message}\n\nLogs: {AppPaths.LogDirectory}", "AirFlash startup failed", 0x10);
            return 1;
        }
    }

    [DllImport("user32.dll", EntryPoint = "MessageBoxW", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr owner, string message, string caption, uint type);
}
