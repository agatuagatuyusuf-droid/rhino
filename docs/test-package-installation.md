# Test Package Installation

CI pipelines produce platform-specific build artifacts for manual Rhino GUI testing.

## Downloading Artifacts

1. Navigate to the CI run on GitHub Actions:
   - `https://github.com/<org>/RhinoCommercialPlatform/actions/runs/<run-id>`

2. Scroll to **Artifacts** section at the bottom of the summary page.

3. Download the relevant artifact. Each framework is published separately:
   - `RCP-windows-net7.0-src-<source>-test-<tested>`
   - `RCP-windows-net48-src-<source>-test-<tested>`
   - `RCP-macos-net7.0-src-<source>-test-<tested>`

## Verifying Artifacts

Each extracted artifact contains `manifest.json` and `SHA256SUMS.txt`. The manifest records the source commit, tested commit, CI run, and aggregate artifact digest.

### Windows

```powershell
Get-Content .\SHA256SUMS.txt
```

Verify every listed file with an SHA256-capable checksum tool.

### macOS

```bash
shasum -a 256 -c SHA256SUMS.txt
```

Every file must report `OK`.

## Extracting

### Windows

```powershell
Expand-Archive -Path .\RCP-windows-*.zip -DestinationPath .\rcp-test
```

### macOS

```bash
unzip RCP-macos-net7.0-src-*-test-*.zip -d rcp-test
```

## Contents

Both artifacts contain:

| File                                              | Description                     |
|---------------------------------------------------|---------------------------------|
| `RhinoCommercialPlatform.Plugin.rhp`              | Rhino plug-in entry point       |
| `RhinoCommercialPlatform.UI.dll`                  | UI layer (Eto.Forms)            |
| `RhinoCommercialPlatform.Core.dll`                | Core abstractions               |
| `RhinoCommercialPlatform.Infrastructure.dll`      | Infrastructure (logging, paths) |
| `RhinoCommercialPlatform.Modules.Abstractions.dll`| Module system interfaces        |
| `RhinoCommercialPlatform.Modules.Foundation.dll`  | Foundation module               |
| `RhinoCommercialPlatform.Platform.Abstractions.dll`| Platform abstraction interfaces |
| `manifest.json`                                   | Build and artifact provenance   |
| `SHA256SUMS.txt`                                  | Per-file integrity checks       |

Windows-only additional file:
- `RhinoCommercialPlatform.Platform.Windows.dll`

macOS-only additional file:
- `RhinoCommercialPlatform.Platform.Mac.dll`

## Loading in Rhino

1. Launch Rhino 8
2. Run `PlugInManager` command
3. Click **Install** and select the `.rhp` file from the extracted directory
4. Confirm the plugin appears as loaded

## Commands

| Command         | Description                          |
|-----------------|--------------------------------------|
| `RCP_Status`    | Display runtime status information   |
| `RCP_OpenPanel` | Open the main Eto.Forms panel        |
| `RCP_VerifyPanel` | Run structured Panel verification  |

## Uninstalling

1. Open Rhino's Plug-in Manager
2. Select "RhinoCommercialPlatform" from the list
3. Click **Uninstall**

Or remove the `.rhp` file from Rhino's plug-ins directory.

## Building Locally

To produce test packages without CI:

```powershell
# Windows (net7.0)
dotnet publish src/RhinoCommercialPlatform.Plugin -c Release -f net7.0 -o dist/windows

# macOS (net7.0)
dotnet publish src/RhinoCommercialPlatform.Plugin -c Release -f net7.0 -o dist/macos
```

The output directory contains all required DLLs and the `.rhp` file.
