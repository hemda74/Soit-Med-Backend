-- =====================================================
-- IMPORTANT: Run this FIRST!
-- This ensures you're using the correct database
-- =====================================================

PRINT '========================================';
PRINT 'SWITCHING TO CORRECT DATABASE';
PRINT '========================================';
PRINT '';

-- Switch to the correct database
USE ITIWebApi44;
GO

-- Verify we're in the right database
PRINT 'Current Database: ' + DB_NAME();
PRINT '';

-- Check if Products table exists
IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL
    PRINT '✓ Products table EXISTS in this database!';
ELSE
    PRINT '✗ Products table DOES NOT EXIST in this database!';

-- Check if ProductCategories table exists
IF OBJECT_ID('dbo.ProductCategories', 'U') IS NOT NULL
    PRINT '✓ ProductCategories table EXISTS in this database!';
ELSE
    PRINT '✗ ProductCategories table DOES NOT EXIST in this database!';

PRINT '';
PRINT '========================================';
PRINT 'If both tables exist, you can now run:';
PRINT 'SeedProductsFromPDF_Safe.sql';
PRINT '========================================';



