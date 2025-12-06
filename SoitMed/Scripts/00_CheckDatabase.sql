-- =====================================================
-- Database Diagnostic Script
-- Run this FIRST to check your database setup
-- =====================================================

PRINT '========================================';
PRINT 'DATABASE DIAGNOSTIC CHECK';
PRINT '========================================';
PRINT '';

-- Check current database
PRINT 'Current Database: ' + DB_NAME();
PRINT '';

-- Check if Products table exists
PRINT 'Checking for Products table...';
IF OBJECT_ID('Products', 'U') IS NOT NULL
    PRINT '✓ Products table EXISTS';
ELSE IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL
    PRINT '✓ Products table EXISTS (in dbo schema)';
ELSE
BEGIN
    PRINT '✗ Products table DOES NOT EXIST!';
    PRINT '';
    PRINT 'ERROR: You need to run migrations first!';
    PRINT 'Run this command in Backend/SoitMed folder:';
    PRINT '  dotnet ef database update';
    PRINT '';
END

-- Check if ProductCategories table exists
PRINT 'Checking for ProductCategories table...';
IF OBJECT_ID('ProductCategories', 'U') IS NOT NULL
    PRINT '✓ ProductCategories table EXISTS';
ELSE IF OBJECT_ID('dbo.ProductCategories', 'U') IS NOT NULL
    PRINT '✓ ProductCategories table EXISTS (in dbo schema)';
ELSE
BEGIN
    PRINT '✗ ProductCategories table DOES NOT EXIST!';
    PRINT '';
    PRINT 'ERROR: You need to run migrations first!';
    PRINT 'Run this command in Backend/SoitMed folder:';
    PRINT '  dotnet ef database update';
    PRINT '';
END

PRINT '';

-- List all tables in database
PRINT 'All tables in current database:';
SELECT 
    TABLE_SCHEMA,
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_SCHEMA, TABLE_NAME;

PRINT '';
PRINT '========================================';
PRINT 'If Products table is missing, run:';
PRINT '  cd Backend/SoitMed';
PRINT '  dotnet ef database update';
PRINT '========================================';



