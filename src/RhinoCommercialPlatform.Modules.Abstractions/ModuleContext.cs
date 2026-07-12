using System;
using RhinoCommercialPlatform.Core.Abstractions;

namespace RhinoCommercialPlatform.Modules.Abstractions;

public class ModuleContext : IModuleContext
{
    public IAppLogger Logger { get; }
    public IAppPaths Paths { get; }

    public ModuleContext(IAppLogger logger, IAppPaths paths)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Paths = paths ?? throw new ArgumentNullException(nameof(paths));
    }
}
