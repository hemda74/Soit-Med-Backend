-- ============================================================================
-- VERIFY PRODUCT MAPPING - Check if products are mapped correctly
-- ============================================================================
-- Run this AFTER running SEED_CATEGORIES_AND_LINK_PRODUCTS.sql
-- ============================================================================

USE [ITIWebApi44];
GO

PRINT '=== VERIFYING PRODUCT MAPPING ===';
PRINT '';

-- ============================================================================
-- STEP 1: Check products by Category text vs CategoryId
-- ============================================================================
PRINT '=== STEP 1: Products by Category (text) vs CategoryId ===';
SELECT 
    p.Category AS CategoryText,
    p.CategoryId,
    pc.Name AS CategoryName,
    pc.ParentCategoryId,
    (SELECT Name FROM ProductCategories WHERE Id = pc.ParentCategoryId) AS ParentCategoryName,
    COUNT(*) AS ProductCount
FROM Products p
LEFT JOIN ProductCategories pc ON p.CategoryId = pc.Id
WHERE p.Category IS NOT NULL
GROUP BY p.Category, p.CategoryId, pc.Name, pc.ParentCategoryId
ORDER BY p.Category, pc.Name;
GO

-- ============================================================================
-- STEP 2: Check for mismatched mappings
-- ============================================================================
PRINT '';
PRINT '=== STEP 2: Potential Mismatches (Category text vs CategoryId) ===';
SELECT 
    p.Id,
    p.Name AS ProductName,
    p.Category AS CategoryText,
    p.CategoryId,
    pc.Name AS CategoryNameFromId,
    CASE 
        WHEN p.CategoryId IS NULL THEN 'NOT MAPPED'
        WHEN p.Category != pc.Name AND p.Category NOT LIKE '%' + pc.Name + '%' THEN 'POSSIBLE MISMATCH'
        ELSE 'OK'
    END AS Status
FROM Products p
LEFT JOIN ProductCategories pc ON p.CategoryId = pc.Id
WHERE p.Category IS NOT NULL
  AND (
    p.CategoryId IS NULL 
    OR (p.Category != pc.Name AND p.Category NOT LIKE '%' + pc.Name + '%')
  )
ORDER BY p.Category, p.Name;
GO

-- ============================================================================
-- STEP 3: Products without CategoryId
-- ============================================================================
PRINT '';
PRINT '=== STEP 3: Products WITHOUT CategoryId (need mapping) ===';
SELECT 
    Category,
    COUNT(*) AS ProductCount,
    STRING_AGG(CAST(Id AS VARCHAR), ', ') AS ProductIds
FROM Products
WHERE CategoryId IS NULL AND Category IS NOT NULL
GROUP BY Category
ORDER BY ProductCount DESC;
GO

-- ============================================================================
-- STEP 4: Category product counts
-- ============================================================================
PRINT '';
PRINT '=== STEP 4: Product Count per Category ===';
SELECT 
    pc.Id,
    pc.Name AS CategoryName,
    pc.ParentCategoryId,
    (SELECT Name FROM ProductCategories WHERE Id = pc.ParentCategoryId) AS ParentName,
    COUNT(p.Id) AS ProductCount,
    SUM(CASE WHEN p.IsActive = 1 THEN 1 ELSE 0 END) AS ActiveProductCount
FROM ProductCategories pc
LEFT JOIN Products p ON pc.Id = p.CategoryId
WHERE pc.IsActive = 1
GROUP BY pc.Id, pc.Name, pc.ParentCategoryId
ORDER BY pc.ParentCategoryId, pc.Name;
GO

-- ============================================================================
-- STEP 5: Summary
-- ============================================================================
PRINT '';
PRINT '=== STEP 5: Summary ===';

DECLARE @TotalProducts INT;
DECLARE @ProductsWithCategoryId INT;
DECLARE @ProductsWithoutCategoryId INT;
DECLARE @MismatchedProducts INT;

SELECT @TotalProducts = COUNT(*) FROM Products;
SELECT @ProductsWithCategoryId = COUNT(*) FROM Products WHERE CategoryId IS NOT NULL;
SELECT @ProductsWithoutCategoryId = COUNT(*) FROM Products WHERE CategoryId IS NULL AND Category IS NOT NULL;

SELECT @MismatchedProducts = COUNT(*)
FROM Products p
LEFT JOIN ProductCategories pc ON p.CategoryId = pc.Id
WHERE p.Category IS NOT NULL 
  AND p.CategoryId IS NOT NULL
  AND p.Category != pc.Name 
  AND p.Category NOT LIKE '%' + pc.Name + '%';

PRINT 'Total Products: ' + CAST(@TotalProducts AS VARCHAR);
PRINT 'Products with CategoryId: ' + CAST(@ProductsWithCategoryId AS VARCHAR);
PRINT 'Products without CategoryId: ' + CAST(@ProductsWithoutCategoryId AS VARCHAR);
PRINT 'Potentially mismatched products: ' + CAST(@MismatchedProducts AS VARCHAR);
PRINT '';

IF @ProductsWithoutCategoryId > 0
BEGIN
    PRINT 'WARNING: Some products are not mapped to categories!';
    PRINT 'Run SEED_CATEGORIES_AND_LINK_PRODUCTS.sql to fix this.';
END

IF @MismatchedProducts > 0
BEGIN
    PRINT 'WARNING: Some products may be mapped to wrong categories!';
    PRINT 'Review the mismatches above and update manually if needed.';
END

IF @ProductsWithoutCategoryId = 0 AND @MismatchedProducts = 0
BEGIN
    PRINT 'SUCCESS: All products are correctly mapped to categories!';
END

GO

