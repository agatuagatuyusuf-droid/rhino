using System;
using RhinoCommercialPlatform.UI.Theme;

namespace RhinoCommercialPlatform.UI.Shell;

/// <summary>
/// Interface for the main panel view, avoiding direct Eto type exposure.
/// </summary>
public interface IPanelView
{
    event EventHandler<NavigationPageId>? NavigationRequested;
    event EventHandler? ThemeToggled;
    event EventHandler? SettingsRequested;
    event EventHandler? OpenLogDirectoryRequested;
    event EventHandler? CopyDiagnosticsRequested;
    event EventHandler? OpenConfigDirectoryRequested;

    void ShowPage(NavigationPageId pageId);
    void ApplyTheme(ThemePalette palette);
}
