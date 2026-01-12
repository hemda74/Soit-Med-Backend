-- Complete fix for ProductCategory type mismatch
-- This script handles ALL dependencies systematically

USE [ITIWebApi44]
GO

PRINT 'Starting complete ProductCategory type fix...'

-- Step 1: Drop ALL foreign key constraints that reference these columns
PRINT 'Step 1: Dropping foreign key constraints...'

DECLARE @sql NVARCHAR(MAX) = ''

-- Drop foreign keys on ProductCategories.ParentCategoryId
SELECT @sql = @sql + 'ALTER TABLE ' + OBJECT_SCHEMA_NAME(parent_object_id) + '.' + OBJECT_NAME(parent_object_id) + ' DROP CONSTRAINT ' + name + CHAR(13) + CHAR(10)
FROM sys.foreign_keys
WHERE referenced_object_id = OBJECT_ID('ProductCategories') 
AND COL_NAME(referenced_object_id, referenced_column_id) = 'Id'
AND OBJECT_NAME(parent_object_id) = 'ProductCategories'

-- Drop foreign keys on Products.CategoryId  
SELECT @sql = @sql + 'ALTER TABLE ' + OBJECT_SCHEMA_NAME(parent_object_id) + '.' + OBJECT_NAME(parent_object_id) + ' DROP CONSTRAINT ' + name + CHAR(13) + CHAR(10)
FROM sys.foreign_keys
WHERE referenced_object_id = OBJECT_ID('ProductCategories') 
AND COL_NAME(referenced_object_id, referenced_column_id) = 'Id'
AND OBJECT_NAME(parent_object_id) = 'Products'

IF LEN(@sql) > 0
BEGIN
    EXEC sp_executesql @sql
    PRINT 'Dropped all foreign key constraints'
END
GO

-- Step 2: Drop ALL indexes that depend on these columns
PRINT 'Step 2: Dropping indexes...'

DECLARE @index_sql NVARCHAR(MAX) = ''

-- Drop indexes on ParentCategoryId
SELECT @index_sql = @index_sql + 'DROP INDEX ' + name + ' ON ' + OBJECT_SCHEMA_NAME(object_id) + '.' + OBJECT_NAME(object_id) + CHAR(13) + CHAR(10)
FROM sys.indexes
WHERE object_id IN (OBJECT_ID('ProductCategories'), OBJECT_ID('Products'))
AND COL_NAME(object_id, index_column_id) IN ('ParentCategoryId', 'CategoryId')

IF LEN(@index_sql) > 0
BEGIN
    EXEC sp_executesql @index_sql
    PRINT 'Dropped all dependent indexes'
END
GO

-- Step 3: Now alter the columns
PRINT 'Step 3: Altering column types...'

ALTER TABLE ProductCategories 
ALTER COLUMN ParentCategoryId NVARCHAR(50) NULL
PRINT 'Changed ProductCategories.ParentCategoryId to NVARCHAR(50)'

ALTER TABLE Products 
ALTER COLUMN CategoryId NVARCHAR(50) NULL  
PRINT 'Changed Products.CategoryId to NVARCHAR(50)'
GO

-- Step 4: Convert existing data
PRINT 'Step 4: Converting existing data...'

UPDATE ProductCategories 
SET ParentCategoryId = CAST(ParentCategoryId AS NVARCHAR(50))
WHERE ParentCategoryId IS NOT NULL AND ISNUMERIC(ParentCategoryId) = 1
PRINT 'Converted existing ParentCategoryId values'

UPDATE Products 
SET CategoryId = CAST(CategoryId AS NVARCHAR(50))
WHERE CategoryId IS NOT NULL AND ISNUMERIC(CategoryId) = 1
PRINT 'Converted existing CategoryId values'
GO

-- Step 5: Recreate foreign key constraints
PRINT 'Step 5: Recreating foreign key constraints...'

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

-- Step 6: Recreate indexes
PRINT 'Step 6: Recreating indexes...'

CREATE INDEX IX_ProductCategories_ParentCategoryId ON ProductCategories(ParentCategoryId)
PRINT 'Recreated IX_ProductCategories_ParentCategoryId'

CREATE INDEX IX_Products_CategoryId ON Products(CategoryId)
PRINT 'Recreated IX_Products_CategoryId'

CREATE INDEX IX_Products_Name_IsActive ON Products(Name, IsActive)
PRINT 'Recreated IX_Products_Name_IsActive'
GO

-- Step 7: Verification
PRINT 'Step 7: Verification...'
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME IN ('ProductCategories', 'Products') 
    AND COLUMN_NAME IN ('ParentCategoryId', 'CategoryId')
ORDER BY TABLE_NAME, COLUMN_NAME
GO

PRINT 'Complete ProductCategory type fix finished successfully!'
