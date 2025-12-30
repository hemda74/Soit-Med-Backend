-- ============================================================================
-- Diagnose why GetMainCategories returns 0 results
-- ============================================================================

-- 1. Check all categories
SELECT 
    'All Categories' AS CheckType,
    COUNT(*) AS Count
FROM ProductCategories;

-- 2. Check active categories
SELECT 
    'Active Categories' AS CheckType,
    COUNT(*) AS Count
FROM ProductCategories
WHERE IsActive = 1;

-- 3. Check main categories (no parent)
SELECT 
    'Main Categories (ParentId IS NULL)' AS CheckType,
    COUNT(*) AS Count
FROM ProductCategories
WHERE ParentCategoryId IS NULL;

-- 4. Check main active categories (what the API should return)
SELECT 
    'Main Active Categories (IsActive=1 AND ParentId IS NULL)' AS CheckType,
    COUNT(*) AS Count
FROM ProductCategories
WHERE IsActive = 1 AND ParentCategoryId IS NULL;

-- 5. Show detailed info for main active categories
SELECT 
    Id,
    Name,
    NameAr,
    IsActive,
    ParentCategoryId,
    DisplayOrder,
    CASE 
        WHEN IsActive = 1 THEN 'TRUE'
        WHEN IsActive = 0 THEN 'FALSE'
        ELSE 'NULL'
    END AS IsActiveStatus,
    CASE 
        WHEN ParentCategoryId IS NULL THEN 'NULL'
        ELSE CAST(ParentCategoryId AS VARCHAR)
    END AS ParentIdStatus
FROM ProductCategories
WHERE IsActive = 1 AND ParentCategoryId IS NULL
ORDER BY DisplayOrder, Name;

-- 6. Check if IsActive is stored as BIT (0/1) or something else
SELECT 
    'IsActive Data Type Check' AS CheckType,
    MIN(CAST(IsActive AS INT)) AS MinValue,
    MAX(CAST(IsActive AS INT)) AS MaxValue,
    COUNT(DISTINCT IsActive) AS DistinctValues
FROM ProductCategories;

-- 7. Check ParentCategoryId values
SELECT 
    'ParentCategoryId Check' AS CheckType,
    COUNT(*) AS TotalCategories,
    SUM(CASE WHEN ParentCategoryId IS NULL THEN 1 ELSE 0 END) AS NullParentCount,
    SUM(CASE WHEN ParentCategoryId IS NOT NULL THEN 1 ELSE 0 END) AS HasParentCount
FROM ProductCategories;

