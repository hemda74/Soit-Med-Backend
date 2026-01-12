-- Fix identity columns by removing IDENTITY property first
-- This handles the fundamental issue with identity columns

USE [ITIWebApi44]
GO

PRINT 'Starting identity column fix...'

-- Check current state
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMNPROPERTY(OBJECT_ID(TABLE_NAME), COLUMN_NAME, 'IsIdentity') AS IsIdentity
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME IN ('ProductCategories', 'Products') 
    AND COLUMN_NAME IN ('Id', 'ParentCategoryId', 'CategoryId')
ORDER BY TABLE_NAME, COLUMN_NAME
GO

-- Step 1: Drop all foreign key constraints
PRINT 'Step 1: Dropping foreign key constraints...'

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

-- Step 2: Drop indexes
PRINT 'Step 2: Dropping indexes...'

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductCategories_ParentCategoryId' AND object_id = OBJECT_ID('ProductCategories'))
BEGIN
    DROP INDEX IX_ProductCategories_ParentCategoryId ON ProductCategories
    PRINT 'Dropped IX_ProductCategories_ParentCategoryId'
END
GO

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

-- Step 3: Drop primary key constraints
PRINT 'Step 3: Dropping primary key constraints...'

IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('ProductCategories') AND type = 'PK')
BEGIN
    DECLARE @PkName NVARCHAR(128)
    SELECT @PkName = name FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('ProductCategories') AND type = 'PK'
    EXEC ('ALTER TABLE ProductCategories DROP CONSTRAINT ' + @PkName)
    PRINT 'Dropped ProductCategories primary key: ' + @PkName
END
GO

IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('Products') AND type = 'PK')
BEGIN
    DECLARE @PkName2 NVARCHAR(128)
    SELECT @PkName2 = name FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('Products') AND type = 'PK'
    EXEC ('ALTER TABLE Products DROP CONSTRAINT ' + @PkName2)
    PRINT 'Dropped Products primary key: ' + @PkName2
END
GO

-- Step 4: Remove IDENTITY property and change column types
PRINT 'Step 4: Removing IDENTITY and changing column types...'

-- For ProductCategories
BEGIN TRY
    -- Create a temporary table to hold the data
    SELECT Id, Name, NameAr, Description, DescriptionAr, IconPath, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt
    INTO #TempProductCategories
    FROM ProductCategories
    
    -- Drop the original table
    DROP TABLE ProductCategories
    
    -- Recreate the table with correct column types
    CREATE TABLE ProductCategories (
        Id NVARCHAR(50) NOT NULL PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        NameAr NVARCHAR(100) NULL,
        Description NVARCHAR(500) NULL,
        DescriptionAr NVARCHAR(500) NULL,
        IconPath NVARCHAR(500) NULL,
        ParentCategoryId NVARCHAR(50) NULL,
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    )
    
    -- Copy data back
    INSERT INTO ProductCategories (Id, Name, NameAr, Description, DescriptionAr, IconPath, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    SELECT Id, Name, NameAr, Description, DescriptionAr, IconPath, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt
    FROM #TempProductCategories
    
    PRINT 'Successfully recreated ProductCategories table with NVARCHAR(50) Id'
END TRY
BEGIN CATCH
    PRINT 'Error recreating ProductCategories: ' + ERROR_MESSAGE()
END CATCH
GO

-- For Products
BEGIN TRY
    -- Create a temporary table to hold the data
    SELECT Id, Name, Model, Provider, ProviderImagePath, Country, Category, CategoryId, BasePrice, Description, ImagePath, DataSheetPath, CatalogPath, Year, InStock, InventoryQuantity, CreatedBy, CreatedAt, UpdatedAt
    INTO #TempProducts
    FROM Products
    
    -- Drop the original table
    DROP TABLE Products
    
    -- Recreate the table with correct column types
    CREATE TABLE Products (
        Id NVARCHAR(50) NOT NULL PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Model NVARCHAR(100) NULL,
        Provider NVARCHAR(100) NULL,
        ProviderImagePath NVARCHAR(500) NULL,
        Country NVARCHAR(100) NULL,
        Category NVARCHAR(100) NULL,
        CategoryId NVARCHAR(50) NULL,
        BasePrice DECIMAL(18,2) NOT NULL,
        Description NVARCHAR(2000) NULL,
        ImagePath NVARCHAR(500) NULL,
        DataSheetPath NVARCHAR(500) NULL,
        CatalogPath NVARCHAR(500) NULL,
        Year INT NULL,
        InStock BIT NOT NULL DEFAULT 1,
        InventoryQuantity INT NULL,
        CreatedBy NVARCHAR(450) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
    )
    
    -- Copy data back
    INSERT INTO Products (Id, Name, Model, Provider, ProviderImagePath, Country, Category, CategoryId, BasePrice, Description, ImagePath, DataSheetPath, CatalogPath, Year, InStock, InventoryQuantity, CreatedBy, CreatedAt, UpdatedAt)
    SELECT Id, Name, Model, Provider, ProviderImagePath, Country, Category, CategoryId, BasePrice, Description, ImagePath, DataSheetPath, CatalogPath, Year, InStock, InventoryQuantity, CreatedBy, CreatedAt, UpdatedAt
    FROM #TempProducts
    
    PRINT 'Successfully recreated Products table with NVARCHAR(50) Id'
END TRY
BEGIN CATCH
    PRINT 'Error recreating Products: ' + ERROR_MESSAGE()
END CATCH
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

-- Step 7: Clean up temp tables
PRINT 'Step 7: Cleaning up...'
DROP TABLE IF EXISTS #TempProductCategories
DROP TABLE IF EXISTS #TempProducts
GO

-- Step 8: Final verification
PRINT 'Step 8: Final verification...'
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMNPROPERTY(OBJECT_ID(TABLE_NAME), COLUMN_NAME, 'IsIdentity') AS IsIdentity
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME IN ('ProductCategories', 'Products') 
    AND COLUMN_NAME IN ('Id', 'ParentCategoryId', 'CategoryId')
ORDER BY TABLE_NAME, COLUMN_NAME
GO

PRINT 'Identity column fix completed successfully!'
