-- ============================================================================
-- Migrate Products from Category (text) to CategoryId (foreign key)
-- ============================================================================
-- This script maps existing Category text values to ProductCategory IDs
-- and updates the CategoryId field in the Products table
-- ============================================================================

BEGIN TRANSACTION;

PRINT '=== MIGRATING PRODUCTS FROM Category TEXT TO CategoryId ===';
PRINT '';

-- Step 1: Show current state
PRINT '--- Current State ---';
SELECT 
    'Products with NULL CategoryId' AS Status,
    COUNT(*) AS Count
FROM Products
WHERE CategoryId IS NULL;

SELECT 
    'Products with CategoryId' AS Status,
    COUNT(*) AS Count
FROM Products
WHERE CategoryId IS NOT NULL;

PRINT '';
PRINT '--- Products by Category Text ---';
SELECT 
    Category,
    COUNT(*) AS ProductCount
FROM Products
WHERE Category IS NOT NULL
GROUP BY Category
ORDER BY Category;

PRINT '';
PRINT '--- Available Categories ---';
SELECT 
    Id,
    Name,
    ParentCategoryId,
    (SELECT Name FROM ProductCategories WHERE Id = pc.ParentCategoryId) AS ParentName
FROM ProductCategories pc
WHERE IsActive = 1
ORDER BY ParentCategoryId, Name;

PRINT '';
PRINT '=== STARTING MIGRATION ===';
PRINT '';

-- Step 2: Map Category text to CategoryId
-- Based on actual data from Products table

-- Subcategories under Radiology Line (ParentCategoryId = 1)
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'X-RAY' AND ParentCategoryId = 1)
WHERE Category IN ('X-RAY', 'X-Ray', 'XRAY')
  AND CategoryId IS NULL;

UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'MOBILE X-RAY' AND ParentCategoryId = 1)
WHERE Category IN ('MOBILE X-RAY', 'Mobile X-Ray')
  AND CategoryId IS NULL;

UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'PORTABLE X-RAY' AND ParentCategoryId = 1)
WHERE Category IN ('PORTABLE X-RAY', 'Portable X-Ray')
  AND CategoryId IS NULL;

UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'DIGITAL FLAT PANEL' AND ParentCategoryId = 1)
WHERE Category = 'DIGITAL FLAT PANEL'
  AND CategoryId IS NULL;

UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'MRI' AND ParentCategoryId = 1)
WHERE Category = 'MRI'
  AND CategoryId IS NULL;

UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'CT Scanner' AND ParentCategoryId = 1)
WHERE Category IN ('CT Scanner', 'CT')
  AND CategoryId IS NULL;

UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'C-ARM' AND ParentCategoryId = 1)
WHERE Category = 'C-ARM'
  AND CategoryId IS NULL;

UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'U-ARM' AND ParentCategoryId = 1)
WHERE Category = 'U-ARM'
  AND CategoryId IS NULL;

UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'DEXA' AND ParentCategoryId = 1)
WHERE Category = 'DEXA'
  AND CategoryId IS NULL;

UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'RADIOGRAPHIC FLUOROSCOPY' AND ParentCategoryId = 1)
WHERE Category = 'RADIOGRAPHIC FLUOROSCOPY'
  AND CategoryId IS NULL;

UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'MAMMOGRAPHY' AND ParentCategoryId = 1)
WHERE Category = 'MAMMOGRAPHY'
  AND CategoryId IS NULL;

UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'ULTRASOUND' AND ParentCategoryId = 1)
WHERE Category = 'ULTRASOUND'
  AND CategoryId IS NULL;

-- Main Categories (no parent)
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'Physiotherapy Line' AND ParentCategoryId IS NULL)
WHERE Category IN ('Physiotherapy Line', 'Physiotherapy')
  AND CategoryId IS NULL;

UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'Dermatology Line' AND ParentCategoryId IS NULL)
WHERE Category = 'Dermatology Line'
  AND CategoryId IS NULL;

UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'LAP Line' AND ParentCategoryId IS NULL)
WHERE Category IN ('IVD Line', 'LAP Line')
  AND CategoryId IS NULL;

-- Handle "Radiology" category - map to Radiology Line main category (ID = 1)
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'Radiology Line' AND ParentCategoryId IS NULL)
WHERE Category = 'Radiology'
  AND CategoryId IS NULL;

PRINT 'Migration completed!';
PRINT '';

-- Step 3: Verification
PRINT '--- Migration Results ---';
SELECT 
    'Products with CategoryId after migration' AS Status,
    COUNT(*) AS Count
FROM Products
WHERE CategoryId IS NOT NULL;

SELECT 
    'Products still with NULL CategoryId' AS Status,
    COUNT(*) AS Count
FROM Products
WHERE CategoryId IS NULL;

PRINT '';
PRINT '--- Products still needing CategoryId (by Category text) ---';
SELECT 
    Category,
    COUNT(*) AS ProductCount
FROM Products
WHERE CategoryId IS NULL AND Category IS NOT NULL
GROUP BY Category
ORDER BY Category;

PRINT '';
PRINT '--- Products by CategoryId (after migration) ---';
SELECT 
    pc.Name AS CategoryName,
    COUNT(p.Id) AS ProductCount
FROM ProductCategories pc
LEFT JOIN Products p ON p.CategoryId = pc.Id
WHERE pc.IsActive = 1
GROUP BY pc.Id, pc.Name
HAVING COUNT(p.Id) > 0
ORDER BY pc.ParentCategoryId, pc.Name;

PRINT '';
PRINT '=== MIGRATION COMPLETE ===';
PRINT '';
PRINT 'Review the results above.';
PRINT 'If everything looks good, uncomment COMMIT TRANSACTION below.';
PRINT 'If there are issues, run ROLLBACK TRANSACTION instead.';
PRINT '';

-- Uncomment to commit:
-- COMMIT TRANSACTION;

-- Uncomment to rollback:
-- ROLLBACK TRANSACTION;

