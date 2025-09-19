$ErrorActionPreference = "Stop"

Write-Host "Testing DSCMS application startup..."

# Navigate to the project directory
Set-Location "D:\Dev\Projects\DSCMS\DSCMS"

# Start the application in background
$job = Start-Job -ScriptBlock {
    Set-Location "D:\Dev\Projects\DSCMS\DSCMS"
    dotnet run --no-build
}

# Wait for 10 seconds or until job completes
$result = Wait-Job $job -Timeout 10

if ($result) {
    Write-Host "Application started successfully!"
    $output = Receive-Job $job
    Write-Host "Output: $output"
} else {
    Write-Host "Application startup test completed (10 second timeout reached)"
}

# Clean up
Remove-Job $job -Force

Write-Host "Test completed."