#!/usr/bin/env python3

from __future__ import annotations

import sys
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parent.parent

errors: list[str] = []


def read_text(relative_path: str) -> str:
    path = REPO_ROOT / relative_path
    try:
        return path.read_text(encoding="utf-8-sig")
    except Exception as exc:
        errors.append(f"READ_FAILED: {relative_path}: {exc}")
        return ""


def require_file(relative_path: str) -> None:
    full_path = REPO_ROOT / relative_path
    if not full_path.is_file():
        errors.append(f"MISSING_FILE: {relative_path}")
    elif full_path.stat().st_size == 0:
        errors.append(f"EMPTY_FILE: {relative_path}")


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


require_file("src/RhinoCommercialPlatform.UI/RhinoCommercialPlatform.UI.csproj")

require_contains(
    "RhinoCommercialPlatform.sln",
    ["RhinoCommercialPlatform.UI.csproj"],
)

require_contains(
    "src/RhinoCommercialPlatform.Plugin/RhinoCommercialPlatform.Plugin.csproj",
    ["RhinoCommercialPlatform.UI.csproj"],
)

require_not_contains(
    "src/RhinoCommercialPlatform.UI/RhinoCommercialPlatform.UI.csproj",
    ["RhinoCommon"],
)

for csproj in sorted((REPO_ROOT / "src").rglob("*.csproj")):
    rel = csproj.relative_to(REPO_ROOT).as_posix()
    if "Plugin" in rel:
        continue
    content = read_text(rel)
    if "RhinoCommon" in content:
        errors.append(f"RHINOCOMMON_OUTSIDE_PLUGIN: {rel}")

for source_file in sorted((REPO_ROOT / "src").rglob("*.cs")):
    rel = source_file.relative_to(REPO_ROOT).as_posix()
    if "Plugin" in rel:
        continue
    content = read_text(rel)
    if "RhinoCommon" in content or "using Rhino;" in content:
        errors.append(f"RHINOCOMMON_OUTSIDE_PLUGIN: {rel}")

require_contains(
    "src/RhinoCommercialPlatform.UI/RhinoCommercialPlatform.UI.csproj",
    ['PackageReference Include="Eto.Forms"'],
)

for csproj in sorted((REPO_ROOT / "src").rglob("*.csproj")):
    rel = csproj.relative_to(REPO_ROOT).as_posix()
    content = read_text(rel)
    if any(p in content for p in ["UseWPF", "UseWindowsForms", "PresentationFramework"]):
        errors.append(f"WPF_WINFORMS_REFERENCE: {rel}")

for source_file in sorted((REPO_ROOT / "src").rglob("*.cs")):
    rel = source_file.relative_to(REPO_ROOT).as_posix()
    content = read_text(rel)
    if "using System.Windows;" in content or "using System.Windows.Forms;" in content:
        errors.append(f"WPF_WINFORMS_USING: {rel}")

require_file("src/RhinoCommercialPlatform.Plugin/Commands/OpenMainPanelCommand.cs")
require_file("src/RhinoCommercialPlatform.Plugin/Panels/MainPanelRegistration.cs")
require_file("src/RhinoCommercialPlatform.UI/Shell/MainPanelView.cs")

pages = ["Dashboard", "Modules", "RuntimeStatus", "Diagnostics", "Settings", "About"]
for page in pages:
    require_file(f"src/RhinoCommercialPlatform.UI/Pages/{page}Page.cs")

require_file("src/RhinoCommercialPlatform.UI/Settings/UserSettingsService.cs")
require_file("src/RhinoCommercialPlatform.UI/Theme/ThemeManager.cs")
require_file("src/RhinoCommercialPlatform.UI/Diagnostics/DiagnosticReportService.cs")

if errors:
    for error in errors:
        print(f"FAIL: {error}")
    print("UI_STRUCTURE_CHECK_FAILED")
    sys.exit(1)

print("UI_STRUCTURE_CHECK_PASS")
sys.exit(0)
