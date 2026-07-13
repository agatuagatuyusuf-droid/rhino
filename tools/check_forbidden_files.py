#!/usr/bin/env python3

from __future__ import annotations

import re
import subprocess
import sys
from pathlib import Path, PurePosixPath


REPO_ROOT = Path(__file__).resolve().parent.parent
SELF_PATH = "tools/check_forbidden_files.py"

FORBIDDEN_DIRECTORIES = {
    "dist",
    "build",
    "release",
    "bin",
    "obj",
    ".vs",
    ".idea",
    "TestResults",
    "coverage",
    "artifacts",
    "__pycache__",
    ".dotnet-tools",
    ".obfuscar-temp",
}

FORBIDDEN_EXTENSIONS = {
    ".exe",
    ".zip",
    ".7z",
    ".rar",
    ".msi",
    ".key",
    ".pfx",
    ".p12",
    ".pyc",
    ".log",
    ".tmp",
    ".dll",
    ".pdb",
    ".rhp",
    ".yak",
}

FORBIDDEN_FILENAMES = {
    ".env",
    "release_private.pem",
}

SENSITIVE_PATH_WORDS = {
    "合同",
    "授权书",
}

TEXT_EXTENSIONS = {
    "",
    ".cs",
    ".csproj",
    ".props",
    ".targets",
    ".json",
    ".yml",
    ".yaml",
    ".md",
    ".txt",
    ".ps1",
    ".py",
    ".sln",
    ".editorconfig",
    ".gitignore",
}

SECRET_PATTERNS = [
    (
        "PRIVATE_KEY",
        re.compile(
            r"-{5}BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-{5}",
            re.IGNORECASE,
        ),
    ),
    (
        "HIGH_RISK_SECRET_ASSIGNMENT",
        re.compile(
            r"""(?im)^\s*
            (?:AWS_SECRET_ACCESS_KEY|GITHUB_TOKEN|LICENSE_PRIVATE_KEY|
            RELEASE_PRIVATE_KEY|JWT_PRIVATE_KEY)
            \s*[:=]\s*
            ["']?
            [A-Za-z0-9_./+=-]{16,}
            ["']?
            \s*$
            """,
            re.VERBOSE,
        ),
    ),
]

errors: list[str] = []


def get_tracked_files() -> list[str]:
    try:
        result = subprocess.run(
            ["git", "ls-files", "-z"],
            cwd=REPO_ROOT,
            capture_output=True,
            check=True,
        )
    except FileNotFoundError:
        errors.append("GIT_NOT_FOUND")
        return []
    except subprocess.CalledProcessError as exc:
        stderr = exc.stderr.decode("utf-8", errors="replace")
        errors.append(
            f"GIT_LS_FILES_FAILED: exit={exc.returncode}: {stderr.strip()}"
        )
        return []

    decoded = result.stdout.decode("utf-8", errors="strict")
    return [item for item in decoded.split("\0") if item]


tracked_files = get_tracked_files()

if not tracked_files and not errors:
    errors.append("NO_TRACKED_FILES_FOUND")

for tracked_file in tracked_files:
    normalized = tracked_file.replace("\\", "/")
    path = PurePosixPath(normalized)

    if any(part in FORBIDDEN_DIRECTORIES for part in path.parts):
        errors.append(f"FORBIDDEN_DIRECTORY: {normalized}")

    if path.suffix.lower() in FORBIDDEN_EXTENSIONS:
        errors.append(f"FORBIDDEN_EXTENSION: {normalized}")

    if path.name in FORBIDDEN_FILENAMES:
        errors.append(f"FORBIDDEN_FILENAME: {normalized}")

    if any(word in normalized for word in SENSITIVE_PATH_WORDS):
        errors.append(f"SENSITIVE_DOCUMENT_PATH: {normalized}")

    if normalized == SELF_PATH:
        continue

    disk_path = REPO_ROOT / Path(normalized)

    if not disk_path.is_file():
        errors.append(f"TRACKED_FILE_MISSING_FROM_WORKTREE: {normalized}")
        continue

    if disk_path.stat().st_size > 2 * 1024 * 1024:
        continue

    if disk_path.suffix.lower() not in TEXT_EXTENSIONS:
        continue

    try:
        content = disk_path.read_text(encoding="utf-8-sig")
    except UnicodeDecodeError:
        continue
    except OSError as exc:
        errors.append(f"READ_FAILED: {normalized}: {exc}")
        continue

    for label, pattern in SECRET_PATTERNS:
        if pattern.search(content):
            errors.append(f"{label}: {normalized}")

if errors:
    for error in sorted(set(errors)):
        print(f"FAIL: {error}")
    print("FORBIDDEN_FILE_CHECK_FAILED")
    sys.exit(1)

print("FORBIDDEN_FILE_CHECK_PASS")
sys.exit(0)
