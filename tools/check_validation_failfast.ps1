Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$validationScript = Join-Path $PSScriptRoot "run_validation.ps1"
$hostExecutable = (Get-Process -Id $PID).Path

Write-Host "> Testing native-command failure propagation"

& $hostExecutable `
    -NoProfile `
    -File $validationScript `
    -FailureProbe

$probeExitCode = $LASTEXITCODE

if ($probeExitCode -eq 0) {
    throw "run_validation.ps1 returned exit code 0 for an intentional native-command failure."
}

Write-Host "VALIDATION_FAILFAST_CHECK_PASS"
