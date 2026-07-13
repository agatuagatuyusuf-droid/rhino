# Cross-Platform Development Policy

## Guiding Principles

1. **Shared code must not use Windows-specific APIs.** All code in `src/` (except platform adapter projects) must be pure managed code that compiles on both Windows and macOS.

2. **UI must use Eto.Forms.** The primary user interface framework is Eto.Forms, which provides a native look on both Windows and macOS. WPF, WinForms, AppKit, and SwiftUI are not permitted as the main UI framework.

3. **`net7.0` is the cross-platform primary target.** The plugin builds for `net7.0` on both Windows and macOS. `net48` is a Windows-only compatibility target.

4. **`net48` is Windows-only.** The `net48` target is provided for compatibility with Rhino 8 on Windows only. It must not be built or tested on macOS.

5. **Platform-specific capabilities use interfaces and adapters.** Platform-specific functionality (e.g., credential storage, file system paths) must be abstracted behind interfaces and implemented in platform adapter projects:
   - `src/RhinoCommercialPlatform.Platform.Windows/`
   - `src/RhinoCommercialPlatform.Platform.Mac/`

6. **No hardcoded paths in business modules.** All file system paths must use `Path.Combine` and platform-agnostic base directories.

7. **No P/Invoke in business modules.** Direct native code invocation must be isolated in platform adapter projects.

8. **OS detection must use `RuntimeInformation`.** All operating system checks must use `System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform()`. Conditional compilation (`#if`) must not be used to fake platform support.

9. **All new features must pass both Windows and macOS CI.** A pull request is not complete until both platform CI jobs succeed.

## Enforcement

- `tools/check_platform_compatibility.py` verifies that shared source code does not contain Windows-specific APIs.
- CI runs on both `windows-latest` and `macos-14` runners.
- Release boundary checks verify correct platform targets.
