[CmdletBinding()]
param(
    [ValidateSet("auto", "windows", "macos")]
    [string]$Platform = "auto",

    [switch]$FailureProbe
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Invoke-NativeChecked {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter()]
        [string[]]$Arguments = @()
    )

    $displayArguments = $Arguments -join " "
    Write-Host "> $FilePath $displayArguments"

    & $FilePath @Arguments
    $exitCode = $LASTEXITCODE

    if ($exitCode -ne 0) {
        throw "Native command failed with exit code $exitCode`: $FilePath $displayArguments"
    }
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path

Push-Location $repoRoot

try {
    if ($FailureProbe) {
        Invoke-NativeChecked `
            -FilePath "python" `
            -Arguments @("-c", "import sys; sys.exit(23)")

        throw "Failure probe unexpectedly continued."
    }

    # Auto-detect platform
    if ($Platform -eq "auto") {
        if ($IsWindows) {
            $Platform = "windows"
        }
        elseif ($IsMacOS) {
            $Platform = "macos"
        }
        else {
            throw "Unsupported operating system."
        }
    }

    Write-Host "=== Platform: $Platform ==="
    Write-Host "=== validation fail-fast self-test ==="
    & (Join-Path $PSScriptRoot "check_validation_failfast.ps1")

    Write-Host "=== dotnet --info ==="
    Invoke-NativeChecked -FilePath "dotnet" -Arguments @("--info")

    Write-Host "=== python --version ==="
    Invoke-NativeChecked -FilePath "python" -Arguments @("--version")

    Write-Host "=== compile Python tools ==="
    Invoke-NativeChecked `
        -FilePath "python" `
        -Arguments @("-m", "compileall", "-q", "tools")

    Write-Host "=== Python tool tests ==="
    Invoke-NativeChecked `
        -FilePath "python" `
        -Arguments @("tools/test_package_test_artifacts.py")

    Write-Host "=== panel verification schema ==="
    Invoke-NativeChecked `
        -FilePath "python" `
        -Arguments @(
            "tools/check_panel_verification_schema.py",
            "validation/rhino-panel-verification.example.json"
        )

    Write-Host "=== check structure ==="
    Invoke-NativeChecked `
        -FilePath "python" `
        -Arguments @("tools/check_structure.py")

    Write-Host "=== check forbidden files ==="
    Invoke-NativeChecked `
        -FilePath "python" `
        -Arguments @("tools/check_forbidden_files.py")

    Write-Host "=== check platform compatibility ==="
    Invoke-NativeChecked `
        -FilePath "python" `
        -Arguments @("tools/check_platform_compatibility.py")

    Write-Host "=== restore ==="
    Invoke-NativeChecked `
        -FilePath "dotnet" `
        -Arguments @("restore", "RhinoCommercialPlatform.sln")

    if ($Platform -eq "windows") {
        Write-Host "=== build Debug (full solution) ==="
        Invoke-NativeChecked `
            -FilePath "dotnet" `
            -Arguments @(
                "build",
                "RhinoCommercialPlatform.sln",
                "-c",
                "Debug",
                "--no-restore"
            )

        Write-Host "=== build Release (full solution) ==="
        Invoke-NativeChecked `
            -FilePath "dotnet" `
            -Arguments @(
                "build",
                "RhinoCommercialPlatform.sln",
                "-c",
                "Release",
                "--no-restore"
            )

        Write-Host "=== unit tests ==="
        Invoke-NativeChecked `
            -FilePath "dotnet" `
            -Arguments @(
                "test",
                "tests/RhinoCommercialPlatform.UnitTests/RhinoCommercialPlatform.UnitTests.csproj",
                "-c",
                "Release",
                "--no-build"
            )

        Write-Host "=== foundation smoke test ==="
        Invoke-NativeChecked `
            -FilePath "dotnet" `
            -Arguments @(
                "run",
                "--project",
                "tests/RhinoCommercialPlatform.Foundation.SmokeTests/RhinoCommercialPlatform.Foundation.SmokeTests.csproj",
                "-c",
                "Release",
                "--no-build"
            )

        Write-Host "=== release boundary (windows) ==="
        Invoke-NativeChecked `
            -FilePath "python" `
            -Arguments @("tools/check_release_boundary.py", "--platform", "windows")
    }
    elseif ($Platform -eq "macos") {
        Write-Host "=== build Debug (plugin net7.0) ==="
        Invoke-NativeChecked `
            -FilePath "dotnet" `
            -Arguments @(
                "build",
                "src/RhinoCommercialPlatform.Plugin/RhinoCommercialPlatform.Plugin.csproj",
                "-c",
                "Debug",
                "-f",
                "net7.0",
                "--no-restore"
            )

        Write-Host "=== build Release (plugin net7.0) ==="
        Invoke-NativeChecked `
            -FilePath "dotnet" `
            -Arguments @(
                "build",
                "src/RhinoCommercialPlatform.Plugin/RhinoCommercialPlatform.Plugin.csproj",
                "-c",
                "Release",
                "-f",
                "net7.0",
                "--no-restore"
            )

        Write-Host "=== build Release unit tests ==="
        Invoke-NativeChecked `
            -FilePath "dotnet" `
            -Arguments @(
                "build",
                "tests/RhinoCommercialPlatform.UnitTests/RhinoCommercialPlatform.UnitTests.csproj",
                "-c",
                "Release",
                "--no-restore"
            )

        Write-Host "=== build Release smoke tests ==="
        Invoke-NativeChecked `
            -FilePath "dotnet" `
            -Arguments @(
                "build",
                "tests/RhinoCommercialPlatform.Foundation.SmokeTests/RhinoCommercialPlatform.Foundation.SmokeTests.csproj",
                "-c",
                "Release",
                "--no-restore"
            )

        Write-Host "=== unit tests ==="
        Invoke-NativeChecked `
            -FilePath "dotnet" `
            -Arguments @(
                "test",
                "tests/RhinoCommercialPlatform.UnitTests/RhinoCommercialPlatform.UnitTests.csproj",
                "-c",
                "Release",
                "--no-build"
            )

        Write-Host "=== foundation smoke test ==="
        Invoke-NativeChecked `
            -FilePath "dotnet" `
            -Arguments @(
                "run",
                "--project",
                "tests/RhinoCommercialPlatform.Foundation.SmokeTests/RhinoCommercialPlatform.Foundation.SmokeTests.csproj",
                "-c",
                "Release",
                "--no-build"
            )

        Write-Host "=== release boundary (macos) ==="
        Invoke-NativeChecked `
            -FilePath "python" `
            -Arguments @("tools/check_release_boundary.py", "--platform", "macos")
    }

    Write-Host "=== git diff --check ==="
    Invoke-NativeChecked `
        -FilePath "git" `
        -Arguments @("diff", "--check")

    Write-Host "=== git diff --cached --check ==="
    Invoke-NativeChecked `
        -FilePath "git" `
        -Arguments @("diff", "--cached", "--check")

    Write-Host "=== git status --short ==="
    Invoke-NativeChecked `
        -FilePath "git" `
        -Arguments @("status", "--short")

    Write-Host "VALIDATION_PASS platform=$Platform"
}
finally {
    Pop-Location
}
