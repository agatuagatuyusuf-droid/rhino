using RhinoCommercialPlatform.Core.Abstractions;

namespace RhinoCommercialPlatform.Modules.Abstractions;

public interface IModuleContext
{
    IAppLogger Logger { get; }
    IAppPaths Paths { get; }
}
