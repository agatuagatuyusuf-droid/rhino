#!/usr/bin/env python3

from __future__ import annotations

import sys
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parent.parent
PLUGIN_RELEASE_ROOT = (
    REPO_ROOT
    / "src"
    / "RhinoCommercialPlatform.Plugin"
    / "bin"
    / "Release"
)

TARGETS = [
    "net48",
    "net7.0-windows",
]

REQUIRED_ASSEMBLIES = {
    "RhinoCommercialPlatform.Core.dll",
    "RhinoCommercialPlatform.Modules.Abstractions.dll",
    "RhinoCommercialPlatform.Infrastructure.dll",
    "RhinoCommercialPlatform.Modules.Foundation.dll",
}

FORBIDDEN_FILENAMES = {
    "RhinoCommon.dll",
    "RhinoCommercialPlatform.UnitTests.dll",
    "RhinoCommercialPlatform.Foundation.SmokeTests.dll",
}

errors: list[str] = []

for target in TARGETS:
    target_root = PLUGIN_RELEASE_ROOT / target

    if not target_root.is_dir():
        errors.append(f"TARGET_OUTPUT_MISSING: {target_root}")
        continue

    rhp_candidates = list(
        target_root.rglob("RhinoCommercialPlatform.Plugin.rhp")
    )

    if len(rhp_candidates) != 1:
        errors.append(
            f"EXPECTED_ONE_RHP: {target}: found={len(rhp_candidates)}"
        )
        continue

    rhp_path = rhp_candidates[0]

    if rhp_path.stat().st_size <= 0:
        errors.append(f"EMPTY_RHP: {rhp_path}")
        continue

    output_directory = rhp_path.parent

    output_files = {
        path.name: path
        for path in output_directory.rglob("*")
        if path.is_file()
    }

    for forbidden_name in FORBIDDEN_FILENAMES:
        if forbidden_name in output_files:
            errors.append(
                f"FORBIDDEN_RELEASE_FILE: {target}: "
                f"{output_files[forbidden_name]}"
            )

    for required_name in REQUIRED_ASSEMBLIES:
        if required_name not in output_files:
            errors.append(
                f"REQUIRED_RELEASE_FILE_MISSING: {target}: "
                f"{required_name}"
            )

    for path in output_directory.rglob("*"):
        if not path.is_file():
            continue

        lower_name = path.name.lower()

        if lower_name.endswith(".zip"):
            errors.append(f"UNEXPECTED_ARCHIVE_IN_OUTPUT: {path}")

        if "unittests" in lower_name or "smoketests" in lower_name:
            errors.append(f"TEST_ARTIFACT_IN_PLUGIN_OUTPUT: {path}")

if errors:
    for error in errors:
        print(f"FAIL: {error}")
    print("RELEASE_BOUNDARY_CHECK_FAILED")
    sys.exit(1)

print("RELEASE_BOUNDARY_CHECK_PASS")
sys.exit(0)
