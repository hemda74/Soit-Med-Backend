-- Check and fix Id column types in ProductCategories and Products
-- The error shows Id columns are still not NVARCHAR(50)

USE [ITIWebApi44]
GO

PRINT 'Checking current Id column types...'

-- Check ProductCategories.Id type
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ProductCategories' 
    AND COLUMN_NAME = 'Id'
GO

-- Check Products.Id type  
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Products' 
    AND COLUMN_NAME = 'Id'
GO

PRINT 'If Id columns are not NVARCHAR(50), we need to fix them...'

-- Fix ProductCategories.Id if needed
DECLARE @ProductCategoriesIdType NVARCHAR(50)
SELECT @ProductCategoriesIdType = DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ProductCategories' AND COLUMN_NAME = 'Id'

IF @ProductCategoriesIdType <> 'nvarchar'
BEGIN
    PRINT 'Fixing ProductCategories.Id column type...'
    
    -- Drop any constraints on ProductCategories.Id
    DECLARE @sql NVARCHAR(MAX) = ''
    SELECT @sql = @sql + 'ALTER TABLE ' + TABLE_SCHEMA_NAME(parent_object_id) + '.' + OBJECT_NAME(parent_object_id) + ' DROP CONSTRAINT ' + name + CHAR(13) + CHAR(10)
    FROM sys.foreign_keys
    WHERE referenced_object_id = OBJECT_ID('ProductCategories') 
    AND COL_NAME(referenced_object_id, referenced_column_id) = 'Id'
    
    IF LEN(@sql) > 0
    BEGIN
        EXEC sp_executesql @sql
        PRINT 'Dropped constraints on ProductCategories.Id'
    END
    
    -- Drop indexes on ProductCategories.Id
    DECLARE @index_sql NVARCHAR(MAX) = ''
    SELECT @index_sql = @index_sql + 'DROP INDEX ' + name + ' ON ' + OBJECT_SCHEMA_NAME(object_id) + '.' + OBJECT_NAME(object_id) + CHAR(13) + CHAR(10)
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('ProductCategories') 
    AND COL_NAME(object_id, index_column_id) = 'Id'
    
    IF LEN(@index_sql) > 0
    BEGIN
        EXEC sp_executesql @index_sql
        PRINT 'Dropped indexes on ProductCategories.Id'
    END
    
    -- Change the column type
    ALTER TABLE ProductCategories ALTER COLUMN Id NVARCHAR(50) NOT NULL
    PRINT 'Changed ProductCategories.Id to NVARCHAR(50)'
END
GO

-- Fix Products.Id if needed
DECLARE @ProductsIdType NVARCHAR(50)
SELECT @ProductsIdType = DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'Id'

IF @ProductsIdType <> 'nvarchar'
BEGIN
    PRINT 'Fixing Products.Id column type...'
    
    -- Drop any constraints on Products.Id
    DECLARE @sql2 NVARCHAR(MAX) = ''
    SELECT @sql2 = @sql2 + 'ALTER TABLE ' + TABLE_SCHEMA_NAME(parent_object_id) + '.' + OBJECT_NAME(parent_object_id) + ' DROP CONSTRAINT ' + name + CHAR(13) + CHAR(10)
    FROM sys.foreign_keys
    WHERE referenced_object_id = OBJECT_ID('Products') 
    AND COL_NAME(referenced_object_id, referenced_column_id) = 'Id'
    
    IF LEN(@sql2) > 0
    BEGIN
        EXEC sp_executesql @sql2
        PRINT 'Dropped constraints on Products.Id'
    END
    
    -- Drop indexes on Products.Id
    DECLARE @index_sql2 NVARCHAR(MAX) = ''
    SELECT @index_sql2 = @index_sql2 + 'DROP INDEX ' + name + ' ON ' + OBJECT_SCHEMA_NAME(object_id) + '.' + OBJECT_NAME(object_id) + CHAR(13) + CHAR(10)
    FROM sys.indexes
    WHERE object_id = OBJECT_ID('Products') 
    AND COL_NAME(object_id, index_column_id) = 'Id'
    
    IF LEN(@index_sql2) > 0
    BEGIN
        EXEC sp_executesql @index_sql2
        PRINT 'Dropped indexes on Products.Id'
    END
    
    -- Change the column type
    ALTER TABLE Products ALTER COLUMN Id NVARCHAR(50) NOT NULL
    PRINT 'Changed Products.Id to NVARCHAR(50)'
END
GO

-- Now recreate the foreign key constraints
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

PRINT 'Id column fix completed successfully!'
