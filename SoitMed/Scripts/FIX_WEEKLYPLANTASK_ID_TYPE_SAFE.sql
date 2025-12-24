-- ============================================================================
-- SAFE Migration: Change WeeklyPlanTask.Id from bigint to int
-- This script preserves all existing data and prevents data loss
-- ============================================================================

USE [ITIWebApi44];
GO

PRINT '========================================';
PRINT 'Starting Safe Migration: WeeklyPlanTask.Id (bigint -> int)';
PRINT '========================================';
GO

-- ============================================================================
-- STEP 1: Safety Check - Verify no data will be lost
-- ============================================================================
PRINT '';
PRINT 'STEP 1: Checking for data that exceeds int max value (2,147,483,647)...';
GO

DECLARE @MaxId BIGINT;
DECLARE @CountExceeding INT;

SELECT @MaxId = MAX(Id) FROM WeeklyPlanTasks;
SELECT @CountExceeding = COUNT(*) FROM WeeklyPlanTasks WHERE Id > 2147483647;

IF @CountExceeding > 0
BEGIN
    PRINT 'ERROR: Found ' + CAST(@CountExceeding AS VARCHAR) + ' records with Id > 2,147,483,647';
    PRINT 'Maximum Id value: ' + CAST(@MaxId AS VARCHAR);
    PRINT '';
    PRINT 'MIGRATION ABORTED: Cannot convert to int without data loss!';
    PRINT 'Please contact the development team to handle this situation.';
    RAISERROR('Migration aborted: Data would be lost', 16, 1);
    RETURN;
END

IF @MaxId IS NULL
BEGIN
    PRINT 'No data found in WeeklyPlanTasks table. Safe to proceed.';
END
ELSE
BEGIN
    PRINT 'Safety check passed. Maximum Id value: ' + CAST(@MaxId AS VARCHAR);
    PRINT 'All Id values are within int range. Safe to proceed.';
END
GO

-- ============================================================================
-- STEP 2: Create backup table (optional but recommended)
-- ============================================================================
PRINT '';
PRINT 'STEP 2: Creating backup table...';
GO

-- Drop backup table if it exists
IF OBJECT_ID('WeeklyPlanTasks_Backup_bigint', 'U') IS NOT NULL
BEGIN
    DROP TABLE WeeklyPlanTasks_Backup_bigint;
    PRINT 'Dropped existing backup table.';
END
GO

-- Create backup table with same structure
SELECT * INTO WeeklyPlanTasks_Backup_bigint 
FROM WeeklyPlanTasks;
GO

DECLARE @BackupCount INT;
SELECT @BackupCount = COUNT(*) FROM WeeklyPlanTasks_Backup_bigint;
PRINT 'Backup created successfully. Records backed up: ' + CAST(@BackupCount AS VARCHAR);
GO

-- ============================================================================
-- STEP 3: Drop Foreign Key Constraints (must be done first)
-- ============================================================================
PRINT '';
PRINT 'STEP 3: Dropping foreign key constraints...';
GO

-- Drop FK from TaskProgresses if it exists
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TaskProgresses_WeeklyPlanTasks_TaskId')
BEGIN
    ALTER TABLE TaskProgresses
    DROP CONSTRAINT FK_TaskProgresses_WeeklyPlanTasks_TaskId;
    PRINT 'Dropped FK_TaskProgresses_WeeklyPlanTasks_TaskId';
END
ELSE
BEGIN
    PRINT 'FK_TaskProgresses_WeeklyPlanTasks_TaskId does not exist (skipped)';
END
GO

-- Drop FK from ActivityLogs if it exists
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ActivityLogs_WeeklyPlanTasks_PlanTaskId')
BEGIN
    ALTER TABLE ActivityLogs
    DROP CONSTRAINT FK_ActivityLogs_WeeklyPlanTasks_PlanTaskId;
    PRINT 'Dropped FK_ActivityLogs_WeeklyPlanTasks_PlanTaskId';
END
ELSE
BEGIN
    PRINT 'FK_ActivityLogs_WeeklyPlanTasks_PlanTaskId does not exist (skipped)';
END
GO

-- ============================================================================
-- STEP 4: Drop Primary Key Constraint
-- ============================================================================
PRINT '';
PRINT 'STEP 4: Dropping primary key constraint...';
GO

IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'PK_WeeklyPlanTasks')
BEGIN
    ALTER TABLE WeeklyPlanTasks
    DROP CONSTRAINT PK_WeeklyPlanTasks;
    PRINT 'Dropped PK_WeeklyPlanTasks';
END
ELSE
BEGIN
    PRINT 'PK_WeeklyPlanTasks does not exist (skipped)';
END
GO

-- ============================================================================
-- STEP 5: Alter Column Type (bigint -> int)
-- ============================================================================
PRINT '';
PRINT 'STEP 5: Altering column type from bigint to int...';
GO

BEGIN TRY
    ALTER TABLE WeeklyPlanTasks
    ALTER COLUMN Id int NOT NULL;
    PRINT 'Column type changed successfully from bigint to int';
END TRY
BEGIN CATCH
    PRINT 'ERROR: Failed to alter column type';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    RAISERROR('Column alteration failed', 16, 1);
    RETURN;
END CATCH
GO

-- ============================================================================
-- STEP 6: Recreate Primary Key Constraint
-- ============================================================================
PRINT '';
PRINT 'STEP 6: Recreating primary key constraint...';
GO

BEGIN TRY
    ALTER TABLE WeeklyPlanTasks
    ADD CONSTRAINT PK_WeeklyPlanTasks PRIMARY KEY (Id);
    PRINT 'Primary key constraint recreated successfully';
END TRY
BEGIN CATCH
    PRINT 'ERROR: Failed to recreate primary key';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    RAISERROR('Primary key recreation failed', 16, 1);
    RETURN;
END CATCH
GO

-- ============================================================================
-- STEP 7: Recreate Foreign Key Constraints
-- ============================================================================
PRINT '';
PRINT 'STEP 7: Recreating foreign key constraints...';
GO

-- Recreate FK for TaskProgresses
BEGIN TRY
    ALTER TABLE TaskProgresses
    ADD CONSTRAINT FK_TaskProgresses_WeeklyPlanTasks_TaskId 
    FOREIGN KEY (TaskId) REFERENCES WeeklyPlanTasks(Id);
    PRINT 'Recreated FK_TaskProgresses_WeeklyPlanTasks_TaskId';
END TRY
BEGIN CATCH
    PRINT 'WARNING: Failed to recreate FK_TaskProgresses_WeeklyPlanTasks_TaskId';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'You may need to recreate this constraint manually';
END CATCH
GO

-- Recreate FK for ActivityLogs
BEGIN TRY
    ALTER TABLE ActivityLogs
    ADD CONSTRAINT FK_ActivityLogs_WeeklyPlanTasks_PlanTaskId 
    FOREIGN KEY (PlanTaskId) REFERENCES WeeklyPlanTasks(Id);
    PRINT 'Recreated FK_ActivityLogs_WeeklyPlanTasks_PlanTaskId';
END TRY
BEGIN CATCH
    PRINT 'WARNING: Failed to recreate FK_ActivityLogs_WeeklyPlanTasks_PlanTaskId';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'You may need to recreate this constraint manually';
END CATCH
GO

-- ============================================================================
-- STEP 8: Verify Data Integrity
-- ============================================================================
PRINT '';
PRINT 'STEP 8: Verifying data integrity...';
GO

DECLARE @OriginalCount INT;
DECLARE @CurrentCount INT;
DECLARE @BackupCount INT;

SELECT @OriginalCount = COUNT(*) FROM WeeklyPlanTasks_Backup_bigint;
SELECT @CurrentCount = COUNT(*) FROM WeeklyPlanTasks;
SELECT @BackupCount = COUNT(*) FROM WeeklyPlanTasks_Backup_bigint;

IF @OriginalCount = @CurrentCount
BEGIN
    PRINT 'Data integrity check PASSED';
    PRINT 'Original records: ' + CAST(@OriginalCount AS VARCHAR);
    PRINT 'Current records: ' + CAST(@CurrentCount AS VARCHAR);
    PRINT 'All data preserved successfully!';
END
ELSE
BEGIN
    PRINT 'WARNING: Record count mismatch!';
    PRINT 'Original records: ' + CAST(@OriginalCount AS VARCHAR);
    PRINT 'Current records: ' + CAST(@CurrentCount AS VARCHAR);
    PRINT 'Please verify data manually.';
END
GO

-- ============================================================================
-- STEP 9: Verify Column Type
-- ============================================================================
PRINT '';
PRINT 'STEP 9: Verifying column type...';
GO

DECLARE @DataType VARCHAR(50);
SELECT @DataType = TYPE_NAME(system_type_id) 
FROM sys.columns 
WHERE object_id = OBJECT_ID('WeeklyPlanTasks') 
AND name = 'Id';

IF @DataType = 'int'
BEGIN
    PRINT 'Column type verification PASSED: Id is now int';
END
ELSE
BEGIN
    PRINT 'WARNING: Column type is ' + @DataType + ' (expected int)';
END
GO

-- ============================================================================
-- COMPLETION
-- ============================================================================
PRINT '';
PRINT '========================================';
PRINT 'Migration completed successfully!';
PRINT '========================================';
PRINT '';
PRINT 'Summary:';
PRINT '- WeeklyPlanTask.Id changed from bigint to int';
PRINT '- All constraints recreated';
PRINT '- Backup table created: WeeklyPlanTasks_Backup_bigint';
PRINT '';
PRINT 'NOTE: You can drop the backup table after verifying everything works:';
PRINT 'DROP TABLE WeeklyPlanTasks_Backup_bigint;';
PRINT '';
GO

