using System;
using RhinoCommercialPlatform.Modules.Abstractions;
using RhinoCommercialPlatform.Core.Abstractions;

namespace RhinoCommercialPlatform.Modules.Foundation;

public class FoundationModule : IPluginModule
{
    public string Id => "foundation";
    public string DisplayName => "Foundation";
    public Version Version => new Version(0, 1, 0);

    private IModuleContext? _context;
    private bool _initialized;

    public void Initialize(IModuleContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        if (_initialized)
            throw new InvalidOperationException("Foundation module is already initialized.");

        _context = context;
        _initialized = true;

        _context.Logger.Information("Foundation module initialized.");
    }

    public void Shutdown()
    {
        if (!_initialized)
            return;

        _context!.Logger.Information("Foundation module shutdown.");
        _context = null;
        _initialized = false;
    }
}
