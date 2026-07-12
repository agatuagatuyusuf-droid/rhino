using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RhinoCommercialPlatform.Core.Abstractions;
using RhinoCommercialPlatform.Modules.Abstractions;

namespace RhinoCommercialPlatform.UnitTests;

[TestClass]
public class ModuleRegistryTests
{
    private sealed class TestModule : IPluginModule
    {
        public string Id { get; }
        public string DisplayName { get; }
        public Version Version { get; } = new Version(1, 0);

        public int InitializeCallCount { get; private set; }
        public int ShutdownCallCount { get; private set; }
        public bool ThrowOnInitialize { get; set; }
        public bool ThrowOnShutdown { get; set; }

        public TestModule(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        public void Initialize(IModuleContext context)
        {
            InitializeCallCount++;
            if (ThrowOnInitialize)
                throw new InvalidOperationException($"Initialize failed for {Id}");
        }

        public void Shutdown()
        {
            ShutdownCallCount++;
            if (ThrowOnShutdown)
                throw new InvalidOperationException($"Shutdown failed for {Id}");
        }
    }

    private sealed class TestLogger : IAppLogger
    {
        public void Debug(string message) { }
        public void Information(string message) { }
        public void Warning(string message) { }
        public void Error(string message) { }
        public void Error(Exception exception, string message) { }
    }

    private sealed class TestPaths : IAppPaths
    {
        public string RootDirectory => Path.GetTempPath();
        public string ConfigDirectory => Path.Combine(RootDirectory, "config");
        public string DataDirectory => Path.Combine(RootDirectory, "data");
        public string CacheDirectory => Path.Combine(RootDirectory, "cache");
        public string LogsDirectory => Path.Combine(RootDirectory, "logs");
        public string UpdatesDirectory => Path.Combine(RootDirectory, "updates");
        public void EnsureCreated() { }
    }

    [TestMethod]
    public void Register_RejectsDuplicateModuleId()
    {
        var registry = new ModuleRegistry();
        registry.Register(new TestModule("test", "Test"));

        try
        {
            registry.Register(new TestModule("test", "Test Duplicate"));
            Assert.Fail("Expected DuplicateModuleException was not thrown.");
        }
        catch (DuplicateModuleException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void InitializeAll_UsesRegistrationOrder()
    {
        var registry = new ModuleRegistry();
        var first = new TestModule("first", "First");
        var second = new TestModule("second", "Second");

        registry.Register(first);
        registry.Register(second);

        var context = new ModuleContext(new TestLogger(), new TestPaths());
        registry.InitializeAll(context);

        Assert.AreEqual(1, first.InitializeCallCount);
        Assert.AreEqual(2, registry.InitializedModules.Count);
        Assert.AreSame(first, registry.InitializedModules[0]);
        Assert.AreSame(second, registry.InitializedModules[1]);
    }

    [TestMethod]
    public void ShutdownAll_UsesReverseInitializationOrder()
    {
        var registry = new ModuleRegistry();
        var first = new TestModule("first", "First");
        var second = new TestModule("second", "Second");

        registry.Register(first);
        registry.Register(second);

        var context = new ModuleContext(new TestLogger(), new TestPaths());
        registry.InitializeAll(context);
        registry.ShutdownAll();

        Assert.AreEqual(1, second.ShutdownCallCount);
        Assert.AreEqual(1, first.ShutdownCallCount);
    }

    [TestMethod]
    public void InitializeFailure_ShutsDownPreviouslyInitializedModules()
    {
        var registry = new ModuleRegistry();
        var first = new TestModule("first", "First");
        var failing = new TestModule("failing", "Failing") { ThrowOnInitialize = true };
        var third = new TestModule("third", "Third");

        registry.Register(first);
        registry.Register(failing);
        registry.Register(third);

        var context = new ModuleContext(new TestLogger(), new TestPaths());

        try
        {
            registry.InitializeAll(context);
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        Assert.AreEqual(1, first.ShutdownCallCount);
        Assert.AreEqual(0, failing.ShutdownCallCount);
        Assert.AreEqual(0, third.InitializeCallCount);
    }

    [TestMethod]
    public void ShutdownAll_IsIdempotent()
    {
        var registry = new ModuleRegistry();
        var module = new TestModule("test", "Test");

        registry.Register(module);

        var context = new ModuleContext(new TestLogger(), new TestPaths());
        registry.InitializeAll(context);

        registry.ShutdownAll();
        registry.ShutdownAll();
        registry.ShutdownAll();

        Assert.AreEqual(1, module.ShutdownCallCount);
    }
}
