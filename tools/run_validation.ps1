$ErrorActionPreference = "Stop"

Write-Host "=== dotnet --info ==="
dotnet --info

Write-Host "=== python --version ==="
python --version

Write-Host "=== check_structure ==="
python (Join-Path $PSScriptRoot "check_structure.py")

Write-Host "=== check_forbidden_files ==="
python (Join-Path $PSScriptRoot "check_forbidden_files.py")

Write-Host "=== dotnet restore ==="
dotnet restore (Join-Path $PSScriptRoot ".." "RhinoCommercialPlatform.sln")

Write-Host "=== dotnet build (Release) ==="
dotnet build (Join-Path $PSScriptRoot ".." "RhinoCommercialPlatform.sln") -c Release --no-restore

Write-Host "=== dotnet test ==="
dotnet test (Join-Path $PSScriptRoot ".." "tests" "RhinoCommercialPlatform.UnitTests" "RhinoCommercialPlatform.UnitTests.csproj") -c Release --no-build

Write-Host "=== smoke test ==="
dotnet run --project (Join-Path $PSScriptRoot ".." "tests" "RhinoCommercialPlatform.Foundation.SmokeTests" "RhinoCommercialPlatform.Foundation.SmokeTests.csproj") -c Release --no-build

Write-Host "=== check_release_boundary ==="
python (Join-Path $PSScriptRoot "check_release_boundary.py")

Write-Host "=== git diff --check ==="
git diff --check

Write-Host "=== git status --short ==="
git status --short

Write-Host "=== VALIDATION_PASS ==="
