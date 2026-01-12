-- Simple fix for Id column types
-- Direct approach without dynamic SQL

USE [ITIWebApi44]
GO

PRINT 'Starting simple Id column fix...'

-- First, let's see what we're working with
SELECT 
    'ProductCategories' AS TableName,
    'Id' AS ColumnName,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ProductCategories' AND COLUMN_NAME = 'Id'
UNION ALL
SELECT 
    'Products' AS TableName,
    'Id' AS ColumnName,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'Id'
GO

-- Drop any remaining constraints that reference ProductCategories.Id
PRINT 'Dropping constraints on ProductCategories.Id...'

-- Drop foreign keys that reference ProductCategories.Id from Products table
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE referenced_object_id = OBJECT_ID('ProductCategories') AND OBJECT_NAME(parent_object_id) = 'Products')
BEGIN
    ALTER TABLE Products DROP CONSTRAINT FK_Products_ProductCategories_CategoryId
    PRINT 'Dropped FK_Products_ProductCategories_CategoryId'
END
GO

-- Drop foreign keys that reference ProductCategories.Id from ProductCategories table (self-reference)
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE referenced_object_id = OBJECT_ID('ProductCategories') AND OBJECT_NAME(parent_object_id) = 'ProductCategories')
BEGIN
    ALTER TABLE ProductCategories DROP CONSTRAINT FK_ProductCategories_ProductCategories_ParentCategoryId
    PRINT 'Dropped FK_ProductCategories_ProductCategories_ParentCategoryId'
END
GO

-- Drop any indexes on Id columns
IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('ProductCategories') AND name LIKE '%Id%')
BEGIN
    DROP INDEX IX_ProductCategories_Id ON ProductCategories
    PRINT 'Dropped IX_ProductCategories_Id'
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Products') AND name LIKE '%Id%')
BEGIN
    DROP INDEX IX_Products_Id ON Products
    PRINT 'Dropped IX_Products_Id'
END
GO

-- Now change the Id columns to NVARCHAR(50)
PRINT 'Changing Id columns to NVARCHAR(50)...'

ALTER TABLE ProductCategories
ALTER COLUMN Id NVARCHAR(50) NOT NULL
PRINT 'Changed ProductCategories.Id to NVARCHAR(50)'

ALTER TABLE Products  
ALTER COLUMN Id NVARCHAR(50) NOT NULL
PRINT 'Changed Products.Id to NVARCHAR(50)'
GO

-- Recreate the foreign key constraints
PRINT 'Recreating foreign key constraints...'

ALTER TABLE ProductCategories 
ADD CONSTRAINT FK_ProductCategories_ProductCategories_ParentCategoryId 
FOREIGN KEY (ParentCategoryId) REFERENCES ProductCategories(Id)
ON DELETE NO ACTION
PRINT 'Recreated FK_ProductCategories_ProductCategories_ParentCategoryId'

ALTER TABLE Products 
ADD CONSTRAINT FK_Products_ProductCategories_CategoryId 
FOREIGN KEY (CategoryId) REFERENCES ProductCategories(Id)
ON DELETE NO ACTION
PRINT 'Recreated FK_Products_ProductCategories_CategoryId'
GO

-- Final verification
PRINT 'Final verification...'
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME IN ('ProductCategories', 'Products') 
    AND COLUMN_NAME IN ('Id', 'ParentCategoryId', 'CategoryId')
ORDER BY TABLE_NAME, COLUMN_NAME
GO

PRINT 'Simple Id column fix completed!'
