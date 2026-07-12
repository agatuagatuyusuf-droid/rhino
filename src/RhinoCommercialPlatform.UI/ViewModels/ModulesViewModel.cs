using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RhinoCommercialPlatform.Modules.Abstractions;

namespace RhinoCommercialPlatform.UI.ViewModels;

public sealed class ModulesViewModel
{
    private readonly ModuleRegistry _registry;

    public ModulesViewModel(ModuleRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public ObservableCollection<ModuleDescriptor> Descriptors
    {
        get
        {
            var descriptors = new ObservableCollection<ModuleDescriptor>();
            var initializedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var initModule in _registry.InitializedModules)
            {
                initializedIds.Add(initModule.Id);
            }

            foreach (var module in _registry.Modules)
            {
                var isInitialized = initializedIds.Contains(module.Id);
                descriptors.Add(new ModuleDescriptor(
                    module.Id,
                    module.DisplayName,
                    module.Version.ToString(),
                    GetModuleDescription(module),
                    GetModuleCategory(module),
                    isInitialized,
                    isInitialized ? "已加载" : "未初始化"
                ));
            }

            return descriptors;
        }
    }

    private static string GetModuleDescription(IPluginModule module)
    {
        return module switch
        {
            _ => $"{module.DisplayName} 模块 - 提供基础功能支持"
        };
    }

    private static string GetModuleCategory(IPluginModule module)
    {
        return module switch
        {
            _ => "基础模块"
        };
    }
}

public sealed class ModuleDescriptor
{
    public string Id { get; }
    public string DisplayName { get; }
    public string Version { get; }
    public string Description { get; }
    public string Category { get; }
    public bool IsInitialized { get; }
    public string StatusText { get; }

    public ModuleDescriptor(
        string id,
        string displayName,
        string version,
        string description,
        string category,
        bool isInitialized,
        string statusText)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Category = category ?? throw new ArgumentNullException(nameof(category));
        IsInitialized = isInitialized;
        StatusText = statusText ?? throw new ArgumentNullException(nameof(statusText));
    }
}
