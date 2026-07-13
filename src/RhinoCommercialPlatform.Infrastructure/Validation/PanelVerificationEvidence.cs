using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace RhinoCommercialPlatform.Infrastructure.Validation;

[DataContract]
public sealed class PanelVerificationEvidence
{
    [DataMember(Name = "schemaVersion", Order = 1)]
    public int SchemaVersion { get; set; }

    [DataMember(Name = "status", Order = 2)]
    public string Status { get; set; } = string.Empty;

    [DataMember(Name = "ciRunId", Order = 3)]
    public string CiRunId { get; set; } = string.Empty;

    [DataMember(Name = "artifactName", Order = 4)]
    public string ArtifactName { get; set; } = string.Empty;

    [DataMember(Name = "artifactSha256", Order = 5)]
    public string ArtifactSha256 { get; set; } = string.Empty;

    [DataMember(Name = "sourceCommit", Order = 6)]
    public string SourceCommit { get; set; } = string.Empty;

    [DataMember(Name = "testedCommit", Order = 7)]
    public string TestedCommit { get; set; } = string.Empty;

    [DataMember(Name = "platform", Order = 8)]
    public string Platform { get; set; } = string.Empty;

    [DataMember(Name = "framework", Order = 9)]
    public string Framework { get; set; } = string.Empty;

    [DataMember(Name = "architecture", Order = 10)]
    public string Architecture { get; set; } = string.Empty;

    [DataMember(Name = "pluginVersion", Order = 11)]
    public string PluginVersion { get; set; } = string.Empty;

    [DataMember(Name = "rhinoVersion", Order = 12)]
    public string RhinoVersion { get; set; } = string.Empty;

    [DataMember(Name = "panelId", Order = 13)]
    public string PanelId { get; set; } = string.Empty;

    [DataMember(Name = "panelRegistered", Order = 14)]
    public bool PanelRegistered { get; set; }

    [DataMember(Name = "panelOpened", Order = 15)]
    public bool PanelOpened { get; set; }

    [DataMember(Name = "panelVisible", Order = 16)]
    public bool PanelVisible { get; set; }

    [DataMember(Name = "panelInstanceType", Order = 17)]
    public string PanelInstanceType { get; set; } = string.Empty;

    [DataMember(Name = "settingsSave", Order = 18)]
    public bool SettingsSave { get; set; }

    [DataMember(Name = "settingsReload", Order = 19)]
    public bool SettingsReload { get; set; }

    [DataMember(Name = "logDirectoryExists", Order = 20)]
    public bool LogDirectoryExists { get; set; }

    [DataMember(Name = "testedAtUtc", Order = 21)]
    public string TestedAtUtc { get; set; } = string.Empty;

    [DataMember(Name = "failures", Order = 22)]
    public List<string> Failures { get; set; } = new List<string>();
}

public static class PanelVerificationEvidenceSerializer
{
    public static string Serialize(PanelVerificationEvidence evidence)
    {
        var serializer = new DataContractJsonSerializer(typeof(PanelVerificationEvidence));
        using (var stream = new MemoryStream())
        {
            serializer.WriteObject(stream, evidence);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

    public static void Write(string path, PanelVerificationEvidence evidence)
    {
        File.WriteAllText(path, Serialize(evidence));
    }
}
