using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhinoCommercialPlatform.Infrastructure;

namespace RhinoCommercialPlatform.UnitTests;

[TestClass]
public class AppPathsTests
{
    [TestMethod]
    public void EnsureCreated_CreatesAllExpectedDirectories()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new AppPaths(tempDir);
            paths.EnsureCreated();

            Assert.IsTrue(Directory.Exists(paths.ConfigDirectory));
            Assert.IsTrue(Directory.Exists(paths.DataDirectory));
            Assert.IsTrue(Directory.Exists(paths.CacheDirectory));
            Assert.IsTrue(Directory.Exists(paths.LogsDirectory));
            Assert.IsTrue(Directory.Exists(paths.UpdatesDirectory));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void Constructor_RejectsEmptyRootDirectory()
    {
        try
        {
            _ = new AppPaths("");
            Assert.Fail("Expected ArgumentException was not thrown.");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }
}
