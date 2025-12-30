-- ============================================================================
-- DIAGNOSTIC: Check ProductCategories Table State
-- ============================================================================
-- Run this in SSMS to see why categories are not being returned
-- ============================================================================

USE [ITIWebApi44];
GO

PRINT '=== PRODUCT CATEGORIES DIAGNOSTIC ===';
PRINT '';

-- 1. Total count of all categories
SELECT 
    'Total Categories' AS Status,
    COUNT(*) AS Count
FROM ProductCategories;
GO

-- 2. Count by IsActive status
SELECT 
    IsActive,
    COUNT(*) AS Count
FROM ProductCategories
GROUP BY IsActive;
GO

-- 3. Count by ParentCategoryId status
SELECT 
    CASE 
        WHEN ParentCategoryId IS NULL THEN 'Main Categories (no parent)'
        ELSE 'Sub Categories (has parent)'
    END AS CategoryType,
    COUNT(*) AS Count
FROM ProductCategories
GROUP BY CASE 
    WHEN ParentCategoryId IS NULL THEN 'Main Categories (no parent)'
    ELSE 'Sub Categories (has parent)'
END;
GO

-- 4. Main active categories (what the API is looking for)
SELECT 
    'Main Active Categories (IsActive=1 AND ParentCategoryId IS NULL)' AS Status,
    COUNT(*) AS Count
FROM ProductCategories
WHERE IsActive = 1 AND ParentCategoryId IS NULL;
GO

-- 5. Show ALL categories with their status
PRINT '';
PRINT '=== ALL CATEGORIES DETAIL ===';
SELECT 
    Id,
    Name,
    NameAr,
    IsActive,
    ParentCategoryId,
    DisplayOrder,
    CASE 
        WHEN IsActive = 1 AND ParentCategoryId IS NULL THEN '✅ Main Active'
        WHEN IsActive = 1 AND ParentCategoryId IS NOT NULL THEN '✅ Sub Active'
        WHEN IsActive = 0 AND ParentCategoryId IS NULL THEN '❌ Main Inactive'
        ELSE '❌ Sub Inactive'
    END AS Status
FROM ProductCategories
ORDER BY IsActive DESC, ParentCategoryId, DisplayOrder, Name;
GO

-- 6. Check if there are any categories that should be main but aren't
PRINT '';
PRINT '=== CATEGORIES THAT SHOULD BE MAIN (but might have wrong settings) ===';
SELECT 
    Id,
    Name,
    IsActive,
    ParentCategoryId,
    'Should be Main Category' AS Recommendation
FROM ProductCategories
WHERE Name IN (
    'Radiology Line',
    'Physiotherapy Line',
    'Dermatology Line',
    'LAP Line',
    'Laboratory Line',
    'Dental Line'
)
ORDER BY Name;
GO

PRINT '';
PRINT '=== DIAGNOSTIC COMPLETE ===';
PRINT '';
PRINT 'If "Main Active Categories" count is 0, check:';
PRINT '  1. Are categories marked as IsActive = 1?';
PRINT '  2. Do main categories have ParentCategoryId = NULL?';
PRINT '  3. Run the update_product_categories.sql script if needed.';
GO

