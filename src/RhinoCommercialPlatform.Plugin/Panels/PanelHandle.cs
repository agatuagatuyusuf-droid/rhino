using System;
using RhinoCommercialPlatform.UI.Shell;

namespace RhinoCommercialPlatform.Plugin.Panels;

/// <summary>
/// A handle for the Eto panel form that avoids direct Eto type dependencies.
/// All Eto interactions go through the UI project.
/// </summary>
public sealed class PanelHandle
{
    private PanelHandle()
    {
    }

    public bool IsVisible { get; private set; }

    /// <summary>
    /// Creates a new panel handle from an IPanelView.
    /// </summary>
    public static PanelHandle Create(IPanelView view, string title)
    {
        var handle = new PanelHandle();

        PanelFormFactory.CreateAndShowForm(view, title,
            onShow: () => handle.IsVisible = true,
            onHide: () => handle.IsVisible = false);

        return handle;
    }

    public void Focus()
    {
        PanelFormFactory.FocusForm();
    }

    public void Show()
    {
        PanelFormFactory.ShowForm();
        IsVisible = true;
    }

    public void Close()
    {
        PanelFormFactory.CloseForm();
        IsVisible = false;
    }
}
