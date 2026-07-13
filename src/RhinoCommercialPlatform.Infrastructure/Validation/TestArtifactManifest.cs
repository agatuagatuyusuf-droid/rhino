using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace RhinoCommercialPlatform.Infrastructure.Validation;

[DataContract]
public sealed class TestArtifactManifest
{
    [DataMember(Name = "platform")]
    public string Platform { get; set; } = string.Empty;

    [DataMember(Name = "framework")]
    public string Framework { get; set; } = string.Empty;

    [DataMember(Name = "sourceCommit")]
    public string SourceCommit { get; set; } = string.Empty;

    [DataMember(Name = "testedCommit")]
    public string TestedCommit { get; set; } = string.Empty;

    [DataMember(Name = "ciRunId")]
    public string CiRunId { get; set; } = string.Empty;

    [DataMember(Name = "artifactName")]
    public string ArtifactName { get; set; } = string.Empty;

    [DataMember(Name = "artifactSha256")]
    public string ArtifactSha256 { get; set; } = string.Empty;

    public static TestArtifactManifest Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new InvalidDataException("Artifact manifest was not found: " + path);
        }

        try
        {
            var serializer = new DataContractJsonSerializer(typeof(TestArtifactManifest));
            using (var stream = File.OpenRead(path))
            {
                var manifest = serializer.ReadObject(stream) as TestArtifactManifest;
                if (manifest == null)
                {
                    throw new InvalidDataException("Artifact manifest is empty.");
                }

                manifest.Validate();
                return manifest;
            }
        }
        catch (InvalidDataException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("Artifact manifest is invalid.", ex);
        }
    }

    public void ValidateLoadedAssembly(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        var testedCommit = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => string.Equals(attribute.Key, "TestedCommit", StringComparison.Ordinal))
            ?.Value;

        if (string.IsNullOrWhiteSpace(testedCommit))
            throw new InvalidDataException("Loaded plugin assembly has no TestedCommit metadata.");

        if (!string.Equals(TestedCommit, testedCommit, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Loaded plugin assembly does not match manifest.testedCommit.");
    }

    private void Validate()
    {
        RequireIdentity(SourceCommit, nameof(SourceCommit), 7);
        RequireIdentity(TestedCommit, nameof(TestedCommit), 7);
        RequireIdentity(CiRunId, nameof(CiRunId), 1);
        RequireIdentity(ArtifactName, nameof(ArtifactName), 1);
        RequireIdentity(Platform, nameof(Platform), 1);
        RequireIdentity(Framework, nameof(Framework), 1);
        RequireIdentity(ArtifactSha256, nameof(ArtifactSha256), 64);

        if (ArtifactSha256.Length != 64 || !ArtifactSha256.All(IsHexDigit))
        {
            throw new InvalidDataException("ArtifactSha256 must be a 64-character hexadecimal digest.");
        }
    }

    private static void RequireIdentity(string value, string name, int minimumLength)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            string.Equals(value, "unknown", StringComparison.OrdinalIgnoreCase) ||
            value.Length < minimumLength)
        {
            throw new InvalidDataException(name + " is missing or unknown.");
        }
    }

    private static bool IsHexDigit(char value)
    {
        return (value >= '0' && value <= '9') ||
               (value >= 'a' && value <= 'f') ||
               (value >= 'A' && value <= 'F');
    }
}
