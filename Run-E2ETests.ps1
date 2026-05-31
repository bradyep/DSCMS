#!/usr/bin/env pwsh
# Run-E2ETests.ps1
# Starts the DSCMS app, runs Playwright E2E tests, then shuts the app down.

param(
    [string]$Url        = "http://localhost:5099",
    [int]   $TimeoutSec = 30
)

$projectRoot = $PSScriptRoot
$appProject  = Join-Path $projectRoot "DSCMS\DSCMS.csproj"
$testProject = Join-Path $projectRoot "DSCMS.Tests"
$exitCode    = 0

# ---------------------------------------------------------------------------
# 1. Build the app first so "dotnet run --no-build" starts almost instantly
# ---------------------------------------------------------------------------
Write-Host "`n>> Building DSCMS ..." -ForegroundColor Cyan

dotnet build $appProject --configuration Debug -nologo -consoleLoggerParameters:NoSummary | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed." -ForegroundColor Red
    exit 1
}

Write-Host ">> Build succeeded." -ForegroundColor Green

# ---------------------------------------------------------------------------
# 2. Free the port if something is already bound to it
# ---------------------------------------------------------------------------
$port = ([System.Uri]$Url).Port
Write-Host "`n>> Checking port $port ..." -ForegroundColor Cyan

$owners = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue |
    Select-Object -ExpandProperty OwningProcess |
    Sort-Object -Unique

if ($owners) {
    foreach ($ownerId in $owners) {
        Write-Host "   Killing PID $ownerId on port $port" -ForegroundColor Yellow
        Stop-Process -Id $ownerId -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Seconds 1   # give the OS a moment to release the socket
}

# ---------------------------------------------------------------------------
# 3. Start the app (no rebuild needed — starts in ~2-3 s)
# ---------------------------------------------------------------------------
Write-Host "`n>> Starting DSCMS on $Url ..." -ForegroundColor Cyan

$appProcess = Start-Process dotnet `
    -ArgumentList "run --project `"$appProject`" --no-build --urls `"$Url`"" `
    -PassThru -NoNewWindow

# ---------------------------------------------------------------------------
# 4. Wait until the app is responding
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
# 5. Run E2E tests
# ---------------------------------------------------------------------------
Write-Host "`n>> Running E2E tests ...`n" -ForegroundColor Cyan

dotnet test $testProject --filter "FullyQualifiedName~E2E" --no-build --logger "console;verbosity=normal"
$exitCode = $LASTEXITCODE

# ---------------------------------------------------------------------------
# 6. Shut down the app
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
# 7. Report and exit with the test exit code
# ---------------------------------------------------------------------------
if ($exitCode -eq 0) {
    Write-Host "`nAll E2E tests passed.`n" -ForegroundColor Green
} else {
    Write-Host "`nOne or more E2E tests failed (exit code $exitCode).`n" -ForegroundColor Red
}

exit $exitCode
