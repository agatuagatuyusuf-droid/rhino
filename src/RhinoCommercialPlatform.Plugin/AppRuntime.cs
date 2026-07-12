using System;
using System.Collections.Generic;
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
    public IPlatformInfo Platform { get; }

    private bool _disposed;

    private AppRuntime(
        PluginMetadata metadata,
        IAppPaths paths,
        IAppLogger logger,
        ModuleRegistry modules,
        IPlatformInfo platform)
    {
        Metadata = metadata;
        Paths = paths;
        Logger = logger;
        Modules = modules;
        Platform = platform ?? throw new ArgumentNullException(nameof(platform));
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

            logger.Information("RhinoCommercialPlatform runtime started.");

            return new AppRuntime(metadata, paths, logger, modules, platform);
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

        return "0.1.0";
    }
}
