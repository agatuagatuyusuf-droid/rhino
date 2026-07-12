using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhinoCommercialPlatform.Core.Abstractions;
using RhinoCommercialPlatform.Infrastructure;

namespace RhinoCommercialPlatform.UnitTests;

[TestClass]
public class FileAppLoggerTests
{
    private sealed class TestClock : IClock
    {
        public DateTimeOffset UtcNow { get; set; } = new DateTimeOffset(2026, 7, 12, 12, 34, 56, TimeSpan.Zero);
    }

    [TestMethod]
    public void Information_WritesPhysicalLogFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new AppPaths(tempDir);
            var clock = new TestClock();

            string content;
            using (var logger = new FileAppLogger(paths, clock))
            {
                logger.Information("Test message.");
            }

            var logFile = Path.Combine(paths.LogsDirectory, "plugin-20260712.log");
            Assert.IsTrue(File.Exists(logFile));

            content = File.ReadAllText(logFile);
            Assert.IsTrue(content.Contains("Test message."));
            Assert.IsTrue(content.Contains("[INFORMATION]"));
            Assert.IsTrue(content.Contains("2026-07-12T12:34:56"));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void Information_RedactsSecretsBeforeWriting()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new AppPaths(tempDir);
            var clock = new TestClock();

            string content;
            using (var logger = new FileAppLogger(paths, clock))
            {
                logger.Information("User token=my-secret-token logged in.");
            }

            var logFile = Path.Combine(paths.LogsDirectory, "plugin-20260712.log");
            Assert.IsTrue(File.Exists(logFile));

            content = File.ReadAllText(logFile);
            Assert.IsFalse(content.Contains("my-secret-token"));
            Assert.IsTrue(content.Contains("[REDACTED]"));
            Assert.IsTrue(content.Contains("User"));
            Assert.IsTrue(content.Contains("logged in."));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void Information_RedactsAuthorizationBearerToken()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new AppPaths(tempDir);
            var clock = new TestClock();

            string content;
            using (var logger = new FileAppLogger(paths, clock))
            {
                logger.Information("Authorization: Bearer abc.def.123");
            }

            var logFile = Path.Combine(paths.LogsDirectory, "plugin-20260712.log");
            Assert.IsTrue(File.Exists(logFile));

            content = File.ReadAllText(logFile);
            Assert.IsTrue(content.Contains("Authorization:"));
            Assert.IsTrue(content.Contains("[REDACTED]"));
            Assert.IsFalse(content.Contains("Bearer"));
            Assert.IsFalse(content.Contains("abc.def.123"));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [TestMethod]
    public void Error_RedactsExceptionMessage()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var paths = new AppPaths(tempDir);
            var clock = new TestClock();

            string content;
            using (var logger = new FileAppLogger(paths, clock))
            {
                var exception = new InvalidOperationException("token=should-be-redacted");
                logger.Error(exception, "An error occurred.");
            }

            var logFile = Path.Combine(paths.LogsDirectory, "plugin-20260712.log");
            Assert.IsTrue(File.Exists(logFile));

            content = File.ReadAllText(logFile);
            Assert.IsFalse(content.Contains("should-be-redacted"));
            Assert.IsTrue(content.Contains("[REDACTED]"));
            Assert.IsTrue(content.Contains("An error occurred."));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}
