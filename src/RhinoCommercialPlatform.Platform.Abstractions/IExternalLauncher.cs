namespace RhinoCommercialPlatform.Platform.Abstractions;

public interface IExternalLauncher
{
    bool OpenDirectory(string directoryPath);
    bool OpenFile(string filePath);
    bool OpenUrl(string url);
}
