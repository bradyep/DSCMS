# System.Runtime Assembly Loading Issue - Troubleshooting Guide

This document addresses the `System.IO.FileNotFoundException: Could not load file or assembly 'System.Runtime, Version=9.0.0.0'` error.

## What Was Fixed

### 1. **Package Version Inconsistencies** ? FIXED
- **Problem**: Mixed package versions (9.0.0 vs 9.0.9) causing assembly conflicts
- **Solution**: Standardized all packages to use version 9.0.0
- **Removed**: Redundant packages that are included in ASP.NET Core framework
  - `Microsoft.Data.Sqlite` (included in EF Core SQLite)
  - `Microsoft.Extensions.DependencyInjection` (included in ASP.NET Core)
  - `Microsoft.Extensions.Hosting` (included in ASP.NET Core)

### 2. **Global SDK Version Lock** ? ADDED
- **Created**: `global.json` to ensure consistent .NET SDK version (9.0.305)
- **Benefit**: Prevents SDK version conflicts across different environments

### 3. **Nullable Reference Warnings** ? FIXED
- **Fixed**: Configuration service nullable reference warnings
- **Benefit**: Prevents potential runtime null reference issues

### 4. **Robust Error Handling** ? ENHANCED
- **Added**: Try-catch blocks in Program.cs startup
- **Added**: Fallback error logging to console and file
- **Benefit**: Better diagnostic information when startup fails

## Current Project Configuration

### Package References (All 9.0.0)
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
<PackageReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Design" Version="9.0.0" />
```

### SDK Configuration (global.json)
```json
{
  "sdk": {
    "version": "9.0.305",
    "rollForward": "latestPatch"
  }
}
```

## If Issues Persist

### Immediate Steps
1. **Clean Build**:
   ```bash
   dotnet clean DSCMS\DSCMS.csproj
   dotnet restore DSCMS\DSCMS.csproj
   dotnet build DSCMS\DSCMS.csproj
   ```

2. **Delete Build Artifacts**:
   - Delete `DSCMS\bin` folder
   - Delete `DSCMS\obj` folder
   - Rebuild project

3. **Restart Development Environment**:
   - Close Visual Studio completely
   - Restart Visual Studio
   - Try debugging again

### Advanced Troubleshooting

#### Run Diagnostic Scripts
- **PowerShell**: `.\diagnose-runtime.ps1`
- **Command Prompt**: `diagnose-runtime.bat`

#### Check Environment
```bash
# Check .NET installation
dotnet --info

# List installed SDKs and runtimes
dotnet --list-sdks
dotnet --list-runtimes

# Verify project packages
dotnet list DSCMS\DSCMS.csproj package
```

#### Manual Assembly Investigation
If the error persists, check these locations:
- `DSCMS\bin\Debug\net9.0\DSCMS.deps.json`
- `DSCMS\bin\Debug\net9.0\DSCMS.runtimeconfig.json`

Look for version mismatches or missing dependencies.

### Potential Causes Still to Consider

1. **Antivirus Interference**:
   - Some antivirus software interferes with .NET assemblies
   - Try temporarily disabling real-time protection
   - Add project folder to antivirus exclusions

2. **Corrupted .NET Installation**:
   - Consider reinstalling .NET 9 SDK
   - Download from: https://dotnet.microsoft.com/download/dotnet/9.0

3. **Visual Studio Issues**:
   - Clear Visual Studio cache: `%localappdata%\Microsoft\VisualStudio\<version>\ComponentModelCache`
   - Repair Visual Studio installation
   - Update to latest Visual Studio version

4. **System PATH Issues**:
   - Ensure .NET is in system PATH
   - Check for conflicting .NET versions in PATH

## Testing the Fix

1. **Build Test**:
   ```bash
   dotnet build DSCMS\DSCMS.csproj --configuration Debug
   ```
   Should complete successfully with only nullable reference warnings.

2. **Run Test**:
   ```bash
   dotnet run --project DSCMS\DSCMS.csproj
   ```
   Application should start without assembly loading errors.

3. **Debug Test**:
   - Use Visual Studio F5 (Start Debugging)
   - Should launch without `System.Runtime` assembly errors

## Prevention

- Always use consistent package versions within the same major version
- Use `global.json` to lock SDK versions across team/environments  
- Regular cleanup of bin/obj folders during development
- Monitor package updates and update all related packages together

## Error Logging

If startup fails, the application now:
1. Logs errors to console
2. Creates `startup-error.log` file with detailed error information
3. Preserves original exception for debugging

Check these logs for additional diagnostic information if issues persist.