#!/usr/bin/env pwsh
# Run-E2ETests.ps1
# Starts the DSCMS app, runs Playwright E2E tests, then shuts the app down.

param(
    [string]$Url        = "http://localhost:5000",
    [int]   $TimeoutSec = 30
)

$projectRoot = $PSScriptRoot
$appProject  = Join-Path $projectRoot "DSCMS\DSCMS.csproj"
$testProject = Join-Path $projectRoot "DSCMS.Tests"
$exitCode    = 0

# ---------------------------------------------------------------------------
# 1. Start the app
# ---------------------------------------------------------------------------
Write-Host "`n>> Starting DSCMS on $Url ..." -ForegroundColor Cyan

$appProcess = Start-Process dotnet `
    -ArgumentList "run --project `"$appProject`" --urls `"$Url`"" `
    -PassThru -NoNewWindow

# ---------------------------------------------------------------------------
# 2. Wait until the app is responding
# ---------------------------------------------------------------------------
Write-Host ">> Waiting for app to be ready (timeout: ${TimeoutSec}s) ..." -ForegroundColor Cyan

$ready    = $false
$deadline = (Get-Date).AddSeconds($TimeoutSec)

while ((Get-Date) -lt $deadline) {
    try {
        $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
        if ($response.StatusCode -lt 500) {
            $ready = $true
            break
        }
    } catch { }
    Start-Sleep -Milliseconds 500
}

if (-not $ready) {
    Write-Host "ERROR: App did not become ready within ${TimeoutSec}s." -ForegroundColor Red
    $appProcess | Stop-Process -Force -ErrorAction SilentlyContinue
    exit 1
}

Write-Host ">> App is ready." -ForegroundColor Green

# ---------------------------------------------------------------------------
# 3. Run E2E tests
# ---------------------------------------------------------------------------
Write-Host "`n>> Running E2E tests ...`n" -ForegroundColor Cyan

dotnet test $testProject --filter "FullyQualifiedName~E2E" --no-build --logger "console;verbosity=normal"
$exitCode = $LASTEXITCODE

# ---------------------------------------------------------------------------
# 4. Shut down the app
# ---------------------------------------------------------------------------
Write-Host "`n>> Stopping DSCMS ..." -ForegroundColor Cyan

# Stop the dotnet process and any child processes it spawned
$appProcess | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue |
    Where-Object { $_.Id -ne $PID } |
    Where-Object { $_.MainWindowTitle -eq "" } |
    Stop-Process -Force -ErrorAction SilentlyContinue

Write-Host ">> Done." -ForegroundColor Cyan

# ---------------------------------------------------------------------------
# 5. Report and exit with the test exit code
# ---------------------------------------------------------------------------
if ($exitCode -eq 0) {
    Write-Host "`nAll E2E tests passed.`n" -ForegroundColor Green
} else {
    Write-Host "`nOne or more E2E tests failed (exit code $exitCode).`n" -ForegroundColor Red
}

exit $exitCode
