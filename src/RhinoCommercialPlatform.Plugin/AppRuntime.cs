using System;
using System.Linq;
using System.Reflection;
using RhinoCommercialPlatform.Core.Abstractions;
using RhinoCommercialPlatform.Core.Runtime;
using RhinoCommercialPlatform.Infrastructure;
using RhinoCommercialPlatform.Modules.Abstractions;
using RhinoCommercialPlatform.Modules.Foundation;

namespace RhinoCommercialPlatform.Plugin;

public sealed class AppRuntime : IDisposable
{
    public PluginMetadata Metadata { get; }
    public IAppPaths Paths { get; }
    public IAppLogger Logger { get; }
    public ModuleRegistry Modules { get; }

    private bool _disposed;

    private AppRuntime(
        PluginMetadata metadata,
        IAppPaths paths,
        IAppLogger logger,
        ModuleRegistry modules)
    {
        Metadata = metadata;
        Paths = paths;
        Logger = logger;
        Modules = modules;
    }

    public static AppRuntime Start()
    {
        IAppPaths? paths = null;
        IAppLogger? logger = null;
        ModuleRegistry? modules = null;

        try
        {
            var runtimeMode = GetRuntimeMode();
            var version = GetVersion();
            var metadata = new PluginMetadata("RhinoCommercialPlatform", version, runtimeMode);

            paths = new AppPaths();
            paths.EnsureCreated();

            var clock = new SystemClock();
            logger = new FileAppLogger(paths, clock);

            modules = new ModuleRegistry();
            modules.Register(new FoundationModule());

            var context = new ModuleContext(logger, paths);
            modules.InitializeAll(context);

            logger.Information("RhinoCommercialPlatform runtime started.");

            return new AppRuntime(metadata, paths, logger, modules);
        }
        catch
        {
            if (modules != null)
            {
                try { modules.ShutdownAll(); } catch { /* Shutdown errors during startup rollback */ }
            }
            if (logger is IDisposable d)
            {
                try { d.Dispose(); } catch { /* Dispose errors during startup rollback */ }
            }
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        try
        {
            Modules.ShutdownAll();
            Logger.Information("RhinoCommercialPlatform runtime stopped.");
        }
        catch
        {
            // Log but don't suppress
            try { Logger.Information("RhinoCommercialPlatform runtime stopped with module shutdown errors."); }
            catch { /* Last resort */ }
        }

        if (Logger is IDisposable d)
        {
            try { d.Dispose(); } catch { /* Ignore dispose errors */ }
        }
    }

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
            // Fall through
        }

        try
        {
            var v = typeof(AppRuntime).Assembly.GetName().Version;
            if (v != null)
                return v.ToString();
        }
        catch
        {
            // Fall through
        }

        return "0.1.0";
    }
}
