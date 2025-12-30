-- ============================================================================
-- TEST: Simulate the Exact Query Used by the API
-- ============================================================================
-- This script runs the EXACT same query that the backend API uses
-- ============================================================================

USE [ITIWebApi44];
GO

PRINT '=== TESTING: Exact API Query ===';
PRINT '';
PRINT 'This is the EXACT query used by GetMainCategoriesAsync:';
PRINT '  WHERE IsActive = true AND ParentCategoryId IS NULL';
PRINT '';

-- This is the EXACT query from ProductCategoryRepository.GetMainCategoriesAsync
SELECT 
    [pc].[Id], 
    [pc].[Name], 
    [pc].[NameAr], 
    [pc].[Description], 
    [pc].[DescriptionAr], 
    [pc].[IconPath], 
    [pc].[ParentCategoryId], 
    [pc].[DisplayOrder], 
    [pc].[IsActive], 
    [pc].[CreatedAt], 
    [pc].[UpdatedAt]
FROM [ProductCategories] AS [pc]
WHERE [pc].[IsActive] = 1 AND [pc].[ParentCategoryId] IS NULL
ORDER BY [pc].[DisplayOrder], [pc].[Name];
GO

PRINT '';
PRINT '=== RESULT COUNT ===';
SELECT COUNT(*) AS MainCategoriesCount
FROM [ProductCategories]
WHERE [IsActive] = 1 AND [ParentCategoryId] IS NULL;
GO

PRINT '';
PRINT '=== DIAGNOSTIC: Why Query Returns 0 ===';
PRINT '';

-- Check 1: Are there ANY categories?
DECLARE @TotalCategories INT;
SELECT @TotalCategories = COUNT(*) FROM ProductCategories;
PRINT 'Total categories in table: ' + CAST(@TotalCategories AS VARCHAR);

IF @TotalCategories = 0
BEGIN
    PRINT 'PROBLEM: ProductCategories table is EMPTY!';
    PRINT 'SOLUTION: Run URGENT_FIX_CATEGORIES.sql to create categories';
END
ELSE
BEGIN
    -- Check 2: How many are active?
    DECLARE @ActiveCategories INT;
    SELECT @ActiveCategories = COUNT(*) FROM ProductCategories WHERE IsActive = 1;
    PRINT 'Active categories: ' + CAST(@ActiveCategories AS VARCHAR);
    
    IF @ActiveCategories = 0
    BEGIN
        PRINT 'PROBLEM: All categories are INACTIVE (IsActive = 0)';
        PRINT 'SOLUTION: Run UPDATE ProductCategories SET IsActive = 1 WHERE ParentCategoryId IS NULL';
    END
    
    -- Check 3: How many have no parent?
    DECLARE @MainCategories INT;
    SELECT @MainCategories = COUNT(*) FROM ProductCategories WHERE ParentCategoryId IS NULL;
    PRINT 'Main categories (no parent): ' + CAST(@MainCategories AS VARCHAR);
    
    IF @MainCategories = 0
    BEGIN
        PRINT 'PROBLEM: All categories have a parent (they are subcategories)';
        PRINT 'SOLUTION: Run UPDATE ProductCategories SET ParentCategoryId = NULL WHERE ...';
    END
    
    -- Check 4: How many match BOTH conditions?
    DECLARE @MainActiveCategories INT;
    SELECT @MainActiveCategories = COUNT(*) 
    FROM ProductCategories 
    WHERE IsActive = 1 AND ParentCategoryId IS NULL;
    PRINT 'Main active categories (IsActive=1 AND ParentCategoryId IS NULL): ' + CAST(@MainActiveCategories AS VARCHAR);
    
    IF @MainActiveCategories = 0
    BEGIN
        PRINT 'PROBLEM: No categories match BOTH conditions!';
        PRINT 'SOLUTION: Run URGENT_FIX_CATEGORIES.sql';
    END
    ELSE
    BEGIN
        PRINT 'SUCCESS: Categories should appear in API!';
    END
END

GO

PRINT '';
PRINT '=== SHOW ALL CATEGORIES WITH STATUS ===';
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
        WHEN IsActive = 1 AND ParentCategoryId IS NULL THEN 'OK - Will appear in API'
        ELSE 'UNKNOWN'
    END AS Status
FROM ProductCategories
ORDER BY IsActive DESC, ParentCategoryId, DisplayOrder, Name;
GO

