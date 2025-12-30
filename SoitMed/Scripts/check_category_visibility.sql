-- ============================================================================
-- CHECK CATEGORY VISIBILITY - What Columns to Inspect
-- ============================================================================
-- This script checks all columns that affect category visibility
-- ============================================================================

USE [ITIWebApi44];
GO

PRINT '=== CHECKING CATEGORY VISIBILITY ===';
PRINT '';
PRINT 'Columns to inspect:';
PRINT '  ProductCategories: IsActive, ParentCategoryId, Name, NameAr, DisplayOrder';
PRINT '  Products: CategoryId, IsActive, Category (legacy)';
PRINT '';

-- ============================================================================
-- STEP 1: Check ProductCategories - Main Categories (what API returns)
-- ============================================================================
PRINT '=== STEP 1: Main Categories (IsActive=1 AND ParentCategoryId IS NULL) ===';
SELECT 
    Id,
    Name,
    NameAr,
    IsActive,
    ParentCategoryId,
    DisplayOrder,
    CASE 
        WHEN IsActive = 0 THEN 'INACTIVE - Will not show'
        WHEN ParentCategoryId IS NOT NULL THEN 'SUB-CATEGORY - Will not show in main list'
        ELSE 'OK - Should show'
    END AS Status
FROM ProductCategories
WHERE IsActive = 1 AND ParentCategoryId IS NULL
ORDER BY DisplayOrder, Name;

PRINT '';
PRINT 'Count: ' + CAST((SELECT COUNT(*) FROM ProductCategories WHERE IsActive = 1 AND ParentCategoryId IS NULL) AS VARCHAR);
GO

-- ============================================================================
-- STEP 2: Check ALL ProductCategories (including inactive)
-- ============================================================================
PRINT '';
PRINT '=== STEP 2: ALL Categories (including inactive) ===';
SELECT 
    Id,
    Name,
    NameAr,
    IsActive,
    ParentCategoryId,
    DisplayOrder,
    CASE 
        WHEN IsActive = 0 THEN 'INACTIVE'
        WHEN ParentCategoryId IS NOT NULL THEN 'SUB-CATEGORY'
        ELSE 'MAIN CATEGORY'
    END AS CategoryType
FROM ProductCategories
ORDER BY IsActive DESC, ParentCategoryId, DisplayOrder, Name;
GO

-- ============================================================================
-- STEP 3: Check Products - CategoryId column (should NOT be NULL)
-- ============================================================================
PRINT '';
PRINT '=== STEP 3: Products - CategoryId Column (should NOT be NULL) ===';
SELECT 
    COUNT(*) AS TotalProducts,
    SUM(CASE WHEN CategoryId IS NULL THEN 1 ELSE 0 END) AS ProductsWithNullCategoryId,
    SUM(CASE WHEN CategoryId IS NOT NULL THEN 1 ELSE 0 END) AS ProductsWithCategoryId,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) AS ActiveProducts,
    SUM(CASE WHEN IsActive = 1 AND CategoryId IS NOT NULL THEN 1 ELSE 0 END) AS ActiveProductsWithCategoryId
FROM Products;
GO

PRINT '';
PRINT '=== Products with NULL CategoryId (PROBLEM - these need migration) ===';
SELECT TOP 20
    Id,
    Name,
    Category AS LegacyCategory,
    CategoryId,
    IsActive
FROM Products
WHERE CategoryId IS NULL
ORDER BY Id;
GO

-- ============================================================================
-- STEP 4: Check Products by CategoryId (grouped)
-- ============================================================================
PRINT '';
PRINT '=== STEP 4: Product Count per CategoryId ===';
SELECT 
    p.CategoryId,
    pc.Name AS CategoryName,
    COUNT(*) AS ProductCount,
    SUM(CASE WHEN p.IsActive = 1 THEN 1 ELSE 0 END) AS ActiveProductCount
FROM Products p
LEFT JOIN ProductCategories pc ON p.CategoryId = pc.Id
GROUP BY p.CategoryId, pc.Name
ORDER BY ProductCount DESC;
GO

-- ============================================================================
-- STEP 5: Check if categories have products (for product count display)
-- ============================================================================
PRINT '';
PRINT '=== STEP 5: Categories with Product Counts ===';
SELECT 
    pc.Id,
    pc.Name,
    pc.NameAr,
    pc.IsActive,
    pc.ParentCategoryId,
    COUNT(p.Id) AS ProductCount,
    SUM(CASE WHEN p.IsActive = 1 THEN 1 ELSE 0 END) AS ActiveProductCount
FROM ProductCategories pc
LEFT JOIN Products p ON pc.Id = p.CategoryId
WHERE pc.IsActive = 1 AND pc.ParentCategoryId IS NULL
GROUP BY pc.Id, pc.Name, pc.NameAr, pc.IsActive, pc.ParentCategoryId
ORDER BY pc.DisplayOrder, pc.Name;
GO

-- ============================================================================
-- STEP 6: Summary - What to Fix
-- ============================================================================
PRINT '';
PRINT '=== STEP 6: SUMMARY - What to Fix ===';

DECLARE @MainCategoriesCount INT;
DECLARE @ProductsWithNullCategoryId INT;
DECLARE @InactiveMainCategories INT;

SELECT @MainCategoriesCount = COUNT(*) 
FROM ProductCategories 
WHERE IsActive = 1 AND ParentCategoryId IS NULL;

SELECT @ProductsWithNullCategoryId = COUNT(*) 
FROM Products 
WHERE CategoryId IS NULL;

SELECT @InactiveMainCategories = COUNT(*) 
FROM ProductCategories 
WHERE IsActive = 0 AND ParentCategoryId IS NULL;

PRINT 'Main Categories (IsActive=1, ParentCategoryId IS NULL): ' + CAST(@MainCategoriesCount AS VARCHAR);
PRINT 'Products with NULL CategoryId: ' + CAST(@ProductsWithNullCategoryId AS VARCHAR);
PRINT 'Inactive Main Categories: ' + CAST(@InactiveMainCategories AS VARCHAR);
PRINT '';

IF @MainCategoriesCount = 0
BEGIN
    PRINT 'PROBLEM: No main categories found!';
    PRINT '  - Check if IsActive = 1 for main categories';
    PRINT '  - Check if ParentCategoryId IS NULL for main categories';
    PRINT '  - Run: URGENT_FIX_CATEGORIES.sql';
END

IF @ProductsWithNullCategoryId > 0
BEGIN
    PRINT 'PROBLEM: Products have NULL CategoryId!';
    PRINT '  - Run: migrate_products_to_categoryid_SSMS.sql';
END

IF @InactiveMainCategories > 0
BEGIN
    PRINT 'WARNING: Some main categories are inactive';
    PRINT '  - Run: UPDATE ProductCategories SET IsActive = 1 WHERE ParentCategoryId IS NULL';
END

IF @MainCategoriesCount > 0 AND @ProductsWithNullCategoryId = 0
BEGIN
    PRINT 'OK: Categories should be visible!';
    PRINT '  - If still not showing, check backend API logs';
    PRINT '  - Check frontend API calls';
END

GO

