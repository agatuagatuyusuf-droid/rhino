import unittest

import package_test_artifacts


class PackageTestArtifactsTests(unittest.TestCase):
    def test_artifact_digest_is_stable_and_order_independent(self) -> None:
        files = [("b.rhp", "2" * 64), ("a.dll", "1" * 64)]

        digest = package_test_artifacts.compute_artifact_sha256(files)

        self.assertEqual(
            "72410e60cf2de9d1884c5505d31315bc454add04e913ba05ac16288e6347ec9a",
            digest,
        )

    def test_manifest_contains_runtime_verification_identity(self) -> None:
        manifest = package_test_artifacts.create_manifest(
            artifact_name="RCP-macos-net7.0-src-1111111-test-2222222",
            platform="macos",
            framework="net7.0",
            configuration="Release",
            source_commit="1" * 40,
            tested_commit="2" * 40,
            ci_run_id="29189796523",
            github_event="pull_request",
            files=[("plugin.rhp", "3" * 64)],
        )

        self.assertEqual("29189796523", manifest["ciRunId"])
        self.assertEqual(
            package_test_artifacts.compute_artifact_sha256(manifest["files"]),
            manifest["artifactSha256"],
        )


if __name__ == "__main__":
    unittest.main()
