using System;
using System.Collections.Generic;
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

        public List<string> Events { get; }
        public bool ThrowOnInitialize { get; set; }
        public bool ThrowOnShutdown { get; set; }

        public TestModule(string id, string displayName, List<string> events)
        {
            Id = id;
            DisplayName = displayName;
            Events = events;
        }

        public void Initialize(IModuleContext context)
        {
            Events.Add($"init:{Id}");
            if (ThrowOnInitialize)
                throw new InvalidOperationException($"Initialize failed for {Id}");
        }

        public void Shutdown()
        {
            Events.Add($"shutdown:{Id}");
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
        registry.Register(new TestModule("test", "Test", new List<string>()));

        try
        {
            registry.Register(new TestModule("test", "Test Duplicate", new List<string>()));
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
        var events = new List<string>();

        var first = new TestModule("first", "First", events);
        var second = new TestModule("second", "Second", events);
        var third = new TestModule("third", "Third", events);

        registry.Register(first);
        registry.Register(second);
        registry.Register(third);

        var context = new ModuleContext(new TestLogger(), new TestPaths());
        registry.InitializeAll(context);

        Assert.AreEqual(3, registry.InitializedModules.Count);

        CollectionAssert.AreEqual(
            new List<string>
            {
                "init:first",
                "init:second",
                "init:third",
            },
            events
        );
    }

    [TestMethod]
    public void ShutdownAll_UsesReverseInitializationOrder()
    {
        var registry = new ModuleRegistry();
        var events = new List<string>();

        var first = new TestModule("first", "First", events);
        var second = new TestModule("second", "Second", events);
        var third = new TestModule("third", "Third", events);

        registry.Register(first);
        registry.Register(second);
        registry.Register(third);

        var context = new ModuleContext(new TestLogger(), new TestPaths());
        registry.InitializeAll(context);
        registry.ShutdownAll();

        CollectionAssert.AreEqual(
            new List<string>
            {
                "init:first",
                "init:second",
                "init:third",
                "shutdown:third",
                "shutdown:second",
                "shutdown:first",
            },
            events
        );
    }

    [TestMethod]
    public void InitializeFailure_RollsBackInReverseOrder()
    {
        var registry = new ModuleRegistry();
        var events = new List<string>();

        var first = new TestModule("first", "First", events);
        var second = new TestModule("second", "Second", events);
        var failing = new TestModule("failing", "Failing", events) { ThrowOnInitialize = true };
        var third = new TestModule("third", "Third", events);

        registry.Register(first);
        registry.Register(second);
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

        CollectionAssert.AreEqual(
            new List<string>
            {
                "init:first",
                "init:second",
                "init:failing",
                "shutdown:second",
                "shutdown:first",
            },
            events
        );
    }

    [TestMethod]
    public void Register_AfterInitialization_Throws()
    {
        var registry = new ModuleRegistry();
        var events = new List<string>();

        registry.Register(new TestModule("first", "First", events));

        var context = new ModuleContext(new TestLogger(), new TestPaths());
        registry.InitializeAll(context);

        try
        {
            registry.Register(new TestModule("second", "Second", events));
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException ex)
        {
            Assert.IsTrue(ex.Message.Contains("before initialization"));
        }
    }

    [TestMethod]
    public void InitializeAll_CannotRunTwice()
    {
        var registry = new ModuleRegistry();
        var events = new List<string>();

        registry.Register(new TestModule("first", "First", events));

        var context = new ModuleContext(new TestLogger(), new TestPaths());
        registry.InitializeAll(context);

        try
        {
            registry.InitializeAll(context);
            Assert.Fail("Expected InvalidOperationException was not thrown.");
        }
        catch (InvalidOperationException ex)
        {
            Assert.IsTrue(ex.Message.Contains("only be initialized once"));
        }
    }

    [TestMethod]
    public void ShutdownAll_ContinuesAfterOneModuleFails()
    {
        var registry = new ModuleRegistry();
        var events = new List<string>();

        var first = new TestModule("first", "First", events);
        var failing = new TestModule("failing", "Failing", events) { ThrowOnShutdown = true };
        var third = new TestModule("third", "Third", events);

        registry.Register(first);
        registry.Register(failing);
        registry.Register(third);

        var context = new ModuleContext(new TestLogger(), new TestPaths());
        registry.InitializeAll(context);

        try
        {
            registry.ShutdownAll();
            Assert.Fail("Expected exception was not thrown.");
        }
        catch (AggregateException)
        {
            // Expected - shutdown continues despite failures
        }
        catch (InvalidOperationException)
        {
            // Also possible if single failure is rethrown directly
        }

        Assert.IsTrue(events.Contains("shutdown:third"));
        Assert.IsTrue(events.Contains("shutdown:failing"));
        Assert.IsTrue(events.Contains("shutdown:first"));
        Assert.AreEqual(0, registry.InitializedModules.Count);
    }

    [TestMethod]
    public void ShutdownAll_ThrowsAfterClosingRemainingModules()
    {
        var registry = new ModuleRegistry();
        var events = new List<string>();

        var first = new TestModule("first", "First", events);
        var failing = new TestModule("failing", "Failing", events) { ThrowOnShutdown = true };
        var third = new TestModule("third", "Third", events);

        registry.Register(first);
        registry.Register(failing);
        registry.Register(third);

        var context = new ModuleContext(new TestLogger(), new TestPaths());
        registry.InitializeAll(context);

        try
        {
            registry.ShutdownAll();
            Assert.Fail("Expected exception was not thrown.");
        }
        catch
        {
            // Expected
        }

        // All modules should have been attempted
        CollectionAssert.AreEqual(
            new List<string>
            {
                "init:first",
                "init:failing",
                "init:third",
                "shutdown:third",
                "shutdown:failing",
                "shutdown:first",
            },
            events
        );
    }

    [TestMethod]
    public void InitializeFailure_WithRollbackFailure_ThrowsAggregate()
    {
        var registry = new ModuleRegistry();
        var events = new List<string>();

        var first = new TestModule("first", "First", events) { ThrowOnShutdown = true };
        var failing = new TestModule("failing", "Failing", events) { ThrowOnInitialize = true };

        registry.Register(first);
        registry.Register(failing);

        var context = new ModuleContext(new TestLogger(), new TestPaths());

        try
        {
            registry.InitializeAll(context);
            Assert.Fail("Expected AggregateException was not thrown.");
        }
        catch (AggregateException ex)
        {
            Assert.IsTrue(ex.InnerExceptions.Count >= 2);
            Assert.IsTrue(ex.Message.Contains("rollback"));
        }
    }

    [TestMethod]
    public void InitializeFailure_ClearsInitializedModules()
    {
        var registry = new ModuleRegistry();
        var events = new List<string>();

        var first = new TestModule("first", "First", events);
        var failing = new TestModule("failing", "Failing", events) { ThrowOnInitialize = true };

        registry.Register(first);
        registry.Register(failing);

        var context = new ModuleContext(new TestLogger(), new TestPaths());

        try
        {
            registry.InitializeAll(context);
        }
        catch
        {
            // Expected
        }

        Assert.AreEqual(0, registry.InitializedModules.Count);
    }

    [TestMethod]
    public void ShutdownAll_BeforeInitialize_IsSafe()
    {
        var registry = new ModuleRegistry();
        registry.ShutdownAll();

        // Should not throw
    }

    [TestMethod]
    public void ShutdownAll_IsIdempotent()
    {
        var registry = new ModuleRegistry();
        var events = new List<string>();

        var module = new TestModule("test", "Test", events);

        registry.Register(module);

        var context = new ModuleContext(new TestLogger(), new TestPaths());
        registry.InitializeAll(context);

        registry.ShutdownAll();
        registry.ShutdownAll();
        registry.ShutdownAll();

        Assert.AreEqual(1, events.FindAll(e => e == "shutdown:test").Count);
    }
}
