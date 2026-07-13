using System;
using RhinoCommercialPlatform.Core.Abstractions;
using RhinoCommercialPlatform.Core.Runtime;
using RhinoCommercialPlatform.Modules.Abstractions;

namespace RhinoCommercialPlatform.UI.ViewModels;

public sealed class RuntimeStatusViewModel
{
    private readonly PluginMetadata _metadata;
    private readonly IPlatformInfo _platform;
    private readonly ModuleRegistry _modules;
    private readonly IAppPaths _paths;
    private readonly IAppLogger _logger;
    private readonly DateTime _startedAtUtc;

    // Optional Rhino document info
    private readonly string? _documentName;
    private readonly string? _documentPath;
    private readonly int? _documentObjectCount;
    private readonly string? _rhinoVersion;
    private readonly string? _lastError;

    public RuntimeStatusViewModel(
        PluginMetadata metadata,
        IPlatformInfo platform,
        ModuleRegistry modules,
        IAppPaths paths,
        IAppLogger logger,
        DateTime startedAtUtc,
        string? documentName = null,
        string? documentPath = null,
        int? documentObjectCount = null,
        string? rhinoVersion = null,
        string? lastError = null)
    {
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _platform = platform ?? throw new ArgumentNullException(nameof(platform));
        _modules = modules ?? throw new ArgumentNullException(nameof(modules));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _startedAtUtc = startedAtUtc;
        _documentName = documentName;
        _documentPath = documentPath;
        _documentObjectCount = documentObjectCount;
        _rhinoVersion = rhinoVersion;
        _lastError = lastError;
    }

    public bool IsRuntimeStarted => true;
    public string RuntimeStartedAt => _startedAtUtc.ToString("yyyy-MM-dd HH:mm:ss UTC");
    public string RuntimeUptime
    {
        get
        {
            var uptime = DateTime.UtcNow - _startedAtUtc;
            if (uptime.TotalDays >= 1)
                return $"{(int)uptime.TotalDays}d {uptime.Hours}h {uptime.Minutes}m";
            if (uptime.TotalHours >= 1)
                return $"{(int)uptime.TotalHours}h {uptime.Minutes}m";
            return $"{(int)uptime.TotalMinutes}m {uptime.Seconds}s";
        }
    }

    public int RegisteredModuleCount => _modules.Modules.Count;
    public int InitializedModuleCount => _modules.InitializedModules.Count;
    public string LogsDirectory => _paths.LogsDirectory;
    public string ConfigDirectory => _paths.ConfigDirectory;
    public string PluginVersion => _metadata.Version;

    // Rhino doc info (nullable)
    public bool HasDocument => !string.IsNullOrEmpty(_documentName);
    public string DocumentName => _documentName ?? "[无打开文档]";
    public string DocumentPath => _documentPath ?? "[无打开文档]";
    public string DocumentObjectCount => _documentObjectCount?.ToString() ?? "[无打开文档]";
    public string RhinoVersion => _rhinoVersion ?? "[未知]";
    public string? LastError => _lastError;
}
