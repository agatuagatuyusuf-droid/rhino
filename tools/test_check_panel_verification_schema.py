import copy
import json
import unittest
from pathlib import Path

from jsonschema import Draft7Validator, FormatChecker


REPO_ROOT = Path(__file__).resolve().parents[1]


class PanelVerificationSchemaTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls) -> None:
        schema = json.loads(
            (REPO_ROOT / "validation/rhino-panel-verification.schema.json").read_text(
                encoding="utf-8"
            )
        )
        cls.validator = Draft7Validator(schema, format_checker=FormatChecker())
        cls.valid_pass = json.loads(
            (REPO_ROOT / "validation/rhino-panel-verification.example.json").read_text(
                encoding="utf-8"
            )
        )

    def test_pass_requires_every_critical_boolean_to_be_true(self) -> None:
        critical_booleans = (
            "panelRegistered",
            "panelOpened",
            "panelVisible",
            "settingsSave",
            "settingsReload",
            "logDirectoryExists",
        )

        for field in critical_booleans:
            with self.subTest(field=field):
                evidence = copy.deepcopy(self.valid_pass)
                evidence[field] = False

                self.assertFalse(self.validator.is_valid(evidence))

    def test_pass_requires_no_failures(self) -> None:
        evidence = copy.deepcopy(self.valid_pass)
        evidence["failures"] = ["PanelVisible"]

        self.assertFalse(self.validator.is_valid(evidence))

    def test_fail_requires_at_least_one_failure(self) -> None:
        evidence = copy.deepcopy(self.valid_pass)
        evidence["status"] = "FAIL"

        self.assertFalse(self.validator.is_valid(evidence))

    def test_consistent_pass_and_fail_are_valid(self) -> None:
        valid_fail = copy.deepcopy(self.valid_pass)
        valid_fail["status"] = "FAIL"
        valid_fail["panelVisible"] = False
        valid_fail["failures"] = ["IsPanelVisible"]

        self.assertTrue(self.validator.is_valid(self.valid_pass))
        self.assertTrue(self.validator.is_valid(valid_fail))


if __name__ == "__main__":
    unittest.main()
