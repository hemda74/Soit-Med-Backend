-- Complete fix including primary key constraints
-- This handles PK constraints that prevent column type changes

USE [ITIWebApi44]
GO

PRINT 'Starting complete primary key and column type fix...'

-- Check current state
SELECT 
    'ProductCategories' AS TableName,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ProductCategories' AND COLUMN_NAME IN ('Id', 'ParentCategoryId')
UNION ALL
SELECT 
    'Products' AS TableName,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Products' AND COLUMN_NAME IN ('Id', 'CategoryId')
GO

-- Step 1: Drop all foreign key constraints
PRINT 'Step 1: Dropping all foreign key constraints...'

-- Drop FK from Products to ProductCategories
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Products_ProductCategories_CategoryId')
BEGIN
    ALTER TABLE Products DROP CONSTRAINT FK_Products_ProductCategories_CategoryId
    PRINT 'Dropped FK_Products_ProductCategories_CategoryId'
END
GO

-- Drop FK from ProductCategories self-reference
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ProductCategories_ProductCategories_ParentCategoryId')
BEGIN
    ALTER TABLE ProductCategories DROP CONSTRAINT FK_ProductCategories_ProductCategories_ParentCategoryId
    PRINT 'Dropped FK_ProductCategories_ProductCategories_ParentCategoryId'
END
GO

-- Step 2: Drop all indexes (including PK)
PRINT 'Step 2: Dropping all indexes...'

-- Drop ProductCategories indexes
DECLARE @ProductCategoriesIndexes CURSOR
DECLARE @IndexName NVARCHAR(128), @TableName NVARCHAR(128)
SET @TableName = 'ProductCategories'

DECLARE @ProductCategoriesIndexList CURSOR FOR
SELECT name FROM sys.indexes WHERE object_id = OBJECT_ID(@TableName)

OPEN @ProductCategoriesIndexList
FETCH NEXT FROM @ProductCategoriesIndexList INTO @IndexName
BEGIN
    EXEC ('DROP INDEX ' + @IndexName + ' ON ' + @TableName)
    PRINT 'Dropped index: ' + @IndexName + ' on ' + @TableName
END
CLOSE @ProductCategoriesIndexList
DEALLOCATE @ProductCategoriesIndexList
GO

-- Drop Products indexes
DECLARE @ProductsIndexes CURSOR
DECLARE @ProductsTableName NVARCHAR(128)
SET @ProductsTableName = 'Products'

DECLARE @ProductsIndexList CURSOR FOR
SELECT name FROM sys.indexes WHERE object_id = OBJECT_ID(@ProductsTableName)

OPEN @ProductsIndexList
FETCH NEXT FROM @ProductsIndexList INTO @IndexName
BEGIN
    EXEC ('DROP INDEX ' + @IndexName + ' ON ' + @ProductsTableName)
    PRINT 'Dropped index: ' + @IndexName + ' on ' + @ProductsTableName
END
CLOSE @ProductsIndexList
DEALLOCATE @ProductsIndexList
GO

-- Step 3: Drop primary key constraints
PRINT 'Step 3: Dropping primary key constraints...'

-- Drop ProductCategories PK
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('ProductCategories') AND type = 'PK')
BEGIN
    DECLARE @PkName NVARCHAR(128)
    SELECT @PkName = name FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('ProductCategories') AND type = 'PK'
    EXEC ('ALTER TABLE ProductCategories DROP CONSTRAINT ' + @PkName)
    PRINT 'Dropped ProductCategories primary key: ' + @PkName
END
GO

-- Drop Products PK
IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('Products') AND type = 'PK')
BEGIN
    DECLARE @PkName2 NVARCHAR(128)
    SELECT @PkName2 = name FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('Products') AND type = 'PK'
    EXEC ('ALTER TABLE Products DROP CONSTRAINT ' + @PkName2)
    PRINT 'Dropped Products primary key: ' + @PkName2
END
GO

-- Step 4: Change all column types to NVARCHAR(50)
PRINT 'Step 4: Changing column types to NVARCHAR(50)...'

ALTER TABLE ProductCategories
ALTER COLUMN Id NVARCHAR(50) NOT NULL
PRINT 'Changed ProductCategories.Id to NVARCHAR(50) NOT NULL'

ALTER TABLE ProductCategories
ALTER COLUMN ParentCategoryId NVARCHAR(50) NULL
PRINT 'Changed ProductCategories.ParentCategoryId to NVARCHAR(50) NULL'

ALTER TABLE Products
ALTER COLUMN Id NVARCHAR(50) NOT NULL
PRINT 'Changed Products.Id to NVARCHAR(50) NOT NULL'

ALTER TABLE Products
ALTER COLUMN CategoryId NVARCHAR(50) NULL
PRINT 'Changed Products.CategoryId to NVARCHAR(50) NULL'
GO

-- Step 5: Recreate primary key constraints
PRINT 'Step 5: Recreating primary key constraints...'

ALTER TABLE ProductCategories
ADD CONSTRAINT PK_ProductCategories PRIMARY KEY (Id)
PRINT 'Recreated ProductCategories primary key'

ALTER TABLE Products
ADD CONSTRAINT PK_Products PRIMARY KEY (Id)
PRINT 'Recreated Products primary key'
GO

-- Step 6: Recreate foreign key constraints
PRINT 'Step 6: Recreating foreign key constraints...'

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

-- Step 7: Recreate essential indexes
PRINT 'Step 7: Recreating essential indexes...'

CREATE INDEX IX_ProductCategories_ParentCategoryId ON ProductCategories(ParentCategoryId)
PRINT 'Recreated IX_ProductCategories_ParentCategoryId'

CREATE INDEX IX_Products_CategoryId ON Products(CategoryId)
PRINT 'Recreated IX_Products_CategoryId'

CREATE INDEX IX_Products_Name_IsActive ON Products(Name, IsActive)
PRINT 'Recreated IX_Products_Name_IsActive'
GO

-- Step 8: Final verification
PRINT 'Step 8: Final verification...'
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

PRINT 'Complete primary key and column type fix completed successfully!'
