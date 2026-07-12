using System;
using RhinoCommercialPlatform.Platform.Abstractions;

namespace RhinoCommercialPlatform.Plugin.Adapters;

/// <summary>
/// Factory for creating platform-specific service implementations.
/// </summary>
public static class PlatformServiceFactory
{
    public static IExternalLauncher CreateExternalLauncher()
    {
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows))
        {
            return new RhinoCommercialPlatform.Platform.Windows.WindowsExternalLauncher();
        }

        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.OSX))
        {
            return new RhinoCommercialPlatform.Platform.Mac.MacExternalLauncher();
        }

        throw new PlatformNotSupportedException("Unsupported operating system.");
    }

    public static ISystemThemeProvider CreateSystemThemeProvider()
    {
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.Windows))
        {
            return new RhinoCommercialPlatform.Platform.Windows.WindowsSystemThemeProvider();
        }

        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
            System.Runtime.InteropServices.OSPlatform.OSX))
        {
            return new RhinoCommercialPlatform.Platform.Mac.MacSystemThemeProvider();
        }

        throw new PlatformNotSupportedException("Unsupported operating system.");
    }
}
