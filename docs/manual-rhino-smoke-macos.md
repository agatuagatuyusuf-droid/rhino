# Manual Rhino GUI Smoke Test (macOS)

## Prerequisites

- Rhino 8 installed on macOS
- macOS `net7.0` CI artifact downloaded and extracted

## Steps

1. Open Rhino 8 on macOS.
2. Open Plug-in Manager (`PlugInManager`).
3. Load `<extracted-artifact>/RhinoCommercialPlatform.Plugin.rhp`.
4. Confirm the plug-in appears as loaded.
5. Run the command `RCP_Status` in Rhino's command line.
6. Verify output includes:
   - Product: RhinoCommercialPlatform
    - Version: 0.2.1
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

> Current macOS status: PASS for PR artifact `RCP-macos-net7.0-src-a7b1e6b-test-45393f4`, including full-window About identity, all six pages, three themes, repeated-open, two real ClosePanel/reopen cycles, and restart persistence. Windows remains `NOT_RUN`, so this is not release evidence and Phase 01 is not globally complete. A `main` artifact retest is still required after merge.
