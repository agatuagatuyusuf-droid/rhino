using System;
using System.Collections.Generic;
using System.Reflection;
using Rhino;
using RhinoCommercialPlatform.UI.Shell;
using RhinoCommercialPlatform.UI.Theme;
using RhinoCommercialPlatform.UI.Pages;
using RhinoCommercialPlatform.UI.ViewModels;
using RhinoCommercialPlatform.UI.Settings;
using RhinoCommercialPlatform.UI.Diagnostics;
using RhinoCommercialPlatform.Modules.Abstractions;

namespace RhinoCommercialPlatform.Plugin.Panels;

/// <summary>
/// Singleton service that manages the main panel lifecycle.
/// </summary>
public sealed class MainPanelService
{
    private static MainPanelService? _instance;
    private static readonly object _lock = new object();

    private readonly AppRuntime _runtime;
    private MainPanelController? _controller;
    private IPanelView? _panelView;
    private bool _disposed;

    private MainPanelService(AppRuntime runtime)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
    }

    public static MainPanelService GetInstance(AppRuntime runtime)
    {
        if (runtime == null)
            throw new ArgumentNullException(nameof(runtime));

        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new MainPanelService(runtime);
                }
            }
        }

        return _instance;
    }

    public IPanelView? GetOrCreatePanelView()
    {
        if (_disposed)
            return null;

        if (_panelView != null &&
            !(_panelView is Eto.Widget widget && widget.IsDisposed))
            return _panelView;

        try
        {
            var state = new MainShellState();
            var themeManager = _runtime.ThemeManager;
            var settingsService = _runtime.UserSettingsService;

            _controller = new MainPanelController(state, themeManager, settingsService);

            var lastPage = _controller.GetLastPageOrDefault();

            var dashboardViewModel = CreateDashboardViewModel();
            var modulesViewModel = new ModulesViewModel(_runtime.Modules);
            var runtimeStatusViewModel = CreateRuntimeStatusViewModel();
            var diagnosticsViewModel = CreateDiagnosticsViewModel();
            var settingsViewModel = new SettingsViewModel(settingsService, themeManager);
            var aboutViewModel = CreateAboutViewModel();

            diagnosticsViewModel.SetSnapshotFactory(() => CreateDiagnosticSnapshot());

            var dashboardPage = new DashboardPage(dashboardViewModel);
            var modulesPage = new ModulesPage(modulesViewModel);
            var runtimeStatusPage = new RuntimeStatusPage(runtimeStatusViewModel);
            var diagnosticsPage = new DiagnosticsPage(diagnosticsViewModel);
            var settingsPage = new SettingsPage(settingsViewModel);
            var aboutPage = new AboutPage(aboutViewModel);

            _panelView = _controller.CreatePanelView(
                dashboardPage,
                modulesPage,
                runtimeStatusPage,
                diagnosticsPage,
                settingsPage,
                aboutPage);

            _panelView.OpenLogDirectoryRequested += (_, _) => OpenDirectory(_runtime.Paths.LogsDirectory);
            _panelView.OpenConfigDirectoryRequested += (_, _) => OpenDirectory(_runtime.Paths.ConfigDirectory);
            _panelView.CopyDiagnosticsRequested += (_, _) => CopyDiagnosticsToClipboard();

            if (lastPage != NavigationPageId.Dashboard)
            {
                state.NavigateTo(lastPage);
                _panelView.ShowPage(lastPage);
            }

            return _panelView;
        }
        catch (Exception ex)
        {
            _runtime.Logger.Error($"Failed to create panel view: {ex.Message}");
            RhinoApp.WriteLine($"Failed to create panel: {ex.Message}");
            return null;
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _controller?.Dispose();
        _controller = null;
        _panelView = null;
        _disposed = true;

        lock (_lock)
        {
            if (_instance == this)
                _instance = null;
        }
    }

    private DashboardViewModel CreateDashboardViewModel()
    {
        return new DashboardViewModel(
            _runtime.Metadata,
            _runtime.Platform,
            _runtime.Modules,
            _runtime.Paths,
            _runtime.Logger,
            _runtime.StartedAtUtc,
            _runtime.LastRuntimeError);
    }

    private RuntimeStatusViewModel CreateRuntimeStatusViewModel()
    {
        string? docName = null;
        string? docPath = null;
        int? docObjCount = null;
        string? rhinoVersion = null;

        try
        {
            var doc = Rhino.RhinoDoc.ActiveDoc;
            if (doc != null)
            {
                docName = doc.Name;
                docPath = doc.Path;
                docObjCount = doc.Objects?.Count;
            }
            rhinoVersion = Rhino.RhinoApp.ExeVersion.ToString();
        }
        catch { }

        return new RuntimeStatusViewModel(
            _runtime.Metadata,
            _runtime.Platform,
            _runtime.Modules,
            _runtime.Paths,
            _runtime.Logger,
            _runtime.StartedAtUtc,
            docName, docPath, docObjCount,
            rhinoVersion, _runtime.LastRuntimeError);
    }

    private DiagnosticsViewModel CreateDiagnosticsViewModel()
    {
        return new DiagnosticsViewModel(
            _runtime.DiagnosticReportService,
            _runtime.Paths.LogsDirectory);
    }

    private AboutViewModel CreateAboutViewModel()
    {
        string? rhinoVersion = null;
        try { rhinoVersion = Rhino.RhinoApp.ExeVersion.ToString(); } catch { }

        return new AboutViewModel(
            _runtime.Metadata,
            _runtime.Platform.OperatingSystem,
            _runtime.Platform.ProcessArchitecture,
            _runtime.Platform.FrameworkDescription,
            rhinoVersion);
    }

    private DiagnosticSnapshot CreateDiagnosticSnapshot()
    {
        var moduleInfos = new List<DiagnosticSnapshot.ModuleInfo>();
        foreach (var module in _runtime.Modules.Modules)
        {
            moduleInfos.Add(new DiagnosticSnapshot.ModuleInfo
            {
                Id = module.Id,
                DisplayName = module.DisplayName,
                Version = module.Version.ToString(),
                Status = "Registered"
            });
        }

        string? commitInfo = null;
        try
        {
            var attr = typeof(AppRuntime).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (attr != null)
                commitInfo = attr.InformationalVersion;
        }
        catch { }

        string? rhinoVersion = null;
        try { rhinoVersion = Rhino.RhinoApp.ExeVersion.ToString(); } catch { }

        return _runtime.DiagnosticReportService.CreateSnapshot(
            _runtime.Metadata.ProductName,
            _runtime.Metadata.Version,
            commitInfo,
            _runtime.Platform.OperatingSystem,
            _runtime.Platform.ProcessArchitecture,
            _runtime.Platform.FrameworkDescription,
            rhinoVersion ?? "Unknown",
            _runtime.Metadata.Mode.ToString(),
            moduleInfos);
    }

    private void OpenDirectory(string path)
    {
        try
        {
            if (_runtime.ExternalLauncher != null)
            {
                _runtime.ExternalLauncher.OpenDirectory(path);
            }
            else
            {
                RhinoApp.WriteLine($"Cannot open directory: {path}");
            }
        }
        catch (Exception ex)
        {
            _runtime.Logger.Error($"Failed to open directory: {ex.Message}");
        }
    }

    private void CopyDiagnosticsToClipboard()
    {
        try
        {
            var snapshot = CreateDiagnosticSnapshot();
            var reportText = _runtime.DiagnosticReportService.GenerateReportText(snapshot);
            ClipboardHelper.CopyText(reportText);
            RhinoApp.WriteLine("Diagnostic report copied to clipboard.");
        }
        catch (Exception ex)
        {
            _runtime.Logger.Error($"Failed to copy diagnostics: {ex.Message}");
        }
    }
}
