# Fix Network Profile - Change to Private
# Run as Administrator

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Network Profile Fix" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get current network profile
$currentProfile = Get-NetConnectionProfile
Write-Host "Current Network Profile:" -ForegroundColor Yellow
$currentProfile | Format-Table Name, InterfaceAlias, NetworkCategory -AutoSize

# Change to Private
Write-Host "Changing network profile to Private..." -ForegroundColor Yellow
$currentProfile | Set-NetConnectionProfile -NetworkCategory Private

Write-Host ""
Write-Host "✅ Network profile changed to Private!" -ForegroundColor Green
Write-Host ""
Write-Host "Now test from Android device:" -ForegroundColor Cyan
Write-Host "http://10.10.9.104:5117/swagger" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")




