# Architecture

## Dependency Direction

```
RhinoCommercialPlatform.Core
    ↑
RhinoCommercialPlatform.Modules.Abstractions
    ↑
RhinoCommercialPlatform.Infrastructure
    ↑
RhinoCommercialPlatform.Modules.Foundation
    ↑
RhinoCommercialPlatform.Plugin
```

- **Core** — Pure abstractions and runtime primitives. No RhinoCommon dependency.
- **Modules.Abstractions** — Module system interfaces and registry. No RhinoCommon dependency.
- **Infrastructure** — File system, logging, redaction, paths. No RhinoCommon dependency.
- **Modules.Foundation** — First concrete module. No RhinoCommon dependency.
- **Plugin** — Rhino adaptation layer. Only this project references RhinoCommon.

## Key Design Decisions

- All business features are added as modules under `Modules.*`.
- Each module has standard `Initialize`/`Shutdown` lifecycle managed by `ModuleRegistry`.
- Logging is centralized in Infrastructure; all modules use `IAppLogger`.
- Plugin project is the only entry point; it creates `AppRuntime` on load.
- No static service locator; dependencies are injected via constructor.
