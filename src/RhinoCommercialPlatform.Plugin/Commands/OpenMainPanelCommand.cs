using System;
using Rhino;
using Rhino.Commands;
using RhinoCommercialPlatform.Plugin.Panels;

namespace RhinoCommercialPlatform.Plugin.Commands;

/// <summary>
/// Command to open the main RhinoCommercialPlatform panel.
/// Usage: RCP_OpenPanel
/// </summary>
public sealed class OpenMainPanelCommand : Command
{
    public override string EnglishName => "RCP_OpenPanel";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        var plugin = RhinoCommercialPlatformPlugin.Instance;
        if (plugin?.Runtime == null)
        {
            RhinoApp.WriteLine("RhinoCommercialPlatform runtime is not available.");
            return Result.Failure;
        }

        try
        {
            var success = MainPanelRegistration.OpenPanel();

            if (success)
            {
                var logger = plugin.Runtime.Logger;
                logger.Information("Main panel opened via RCP_OpenPanel command.");
                return Result.Success;
            }

            RhinoApp.WriteLine("Failed to open RhinoCommercialPlatform panel.");
            return Result.Failure;
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Error opening panel: {ex.Message}");
            plugin.Runtime.Logger.Error($"RCP_OpenPanel failed: {ex.Message}");

            return Result.Failure;
        }
    }
}
