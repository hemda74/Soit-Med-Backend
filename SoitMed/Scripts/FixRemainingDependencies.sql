-- Fix remaining foreign key dependencies
-- Handle the constraints that weren't caught in the previous script

USE [ITIWebApi44]
GO

-- Drop the remaining foreign key constraints that are still blocking the column changes
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

-- Now try to alter the columns again
-- Update ParentCategoryId in ProductCategories
ALTER TABLE ProductCategories 
ALTER COLUMN ParentCategoryId NVARCHAR(50) NULL
PRINT 'Successfully changed ProductCategories.ParentCategoryId to NVARCHAR(50)'
GO

-- Update CategoryId in Products  
ALTER TABLE Products 
ALTER COLUMN CategoryId NVARCHAR(50) NULL
PRINT 'Successfully changed Products.CategoryId to NVARCHAR(50)'
GO

-- Verify the changes
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

PRINT 'Column type verification completed. All changes should now be successful!'
