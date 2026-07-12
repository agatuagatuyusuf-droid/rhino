#!/usr/bin/env python3
import subprocess
import sys
import os

result = subprocess.run(["git", "ls-files"], capture_output=True, text=True, cwd=os.path.dirname(os.path.abspath(__file__)) + "/..")
tracked = result.stdout.strip().split("\n")

forbidden_patterns = [
    "dist/", "build/", "release/", "*.exe", "*.zip", "*.7z", "*.rar",
    "*.msi", ".env", "*.key", "*.pfx", "*.p12", "release_private.pem",
    "tools/.dotnet-tools/", "tools/.obfuscar-temp/", "__pycache__/",
    "*.pyc", "*.log", "*.tmp", "bin/", "obj/",
]

errors = []

for f in tracked:
    f = f.strip()
    if not f:
        continue
    for pat in forbidden_patterns:
        if pat.startswith("*."):
            if f.endswith(pat[1:]):
                errors.append(f"FORBIDDEN: {f} matches {pat}")
        elif pat in f:
            errors.append(f"FORBIDDEN: {f} contains pattern '{pat}'")

if errors:
    for e in errors:
        print(f"FAIL: {e}")
    print("FORBIDDEN_FILE_CHECK_FAILED")
    sys.exit(1)
else:
    print("FORBIDDEN_FILE_CHECK_PASS")
    sys.exit(0)
