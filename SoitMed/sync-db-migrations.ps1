# PowerShell script to sync database with code after manual DB changes
# Run this script from the Backend/SoitMed directory

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Database Migration Sync Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if we're in the right directory
if (-not (Test-Path "SoitMed.csproj")) {
    Write-Host "ERROR: SoitMed.csproj not found. Please run this script from Backend/SoitMed directory" -ForegroundColor Red
    exit 1
}

Write-Host "Step 1: Checking current migration status..." -ForegroundColor Yellow
Write-Host ""
dotnet ef migrations list
Write-Host ""

Write-Host "Step 2: Creating test migration to detect differences..." -ForegroundColor Yellow
Write-Host "This will show what EF Core detects as differences between your models and database" -ForegroundColor Gray
Write-Host ""

$migrationName = "SyncWithManualDBChanges_$(Get-Date -Format 'yyyyMMddHHmmss')"
Write-Host "Creating migration: $migrationName" -ForegroundColor Cyan
dotnet ef migrations add $migrationName

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Migration created successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "NEXT STEPS:" -ForegroundColor Yellow
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "1. Review the migration file: Migrations/$migrationName.cs" -ForegroundColor White
    Write-Host "2. Check what changes it detected:" -ForegroundColor White
    Write-Host "   - If EMPTY: Your code is already in sync! Remove it with:" -ForegroundColor Green
    Write-Host "     dotnet ef migrations remove" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   - If it has changes ALREADY in your DB:" -ForegroundColor Yellow
    Write-Host "     a) Update your C# models to match the database" -ForegroundColor White
    Write-Host "     b) Remove this migration: dotnet ef migrations remove" -ForegroundColor Gray
    Write-Host "     c) Create new migration (should be empty now)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "   - If it has changes NOT in your DB:" -ForegroundColor Yellow
    Write-Host "     Apply it: dotnet ef database update" -ForegroundColor Gray
    Write-Host ""
    Write-Host "3. After syncing, verify with:" -ForegroundColor White
    Write-Host "   dotnet ef migrations add TestSyncCheck" -ForegroundColor Gray
    Write-Host "   (Should be empty if everything is synced)" -ForegroundColor Gray
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "ERROR: Failed to create migration. Check the errors above." -ForegroundColor Red
    Write-Host "Common issues:" -ForegroundColor Yellow
    Write-Host "  - Application is still running (stop it first)" -ForegroundColor White
    Write-Host "  - Build errors (fix compilation errors first)" -ForegroundColor White
    Write-Host "  - Database connection issues" -ForegroundColor White
}







