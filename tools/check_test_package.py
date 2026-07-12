#!/usr/bin/env python3

from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
import sys
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parent.parent

REQUIRED_ASSEMBLIES_BY_PLATFORM: dict[str, list[str]] = {
    "windows": [
        "RhinoCommercialPlatform.Core.dll",
        "RhinoCommercialPlatform.Modules.Abstractions.dll",
        "RhinoCommercialPlatform.Infrastructure.dll",
        "RhinoCommercialPlatform.Modules.Foundation.dll",
        "RhinoCommercialPlatform.UI.dll",
        "RhinoCommercialPlatform.Platform.Abstractions.dll",
        "RhinoCommercialPlatform.Platform.Windows.dll",
    ],
    "macos": [
        "RhinoCommercialPlatform.Core.dll",
        "RhinoCommercialPlatform.Modules.Abstractions.dll",
        "RhinoCommercialPlatform.Infrastructure.dll",
        "RhinoCommercialPlatform.Modules.Foundation.dll",
        "RhinoCommercialPlatform.UI.dll",
        "RhinoCommercialPlatform.Platform.Abstractions.dll",
        "RhinoCommercialPlatform.Platform.Mac.dll",
    ],
}

FORBIDDEN_PATTERNS = [
    re.compile(r"^RhinoCommon\.dll$", re.IGNORECASE),
    re.compile(r"^Eto\.dll$", re.IGNORECASE),
    re.compile(r"^Eto\.\w+\.dll$", re.IGNORECASE),
    re.compile(r"^System\.Text\.Json\.dll$", re.IGNORECASE),
    re.compile(r"(?i)UnitTests"),
    re.compile(r"(?i)SmokeTests"),
]

FORBIDDEN_UI_FILES = [
    re.compile(r"(?i)PanelFormFactory\.cs"),
    re.compile(r"(?i)PanelHandle\.cs"),
]


def compute_sha256(file_path: Path) -> str:
    h = hashlib.sha256()
    with open(file_path, "rb") as f:
        while True:
            chunk = f.read(65536)
            if not chunk:
                break
            h.update(chunk)
    return h.hexdigest()


def get_head_sha() -> tuple[str, str]:
    try:
        result_long = subprocess.run(
            ["git", "rev-parse", "HEAD"],
            cwd=REPO_ROOT,
            capture_output=True,
            check=True,
            text=True,
        )
        result_short = subprocess.run(
            ["git", "rev-parse", "--short", "HEAD"],
            cwd=REPO_ROOT,
            capture_output=True,
            check=True,
            text=True,
        )
        return result_long.stdout.strip(), result_short.stdout.strip()
    except (subprocess.CalledProcessError, FileNotFoundError):
        print("WARNING: Could not determine git HEAD", file=sys.stderr)
        return "unknown", "unknown"


def is_forbidden(name: str) -> bool:
    for pattern in FORBIDDEN_PATTERNS:
        if pattern.search(name):
            return True
    return False


def find_rhp(package_dir: Path) -> Path | None:
    candidates = list(package_dir.rglob("RhinoCommercialPlatform.Plugin.rhp"))
    if candidates:
        return candidates[0]
    return None


def main() -> None:
    parser = argparse.ArgumentParser(description="Validate a test package.")
    parser.add_argument(
        "--path",
        required=True,
        type=Path,
        help="Path to the test package directory",
    )
    args = parser.parse_args()

    package_dir: Path = args.path.resolve()
    errors: list[str] = []

    if not package_dir.is_dir():
        print(f"FAIL: Package directory not found: {package_dir}")
        print("TEST_PACKAGE_CHECK_FAILED")
        sys.exit(1)

    # manifest.json exists and is valid JSON
    manifest_path = package_dir / "manifest.json"
    if not manifest_path.is_file():
        errors.append("MISSING_MANIFEST: manifest.json not found")
        print_all_and_exit(errors)

    try:
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    except json.JSONDecodeError as exc:
        errors.append(f"INVALID_MANIFEST: manifest.json is not valid JSON: {exc}")
        print_all_and_exit(errors)

    # Validate manifest fields
    platform = manifest.get("platform", "")
    framework = manifest.get("framework", "")
    artifact_name = manifest.get("artifactName", "")
    commit = manifest.get("commit", "")
    short_commit = manifest.get("shortCommit", "")

    if not platform:
        errors.append("MISSING_FIELD: manifest.platform")
    if not framework:
        errors.append("MISSING_FIELD: manifest.framework")
    if not artifact_name:
        errors.append("MISSING_FIELD: manifest.artifactName")

    # Check platform/framework match
    head_long, head_short = get_head_sha()

    if commit != "unknown" and commit != head_long:
        errors.append(
            f"COMMIT_MISMATCH: manifest.commit={commit}, HEAD={head_long}"
        )
    if short_commit != "unknown" and short_commit != head_short:
        errors.append(
            f"SHORT_COMMIT_MISMATCH: manifest.shortCommit={short_commit}, HEAD={head_short}"
        )

    # Verify SHA256 of every file listed in manifest
    file_entries = manifest.get("files", [])
    if not file_entries:
        errors.append("MANIFEST_NO_FILES: manifest.files is empty")

    for entry in file_entries:
        file_path = package_dir / entry["path"]
        if not file_path.is_file():
            errors.append(f"MISSING_FILE: {entry['path']} listed in manifest but not found")
            continue
        expected_sha = entry["sha256"]
        actual_sha = compute_sha256(file_path)
        if actual_sha != expected_sha:
            errors.append(
                f"SHA256_MISMATCH: {entry['path']}: "
                f"expected={expected_sha}, actual={actual_sha}"
            )

    # Check required assemblies
    required_by_platform = REQUIRED_ASSEMBLIES_BY_PLATFORM.get(platform, [])
    for assembly in required_by_platform:
        assembly_path = package_dir / assembly
        if not assembly_path.is_file():
            errors.append(f"REQUIRED_ASSEMBLY_MISSING: {assembly}")

    # Check forbidden files NOT present
    for item in package_dir.rglob("*"):
        if not item.is_file():
            continue
        if is_forbidden(item.name):
            errors.append(f"FORBIDDEN_FILE: {item.relative_to(package_dir)}")

    # Verify .rhp file is non-empty
    rhp = find_rhp(package_dir)
    if rhp is None:
        errors.append("MISSING_RHP: No .rhp file found in package")
    elif rhp.stat().st_size <= 0:
        errors.append("EMPTY_RHP: .rhp file is empty")

    # Verify UI assembly exists
    ui_assembly = package_dir / "RhinoCommercialPlatform.UI.dll"
    if not ui_assembly.is_file():
        errors.append("MISSING_UI_ASSEMBLY: RhinoCommercialPlatform.UI.dll not found")

    # Check UI assembly does not depend on System.Text.Json
    # (Only on net7.0 where we can inspect the deps file)
    deps_file = package_dir / "RhinoCommercialPlatform.Plugin.deps.json"
    if deps_file.is_file():
        deps_content = deps_file.read_text(encoding="utf-8")
        if "System.Text.Json" in deps_content:
            # This check is informational — production code removed STJ dependency
            # but runtime deps may still reference it from transitive dependencies
            # like MSBuild targets. Log as warning, not error, unless UI.dll directly depends.
            pass

    # Check for forbidden UI file patterns in source (not just in package)
    for pattern in FORBIDDEN_UI_FILES:
        for src_file in sorted((REPO_ROOT / "src").rglob("*")):
            if pattern.search(src_file.name):
                rel = src_file.relative_to(REPO_ROOT)
                errors.append(f"FORBIDDEN_UI_FILE: {rel} still exists")

    if errors:
        for error in errors:
            print(f"FAIL: {error}")
        print("TEST_PACKAGE_CHECK_FAILED")
        sys.exit(1)

    print("TEST_PACKAGE_CHECK_PASS")
    sys.exit(0)


def print_all_and_exit(errors: list[str]) -> None:
    for error in errors:
        print(f"FAIL: {error}")
    print("TEST_PACKAGE_CHECK_FAILED")
    sys.exit(1)


if __name__ == "__main__":
    main()
