using System;
using System.Collections.Generic;
using RhinoCommercialPlatform.Core.Abstractions;
using RhinoCommercialPlatform.Core.Runtime;
using RhinoCommercialPlatform.Modules.Abstractions;
using RhinoCommercialPlatform.UI.Shell;

namespace RhinoCommercialPlatform.UI.ViewModels;

public sealed class DashboardViewModel
{
    private readonly PluginMetadata _metadata;
    private readonly IPlatformInfo _platform;
    private readonly ModuleRegistry _modules;
    private readonly IAppPaths _paths;
    private readonly IAppLogger _logger;
    private readonly DateTime _runtimeStartedAt;
    private readonly string? _lastRuntimeError;

    public DashboardViewModel(
        PluginMetadata metadata,
        IPlatformInfo platform,
        ModuleRegistry modules,
        IAppPaths paths,
        IAppLogger logger,
        DateTime runtimeStartedAt,
        string? lastRuntimeError = null)
    {
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _platform = platform ?? throw new ArgumentNullException(nameof(platform));
        _modules = modules ?? throw new ArgumentNullException(nameof(modules));
        _paths = paths ?? throw new ArgumentNullException(nameof(paths));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _runtimeStartedAt = runtimeStartedAt;
        _lastRuntimeError = lastRuntimeError;
    }

    public string ProductName => _metadata.ProductName;
    public string PluginVersion => _metadata.Version;
    public string RunMode => _metadata.Mode.ToString();
    public string OperatingSystem => _platform.OperatingSystem;
    public string OperatingSystemDescription => _platform.OperatingSystemDescription;
    public string ProcessArchitecture => _platform.ProcessArchitecture;
    public string FrameworkDescription => _platform.FrameworkDescription;
    public int RegisteredModuleCount => _modules.Modules.Count;
    public int InitializedModuleCount => _modules.InitializedModules.Count;
    public string LogsDirectory => _paths.LogsDirectory;
    public string ConfigDirectory => _paths.ConfigDirectory;
    public string PluginStatus => "正常运行";
    public string CurrentTime => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    public string? LastRuntimeError => _lastRuntimeError;

    public List<QuickEntry> QuickEntries => new List<QuickEntry>
    {
        new QuickEntry("功能模块", "查看和管理功能模块", NavigationPageId.Modules),
        new QuickEntry("诊断中心", "查看系统诊断信息", NavigationPageId.Diagnostics),
        new QuickEntry("设置", "配置插件行为", NavigationPageId.Settings),
        new QuickEntry("打开日志目录", "查看日志文件夹", null)
    };
}

public sealed class QuickEntry
{
    public string Title { get; }
    public string Description { get; }
    public NavigationPageId? TargetPage { get; }

    public QuickEntry(string title, string description, NavigationPageId? targetPage)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        TargetPage = targetPage;
    }
}
