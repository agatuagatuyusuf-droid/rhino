using System;
using Rhino;
using Rhino.Commands;

namespace RhinoCommercialPlatform.Plugin.Commands;

public sealed class PlatformStatusCommand : Command
{
    public override string EnglishName => "RCP_Status";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        var plugin = RhinoCommercialPlatformPlugin.Instance;
        if (plugin == null)
        {
            RhinoApp.WriteLine("RhinoCommercialPlatform runtime is not available.");
            return Result.Failure;
        }

        var runtime = plugin.Runtime;
        if (runtime == null)
        {
            RhinoApp.WriteLine("RhinoCommercialPlatform runtime is not available.");
            return Result.Failure;
        }

        RhinoApp.WriteLine($"Product: {runtime.Metadata.ProductName}");
        RhinoApp.WriteLine($"Version: {runtime.Metadata.Version}");
        RhinoApp.WriteLine($"Mode: {runtime.Metadata.Mode}");
        RhinoApp.WriteLine($"Logs: {runtime.Paths.LogsDirectory}");
        RhinoApp.WriteLine($"Operating System: {runtime.Platform.OperatingSystem}");
        RhinoApp.WriteLine($"OS Description: {runtime.Platform.OperatingSystemDescription}");
        RhinoApp.WriteLine($"Process Architecture: {runtime.Platform.ProcessArchitecture}");
        RhinoApp.WriteLine($"Framework: {runtime.Platform.FrameworkDescription}");
        RhinoApp.WriteLine("Modules:");

        foreach (var module in runtime.Modules.Modules)
        {
            RhinoApp.WriteLine($"  - {module.Id} ({module.DisplayName}) v{module.Version}");
        }

        return Result.Success;
    }
}
