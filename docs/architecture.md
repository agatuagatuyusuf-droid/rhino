# Architecture

## Dependency Direction

```
RhinoCommercialPlatform.Core
    ↑
RhinoCommercialPlatform.Modules.Abstractions
    ↑
RhinoCommercialPlatform.Infrastructure
    ↑
RhinoCommercialPlatform.Modules.Foundation
    ↑
RhinoCommercialPlatform.Platform.Abstractions          RhinoCommercialPlatform.UI
    ↑                                                       ↓
RhinoCommercialPlatform.Platform.Windows     ─────→    RhinoCommercialPlatform.Plugin
RhinoCommercialPlatform.Platform.Mac         ─────→           ↑
    ↑                                                       (references UI + Platform)
    └───────────────────────────────────────────────────────┘
```

- **Core** — Pure abstractions and runtime primitives. No RhinoCommon dependency.
- **Modules.Abstractions** — Module system interfaces and registry. No RhinoCommon dependency.
- **Infrastructure** — File system, logging, redaction, paths. No RhinoCommon dependency.
- **Modules.Foundation** — First concrete module. No RhinoCommon dependency.
- **Platform.Abstractions** — Interfaces for platform-specific capabilities (theme detection, external launcher). No RhinoCommon dependency.
- **Platform.Windows** — Windows implementation of platform abstractions (theme detection via registry placeholder, external launcher via `Process.Start`).
- **Platform.Mac** — macOS implementation of platform abstractions (theme detection placeholder, external launcher via `Process.Start`).
- **UI** — Cross-platform Eto.Forms panel with pages, settings, theme system, and diagnostics. Depends on Core, Infrastructure, Modules.Abstractions, and Platform.Abstractions. No RhinoCommon dependency.
- **Plugin** — Rhino adaptation layer. Only this project references RhinoCommon. Wires all projects together via `AppRuntime`.

## Cross-Platform

- Plugin targets: `net7.0` (Windows + macOS), `net48` (Windows only)
- All shared projects (including UI) target `netstandard2.0`
- Platform detection uses `System.Runtime.InteropServices.RuntimeInformation`
- Platform adapters live in `Platform.Windows` and `Platform.Mac` projects
- UI uses Eto.Forms for cross-platform rendering

## Project Descriptions

### RhinoCommercialPlatform.UI

Cross-platform Eto.Forms panel providing the main user interface. Contains:
- **Shell** — Panel controller (`MainPanelController`), state (`MainShellState`), view (`MainPanelView`), form factory (`PanelFormFactory`), and navigation model
- **Pages** — Six pages: Dashboard, Modules, RuntimeStatus, Diagnostics, Settings, About
- **Components** — Reusable widgets: AppHeader, SidebarNavigation, StatusFooter, InfoCard, EmptyStateView, ErrorStateView
- **ViewModels** — View-models for each page (DashboardViewModel, ModulesViewModel, RuntimeStatusViewModel, DiagnosticsViewModel, SettingsViewModel, AboutViewModel)
- **Settings** — User settings persistence (`UserSettingsService`) with JSON file storage, atomic writes, and sanitization
- **Theme** — Theme manager (`ThemeManager`), color palette (`ThemePalette`) with Light/Dark/System modes, and design tokens (`UiTokens`)
- **Diagnostics** — Diagnostic report service (`DiagnosticReportService`) and log reader (`RecentLogReader`)

### RhinoCommercialPlatform.Platform.Abstractions

Interfaces for platform-specific functionality:
- `ISystemThemeProvider` — Detects current OS theme (Light/Dark)
- `IExternalLauncher` — Opens directories in the platform file manager

### RhinoCommercialPlatform.Platform.Windows

Windows implementation of platform abstractions:
- `WindowsSystemThemeProvider` — Returns `SystemTheme.Light` (no P/Invoke registry access)
- `WindowsExternalLauncher` — Opens directories via `Process.Start("explorer.exe", path)`

### RhinoCommercialPlatform.Platform.Mac

macOS implementation of platform abstractions:
- `MacSystemThemeProvider` — Returns `SystemTheme.Light` (no AppKit dependency)
- `MacExternalLauncher` — Opens directories via `Process.Start("open", path)`

## Key Design Decisions

- All business features are added as modules under `Modules.*`.
- Each module has standard `Initialize`/`Shutdown` lifecycle managed by `ModuleRegistry`.
- Logging is centralized in Infrastructure; all modules use `IAppLogger`.
- Plugin project is the only entry point; it creates `AppRuntime` on load.
- No static service locator; dependencies are injected via constructor.
- UI is fully cross-platform with Eto.Forms; no WPF or AppKit code.
- Settings are persisted as JSON with atomic writes for data integrity.
- Theme system supports System/Light/Dark with per-component `ApplyTheme()` pattern.
