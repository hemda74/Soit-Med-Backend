-- ============================================================================
-- Safe SQL Script to Update Product Categories
-- ============================================================================
-- This script:
-- 1. Merges LAP and IDV categories into one (keeping LAP, removing IDV)
-- 2. Removes "Ear and Nose" / "ENT" category
-- 3. Removes "Emergency" category
--
-- IMPORTANT: Run this script in a transaction and verify results before committing
-- ============================================================================

-- ============================================================================
-- STEP 0: STUDY THE DATABASE - View ALL categories first
-- ============================================================================
-- Run this query first to see all categories in the database
PRINT '=== ALL PRODUCT CATEGORIES IN DATABASE ===';
SELECT 
    Id,
    Name,
    NameAr,
    Description,
    DescriptionAr,
    ParentCategoryId,
    DisplayOrder,
    IsActive,
    (SELECT COUNT(*) FROM Products WHERE CategoryId = ProductCategories.Id) AS ProductCount,
    (SELECT COUNT(*) FROM ProductCategories WHERE ParentCategoryId = ProductCategories.Id) AS SubCategoryCount,
    CreatedAt,
    UpdatedAt
FROM ProductCategories
ORDER BY DisplayOrder, Name;

PRINT '';
PRINT '=== CATEGORIES TO BE MODIFIED ===';
-- Show categories that match our criteria
SELECT 
    Id,
    Name,
    NameAr,
    (SELECT COUNT(*) FROM Products WHERE CategoryId = ProductCategories.Id) AS ProductCount
FROM ProductCategories
WHERE LOWER(Name) LIKE '%lap%' 
   OR LOWER(Name) LIKE '%idv%'
   OR LOWER(Name) LIKE '%ear%' 
   OR LOWER(Name) LIKE '%nose%'
   OR LOWER(Name) LIKE '%ent%'
   OR LOWER(Name) LIKE '%emergency%'
   OR LOWER(NameAr) LIKE '%طوارئ%'
   OR LOWER(NameAr) LIKE '%أنف%'
   OR LOWER(NameAr) LIKE '%أذن%'
ORDER BY Name;

PRINT '';
PRINT '=== PRODUCTS IN CATEGORIES TO BE MODIFIED ===';
-- Show products that will be affected
SELECT 
    p.Id AS ProductId,
    p.Name AS ProductName,
    p.CategoryId,
    pc.Name AS CategoryName,
    pc.NameAr AS CategoryNameAr
FROM Products p
INNER JOIN ProductCategories pc ON p.CategoryId = pc.Id
WHERE LOWER(pc.Name) LIKE '%lap%' 
   OR LOWER(pc.Name) LIKE '%idv%'
   OR LOWER(pc.Name) LIKE '%ear%' 
   OR LOWER(pc.Name) LIKE '%nose%'
   OR LOWER(pc.Name) LIKE '%ent%'
   OR LOWER(pc.Name) LIKE '%emergency%'
   OR LOWER(pc.NameAr) LIKE '%طوارئ%'
   OR LOWER(pc.NameAr) LIKE '%أنف%'
   OR LOWER(pc.NameAr) LIKE '%أذن%'
ORDER BY pc.Name, p.Name;

PRINT '';
PRINT '=== REVIEW THE ABOVE RESULTS BEFORE PROCEEDING ===';
PRINT 'If the results look correct, uncomment the BEGIN TRANSACTION line below';
PRINT 'and continue with the script.';
PRINT '';

-- ============================================================================
-- UNCOMMENT THE LINE BELOW TO START THE TRANSACTION AND MAKE CHANGES
-- ============================================================================
-- BEGIN TRANSACTION;

-- ============================================================================
-- STEP 2: Rename IVD Line to LAP Line
-- ============================================================================
-- Based on database analysis: ID 8 = "IVD Line" (8 products)
-- No LAP category exists, so we'll rename IVD Line to LAP Line

DECLARE @IdvCategoryId BIGINT;

-- Find IVD category (ID 8 based on your data)
SELECT @IdvCategoryId = Id
FROM ProductCategories
WHERE Id = 8 OR LOWER(Name) LIKE '%ivd%' OR LOWER(Name) LIKE '%idv%';

IF @IdvCategoryId IS NOT NULL
BEGIN
    DECLARE @ProductCount INT;
    SELECT @ProductCount = COUNT(*) FROM Products WHERE CategoryId = @IdvCategoryId;
    
    PRINT 'Renaming IVD Line (ID: ' + CAST(@IdvCategoryId AS VARCHAR) + ') to LAP Line';
    PRINT 'This category has ' + CAST(@ProductCount AS VARCHAR) + ' products';
    
    -- Rename IVD Line to LAP Line
    UPDATE ProductCategories
    SET Name = 'LAP Line',
        NameAr = COALESCE(NameAr, '?? ??????? ????????'), -- Keep Arabic name if exists
        UpdatedAt = GETUTCDATE()
    WHERE Id = @IdvCategoryId;
    
    PRINT 'Successfully renamed IVD Line to LAP Line';
END
ELSE
BEGIN
    PRINT 'IVD Line category not found - skipping rename';
END

-- ============================================================================
-- STEP 3: Remove "Ear, Nose & Throat" category (ID 6)
-- ============================================================================
-- Based on database analysis: ID 6 = "Ear, Nose & Throat" (0 products)

DECLARE @EarNoseCategoryId BIGINT = 6;
DECLARE @EarNoseProductCount INT;

-- Check if category exists and has products
SELECT @EarNoseProductCount = COUNT(*)
FROM Products
WHERE CategoryId = @EarNoseCategoryId;

IF EXISTS (SELECT 1 FROM ProductCategories WHERE Id = @EarNoseCategoryId)
BEGIN
    IF @EarNoseProductCount > 0
    BEGIN
        PRINT 'WARNING: Found ' + CAST(@EarNoseProductCount AS VARCHAR) + ' products in Ear, Nose & Throat category.';
        PRINT 'Setting products CategoryId to NULL.';
        
        UPDATE Products
        SET CategoryId = NULL,
            UpdatedAt = GETUTCDATE()
        WHERE CategoryId = @EarNoseCategoryId;
    END
    
    -- Remove subcategories first (if any)
    UPDATE ProductCategories
    SET ParentCategoryId = NULL,
        UpdatedAt = GETUTCDATE()
    WHERE ParentCategoryId = @EarNoseCategoryId;
    
    -- Delete the category
    DELETE FROM ProductCategories
    WHERE Id = @EarNoseCategoryId;
    
    PRINT 'Removed Ear, Nose & Throat category (ID: ' + CAST(@EarNoseCategoryId AS VARCHAR) + ')';
END
ELSE
BEGIN
    PRINT 'Ear, Nose & Throat category (ID: 6) not found - skipping';
END

-- ============================================================================
-- STEP 4: Remove "Emergency Line" category (ID 5)
-- ============================================================================
-- Based on database analysis: ID 5 = "Emergency Line" (0 products)

DECLARE @EmergencyCategoryId BIGINT = 5;
DECLARE @EmergencyProductCount INT;

-- Check if category exists and has products
SELECT @EmergencyProductCount = COUNT(*)
FROM Products
WHERE CategoryId = @EmergencyCategoryId;

IF EXISTS (SELECT 1 FROM ProductCategories WHERE Id = @EmergencyCategoryId)
BEGIN
    IF @EmergencyProductCount > 0
    BEGIN
        PRINT 'WARNING: Found ' + CAST(@EmergencyProductCount AS VARCHAR) + ' products in Emergency Line category.';
        PRINT 'Setting products CategoryId to NULL.';
        
        UPDATE Products
        SET CategoryId = NULL,
            UpdatedAt = GETUTCDATE()
        WHERE CategoryId = @EmergencyCategoryId;
    END
    
    -- Remove subcategories first (if any)
    UPDATE ProductCategories
    SET ParentCategoryId = NULL,
        UpdatedAt = GETUTCDATE()
    WHERE ParentCategoryId = @EmergencyCategoryId;
    
    -- Delete the category
    DELETE FROM ProductCategories
    WHERE Id = @EmergencyCategoryId;
    
    PRINT 'Removed Emergency Line category (ID: ' + CAST(@EmergencyCategoryId AS VARCHAR) + ')';
END
ELSE
BEGIN
    PRINT 'Emergency Line category (ID: 5) not found - skipping';
END

-- ============================================================================
-- VERIFICATION: Check results
-- ============================================================================
PRINT '';
PRINT '=== VERIFICATION ===';
PRINT 'Remaining categories:';
SELECT 
    Id,
    Name,
    NameAr,
    (SELECT COUNT(*) FROM Products WHERE CategoryId = ProductCategories.Id) AS ProductCount
FROM ProductCategories
WHERE IsActive = 1
ORDER BY DisplayOrder, Name;

PRINT '';
PRINT 'Products with NULL category (need manual assignment):';
SELECT COUNT(*) AS ProductsWithoutCategory
FROM Products
WHERE CategoryId IS NULL;

-- ============================================================================
-- ROLLBACK INSTRUCTIONS:
-- If something goes wrong, run: ROLLBACK TRANSACTION;
-- If everything looks good, run: COMMIT TRANSACTION;
-- ============================================================================

-- Uncomment the line below to commit the changes:
-- COMMIT TRANSACTION;

-- If you want to rollback instead:
-- ROLLBACK TRANSACTION;

