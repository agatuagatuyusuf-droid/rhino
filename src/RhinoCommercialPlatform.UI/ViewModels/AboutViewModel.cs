using System;
using RhinoCommercialPlatform.Core.Runtime;

namespace RhinoCommercialPlatform.UI.ViewModels;

public sealed class AboutViewModel
{
    private readonly PluginMetadata _metadata;
    private readonly string _platform;
    private readonly string _architecture;
    private readonly string _runtimeVersion;
    private readonly string? _rhinoVersion;
    private readonly string? _buildCommit;

    public AboutViewModel(
        PluginMetadata metadata,
        string platform,
        string architecture,
        string runtimeVersion,
        string? rhinoVersion = null,
        string? buildCommit = null)
    {
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _platform = platform ?? throw new ArgumentNullException(nameof(platform));
        _architecture = architecture ?? throw new ArgumentNullException(nameof(architecture));
        _runtimeVersion = runtimeVersion ?? throw new ArgumentNullException(nameof(runtimeVersion));
        _rhinoVersion = rhinoVersion;
        _buildCommit = buildCommit;
    }

    public string ProductName => "RhinoCommercialPlatform";
    public string ProductVersion => _metadata.Version;
    public string Company => "SKY OCEAN";
    public string Platform => _platform;
    public string Architecture => _architecture;
    public string RunMode => _metadata.Mode.ToString();
    public string RhinoVersion => _rhinoVersion ?? "[未知]";
    public string RuntimeVersion => _runtimeVersion;
    public string BuildCommit => _buildCommit ?? "[未指定]";
    public string Copyright => "Copyright © SKY OCEAN";
    public bool IsDevelopmentBuild => _metadata.Mode == RuntimeMode.Development;
    public string ProjectStatus => "Development Preview";

    // Placeholder for future features (not yet implemented)
    public string LicenseStatus => "尚未启用";
    public string ActivationStatus => "尚未启用";
}
