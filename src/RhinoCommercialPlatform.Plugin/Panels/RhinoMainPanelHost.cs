using System;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
using Rhino;
using RhinoCommercialPlatform.UI.Shell;

namespace RhinoCommercialPlatform.Plugin.Panels;

/// <summary>
/// Real Rhino-managed panel host.
/// Registered via Rhino.UI.Panels.RegisterPanel and created by Rhino on demand.
/// Must have a public parameterless constructor and a GuidAttribute.
/// </summary>
[Guid("7B3E4F2A-1D8C-4E5F-9A6B-3C2D1E0F8A7B")]
public sealed class RhinoMainPanelHost : Panel
{
    public RhinoMainPanelHost()
    {
        try
        {
            var plugin = RhinoCommercialPlatformPlugin.Instance;
            if (plugin?.Runtime == null)
            {
                ShowRuntimeMissingError();
                return;
            }

            var panelService = MainPanelService.GetInstance(plugin.Runtime);
            var mainView = panelService.CreatePanelView();

            if (mainView == null)
            {
                ShowRuntimeMissingError();
                return;
            }

            // MainPanelView is an Eto.Forms.Panel that implements IPanelView
            if (mainView is Control control)
            {
                Content = control;
            }
            else
            {
                ShowRuntimeMissingError();
            }
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"RhinoMainPanelHost constructor error: {ex.Message}");
            ShowRuntimeMissingError();
        }
    }

    private void ShowRuntimeMissingError()
    {
        Content = new Label
        {
            Text = "RhinoCommercialPlatform runtime is not available.\nPlease restart Rhino or reload the plugin.",
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb(0xCC, 0x33, 0x33),
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };
    }
}
