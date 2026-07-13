# Manual Main Panel Smoke Test (macOS)

## Prerequisites

- Rhino 8 installed on macOS Apple Silicon
- CI build artifact downloaded (see [test-package-installation.md](test-package-installation.md))
- SHA256SUMS file for artifact verification

## Steps

### 1. Download the macOS artifact

Download `RCP-macos-net7.0-src-<source>-test-<tested>` from the CI run's Artifacts section.

### 2. Extract to test directory

```bash
unzip RCP-macos-net7.0-src-*-test-*.zip -d rcp-test
```

Verify the extraction contains:
- `RhinoCommercialPlatform.Plugin.rhp`
- `RhinoCommercialPlatform.UI.dll`
- `RhinoCommercialPlatform.Core.dll`
- `RhinoCommercialPlatform.Infrastructure.dll`
- `RhinoCommercialPlatform.Modules.Abstractions.dll`
- `RhinoCommercialPlatform.Modules.Foundation.dll`
- `RhinoCommercialPlatform.Platform.Abstractions.dll`
- `RhinoCommercialPlatform.Platform.Mac.dll`
- `manifest.json`
- `SHA256SUMS.txt`

### 3. Verify package identity and SHA256SUMS

Run the checks from the repository root. `SHA256SUMS.txt` verifies each extracted file; `artifactSha256` is the manifest's deterministic package-content digest, not the downloaded zip hash.

```bash
(cd rcp-test && shasum -a 256 -c SHA256SUMS.txt)
TESTED_COMMIT=$(python3 -c 'import json; print(json.load(open("rcp-test/manifest.json"))["testedCommit"])')
python3 tools/check_test_package.py \
  --path rcp-test \
  --commit "$TESTED_COMMIT"
```

Both commands must pass, and `manifest.json` must contain no `unknown` build identity.

### 4. Open Rhino 8

Launch Rhino 8 on macOS.

### 5. Open PlugInManager

Type `PlugInManager` in Rhino's command line and press Enter.

### 6. Load .rhp file

Click **Install** and browse to `rcp-test/RhinoCommercialPlatform.Plugin.rhp`.

Confirm the plug-in appears as loaded in the Plug-in Manager.

### 7. Execute RCP_Status

Type `RCP_Status` in Rhino's command line and press Enter.

Verify output includes:
```
Product: RhinoCommercialPlatform
Version: 0.2.1
Mode: Production
Logs: <path>
Operating System: macOS
OS Description: macOS ...
Process Architecture: Arm64 / X64
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
- **关于** — product info, platform info, build info, licensing placeholder; verify Build Commit equals `manifest.testedCommit`

### 10. Test theme switching

- Click the 🌓 button in the header
- Verify the panel switches to Light theme (lighter colors)
- Click again to switch to Dark theme (darker colors)
- Click again to return to System theme (follows platform)
- Verify all components (header, sidebar, cards, text, buttons) update their colors

### 11. Test settings save/load

- Navigate to **设置** (Settings) page
- Change theme to "暗黑" (Dark)
- Check "记住上次打开的页面" (Remember last page)
- Click "保存设置" (Save Settings)
- Verify the saved configuration records `themeMode=Dark`, `rememberLastPage=true`, and `lastPage=Settings`

### 12. Close and reopen panel twice

- Invoke the real `MainPanelRegistration.ClosePanel` path through the Rhino MCP check, then run `RCP_OpenPanel`
- Repeat the close/reopen cycle once more
- After each reopen, verify `instances=1` and `visibleWindows=1`

### 13. Close Rhino, reopen, verify settings persist

- Quit Rhino 8 completely
- Launch Rhino 8 again
- Load the .rhp file again (if not auto-loaded)
- Run `RCP_OpenPanel`
- Without navigating first, verify the panel restores directly to **设置**
- Verify theme is still set to "暗黑"
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
| 12   | Two real close/reopen cycles   | Each returns one visible panel |
| 13   | Settings persistence           | Dark + Settings survive restart |

## Troubleshooting

- If the plugin fails to load, check `~/Library/Application Support/RhinoCommercialPlatform/logs/` for error details
- If the panel does not appear, verify the plugin loaded successfully via `PlugInManager`
- If theme changes do not persist, check `~/Library/Application Support/RhinoCommercialPlatform/config/user-settings.json`

## Status Tracking

| Status   | Meaning                     |
|----------|-----------------------------|
| PASS     | All steps verified on Rhino |
| NOT_RUN  | Not yet executed            |
| FAIL     | Verification failed         |
| BLOCKED  | No Rhino 8 environment      |

> Current macOS status: PASS for PR artifact `RCP-macos-net7.0-src-a7b1e6b-test-45393f4`, including full-window About identity, all six pages, three themes, repeated-open, two real ClosePanel/reopen cycles, and restart persistence. Windows remains `NOT_RUN`, so this is not release evidence and Phase 01 is not globally complete. A `main` artifact retest is still required after merge. Record that final result in [rhino-smoke-evidence.md](rhino-smoke-evidence.md) and [rhino-gui-status.json](../validation/rhino-gui-status.json).
