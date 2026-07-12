# Test Package Installation

CI pipelines produce platform-specific build artifacts for manual Rhino GUI testing.

## Downloading Artifacts

1. Navigate to the CI run on GitHub Actions:
   - `https://github.com/<org>/RhinoCommercialPlatform/actions/runs/<run-id>`

2. Scroll to **Artifacts** section at the bottom of the summary page.

3. Download the relevant artifact:
   - `RhinoCommercialPlatform-windows.zip` — Windows build (`net7.0` + `net48`)
   - `RhinoCommercialPlatform-macos.zip` — macOS build (`net7.0` only)

## Verifying Artifacts

Each CI run publishes SHA256 checksums as separate artifacts:
- `SHA256SUMS-windows.txt`
- `SHA256SUMS-macos.txt`

### Windows

```powershell
Get-FileHash .\RhinoCommercialPlatform-windows.zip -Algorithm SHA256
```

Compare the output with the content of `SHA256SUMS-windows.txt`.

### macOS

```bash
shasum -a 256 RhinoCommercialPlatform-macos.zip
```

Compare the output with the content of `SHA256SUMS-macos.txt`.

## Extracting

### Windows

```powershell
Expand-Archive -Path .\RhinoCommercialPlatform-windows.zip -DestinationPath .\rcp-test
```

### macOS

```bash
unzip RhinoCommercialPlatform-macos.zip -d rcp-test
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
| `Eto.dll`                                         | Eto.Forms cross-platform UI     |
| `Eto.*.dll`                                       | Eto platform-specific renderers |

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
