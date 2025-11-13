# Applying the Sync Migration

## Current Situation
The migration `SyncWithManualDBChanges_20251113123136` detected these changes:
1. `SalesmanTargets.CreatedByManagerId` → nullable
2. `SalesmanTargets.TargetRevenue` → new column
3. `SalesmanTargets.TargetType` → new column  
4. `RecentOfferActivities` → new table

Your C# models already have these properties, so the code is correct.

## Step 1: Check if Changes Already Exist in Database

Run this SQL in SSMS to check:

```sql
-- Check if RecentOfferActivities table exists
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'RecentOfferActivities';

-- Check if SalesmanTargets has the new columns
SELECT COLUMN_NAME, IS_NULLABLE, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SalesmanTargets' 
AND COLUMN_NAME IN ('TargetRevenue', 'TargetType', 'CreatedByManagerId');
```

## Step 2: Apply the Migration

### If changes are NOT in database:
```powershell
dotnet ef database update
```

### If changes ARE already in database:

You need to mark the migration as applied without running it. Unfortunately, EF Core doesn't have a direct command for this, so you have two options:

**Option A: Manually insert into __EFMigrationsHistory**
```sql
-- Get the migration name from the file: 20251113103153_SyncWithManualDBChanges_20251113123136
INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES ('20251113103153_SyncWithManualDBChanges_20251113123136', '8.0.3');
```

**Option B: Modify migration to be idempotent, then apply**
Edit the migration file to make operations idempotent (check if exists before creating), then apply it.

## Step 3: Verify Sync

After applying/marking as applied, verify everything is synced:

```powershell
dotnet ef migrations add TestSyncCheck
```

If this creates an empty migration, you're good! Remove it:
```powershell
dotnet ef migrations remove
```

## Recommended Approach

1. **First, check in SSMS** if the changes exist
2. **If they exist**: Use Option A above to mark migration as applied
3. **If they don't exist**: Run `dotnet ef database update`
4. **Verify** with the test migration

