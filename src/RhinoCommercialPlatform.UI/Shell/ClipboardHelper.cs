using Eto.Forms;

namespace RhinoCommercialPlatform.UI.Shell;

/// <summary>
/// Helper for clipboard operations, avoiding direct Eto dependency in other projects.
/// </summary>
public static class ClipboardHelper
{
    public static void CopyText(string text)
    {
        Clipboard.Instance.Text = text;
    }
}
