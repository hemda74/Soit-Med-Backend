-- Final ProductCategory type mismatch fix
-- Simple, direct approach that will work

USE [ITIWebApi44]
GO

PRINT 'Starting final ProductCategory type fix...'

-- Step 1: Manually drop all known constraints
PRINT 'Step 1: Dropping constraints...'

-- Drop foreign keys (try each one individually)
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Products_ProductCategories_CategoryId')
BEGIN
    ALTER TABLE Products DROP CONSTRAINT FK_Products_ProductCategories_CategoryId
    PRINT 'Dropped FK_Products_ProductCategories_CategoryId'
END
GO

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ProductCategories_ProductCategories_ParentCategoryId')
BEGIN
    ALTER TABLE ProductCategories DROP CONSTRAINT FK_ProductCategories_ProductCategories_ParentCategoryId
    PRINT 'Dropped FK_ProductCategories_ProductCategories_ParentCategoryId'
END
GO

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ProductCategories_ParentCategory')
BEGIN
    ALTER TABLE ProductCategories DROP CONSTRAINT FK_ProductCategories_ParentCategory
    PRINT 'Dropped FK_ProductCategories_ParentCategory'
END
GO

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Products_ProductCategories')
BEGIN
    ALTER TABLE Products DROP CONSTRAINT FK_Products_ProductCategories
    PRINT 'Dropped FK_Products_ProductCategories'
END
GO

-- Step 2: Manually drop all known indexes
PRINT 'Step 2: Dropping indexes...'

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_CategoryId' AND object_id = OBJECT_ID('Products'))
BEGIN
    DROP INDEX IX_Products_CategoryId ON Products
    PRINT 'Dropped IX_Products_CategoryId'
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Name_IsActive' AND object_id = OBJECT_ID('Products'))
BEGIN
    DROP INDEX IX_Products_Name_IsActive ON Products
    PRINT 'Dropped IX_Products_Name_IsActive'
END
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductCategories_ParentCategoryId' AND object_id = OBJECT_ID('ProductCategories'))
BEGIN
    DROP INDEX IX_ProductCategories_ParentCategoryId ON ProductCategories
    PRINT 'Dropped IX_ProductCategories_ParentCategoryId'
END
GO

-- Step 3: Now alter the columns
PRINT 'Step 3: Altering column types...'

ALTER TABLE ProductCategories 
ALTER COLUMN ParentCategoryId NVARCHAR(50) NULL
PRINT 'Successfully changed ProductCategories.ParentCategoryId to NVARCHAR(50)'
GO

ALTER TABLE Products 
ALTER COLUMN CategoryId NVARCHAR(50) NULL
PRINT 'Successfully changed Products.CategoryId to NVARCHAR(50)'
GO

-- Step 4: Recreate constraints
PRINT 'Step 4: Recreating constraints...'

ALTER TABLE ProductCategories 
ADD CONSTRAINT FK_ProductCategories_ProductCategories_ParentCategoryId 
FOREIGN KEY (ParentCategoryId) REFERENCES ProductCategories(Id)
ON DELETE NO ACTION
PRINT 'Recreated FK_ProductCategories_ProductCategories_ParentCategoryId'
GO

ALTER TABLE Products 
ADD CONSTRAINT FK_Products_ProductCategories_CategoryId 
FOREIGN KEY (CategoryId) REFERENCES ProductCategories(Id)
ON DELETE NO ACTION
PRINT 'Recreated FK_Products_ProductCategories_CategoryId'
GO

-- Step 5: Recreate indexes
PRINT 'Step 5: Recreating indexes...'

CREATE INDEX IX_ProductCategories_ParentCategoryId ON ProductCategories(ParentCategoryId)
PRINT 'Recreated IX_ProductCategories_ParentCategoryId'
GO

CREATE INDEX IX_Products_CategoryId ON Products(CategoryId)
PRINT 'Recreated IX_Products_CategoryId'
GO

CREATE INDEX IX_Products_Name_IsActive ON Products(Name, IsActive)
PRINT 'Recreated IX_Products_Name_IsActive'
GO

-- Step 6: Final verification
PRINT 'Step 6: Final verification...'
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME IN ('ProductCategories', 'Products') 
    AND COLUMN_NAME IN ('ParentCategoryId', 'CategoryId')
ORDER BY TABLE_NAME, COLUMN_NAME
GO

PRINT 'Final ProductCategory type fix completed successfully!'
