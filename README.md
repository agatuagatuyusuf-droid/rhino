# RhinoCommercialPlatform

Commercial-grade modular Rhino 8 plug-in platform supporting Windows and macOS.

## Phase 00 — Foundation

This repository contains the Phase 00 commercial foundation for a Rhino 8 plug-in platform.
It establishes the build, test, and extensibility infrastructure only. No business-specific
functionality is included at this stage.

## Requirements

- Rhino 8 (Windows or macOS)
- .NET SDK 8.0
- Python 3.x (for validation scripts)

## Target Frameworks

- Plug-in: `net7.0` (Windows and macOS), `net48` (Windows only)
- Tests: `net8.0`

## Directory Structure

```
RhinoCommercialPlatform.sln
Directory.Build.props
Directory.Packages.props
global.json
src/
  RhinoCommercialPlatform.Core/          — Abstractions & runtime primitives (netstandard2.0)
  RhinoCommercialPlatform.Modules.Abstractions/ — Module system interfaces (netstandard2.0)
  RhinoCommercialPlatform.Infrastructure/ — File system, logging, paths (netstandard2.0)
  RhinoCommercialPlatform.Modules.Foundation/  — Foundation module (netstandard2.0)
  RhinoCommercialPlatform.Plugin/        — Rhino plug-in entry point (net7.0 + net48)
tests/
  RhinoCommercialPlatform.UnitTests/     — Unit tests (net8.0)
  RhinoCommercialPlatform.Foundation.SmokeTests/ — Foundation smoke test (net8.0)
tools/
  check_structure.py                     — File structure verification
  check_forbidden_files.py               — Forbidden file detection
  check_release_boundary.py              — Release artifact boundary check
  check_platform_compatibility.py        — Platform API compatibility check
  run_validation.ps1                     — Complete validation pipeline
docs/
  architecture.md                        — Architecture documentation
  phases.md                              — Phase roadmap
  cross-platform-policy.md               — Cross-platform development policy
  manual-rhino-smoke.md                  — Manual Rhino GUI smoke test index
  manual-rhino-smoke-windows.md          — Windows manual smoke test guide
  manual-rhino-smoke-macos.md            — macOS manual smoke test guide
.github/workflows/ci.yml                — CI pipeline (Windows + macOS)
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

## Status

> Phase 00 includes automated Windows and macOS validation pipelines. A specific commit is considered CI-verified only when both Windows and macOS runs have completed successfully. Rhino GUI loading remains a separate manual verification.

## What Phase 00 Does NOT Include

- License server / activation
- Auto-updater
- Business-specific modules
- WPF / Eto UI
- Web API
- Database
