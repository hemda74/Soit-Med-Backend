-- Quick check to see what categories exist and why they might not be returned
-- Run this to diagnose the issue

SELECT 
    Id,
    Name,
    NameAr,
    IsActive,
    ParentCategoryId,
    DisplayOrder,
    (SELECT COUNT(*) FROM Products WHERE CategoryId = ProductCategories.Id) AS ProductCount
FROM ProductCategories
ORDER BY DisplayOrder, Name;

-- Check specifically for main categories (what the API should return)
SELECT 
    Id,
    Name,
    NameAr,
    IsActive,
    ParentCategoryId,
    DisplayOrder,
    (SELECT COUNT(*) FROM Products WHERE CategoryId = ProductCategories.Id) AS ProductCount
FROM ProductCategories
WHERE IsActive = 1 AND ParentCategoryId IS NULL
ORDER BY DisplayOrder, Name;

-- Count how many should be returned
SELECT COUNT(*) AS MainActiveCategoriesCount
FROM ProductCategories
WHERE IsActive = 1 AND ParentCategoryId IS NULL;

