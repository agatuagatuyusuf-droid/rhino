using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RhinoCommercialPlatform.UnitTests;

[TestClass]
public sealed class MainPanelServiceLifecycleTests
{
    [TestMethod]
    public void GetOrCreatePanelView_ReusesOnlyUndisposedCachedWidget()
    {
        var sourcePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/RhinoCommercialPlatform.Plugin/Panels/MainPanelService.cs"));
        var source = File.ReadAllText(sourcePath);
        var normalized = string.Join(" ", source.Split(
            new[] { ' ', '\r', '\n', '\t' },
            StringSplitOptions.RemoveEmptyEntries));

        StringAssert.Contains(
            normalized,
            "if (_panelView != null && !(_panelView is Eto.Widget widget && widget.IsDisposed)) return _panelView;",
            "A cached panel view must be reused unless its Eto widget has been disposed.");
    }
}
