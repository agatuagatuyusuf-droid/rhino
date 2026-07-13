using System.Runtime.InteropServices;
using RhinoCommercialPlatform.Core.Abstractions;

namespace RhinoCommercialPlatform.Core.Runtime;

public sealed class RuntimePlatformInfo : IPlatformInfo
{
    public bool IsWindows =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public bool IsMacOS =>
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public string OperatingSystem
    {
        get
        {
            if (IsWindows)
            {
                return "Windows";
            }

            if (IsMacOS)
            {
                return "macOS";
            }

            return "Unsupported";
        }
    }

    public string OperatingSystemDescription =>
        RuntimeInformation.OSDescription;

    public string ProcessArchitecture =>
        RuntimeInformation.ProcessArchitecture.ToString();

    public string FrameworkDescription =>
        RuntimeInformation.FrameworkDescription;
}
