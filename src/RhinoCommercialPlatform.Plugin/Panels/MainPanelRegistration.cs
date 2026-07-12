using System;
using Rhino;

namespace RhinoCommercialPlatform.Plugin.Panels;

/// <summary>
/// Manages the main panel lifetime using Rhino.UI.Panels API.
/// </summary>
public static class MainPanelRegistration
{
    private static readonly object _lock = new object();
    private static bool _registered;

    public static readonly Guid PanelId = new Guid("7B3E4F2A-1D8C-4E5F-9A6B-3C2D1E0F8A7B");
    public const string PanelName = "RhinoCommercialPlatform";

    /// <summary>
    /// Registers the panel with Rhino's panel system.
    /// Safe to call multiple times — subsequent calls are no-ops.
    /// </summary>
    public static void Register()
    {
        lock (_lock)
        {
            if (_registered)
            {
                RhinoApp.WriteLine($"Panel '{PanelName}' already registered.");
                return;
            }

            var plugin = RhinoCommercialPlatformPlugin.Instance;
            if (plugin == null)
            {
                RhinoApp.WriteLine($"Cannot register panel '{PanelName}': plugin instance is null.");
                return;
            }

            try
            {
                global::Rhino.UI.Panels.RegisterPanel(plugin, typeof(RhinoMainPanelHost), PanelName, null);
                _registered = true;
                RhinoApp.WriteLine($"Panel '{PanelName}' registered successfully.");
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Failed to register panel '{PanelName}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Opens or focuses the main panel using Rhino's panel system.
    /// Safe to call multiple times — Rhino handles single-instance.
    /// </summary>
    public static bool OpenPanel()
    {
        try
        {
            global::Rhino.UI.Panels.OpenPanel(typeof(RhinoMainPanelHost));
            RhinoApp.WriteLine($"Panel '{PanelName}' opened.");
            return true;
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Failed to open panel '{PanelName}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Closes the main panel using Rhino's panel system.
    /// </summary>
    public static void ClosePanel()
    {
        try
        {
            global::Rhino.UI.Panels.ClosePanel(PanelId);
            RhinoApp.WriteLine($"Panel '{PanelName}' closed.");
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Failed to close panel '{PanelName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Whether the panel is currently visible in Rhino's panel system.
    /// </summary>
    public static bool IsPanelVisible()
    {
        try
        {
            return global::Rhino.UI.Panels.IsPanelVisible(typeof(RhinoMainPanelHost));
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Failed to check panel visibility '{PanelName}': {ex.Message}");
            return false;
        }
    }
}
