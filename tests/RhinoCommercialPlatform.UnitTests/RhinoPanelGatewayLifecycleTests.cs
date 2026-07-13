using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RhinoCommercialPlatform.UnitTests;

[TestClass]
public sealed class RhinoPanelGatewayLifecycleTests
{
    [TestMethod]
    public void MacPanelFloat_HidesBeforeShowingToRaiseTheExistingWindow()
    {
        var sourcePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/RhinoCommercialPlatform.Plugin/Panels/RhinoPanelGateway.cs"));
        var source = File.ReadAllText(sourcePath);
        var normalized = string.Join(" ", source.Split(
            new[] { ' ', '\r', '\n', '\t' },
            StringSplitOptions.RemoveEmptyEntries));

        var hide = normalized.IndexOf("FloatPanelMode.Hide", StringComparison.Ordinal);
        var show = normalized.IndexOf("FloatPanelMode.Show", StringComparison.Ordinal);

        Assert.IsTrue(hide >= 0, "macOS panel must first be hidden.");
        Assert.IsTrue(show > hide, "macOS panel must be shown after it is hidden.");
        Assert.IsFalse(normalized.Contains("AsyncInvoke", StringComparison.Ordinal));
        StringAssert.Contains(
            normalized,
            "else { global::Rhino.UI.Panels.OpenPanel(panelHostType, makeSelectedPanel); }");
        StringAssert.Contains(normalized, "GetVisibleMacPanel(panelHostType.GUID) != null");
        StringAssert.Contains(normalized, "return GetVisibleMacPanel(panelId);");
        StringAssert.Contains(
            normalized,
            "control.Loaded && control.ParentWindow?.Visible == true");
    }

    [TestMethod]
    public void MacPanelClose_ClosesTheVisibleWindowWithoutLeavingAnUnloadedHost()
    {
        var sourcePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/RhinoCommercialPlatform.Plugin/Panels/RhinoPanelGateway.cs"));
        var source = File.ReadAllText(sourcePath);
        var normalized = string.Join(" ", source.Split(
            new[] { ' ', '\r', '\n', '\t' },
            StringSplitOptions.RemoveEmptyEntries));
        var closeStart = normalized.IndexOf("public void ClosePanel", StringComparison.Ordinal);
        var closeEnd = normalized.IndexOf("public bool IsPanelVisible", StringComparison.Ordinal);
        var closeMethod = normalized.Substring(closeStart, closeEnd - closeStart);

        StringAssert.Contains(closeMethod, "if (global::Rhino.Runtime.HostUtils.RunningOnOSX)");
        StringAssert.Contains(closeMethod, "GetVisibleMacPanel(panelId)");
        StringAssert.Contains(closeMethod, "control.ParentWindow?.Close();");
        StringAssert.Contains(closeMethod, "else { global::Rhino.UI.Panels.ClosePanel(panelId); }");
    }
}
