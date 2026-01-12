-- Simple connection test
SELECT @@SERVERNAME AS ServerName, DB_NAME() AS DatabaseName, GETDATE() AS CurrentTime
GO

-- Check if ProductCategories table exists
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProductCategories'
GO

-- Check current column types
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ProductCategories' 
    AND COLUMN_NAME = 'ParentCategoryId'
GO
