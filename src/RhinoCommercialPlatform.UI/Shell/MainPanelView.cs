using System;
using Eto.Forms;
using Eto.Drawing;
using RhinoCommercialPlatform.UI.Components;
using RhinoCommercialPlatform.UI.Pages;
using RhinoCommercialPlatform.UI.Theme;
using RhinoCommercialPlatform.UI.ViewModels;

namespace RhinoCommercialPlatform.UI.Shell;

public sealed class MainPanelView : Panel, IPanelView
{
    private readonly MainShellState _state;
    private readonly ThemeManager _themeManager;

    // Components
    private readonly AppHeader _header;
    private readonly SidebarNavigation _sidebar;
    private readonly StatusFooter _footer;
    private readonly Panel _contentArea;

    // Pages
    private readonly DashboardPage _dashboardPage;
    private readonly ModulesPage _modulesPage;
    private readonly RuntimeStatusPage _runtimeStatusPage;
    private readonly DiagnosticsPage _diagnosticsPage;
    private readonly SettingsPage _settingsPage;
    private readonly AboutPage _aboutPage;

    // Events
    public event EventHandler<NavigationPageId>? NavigationRequested;
    public event EventHandler? ThemeToggled;
    public event EventHandler? SettingsRequested;
    public event EventHandler? OpenLogDirectoryRequested;
    public event EventHandler? CopyDiagnosticsRequested;
    public event EventHandler? OpenConfigDirectoryRequested;

    public MainPanelView(
        MainShellState state,
        ThemeManager themeManager,
        DashboardPage dashboardPage,
        ModulesPage modulesPage,
        RuntimeStatusPage runtimeStatusPage,
        DiagnosticsPage diagnosticsPage,
        SettingsPage settingsPage,
        AboutPage aboutPage)
    {
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _themeManager = themeManager ?? throw new ArgumentNullException(nameof(themeManager));
        _dashboardPage = dashboardPage ?? throw new ArgumentNullException(nameof(dashboardPage));
        _modulesPage = modulesPage ?? throw new ArgumentNullException(nameof(modulesPage));
        _runtimeStatusPage = runtimeStatusPage ?? throw new ArgumentNullException(nameof(runtimeStatusPage));
        _diagnosticsPage = diagnosticsPage ?? throw new ArgumentNullException(nameof(diagnosticsPage));
        _settingsPage = settingsPage ?? throw new ArgumentNullException(nameof(settingsPage));
        _aboutPage = aboutPage ?? throw new ArgumentNullException(nameof(aboutPage));

        // Create components
        _header = new AppHeader();
        _sidebar = new SidebarNavigation();
        _footer = new StatusFooter();
        _contentArea = new Panel();

        // Wire up header events
        _header.ThemeButtonClicked += (_, _) => ThemeToggled?.Invoke(this, EventArgs.Empty);
        _header.SettingsButtonClicked += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);

        // Wire up sidebar events
        _sidebar.NavigationRequested += (_, pageId) =>
        {
            _state.NavigateTo(pageId);
            NavigationRequested?.Invoke(this, pageId);
        };

        // Wire up page events
        _dashboardPage.NavigationRequested += (_, pageId) =>
        {
            _state.NavigateTo(pageId);
            NavigationRequested?.Invoke(this, pageId);
        };
        _dashboardPage.OpenLogDirectoryRequested += (_, _) => OpenLogDirectoryRequested?.Invoke(this, EventArgs.Empty);

        _diagnosticsPage.OpenLogDirectoryRequested += (_, _) => OpenLogDirectoryRequested?.Invoke(this, EventArgs.Empty);
        _diagnosticsPage.CopyDiagnosticsRequested += (_, _) => CopyDiagnosticsRequested?.Invoke(this, EventArgs.Empty);

        _settingsPage.OpenConfigDirectoryRequested += (_, _) => OpenConfigDirectoryRequested?.Invoke(this, EventArgs.Empty);

        // Wire up state changes
        _state.PageChanged += OnPageChanged;

        // Apply initial theme
        _themeManager.ThemeChanged += OnThemeChanged;

        // Build layout
        var mainLayout = new DynamicLayout
        {
            Spacing = new Size(0, 0),
            Padding = Padding.Empty
        };

        // Header row
        mainLayout.Add(_header);

        // Content row: sidebar + content area
        var contentRow = new DynamicLayout
        {
            Padding = Padding.Empty,
            Spacing = new Size(0, 0)
        };
        contentRow.AddRow(_sidebar, _contentArea);

        mainLayout.Add(contentRow, true); // True = scale
        mainLayout.Add(_footer);

        Content = mainLayout;

        // Apply initial theme and show default page
        ApplyTheme(_themeManager.CurrentPalette);
        ShowPage(_state.CurrentPage);
    }

    public void ApplyTheme(ThemePalette palette)
    {
        _header.ApplyTheme(palette);
        _sidebar.ApplyTheme(palette);
        _footer.ApplyTheme(palette);

        _dashboardPage.ApplyTheme(palette);
        _modulesPage.ApplyTheme(palette);
        _runtimeStatusPage.ApplyTheme(palette);
        _diagnosticsPage.ApplyTheme(palette);
        _settingsPage.ApplyTheme(palette);
        _aboutPage.ApplyTheme(palette);
    }

    public void ShowPage(NavigationPageId pageId)
    {
        _sidebar.SelectedPage = pageId;

        switch (pageId)
        {
            case NavigationPageId.Dashboard:
                _contentArea.Content = _dashboardPage;
                break;
            case NavigationPageId.Modules:
                _contentArea.Content = _modulesPage;
                break;
            case NavigationPageId.RuntimeStatus:
                _contentArea.Content = _runtimeStatusPage;
                break;
            case NavigationPageId.Diagnostics:
                _contentArea.Content = _diagnosticsPage;
                break;
            case NavigationPageId.Settings:
                _contentArea.Content = _settingsPage;
                break;
            case NavigationPageId.About:
                _contentArea.Content = _aboutPage;
                break;
            default:
                _contentArea.Content = _dashboardPage;
                break;
        }
    }

    private void OnPageChanged(object? sender, NavigationPageId pageId)
    {
        ShowPage(pageId);
    }

    private void OnThemeChanged(object? sender, ThemePalette palette)
    {
        ApplyTheme(palette);
    }
}
