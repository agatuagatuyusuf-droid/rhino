using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RhinoCommercialPlatform.UnitTests;

[TestClass]
public sealed class MainPanelServiceLifecycleTests
{
    [TestMethod]
    public void CreatePanelView_DoesNotReuseAViewAcrossRhinoPanelHosts()
    {
        var sourcePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/RhinoCommercialPlatform.Plugin/Panels/MainPanelService.cs"));
        var source = File.ReadAllText(sourcePath);
        var normalized = string.Join(" ", source.Split(
            new[] { ' ', '\r', '\n', '\t' },
            StringSplitOptions.RemoveEmptyEntries));

        Assert.IsFalse(normalized.Contains("private IPanelView? _panelView", StringComparison.Ordinal));
        StringAssert.Contains(normalized, "var controller = new MainPanelController(");
        StringAssert.Contains(normalized, "return panelView;");
    }

    [TestMethod]
    public void CreatePanelView_SharesControllerSettingsWithSettingsViewModel()
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
            "new SettingsViewModel(settingsService, themeManager, controller.Settings)");
        Assert.IsFalse(normalized.Contains(
            "new SettingsViewModel(settingsService, themeManager);",
            StringComparison.Ordinal));
    }

    [TestMethod]
    public void MainPanelView_UnsubscribesFromThemeChangesWhenDisposed()
    {
        var sourcePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/RhinoCommercialPlatform.UI/Shell/MainPanelView.cs"));
        var source = File.ReadAllText(sourcePath);
        var normalized = string.Join(" ", source.Split(
            new[] { ' ', '\r', '\n', '\t' },
            StringSplitOptions.RemoveEmptyEntries));

        StringAssert.Contains(normalized, "protected override void Dispose(bool disposing)");
        StringAssert.Contains(normalized, "_themeManager.ThemeChanged -= OnThemeChanged;");
        Assert.IsFalse(normalized.Contains("override void OnUnLoad", StringComparison.Ordinal));
    }

    [TestMethod]
    public void VerifyPanel_InspectsTheRhinoHostedView()
    {
        var sourcePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "../../../../../src/RhinoCommercialPlatform.Plugin/Commands/VerifyPanelCommand.cs"));
        var source = File.ReadAllText(sourcePath);

        Assert.IsFalse(source.Contains("GetOrCreatePanelView", StringComparison.Ordinal));
        StringAssert.Contains(source, "host.Content.GetType() == typeof(MainPanelView)");
    }
}
