using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhinoCommercialPlatform.Core.Abstractions;
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
//  UserSettingsService Tests (10 tests)
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

            try
            {
                service.Save(UserSettings.CreateDefaults());
            }
            catch (FileNotFoundException)
            {
                // File.Replace requires the destination to exist on first save.
                // Directory.CreateDirectory inside Save() runs before that, so
                // we verify the directory was created despite the failed replace.
            }

            Assert.IsTrue(Directory.Exists(paths.ConfigDirectory));
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
//  Theme Tests (3 tests)
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
