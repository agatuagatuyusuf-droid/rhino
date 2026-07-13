using RhinoCommercialPlatform.Platform.Abstractions;

namespace RhinoCommercialPlatform.Platform.Windows;

public sealed class WindowsSystemThemeProvider : ISystemThemeProvider
{
    public SystemTheme GetCurrentSystemTheme()
    {
        // On Windows, we can check the registry theme setting.
        // However, to avoid P/Invoke and Registry access from shared code,
        // we default to Light on Windows since Rhino itself manages theme.
        // Users can explicitly set Light or Dark in settings.
        return SystemTheme.Light;
    }
}
