#!/usr/bin/env python3

from __future__ import annotations

import argparse
import hashlib
import json
import re
import subprocess
import sys
from pathlib import Path

from package_test_artifacts import compute_artifact_sha256


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
    parser.add_argument(
        "--commit",
        default=None,
        help="Expected tested commit SHA (optional; auto-detected if not provided)",
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
    source_commit = manifest.get("sourceCommit", "")
    source_commit_short = manifest.get("sourceCommitShort", "")
    tested_commit = manifest.get("testedCommit", "")
    tested_commit_short = manifest.get("testedCommitShort", "")
    ci_run_id = manifest.get("ciRunId", "")
    artifact_sha256 = manifest.get("artifactSha256", "")
    github_event = manifest.get("githubEvent", "")

    if not platform:
        errors.append("MISSING_FIELD: manifest.platform")
    if not framework:
        errors.append("MISSING_FIELD: manifest.framework")
    if not artifact_name:
        errors.append("MISSING_FIELD: manifest.artifactName")
    if not source_commit:
        errors.append("MISSING_FIELD: manifest.sourceCommit")
    if not source_commit_short:
        errors.append("MISSING_FIELD: manifest.sourceCommitShort")
    if not tested_commit:
        errors.append("MISSING_FIELD: manifest.testedCommit")
    if not tested_commit_short:
        errors.append("MISSING_FIELD: manifest.testedCommitShort")
    if not ci_run_id:
        errors.append("MISSING_FIELD: manifest.ciRunId")
    if not artifact_sha256:
        errors.append("MISSING_FIELD: manifest.artifactSha256")
    if not github_event:
        errors.append("MISSING_FIELD: manifest.githubEvent")

    identity_fields = {
        "sourceCommit": source_commit,
        "testedCommit": tested_commit,
        "ciRunId": ci_run_id,
        "artifactName": artifact_name,
        "artifactSha256": artifact_sha256,
    }
    for name, value in identity_fields.items():
        if str(value).lower() == "unknown":
            errors.append(f"UNKNOWN_IDENTITY: manifest.{name}")

    # Verify artifact name matches expected format
    expected_artifact = f"RCP-{platform}-{framework}-src-{source_commit_short}-test-{tested_commit_short}"
    if artifact_name != expected_artifact:
        errors.append(
            f"ARTIFACT_NAME_MISMATCH: expected={expected_artifact}, actual={artifact_name}"
        )

    # Verify tested commit against expected
    expected_commit = args.commit
    if expected_commit:
        if tested_commit != expected_commit:
            errors.append(
                f"TESTED_COMMIT_MISMATCH: manifest.testedCommit={tested_commit}, expected={expected_commit}"
            )
        if tested_commit_short != expected_commit[:7]:
            errors.append(
                f"TESTED_COMMIT_SHORT_MISMATCH: manifest.testedCommitShort={tested_commit_short}, expected={expected_commit[:7]}"
            )

    # Verify SHA256 of every file listed in manifest
    file_entries = manifest.get("files", [])
    if not file_entries:
        errors.append("MANIFEST_NO_FILES: manifest.files is empty")
    elif compute_artifact_sha256(file_entries) != artifact_sha256:
        errors.append("ARTIFACT_SHA256_MISMATCH: manifest artifact digest is invalid")

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
