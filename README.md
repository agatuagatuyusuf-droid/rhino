# RhinoCommercialPlatform

Commercial-grade modular Rhino 8 plug-in platform.

## Phase 00 — Foundation

This repository contains the Phase 00 commercial foundation for a Rhino 8 plug-in platform.
It establishes the build, test, and extensibility infrastructure only. No business-specific
functionality is included at this stage.

## Requirements

- Rhino 8 (Windows)
- .NET SDK 8.0
- Python 3.x (for validation scripts)

## Target Frameworks

- Plug-in: `net48`, `net7.0-windows`
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
  RhinoCommercialPlatform.Plugin/        — Rhino plug-in entry point (net48 + net7.0-windows)
tests/
  RhinoCommercialPlatform.UnitTests/     — Unit tests (net8.0)
  RhinoCommercialPlatform.Foundation.SmokeTests/ — Foundation smoke test (net8.0)
tools/
  check_structure.py                     — File structure verification
  check_forbidden_files.py               — Forbidden file detection
  check_release_boundary.py              — Release artifact boundary check
  run_validation.ps1                     — Complete validation pipeline
docs/
  architecture.md                        — Architecture documentation
  phases.md                              — Phase roadmap
  manual-rhino-smoke.md                  — Manual Rhino GUI smoke test guide
.github/workflows/ci.yml                — CI pipeline
```

## Build & Test

```powershell
# Restore
dotnet restore RhinoCommercialPlatform.sln

# Build
dotnet build RhinoCommercialPlatform.sln -c Release

# Run unit tests
dotnet test tests/RhinoCommercialPlatform.UnitTests/RhinoCommercialPlatform.UnitTests.csproj -c Release

# Run foundation smoke test
dotnet run --project tests/RhinoCommercialPlatform.Foundation.SmokeTests/RhinoCommercialPlatform.Foundation.SmokeTests.csproj -c Release

# Full validation
powershell -ExecutionPolicy Bypass -File tools/run_validation.ps1
```

## Commands

- `RCP_Status` — Displays runtime status including product name, version, mode, log path, and loaded modules.

## Status

> Release build and foundation smoke are CI-verified. Rhino GUI load verification requires a Windows environment with Rhino 8 installed and is not part of automated CI at this phase.

## What Phase 00 Does NOT Include

- License server / activation
- Auto-updater
- Business-specific modules
- WPF UI
- Web API
- Database
