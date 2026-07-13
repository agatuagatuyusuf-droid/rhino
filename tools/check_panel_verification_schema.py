#!/usr/bin/env python3

import argparse
import json
from pathlib import Path

from jsonschema import Draft7Validator, FormatChecker


def main() -> None:
    parser = argparse.ArgumentParser(description="Validate Rhino panel evidence JSON.")
    parser.add_argument("path", type=Path)
    parser.add_argument(
        "--schema",
        type=Path,
        default=Path("validation/rhino-panel-verification.schema.json"),
    )
    args = parser.parse_args()

    schema = json.loads(args.schema.read_text(encoding="utf-8"))
    evidence = json.loads(args.path.read_text(encoding="utf-8"))
    validator = Draft7Validator(schema, format_checker=FormatChecker())
    errors = sorted(validator.iter_errors(evidence), key=lambda error: list(error.path))
    if errors:
        for error in errors:
            location = ".".join(str(part) for part in error.path) or "$"
            print(f"FAIL: {location}: {error.message}")
        raise SystemExit(1)

    print("PANEL_VERIFICATION_SCHEMA_PASS")


if __name__ == "__main__":
    main()
