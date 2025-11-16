# Syncing Database Changes with Code (After Manual DB Changes)

## Overview
You've made manual changes to the database using SSMS. To ensure migrations work correctly going forward, you need to sync your code models with the database state.

## Step-by-Step Process

### Step 1: Stop the Application
**IMPORTANT**: Stop your running application before proceeding. The application (process 21524) is currently locking files.

### Step 2: Check Current Migration Status
First, let's see what migrations have been applied:

```powershell
cd Backend/SoitMed
dotnet ef migrations list
```

This shows which migrations EF Core thinks are applied to the database.

### Step 3: Create a Test Migration to Detect Differences
Create a new migration to see what EF Core detects as differences between your models and the database:

```powershell
dotnet ef migrations add SyncWithManualDBChanges
```

**Review the generated migration file** in `Migrations/` folder. This will show you:
- What EF Core thinks needs to be added (already in DB from your manual changes)
- What EF Core thinks needs to be removed (if models don't match DB)
- What EF Core thinks needs to be modified

### Step 4: Handle the Migration Based on What You See

#### Scenario A: Migration is Empty (No Changes Detected)
If the migration is empty or has no meaningful changes, your code is already in sync! You can:
1. Delete the migration: `dotnet ef migrations remove`
2. Or keep it as a checkpoint

#### Scenario B: Migration Contains Changes Already in Database
If the migration shows changes that you've already applied manually:

1. **Option 1: Mark Migration as Applied Without Running It** (Recommended)
   ```powershell
   # First, check the migration name
   dotnet ef migrations list
   
   # Mark it as applied without executing it
   dotnet ef database update SyncWithManualDBChanges --no-transactions
   ```
   
   However, if this fails because the changes are already there, use Option 2.

2. **Option 2: Create Empty Migration and Mark as Applied**
   ```powershell
   # Remove the migration we just created
   dotnet ef migrations remove
   
   # Create an empty migration
   dotnet ef migrations add SyncWithManualDBChanges --empty
   
   # Edit the migration file to be empty (or add comments)
   # Then mark it as applied
   dotnet ef database update SyncWithManualDBChanges
   ```

#### Scenario C: Migration Shows Missing Changes in Code Models
If the migration shows that your code models are missing properties/columns that exist in the database:

1. **Update your C# models** to match the database schema
2. **Remove the migration**: `dotnet ef migrations remove`
3. **Create a new migration**: `dotnet ef migrations add SyncWithManualDBChanges`
4. **Review and apply**: The migration should now be empty or minimal

### Step 5: Verify Sync Status
After syncing, verify everything is in sync:

```powershell
# Try creating another test migration
dotnet ef migrations add TestSyncCheck

# Check if it's empty (good sign - means everything is synced)
# If empty, remove it:
dotnet ef migrations remove
```

### Step 6: Test Future Migrations
Create a test migration to ensure future migrations will work:

```powershell
# Create a dummy migration
dotnet ef migrations add TestFutureMigration

# Check the generated file - it should be empty or minimal
# Remove it if it's empty:
dotnet ef migrations remove
```

## Alternative: Using EF Core Power Tools (Visual Studio)
If you have Visual Studio, you can use EF Core Power Tools to:
1. Compare your models with the database
2. Generate a migration script that matches the current database state

## Quick Commands Reference

```powershell
# Navigate to project
cd Backend/SoitMed

# List all migrations
dotnet ef migrations list

# Create new migration
dotnet ef migrations add MigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove

# Apply migrations to database
dotnet ef database update

# Apply specific migration
dotnet ef database update MigrationName

# Generate SQL script (without applying)
dotnet ef migrations script

# Generate SQL script from specific migration
dotnet ef migrations script FromMigration ToMigration
```

## Important Notes

1. **Always backup your database** before running migrations
2. **Stop the application** before creating/applying migrations
3. **Review migration files** before applying them
4. **Test in development** before applying to production
5. **Keep migrations in source control** - they're part of your codebase

## Troubleshooting

### "The migration already exists in the database"
This means EF Core thinks the migration is applied, but your code models don't match. You need to:
1. Update your models to match the database
2. Create a new migration (should be empty if everything matches)

### "Cannot create migration - build errors"
Fix any compilation errors first, then try again.

### "Database update fails"
Check if the changes in the migration conflict with existing database schema. You may need to manually edit the migration file.

## Next Steps After Syncing

Once synced, you can:
1. Create new migrations normally: `dotnet ef migrations add YourNewFeature`
2. Apply them: `dotnet ef database update`
3. Continue development with confidence that migrations will work







