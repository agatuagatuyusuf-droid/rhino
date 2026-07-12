using System;
using Rhino;
using Rhino.PlugIns;

namespace RhinoCommercialPlatform.Plugin;

public sealed class RhinoCommercialPlatformPlugin : PlugIn
{
    public static RhinoCommercialPlatformPlugin? Instance
    {
        get;
        private set;
    }

    internal AppRuntime? Runtime
    {
        get;
        private set;
    }

    public RhinoCommercialPlatformPlugin()
    {
        Instance = this;
    }

    protected override LoadReturnCode OnLoad(
        ref string errorMessage)
    {
        if (Runtime != null)
        {
            errorMessage = "Runtime is already initialized.";
            return LoadReturnCode.ErrorShowDialog;
        }

        try
        {
            Runtime = AppRuntime.Start();

            RhinoApp.WriteLine(
                $"RhinoCommercialPlatform " +
                $"{Runtime.Metadata.Version} loaded.");

            return LoadReturnCode.Success;
        }
        catch (Exception startupException)
        {
            var details =
                $"Failed to start RhinoCommercialPlatform runtime: " +
                $"{startupException.GetType().Name}: " +
                $"{startupException.Message}";

            if (Runtime != null)
            {
                try
                {
                    Runtime.Dispose();
                }
                catch (Exception cleanupException)
                {
                    details +=
                        $" Cleanup also failed: " +
                        $"{cleanupException.GetType().Name}: " +
                        $"{cleanupException.Message}";

                    RhinoApp.WriteLine(details);
                }
                finally
                {
                    Runtime = null;
                }
            }

            errorMessage = details;
            return LoadReturnCode.ErrorShowDialog;
        }
    }

    protected override void OnShutdown()
    {
        try
        {
            Runtime?.Dispose();
        }
        catch (Exception shutdownException)
        {
            RhinoApp.WriteLine(
                $"RhinoCommercialPlatform shutdown failed: " +
                $"{shutdownException.GetType().Name}: " +
                $"{shutdownException.Message}");
        }
        finally
        {
            Runtime = null;
            Instance = null;
        }
    }
}
