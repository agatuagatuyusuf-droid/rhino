using System;
using Rhino;
using Rhino.PlugIns;
using RhinoCommercialPlatform.Platform.Abstractions;

namespace RhinoCommercialPlatform.Plugin.Panels;

public sealed class RhinoPanelGateway : IRhinoPanelGateway
{
    public bool RegisterPanel(object pluginInstance, Type panelType, string panelName, object? icon)
    {
        if (pluginInstance == null)
            throw new ArgumentNullException(nameof(pluginInstance));
        if (panelType == null)
            throw new ArgumentNullException(nameof(panelType));
        if (string.IsNullOrWhiteSpace(panelName))
            throw new ArgumentException("Panel name must not be empty.", nameof(panelName));

        var plugin = pluginInstance as PlugIn;
        if (plugin == null)
        {
            RhinoApp.WriteLine($"Cannot register panel '{panelName}': pluginInstance is not a valid PlugIn.");
            return false;
        }

        try
        {
            var rhinoIcon = icon as System.Drawing.Icon;
            global::Rhino.UI.Panels.RegisterPanel(plugin, panelType, panelName, rhinoIcon);
            return true;
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Failed to register panel '{panelName}': {ex.Message}");
            return false;
        }
    }

    public bool OpenPanel(Type panelHostType, bool makeSelectedPanel)
    {
        if (panelHostType == null)
            throw new ArgumentNullException(nameof(panelHostType));

        try
        {
            global::Rhino.UI.Panels.OpenPanel(panelHostType, makeSelectedPanel);
            if (global::Rhino.Runtime.HostUtils.RunningOnOSX)
                global::Rhino.UI.Panels.FloatPanel(panelHostType.GUID, global::Rhino.UI.Panels.FloatPanelMode.Show);
            return true;
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Failed to open panel '{panelHostType.Name}': {ex.Message}");
            return false;
        }
    }

    public void ClosePanel(Guid panelId)
    {
        try
        {
            global::Rhino.UI.Panels.ClosePanel(panelId);
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Failed to close panel ({panelId}): {ex.Message}");
        }
    }

    public bool IsPanelVisible(Type panelHostType, bool isSelectedTab)
    {
        if (panelHostType == null)
            throw new ArgumentNullException(nameof(panelHostType));

        try
        {
            return global::Rhino.UI.Panels.IsPanelVisible(panelHostType, isSelectedTab);
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Failed to check panel visibility '{panelHostType.Name}': {ex.Message}");
            return false;
        }
    }

    public object? GetPanel(Guid panelId)
    {
        try
        {
#pragma warning disable CS0618 // GetPanel(Guid) is obsolete but is the only viable option without a RhinoDoc reference
            return global::Rhino.UI.Panels.GetPanel(panelId);
#pragma warning restore CS0618
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"Failed to get panel ({panelId}): {ex.Message}");
            return null;
        }
    }
}
