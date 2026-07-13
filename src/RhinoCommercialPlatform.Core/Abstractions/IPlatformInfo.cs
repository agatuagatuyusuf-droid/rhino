namespace RhinoCommercialPlatform.Core.Abstractions;

public interface IPlatformInfo
{
    bool IsWindows { get; }
    bool IsMacOS { get; }
    string OperatingSystem { get; }
    string OperatingSystemDescription { get; }
    string ProcessArchitecture { get; }
    string FrameworkDescription { get; }
}
