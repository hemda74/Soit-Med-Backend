-- ============================================================================
-- ADD PERFORMANCE INDEXES
-- ============================================================================
-- This script adds indexes to improve query performance and prevent timeouts
-- ============================================================================

USE [ITIWebApi44];
GO

PRINT '=== ADDING PERFORMANCE INDEXES ===';
PRINT '';

BEGIN TRANSACTION;
GO

-- Index 1: ProductCategories - Main query optimization
-- Used by: GetMainCategoriesAsync (WHERE IsActive = 1 AND ParentCategoryId IS NULL)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductCategories_IsActive_ParentCategoryId' AND object_id = OBJECT_ID('ProductCategories'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProductCategories_IsActive_ParentCategoryId
    ON ProductCategories (IsActive, ParentCategoryId)
    INCLUDE (Name, NameAr, DisplayOrder, IconPath);
    PRINT '✓ Created index: IX_ProductCategories_IsActive_ParentCategoryId';
END
ELSE
BEGIN
    PRINT '⚠ Index already exists: IX_ProductCategories_IsActive_ParentCategoryId';
END
GO

-- Index 2: Products - CategoryId lookup (for product counts)
-- Used by: Product count queries (WHERE CategoryId = X)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_CategoryId' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_CategoryId
    ON Products (CategoryId)
    WHERE CategoryId IS NOT NULL;
    PRINT '✓ Created index: IX_Products_CategoryId';
END
ELSE
BEGIN
    PRINT '⚠ Index already exists: IX_Products_CategoryId';
END
GO

-- Index 3: Products - IsActive filter
-- Used by: GetAllActiveAsync (WHERE IsActive = 1)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_IsActive' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_IsActive
    ON Products (IsActive)
    INCLUDE (Name, CategoryId)
    WHERE IsActive = 1;
    PRINT '✓ Created index: IX_Products_IsActive';
END
ELSE
BEGIN
    PRINT '⚠ Index already exists: IX_Products_IsActive';
END
GO

-- Index 4: Products - Composite for category + active filter
-- Used by: GetByCategoryIdAsync (WHERE IsActive = 1 AND CategoryId = X)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_IsActive_CategoryId' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_IsActive_CategoryId
    ON Products (IsActive, CategoryId)
    INCLUDE (Name, Model, BasePrice, ImagePath)
    WHERE IsActive = 1;
    PRINT '✓ Created index: IX_Products_IsActive_CategoryId';
END
ELSE
BEGIN
    PRINT '⚠ Index already exists: IX_Products_IsActive_CategoryId';
END
GO

-- Index 5: ProductCategories - DisplayOrder for sorting
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ProductCategories_DisplayOrder' AND object_id = OBJECT_ID('ProductCategories'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ProductCategories_DisplayOrder
    ON ProductCategories (DisplayOrder, Name)
    WHERE IsActive = 1;
    PRINT '✓ Created index: IX_ProductCategories_DisplayOrder';
END
ELSE
BEGIN
    PRINT '⚠ Index already exists: IX_ProductCategories_DisplayOrder';
END
GO

PRINT '';
PRINT '=== INDEX CREATION COMPLETE ===';
PRINT '';
PRINT 'Review the results above. If all indexes were created successfully, commit the transaction.';
PRINT '';
PRINT 'To commit: COMMIT TRANSACTION;';
PRINT 'To rollback: ROLLBACK TRANSACTION;';
PRINT '';

-- Uncomment to commit:
-- COMMIT TRANSACTION;

-- Uncomment to rollback:
-- ROLLBACK TRANSACTION;

