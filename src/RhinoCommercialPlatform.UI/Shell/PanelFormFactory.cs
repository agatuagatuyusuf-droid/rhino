using System;
using Eto.Forms;

namespace RhinoCommercialPlatform.UI.Shell;

/// <summary>
/// Manages the Eto Form lifecycle. All Eto-specific code is contained here.
/// </summary>
public static class PanelFormFactory
{
    private static Form? _panelForm;
    private static Action? _onShow;
    private static Action? _onHide;

    /// <summary>
    /// Creates the form with the given panel view and shows it.
    /// </summary>
    public static void CreateAndShowForm(IPanelView panelView, string title,
        Action? onShow = null, Action? onHide = null)
    {
        if (_panelForm != null)
            return;

        _onShow = onShow;
        _onHide = onHide;

        // IPanelView is implemented by MainPanelView which extends Panel (Eto.Forms.Panel)
        var contentControl = (Control)panelView;

        _panelForm = new Form
        {
            Title = title,
            Content = contentControl,
            Size = new Eto.Drawing.Size(460, 600),
            MinimumSize = new Eto.Drawing.Size(360, 300),
            Resizable = true,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.Default,
            Padding = Eto.Drawing.Padding.Empty,
            BackgroundColor = Eto.Drawing.Colors.Transparent
        };

        _panelForm.Closing += (sender, args) =>
        {
            _panelForm!.Visible = false;
            args.Cancel = true;
            _onHide?.Invoke();
        };

        _panelForm.Shown += (_, _) =>
        {
            _onShow?.Invoke();
        };
    }

    public static void ShowForm()
    {
        if (_panelForm != null)
        {
            _panelForm.Show();
            _onShow?.Invoke();
        }
    }

    public static void FocusForm()
    {
        if (_panelForm != null)
        {
            _panelForm.Focus();
        }
    }

    public static void CloseForm()
    {
        if (_panelForm != null)
        {
            _panelForm.Close();
            _panelForm.Dispose();
            _panelForm = null;
        }

        _onShow = null;
        _onHide = null;
    }
}
