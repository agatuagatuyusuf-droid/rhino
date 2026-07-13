using System;
using System.Collections.Generic;

namespace RhinoCommercialPlatform.Modules.Abstractions;

public sealed class ModuleRegistry
{
    private enum RegistryState
    {
        Configuring = 0,
        Initializing = 1,
        Initialized = 2,
        Shutdown = 3
    }

    private readonly List<IPluginModule> _modules =
        new List<IPluginModule>();

    private readonly List<IPluginModule> _initializedModules =
        new List<IPluginModule>();

    private readonly HashSet<string> _registeredIds =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    private RegistryState _state = RegistryState.Configuring;

    public IReadOnlyList<IPluginModule> Modules =>
        _modules.AsReadOnly();

    public IReadOnlyList<IPluginModule> InitializedModules =>
        _initializedModules.AsReadOnly();

    public void Register(IPluginModule module)
    {
        if (module == null)
        {
            throw new ArgumentNullException(nameof(module));
        }

        if (_state != RegistryState.Configuring)
        {
            throw new InvalidOperationException(
                "Modules can only be registered before initialization.");
        }

        if (string.IsNullOrWhiteSpace(module.Id))
        {
            throw new ArgumentException(
                "Module ID cannot be null or empty.",
                nameof(module));
        }

        if (!_registeredIds.Add(module.Id))
        {
            throw new DuplicateModuleException(module.Id);
        }

        _modules.Add(module);
    }

    public void InitializeAll(IModuleContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (_state != RegistryState.Configuring)
        {
            throw new InvalidOperationException(
                "The module registry can only be initialized once.");
        }

        _state = RegistryState.Initializing;

        try
        {
            foreach (var module in _modules)
            {
                module.Initialize(context);
                _initializedModules.Add(module);
            }

            _state = RegistryState.Initialized;
        }
        catch (Exception initializationException)
        {
            var rollbackErrors = new List<Exception>();

            for (var index = _initializedModules.Count - 1;
                 index >= 0;
                 index--)
            {
                try
                {
                    _initializedModules[index].Shutdown();
                }
                catch (Exception rollbackException)
                {
                    rollbackErrors.Add(rollbackException);
                }
            }

            _initializedModules.Clear();
            _state = RegistryState.Shutdown;

            if (rollbackErrors.Count > 0)
            {
                var allErrors = new List<Exception>
                {
                    initializationException
                };

                allErrors.AddRange(rollbackErrors);

                throw new AggregateException(
                    "Module initialization failed and one or more " +
                    "rollback operations also failed.",
                    allErrors);
            }

            throw;
        }
    }

    public void ShutdownAll()
    {
        if (_state == RegistryState.Shutdown)
        {
            return;
        }

        if (_state == RegistryState.Initializing)
        {
            throw new InvalidOperationException(
                "The registry cannot be shut down while initialization " +
                "is in progress.");
        }

        if (_state == RegistryState.Configuring)
        {
            _state = RegistryState.Shutdown;
            return;
        }

        var shutdownErrors = new List<Exception>();

        for (var index = _initializedModules.Count - 1;
             index >= 0;
             index--)
        {
            try
            {
                _initializedModules[index].Shutdown();
            }
            catch (Exception shutdownException)
            {
                shutdownErrors.Add(shutdownException);
            }
        }

        _initializedModules.Clear();
        _state = RegistryState.Shutdown;

        if (shutdownErrors.Count == 1)
        {
            throw shutdownErrors[0];
        }

        if (shutdownErrors.Count > 1)
        {
            throw new AggregateException(
                "One or more modules failed during shutdown.",
                shutdownErrors);
        }
    }
}
