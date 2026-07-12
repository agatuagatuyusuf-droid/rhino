using System;
using System.Collections.Generic;
using System.Reflection;
using RhinoCommercialPlatform.Core.Abstractions;
using RhinoCommercialPlatform.Core.Runtime;
using RhinoCommercialPlatform.Infrastructure;
using RhinoCommercialPlatform.Modules.Abstractions;
using RhinoCommercialPlatform.Modules.Foundation;
using RhinoCommercialPlatform.Platform.Abstractions;
using RhinoCommercialPlatform.UI.Settings;
using RhinoCommercialPlatform.UI.Diagnostics;
using RhinoCommercialPlatform.UI.Theme;

namespace RhinoCommercialPlatform.Plugin;

public sealed class AppRuntime : IDisposable
{
    public PluginMetadata Metadata { get; }
    public IAppPaths Paths { get; }
    public IAppLogger Logger { get; }
    public ModuleRegistry Modules { get; }
    public IPlatformInfo Platform { get; }

    // UI-related services
    public IUserSettingsService UserSettingsService { get; private set; }
    public DiagnosticReportService DiagnosticReportService { get; private set; }
    public ThemeManager ThemeManager { get; private set; }
    public DateTime StartedAtUtc { get; }
    public string? LastRuntimeError { get; private set; }
    public string? CurrentLogFile { get; private set; }

    // Platform services
    public IExternalLauncher? ExternalLauncher { get; private set; }
    public ISystemThemeProvider? SystemThemeProvider { get; private set; }

    private bool _disposed;

    private AppRuntime(
        PluginMetadata metadata,
        IAppPaths paths,
        IAppLogger logger,
        ModuleRegistry modules,
        IPlatformInfo platform,
        IUserSettingsService userSettingsService,
        DiagnosticReportService diagnosticReportService,
        ThemeManager themeManager,
        DateTime startedAtUtc,
        IExternalLauncher? externalLauncher,
        ISystemThemeProvider? systemThemeProvider)
    {
        Metadata = metadata;
        Paths = paths;
        Logger = logger;
        Modules = modules;
        Platform = platform ?? throw new ArgumentNullException(nameof(platform));
        UserSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
        DiagnosticReportService = diagnosticReportService ?? throw new ArgumentNullException(nameof(diagnosticReportService));
        ThemeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
        StartedAtUtc = startedAtUtc;
        ExternalLauncher = externalLauncher;
        SystemThemeProvider = systemThemeProvider;
    }

    public static AppRuntime Start(IServiceProvider? serviceProvider = null)
    {
        IAppPaths? paths = null;
        IAppLogger? logger = null;
        ModuleRegistry? modules = null;

        try
        {
            var startedAtUtc = DateTime.UtcNow;
            var runtimeMode = GetRuntimeMode();
            var version = GetVersion();
            var metadata = new PluginMetadata("RhinoCommercialPlatform", version, runtimeMode);

            paths = new AppPaths();
            paths.EnsureCreated();

            var clock = new SystemClock();
            var platform = new RuntimePlatformInfo();
            logger = new FileAppLogger(paths, clock);

            logger.Information(
                $"Platform: {platform.OperatingSystem}; " +
                $"OS: {platform.OperatingSystemDescription}; " +
                $"Architecture: {platform.ProcessArchitecture}; " +
                $"Framework: {platform.FrameworkDescription}");

            modules = new ModuleRegistry();
            modules.Register(new FoundationModule());

            var context = new ModuleContext(logger, paths);
            modules.InitializeAll(context);

            // Create UI services
            var userSettingsService = new UserSettingsService(paths, logger);
            var diagnosticReportService = new DiagnosticReportService(paths.LogsDirectory, paths.ConfigDirectory);
            var systemThemeProvider = CreateSystemThemeProvider();
            var themeManager = new ThemeManager(systemThemeProvider);
            var externalLauncher = CreateExternalLauncher(platform);

            logger.Information("RhinoCommercialPlatform runtime started.");

            return new AppRuntime(
                metadata,
                paths,
                logger,
                modules,
                platform,
                userSettingsService,
                diagnosticReportService,
                themeManager,
                startedAtUtc,
                externalLauncher,
                systemThemeProvider);
        }
        catch (Exception startupException)
        {
            var rollbackErrors = new List<Exception>();

            if (modules != null)
            {
                try
                {
                    modules.ShutdownAll();
                }
                catch (Exception rollbackException)
                {
                    rollbackErrors.Add(rollbackException);
                }
            }

            if (logger is IDisposable disposableLogger)
            {
                try
                {
                    disposableLogger.Dispose();
                }
                catch (Exception disposeException)
                {
                    rollbackErrors.Add(disposeException);
                }
            }

            if (rollbackErrors.Count > 0)
            {
                var allErrors = new List<Exception>
                {
                    startupException
                };

                allErrors.AddRange(rollbackErrors);

                throw new AggregateException(
                    "Runtime startup failed and rollback was not clean.",
                    allErrors);
            }

            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        var shutdownErrors = new List<Exception>();

        try
        {
            Modules.ShutdownAll();
        }
        catch (Exception shutdownException)
        {
            shutdownErrors.Add(shutdownException);

            try
            {
                Logger.Error(
                    shutdownException,
                    "One or more modules failed during runtime shutdown.");
            }
            catch (Exception loggingException)
            {
                shutdownErrors.Add(loggingException);
            }
        }

        try
        {
            Logger.Information(
                "RhinoCommercialPlatform runtime stopped.");
        }
        catch (Exception loggingException)
        {
            shutdownErrors.Add(loggingException);
        }

        if (Logger is IDisposable disposableLogger)
        {
            try
            {
                disposableLogger.Dispose();
            }
            catch (Exception disposeException)
            {
                shutdownErrors.Add(disposeException);
            }
        }

        _disposed = true;

        if (shutdownErrors.Count > 0)
        {
            throw new AggregateException(
                "RhinoCommercialPlatform runtime shutdown failed.",
                shutdownErrors);
        }
    }

    public TimeSpan Uptime => DateTime.UtcNow - StartedAtUtc;

    private static RuntimeMode GetRuntimeMode()
    {
#if DEBUG
        return RuntimeMode.Development;
#else
        return RuntimeMode.Production;
#endif
    }

    private static string GetVersion()
    {
        try
        {
            var attr = typeof(AppRuntime).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (attr != null && !string.IsNullOrWhiteSpace(attr.InformationalVersion))
                return attr.InformationalVersion;
        }
        catch
        {
            // Fallback for non-critical metadata read
        }

        try
        {
            var v = typeof(AppRuntime).Assembly.GetName().Version;
            if (v != null)
                return v.ToString();
        }
        catch
        {
            // Fallback for non-critical metadata read
        }

        return "0.2.0";
    }

    private static IExternalLauncher? CreateExternalLauncher(IPlatformInfo platform)
    {
        if (platform.IsWindows)
        {
            return new RhinoCommercialPlatform.Platform.Windows.WindowsExternalLauncher();
        }
        else if (platform.IsMacOS)
        {
            return new RhinoCommercialPlatform.Platform.Mac.MacExternalLauncher();
        }

        return null;
    }

    private static ISystemThemeProvider? CreateSystemThemeProvider()
    {
        // Use a Rhino-aware provider that reads the actual system theme.
        // The OS-level providers (WindowsSystemThemeProvider, MacSystemThemeProvider)
        // are kept for non-Rhino hosting scenarios but are not used here.
        return new PluginSystemThemeProvider();
    }
}
