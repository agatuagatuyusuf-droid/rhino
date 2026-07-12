#!/usr/bin/env python3

from __future__ import annotations

import re
import sys
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parent.parent

SHARED_SOURCE_PATTERNS = [
    # Windows-only types and APIs
    (r'\busing\s+System\.Windows\b', 'using System.Windows'),
    (r'\busing\s+System\.Windows\.Forms\b', 'using System.Windows.Forms'),
    (r'\bMicrosoft\.Win32\.Registry\b', 'Microsoft.Win32.Registry'),
    # WPF / WinForms project settings
    (r'\bUseWPF\b', 'UseWPF'),
    (r'\bUseWindowsForms\b', 'UseWindowsForms'),
    # Platform-specific target frameworks in shared code
    (r'\bnet7\.0-windows\b', 'net7.0-windows'),
    (r'\bnet7\.0-macos\b', 'net7.0-macos'),
    # Hardcoded platform paths
    (r'kernel32\.dll', 'kernel32.dll'),
    (r'user32\.dll', 'user32.dll'),
    (r'shell32\.dll', 'shell32.dll'),
    (r'DllImport\(', 'DllImport('),
    (r'C:\\\\', 'C:\\'),
    (r'%LOCALAPPDATA%', '%LOCALAPPDATA%'),
    (r'/Applications/', '/Applications/'),
]

# These patterns are allowed in shared code
ALLOWED_PATTERNS = [
    r'System\.Runtime\.InteropServices',
    r'RuntimeInformation',
    r'ComVisible',
    r'Guid',
    r'OSPlatform\.Windows',
    r'OSPlatform\.OSX',
]

errors: list[str] = []


def is_shared_source(path: Path) -> bool:
    """Check if a file is in the shared src directories (non-plugin)."""
    try:
        rel = path.relative_to(REPO_ROOT)
    except ValueError:
        return False

    # Plugin project must be checked separately — it references Rhino
    # But we still check its .cs files for platform-specific patterns
    return True


def check_file(path: Path) -> None:
    """Check a single file for platform compatibility issues."""
    try:
        content = path.read_text(encoding='utf-8-sig')
    except (UnicodeDecodeError, OSError):
        return

    for pattern, label in SHARED_SOURCE_PATTERNS:
        if re.search(pattern, content):
            # Skip DllImport check if it's part of a safe interop declaration
            # that uses RuntimeInformation
            if label == 'DllImport(' and _is_safe_interop(content):
                continue
            errors.append(f"{label}: {path.relative_to(REPO_ROOT)}")


def _is_safe_interop(content: str) -> bool:
    """Check if DllImport usage is paired with platform check."""
    # This allows DllImport only if it's guarded or in platform-specific files
    # For now, we flag all DllImport in shared code
    return False


# Check all .cs and .csproj files in src/
src_dir = REPO_ROOT / 'src'
for path in sorted(src_dir.rglob('*')):
    if not path.is_file():
        continue
    if path.suffix.lower() not in {'.cs', '.csproj'}:
        continue
    check_file(path)

if errors:
    for error in sorted(set(errors)):
        print(f"FAIL: {error}")
    print("PLATFORM_COMPATIBILITY_CHECK_FAILED")
    sys.exit(1)

print("PLATFORM_COMPATIBILITY_CHECK_PASS")
sys.exit(0)
