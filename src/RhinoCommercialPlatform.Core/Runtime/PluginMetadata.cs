using System;

namespace RhinoCommercialPlatform.Core.Runtime;

public class PluginMetadata
{
    public string ProductName { get; }
    public string Version { get; }
    public RuntimeMode Mode { get; }

    public PluginMetadata(string productName, string version, RuntimeMode mode)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty.", nameof(productName));
        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Version cannot be empty.", nameof(version));

        ProductName = productName;
        Version = version;
        Mode = mode;
    }
}
