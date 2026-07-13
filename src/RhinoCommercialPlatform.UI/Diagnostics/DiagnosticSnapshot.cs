using System;
using System.Collections.Generic;

namespace RhinoCommercialPlatform.UI.Diagnostics;

public sealed class DiagnosticSnapshot
{
    public string ProductName { get; set; } = "";
    public string PluginVersion { get; set; } = "";
    public string? CommitInfo { get; set; }
    public string OperatingSystem { get; set; } = "";
    public string ProcessArchitecture { get; set; } = "";
    public string RuntimeVersion { get; set; } = "";
    public string RhinoVersion { get; set; } = "";
    public string LogDirectory { get; set; } = "";
    public string ConfigDirectory { get; set; } = "";
    public List<ModuleInfo> Modules { get; set; } = new List<ModuleInfo>();
    public List<string> RecentLogLines { get; set; } = new List<string>();
    public string ExportTimeUtc { get; set; } = "";
    public string RunMode { get; set; } = "";

    public sealed class ModuleInfo
    {
        public string Id { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string Version { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
