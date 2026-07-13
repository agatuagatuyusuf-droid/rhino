# UI Architecture

## Cross-Platform Approach

The UI uses **Eto.Forms** — a cross-platform GUI toolkit that renders native controls on each platform:
- **Windows**: Native WinForms/WPF rendering
- **macOS**: Native AppKit (Cocoa) rendering

All UI code resides in `src/RhinoCommercialPlatform.UI/` targeting `netstandard2.0`, ensuring the same binary runs on both platforms. Platform-specific adapters (theme detection, file launcher) live in `Platform.Windows` and `Platform.Mac` projects behind interfaces in `Platform.Abstractions`.

## Project Structure and Dependencies

```
RhinoCommercialPlatform.UI (netstandard2.0)
├── Components/          — Reusable UI widgets
├── Diagnostics/         — Diagnostic snapshot and report services
├── Pages/               — Full-page views (one per navigation tab)
├── Settings/            — User settings persistence (JSON)
├── Shell/               — Panel controller, state, factory, view
├── Theme/               — Color palette, theme modes, design tokens
└── ViewModels/          — View-models for each page
```

Dependencies:
- `RhinoCommercialPlatform.Core` — PluginMetadata, IPlatformInfo, IAppPaths, IAppLogger
- `RhinoCommercialPlatform.Infrastructure` — FileAppLogger, AppPaths, RuntimePlatformInfo
- `RhinoCommercialPlatform.Modules.Abstractions` — ModuleRegistry, IPluginModule
- `RhinoCommercialPlatform.Platform.Abstractions` — ISystemThemeProvider, IExternalLauncher
- **Eto.Forms** — Cross-platform GUI framework
- **System.Text.Json** — Settings serialization

The Plugin project creates the `AppRuntime` which constructs all UI services and wires them together in `MainPanelService`.

## Panel Lifecycle

```
Rhino loads plugin
  └─ Plugin.OnLoad
       └─ AppRuntime.Start()
            ├─ Create logger, paths, platform, modules
            └─ Create UI services: UserSettingsService, ThemeManager, DiagnosticReportService
       └─ MainPanelRegistration.Register()
       └─ HandleAutoOpen() (if setting enabled)

User runs RCP_OpenPanel
  └─ OpenMainPanelCommand.RunCommand()
       └─ MainPanelRegistration.OpenPanel()
            └─ MainPanelService.GetOrCreatePanelView()
                 ├─ new MainShellState()
                 ├─ new MainPanelController(state, themeManager, settingsService)
                 ├─ Create pages with their view-models
                 └─ MainPanelController.CreatePanelView(...)
                      └─ new MainPanelView(state, themeManager, ...)
                           └─ PanelFormFactory.CreateAndShowForm(view, title)
                                └─ Eto Form created, shown, lifecycle starts

Panel closing (Form.Closing)
  └─ Cancel close, hide instead
  └─ PanelFormFactory.CloseForm() (explicit call)

Plugin.OnShutdown
  └─ MainPanelService.Dispose()
  └─ AppRuntime.Dispose()
       └─ Modules.ShutdownAll()
       └─ Logger cleanup
```

## Theme System

Three modes managed by `ThemeManager`:
- **System** — Detects platform theme via `ISystemThemeProvider` (falls back to Light)
- **Light** — Fixed light palette
- **Dark** — Fixed dark palette (respects macOS dark mode when System mode used)

`ThemePalette` defines ~30 color tokens covering backgrounds, text, interactive states, status colors, navigation, and buttons. Each component implements `ApplyTheme(ThemePalette)` to re-color itself. Theme changes trigger an event that propagates to `MainPanelView`, which cascades to all child components and pages.

`ThemeMode` cycles: System → Light → Dark → System (via header toggle button). Theme preference is persisted in user settings.

## Settings Persistence

**Schema**: `UserSettings` with fields:
- `SchemaVersion`, `ThemeMode`, `AutoOpenPanel`, `RememberLastPage`, `LastPage`
- `LogLevel`, `Language`, `UpdatedAtUtc`

**Storage**: JSON file at `{ConfigDirectory}/user-settings.json`, using `System.Text.Json`.

**Service**: `UserSettingsService` (implements `IUserSettingsService`):
- Atomic writes via temp-file + `File.Replace`
- Graceful fallback to defaults on read/parse errors
- Sanitization of all loaded values against known valid sets
- `Load()`, `Save()`, `ResetToDefaults()` operations

**Flow**: `MainPanelController` loads settings on construction, applies theme and last-page state, and saves on theme toggle or page navigation.

## Navigation Model

`NavigationPageId` enum:
- `Dashboard`, `Modules`, `RuntimeStatus`, `Diagnostics`, `Settings`, `About`

`MainShellState` holds current/previous page and fires `PageChanged` events.

`SidebarNavigation` renders all navigation items and emits `NavigationRequested` events.

`MainPanelView.ShowPage(pageId)` switches `_contentArea.Content` to the corresponding page.

`MainPanelController` wires navigation events, persists last-page when `RememberLastPage` is enabled, and provides `GetLastPageOrDefault()` for restoring on panel open.

`DashboardPage` includes quick-entry buttons that navigate directly to other pages.

## Key Design Decisions

- **No direct Eto dependency in Plugin project** — `IPanelView` abstracts the panel; `PanelFormFactory` encapsulates all Eto Form creation; `PanelHandle` wraps form visibility.
- **Constructor injection** throughout — no static service locator.
- **Pages are eager-created** when the panel is first opened (small number of pages, fast construction).
- **Form hides on close** (Cancel=true) rather than destroying — `CloseForm()` is called explicitly during shutdown.
- **Settings write is best-effort** — save failures during navigation/theme-toggle are silently caught.
