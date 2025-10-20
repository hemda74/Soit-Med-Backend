-- =====================================================
-- Database Structure Debug Script
-- =====================================================
-- This script will help us understand what tables exist and their structure

PRINT '=====================================================';
PRINT 'CHECKING EXISTING TABLES';
PRINT '=====================================================';

-- Check if WeeklyPlans table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WeeklyPlans]') AND type in (N'U'))
BEGIN
    PRINT 'WeeklyPlans table EXISTS';
    
    -- Show table structure
    PRINT 'WeeklyPlans table structure:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'WeeklyPlans'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT 'WeeklyPlans table does NOT exist';
END

PRINT '';

-- Check if WeeklyPlanItems table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WeeklyPlanItems]') AND type in (N'U'))
BEGIN
    PRINT 'WeeklyPlanItems table EXISTS';
    
    -- Show table structure
    PRINT 'WeeklyPlanItems table structure:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'WeeklyPlanItems'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT 'WeeklyPlanItems table does NOT exist';
END

PRINT '';

-- Check if Clients table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') AND type in (N'U'))
BEGIN
    PRINT 'Clients table EXISTS';
    
    -- Show table structure
    PRINT 'Clients table structure:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Clients'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT 'Clients table does NOT exist';
END

PRINT '';

-- Check if SalesReports table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalesReports]') AND type in (N'U'))
BEGIN
    PRINT 'SalesReports table EXISTS';
    
    -- Show table structure
    PRINT 'SalesReports table structure:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH,
        IS_NULLABLE,
        COLUMN_DEFAULT
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'SalesReports'
    ORDER BY ORDINAL_POSITION;
END
ELSE
BEGIN
    PRINT 'SalesReports table does NOT exist';
END

PRINT '';

-- Show all tables that start with 'Weekly' or 'Client' or 'Sales'
PRINT 'All related tables in database:';
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' 
AND (TABLE_NAME LIKE '%Weekly%' OR TABLE_NAME LIKE '%Client%' OR TABLE_NAME LIKE '%Sales%')
ORDER BY TABLE_NAME;

PRINT '';

-- Check if AspNetUsers table exists
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND type in (N'U'))
BEGIN
    PRINT 'AspNetUsers table EXISTS';
    
    -- Check if ahmed@soitmed.com exists
    IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'ahmed@soitmed.com')
    BEGIN
        PRINT 'User ahmed@soitmed.com EXISTS';
        SELECT Id, Email, UserName FROM AspNetUsers WHERE Email = 'ahmed@soitmed.com';
    END
    ELSE
    BEGIN
        PRINT 'User ahmed@soitmed.com does NOT exist';
        PRINT 'Available users:';
        SELECT TOP 5 Id, Email, UserName FROM AspNetUsers;
    END
END
ELSE
BEGIN
    PRINT 'AspNetUsers table does NOT exist';
END

PRINT '=====================================================';
PRINT 'DEBUG COMPLETE';
PRINT '=====================================================';
