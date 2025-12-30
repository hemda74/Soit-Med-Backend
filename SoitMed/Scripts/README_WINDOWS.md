# Running SQL Scripts on Windows

## Quick Start

### Option 1: Using PowerShell Script (Recommended)

1. **Open PowerShell** in the `Scripts` folder:
   ```powershell
   cd C:\path\to\soitmed\backend\SoitMed\Scripts
   ```

2. **Run a single script:**
   ```powershell
   .\run_sql.ps1 diagnose_slow_queries.sql
   ```

3. **Run all fixes in order:**
   ```powershell
   .\run_all_fixes.ps1
   ```

### Option 2: Using sqlcmd (Command Line)

1. **Run a single script:**
   ```cmd
   sqlcmd -S "10.10.9.104\SQLEXPRESS,1433" -d "ITIWebApi44" -U "soitmed" -P "356120Ah" -C -i "diagnose_slow_queries.sql" -l 300
   ```

2. **Run all scripts in order:**
   ```cmd
   sqlcmd -S "10.10.9.104\SQLEXPRESS,1433" -d "ITIWebApi44" -U "soitmed" -P "356120Ah" -C -i "fix_slow_queries.sql" -l 300
   sqlcmd -S "10.10.9.104\SQLEXPRESS,1433" -d "ITIWebApi44" -U "soitmed" -P "356120Ah" -C -i "URGENT_FIX_CATEGORIES.sql" -l 300
   sqlcmd -S "10.10.9.104\SQLEXPRESS,1433" -d "ITIWebApi44" -U "soitmed" -P "356120Ah" -C -i "migrate_products_to_categoryid_SSMS.sql" -l 300
   ```

### Option 3: Using SQL Server Management Studio (SSMS)

1. **Open SSMS** and connect to: `10.10.9.104\SQLEXPRESS`
2. **Open** the SQL script file (`.sql`)
3. **Execute** the script (F5 or Execute button)

## Available Scripts

### Diagnostic Scripts
- `diagnose_slow_queries.sql` - Identifies why queries are slow
- `check_categories_state.sql` - Checks category and product state

### Fix Scripts (Run in Order)
1. `fix_slow_queries.sql` - Adds indexes, updates statistics, rebuilds indexes
2. `URGENT_FIX_CATEGORIES.sql` - Ensures categories are active and properly configured
3. `migrate_products_to_categoryid_SSMS.sql` - Migrates products from Category (text) to CategoryId (FK)

### Performance Scripts
- `add_performance_indexes.sql` - Adds performance indexes (if not in fix_slow_queries.sql)

## Recommended Order

1. **Diagnose** (optional):
   ```powershell
   .\run_sql.ps1 diagnose_slow_queries.sql
   ```

2. **Fix Performance**:
   ```powershell
   .\run_sql.ps1 fix_slow_queries.sql
   ```

3. **Fix Categories**:
   ```powershell
   .\run_sql.ps1 URGENT_FIX_CATEGORIES.sql
   ```

4. **Migrate Products** (IMPORTANT):
   ```powershell
   .\run_sql.ps1 migrate_products_to_categoryid_SSMS.sql
   ```

Or run all at once:
```powershell
.\run_all_fixes.ps1
```

## Troubleshooting

### PowerShell Execution Policy Error
If you get an execution policy error, run:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Connection Timeout
If queries timeout, increase the timeout in the script or use:
```cmd
sqlcmd ... -l 600
```

### Permission Denied
Make sure the user `soitmed` has sufficient permissions:
- ALTER TABLE
- CREATE INDEX
- UPDATE
- SELECT

