#!/usr/bin/env python3
import sys
import os

REQUIRED_FILES = [
    "RhinoCommercialPlatform.sln",
    "Directory.Build.props",
    "Directory.Packages.props",
    "global.json",
    ".editorconfig",
    ".gitignore",
    "README.md",
    "src/RhinoCommercialPlatform.Core/RhinoCommercialPlatform.Core.csproj",
    "src/RhinoCommercialPlatform.Core/Abstractions/IAppLogger.cs",
    "src/RhinoCommercialPlatform.Core/Abstractions/IAppPaths.cs",
    "src/RhinoCommercialPlatform.Core/Abstractions/IClock.cs",
    "src/RhinoCommercialPlatform.Core/Runtime/RuntimeMode.cs",
    "src/RhinoCommercialPlatform.Core/Runtime/PluginMetadata.cs",
    "src/RhinoCommercialPlatform.Core/Runtime/SystemClock.cs",
    "src/RhinoCommercialPlatform.Modules.Abstractions/RhinoCommercialPlatform.Modules.Abstractions.csproj",
    "src/RhinoCommercialPlatform.Modules.Abstractions/IPluginModule.cs",
    "src/RhinoCommercialPlatform.Modules.Abstractions/IModuleContext.cs",
    "src/RhinoCommercialPlatform.Modules.Abstractions/ModuleContext.cs",
    "src/RhinoCommercialPlatform.Modules.Abstractions/ModuleRegistry.cs",
    "src/RhinoCommercialPlatform.Modules.Abstractions/DuplicateModuleException.cs",
    "src/RhinoCommercialPlatform.Infrastructure/RhinoCommercialPlatform.Infrastructure.csproj",
    "src/RhinoCommercialPlatform.Infrastructure/AppPaths.cs",
    "src/RhinoCommercialPlatform.Infrastructure/FileAppLogger.cs",
    "src/RhinoCommercialPlatform.Infrastructure/SecretRedactor.cs",
    "src/RhinoCommercialPlatform.Modules.Foundation/RhinoCommercialPlatform.Modules.Foundation.csproj",
    "src/RhinoCommercialPlatform.Modules.Foundation/FoundationModule.cs",
    "src/RhinoCommercialPlatform.Plugin/RhinoCommercialPlatform.Plugin.csproj",
    "src/RhinoCommercialPlatform.Plugin/RhinoCommercialPlatformPlugin.cs",
    "src/RhinoCommercialPlatform.Plugin/AppRuntime.cs",
    "src/RhinoCommercialPlatform.Plugin/Commands/PlatformStatusCommand.cs",
    "src/RhinoCommercialPlatform.Plugin/Properties/AssemblyInfo.cs",
    "tests/RhinoCommercialPlatform.UnitTests/RhinoCommercialPlatform.UnitTests.csproj",
    "tests/RhinoCommercialPlatform.UnitTests/AppPathsTests.cs",
    "tests/RhinoCommercialPlatform.UnitTests/SecretRedactorTests.cs",
    "tests/RhinoCommercialPlatform.UnitTests/ModuleRegistryTests.cs",
    "tests/RhinoCommercialPlatform.UnitTests/FileAppLoggerTests.cs",
    "tests/RhinoCommercialPlatform.Foundation.SmokeTests/RhinoCommercialPlatform.Foundation.SmokeTests.csproj",
    "tests/RhinoCommercialPlatform.Foundation.SmokeTests/Program.cs",
    "tools/check_structure.py",
    "tools/check_forbidden_files.py",
    "tools/check_release_boundary.py",
    "tools/run_validation.ps1",
    "docs/architecture.md",
    "docs/phases.md",
    "docs/manual-rhino-smoke.md",
    ".github/workflows/ci.yml",
]

errors = []

for f in REQUIRED_FILES:
    full = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", f)
    full = os.path.normpath(full)
    if not os.path.isfile(full):
        errors.append(f"MISSING: {f}")
    elif os.path.getsize(full) == 0:
        errors.append(f"EMPTY: {f}")

if errors:
    for e in errors:
        print(f"FAIL: {e}")
    print("STRUCTURE_CHECK_FAILED")
    sys.exit(1)
else:
    print("STRUCTURE_CHECK_PASS")
    sys.exit(0)
