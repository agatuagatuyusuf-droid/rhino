#!/usr/bin/env python3
import sys
import os

BASE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.normpath(os.path.join(BASE, ".."))

TARGETS = ["net48", "net7.0-windows"]
REQUIRED_ASSEMBLIES = [
    "RhinoCommercialPlatform.Core.dll",
    "RhinoCommercialPlatform.Modules.Abstractions.dll",
    "RhinoCommercialPlatform.Infrastructure.dll",
    "RhinoCommercialPlatform.Modules.Foundation.dll",
]
FORBIDDEN_FILES = [
    "RhinoCommon.dll",
    "RhinoCommercialPlatform.UnitTests.dll",
    "RhinoCommercialPlatform.Foundation.SmokeTests.dll",
]

errors = []

for target in TARGETS:
    plugin_dir = os.path.join(REPO, "src", "RhinoCommercialPlatform.Plugin", "bin", "Release", target)

    if not os.path.isdir(plugin_dir):
        errors.append(f"Build output directory not found: {plugin_dir}")
        continue

    rhp = os.path.join(plugin_dir, "RhinoCommercialPlatform.Plugin.rhp")
    if not os.path.isfile(rhp):
        errors.append(f"Missing .rhp in {target}: {rhp}")
    elif os.path.getsize(rhp) == 0:
        errors.append(f"Empty .rhp in {target}: {rhp}")

    for forbidden in FORBIDDEN_FILES:
        fpath = os.path.join(plugin_dir, forbidden)
        if os.path.isfile(fpath):
            errors.append(f"Forbidden file present in {target}: {forbidden}")

    for req in REQUIRED_ASSEMBLIES:
        fpath = os.path.join(plugin_dir, req)
        if not os.path.isfile(fpath):
            errors.append(f"Missing required assembly in {target}: {req}")

if errors:
    for e in errors:
        print(f"FAIL: {e}")
    print("RELEASE_BOUNDARY_CHECK_FAILED")
    sys.exit(1)
else:
    print("RELEASE_BOUNDARY_CHECK_PASS")
    sys.exit(0)
