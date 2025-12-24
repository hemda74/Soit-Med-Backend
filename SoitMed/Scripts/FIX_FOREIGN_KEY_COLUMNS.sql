-- ============================================================================
-- Fix Foreign Key Columns: Change TaskId and PlanTaskId from bigint to int
-- This script fixes the columns that reference WeeklyPlanTasks.Id
-- ============================================================================

USE [ITIWebApi44];
GO

PRINT '========================================';
PRINT 'Fixing Foreign Key Columns (bigint -> int)';
PRINT '========================================';
GO

-- ============================================================================
-- STEP 1: Check current column types
-- ============================================================================
PRINT '';
PRINT 'STEP 1: Checking current column types...';
GO

SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    TYPE_NAME(c.system_type_id) AS DataType
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE (t.name = 'TaskProgresses' AND c.name = 'TaskId')
   OR (t.name = 'ActivityLogs' AND c.name = 'PlanTaskId')
ORDER BY t.name, c.name;
GO

-- ============================================================================
-- STEP 2: Check for data that exceeds int max
-- ============================================================================
PRINT '';
PRINT 'STEP 2: Checking for data that exceeds int max...';
GO

-- Check TaskProgresses.TaskId
DECLARE @TaskProgressCount INT;
SELECT @TaskProgressCount = COUNT(*) 
FROM TaskProgresses 
WHERE TaskId > 2147483647;

IF @TaskProgressCount > 0
BEGIN
    PRINT 'ERROR: Found ' + CAST(@TaskProgressCount AS VARCHAR) + ' TaskProgresses records with TaskId > 2,147,483,647';
    RAISERROR('Cannot convert TaskProgresses.TaskId to int - data would be lost', 16, 1);
    RETURN;
END
ELSE
BEGIN
    PRINT 'TaskProgresses.TaskId: All values are within int range';
END
GO

-- Check ActivityLogs.PlanTaskId
DECLARE @ActivityLogCount INT;
SELECT @ActivityLogCount = COUNT(*) 
FROM ActivityLogs 
WHERE PlanTaskId > 2147483647;

IF @ActivityLogCount > 0
BEGIN
    PRINT 'ERROR: Found ' + CAST(@ActivityLogCount AS VARCHAR) + ' ActivityLogs records with PlanTaskId > 2,147,483,647';
    RAISERROR('Cannot convert ActivityLogs.PlanTaskId to int - data would be lost', 16, 1);
    RETURN;
END
ELSE
BEGIN
    PRINT 'ActivityLogs.PlanTaskId: All values are within int range';
END
GO

-- ============================================================================
-- STEP 3: Drop Foreign Key Constraints (if they exist)
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
-- STEP 4: Alter TaskProgresses.TaskId column
-- ============================================================================
PRINT '';
PRINT 'STEP 4: Altering TaskProgresses.TaskId column...';
GO

BEGIN TRY
    ALTER TABLE TaskProgresses
    ALTER COLUMN TaskId int NOT NULL;
    PRINT 'TaskProgresses.TaskId changed successfully from bigint to int';
END TRY
BEGIN CATCH
    DECLARE @ErrorMsg1 NVARCHAR(4000) = ERROR_MESSAGE();
    PRINT 'ERROR: Failed to alter TaskProgresses.TaskId';
    PRINT 'Error: ' + @ErrorMsg1;
    -- Don't abort, continue with ActivityLogs
END CATCH
GO

-- ============================================================================
-- STEP 5: Alter ActivityLogs.PlanTaskId column
-- ============================================================================
PRINT '';
PRINT 'STEP 5: Altering ActivityLogs.PlanTaskId column...';
GO

BEGIN TRY
    ALTER TABLE ActivityLogs
    ALTER COLUMN PlanTaskId int NOT NULL;
    PRINT 'ActivityLogs.PlanTaskId changed successfully from bigint to int';
END TRY
BEGIN CATCH
    DECLARE @ErrorMsg2 NVARCHAR(4000) = ERROR_MESSAGE();
    PRINT 'ERROR: Failed to alter ActivityLogs.PlanTaskId';
    PRINT 'Error: ' + @ErrorMsg2;
END CATCH
GO

-- ============================================================================
-- STEP 6: Recreate Foreign Key Constraints
-- ============================================================================
PRINT '';
PRINT 'STEP 6: Recreating foreign key constraints...';
GO

-- Recreate FK for TaskProgresses
BEGIN TRY
    ALTER TABLE TaskProgresses
    ADD CONSTRAINT FK_TaskProgresses_WeeklyPlanTasks_TaskId 
    FOREIGN KEY (TaskId) REFERENCES WeeklyPlanTasks(Id);
    PRINT 'Recreated FK_TaskProgresses_WeeklyPlanTasks_TaskId successfully';
END TRY
BEGIN CATCH
    DECLARE @ErrorMsg3 NVARCHAR(4000) = ERROR_MESSAGE();
    PRINT 'ERROR: Failed to recreate FK_TaskProgresses_WeeklyPlanTasks_TaskId';
    PRINT 'Error: ' + @ErrorMsg3;
END CATCH
GO

-- Recreate FK for ActivityLogs
BEGIN TRY
    ALTER TABLE ActivityLogs
    ADD CONSTRAINT FK_ActivityLogs_WeeklyPlanTasks_PlanTaskId 
    FOREIGN KEY (PlanTaskId) REFERENCES WeeklyPlanTasks(Id);
    PRINT 'Recreated FK_ActivityLogs_WeeklyPlanTasks_PlanTaskId successfully';
END TRY
BEGIN CATCH
    DECLARE @ErrorMsg4 NVARCHAR(4000) = ERROR_MESSAGE();
    PRINT 'ERROR: Failed to recreate FK_ActivityLogs_WeeklyPlanTasks_PlanTaskId';
    PRINT 'Error: ' + @ErrorMsg4;
END CATCH
GO

-- ============================================================================
-- STEP 7: Verify Column Types
-- ============================================================================
PRINT '';
PRINT 'STEP 7: Verifying column types...';
GO

SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    TYPE_NAME(c.system_type_id) AS DataType,
    CASE 
        WHEN TYPE_NAME(c.system_type_id) = 'int' THEN '✓ Correct'
        ELSE '✗ Wrong type'
    END AS Status
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
WHERE (t.name = 'TaskProgresses' AND c.name = 'TaskId')
   OR (t.name = 'ActivityLogs' AND c.name = 'PlanTaskId')
   OR (t.name = 'WeeklyPlanTasks' AND c.name = 'Id')
ORDER BY t.name, c.name;
GO

-- ============================================================================
-- COMPLETION
-- ============================================================================
PRINT '';
PRINT '========================================';
PRINT 'Foreign Key Columns Fix Completed!';
PRINT '========================================';
PRINT '';
PRINT 'All foreign key columns should now be int type.';
PRINT 'Verify the column types in the output above.';
PRINT '';
GO

