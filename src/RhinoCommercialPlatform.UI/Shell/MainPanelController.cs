using System;
using Eto.Forms;
using RhinoCommercialPlatform.UI.Pages;
using RhinoCommercialPlatform.UI.Settings;
using RhinoCommercialPlatform.UI.Theme;
using RhinoCommercialPlatform.UI.ViewModels;

namespace RhinoCommercialPlatform.UI.Shell;

public sealed class MainPanelController
{
    private readonly MainShellState _state;
    private readonly ThemeManager _themeManager;
    private readonly IUserSettingsService _settingsService;
    private readonly UserSettings _initialSettings;
    private IPanelView? _panelView;
    private bool _disposed;

    public MainShellState State => _state;
    public ThemeManager ThemeManager => _themeManager;
    public IPanelView? PanelView => _panelView;
    public UserSettings Settings => _initialSettings;

    public event EventHandler? PanelViewCreated;

    public MainPanelController(
        MainShellState state,
        ThemeManager themeManager,
        IUserSettingsService settingsService)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        // Load saved settings
        _initialSettings = _settingsService.Load();
        ApplySettingsToState();
    }

    public IPanelView CreatePanelView(
        DashboardPage dashboardPage,
        ModulesPage modulesPage,
        RuntimeStatusPage runtimeStatusPage,
        DiagnosticsPage diagnosticsPage,
        SettingsPage settingsPage,
        AboutPage aboutPage)
    {
        if (_panelView != null)
        {
            // Return existing instance if already created
            return _panelView;
        }

        _panelView = new MainPanelView(
            _state,
            _themeManager,
            dashboardPage,
            modulesPage,
            runtimeStatusPage,
            diagnosticsPage,
            settingsPage,
            aboutPage);

        // Wire up events
        _panelView.ThemeToggled += OnThemeToggled;
        _panelView.NavigationRequested += OnNavigationRequested;

        PanelViewCreated?.Invoke(this, EventArgs.Empty);

        return _panelView;
    }

    public NavigationPageId GetLastPageOrDefault()
    {
        if (_initialSettings.RememberLastPage)
        {
            var lastPage = _initialSettings.LastPage;
            return lastPage?.ToLowerInvariant() switch
            {
                "dashboard" => NavigationPageId.Dashboard,
                "modules" => NavigationPageId.Modules,
                "runtimestatus" => NavigationPageId.RuntimeStatus,
                "diagnostics" => NavigationPageId.Diagnostics,
                "settings" => NavigationPageId.Settings,
                "about" => NavigationPageId.About,
                _ => NavigationPageId.Dashboard
            };
        }

        return NavigationPageId.Dashboard;
    }

    public void SaveCurrentPage(NavigationPageId pageId)
    {
        try
        {
            _initialSettings.LastPage = pageId.ToString();
            _initialSettings.UpdatedAtUtc = DateTime.UtcNow;
            _settingsService.Save(_initialSettings);
        }
        catch (Exception)
        {
            // Silently handle save failures during navigation
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _panelView = null;
        _disposed = true;
    }

    private void ApplySettingsToState()
    {
        var themeMode = _initialSettings.ThemeMode;
        switch (themeMode?.ToLowerInvariant())
        {
            case "light":
                _themeManager.CurrentMode = ThemeMode.Light;
                break;
            case "dark":
                _themeManager.CurrentMode = ThemeMode.Dark;
                break;
            default:
                _themeManager.CurrentMode = ThemeMode.System;
                break;
        }

        if (_initialSettings.RememberLastPage)
        {
            var defaultPage = GetLastPageOrDefault();
            if (defaultPage != NavigationPageId.Dashboard)
            {
                _state.NavigateTo(defaultPage);
            }
        }
    }

    private void OnThemeToggled(object? sender, EventArgs e)
    {
        // Cycle through themes: System -> Light -> Dark -> System
        switch (_themeManager.CurrentMode)
        {
            case ThemeMode.System:
                _themeManager.CurrentMode = ThemeMode.Light;
                break;
            case ThemeMode.Light:
                _themeManager.CurrentMode = ThemeMode.Dark;
                break;
            case ThemeMode.Dark:
                _themeManager.CurrentMode = ThemeMode.System;
                break;
        }

        // Save theme preference
        var themeStr = _themeManager.CurrentMode switch
        {
            ThemeMode.Light => "Light",
            ThemeMode.Dark => "Dark",
            _ => "System"
        };

        try
        {
            _initialSettings.ThemeMode = themeStr;
            _initialSettings.UpdatedAtUtc = DateTime.UtcNow;
            _settingsService.Save(_initialSettings);
        }
        catch (Exception)
        {
            // Silently handle save failures
        }
    }

    private void OnNavigationRequested(object? sender, NavigationPageId pageId)
    {
        if (_initialSettings.RememberLastPage)
        {
            SaveCurrentPage(pageId);
        }
    }
}
