[CmdletBinding()]
param(
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

    Write-Host "=== check structure ==="
    Invoke-NativeChecked `
        -FilePath "python" `
        -Arguments @("tools/check_structure.py")

    Write-Host "=== check forbidden files ==="
    Invoke-NativeChecked `
        -FilePath "python" `
        -Arguments @("tools/check_forbidden_files.py")

    Write-Host "=== restore ==="
    Invoke-NativeChecked `
        -FilePath "dotnet" `
        -Arguments @("restore", "RhinoCommercialPlatform.sln")

    Write-Host "=== build Debug ==="
    Invoke-NativeChecked `
        -FilePath "dotnet" `
        -Arguments @(
            "build",
            "RhinoCommercialPlatform.sln",
            "-c",
            "Debug",
            "--no-restore"
        )

    Write-Host "=== build Release ==="
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

    Write-Host "=== release boundary ==="
    Invoke-NativeChecked `
        -FilePath "python" `
        -Arguments @("tools/check_release_boundary.py")

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

    Write-Host "VALIDATION_PASS"
}
finally {
    Pop-Location
}
