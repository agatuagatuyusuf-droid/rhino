using System;
using System.Collections.Generic;
using System.Linq;
using RhinoCommercialPlatform.Core.Abstractions;

namespace RhinoCommercialPlatform.Modules.Abstractions;

public class ModuleRegistry
{
    private readonly List<IPluginModule> _modules = new List<IPluginModule>();
    private readonly List<IPluginModule> _initializedModules = new List<IPluginModule>();
    private readonly HashSet<string> _registeredIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    private bool _shutdownCompleted;

    public IReadOnlyList<IPluginModule> Modules => _modules.AsReadOnly();
    public IReadOnlyList<IPluginModule> InitializedModules => _initializedModules.AsReadOnly();

    public void Register(IPluginModule module)
    {
        if (module == null)
            throw new ArgumentNullException(nameof(module));
        if (string.IsNullOrWhiteSpace(module.Id))
            throw new ArgumentException("Module ID cannot be null or empty.", nameof(module));
        if (!_registeredIds.Add(module.Id))
            throw new DuplicateModuleException(module.Id);

        _modules.Add(module);
    }

    public void InitializeAll(IModuleContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var completed = new List<IPluginModule>();

        try
        {
            foreach (var module in _modules)
            {
                module.Initialize(context);
                completed.Add(module);
                _initializedModules.Add(module);
            }
        }
        catch
        {
            for (int i = completed.Count - 1; i >= 0; i--)
            {
                try
                {
                    completed[i].Shutdown();
                    _initializedModules.Remove(completed[i]);
                }
                catch
                {
                    // Swallow shutdown exceptions during rollback
                }
            }

            _shutdownCompleted = true;
            throw;
        }
    }

    public void ShutdownAll()
    {
        if (_shutdownCompleted)
            return;

        var shutdownErrors = new List<Exception>();

        for (int i = _initializedModules.Count - 1; i >= 0; i--)
        {
            try
            {
                _initializedModules[i].Shutdown();
            }
            catch (Exception ex)
            {
                shutdownErrors.Add(ex);
            }
        }

        _initializedModules.Clear();
        _shutdownCompleted = true;

        if (shutdownErrors.Count > 0)
        {
            if (shutdownErrors.Count == 1)
                throw shutdownErrors[0];
            throw new AggregateException("One or more modules failed during shutdown.", shutdownErrors);
        }
    }
}
