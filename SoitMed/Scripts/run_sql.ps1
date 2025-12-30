# ============================================================================
# Run SQL Scripts from PowerShell (Windows)
# ============================================================================
# Usage: .\run_sql.ps1 <script_name.sql>
# Example: .\run_sql.ps1 diagnose_slow_queries.sql
# ============================================================================

param(
    [Parameter(Mandatory=$true)]
    [string]$ScriptFile
)

# Database connection settings
$Server = "10.10.9.104\SQLEXPRESS,1433"
$Database = "ITIWebApi44"
$Username = "soitmed"
$Password = "356120Ah"

# Check if script file exists
if (-not (Test-Path $ScriptFile)) {
    Write-Host "ERROR: Script file '$ScriptFile' not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Available SQL scripts:" -ForegroundColor Yellow
    Get-ChildItem -Filter "*.sql" | ForEach-Object { Write-Host "  - $($_.Name)" }
    exit 1
}

Write-Host "=== Running SQL Script: $ScriptFile ===" -ForegroundColor Green
Write-Host ""

# Build connection string
$ConnectionString = "Server=$Server;Database=$Database;User Id=$Username;Password=$Password;TrustServerCertificate=True;Encrypt=False;Connection Timeout=60;Command Timeout=300;"

try {
    # Read SQL script
    $SqlScript = Get-Content $ScriptFile -Raw -Encoding UTF8
    
    # Create connection
    $Connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $Connection.Open()
    
    # Split by GO statements
    $Batches = $SqlScript -split '(?i)^\s*GO\s*$', 0, 'Multiline'
    
    $BatchNumber = 0
    foreach ($Batch in $Batches) {
        $Batch = $Batch.Trim()
        if ([string]::IsNullOrWhiteSpace($Batch) -or $Batch.StartsWith('--')) {
            continue
        }
        
        $BatchNumber++
        try {
            $Command = $Connection.CreateCommand()
            $Command.CommandText = $Batch
            $Command.CommandTimeout = 300
            
            # Execute command
            if ($Batch.Trim().ToUpper().StartsWith('SELECT')) {
                $Adapter = New-Object System.Data.SqlClient.SqlDataAdapter($Command)
                $DataSet = New-Object System.Data.DataSet
                $Adapter.Fill($DataSet) | Out-Null
                
                if ($DataSet.Tables.Count -gt 0 -and $DataSet.Tables[0].Rows.Count -gt 0) {
                    $DataSet.Tables[0] | Format-Table -AutoSize
                }
            } else {
                $RowsAffected = $Command.ExecuteNonQuery()
                if ($RowsAffected -ge 0) {
                    Write-Host "Batch $BatchNumber executed. Rows affected: $RowsAffected" -ForegroundColor Gray
                }
            }
        } catch {
            Write-Host "Error in batch $BatchNumber : $_" -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
    Write-Host "✓ Script executed successfully!" -ForegroundColor Green
    $Connection.Close()
    
} catch {
    Write-Host "✗ Error: $_" -ForegroundColor Red
    exit 1
}

