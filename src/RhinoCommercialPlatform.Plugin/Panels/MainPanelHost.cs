using System;

namespace RhinoCommercialPlatform.Plugin.Panels;

/// <summary>
/// This type exists solely for backward compatibility.
/// The real Rhino-managed panel is <see cref="RhinoMainPanelHost"/>.
/// </summary>
[Obsolete("Use RhinoMainPanelHost instead, which is registered via Rhino.UI.Panels.RegisterPanel.")]
public static class MainPanelHost
{
}
