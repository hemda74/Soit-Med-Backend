-- Migration: Add Equipment Categories to Clients and WeeklyPlanTasks
-- Date: 2025-01-XX
-- Description: Adds InterestedEquipmentCategories to Clients table and EquipmentCategories to WeeklyPlanTasks table

-- Add InterestedEquipmentCategories column to Clients table
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') 
    AND name = 'InterestedEquipmentCategories'
)
BEGIN
    ALTER TABLE [dbo].[Clients]
    ADD InterestedEquipmentCategories NVARCHAR(MAX) NULL;
    
    PRINT 'Added InterestedEquipmentCategories column to Clients table';
END
ELSE
BEGIN
    PRINT 'InterestedEquipmentCategories column already exists in Clients table';
END
GO

-- Add EquipmentCategories column to WeeklyPlanTasks table
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[WeeklyPlanTasks]') 
    AND name = 'EquipmentCategories'
)
BEGIN
    ALTER TABLE [dbo].[WeeklyPlanTasks]
    ADD EquipmentCategories NVARCHAR(MAX) NULL;
    
    PRINT 'Added EquipmentCategories column to WeeklyPlanTasks table';
END
ELSE
BEGIN
    PRINT 'EquipmentCategories column already exists in WeeklyPlanTasks table';
END
GO

-- Add indexes for better query performance (optional, for JSON queries)
-- Note: SQL Server 2016+ supports JSON indexes, but for simplicity we'll rely on full-text search
-- If needed, consider adding computed columns for specific categories

PRINT 'Migration completed successfully';
PRINT '';
PRINT 'NOTE: After running this migration, you can populate existing data by running:';
PRINT 'POPULATE_EQUIPMENT_CATEGORIES_FOR_EXISTING_DATA.sql';
PRINT 'This will extract equipment categories from existing offers and populate the new columns.';
GO

