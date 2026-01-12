-- Verify the current database schema
-- Check what we actually have after the fix

USE [ITIWebApi44]
GO

PRINT '=== CURRENT DATABASE SCHEMA VERIFICATION ==='

-- Check ProductCategories table structure
PRINT 'ProductCategories table structure:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMNPROPERTY(OBJECT_ID('ProductCategories'), COLUMN_NAME, 'IsIdentity') AS IsIdentity
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ProductCategories' 
    AND COLUMN_NAME IN ('Id', 'ParentCategoryId')
ORDER BY COLUMN_NAME
GO

-- Check Products table structure  
PRINT 'Products table structure:'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMNPROPERTY(OBJECT_ID('Products'), COLUMN_NAME, 'IsIdentity') AS IsIdentity
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Products' 
    AND COLUMN_NAME IN ('Id', 'CategoryId')
ORDER BY COLUMN_NAME
GO

-- Check foreign key constraints
PRINT 'Foreign key constraints:'
SELECT 
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fc ON fk.object_id = fc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) IN ('ProductCategories', 'Products')
ORDER BY TableName, ForeignKeyName
GO

-- Check sample data
PRINT 'Sample ProductCategories data:'
SELECT TOP 3 Id, ParentCategoryId, Name FROM ProductCategories
GO

PRINT 'Sample Products data:'
SELECT TOP 3 Id, CategoryId, Name FROM Products
GO

PRINT '=== SCHEMA VERIFICATION COMPLETED ==='
