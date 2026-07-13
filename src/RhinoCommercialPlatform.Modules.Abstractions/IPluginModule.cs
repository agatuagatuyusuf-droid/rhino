using System;

namespace RhinoCommercialPlatform.Modules.Abstractions;

public interface IPluginModule
{
    string Id { get; }
    string DisplayName { get; }
    Version Version { get; }

    void Initialize(IModuleContext context);
    void Shutdown();
}
