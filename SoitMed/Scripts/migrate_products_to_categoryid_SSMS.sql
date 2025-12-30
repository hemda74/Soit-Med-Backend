-- ============================================================================
-- MIGRATE PRODUCTS FROM Category (text) TO CategoryId (foreign key)
-- ============================================================================
-- Run this script in SQL Server Management Studio (SSMS)
-- This will map existing Category text values to ProductCategory IDs
-- ============================================================================

USE [ITIWebApi44];
GO

-- ============================================================================
-- STEP 1: Preview - See what will be migrated
-- ============================================================================
PRINT '=== PREVIEW: Products by Category Text ===';
SELECT 
    Category,
    COUNT(*) AS ProductCount,
    CASE 
        WHEN Category IN ('X-RAY', 'X-Ray', 'XRAY') THEN 'Will map to: X-RAY subcategory'
        WHEN Category IN ('MOBILE X-RAY', 'Mobile X-Ray') THEN 'Will map to: MOBILE X-RAY subcategory'
        WHEN Category IN ('PORTABLE X-RAY', 'Portable X-Ray') THEN 'Will map to: PORTABLE X-RAY subcategory'
        WHEN Category = 'DIGITAL FLAT PANEL' THEN 'Will map to: DIGITAL FLAT PANEL subcategory'
        WHEN Category = 'MRI' THEN 'Will map to: MRI subcategory'
        WHEN Category IN ('CT Scanner', 'CT') THEN 'Will map to: CT Scanner subcategory'
        WHEN Category = 'C-ARM' THEN 'Will map to: C-ARM subcategory'
        WHEN Category = 'U-ARM' THEN 'Will map to: U-ARM subcategory'
        WHEN Category = 'DEXA' THEN 'Will map to: DEXA subcategory'
        WHEN Category = 'RADIOGRAPHIC FLUOROSCOPY' THEN 'Will map to: RADIOGRAPHIC FLUOROSCOPY subcategory'
        WHEN Category = 'MAMMOGRAPHY' THEN 'Will map to: MAMMOGRAPHY subcategory'
        WHEN Category = 'ULTRASOUND' THEN 'Will map to: ULTRASOUND subcategory'
        WHEN Category IN ('Physiotherapy Line', 'Physiotherapy') THEN 'Will map to: Physiotherapy Line main category'
        WHEN Category = 'Dermatology Line' THEN 'Will map to: Dermatology Line main category'
        WHEN Category IN ('IVD Line', 'LAP Line') THEN 'Will map to: LAP Line main category'
        WHEN Category = 'Radiology' THEN 'Will map to: Radiology Line main category'
        ELSE 'No mapping found - will remain NULL'
    END AS MigrationTarget
FROM Products
WHERE Category IS NOT NULL AND CategoryId IS NULL
GROUP BY Category
ORDER BY Category;
GO

PRINT '';
PRINT '=== Available Categories ===';
SELECT 
    Id,
    Name,
    ParentCategoryId,
    (SELECT Name FROM ProductCategories WHERE Id = pc.ParentCategoryId) AS ParentName
FROM ProductCategories pc
WHERE IsActive = 1
ORDER BY ParentCategoryId, Name;
GO

-- ============================================================================
-- STEP 2: Safety Check - Verify categories exist before migration
-- ============================================================================
PRINT '';
PRINT '=== SAFETY CHECK: Verifying all required categories exist ===';
PRINT '';

DECLARE @MissingCategories TABLE (CategoryName VARCHAR(100));

-- Check for missing subcategories
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'X-RAY' AND ParentCategoryId = 1)
    INSERT INTO @MissingCategories VALUES ('X-RAY');
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'MOBILE X-RAY' AND ParentCategoryId = 1)
    INSERT INTO @MissingCategories VALUES ('MOBILE X-RAY');
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'PORTABLE X-RAY' AND ParentCategoryId = 1)
    INSERT INTO @MissingCategories VALUES ('PORTABLE X-RAY');
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'DIGITAL FLAT PANEL' AND ParentCategoryId = 1)
    INSERT INTO @MissingCategories VALUES ('DIGITAL FLAT PANEL');
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'MRI' AND ParentCategoryId = 1)
    INSERT INTO @MissingCategories VALUES ('MRI');
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'CT Scanner' AND ParentCategoryId = 1)
    INSERT INTO @MissingCategories VALUES ('CT Scanner');
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'C-ARM' AND ParentCategoryId = 1)
    INSERT INTO @MissingCategories VALUES ('C-ARM');
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'U-ARM' AND ParentCategoryId = 1)
    INSERT INTO @MissingCategories VALUES ('U-ARM');
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'DEXA' AND ParentCategoryId = 1)
    INSERT INTO @MissingCategories VALUES ('DEXA');
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'RADIOGRAPHIC FLUOROSCOPY' AND ParentCategoryId = 1)
    INSERT INTO @MissingCategories VALUES ('RADIOGRAPHIC FLUOROSCOPY');
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'MAMMOGRAPHY' AND ParentCategoryId = 1)
    INSERT INTO @MissingCategories VALUES ('MAMMOGRAPHY');
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'ULTRASOUND' AND ParentCategoryId = 1)
    INSERT INTO @MissingCategories VALUES ('ULTRASOUND');

-- Check for missing main categories
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Physiotherapy Line' AND ParentCategoryId IS NULL)
    INSERT INTO @MissingCategories VALUES ('Physiotherapy Line');
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Dermatology Line' AND ParentCategoryId IS NULL)
    INSERT INTO @MissingCategories VALUES ('Dermatology Line');
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'LAP Line' AND ParentCategoryId IS NULL)
    INSERT INTO @MissingCategories VALUES ('LAP Line');
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Radiology Line' AND ParentCategoryId IS NULL)
    INSERT INTO @MissingCategories VALUES ('Radiology Line');

IF EXISTS (SELECT 1 FROM @MissingCategories)
BEGIN
    PRINT '❌ ERROR: Missing required categories!';
    PRINT 'The following categories are missing from ProductCategories table:';
    SELECT CategoryName FROM @MissingCategories;
    PRINT '';
    PRINT '⚠️  MIGRATION ABORTED - Please create missing categories first!';
    PRINT 'Run ROLLBACK TRANSACTION if transaction was started.';
    RETURN;
END
ELSE
BEGIN
    PRINT '✅ All required categories exist. Safe to proceed.';
    PRINT '';
END
GO

-- ============================================================================
-- STEP 3: Start Transaction
-- ============================================================================
BEGIN TRANSACTION;
GO

PRINT '';
PRINT '=== STARTING MIGRATION ===';
PRINT '⚠️  IMPORTANT: This script will ONLY UPDATE CategoryId field.';
PRINT '⚠️  The Category text field will remain unchanged (no data loss).';
PRINT '⚠️  Only products with CategoryId IS NULL will be updated.';
PRINT '';

-- ============================================================================
-- STEP 3: Map Subcategories under Radiology Line (ParentCategoryId = 1)
-- ============================================================================

-- X-RAY
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'X-RAY' AND ParentCategoryId = 1),
    UpdatedAt = GETUTCDATE()
WHERE Category IN ('X-RAY', 'X-Ray', 'XRAY')
  AND CategoryId IS NULL;
PRINT 'Mapped X-RAY products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- MOBILE X-RAY
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'MOBILE X-RAY' AND ParentCategoryId = 1),
    UpdatedAt = GETUTCDATE()
WHERE Category IN ('MOBILE X-RAY', 'Mobile X-Ray')
  AND CategoryId IS NULL;
PRINT 'Mapped MOBILE X-RAY products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- PORTABLE X-RAY
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'PORTABLE X-RAY' AND ParentCategoryId = 1),
    UpdatedAt = GETUTCDATE()
WHERE Category IN ('PORTABLE X-RAY', 'Portable X-Ray')
  AND CategoryId IS NULL;
PRINT 'Mapped PORTABLE X-RAY products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- DIGITAL FLAT PANEL
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'DIGITAL FLAT PANEL' AND ParentCategoryId = 1),
    UpdatedAt = GETUTCDATE()
WHERE Category = 'DIGITAL FLAT PANEL'
  AND CategoryId IS NULL;
PRINT 'Mapped DIGITAL FLAT PANEL products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- MRI
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'MRI' AND ParentCategoryId = 1),
    UpdatedAt = GETUTCDATE()
WHERE Category = 'MRI'
  AND CategoryId IS NULL;
PRINT 'Mapped MRI products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- CT Scanner
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'CT Scanner' AND ParentCategoryId = 1),
    UpdatedAt = GETUTCDATE()
WHERE Category IN ('CT Scanner', 'CT')
  AND CategoryId IS NULL;
PRINT 'Mapped CT Scanner products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- C-ARM
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'C-ARM' AND ParentCategoryId = 1),
    UpdatedAt = GETUTCDATE()
WHERE Category = 'C-ARM'
  AND CategoryId IS NULL;
PRINT 'Mapped C-ARM products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- U-ARM
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'U-ARM' AND ParentCategoryId = 1),
    UpdatedAt = GETUTCDATE()
WHERE Category = 'U-ARM'
  AND CategoryId IS NULL;
PRINT 'Mapped U-ARM products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- DEXA
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'DEXA' AND ParentCategoryId = 1),
    UpdatedAt = GETUTCDATE()
WHERE Category = 'DEXA'
  AND CategoryId IS NULL;
PRINT 'Mapped DEXA products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- RADIOGRAPHIC FLUOROSCOPY
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'RADIOGRAPHIC FLUOROSCOPY' AND ParentCategoryId = 1),
    UpdatedAt = GETUTCDATE()
WHERE Category = 'RADIOGRAPHIC FLUOROSCOPY'
  AND CategoryId IS NULL;
PRINT 'Mapped RADIOGRAPHIC FLUOROSCOPY products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- MAMMOGRAPHY
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'MAMMOGRAPHY' AND ParentCategoryId = 1),
    UpdatedAt = GETUTCDATE()
WHERE Category = 'MAMMOGRAPHY'
  AND CategoryId IS NULL;
PRINT 'Mapped MAMMOGRAPHY products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- ULTRASOUND
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'ULTRASOUND' AND ParentCategoryId = 1),
    UpdatedAt = GETUTCDATE()
WHERE Category = 'ULTRASOUND'
  AND CategoryId IS NULL;
PRINT 'Mapped ULTRASOUND products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- ============================================================================
-- STEP 4: Map Main Categories (no parent)
-- ============================================================================

-- Physiotherapy Line
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'Physiotherapy Line' AND ParentCategoryId IS NULL),
    UpdatedAt = GETUTCDATE()
WHERE Category IN ('Physiotherapy Line', 'Physiotherapy')
  AND CategoryId IS NULL;
PRINT 'Mapped Physiotherapy Line products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- Dermatology Line
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'Dermatology Line' AND ParentCategoryId IS NULL),
    UpdatedAt = GETUTCDATE()
WHERE Category = 'Dermatology Line'
  AND CategoryId IS NULL;
PRINT 'Mapped Dermatology Line products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- LAP Line (formerly IVD Line)
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'LAP Line' AND ParentCategoryId IS NULL),
    UpdatedAt = GETUTCDATE()
WHERE Category IN ('IVD Line', 'LAP Line')
  AND CategoryId IS NULL;
PRINT 'Mapped LAP Line (IVD Line) products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- Radiology (maps to Radiology Line main category)
UPDATE Products
SET CategoryId = (SELECT TOP 1 Id FROM ProductCategories WHERE Name = 'Radiology Line' AND ParentCategoryId IS NULL),
    UpdatedAt = GETUTCDATE()
WHERE Category = 'Radiology'
  AND CategoryId IS NULL;
PRINT 'Mapped Radiology products: ' + CAST(@@ROWCOUNT AS VARCHAR) + ' products';

-- ============================================================================
-- STEP 5: Verification and Safety Check
-- ============================================================================
PRINT '';
PRINT '=== MIGRATION RESULTS ===';
PRINT '';

DECLARE @ProductsWithCategoryId INT;
DECLARE @ProductsStillNull INT;
DECLARE @TotalProducts INT;

SELECT @ProductsWithCategoryId = COUNT(*) FROM Products WHERE CategoryId IS NOT NULL;
SELECT @ProductsStillNull = COUNT(*) FROM Products WHERE CategoryId IS NULL;
SELECT @TotalProducts = COUNT(*) FROM Products;

SELECT 
    'Products with CategoryId after migration' AS Status,
    @ProductsWithCategoryId AS Count,
    CAST(ROUND((@ProductsWithCategoryId * 100.0 / NULLIF(@TotalProducts, 0)), 2) AS DECIMAL(5,2)) AS Percentage;
GO

SELECT 
    'Products still with NULL CategoryId' AS Status,
    COUNT(*) AS Count
FROM Products
WHERE CategoryId IS NULL;
GO

-- Safety check: Verify no products were accidentally updated
PRINT '';
PRINT '=== SAFETY CHECK: Verifying data integrity ===';
SELECT 
    'Products where CategoryId was updated but Category text doesn''t match' AS CheckType,
    COUNT(*) AS Count
FROM Products p
INNER JOIN ProductCategories pc ON p.CategoryId = pc.Id
WHERE p.Category IS NOT NULL 
  AND p.CategoryId IS NOT NULL
  AND p.Category != pc.Name
  AND p.Category NOT IN (
      -- Allow some known variations
      SELECT 'Radiology' WHERE pc.Name = 'Radiology Line'
      UNION ALL
      SELECT 'Physiotherapy' WHERE pc.Name = 'Physiotherapy Line'
      UNION ALL
      SELECT 'IVD Line' WHERE pc.Name = 'LAP Line'
  );
GO

-- Show sample of migrated products for verification
PRINT '';
PRINT '=== SAMPLE: First 10 migrated products (for verification) ===';
SELECT TOP 10
    p.Id,
    p.Name,
    p.Category AS OldCategoryText,
    p.CategoryId,
    pc.Name AS NewCategoryName,
    CASE 
        WHEN p.Category IS NOT NULL AND pc.Name IS NOT NULL THEN '✅ Migrated'
        WHEN p.CategoryId IS NULL THEN '⚠️  Not migrated'
        ELSE '✅ Has CategoryId'
    END AS Status
FROM Products p
LEFT JOIN ProductCategories pc ON p.CategoryId = pc.Id
WHERE p.CategoryId IS NOT NULL
ORDER BY p.Id;
GO

PRINT '';
PRINT '=== Products by CategoryId (after migration) ===';
SELECT 
    pc.Name AS CategoryName,
    COUNT(p.Id) AS ProductCount
FROM ProductCategories pc
LEFT JOIN Products p ON p.CategoryId = pc.Id
WHERE pc.IsActive = 1
GROUP BY pc.Id, pc.Name, pc.ParentCategoryId
HAVING COUNT(p.Id) > 0
ORDER BY pc.ParentCategoryId, pc.Name;
GO

PRINT '';
PRINT '=== Products still needing CategoryId (by Category text) ===';
SELECT 
    Category,
    COUNT(*) AS ProductCount
FROM Products
WHERE CategoryId IS NULL AND Category IS NOT NULL
GROUP BY Category
ORDER BY Category;
GO

PRINT '';
PRINT '=== FINAL SAFETY SUMMARY ===';
PRINT '';
PRINT '✅ This script is SAFE because:';
PRINT '   1. Only UPDATES CategoryId field (does not DELETE any data)';
PRINT '   2. Keeps the Category text field unchanged (backward compatible)';
PRINT '   3. Only updates products where CategoryId IS NULL (won''t overwrite existing data)';
PRINT '   4. Uses TRANSACTION (can rollback if needed)';
PRINT '   5. Verifies all categories exist before starting';
PRINT '';
PRINT '⚠️  IMPORTANT: Review all results above before committing!';
PRINT '';
PRINT '=== NEXT STEPS ===';
PRINT '1. Review the migration results above';
PRINT '2. Check the sample migrated products';
PRINT '3. If everything looks correct, run: COMMIT TRANSACTION;';
PRINT '4. If there are any issues, run: ROLLBACK TRANSACTION;';
PRINT '';

-- ============================================================================
-- UNCOMMENT ONE OF THE FOLLOWING LINES AFTER REVIEWING RESULTS:
-- ============================================================================

-- COMMIT TRANSACTION;  -- ✅ Uncomment this to SAVE changes (only after reviewing results!)
-- ROLLBACK TRANSACTION;  -- ⚠️  Uncomment this to UNDO changes (if something went wrong)

