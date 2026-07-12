# Manual Main Panel Smoke Test (Windows)

## Prerequisites

- Rhino 8 installed on Windows
- CI build artifact downloaded (see [test-package-installation.md](test-package-installation.md))
- SHA256SUMS file for artifact verification

## Steps

### 1. Download the Windows artifact

Download `RhinoCommercialPlatform-windows.zip` from the CI run's summary page (Actions tab).

### 2. Verify SHA256SUMS

```powershell
# Compare the SHA256 of the downloaded zip against the published checksum
Get-FileHash .\RhinoCommercialPlatform-windows.zip -Algorithm SHA256
```

Compare the output hash against the value in `SHA256SUMS-windows.txt` from the CI run.

### 3. Extract to test directory

```powershell
Expand-Archive -Path .\RhinoCommercialPlatform-windows.zip -DestinationPath .\rcp-test
```

Verify the extraction contains:
- `RhinoCommercialPlatform.Plugin.rhp`
- `RhinoCommercialPlatform.UI.dll`
- `RhinoCommercialPlatform.Core.dll`
- `RhinoCommercialPlatform.Infrastructure.dll`
- `RhinoCommercialPlatform.Modules.Abstractions.dll`
- `RhinoCommercialPlatform.Modules.Foundation.dll`
- `RhinoCommercialPlatform.Platform.Abstractions.dll`
- `RhinoCommercialPlatform.Platform.Windows.dll`
- `Eto.dll`

### 4. Open Rhino 8

Launch Rhino 8 on Windows.

### 5. Open PlugInManager

Type `PlugInManager` in Rhino's command line and press Enter.

### 6. Load .rhp file

Click **Install** and browse to `rcp-test\RhinoCommercialPlatform.Plugin.rhp`.

Confirm the plug-in appears as loaded in the Plug-in Manager.

### 7. Execute RCP_Status

Type `RCP_Status` in Rhino's command line and press Enter.

Verify output includes:
```
Product: RhinoCommercialPlatform
Version: 0.2.0
Mode: Production
Logs: <path>
Operating System: Windows
OS Description: Microsoft Windows ...
Process Architecture: X64 / Arm64
Framework: .NET ...
Modules:
  - foundation (Foundation) v0.1.0
```

### 8. Execute RCP_OpenPanel

Type `RCP_OpenPanel` in Rhino's command line and press Enter.

Verify a panel titled "RhinoCommercialPlatform" appears.

### 9. Verify main panel appears with all pages

Confirm the panel has:
- A header with "RhinoCommercialPlatform" title, theme toggle (🌓), and settings (⚙) buttons
- A sidebar on the left with six navigation items: 首页, 功能模块, 运行状态, 诊断中心, 设置, 关于
- A content area showing the Dashboard page by default
- A status footer at the bottom

Click each navigation item and verify the content area switches to the corresponding page:
- **首页** — product info, platform info, module status, paths, quick entries
- **功能模块** — module list with status indicators
- **运行状态** — runtime status, module info, Rhino document info, version info
- **诊断中心** — log path, recent log list, diagnostic info
- **设置** — theme selector, auto-open checkbox, remember-page checkbox, log level dropdown
- **关于** — product info, platform info, build info, licensing placeholder

### 10. Test theme switching

- Click the 🌓 button in the header
- Verify the panel switches to Light theme (lighter colors)
- Click again to switch to Dark theme (darker colors)
- Click again to return to System theme (follows platform)
- Verify all components (header, sidebar, cards, text, buttons) update their colors

### 11. Test settings save/load

- Navigate to **设置** (Settings) page
- Change theme to "暗黑" (Dark)
- Change log level to "Debug"
- Check "记住上次打开的页面" (Remember last page)
- Click "保存设置" (Save Settings)
- Verify "设置已保存" confirmation dialog appears

### 12. Close and reopen panel

- Close the panel window
- Run `RCP_OpenPanel` again
- Verify the panel reappears with the same content

### 13. Close Rhino, reopen, verify settings persist

- Close Rhino 8 completely
- Launch Rhino 8 again
- Load the .rhp file again (if not auto-loaded)
- Run `RCP_OpenPanel`
- Navigate to **设置**
- Verify theme is still set to "暗黑" and log level to "Debug"
- Verify "记住上次打开的页面" is still checked

## Expected Results

| Step | Description                    | Expected Result               |
|------|--------------------------------|-------------------------------|
| 1-3  | Download and extract artifact  | Files extracted successfully  |
| 4-6  | Load plugin in Rhino           | Plugin loads without error    |
| 7    | RCP_Status                     | Status output matches spec    |
| 8    | RCP_OpenPanel                  | Panel appears                 |
| 9    | Page navigation                | All 6 pages render correctly  |
| 10   | Theme switching                | Light/Dark/System all work    |
| 11   | Settings save                  | Settings saved without error  |
| 12   | Close/reopen panel             | Panel reopens correctly       |
| 13   | Settings persistence           | Settings survive Rhino restart|

## Troubleshooting

- If the plugin fails to load, check `%LOCALAPPDATA%\RhinoCommercialPlatform\logs\` for error details
- If the panel does not appear, verify the plugin loaded successfully via `PlugInManager`
- If theme changes do not persist, check `%LOCALAPPDATA%\RhinoCommercialPlatform\config\user-settings.json`

## Status Tracking

| Status   | Meaning                     |
|----------|-----------------------------|
| PASS     | All steps verified on Rhino |
| NOT_RUN  | Not yet executed            |
| FAIL     | Verification failed         |
| BLOCKED  | No Rhino 8 environment      |

> Record results in [rhino-smoke-evidence.md](rhino-smoke-evidence.md) and update [rhino-gui-status.json](../validation/rhino-gui-status.json).
