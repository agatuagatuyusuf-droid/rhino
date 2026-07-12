# Rhino GUI Smoke Test Evidence

Use this template to record manual GUI verification results.

## Test Metadata

| Field              | Value                          |
|--------------------|--------------------------------|
| **Test Date**      | YYYY-MM-DD                     |
| **Tester**         |                                |
| **Platform**       | Windows / macOS                 |
| **Rhino Version**  | 8.x                             |
| **Plugin Version** | 0.2.0                           |
| **Build Commit**   | `<commit-sha>`                  |
| **CI Run ID**      | `<run-id>`                      |

## Artifact Verification

- [ ] Artifact downloaded: `RhinoCommercialPlatform-<platform>.zip`
- [ ] SHA256 checksum matches `SHA256SUMS-<platform>.txt`
- [ ] All expected DLLs present in extracted directory

## Step Results

| #  | Step                              | Pass/Fail | Notes                       |
|----|-----------------------------------|-----------|-----------------------------|
| 1  | Load plugin in Rhino              |           |                             |
| 2  | RCP_Status output                 |           |                             |
| 3  | RCP_OpenPanel shows panel         |           |                             |
| 4  | Dashboard page renders            |           |                             |
| 5  | Modules page renders              |           |                             |
| 6  | RuntimeStatus page renders        |           |                             |
| 7  | Diagnostics page renders          |           |                             |
| 8  | Settings page renders             |           |                             |
| 9  | About page renders                |           |                             |
| 10 | Theme switch to Light             |           |                             |
| 11 | Theme switch to Dark              |           |                             |
| 12 | Theme switch to System            |           |                             |
| 13 | Save settings                     |           |                             |
| 14 | Close and reopen panel            |           |                             |
| 15 | Settings persist after Rhino restart|         |                             |

## Screenshots

Attach screenshots for each page:

1. **Dashboard** — `screenshots/dashboard-<platform>.png`
2. **Modules** — `screenshots/modules-<platform>.png`
3. **RuntimeStatus** — `screenshots/runtimestatus-<platform>.png`
4. **Diagnostics** — `screenshots/diagnostics-<platform>.png`
5. **Settings** — `screenshots/settings-<platform>.png`
6. **About** — `screenshots/about-<platform>.png`
7. **Dark theme** — `screenshots/dark-theme-<platform>.png`
8. **Light theme** — `screenshots/light-theme-<platform>.png`

## Log Verification

- [ ] `plugin-YYYYMMDD.log` exists
- [ ] Contains "Foundation module initialized."
- [ ] Contains "RhinoCommercialPlatform runtime started."
- [ ] Contains "Main panel shown."
- [ ] Contains "Main panel opened via RCP_OpenPanel command."
- [ ] Contains "Foundation module shutdown." (on Rhino exit)
- [ ] Contains "RhinoCommercialPlatform runtime stopped." (on Rhino exit)

## Overall Result

- [ ] **PASS** — All steps verified successfully
- [ ] **FAIL** — One or more steps failed (describe below)
- [ ] **BLOCKED** — Could not complete (describe blocker)

## Issues Found

| Issue | Description | Steps to Reproduce |
|-------|-------------|-------------------|
|       |             |                    |

## Notes

<!-- Free-form observations -->
