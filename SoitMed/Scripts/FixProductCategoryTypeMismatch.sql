-- Fix ProductCategory ParentCategoryId type mismatch
-- Change from bigint to nvarchar(50) to match BaseEntity.Id type

USE [YourDatabaseName] -- Replace with actual database name
GO

-- Drop existing index if it exists
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductCategories_ParentCategoryId' AND object_id = OBJECT_ID('ProductCategories'))
BEGIN
    DROP INDEX IX_ProductCategories_ParentCategoryId ON ProductCategories
END
GO

-- Drop foreign key constraint if it exists
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ProductCategories_ProductCategories_ParentCategoryId')
BEGIN
    ALTER TABLE ProductCategories DROP CONSTRAINT FK_ProductCategories_ProductCategories_ParentCategoryId
END
GO

-- Drop foreign key constraint for Products if it exists
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Products_ProductCategories_CategoryId')
BEGIN
    ALTER TABLE Products DROP CONSTRAINT FK_Products_ProductCategories_CategoryId
END
GO

-- Update ParentCategoryId to string type (convert existing bigint values to string)
ALTER TABLE ProductCategories 
ALTER COLUMN ParentCategoryId NVARCHAR(50) NULL
GO

-- Update CategoryId in Products to string type
ALTER TABLE Products 
ALTER COLUMN CategoryId NVARCHAR(50) NULL
GO

-- Convert existing bigint values to strings
UPDATE ProductCategories 
SET ParentCategoryId = CAST(ParentCategoryId AS NVARCHAR(50))
WHERE ParentCategoryId IS NOT NULL
GO

UPDATE Products 
SET CategoryId = CAST(CategoryId AS NVARCHAR(50))
WHERE CategoryId IS NOT NULL
GO

-- Recreate foreign key constraint for ProductCategories self-reference
ALTER TABLE ProductCategories 
ADD CONSTRAINT FK_ProductCategories_ProductCategories_ParentCategoryId 
FOREIGN KEY (ParentCategoryId) REFERENCES ProductCategories(Id)
ON DELETE NO ACTION
GO

-- Recreate foreign key constraint for Products
ALTER TABLE Products 
ADD CONSTRAINT FK_Products_ProductCategories_CategoryId 
FOREIGN KEY (CategoryId) REFERENCES ProductCategories(Id)
ON DELETE NO ACTION
GO

-- Recreate index
CREATE INDEX IX_ProductCategories_ParentCategoryId ON ProductCategories(ParentCategoryId)
GO

PRINT 'ProductCategory type mismatch fix completed successfully'
