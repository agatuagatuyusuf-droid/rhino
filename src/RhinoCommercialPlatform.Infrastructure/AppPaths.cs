using System;
using System.IO;
using RhinoCommercialPlatform.Core.Abstractions;

namespace RhinoCommercialPlatform.Infrastructure;

public class AppPaths : IAppPaths
{
    public string RootDirectory { get; }
    public string ConfigDirectory => Path.Combine(RootDirectory, "config");
    public string DataDirectory => Path.Combine(RootDirectory, "data");
    public string CacheDirectory => Path.Combine(RootDirectory, "cache");
    public string LogsDirectory => Path.Combine(RootDirectory, "logs");
    public string UpdatesDirectory => Path.Combine(RootDirectory, "updates");

    public AppPaths()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RhinoCommercialPlatform"))
    {
    }

    public AppPaths(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("Root directory cannot be empty.", nameof(rootDirectory));

        RootDirectory = rootDirectory;
    }

    public void EnsureCreated()
    {
        Directory.CreateDirectory(ConfigDirectory);
        Directory.CreateDirectory(DataDirectory);
        Directory.CreateDirectory(CacheDirectory);
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(UpdatesDirectory);
    }
}
