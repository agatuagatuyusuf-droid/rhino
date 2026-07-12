using System;

namespace RhinoCommercialPlatform.Plugin.Panels;

/// <summary>
/// Panel host placeholder. The actual panel is managed as an Eto Form
/// by MainPanelRegistration. This class exists for API compatibility.
/// </summary>
[Obsolete("Use MainPanelRegistration to open/close the panel.")]
public sealed class MainPanelHost
{
    private MainPanelHost()
    {
    }
}
