namespace RhinoCommercialPlatform.Core.Abstractions;

public interface IAppPaths
{
    string RootDirectory { get; }
    string ConfigDirectory { get; }
    string DataDirectory { get; }
    string CacheDirectory { get; }
    string LogsDirectory { get; }
    string UpdatesDirectory { get; }

    void EnsureCreated();
}
