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


# === Basic project file checks ===

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

# === RhinoCommon only in Plugin project ===

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

# === Eto.Forms in UI project ===

require_contains(
    "src/RhinoCommercialPlatform.UI/RhinoCommercialPlatform.UI.csproj",
    ['PackageReference Include="Eto.Forms"'],
)

# === No WPF/WinForms ===

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

# === Key files exist ===

require_file("src/RhinoCommercialPlatform.Plugin/Commands/OpenMainPanelCommand.cs")
require_file("src/RhinoCommercialPlatform.Plugin/Panels/MainPanelRegistration.cs")
require_file("src/RhinoCommercialPlatform.UI/Shell/MainPanelView.cs")

pages = ["Dashboard", "Modules", "RuntimeStatus", "Diagnostics", "Settings", "About"]
for page in pages:
    require_file(f"src/RhinoCommercialPlatform.UI/Pages/{page}Page.cs")

require_file("src/RhinoCommercialPlatform.UI/Settings/UserSettingsService.cs")
require_file("src/RhinoCommercialPlatform.UI/Theme/ThemeManager.cs")
require_file("src/RhinoCommercialPlatform.UI/Diagnostics/DiagnosticReportService.cs")

# === Native Rhino Panel checks ===

plugin_panels_dir = REPO_ROOT / "src/RhinoCommercialPlatform.Plugin/Panels"

# 1. MainPanelRegistration must use gateway.RegisterPanel (via IRhinoPanelGateway)
require_contains(
    "src/RhinoCommercialPlatform.Plugin/Panels/MainPanelRegistration.cs",
    ["gateway.RegisterPanel"],
)

# 2. Must use gateway.OpenPanel (via IRhinoPanelGateway)
require_contains(
    "src/RhinoCommercialPlatform.Plugin/Panels/MainPanelRegistration.cs",
    ["gateway.OpenPanel"],
)

# 3. RhinoMainPanelHost exists and inherits Eto.Forms.Panel
rhino_host = read_text("src/RhinoCommercialPlatform.Plugin/Panels/RhinoMainPanelHost.cs")
if ": Panel" not in rhino_host and "class RhinoMainPanelHost" not in rhino_host:
    errors.append("RhinoMainPanelHost must inherit Panel (Eto.Forms.Panel)")

# 4. RhinoMainPanelHost has GuidAttribute
if "[Guid(\"7B3E4F2A-1D8C-4E5F-9A6B-3C2D1E0F8A7B\")]" not in rhino_host:
    errors.append("RhinoMainPanelHost must have GuidAttribute with the well-known GUID")

# 5. PanelFormFactory.cs is deleted
panel_form_factory = plugin_panels_dir.parent.parent / "UI/Shell/PanelFormFactory.cs"
if panel_form_factory.exists():
    errors.append("PanelFormFactory.cs still exists — must be deleted")

# 6. PanelHandle.cs is deleted
panel_handle = plugin_panels_dir / "PanelHandle.cs"
if panel_handle.exists():
    errors.append("PanelHandle.cs still exists — must be deleted")

# 7. No Eto.Form creation in Plugin/Panels
for cs_file in sorted(plugin_panels_dir.rglob("*.cs")):
    content = cs_file.read_text(encoding="utf-8-sig")
    if "new Form(" in content or "new Eto.Forms.Form" in content:
        errors.append(f"FORBIDDEN_FORM_CREATION: {cs_file.relative_to(REPO_ROOT)} creates Eto Form directly")

# 8. MainPanelView does not reference RhinoCommon
main_panel_view = read_text("src/RhinoCommercialPlatform.UI/Shell/MainPanelView.cs")
if "RhinoCommon" in main_panel_view or "using Rhino;" in main_panel_view:
    errors.append("MainPanelView must not reference RhinoCommon")

# 9. Register does not just print log
main_reg = read_text("src/RhinoCommercialPlatform.Plugin/Panels/MainPanelRegistration.cs")
if "registration prepared" in main_reg.lower() and "RegisterPanel" not in main_reg:
    errors.append("Register() must call RegisterPanel, not just log a message")

# 10. Only Plugin references RhinoCommon
# (already checked above)

# 11. System.Text.Json not referenced in UI csproj
ui_csproj = read_text("src/RhinoCommercialPlatform.UI/RhinoCommercialPlatform.UI.csproj")
if "System.Text.Json" in ui_csproj:
    errors.append("UI project must not reference System.Text.Json")

# 12. Directory.Packages.props does not have System.Text.Json
packages_props = read_text("Directory.Packages.props")
if "System.Text.Json" in packages_props:
    errors.append("Directory.Packages.props must not reference System.Text.Json")

# === Validation schema file ===

require_file("validation/rhino-panel-verification.schema.json")

# === Output ===

if errors:
    for error in errors:
        print(f"FAIL: {error}")
    print("UI_STRUCTURE_CHECK_FAILED")
    sys.exit(1)

print("UI_STRUCTURE_CHECK_PASS")
sys.exit(0)
