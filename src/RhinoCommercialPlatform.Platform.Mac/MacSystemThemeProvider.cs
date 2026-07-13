using RhinoCommercialPlatform.Platform.Abstractions;

namespace RhinoCommercialPlatform.Platform.Mac;

public sealed class MacSystemThemeProvider : ISystemThemeProvider
{
    public SystemTheme GetCurrentSystemTheme()
    {
        // On macOS, reading the system appearance requires AppKit which is
        // not available in netstandard2.0. We default to Light and allow
        // explicit user override via settings.
        return SystemTheme.Light;
    }
}
