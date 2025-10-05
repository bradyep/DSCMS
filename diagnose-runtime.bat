@echo off
echo === DSCMS Runtime Diagnostics ===
echo.

echo 1. Checking .NET versions:
dotnet --version
echo.
dotnet --list-sdks
echo.
dotnet --list-runtimes
echo.

echo 2. Cleaning project:
dotnet clean DSCMS\DSCMS.csproj
echo.

echo 3. Restoring packages:
dotnet restore DSCMS\DSCMS.csproj
echo.

echo 4. Building project:
dotnet build DSCMS\DSCMS.csproj
echo.

echo 5. If issues persist, try:
echo    - Delete bin and obj folders
echo    - Restart Visual Studio
echo    - Run as Administrator
echo.

pause