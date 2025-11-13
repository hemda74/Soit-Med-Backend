# Fix: Invalid column name 'Year' Error

## Problem

The `OfferEquipment` model has a `Year` property in the C# code, but the database table doesn't have the corresponding column. This causes a SQL error when trying to insert or query equipment.

## Root Cause

The `Year` property was added to the `OfferEquipment.cs` model after the last migration was created, but no migration was generated to update the database schema.

## Solution

### Option 1: Apply SQL Script Directly (Quickest - Recommended)

1. **Stop the backend server** (this is important for Option 2, but for SQL script you can keep it running)

2. **Run the SQL migration script**:

      - Open SQL Server Management Studio (SSMS) or Azure Data Studio
      - Connect to your database
      - Open and execute: `Soit-Med-Backend/SoitMed/Scripts/AddYearColumnToOfferEquipment.sql`

3. **Restart the backend** (if you stopped it)

The script is idempotent - it checks if the column exists before adding it, so it's safe to run multiple times.

### Option 2: Generate Proper EF Migration (More Complete)

1. **Stop the backend server** (required - the running process locks the files)

2. **Navigate to the SoitMed project**:

      ```powershell
      cd Soit-Med-Backend/SoitMed
      ```

3. **Create the migration**:

      ```powershell
      dotnet ef migrations add AddYearToOfferEquipment
      ```

4. **Apply the migration**:

      ```powershell
      dotnet ef database update
      ```

5. **Start the backend again**

## Verification

After applying either fix, try creating an offer again. The error should be resolved.

You can verify the column exists by running:

```sql
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'OfferEquipment'
ORDER BY ORDINAL_POSITION;
```

## Technical Details

- **Model File**: `Soit-Med-Backend/SoitMed/Models/OfferEquipment.cs` (line 30)
- **Property**: `public int? Year { get; set; }`
- **Error**: `Microsoft.Data.SqlClient.SqlException: Invalid column name 'Year'`
- **SQL Script**: `Soit-Med-Backend/SoitMed/Scripts/AddYearColumnToOfferEquipment.sql`

## Why This Happened

The `Year` property was likely added to support equipment manufacturing year tracking, but the developer forgot to create and apply a migration. Entity Framework tried to map this property to a database column that doesn't exist, causing the SQL error.
