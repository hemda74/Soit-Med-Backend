-- ============================================================================
-- FIX SLOW QUERIES
-- ============================================================================
-- This script fixes common causes of slow queries
-- Run diagnose_slow_queries.sql first to identify issues
-- ============================================================================

USE [ITIWebApi44];
GO

PRINT '=== FIXING SLOW QUERIES ===';
PRINT '';

BEGIN TRANSACTION;
GO

-- ============================================================================
-- STEP 1: Update Statistics (Helps query optimizer)
-- ============================================================================
PRINT '=== STEP 1: Updating Statistics ===';
UPDATE STATISTICS Products WITH FULLSCAN;
PRINT '✓ Updated statistics for Products table';

UPDATE STATISTICS ProductCategories WITH FULLSCAN;
PRINT '✓ Updated statistics for ProductCategories table';
GO

-- ============================================================================
-- STEP 2: Rebuild Fragmented Indexes
-- ============================================================================
PRINT '';
PRINT '=== STEP 2: Rebuilding Fragmented Indexes ===';

-- Rebuild all indexes on Products table
DECLARE @sql NVARCHAR(MAX) = '';
SELECT @sql = @sql + 
    'ALTER INDEX [' + i.name + '] ON [Products] REBUILD WITH (ONLINE = OFF);' + CHAR(13)
FROM 
    sys.indexes i
INNER JOIN 
    sys.dm_db_index_physical_stats(DB_ID(), OBJECT_ID('Products'), NULL, NULL, 'DETAILED') ips
    ON i.object_id = ips.object_id AND i.index_id = ips.index_id
WHERE 
    i.object_id = OBJECT_ID('Products')
    AND i.type > 0  -- Exclude heap
    AND ips.avg_fragmentation_in_percent > 10
    AND ips.page_count > 100;

IF @sql <> ''
BEGIN
    EXEC sp_executesql @sql;
    PRINT '✓ Rebuilt fragmented indexes on Products table';
END
ELSE
BEGIN
    PRINT '⚠ No fragmented indexes found on Products table';
END
GO

-- Rebuild all indexes on ProductCategories table
DECLARE @sql2 NVARCHAR(MAX) = '';
SELECT @sql2 = @sql2 + 
    'ALTER INDEX [' + i.name + '] ON [ProductCategories] REBUILD WITH (ONLINE = OFF);' + CHAR(13)
FROM 
    sys.indexes i
INNER JOIN 
    sys.dm_db_index_physical_stats(DB_ID(), OBJECT_ID('ProductCategories'), NULL, NULL, 'DETAILED') ips
    ON i.object_id = ips.object_id AND i.index_id = ips.index_id
WHERE 
    i.object_id = OBJECT_ID('ProductCategories')
    AND i.type > 0  -- Exclude heap
    AND ips.avg_fragmentation_in_percent > 10
    AND ips.page_count > 100;

IF @sql2 <> ''
BEGIN
    EXEC sp_executesql @sql2;
    PRINT '✓ Rebuilt fragmented indexes on ProductCategories table';
END
ELSE
BEGIN
    PRINT '⚠ No fragmented indexes found on ProductCategories table';
END
GO

-- ============================================================================
-- STEP 3: Add Missing Indexes (if not already added)
-- ============================================================================
PRINT '';
PRINT '=== STEP 3: Adding Performance Indexes ===';

-- Index 1: ProductCategories - Main query optimization
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

-- Index 2: Products - CategoryId lookup
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

-- ============================================================================
-- STEP 4: Clear Query Plan Cache (Force recompilation)
-- ============================================================================
PRINT '';
PRINT '=== STEP 4: Clearing Query Plan Cache ===';
DBCC FREEPROCCACHE;
PRINT '✓ Cleared query plan cache';
GO

PRINT '';
PRINT '=== FIX COMPLETE ===';
PRINT '';
PRINT 'Review the results above. If all steps completed successfully, commit the transaction.';
PRINT '';
PRINT 'To commit: COMMIT TRANSACTION;';
PRINT 'To rollback: ROLLBACK TRANSACTION;';
PRINT '';
PRINT 'After committing, test your queries again - they should be much faster!';
PRINT '';

-- Uncomment to commit:
-- COMMIT TRANSACTION;

-- Uncomment to rollback:
-- ROLLBACK TRANSACTION;

