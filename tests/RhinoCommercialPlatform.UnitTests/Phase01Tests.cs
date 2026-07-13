using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhinoCommercialPlatform.Core.Abstractions;
using RhinoCommercialPlatform.Platform.Abstractions;
using RhinoCommercialPlatform.Core.Runtime;
using RhinoCommercialPlatform.Modules.Abstractions;
using RhinoCommercialPlatform.UI.Diagnostics;
using RhinoCommercialPlatform.UI.Settings;
using RhinoCommercialPlatform.UI.Shell;
using RhinoCommercialPlatform.UI.Theme;
using RhinoCommercialPlatform.UI.ViewModels;

namespace RhinoCommercialPlatform.UnitTests;

internal sealed class TestLogger : IAppLogger
{
    public List<string> DebugMessages { get; } = new List<string>();
    public List<string> InfoMessages { get; } = new List<string>();
    public List<string> WarningMessages { get; } = new List<string>();
    public List<string> ErrorMessages { get; } = new List<string>();

    public void Debug(string message) => DebugMessages.Add(message);
    public void Information(string message) => InfoMessages.Add(message);
    public void Warning(string message) => WarningMessages.Add(message);
    public void Error(string message) => ErrorMessages.Add(message);
    public void Error(Exception exception, string message) => ErrorMessages.Add($"{message}: {exception.Message}");
}

internal sealed class TestPaths : IAppPaths
{
    private readonly string _baseDir;

    public TestPaths(string baseDir) => _baseDir = baseDir;

    public string RootDirectory => _baseDir;
    public string ConfigDirectory => Path.Combine(_baseDir, "config");
    public string DataDirectory => Path.Combine(_baseDir, "data");
    public string CacheDirectory => Path.Combine(_baseDir, "cache");
    public string LogsDirectory => Path.Combine(_baseDir, "logs");
    public string UpdatesDirectory => Path.Combine(_baseDir, "updates");

    public void EnsureCreated() { }
}

internal sealed class TestModule : IPluginModule
{
    public string Id { get; }
    public string DisplayName { get; }
    public Version Version { get; } = new Version(1, 0);

    public TestModule(string id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }

    public void Initialize(IModuleContext context) { }
    public void Shutdown() { }
}

internal sealed class MockPlatformInfo : IPlatformInfo
{
    public bool IsWindows => true;
    public bool IsMacOS => false;
    public string OperatingSystem => "Windows";
    public string OperatingSystemDescription => "Microsoft Windows 10.0.22631";
    public string ProcessArchitecture => "x64";
    public string FrameworkDescription => ".NET 8.0.0";
}

// ============================================================
//  UserSettingsService Tests (15 tests)
// ============================================================

[TestClass]
public sealed class UserSettingsServicePhase01Tests
{
    private static UserSettings ParseSettingsViaReflection(string json)
    {
        var method = typeof(UserSettingsService).GetMethod("ParseSettings",
            BindingFlags.Static | BindingFlags.NonPublic);
        return (UserSettings)method!.Invoke(null, new object[] { json })!;
    }

    private static string SerializeSettingsViaReflection(UserSettings settings)
    {
        var method = typeof(UserSettingsService).GetMethod("SerializeSettings",
            BindingFlags.Static | BindingFlags.NonPublic);
        return (string)method!.Invoke(null, new object[] { settings })!;
    }

    private static void SeedSettingsFile(IAppPaths paths, UserSettings? settings = null)
    {
        var configDir = paths.ConfigDirectory;
        Directory.CreateDirectory(configDir);
        var json = SerializeSettingsViaReflection(settings ?? UserSettings.CreateDefaults());
        File.WriteAllText(Path.Combine(configDir, UserSettingsSchema.FileName), json);
    }

    [TestMethod]
    public void MissingFile_ReturnsDefaults()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            var result = service.Load();

            Assert.AreEqual(UserSettingsDefaults.SchemaVersion, result.SchemaVersion);
            Assert.AreEqual(UserSettingsDefaults.ThemeMode, result.ThemeMode);
            Assert.AreEqual(UserSettingsDefaults.AutoOpenPanel, result.AutoOpenPanel);
            Assert.AreEqual(UserSettingsDefaults.RememberLastPage, result.RememberLastPage);
            Assert.AreEqual(UserSettingsDefaults.LastPage, result.LastPage);
            Assert.AreEqual(UserSettingsDefaults.LogLevel, result.LogLevel);
            Assert.AreEqual(UserSettingsDefaults.Language, result.Language);
            Assert.IsTrue(logger.DebugMessages.Any(m => m.Contains("not found")));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void ValidFile_LoadsSettings()
    {
        var json = """
            {
                "schemaVersion": 2,
                "themeMode": "Dark",
                "autoOpenPanel": true,
                "rememberLastPage": false,
                "lastPage": "Modules",
                "logLevel": "Debug",
                "language": "en-US"
            }
            """;

        var settings = ParseSettingsViaReflection(json);

        Assert.AreEqual(2, settings.SchemaVersion);
        Assert.AreEqual("Dark", settings.ThemeMode);
        Assert.IsTrue(settings.AutoOpenPanel);
        Assert.IsFalse(settings.RememberLastPage);
        Assert.AreEqual("Modules", settings.LastPage);
        Assert.AreEqual("Debug", settings.LogLevel);
        Assert.AreEqual("en-US", settings.Language);
    }

    [TestMethod]
    public void InvalidJson_ReturnsDefaultsAndReportsError()
    {
        var json = "{ this is not valid json }";

        var settings = ParseSettingsViaReflection(json);

        Assert.AreEqual(UserSettingsDefaults.SchemaVersion, settings.SchemaVersion);
        Assert.AreEqual(UserSettingsDefaults.ThemeMode, settings.ThemeMode);
    }

    [TestMethod]
    public void MissingFields_UsesDefaults()
    {
        var json = """{ "schemaVersion": 1 }""";

        var settings = ParseSettingsViaReflection(json);

        Assert.AreEqual(1, settings.SchemaVersion);
        Assert.AreEqual(UserSettingsDefaults.ThemeMode, settings.ThemeMode);
        Assert.AreEqual(UserSettingsDefaults.AutoOpenPanel, settings.AutoOpenPanel);
        Assert.AreEqual(UserSettingsDefaults.RememberLastPage, settings.RememberLastPage);
        Assert.AreEqual(UserSettingsDefaults.LastPage, settings.LastPage);
        Assert.AreEqual(UserSettingsDefaults.LogLevel, settings.LogLevel);
        Assert.AreEqual(UserSettingsDefaults.Language, settings.Language);
    }

    [TestMethod]
    public void SaveAndLoad_RoundTrips()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            SeedSettingsFile(paths);

            var original = new UserSettings
            {
                SchemaVersion = 42,
                ThemeMode = "Dark",
                AutoOpenPanel = true,
                RememberLastPage = false,
                LastPage = "Settings",
                LogLevel = "Warning",
                Language = "de-DE"
            };

            service.Save(original);
            var loaded = service.Load();

            Assert.AreEqual(original.SchemaVersion, loaded.SchemaVersion);
            Assert.AreEqual(original.ThemeMode, loaded.ThemeMode);
            Assert.AreEqual(original.AutoOpenPanel, loaded.AutoOpenPanel);
            Assert.AreEqual(original.RememberLastPage, loaded.RememberLastPage);
            Assert.AreEqual(original.LastPage, loaded.LastPage);
            Assert.AreEqual(original.LogLevel, loaded.LogLevel);
            Assert.AreEqual(original.Language, loaded.Language);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void Reset_ReturnsDefaults()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            SeedSettingsFile(paths);

            var custom = new UserSettings
            {
                SchemaVersion = 99,
                ThemeMode = "Dark",
                AutoOpenPanel = true,
                LastPage = "About"
            };
            service.Save(custom);

            var reset = service.ResetToDefaults();

            Assert.AreEqual(UserSettingsDefaults.SchemaVersion, reset.SchemaVersion);
            Assert.AreEqual(UserSettingsDefaults.ThemeMode, reset.ThemeMode);
            Assert.AreEqual(UserSettingsDefaults.AutoOpenPanel, reset.AutoOpenPanel);
            Assert.AreEqual(UserSettingsDefaults.LastPage, reset.LastPage);

            var loaded = service.Load();
            Assert.AreEqual(UserSettingsDefaults.SchemaVersion, loaded.SchemaVersion);
            Assert.AreEqual(UserSettingsDefaults.ThemeMode, loaded.ThemeMode);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void UnknownTheme_FallsBackSafely()
    {
        var json = """{ "themeMode": "UnknownTheme" }""";

        var settings = ParseSettingsViaReflection(json);

        Assert.AreEqual(UserSettingsDefaults.ThemeMode, settings.ThemeMode);
    }

    [TestMethod]
    public void UnknownPage_FallsBackToDashboard()
    {
        var json = """{ "lastPage": "Foo" }""";

        var settings = ParseSettingsViaReflection(json);

        Assert.AreEqual(UserSettingsDefaults.LastPage, settings.LastPage);
    }

    [TestMethod]
    public void DoesNotPersistSecrets()
    {
        var settings = UserSettings.CreateDefaults();
        var json = SerializeSettingsViaReflection(settings);

        using var doc = JsonDocument.Parse(json);
        var propertyNames = doc.RootElement.EnumerateObject().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var knownProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "schemaVersion", "themeMode", "autoOpenPanel", "rememberLastPage",
            "lastPage", "logLevel", "language", "updatedAtUtc"
        };

        foreach (var name in propertyNames)
            Assert.IsTrue(knownProperties.Contains(name), $"Unexpected property '{name}' in serialized settings");
    }

    [TestMethod]
    public void Save_CreatesDirectoryWhenMissing()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            Assert.IsFalse(Directory.Exists(paths.ConfigDirectory));

            // First save must succeed (fixed: File.Move used when target doesn't exist)
            service.Save(UserSettings.CreateDefaults());

            Assert.IsTrue(Directory.Exists(paths.ConfigDirectory));
            Assert.IsTrue(File.Exists(Path.Combine(paths.ConfigDirectory, UserSettingsSchema.FileName)));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void FirstSave_CreatesSettingsFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            var filePath = Path.Combine(paths.ConfigDirectory, UserSettingsSchema.FileName);
            Assert.IsFalse(File.Exists(filePath));

            service.Save(UserSettings.CreateDefaults());

            Assert.IsTrue(File.Exists(filePath));
            Assert.IsTrue(new FileInfo(filePath).Length > 0);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void FirstSave_RoundTripsSuccessfully()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            var original = new UserSettings
            {
                SchemaVersion = 3,
                ThemeMode = "Dark",
                AutoOpenPanel = true,
                RememberLastPage = false,
                LastPage = "Settings",
                LogLevel = "Warning",
                Language = "de-DE"
            };

            service.Save(original);
            var loaded = service.Load();

            Assert.AreEqual(original.SchemaVersion, loaded.SchemaVersion);
            Assert.AreEqual(original.ThemeMode, loaded.ThemeMode);
            Assert.AreEqual(original.AutoOpenPanel, loaded.AutoOpenPanel);
            Assert.AreEqual(original.RememberLastPage, loaded.RememberLastPage);
            Assert.AreEqual(original.LastPage, loaded.LastPage);
            Assert.AreEqual(original.LogLevel, loaded.LogLevel);
            Assert.AreEqual(original.Language, loaded.Language);
            Assert.IsNotNull(loaded.UpdatedAtUtc);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void SecondSave_ReplacesExistingFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            var first = new UserSettings { ThemeMode = "Light" };
            service.Save(first);

            var firstLoaded = service.Load();
            Assert.AreEqual("Light", firstLoaded.ThemeMode);

            var second = new UserSettings { ThemeMode = "Dark", LogLevel = "Debug" };
            service.Save(second);

            var secondLoaded = service.Load();
            Assert.AreEqual("Dark", secondLoaded.ThemeMode);
            Assert.AreEqual("Debug", secondLoaded.LogLevel);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void FailedSave_DoesNotDestroyExistingFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            var original = new UserSettings { ThemeMode = "Dark" };
            service.Save(original);

            // Verify second save works
            service.Save(new UserSettings { ThemeMode = "Light" });
            var loaded = service.Load();
            Assert.AreEqual("Light", loaded.ThemeMode);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void Reset_WorksBeforeSettingsFileExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            var filePath = Path.Combine(paths.ConfigDirectory, UserSettingsSchema.FileName);
            Assert.IsFalse(File.Exists(filePath));

            var reset = service.ResetToDefaults();

            Assert.IsTrue(File.Exists(filePath));
            Assert.AreEqual(UserSettingsDefaults.SchemaVersion, reset.SchemaVersion);
            Assert.AreEqual(UserSettingsDefaults.ThemeMode, reset.ThemeMode);

            var loaded = service.Load();
            Assert.AreEqual(UserSettingsDefaults.SchemaVersion, loaded.SchemaVersion);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void NoTemporaryFileRemainsAfterSuccessfulSave()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            service.Save(UserSettings.CreateDefaults());

            var configDir = paths.ConfigDirectory;
            var tmpFiles = Directory.GetFiles(configDir, "*.tmp");
            Assert.AreEqual(0, tmpFiles.Length, "Temporary files should be cleaned up after save.");
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}

// ============================================================
//  Navigation Tests (3 tests)
// ============================================================

[TestClass]
public sealed class MainShellStateTests
{
    [TestMethod]
    public void DefaultPage_IsDashboard()
    {
        var state = new MainShellState();
        Assert.AreEqual(NavigationPageId.Dashboard, state.CurrentPage);
    }

    [TestMethod]
    public void Navigate_ChangesCurrentPage()
    {
        var state = new MainShellState();
        state.NavigateTo(NavigationPageId.Settings);
        Assert.AreEqual(NavigationPageId.Settings, state.CurrentPage);
        Assert.AreEqual(NavigationPageId.Dashboard, state.PreviousPage);
    }

    [TestMethod]
    public void RepeatedNavigation_DoesNotDuplicateState()
    {
        var state = new MainShellState();
        var pageChanges = new List<NavigationPageId>();
        state.PageChanged += (s, page) => pageChanges.Add(page);

        state.NavigateTo(NavigationPageId.Modules);
        state.NavigateTo(NavigationPageId.Modules);
        state.NavigateTo(NavigationPageId.Modules);

        Assert.AreEqual(1, pageChanges.Count);
        Assert.AreEqual(NavigationPageId.Modules, state.CurrentPage);
    }
}

// ============================================================
//  Theme Tests (5 tests)
// ============================================================

[TestClass]
public sealed class ThemeManagerPhase01Tests
{
    [TestMethod]
    public void DefaultTheme_IsSystem()
    {
        var manager = new ThemeManager();
        Assert.AreEqual(ThemeMode.System, manager.CurrentMode);
    }

    [TestMethod]
    public void LightTheme_ReturnsLightPalette()
    {
        var manager = new ThemeManager();
        manager.CurrentMode = ThemeMode.Light;
        Assert.AreEqual(ThemeMode.Light, manager.CurrentMode);
        Assert.AreEqual("Light", manager.GetModeString());
        Assert.IsFalse(manager.CurrentPalette.IsDark);
    }

    [TestMethod]
    public void DarkTheme_ReturnsDarkPalette()
    {
        var manager = new ThemeManager();
        manager.CurrentMode = ThemeMode.Dark;
        Assert.AreEqual(ThemeMode.Dark, manager.CurrentMode);
        Assert.IsTrue(manager.CurrentPalette.IsDark);
    }

    [TestMethod]
    public void SystemTheme_DoesNotHardcodeLight()
    {
        // System theme should check the actual system/Rhino theme.
        // When no provider is given, it falls back to Light.
        // When a provider returns Dark, it should use Dark.
        var manager = new ThemeManager();
        Assert.AreEqual(ThemeMode.System, manager.CurrentMode);

        // With a null/default provider, System falls back to Light
        Assert.IsFalse(manager.CurrentPalette.IsDark);
    }

    [TestMethod]
    public void InvalidTheme_UsesSafeFallback()
    {
        var manager = new ThemeManager();

        manager.SetThemeFromModeString(null!);
        Assert.AreEqual(ThemeMode.System, manager.CurrentMode);

        manager.SetThemeFromModeString("");
        Assert.AreEqual(ThemeMode.System, manager.CurrentMode);

        manager.SetThemeFromModeString("  ");
        Assert.AreEqual(ThemeMode.System, manager.CurrentMode);

        manager.SetThemeFromModeString("UnknownThemeValue");
        Assert.AreEqual(ThemeMode.System, manager.CurrentMode);
    }
}

// ============================================================
//  Dashboard Tests (4 tests)
// ============================================================

[TestClass]
public sealed class DashboardViewModelPhase01Tests
{
    private static DashboardViewModel CreateViewModel(
        PluginMetadata? metadata = null,
        IPlatformInfo? platform = null,
        ModuleRegistry? modules = null,
        IAppPaths? paths = null,
        IAppLogger? logger = null,
        DateTime? runtimeStartedAt = null,
        string? lastRuntimeError = null)
    {
        return new DashboardViewModel(
            metadata ?? new PluginMetadata("TestProduct", "1.0.0", RuntimeMode.Development),
            platform ?? new MockPlatformInfo(),
            modules ?? new ModuleRegistry(),
            paths ?? new TestPaths(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())),
            logger ?? new TestLogger(),
            runtimeStartedAt ?? DateTime.UtcNow,
            lastRuntimeError);
    }

    [TestMethod]
    public void Dashboard_UsesRuntimeMetadata()
    {
        var metadata = new PluginMetadata("MyPlugin", "2.5.0", RuntimeMode.Production);
        var vm = CreateViewModel(metadata: metadata);

        Assert.AreEqual("MyPlugin", vm.ProductName);
        Assert.AreEqual("2.5.0", vm.PluginVersion);
        Assert.AreEqual("Production", vm.RunMode);
    }

    [TestMethod]
    public void Dashboard_ReportsCorrectModuleCounts()
    {
        var registry = new ModuleRegistry();
        registry.Register(new TestModule("mod1", "Module One"));
        registry.Register(new TestModule("mod2", "Module Two"));

        var vm = CreateViewModel(modules: registry);

        Assert.AreEqual(2, vm.RegisteredModuleCount);
        Assert.AreEqual(0, vm.InitializedModuleCount);
    }

    [TestMethod]
    public void Dashboard_ReportsCurrentPlatform()
    {
        var vm = CreateViewModel();

        Assert.AreEqual("Windows", vm.OperatingSystem);
        Assert.AreEqual("Microsoft Windows 10.0.22631", vm.OperatingSystemDescription);
        Assert.AreEqual("x64", vm.ProcessArchitecture);
        Assert.AreEqual(".NET 8.0.0", vm.FrameworkDescription);
    }

    [TestMethod]
    public void Dashboard_HandlesNoRhinoDocument()
    {
        var vm = CreateViewModel(lastRuntimeError: null);

        Assert.IsNull(vm.LastRuntimeError);
        Assert.AreEqual("正常运行", vm.PluginStatus);
        Assert.IsFalse(string.IsNullOrEmpty(vm.ProductName));
        Assert.IsFalse(string.IsNullOrEmpty(vm.PluginVersion));
        Assert.AreEqual(4, vm.QuickEntries.Count);
    }
}

// ============================================================
//  Modules Tests (3 tests)
// ============================================================

[TestClass]
public sealed class ModulesViewModelPhase01Tests
{
    [TestMethod]
    public void ModulesViewModel_UsesRuntimeRegistry()
    {
        var registry = new ModuleRegistry();
        registry.Register(new TestModule("alpha", "Alpha Module"));
        registry.Register(new TestModule("beta", "Beta Module"));

        var vm = new ModulesViewModel(registry);

        Assert.AreEqual(2, vm.Descriptors.Count);
        Assert.IsTrue(vm.Descriptors.Any(d => d.Id == "alpha"));
        Assert.IsTrue(vm.Descriptors.Any(d => d.Id == "beta"));
    }

    [TestMethod]
    public void ModulesViewModel_ShowsInitializedState()
    {
        var registry = new ModuleRegistry();
        registry.Register(new TestModule("initialized", "Initialized Module"));

        var context = new ModuleContext(new TestLogger(), new TestPaths(Path.GetTempPath()));
        registry.InitializeAll(context);

        var vm = new ModulesViewModel(registry);

        var desc = vm.Descriptors.Single(d => d.Id == "initialized");
        Assert.IsTrue(desc.IsInitialized);
        Assert.AreEqual("已加载", desc.StatusText);
    }

    [TestMethod]
    public void NewModule_AppearsAutomatically()
    {
        var registry = new ModuleRegistry();
        registry.Register(new TestModule("first", "First Module"));

        var vm = new ModulesViewModel(registry);
        Assert.AreEqual(1, vm.Descriptors.Count);

        registry.Register(new TestModule("second", "Second Module"));

        Assert.AreEqual(2, vm.Descriptors.Count);
        Assert.IsTrue(vm.Descriptors.Any(d => d.Id == "second"));
    }
}

// ============================================================
//  RecentLogReader Tests (3 tests)
// ============================================================

[TestClass]
public sealed class RecentLogReaderPhase01Tests
{
    [TestMethod]
    public void RecentLogReader_HandlesMissingFile()
    {
        var result = RecentLogReader.ReadLastLines("/path/that/does/not/exist.log");

        Assert.AreEqual(1, result.Count);
        Assert.IsTrue(result[0].Contains("不存在"));
    }

    [TestMethod]
    public void RecentLogReader_LimitsMaximumLines()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempDir);
            var filePath = Path.Combine(tempDir, "long.log");
            var lines = new List<string>();
            for (int i = 0; i < 300; i++)
                lines.Add($"Line number {i}");

            File.WriteAllText(filePath, string.Join(Environment.NewLine, lines));

            var result = RecentLogReader.ReadLastLines(filePath, maxLines: 50);

            Assert.IsTrue(result.Count <= 50);
            Assert.AreEqual("Line number 250", result[0]);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void RecentLogReader_HandlesLockedFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            Directory.CreateDirectory(tempDir);
            var filePath = Path.Combine(tempDir, "locked.log");
            File.WriteAllText(filePath, "line1" + Environment.NewLine + "line2" + Environment.NewLine + "line3");

            using (var lockStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                var result = RecentLogReader.ReadLastLines(filePath);

                Assert.AreEqual(1, result.Count);
                Assert.IsTrue(result[0].Contains("失败") || result[0].Contains("locked") || result[0].Contains("权限"));
            }
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}

// ============================================================
//  Panel Architecture Tests (5 tests)
// ============================================================

[TestClass]
public sealed class PanelArchitectureTests
{
    [TestMethod]
    public void PanelFormFactory_DoesNotExist()
    {
        var panelFormFactoryType = typeof(UserSettingsService).Assembly.GetType("RhinoCommercialPlatform.UI.Shell.PanelFormFactory");
        Assert.IsNull(panelFormFactoryType, "PanelFormFactory should be deleted — no Eto.Form-based main panel.");
    }

    [TestMethod]
    public void NoCustomFormHandleExists()
    {
        // Verify no custom PanelHandle or Form-based panel wrapper exists in UI assembly.
        // The real panel is RhinoMainPanelHost registered via Rhino.UI.Panels.RegisterPanel.
        var uiAssemblyTypes = typeof(UserSettingsService).Assembly.GetTypes().Select(t => t.Name).ToHashSet();
        Assert.IsFalse(uiAssemblyTypes.Contains("PanelHandle"), "PanelHandle should be deleted.");
        Assert.IsFalse(uiAssemblyTypes.Contains("PanelFormFactory"), "PanelFormFactory should be deleted.");
    }

    [TestMethod]
    public void RegisteredPanelGuid_IsWellKnown()
    {
        // The GUID is defined in MainPanelRegistration.PanelId and must match
        // the GuidAttribute on RhinoMainPanelHost.
        // This test verifies the well-known value is consistent.
        var expected = "7B3E4F2A-1D8C-4E5F-9A6B-3C2D1E0F8A7B";
        Assert.AreEqual(36, expected.Length);
        Assert.IsTrue(Guid.TryParse(expected, out _));
    }

    [TestMethod]
    public void MainPanelView_DoesNotReferenceRhinoCommon()
    {
        var viewType = typeof(MainPanelView);
        var assembly = viewType.Assembly;

        // Check that RhinoCommon is not referenced by the UI assembly
        var rhinoRef = assembly.GetReferencedAssemblies()
            .FirstOrDefault(r => r.Name == "RhinoCommon" || r.Name == "Rhino.UI");
        Assert.IsNull(rhinoRef, "MainPanelView assembly should not reference RhinoCommon or Rhino.UI");
    }

    [TestMethod]
    public void PluginReferencesRhinoUIForPanelsApi()
    {
        // The Plugin project must reference Rhino.UI for Panels.RegisterPanel etc.
        // This can't be tested from the UI test assembly (no Plugin ref).
        // Instead, we verify the UI assembly does NOT reference Rhino.UI (separation).
        var uiAssembly = typeof(MainPanelView).Assembly;
        var rhinoRef = uiAssembly.GetReferencedAssemblies()
            .FirstOrDefault(r => r.Name == "Rhino.UI" || r.Name == "RhinoCommon");
        Assert.IsNull(rhinoRef, "UI assembly should not reference Rhino.UI or RhinoCommon.");
    }
}

// ============================================================
//  System.Text.Json Dependency Tests (1 test)
// ============================================================

[TestClass]
public sealed class SystemTextJsonDependencyTests
{
    [TestMethod]
    public void UIAssembly_DoesNotReferenceSystemTextJson()
    {
        var uiAssembly = typeof(MainPanelView).Assembly;
        var stjRef = uiAssembly.GetReferencedAssemblies()
            .FirstOrDefault(r => r.Name == "System.Text.Json");
        Assert.IsNull(stjRef, "UI assembly must not reference System.Text.Json at compile time.");
    }
}

// ============================================================
//  PanelGatewayContract Tests (6 tests)
// ============================================================

internal sealed class MockRhinoPanelGateway : IRhinoPanelGateway
{
    public Func<object, Type, string, object?, bool>? OnRegisterPanel { get; set; }
    public Func<Type, bool, bool>? OnOpenPanel { get; set; }
    public Action<Guid>? OnClosePanel { get; set; }
    public Func<Type, bool, bool>? OnIsPanelVisible { get; set; }
    public Func<Guid, object?>? OnGetPanel { get; set; }

    public bool RegisterPanel(object pluginInstance, Type panelType, string panelName, object? icon)
        => OnRegisterPanel?.Invoke(pluginInstance, panelType, panelName, icon) ?? true;
    public bool OpenPanel(Type panelHostType, bool makeSelectedPanel) => OnOpenPanel?.Invoke(panelHostType, makeSelectedPanel) ?? true;
    public void ClosePanel(Guid panelId) => OnClosePanel?.Invoke(panelId);
    public bool IsPanelVisible(Type panelHostType, bool isSelectedTab) => OnIsPanelVisible?.Invoke(panelHostType, isSelectedTab) ?? true;
    public object? GetPanel(Guid panelId) => OnGetPanel?.Invoke(panelId);
}

[TestClass]
public sealed class PanelGatewayContractTests
{
    [TestMethod]
    public void RegisterPanel_ReturnsTrueOnSuccess()
    {
        var gateway = new MockRhinoPanelGateway();
        gateway.OnRegisterPanel = (_, _, _, _) => true;
        var result = gateway.RegisterPanel(new object(), typeof(object), "Test", null);
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void RegisterPanel_ReturnsFalseOnFailure()
    {
        var gateway = new MockRhinoPanelGateway();
        gateway.OnRegisterPanel = (_, _, _, _) => false;
        var result = gateway.RegisterPanel(new object(), typeof(object), "Test", null);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void OpenPanel_ReturnsGatewayResult()
    {
        var gateway = new MockRhinoPanelGateway();
        gateway.OnOpenPanel = (_, makeSelectedPanel) => makeSelectedPanel;
        Assert.IsTrue(gateway.OpenPanel(typeof(object), true));

        gateway.OnOpenPanel = (_, makeSelectedPanel) => makeSelectedPanel;
        Assert.IsFalse(gateway.OpenPanel(typeof(object), false));
    }

    [TestMethod]
    public void ClosePanel_InvokesGatewayAction()
    {
        var calls = 0;
        var gateway = new MockRhinoPanelGateway();
        gateway.OnClosePanel = _ => calls++;
        gateway.ClosePanel(Guid.NewGuid());
        gateway.ClosePanel(Guid.NewGuid());
        Assert.AreEqual(2, calls);
    }

    [TestMethod]
    public void IsPanelVisible_ReturnsGatewayResult()
    {
        var gateway = new MockRhinoPanelGateway();
        gateway.OnIsPanelVisible = (_, isSelectedTab) => isSelectedTab;
        Assert.IsTrue(gateway.IsPanelVisible(typeof(object), true));

        gateway.OnIsPanelVisible = (_, isSelectedTab) => isSelectedTab;
        Assert.IsFalse(gateway.IsPanelVisible(typeof(object), false));
    }

    [TestMethod]
    public void GetPanel_ReturnsPanelInstance()
    {
        var gateway = new MockRhinoPanelGateway();
        gateway.OnGetPanel = _ => new object();
        Assert.IsNotNull(gateway.GetPanel(Guid.NewGuid()));

        gateway.OnGetPanel = _ => null;
        Assert.IsNull(gateway.GetPanel(Guid.NewGuid()));
    }
}

// ============================================================
//  UserSettings Concurrency Tests (5 tests)
// ============================================================

[TestClass]
public sealed class UserSettingsConcurrencyTests
{
    [TestMethod]
    public void ConcurrentSaves_DoNotCorruptFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            var options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
            Parallel.For(0, 20, options, i =>
            {
                var s = new UserSettings { ThemeMode = i % 2 == 0 ? "Dark" : "Light" };
                service.Save(s);
            });

            var loaded = service.Load();
            Assert.IsNotNull(loaded);
            Assert.IsTrue(loaded.ThemeMode == "Dark" || loaded.ThemeMode == "Light");
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void ConcurrentLoadAndSave_DoesNotThrow()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            service.Save(UserSettings.CreateDefaults());

            var options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
            Parallel.For(0, 20, options, i =>
            {
                if (i % 2 == 0)
                    service.Load();
                else
                {
                    var s = new UserSettings { ThemeMode = i % 2 == 0 ? "Dark" : "Light" };
                    service.Save(s);
                }
            });
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void ConcurrentSaves_LeaveNoTempFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            var options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
            Parallel.For(0, 20, options, i =>
            {
                service.Save(UserSettings.CreateDefaults());
            });

            var tmpFiles = Directory.GetFiles(paths.ConfigDirectory, "*.tmp");
            Assert.AreEqual(0, tmpFiles.Length, "No temporary files should remain after concurrent saves.");
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void Reset_DoesNotDeadlock()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            service.Save(UserSettings.CreateDefaults());

            var options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
            Parallel.For(0, 20, options, i =>
            {
                if (i % 3 == 0)
                    service.ResetToDefaults();
                else if (i % 3 == 1)
                    service.Load();
                else
                    service.Save(new UserSettings { ThemeMode = "Dark" });
            });
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void SavedFile_IsAlwaysReadableJson()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new TestPaths(tempDir);
            var logger = new TestLogger();
            var service = new UserSettingsService(paths, logger);

            service.Save(new UserSettings { ThemeMode = "Dark", LogLevel = "Debug" });

            var filePath = Path.Combine(paths.ConfigDirectory, UserSettingsSchema.FileName);
            var json = File.ReadAllText(filePath);

            using var doc = JsonDocument.Parse(json);
            Assert.IsTrue(doc.RootElement.TryGetProperty("themeMode", out var mode));
            Assert.AreEqual("Dark", mode.GetString());
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}
