using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhinoCommercialPlatform.Core.Runtime;

namespace RhinoCommercialPlatform.UnitTests;

[TestClass]
public sealed class RuntimePlatformInfoTests
{
    [TestMethod]
    public void OperatingSystem_ReturnsSupportedValue()
    {
        var info = new RuntimePlatformInfo();
        var os = info.OperatingSystem;
        Assert.IsTrue(os is "Windows" or "macOS" or "Unsupported",
            $"Unexpected OS value: {os}");
    }

    [TestMethod]
    public void OperatingSystemDescription_IsNotEmpty()
    {
        var info = new RuntimePlatformInfo();
        Assert.IsFalse(string.IsNullOrWhiteSpace(info.OperatingSystemDescription));
    }

    [TestMethod]
    public void ProcessArchitecture_IsNotEmpty()
    {
        var info = new RuntimePlatformInfo();
        Assert.IsFalse(string.IsNullOrWhiteSpace(info.ProcessArchitecture));
    }

    [TestMethod]
    public void FrameworkDescription_IsNotEmpty()
    {
        var info = new RuntimePlatformInfo();
        Assert.IsFalse(string.IsNullOrWhiteSpace(info.FrameworkDescription));
    }

    [TestMethod]
    public void ExactlyOneSupportedPlatformFlag_IsTrue()
    {
        var info = new RuntimePlatformInfo();
        // On any supported OS, exactly one of IsWindows or IsMacOS should be true
        Assert.IsTrue(info.IsWindows ^ info.IsMacOS,
            $"Expected exactly one platform flag true: IsWindows={info.IsWindows}, IsMacOS={info.IsMacOS}");
    }
}
