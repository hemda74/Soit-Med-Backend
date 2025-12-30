# ============================================================================
# Run All Fix Scripts in Order (PowerShell - Windows)
# ============================================================================
# This script runs all the fix scripts in the correct order
# ============================================================================

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Running All Fix Scripts" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Diagnose (optional)
Write-Host "Step 1: Diagnosing slow queries..." -ForegroundColor Yellow
Write-Host "(This is optional - you can skip)" -ForegroundColor Yellow
$RunDiagnostic = Read-Host "Run diagnostic? (y/n)"
if ($RunDiagnostic -eq 'y' -or $RunDiagnostic -eq 'Y') {
    .\run_sql.ps1 diagnose_slow_queries.sql
    Write-Host ""
    Read-Host "Press Enter to continue to fixes..."
    Write-Host ""
}

# Step 2: Fix slow queries
Write-Host "Step 2: Fixing slow queries (indexes, statistics)..." -ForegroundColor Green
.\run_sql.ps1 fix_slow_queries.sql
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to run fix_slow_queries.sql" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 3: Fix categories
Write-Host "Step 3: Fixing categories (ensuring they're active)..." -ForegroundColor Green
.\run_sql.ps1 URGENT_FIX_CATEGORIES.sql
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to run URGENT_FIX_CATEGORIES.sql" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 4: Migrate products
Write-Host "Step 4: Migrating products (Category -> CategoryId)..." -ForegroundColor Green
Write-Host "⚠️  This is the most important step!" -ForegroundColor Yellow
$RunMigration = Read-Host "Run product migration? (y/n)"
if ($RunMigration -eq 'y' -or $RunMigration -eq 'Y') {
    .\run_sql.ps1 migrate_products_to_categoryid_SSMS.sql
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to run migrate_products_to_categoryid_SSMS.sql" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "Skipped product migration" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  All Scripts Completed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Restart your backend application"
Write-Host "  2. Test the categories API endpoint"
Write-Host "  3. Check if categories appear in the customer app"

