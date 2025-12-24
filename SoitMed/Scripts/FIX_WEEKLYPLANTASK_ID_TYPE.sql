-- Fix WeeklyPlanTask.Id column type from bigint to int
-- This script changes the Id column from bigint to int to match the C# model

USE [ITIWebApi44];
GO

-- Step 1: Check if any existing Id values exceed int max (2,147,483,647)
-- If this query returns any rows, you cannot convert to int without data loss
SELECT Id, COUNT(*) as Count
FROM WeeklyPlanTasks
WHERE Id > 2147483647
GROUP BY Id;
GO

-- Step 2: Check for foreign key constraints that reference WeeklyPlanTasks.Id
-- These need to be dropped before altering the column
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS ReferencingTable,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ReferencingColumn
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.referenced_object_id) = 'WeeklyPlanTasks'
    AND COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) = 'Id';
GO

-- Step 3: Drop foreign key constraints FIRST (they reference the primary key)
ALTER TABLE TaskProgresses
DROP CONSTRAINT FK_TaskProgresses_WeeklyPlanTasks_TaskId;
GO

ALTER TABLE ActivityLogs
DROP CONSTRAINT FK_ActivityLogs_WeeklyPlanTasks_PlanTaskId;
GO

-- Step 4: Drop Primary Key constraint (after foreign keys are dropped)
ALTER TABLE WeeklyPlanTasks
DROP CONSTRAINT PK_WeeklyPlanTasks;
GO

-- Step 5: Alter the column type from bigint to int
-- WARNING: This will fail if any Id values exceed 2,147,483,647
ALTER TABLE WeeklyPlanTasks
ALTER COLUMN Id int NOT NULL;
GO

-- Step 6: Recreate Primary Key constraint
ALTER TABLE WeeklyPlanTasks
ADD CONSTRAINT PK_WeeklyPlanTasks PRIMARY KEY (Id);
GO

-- Step 7: Recreate foreign key constraints
ALTER TABLE TaskProgresses
ADD CONSTRAINT FK_TaskProgresses_WeeklyPlanTasks_TaskId 
FOREIGN KEY (TaskId) REFERENCES WeeklyPlanTasks(Id);
GO

ALTER TABLE ActivityLogs
ADD CONSTRAINT FK_ActivityLogs_WeeklyPlanTasks_PlanTaskId 
FOREIGN KEY (PlanTaskId) REFERENCES WeeklyPlanTasks(Id);
GO

PRINT 'WeeklyPlanTask.Id column type changed from bigint to int successfully!';
GO

