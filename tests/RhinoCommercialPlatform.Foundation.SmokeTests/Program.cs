using System;
using System.IO;
using RhinoCommercialPlatform.Core.Abstractions;
using RhinoCommercialPlatform.Core.Runtime;
using RhinoCommercialPlatform.Infrastructure;
using RhinoCommercialPlatform.Modules.Abstractions;
using RhinoCommercialPlatform.Modules.Foundation;

namespace RhinoCommercialPlatform.Foundation.SmokeTests;

class Program
{
    static int Main(string[] args)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "RCP_Smoke_" + Guid.NewGuid().ToString("N"));
        IAppLogger? logger = null;
        ModuleRegistry? modules = null;

        try
        {
            Directory.CreateDirectory(tempDir);

            var paths = new AppPaths(tempDir);
            paths.EnsureCreated();

            var clock = new SystemClock();
            logger = new FileAppLogger(paths, clock);

            modules = new ModuleRegistry();
            modules.Register(new FoundationModule());

            var context = new ModuleContext(logger, paths);
            modules.InitializeAll(context);

            Assert(modules.Modules.Count == 1, $"Expected 1 module, got {modules.Modules.Count}");
            Assert(modules.InitializedModules.Count == 1, $"Expected 1 initialized module, got {modules.InitializedModules.Count}");

            logger.Information("Smoke test log with token=secret-value");

            modules.ShutdownAll();
            modules = null;

            ((FileAppLogger)logger).Dispose();
            logger = null;

            var logFile = Path.Combine(paths.LogsDirectory, $"plugin-{DateTimeOffset.UtcNow:yyyyMMdd}.log");
            Assert(File.Exists(logFile), $"Log file not found: {logFile}");

            var logContent = File.ReadAllText(logFile);

            Assert(logContent.Contains("Foundation module initialized."),
                "Missing 'Foundation module initialized.' in log");
            Assert(logContent.Contains("Foundation module shutdown."),
                "Missing 'Foundation module shutdown.' in log");
            Assert(logContent.Contains("[REDACTED]"),
                "Missing '[REDACTED]' in log - secrets were not redacted");
            Assert(!logContent.Contains("secret-value"),
                "Found 'secret-value' in log - secrets were not redacted");

            // Platform info validation
            var platformInfo = new RuntimePlatformInfo();
            Assert(!string.IsNullOrWhiteSpace(platformInfo.OperatingSystem),
                "OperatingSystem should not be empty");
            Assert(!string.IsNullOrWhiteSpace(platformInfo.OperatingSystemDescription),
                "OperatingSystemDescription should not be empty");
            Assert(!string.IsNullOrWhiteSpace(platformInfo.ProcessArchitecture),
                "ProcessArchitecture should not be empty");
            Assert(!string.IsNullOrWhiteSpace(platformInfo.FrameworkDescription),
                "FrameworkDescription should not be empty");

            Console.WriteLine($"Platform: {platformInfo.OperatingSystem}");

            Console.WriteLine($"FOUNDATION_SMOKE_PASS platform={platformInfo.OperatingSystem}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FOUNDATION_SMOKE_FAIL: {ex.GetType().Name}: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
        finally
        {
            if (logger is IDisposable d)
                try { d.Dispose(); } catch { }
            if (modules != null)
                try { modules.ShutdownAll(); } catch { }

            if (Directory.Exists(tempDir))
                try { Directory.Delete(tempDir, recursive: true); } catch { }
        }
    }

    static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException($"Smoke assertion failed: {message}");
    }
}
