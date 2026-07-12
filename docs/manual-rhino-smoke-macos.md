# Manual Rhino GUI Smoke Test (macOS)

## Prerequisites

- Rhino 8 installed on macOS
- Plugin built with `net7.0` target (macOS build)

## Steps

1. Open Rhino 8 on macOS.
2. Open Plug-in Manager (`PlugInManager`).
3. Load `src/RhinoCommercialPlatform.Plugin/bin/Release/net7.0/RhinoCommercialPlatform.Plugin.rhp`.
4. Confirm the plug-in appears as loaded.
5. Run the command `RCP_Status` in Rhino's command line.
6. Verify output includes:
   - Product: RhinoCommercialPlatform
   - Version: 0.1.0
   - Mode: Production
   - Operating System: macOS
   - OS Description: macOS ...
   - Process Architecture: Arm64 / X64
   - Framework: .NET ...
   - Logs: <path>
   - Modules: foundation (Foundation) v0.1.0
7. Close Rhino.
8. Check log file under the logs directory shown by `RCP_Status`:
   - "Foundation module initialized."
   - "RhinoCommercialPlatform runtime started."
   - Platform, OS, Architecture, Framework info logged
   - "Foundation module shutdown."
   - "RhinoCommercialPlatform runtime stopped."
9. Reopen Rhino and run `RCP_Status` again to confirm no duplicate registration errors.

## Status Tracking

| Status   | Meaning                     |
|----------|-----------------------------|
| PASS     | All steps verified on Rhino |
| NOT_RUN  | Not yet executed            |
| FAIL     | Verification failed         |
| BLOCKED  | No Rhino 8 environment      |

> Current Rhino GUI verification status: NOT_RUN
