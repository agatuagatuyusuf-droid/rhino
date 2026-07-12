using System;
using Rhino;
using Rhino.PlugIns;

namespace RhinoCommercialPlatform.Plugin;

public sealed class RhinoCommercialPlatformPlugin : PlugIn
{
    public static RhinoCommercialPlatformPlugin? Instance { get; private set; }
    internal AppRuntime? Runtime { get; private set; }

    public RhinoCommercialPlatformPlugin()
    {
        Instance = this;
    }

    protected override LoadReturnCode OnLoad(ref string errorMessage)
    {
        if (Runtime != null)
        {
            errorMessage = "Runtime is already initialized.";
            return LoadReturnCode.ErrorShowDialog;
        }

        try
        {
            Runtime = AppRuntime.Start();
            RhinoApp.WriteLine($"RhinoCommercialPlatform {Runtime.Metadata.Version} loaded.");
            return LoadReturnCode.Success;
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to start RhinoCommercialPlatform runtime: {ex.GetType().Name}: {ex.Message}";

            if (Runtime != null)
            {
                try { Runtime.Dispose(); } catch { /* Ignore */ }
                Runtime = null;
            }

            return LoadReturnCode.ErrorShowDialog;
        }
    }

    protected override void OnShutdown()
    {
        Runtime?.Dispose();
        Runtime = null;
        Instance = null;
    }
}
