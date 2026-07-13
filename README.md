# RhinoCommercialPlatform

Commercial-grade modular Rhino 8 plug-in platform supporting Windows and macOS.

## Phase 01 — Cross-Platform UI

This repository contains the Phase 01 commercial foundation for a Rhino 8 plug-in platform,
including a cross-platform Eto.Forms main panel with settings persistence and theme support.

## Requirements

- Rhino 8 (Windows or macOS)
- .NET SDK 8.0
- Python 3.x (for validation scripts)

## Target Frameworks

- Plug-in: `net7.0` (Windows and macOS), `net48` (Windows only)
- Tests: `net8.0`
- Shared libraries: `netstandard2.0`

## Directory Structure

```
RhinoCommercialPlatform.sln
Directory.Build.props
Directory.Packages.props
global.json
src/
  RhinoCommercialPlatform.Core/                    — Abstractions & runtime primitives (netstandard2.0)
  RhinoCommercialPlatform.Modules.Abstractions/    — Module system interfaces (netstandard2.0)
  RhinoCommercialPlatform.Infrastructure/          — File system, logging, paths (netstandard2.0)
  RhinoCommercialPlatform.Modules.Foundation/      — Foundation module (netstandard2.0)
  RhinoCommercialPlatform.Platform.Abstractions/   — Platform abstraction interfaces (netstandard2.0)
  RhinoCommercialPlatform.Platform.Windows/        — Windows platform adapters (netstandard2.0)
  RhinoCommercialPlatform.Platform.Mac/            — macOS platform adapters (netstandard2.0)
  RhinoCommercialPlatform.UI/                     — Cross-platform Eto.Forms UI (netstandard2.0)
  RhinoCommercialPlatform.Plugin/                 — Rhino plug-in entry point (net7.0 + net48)
tests/
  RhinoCommercialPlatform.UnitTests/              — Unit tests (net8.0)
  RhinoCommercialPlatform.Foundation.SmokeTests/  — Foundation smoke test (net8.0)
tools/
  check_structure.py                              — File structure verification
  check_forbidden_files.py                        — Forbidden file detection
  check_release_boundary.py                       — Release artifact boundary check
  check_platform_compatibility.py                 — Platform API compatibility check
  run_validation.ps1                              — Complete validation pipeline
docs/
  architecture.md                                 — Architecture documentation
  phases.md                                       — Phase roadmap
  cross-platform-policy.md                        — Cross-platform development policy
  ui-architecture.md                              — UI architecture documentation
  ui-visual-spec.md                               — UI visual specification
  manual-rhino-smoke.md                           — Manual Rhino GUI smoke test index
  manual-rhino-smoke-windows.md                   — Windows manual smoke test guide
  manual-rhino-smoke-macos.md                     — macOS manual smoke test guide
  manual-main-panel-smoke-windows.md              — Windows main panel smoke test guide
  manual-main-panel-smoke-macos.md                — macOS main panel smoke test guide
  test-package-installation.md                    — CI test package installation guide
  rhino-smoke-evidence.md                         — GUI verification evidence template
validation/
  rhino-gui-status.json                           — Rhino GUI verification status tracker
.github/workflows/ci.yml                         — CI pipeline (Windows + macOS)
```

## Build & Test

```powershell
# Restore
dotnet restore RhinoCommercialPlatform.sln

# Build (Windows)
dotnet build RhinoCommercialPlatform.sln -c Release

# Build (macOS, net7.0 only)
dotnet build src/RhinoCommercialPlatform.Plugin/RhinoCommercialPlatform.Plugin.csproj -c Release -f net7.0

# Run unit tests
dotnet test tests/RhinoCommercialPlatform.UnitTests/RhinoCommercialPlatform.UnitTests.csproj -c Release

# Run foundation smoke test
dotnet run --project tests/RhinoCommercialPlatform.Foundation.SmokeTests/RhinoCommercialPlatform.Foundation.SmokeTests.csproj -c Release

# Full validation
pwsh -NoProfile -File tools/run_validation.ps1
```

## Commands

- `RCP_Status` — Displays runtime status including product name, version, mode, log path, loaded modules, and platform information.
- `RCP_OpenPanel` — Opens the cross-platform Eto.Forms main panel with navigation, settings, diagnostics, and theme support.

## UI

The main panel (`RCP_OpenPanel`) provides:
- **Dashboard** — Overview of product, platform, module, and path information
- **Modules** — List of registered modules with initialization status
- **Runtime Status** — Runtime uptime, Rhino document info, and version details
- **Diagnostics** — Log viewer, diagnostic report generation, and system information
- **Settings** — Theme (System/Light/Dark), auto-open, remember-last-page, log level
- **About** — Product version, platform info, build info, and licensing placeholders

Theme modes cycle via the header toggle button: System → Light → Dark → System.
Settings are persisted as JSON with atomic writes and survive Rhino restarts.

## Status

> Phase 01 code is implemented, but release closeout is pending. Automated CI does not replace the required Windows net7/net48 and macOS net7 Rhino GUI evidence tracked in `validation/rhino-gui-status.json`.

## What Phase 01 Does NOT Include

- License server / activation
- Auto-updater
- Business-specific modules
- Web API
- Database
