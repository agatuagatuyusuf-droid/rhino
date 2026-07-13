using System;

namespace RhinoCommercialPlatform.Modules.Abstractions;

public class DuplicateModuleException : InvalidOperationException
{
    public string ModuleId { get; }

    public DuplicateModuleException(string moduleId)
        : base($"A module with ID '{moduleId}' is already registered.")
    {
        ModuleId = moduleId;
    }
}
