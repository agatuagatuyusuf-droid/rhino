#!/usr/bin/env python3

from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys
from datetime import datetime, timezone
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parent.parent
PLUGIN_RELEASE_ROOT = (
    REPO_ROOT
    / "src"
    / "RhinoCommercialPlatform.Plugin"
    / "bin"
    / "Release"
)
CI_ARTIFACTS_ROOT = REPO_ROOT / "ci-artifacts"

FORBIDDEN_FILE_PATTERNS = [
    re.compile(r"^RhinoCommon\.dll$", re.IGNORECASE),
    re.compile(r"^Eto\.dll$", re.IGNORECASE),
    re.compile(r"^Eto\.\w+\.dll$", re.IGNORECASE),
    re.compile(r"\.pdb$", re.IGNORECASE),
    re.compile(r"UnitTests", re.IGNORECASE),
    re.compile(r"SmokeTests", re.IGNORECASE),
]

FORBIDDEN_DIRECTORIES = {"obj", "bin"}


def compute_sha256(file_path: Path) -> str:
    h = hashlib.sha256()
    with open(file_path, "rb") as f:
        while True:
            chunk = f.read(65536)
            if not chunk:
                break
            h.update(chunk)
    return h.hexdigest()


def get_short_sha() -> str:
    try:
        result = subprocess.run(
            ["git", "rev-parse", "--short", "HEAD"],
            cwd=REPO_ROOT,
            capture_output=True,
            check=True,
            text=True,
        )
        return result.stdout.strip()
    except (subprocess.CalledProcessError, FileNotFoundError):
        print("WARNING: Could not determine git SHA, using 'unknown'", file=sys.stderr)
        return "unknown"


def get_long_sha() -> str:
    try:
        result = subprocess.run(
            ["git", "rev-parse", "HEAD"],
            cwd=REPO_ROOT,
            capture_output=True,
            check=True,
            text=True,
        )
        return result.stdout.strip()
    except (subprocess.CalledProcessError, FileNotFoundError):
        return "unknown"


def get_version() -> str:
    csproj_path = (
        REPO_ROOT
        / "src"
        / "RhinoCommercialPlatform.Plugin"
        / "RhinoCommercialPlatform.Plugin.csproj"
    )
    try:
        content = csproj_path.read_text(encoding="utf-8-sig")
        match = re.search(r"<Version>(.*?)</Version>", content)
        if match:
            return match.group(1)
    except Exception:
        pass
    return "0.0.0"


def is_forbidden(name: str) -> bool:
    for pattern in FORBIDDEN_FILE_PATTERNS:
        if pattern.search(name):
            return True
    return False


def should_exclude(path: Path, source_root: Path) -> bool:
    try:
        rel = path.relative_to(source_root)
    except ValueError:
        return True

    for part in rel.parts:
        if part in FORBIDDEN_DIRECTORIES:
            return True

    return is_forbidden(path.name)


def collect_files(source_dir: Path) -> list[Path]:
    files: list[Path] = []
    for path in sorted(source_dir.rglob("*")):
        if not path.is_file():
            continue
        if should_exclude(path, source_dir):
            continue
        files.append(path)
    return files


def copy_files(
    files: list[Path], source_root: Path, dest_root: Path
) -> list[tuple[str, str]]:
    manifest_entries: list[tuple[str, str]] = []
    for src_path in files:
        rel = src_path.relative_to(source_root)
        dest_path = dest_root / rel
        dest_path.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(src_path, dest_path)
        sha = compute_sha256(dest_path)
        manifest_entries.append((rel.as_posix(), sha))
    return manifest_entries


def create_manifest(
    artifact_name: str,
    platform: str,
    framework: str,
    configuration: str,
    files: list[tuple[str, str]],
) -> dict:
    return {
        "product": "RhinoCommercialPlatform",
        "version": get_version(),
        "platform": platform,
        "framework": framework,
        "configuration": configuration,
        "commit": get_long_sha(),
        "shortCommit": get_short_sha(),
        "buildUtc": datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
        "artifactName": artifact_name,
        "files": [{"path": p, "sha256": s} for p, s in files],
    }


def create_sha256sums(files: list[tuple[str, str]]) -> str:
    lines: list[str] = []
    for path, sha in files:
        lines.append(f"{sha}  {path}")
    lines.append("")
    return "\n".join(lines)


def create_readme(
    artifact_name: str,
    platform: str,
    framework: str,
) -> str:
    if platform == "windows":
        rhino_ext = ".rhp"
        install_dir = "%APPDATA%\\McNeel\\Rhino\\8.0\\Plug-ins\\RhinoCommercialPlatform"
        arch_hint = ""
    else:
        rhino_ext = ".rhp"
        install_dir = "~/Library/Application Support/McNeel/Rhino/8.0/Plug-ins/RhinoCommercialPlatform"
        arch_hint = " (macOS)"

    return f"""# RhinoCommercialPlatform Test Package

**Artifact:** {artifact_name}
**Platform:** {platform}{arch_hint}
**Framework:** {framework}

## Installation

1. Close Rhino if it is running.

2. Extract the contents of this package to the Rhino Plug-ins directory:

   **{install_dir}**

   Create the directory if it does not exist.

3. Launch Rhino and run the command:

   `RCP_OpenPanel`

   to open the main panel.

## Contents

This package contains the RhinoCommercialPlatform plugin ({artifact_name}{rhino_ext})
and all required dependencies. It is ready for side-by-side manual testing.

## Verification

A manifest.json and SHA256SUMS.txt are included. To verify integrity:

   $ cd {artifact_name}
   $ shasum -a 256 -c SHA256SUMS.txt

## Notes

- This is a test artifact and should NOT be used in production.
- The plugin targets Rhino 8 (or later).
"""


def main() -> None:
    parser = argparse.ArgumentParser(description="Package test artifacts.")
    parser.add_argument(
        "--platform",
        required=True,
        choices=["windows", "macos"],
        help="Target platform",
    )
    parser.add_argument(
        "--framework",
        required=True,
        choices=["net7.0", "net48"],
        help="Target framework",
    )
    parser.add_argument(
        "--configuration",
        default="Release",
        help="Build configuration (default: Release)",
    )
    args = parser.parse_args()

    if args.platform == "macos" and args.framework == "net48":
        print("ERROR: net48 is not supported on macOS")
        sys.exit(1)

    short_sha = get_short_sha()
    artifact_name = f"RCP-{args.platform}-{args.framework}-{short_sha}"
    source_dir = PLUGIN_RELEASE_ROOT / args.framework

    if not source_dir.is_dir():
        print(f"ERROR: Source directory not found: {source_dir}")
        sys.exit(1)

    rhp_files = list(source_dir.rglob("RhinoCommercialPlatform.Plugin.rhp"))
    if len(rhp_files) != 1:
        print(
            f"ERROR: Expected exactly one .rhp file, found {len(rhp_files)} in {source_dir}"
        )
        sys.exit(1)

    output_root = source_dir if rhp_files[0].parent == source_dir else rhp_files[0].parent

    dest_dir = CI_ARTIFACTS_ROOT / artifact_name
    if dest_dir.exists():
        shutil.rmtree(dest_dir)
    dest_dir.mkdir(parents=True, exist_ok=True)

    files = collect_files(output_root)
    manifest_files = copy_files(files, output_root, dest_dir)

    manifest = create_manifest(
        artifact_name=artifact_name,
        platform=args.platform,
        framework=args.framework,
        configuration=args.configuration,
        files=manifest_files,
    )

    manifest_path = dest_dir / "manifest.json"
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    # re-checksum manifest
    manifest_sha = compute_sha256(manifest_path)
    manifest_files.append(("manifest.json", manifest_sha))

    sha256_path = dest_dir / "SHA256SUMS.txt"
    sha256_path.write_text(create_sha256sums(manifest_files), encoding="utf-8")

    readme_path = dest_dir / "README-TESTING.md"
    readme_path.write_text(
        create_readme(artifact_name, args.platform, args.framework),
        encoding="utf-8",
    )

    total_size = sum(
        p.stat().st_size for p in dest_dir.rglob("*") if p.is_file()
    )
    file_count = len(list(dest_dir.rglob("*")))
    print(
        f"PACKAGED: {artifact_name}/  ({file_count} files, {total_size} bytes)"
    )
    print(f"OUTPUT: {dest_dir}")


if __name__ == "__main__":
    main()
