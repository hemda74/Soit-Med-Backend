-- Migration script to add TargetType and TargetRevenue columns to SalesmanTargets table
-- Run this script on your database to update the schema

DECLARE @TargetTypeAdded BIT = 0;
DECLARE @TargetRevenueAdded BIT = 0;

-- Check if TargetType column exists, if not add it
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[SalesmanTargets]') 
    AND name = 'TargetType'
)
BEGIN
    ALTER TABLE [dbo].[SalesmanTargets]
    ADD [TargetType] INT NOT NULL DEFAULT 2; -- Default to Activity (2) for existing records
    
    SET @TargetTypeAdded = 1;
    PRINT 'Added TargetType column to SalesmanTargets table';
END
ELSE
BEGIN
    PRINT 'TargetType column already exists in SalesmanTargets table';
END

-- Check if TargetRevenue column exists, if not add it
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[SalesmanTargets]') 
    AND name = 'TargetRevenue'
)
BEGIN
    ALTER TABLE [dbo].[SalesmanTargets]
    ADD [TargetRevenue] DECIMAL(18, 2) NULL;
    
    SET @TargetRevenueAdded = 1;
    PRINT 'Added TargetRevenue column to SalesmanTargets table';
END
ELSE
BEGIN
    PRINT 'TargetRevenue column already exists in SalesmanTargets table';
END

-- Only update records if TargetType was just added
-- Note: Since TargetRevenue is NULL for existing activity targets, 
-- all existing records will remain as Activity (2) which is correct
IF @TargetTypeAdded = 1
BEGIN
    -- All existing records are already set to Activity (2) by the DEFAULT value
    -- No need to update them since they don't have TargetRevenue set
    PRINT 'Existing records set to Activity type (default)';
END

PRINT 'Migration completed successfully';

