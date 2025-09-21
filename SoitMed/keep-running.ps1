Write-Host "Starting SoitMed Backend with Auto-Restart..." -ForegroundColor Green
Write-Host "Press Ctrl+C to stop" -ForegroundColor Yellow
Write-Host ""

while ($true) {
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] Starting application..." -ForegroundColor Cyan
    
    try {
        dotnet run --urls "http://localhost:5117"
    }
    catch {
        Write-Host "[$timestamp] Error occurred: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] Application stopped. Restarting in 5 seconds..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
    Write-Host ""
}
