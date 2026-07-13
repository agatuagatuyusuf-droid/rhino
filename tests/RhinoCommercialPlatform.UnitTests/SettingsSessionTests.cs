using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhinoCommercialPlatform.UI.Settings;
using RhinoCommercialPlatform.UI.Shell;
using RhinoCommercialPlatform.UI.Theme;
using RhinoCommercialPlatform.UI.ViewModels;

namespace RhinoCommercialPlatform.UnitTests;

[TestClass]
public sealed class SettingsSessionTests
{
    private string _tempDirectory = null!;
    private UserSettingsService _settingsService = null!;
    private ThemeManager _themeManager = null!;

    [TestInitialize]
    public void Initialize()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _settingsService = new UserSettingsService(
            new TestPaths(_tempDirectory),
            new TestLogger());
        _themeManager = new ThemeManager();
        _settingsService.Save(UserSettings.CreateDefaults());
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
    }

    [TestMethod]
    public void SettingsSave_PreservesLastPageSavedByController()
    {
        var (controller, viewModel) = CreateSession();

        controller.SaveCurrentPage(NavigationPageId.Modules);
        viewModel.SelectedLogLevel = "Debug";
        viewModel.Save();

        var savedSettings = _settingsService.Load();
        Assert.AreEqual("Modules", savedSettings.LastPage);
        Assert.AreEqual("Debug", savedSettings.LogLevel);
    }

    [TestMethod]
    public void ResetToDefaults_KeepsSharedSettingsInstance()
    {
        var (controller, viewModel) = CreateSession();
        var sharedSettings = controller.Settings;
        viewModel.SelectedTheme = "Dark";
        sharedSettings.SchemaVersion = 2;
        sharedSettings.AutoOpenPanel = true;
        sharedSettings.RememberLastPage = false;
        sharedSettings.LastPage = "About";
        sharedSettings.LogLevel = "Debug";
        sharedSettings.Language = "en-US";
        sharedSettings.UpdatedAtUtc = DateTime.UnixEpoch;

        viewModel.ResetToDefaults();

        Assert.AreSame(sharedSettings, viewModel.CurrentSettings);
        AssertSettingsEqual(_settingsService.Load(), controller.Settings);
    }

    [TestMethod]
    public void Reload_KeepsSharedSettingsInstance()
    {
        var (controller, viewModel) = CreateSession();
        var sharedSettings = controller.Settings;
        sharedSettings.UpdatedAtUtc = DateTime.UnixEpoch;
        _settingsService.Save(new UserSettings
        {
            SchemaVersion = 2,
            ThemeMode = "Dark",
            AutoOpenPanel = true,
            RememberLastPage = false,
            LastPage = "About",
            LogLevel = "Debug",
            Language = "en-US"
        });
        var reloadedSettings = _settingsService.Load();

        viewModel.Reload();

        Assert.AreSame(sharedSettings, viewModel.CurrentSettings);
        AssertSettingsEqual(reloadedSettings, controller.Settings);
    }

    private (MainPanelController Controller, SettingsViewModel ViewModel) CreateSession()
    {
        var controller = new MainPanelController(
            new MainShellState(),
            _themeManager,
            _settingsService);
        var viewModel = new SettingsViewModel(
            _settingsService,
            _themeManager,
            controller.Settings);
        return (controller, viewModel);
    }

    private static void AssertSettingsEqual(UserSettings expected, UserSettings actual)
    {
        Assert.AreEqual(expected.SchemaVersion, actual.SchemaVersion);
        Assert.AreEqual(expected.ThemeMode, actual.ThemeMode);
        Assert.AreEqual(expected.AutoOpenPanel, actual.AutoOpenPanel);
        Assert.AreEqual(expected.RememberLastPage, actual.RememberLastPage);
        Assert.AreEqual(expected.LastPage, actual.LastPage);
        Assert.AreEqual(expected.LogLevel, actual.LogLevel);
        Assert.AreEqual(expected.Language, actual.Language);
        Assert.AreEqual(expected.UpdatedAtUtc, actual.UpdatedAtUtc);
    }
}
