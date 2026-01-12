-- Fix ProductCategory ParentCategoryId type mismatch
-- Change from bigint to nvarchar(50) to match BaseEntity.Id type

USE [ITIWebApi44]
GO

-- Drop foreign key constraints first (they depend on the columns)
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

-- Drop indexes that depend on the columns
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

-- Now alter the column types
-- Update ParentCategoryId in ProductCategories
ALTER TABLE ProductCategories 
ALTER COLUMN ParentCategoryId NVARCHAR(50) NULL
PRINT 'Changed ProductCategories.ParentCategoryId to NVARCHAR(50)'
GO

-- Update CategoryId in Products
ALTER TABLE Products 
ALTER COLUMN CategoryId NVARCHAR(50) NULL
PRINT 'Changed Products.CategoryId to NVARCHAR(50)'
GO

-- Convert existing bigint values to strings (only if they exist)
UPDATE ProductCategories 
SET ParentCategoryId = CAST(ParentCategoryId AS NVARCHAR(50))
WHERE ParentCategoryId IS NOT NULL AND ISNUMERIC(ParentCategoryId) = 1
PRINT 'Converted existing ParentCategoryId values to strings'
GO

UPDATE Products 
SET CategoryId = CAST(CategoryId AS NVARCHAR(50))
WHERE CategoryId IS NOT NULL AND ISNUMERIC(CategoryId) = 1
PRINT 'Converted existing CategoryId values to strings'
GO

-- Recreate foreign key constraints
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

-- Recreate indexes
CREATE INDEX IX_ProductCategories_ParentCategoryId ON ProductCategories(ParentCategoryId)
PRINT 'Recreated IX_ProductCategories_ParentCategoryId'
GO

CREATE INDEX IX_Products_CategoryId ON Products(CategoryId)
PRINT 'Recreated IX_Products_CategoryId'
GO

CREATE INDEX IX_Products_Name_IsActive ON Products(Name, IsActive)
PRINT 'Recreated IX_Products_Name_IsActive'
GO

PRINT 'ProductCategory type mismatch fix completed successfully!'
