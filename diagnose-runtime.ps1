# DSCMS Diagnostic Script
# This script helps diagnose common .NET runtime issues

Write-Host "=== DSCMS Runtime Diagnostics ===" -ForegroundColor Green
Write-Host ""

# Check .NET version
Write-Host "1. Checking .NET SDK and Runtime versions:" -ForegroundColor Yellow
dotnet --version
Write-Host ""
dotnet --list-sdks
Write-Host ""
dotnet --list-runtimes
Write-Host ""

# Check project file
Write-Host "2. Checking project configuration:" -ForegroundColor Yellow
if (Test-Path "DSCMS\DSCMS.csproj") {
    Write-Host "Project file found. Target framework:"
    Select-String -Path "DSCMS\DSCMS.csproj" -Pattern "TargetFramework"
    Write-Host ""
} else {
    Write-Host "ERROR: DSCMS.csproj not found!" -ForegroundColor Red
}

# Check global.json
Write-Host "3. Checking global.json:" -ForegroundColor Yellow
if (Test-Path "global.json") {
    Write-Host "global.json found:"
    Get-Content "global.json"
    Write-Host ""
} else {
    Write-Host "No global.json found (this is okay)" -ForegroundColor Gray
    Write-Host ""
}

# Clean and restore
Write-Host "4. Cleaning and restoring packages:" -ForegroundColor Yellow
try {
    dotnet clean DSCMS\DSCMS.csproj --verbosity quiet
    Write-Host "Clean completed successfully" -ForegroundColor Green
} catch {
    Write-Host "Clean failed: $($_.Exception.Message)" -ForegroundColor Red
}

try {
    dotnet restore DSCMS\DSCMS.csproj --verbosity quiet
    Write-Host "Restore completed successfully" -ForegroundColor Green
} catch {
    Write-Host "Restore failed: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Build
Write-Host "5. Building project:" -ForegroundColor Yellow
try {
    $buildOutput = dotnet build DSCMS\DSCMS.csproj --verbosity quiet 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Build completed successfully" -ForegroundColor Green
    } else {
        Write-Host "Build failed:" -ForegroundColor Red
        Write-Host $buildOutput
    }
} catch {
    Write-Host "Build error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Check for common problematic files
Write-Host "6. Checking for problematic files:" -ForegroundColor Yellow

$problematicPaths = @(
    "DSCMS\bin\Debug\net9.0\*.deps.json",
    "DSCMS\bin\Debug\net9.0\*.runtimeconfig.json",
    "DSCMS\obj\**\*.cache"
)

foreach ($path in $problematicPaths) {
    $files = Get-ChildItem -Path $path -ErrorAction SilentlyContinue
    if ($files) {
        Write-Host "Found files at $path" -ForegroundColor Gray
    }
}
Write-Host ""

# Check environment variables
Write-Host "7. Checking relevant environment variables:" -ForegroundColor Yellow
$envVars = @("DOTNET_ROOT", "DOTNET_MULTILEVEL_LOOKUP", "ASPNETCORE_ENVIRONMENT")
foreach ($env in $envVars) {
    $value = [Environment]::GetEnvironmentVariable($env)
    if ($value) {
        Write-Host "$env = $value" -ForegroundColor Gray
    }
}
Write-Host ""

Write-Host "=== Diagnostic Complete ===" -ForegroundColor Green
Write-Host ""
Write-Host "If you're still experiencing issues:" -ForegroundColor Yellow
Write-Host "1. Try deleting the bin and obj folders completely"
Write-Host "2. Restart Visual Studio or your IDE"
Write-Host "3. Check if antivirus is interfering with .NET files"
Write-Host "4. Try running: dotnet --info"
Write-Host ""