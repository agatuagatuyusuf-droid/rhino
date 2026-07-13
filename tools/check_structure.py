#!/usr/bin/env python3

from __future__ import annotations

import sys
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parent.parent

REQUIRED_FILES = [
    "RhinoCommercialPlatform.sln",
    "Directory.Build.props",
    "Directory.Packages.props",
    "global.json",
    ".editorconfig",
    ".gitignore",
    "README.md",
    ".github/workflows/ci.yml",
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
    "src/RhinoCommercialPlatform.Plugin/Commands/VerifyPanelCommand.cs",
    "src/RhinoCommercialPlatform.Plugin/Properties/AssemblyInfo.cs",
    "src/RhinoCommercialPlatform.Platform.Abstractions/IRhinoPanelGateway.cs",
    "src/RhinoCommercialPlatform.Platform.Abstractions/ISystemThemeProvider.cs",
    "src/RhinoCommercialPlatform.Platform.Abstractions/IExternalLauncher.cs",
    "src/RhinoCommercialPlatform.Plugin/Panels/MainPanelRegistration.cs",
    "src/RhinoCommercialPlatform.Plugin/Panels/RhinoPanelGateway.cs",
    "src/RhinoCommercialPlatform.Plugin/Panels/RhinoMainPanelHost.cs",
    "tests/RhinoCommercialPlatform.UnitTests/RhinoCommercialPlatform.UnitTests.csproj",
    "tests/RhinoCommercialPlatform.UnitTests/AppPathsTests.cs",
    "tests/RhinoCommercialPlatform.UnitTests/SecretRedactorTests.cs",
    "tests/RhinoCommercialPlatform.UnitTests/ModuleRegistryTests.cs",
    "tests/RhinoCommercialPlatform.UnitTests/FileAppLoggerTests.cs",
    "tests/RhinoCommercialPlatform.UnitTests/Phase01Tests.cs",
    "tests/RhinoCommercialPlatform.Foundation.SmokeTests/RhinoCommercialPlatform.Foundation.SmokeTests.csproj",
    "tests/RhinoCommercialPlatform.Foundation.SmokeTests/Program.cs",
    "tools/check_structure.py",
    "tools/check_forbidden_files.py",
    "tools/check_release_boundary.py",
    "tools/check_validation_failfast.ps1",
    "tools/run_validation.ps1",
    "tools/check_platform_compatibility.py",
    "tools/check_test_package.py",
    "tools/package_test_artifacts.py",
    "docs/architecture.md",
    "docs/phases.md",
    "docs/manual-rhino-smoke.md",
    "docs/cross-platform-policy.md",
    "docs/manual-rhino-smoke-windows.md",
    "docs/manual-rhino-smoke-macos.md",
    "validation/rhino-panel-verification.schema.json",
]

EXPECTED_PROJECTS = [
    r"src\RhinoCommercialPlatform.Core\RhinoCommercialPlatform.Core.csproj",
    r"src\RhinoCommercialPlatform.Modules.Abstractions\RhinoCommercialPlatform.Modules.Abstractions.csproj",
    r"src\RhinoCommercialPlatform.Infrastructure\RhinoCommercialPlatform.Infrastructure.csproj",
    r"src\RhinoCommercialPlatform.Modules.Foundation\RhinoCommercialPlatform.Modules.Foundation.csproj",
    r"src\RhinoCommercialPlatform.Plugin\RhinoCommercialPlatform.Plugin.csproj",
    r"src\RhinoCommercialPlatform.Platform.Abstractions\RhinoCommercialPlatform.Platform.Abstractions.csproj",
    r"tests\RhinoCommercialPlatform.UnitTests\RhinoCommercialPlatform.UnitTests.csproj",
    r"tests\RhinoCommercialPlatform.Foundation.SmokeTests\RhinoCommercialPlatform.Foundation.SmokeTests.csproj",
]

errors: list[str] = []


def read_text(relative_path: str) -> str:
    path = REPO_ROOT / relative_path
    try:
        return path.read_text(encoding="utf-8-sig")
    except Exception as exc:
        errors.append(f"READ_FAILED: {relative_path}: {exc}")
        return ""


def require_contains(relative_path: str, required_values: list[str]) -> None:
    content = read_text(relative_path)
    for value in required_values:
        if value not in content:
            errors.append(
                f"MISSING_CONTENT: {relative_path} does not contain {value!r}"
            )


def require_not_contains(relative_path: str, forbidden_values: list[str]) -> None:
    content = read_text(relative_path)
    for value in forbidden_values:
        if value in content:
            errors.append(
                f"FORBIDDEN_CONTENT: {relative_path} contains {value!r}"
            )


for relative_path in REQUIRED_FILES:
    full_path = REPO_ROOT / relative_path
    if not full_path.is_file():
        errors.append(f"MISSING_FILE: {relative_path}")
    elif full_path.stat().st_size == 0:
        errors.append(f"EMPTY_FILE: {relative_path}")

solution = read_text("RhinoCommercialPlatform.sln")
for project in EXPECTED_PROJECTS:
    if project not in solution:
        errors.append(f"SOLUTION_PROJECT_MISSING: {project}")

require_contains(
    "src/RhinoCommercialPlatform.Plugin/RhinoCommercialPlatform.Plugin.csproj",
    [
        "<TargetFrameworks>net7.0;net48</TargetFrameworks>",
        "<EnableDynamicLoading>true</EnableDynamicLoading>",
        "<TargetExt>.rhp</TargetExt>",
        'PackageReference Include="RhinoCommon"',
        'PrivateAssets="all"',
        'ExcludeAssets="runtime"',
        "RhinoCommercialPlatform.Core.csproj",
        "RhinoCommercialPlatform.Modules.Abstractions.csproj",
        "RhinoCommercialPlatform.Infrastructure.csproj",
        "RhinoCommercialPlatform.Modules.Foundation.csproj",
    ],
)

require_not_contains(
    "src/RhinoCommercialPlatform.Plugin/RhinoCommercialPlatform.Plugin.csproj",
    [
        "<HintPath>",
        "C:\\Program Files\\Rhino",
        "C:/Program Files/Rhino",
    ],
)

require_contains(
    "src/RhinoCommercialPlatform.Plugin/RhinoCommercialPlatformPlugin.cs",
    [
        "AppRuntime.Start()",
        "OnLoad",
        "OnShutdown",
        "finally",
        "Runtime = null",
        "Instance = null",
    ],
)

require_contains(
    "src/RhinoCommercialPlatform.Plugin/AppRuntime.cs",
    [
        "new FoundationModule()",
        "modules.InitializeAll(context)",
        "Modules.ShutdownAll()",
        "AggregateException",
        "RuntimePlatformInfo",
        "IPlatformInfo",
    ],
)

require_contains(
    "src/RhinoCommercialPlatform.Plugin/Commands/PlatformStatusCommand.cs",
    [
        'EnglishName => "RCP_Status"',
        "RhinoCommercialPlatformPlugin.Instance",
        "plugin.Runtime",
        "runtime.Modules.Modules",
        "Product:",
        "Version:",
        "Mode:",
        "Logs:",
        "Modules:",
        "Operating System:",
        "Process Architecture:",
        "Framework:",
    ],
)

require_not_contains(
    "src/RhinoCommercialPlatform.Plugin/Commands/PlatformStatusCommand.cs",
    [
        "new AppRuntime",
        "AppRuntime.Start()",
    ],
)

require_contains(
    "src/RhinoCommercialPlatform.Plugin/Commands/VerifyPanelCommand.cs",
    [
        'EnglishName => "RCP_VerifyPanel"',
        "MainPanelRegistration",
        "EnsureRegistered",
        "RCP_VERIFY:",
        "RCP_PANEL_VERIFY_PASS",
        "RCP_PANEL_VERIFY_FAIL",
    ],
)

require_contains(
    "src/RhinoCommercialPlatform.Platform.Abstractions/IRhinoPanelGateway.cs",
    ["GetPanel"],
)

require_not_contains(
    "src/RhinoCommercialPlatform.Platform.Abstractions/IRhinoPanelGateway.cs",
    ["IsPanelRegistered"],
)

require_contains(
    "src/RhinoCommercialPlatform.Plugin/Panels/MainPanelRegistration.cs",
    [
        "IRhinoPanelGateway",
        "EnsureRegistered",
        "throw new InvalidOperationException",
        "RegistrationState",
        "Succeeded",
        "Failed",
    ],
)

require_contains(
    "src/RhinoCommercialPlatform.Plugin/Panels/MainPanelRegistration.cs",
    [
        "private static RegistrationState _state",
        "internal static void SetGateway",
        "EnsureRegistered()",
        "IsPanelVisible",
        "GetPanel",
    ],
)

require_not_contains(
    "src/RhinoCommercialPlatform.Plugin/Panels/MainPanelRegistration.cs",
    ["IsPanelRegistered("],
)

require_contains(
    "src/RhinoCommercialPlatform.Plugin/Panels/RhinoPanelGateway.cs",
    [
        "IRhinoPanelGateway",
        "Rhino.UI.Panels",
    ],
)

require_contains(
    ".github/workflows/ci.yml",
    [
        "windows-latest",
        "macos-14",
        "tools/run_validation.ps1",
        "actions/checkout@v4",
        "actions/setup-dotnet@v4",
        "actions/setup-python@v5",
        "SOURCE_COMMIT",
        "TESTED_COMMIT",
        "${{ github.event.pull_request.head.sha || github.sha }}",
        "${{ github.sha }}",
        "src-${{",
        "-test-${{",
    ],
)

require_contains(
    ".gitignore",
    ["rhino-panel-verification.json"],
)

require_not_contains(
    ".github/workflows/ci.yml",
    [
        "continue-on-error: true",
        "powershell -ExecutionPolicy",
        'echo "tests passed"',
    ],
)

require_contains(
    "tools/run_validation.ps1",
    [
        "Invoke-NativeChecked",
        "$LASTEXITCODE",
        "check_validation_failfast.ps1",
        "check_platform_compatibility.py",
        "dotnet",
        "test",
        "RhinoCommercialPlatform.Foundation.SmokeTests.csproj",
        "check_release_boundary.py",
        "VALIDATION_PASS",
        "platform=$Platform",
    ],
)

require_contains(
    "tests/RhinoCommercialPlatform.Foundation.SmokeTests/Program.cs",
    [
        "FoundationModule",
        "InitializeAll",
        "ShutdownAll",
        "File.ReadAllText",
        "FOUNDATION_SMOKE_PASS",
        "secret-value",
        "[REDACTED]",
        "RuntimePlatformInfo",
        "OperatingSystem",
    ],
)

require_contains(
    "validation/rhino-panel-verification.schema.json",
    [
        "panelRegistered",
        "panelOpened",
        "panelVisible",
        "artifactSha256",
        "sourceCommit",
        "testedCommit",
        "Rhino Panel Verification Evidence",
    ],
)

non_rhino_projects = [
    "src/RhinoCommercialPlatform.Core",
    "src/RhinoCommercialPlatform.Modules.Abstractions",
    "src/RhinoCommercialPlatform.Infrastructure",
    "src/RhinoCommercialPlatform.Modules.Foundation",
    "src/RhinoCommercialPlatform.Platform.Abstractions",
]

for project_directory in non_rhino_projects:
    directory = REPO_ROOT / project_directory
    if not directory.is_dir():
        continue

    for path in directory.rglob("*"):
        if not path.is_file():
            continue
        if path.suffix.lower() not in {".cs", ".csproj", ".props"}:
            continue

        content = path.read_text(encoding="utf-8-sig")
        if "RhinoCommon" in content or "using Rhino;" in content:
            errors.append(
                f"RHINO_DEPENDENCY_OUTSIDE_PLUGIN: {path.relative_to(REPO_ROOT)}"
            )

source_files = list((REPO_ROOT / "src").rglob("*.cs"))
for source_file in source_files:
    content = source_file.read_text(encoding="utf-8-sig")
    suspicious_patterns = [
        "catch { /* Ignore",
        "catch { /* Last resort",
        "catch { /* Shutdown errors",
    ]

    for pattern in suspicious_patterns:
        if pattern in content:
            errors.append(
                f"SILENT_CATCH: {source_file.relative_to(REPO_ROOT)} contains {pattern!r}"
            )

if errors:
    for error in errors:
        print(f"FAIL: {error}")
    print("STRUCTURE_CHECK_FAILED")
    sys.exit(1)

print("STRUCTURE_CHECK_PASS")
sys.exit(0)
