-- ============================================================================
-- CHECK CURRENT STATE - See if products are already linked
-- ============================================================================

USE [ITIWebApi44];
GO

PRINT '=== CHECKING CURRENT STATE ===';
PRINT '';

-- Check how many products have CategoryId
SELECT 
    'Products with CategoryId' AS Status,
    COUNT(*) AS Count
FROM Products
WHERE CategoryId IS NOT NULL;
GO

-- Check how many products DON'T have CategoryId
SELECT 
    'Products WITHOUT CategoryId' AS Status,
    COUNT(*) AS Count
FROM Products
WHERE CategoryId IS NULL;
GO

-- Show products by Category text (to see what needs mapping)
SELECT 
    Category AS CategoryText,
    COUNT(*) AS ProductCount,
    SUM(CASE WHEN CategoryId IS NULL THEN 1 ELSE 0 END) AS WithoutCategoryId,
    SUM(CASE WHEN CategoryId IS NOT NULL THEN 1 ELSE 0 END) AS WithCategoryId
FROM Products
WHERE Category IS NOT NULL
GROUP BY Category
ORDER BY Category;
GO

-- Show main categories
SELECT 
    'Main Categories' AS Type,
    Id,
    Name,
    IsActive,
    ParentCategoryId
FROM ProductCategories
WHERE IsActive = 1 AND ParentCategoryId IS NULL
ORDER BY DisplayOrder, Name;
GO

-- Show product counts per category
SELECT 
    pc.Id AS CategoryId,
    pc.Name AS CategoryName,
    COUNT(p.Id) AS ProductCount
FROM ProductCategories pc
LEFT JOIN Products p ON pc.Id = p.CategoryId
WHERE pc.IsActive = 1 AND pc.ParentCategoryId IS NULL
GROUP BY pc.Id, pc.Name
ORDER BY ProductCount DESC, pc.Name;
GO

PRINT '';
PRINT 'If products already have CategoryId, categories should be visible!';
PRINT 'Check the API: GET /api/ProductCategory/main';

