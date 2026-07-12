using System;
using Rhino;

namespace RhinoCommercialPlatform.Plugin.Panels;

/// <summary>
/// Manages the main panel lifetime.
/// Uses a factory-based approach to avoid direct Eto dependency in this project.
/// </summary>
public static class MainPanelRegistration
{
    private static readonly object _lock = new object();
    private static PanelHandle? _panelHandle;

    public static readonly Guid PanelId = new Guid("7B3E4F2A-1D8C-4E5F-9A6B-3C2D1E0F8A7B");
    public const string PanelName = "RhinoCommercialPlatform";

    /// <summary>
    /// Registers the panel.
    /// </summary>
    public static void Register()
    {
        RhinoApp.WriteLine($"Panel '{PanelName}' registration prepared.");
    }

    /// <summary>
    /// Opens or focuses the main panel.
    /// </summary>
    public static bool OpenPanel()
    {
        lock (_lock)
        {
            try
            {
                if (_panelHandle != null && _panelHandle.IsVisible)
                {
                    _panelHandle.Focus();
                    RhinoApp.WriteLine($"Panel '{PanelName}' focused.");
                    return true;
                }

                if (_panelHandle != null)
                {
                    _panelHandle.Show();
                    RhinoApp.WriteLine($"Panel '{PanelName}' re-shown.");
                    return true;
                }

                return CreateAndShowPanel();
            }
            catch (Exception ex)
            {
                RhinoApp.WriteLine($"Failed to open panel: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Closes the panel.
    /// </summary>
    public static void ClosePanel()
    {
        lock (_lock)
        {
            _panelHandle?.Close();
            _panelHandle = null;
            RhinoApp.WriteLine($"Panel '{PanelName}' closed.");
        }
    }

    /// <summary>
    /// Whether the panel is visible.
    /// </summary>
    public static bool IsPanelVisible() => _panelHandle?.IsVisible ?? false;

    private static bool CreateAndShowPanel()
    {
        var plugin = RhinoCommercialPlatformPlugin.Instance;
        if (plugin?.Runtime == null)
        {
            RhinoApp.WriteLine("Runtime is not available.");
            return false;
        }

        var panelService = MainPanelService.GetInstance(plugin.Runtime);
        var mainView = panelService.GetOrCreatePanelView();

        if (mainView == null)
        {
            RhinoApp.WriteLine("Failed to create panel view.");
            return false;
        }

        _panelHandle = PanelHandle.Create(mainView, PanelName);
        _panelHandle.Show();

        plugin.Runtime.Logger.Information("Main panel shown.");
        RhinoApp.WriteLine($"Panel '{PanelName}' created and shown.");
        return true;
    }
}
