# Manual Rhino GUI Smoke Test

## Steps

1. Open Rhino 8 on Windows.
2. Open Plug-in Manager (`PlugInManager`).
3. Load `src/RhinoCommercialPlatform.Plugin/bin/Release/net48/RhinoCommercialPlatform.Plugin.rhp` (or `net7.0-windows` variant).
4. Confirm the plug-in appears as loaded.
5. Run the command `RCP_Status` in Rhino's command line.
6. Verify output includes:
   - Product: RhinoCommercialPlatform
   - Version: 0.1.0
   - Mode: Production
   - Logs: <path>
   - Modules: foundation (Foundation) v0.1.0
7. Close Rhino.
8. Check log file under `%LOCALAPPDATA%\RhinoCommercialPlatform\logs\plugin-YYYYMMDD.log`:
   - "Foundation module initialized."
   - "RhinoCommercialPlatform runtime started."
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

> Release build and foundation smoke are CI-verified. Rhino GUI load verification requires a Windows environment with Rhino 8 installed and is not part of automated CI at this phase.
