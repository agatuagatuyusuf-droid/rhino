using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RhinoCommercialPlatform.UnitTests;

[TestClass]
public sealed class RhinoPanelGatewayLifecycleTests
{
    [TestMethod]
    public void MacPanelFloat_IsDeferredToTheUiEventLoop()
    {
        var sourcePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/RhinoCommercialPlatform.Plugin/Panels/RhinoPanelGateway.cs"));
        var source = File.ReadAllText(sourcePath);
        var normalized = string.Join(" ", source.Split(
            new[] { ' ', '\r', '\n', '\t' },
            StringSplitOptions.RemoveEmptyEntries));

        StringAssert.Contains(
            normalized,
            "Eto.Forms.Application.Instance.AsyncInvoke(() =>");
        StringAssert.Contains(
            normalized,
            "panelHostType.GUID, global::Rhino.UI.Panels.FloatPanelMode.Show");
    }
}
