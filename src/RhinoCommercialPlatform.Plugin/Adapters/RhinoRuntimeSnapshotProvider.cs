using System;
using System.Collections.Generic;
using RhinoCommercialPlatform.Core.Runtime;

namespace RhinoCommercialPlatform.Plugin.Adapters;

/// <summary>
/// Provides runtime snapshot data by adapting Rhino API data to UI-friendly formats.
/// </summary>
public sealed class RhinoRuntimeSnapshotProvider
{
    private readonly AppRuntime _runtime;

    public RhinoRuntimeSnapshotProvider(AppRuntime runtime)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    /// <summary>
    /// Gets a dictionary of runtime health indicators.
    /// </summary>
    public Dictionary<string, string> GetHealthSnapshot()
    {
        return new Dictionary<string, string>
        {
            { "RuntimeStarted", _runtime.StartedAtUtc.ToString("o") },
            { "Uptime", _runtime.Uptime.ToString(@"d\.hh\:mm\:ss") },
            { "ModulesRegistered", _runtime.Modules.Modules.Count.ToString() },
            { "ModulesInitialized", _runtime.Modules.InitializedModules.Count.ToString() },
            { "Platform", _runtime.Platform.OperatingSystem },
            { "Mode", _runtime.Metadata.Mode.ToString() },
            { "Version", _runtime.Metadata.Version }
        };
    }

    /// <summary>
    /// Gets a value indicating whether the runtime is healthy.
    /// </summary>
    public bool IsHealthy()
    {
        try
        {
            return _runtime.Modules.InitializedModules.Count > 0
                   && _runtime.Modules.Modules.Count > 0
                   && !string.IsNullOrEmpty(_runtime.Platform.OperatingSystem);
        }
        catch
        {
            return false;
        }
    }
}
