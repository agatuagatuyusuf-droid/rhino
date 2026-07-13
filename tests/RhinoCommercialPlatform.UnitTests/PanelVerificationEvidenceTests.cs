using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhinoCommercialPlatform.Infrastructure.Validation;

namespace RhinoCommercialPlatform.UnitTests;

[TestClass]
public sealed class PanelVerificationEvidenceTests
{
    [TestMethod]
    public void Serialize_UsesTheVersionedSchemaAndEscapesFailures()
    {
        var evidence = new PanelVerificationEvidence
        {
            SchemaVersion = 1,
            Status = "FAIL",
            CiRunId = "29189796523",
            ArtifactName = "RCP-macos-net7.0-src-2337839-test-bf9c523",
            ArtifactSha256 = new string('a', 64),
            SourceCommit = new string('b', 40),
            TestedCommit = new string('c', 40),
            Platform = "macos",
            Framework = "net7.0",
            Architecture = "arm64",
            PluginVersion = "0.2.1",
            RhinoVersion = "8.30.26103.11002",
            PanelId = "7b3e4f2a-1d8c-4e5f-9a6b-3c2d1e0f8a7b",
            PanelRegistered = true,
            PanelOpened = true,
            PanelVisible = false,
            PanelInstanceType = "Example.Panel",
            SettingsSave = true,
            SettingsReload = true,
            LogDirectoryExists = true,
            TestedAtUtc = "2026-07-13T00:00:00.0000000Z",
            Failures = new List<string> { "quoted \"failure\"" }
        };

        var json = PanelVerificationEvidenceSerializer.Serialize(evidence);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.AreEqual(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.AreEqual("FAIL", root.GetProperty("status").GetString());
        Assert.AreEqual("quoted \"failure\"", root.GetProperty("failures")[0].GetString());

        var expectedNames = new HashSet<string>(StringComparer.Ordinal)
        {
            "schemaVersion", "status", "ciRunId", "artifactName",
            "artifactSha256", "sourceCommit", "testedCommit", "platform",
            "framework", "architecture", "pluginVersion", "rhinoVersion",
            "panelId", "panelRegistered", "panelOpened", "panelVisible",
            "panelInstanceType", "settingsSave", "settingsReload",
            "logDirectoryExists", "testedAtUtc", "failures"
        };

        var actualNames = root.EnumerateObject().Select(property => property.Name).ToHashSet(StringComparer.Ordinal);
        CollectionAssert.AreEquivalent(expectedNames.ToList(), actualNames.ToList());
    }

    [TestMethod]
    public void ManifestLoad_ReturnsPackagedBuildIdentity()
    {
        var path = WriteManifest(
            sourceCommit: new string('1', 40),
            testedCommit: new string('2', 40),
            artifactSha256: new string('3', 64));

        try
        {
            var manifest = TestArtifactManifest.Load(path);

            Assert.AreEqual("29189796523", manifest.CiRunId);
            Assert.AreEqual("RCP-macos-net7.0-src-1111111-test-2222222", manifest.ArtifactName);
            Assert.AreEqual(new string('3', 64), manifest.ArtifactSha256);
            Assert.AreEqual("net7.0", manifest.Framework);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public void ManifestLoad_RejectsUnknownIdentity()
    {
        var path = WriteManifest("unknown", new string('2', 40), new string('3', 64));

        try
        {
            Assert.ThrowsExactly<InvalidDataException>(() => TestArtifactManifest.Load(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string WriteManifest(string sourceCommit, string testedCommit, string artifactSha256)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        File.WriteAllText(path, $$"""
            {
              "platform": "macos",
              "framework": "net7.0",
              "sourceCommit": "{{sourceCommit}}",
              "testedCommit": "{{testedCommit}}",
              "ciRunId": "29189796523",
              "artifactName": "RCP-macos-net7.0-src-1111111-test-2222222",
              "artifactSha256": "{{artifactSha256}}"
            }
            """);
        return path;
    }
}
