#!/usr/bin/env python3

from __future__ import annotations

import argparse
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

WINDOWS_TARGETS = ["net7.0", "net48"]
MACOS_TARGETS = ["net7.0"]

errors: list[str] = []


def check_target(target_root: Path) -> None:
    if not target_root.is_dir():
        errors.append(f"TARGET_OUTPUT_MISSING: {target_root}")
        return

    rhp_candidates = list(
        target_root.rglob("RhinoCommercialPlatform.Plugin.rhp")
    )

    if len(rhp_candidates) != 1:
        errors.append(
            f"EXPECTED_ONE_RHP: {target_root.name}: found={len(rhp_candidates)}"
        )
        return

    rhp_path = rhp_candidates[0]

    if rhp_path.stat().st_size <= 0:
        errors.append(f"EMPTY_RHP: {rhp_path}")
        return

    output_directory = rhp_path.parent

    output_files = {
        path.name: path
        for path in output_directory.rglob("*")
        if path.is_file()
    }

    for forbidden_name in FORBIDDEN_FILENAMES:
        if forbidden_name in output_files:
            errors.append(
                f"FORBIDDEN_RELEASE_FILE: {target_root.name}: "
                f"{output_files[forbidden_name]}"
            )

    for required_name in REQUIRED_ASSEMBLIES:
        if required_name not in output_files:
            errors.append(
                f"REQUIRED_RELEASE_FILE_MISSING: {target_root.name}: "
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


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "--platform",
        required=True,
        choices=["windows", "macos"],
        help="Target platform for release boundary check",
    )
    args = parser.parse_args()

    if args.platform == "windows":
        targets = WINDOWS_TARGETS
    else:
        targets = MACOS_TARGETS

    for target in targets:
        target_root = PLUGIN_RELEASE_ROOT / target
        check_target(target_root)

    if errors:
        for error in errors:
            print(f"FAIL: {error}")
        print("RELEASE_BOUNDARY_CHECK_FAILED")
        sys.exit(1)

    print("RELEASE_BOUNDARY_CHECK_PASS")
    sys.exit(0)


if __name__ == "__main__":
    main()
