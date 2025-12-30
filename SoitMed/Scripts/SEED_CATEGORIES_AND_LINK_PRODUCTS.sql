-- ============================================================================
-- SEED CATEGORIES AND LINK PRODUCTS
-- ============================================================================
-- This script:
-- 1. Creates all main categories if they don't exist
-- 2. Creates all subcategories if they don't exist
-- 3. Links all products to categories based on their Category (text) field
-- ============================================================================

USE [ITIWebApi44];
GO

PRINT '=== SEEDING CATEGORIES AND LINKING PRODUCTS ===';
PRINT '';

BEGIN TRANSACTION;
GO

-- ============================================================================
-- STEP 1: CREATE MAIN CATEGORIES
-- ============================================================================
PRINT '=== STEP 1: Creating Main Categories ===';

-- Radiology Line
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Radiology Line' AND ParentCategoryId IS NULL)
BEGIN
    INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES ('Radiology Line', N'خط الأشعة', 'Medical imaging and radiology equipment', N'معدات التصوير الطبي والأشعة', NULL, 1, 1, GETUTCDATE(), GETUTCDATE());
    PRINT 'Created: Radiology Line';
END
ELSE
BEGIN
    UPDATE ProductCategories 
    SET IsActive = 1, ParentCategoryId = NULL, DisplayOrder = 1
    WHERE Name = 'Radiology Line' AND ParentCategoryId IS NULL;
    PRINT 'Updated: Radiology Line';
END
GO

-- Physiotherapy Line
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Physiotherapy Line' AND ParentCategoryId IS NULL)
BEGIN
    INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES ('Physiotherapy Line', N'خط العلاج الطبيعي', 'Physiotherapy and rehabilitation equipment', N'معدات العلاج الطبيعي وإعادة التأهيل', NULL, 2, 1, GETUTCDATE(), GETUTCDATE());
    PRINT 'Created: Physiotherapy Line';
END
ELSE
BEGIN
    UPDATE ProductCategories 
    SET IsActive = 1, ParentCategoryId = NULL, DisplayOrder = 2
    WHERE Name = 'Physiotherapy Line' AND ParentCategoryId IS NULL;
    PRINT 'Updated: Physiotherapy Line';
END
GO

-- Dental Line
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Dental Line' AND ParentCategoryId IS NULL)
BEGIN
    INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES ('Dental Line', N'خط طب الأسنان', 'Dental equipment and supplies', N'معدات ولوازم طب الأسنان', NULL, 3, 1, GETUTCDATE(), GETUTCDATE());
    PRINT 'Created: Dental Line';
END
ELSE
BEGIN
    UPDATE ProductCategories 
    SET IsActive = 1, ParentCategoryId = NULL, DisplayOrder = 3
    WHERE Name = 'Dental Line' AND ParentCategoryId IS NULL;
    PRINT 'Updated: Dental Line';
END
GO

-- Laboratory Line
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Laboratory Line' AND ParentCategoryId IS NULL)
BEGIN
    INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES ('Laboratory Line', N'خط المختبر', 'Laboratory equipment and diagnostics', N'معدات المختبر والتشخيص', NULL, 4, 1, GETUTCDATE(), GETUTCDATE());
    PRINT 'Created: Laboratory Line';
END
ELSE
BEGIN
    UPDATE ProductCategories 
    SET IsActive = 1, ParentCategoryId = NULL, DisplayOrder = 4
    WHERE Name = 'Laboratory Line' AND ParentCategoryId IS NULL;
    PRINT 'Updated: Laboratory Line';
END
GO

-- Dermatology Line
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Dermatology Line' AND ParentCategoryId IS NULL)
BEGIN
    INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES ('Dermatology Line', N'خط الأمراض الجلدية', 'Dermatology equipment and devices', N'معدات وأجهزة الأمراض الجلدية', NULL, 7, 1, GETUTCDATE(), GETUTCDATE());
    PRINT 'Created: Dermatology Line';
END
ELSE
BEGIN
    UPDATE ProductCategories 
    SET IsActive = 1, ParentCategoryId = NULL, DisplayOrder = 7
    WHERE Name = 'Dermatology Line' AND ParentCategoryId IS NULL;
    PRINT 'Updated: Dermatology Line';
END
GO

-- LAP Line (merged from IVD Line)
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'LAP Line' AND ParentCategoryId IS NULL)
BEGIN
    INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES ('LAP Line', N'خط المختبر', 'In Vitro Diagnostics equipment', N'معدات التشخيص المخبري', NULL, 8, 1, GETUTCDATE(), GETUTCDATE());
    PRINT 'Created: LAP Line';
END
ELSE
BEGIN
    UPDATE ProductCategories 
    SET IsActive = 1, ParentCategoryId = NULL, DisplayOrder = 8
    WHERE Name = 'LAP Line' AND ParentCategoryId IS NULL;
    PRINT 'Updated: LAP Line';
END
GO

-- ============================================================================
-- STEP 2: GET MAIN CATEGORY IDs AND CREATE SUBCATEGORIES
-- ============================================================================
PRINT '';
PRINT '=== STEP 2: Creating Subcategories ===';

-- Declare all variables at the start of this batch
DECLARE @RadiologyLineId BIGINT;
DECLARE @PhysiotherapyLineId BIGINT;
DECLARE @DentalLineId BIGINT;
DECLARE @LaboratoryLineId BIGINT;
DECLARE @DermatologyLineId BIGINT;
DECLARE @LAPLineId BIGINT;

-- Get main category IDs
SELECT @RadiologyLineId = Id FROM ProductCategories WHERE Name = 'Radiology Line' AND ParentCategoryId IS NULL;
SELECT @PhysiotherapyLineId = Id FROM ProductCategories WHERE Name = 'Physiotherapy Line' AND ParentCategoryId IS NULL;
SELECT @DentalLineId = Id FROM ProductCategories WHERE Name = 'Dental Line' AND ParentCategoryId IS NULL;
SELECT @LaboratoryLineId = Id FROM ProductCategories WHERE Name = 'Laboratory Line' AND ParentCategoryId IS NULL;
SELECT @DermatologyLineId = Id FROM ProductCategories WHERE Name = 'Dermatology Line' AND ParentCategoryId IS NULL;
SELECT @LAPLineId = Id FROM ProductCategories WHERE Name = 'LAP Line' AND ParentCategoryId IS NULL;

-- ============================================================================
-- STEP 3: CREATE RADIOLOGY SUBCATEGORIES
-- ============================================================================
IF @RadiologyLineId IS NOT NULL
BEGIN
    -- X-RAY
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'X-RAY' AND ParentCategoryId = @RadiologyLineId)
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('X-RAY', N'الأشعة السينية', NULL, NULL, @RadiologyLineId, 1, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: X-RAY';
    END
    
    -- MOBILE X-RAY
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'MOBILE X-RAY' AND ParentCategoryId = @RadiologyLineId)
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('MOBILE X-RAY', N'الأشعة السينية المتنقلة', NULL, NULL, @RadiologyLineId, 2, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: MOBILE X-RAY';
    END
    
    -- PORTABLE X-RAY
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'PORTABLE X-RAY' AND ParentCategoryId = @RadiologyLineId)
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('PORTABLE X-RAY', N'الأشعة السينية المحمولة', NULL, NULL, @RadiologyLineId, 3, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: PORTABLE X-RAY';
    END
    
    -- DIGITAL FLAT PANEL
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'DIGITAL FLAT PANEL' AND ParentCategoryId = @RadiologyLineId)
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('DIGITAL FLAT PANEL', N'الشاشة المسطحة الرقمية', NULL, NULL, @RadiologyLineId, 4, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: DIGITAL FLAT PANEL';
    END
    
    -- MRI
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'MRI' AND ParentCategoryId = @RadiologyLineId)
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('MRI', N'الرنين المغناطيسي', NULL, NULL, @RadiologyLineId, 5, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: MRI';
    END
    
    -- CT Scanner
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'CT Scanner' AND ParentCategoryId = @RadiologyLineId)
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('CT Scanner', N'الماسح الضوئي', NULL, NULL, @RadiologyLineId, 6, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: CT Scanner';
    END
    
    -- C-ARM
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'C-ARM' AND ParentCategoryId = @RadiologyLineId)
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('C-ARM', N'جهاز C-ARM', NULL, NULL, @RadiologyLineId, 7, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: C-ARM';
    END
    
    -- U-ARM
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'U-ARM' AND ParentCategoryId = @RadiologyLineId)
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('U-ARM', N'جهاز U-ARM', NULL, NULL, @RadiologyLineId, 8, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: U-ARM';
    END
    
    -- DEXA
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'DEXA' AND ParentCategoryId = @RadiologyLineId)
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('DEXA', N'قياس كثافة العظام', NULL, NULL, @RadiologyLineId, 9, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: DEXA';
    END
    
    -- RADIOGRAPHIC FLUOROSCOPY
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'RADIOGRAPHIC FLUOROSCOPY' AND ParentCategoryId = @RadiologyLineId)
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('RADIOGRAPHIC FLUOROSCOPY', N'التنظير الشعاعي', NULL, NULL, @RadiologyLineId, 10, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: RADIOGRAPHIC FLUOROSCOPY';
    END
    
    -- MAMMOGRAPHY
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'MAMMOGRAPHY' AND ParentCategoryId = @RadiologyLineId)
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('MAMMOGRAPHY', N'تصوير الثدي', NULL, NULL, @RadiologyLineId, 11, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: MAMMOGRAPHY';
    END
    
    -- ULTRASOUND
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'ULTRASOUND' AND ParentCategoryId = @RadiologyLineId)
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('ULTRASOUND', N'الموجات فوق الصوتية', NULL, NULL, @RadiologyLineId, 12, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: ULTRASOUND';
    END
END
GO

-- ============================================================================
-- STEP 3: GET SUBCATEGORY IDs FOR PRODUCT MAPPING AND LINK PRODUCTS
-- ============================================================================
PRINT '';
PRINT '=== STEP 3: Linking Products to Categories ===';

-- Declare subcategory variables
DECLARE @XRayId BIGINT;
DECLARE @MobileXRayId BIGINT;
DECLARE @PortableXRayId BIGINT;
DECLARE @DigitalFlatPanelId BIGINT;
DECLARE @MRIId BIGINT;
DECLARE @CTScannerId BIGINT;
DECLARE @CArmId BIGINT;
DECLARE @UArmId BIGINT;
DECLARE @DEXAId BIGINT;
DECLARE @RadiographicFluoroscopyId BIGINT;
DECLARE @MammographyId BIGINT;
DECLARE @UltrasoundId BIGINT;

-- Declare main category variables again (needed after GO statement)
DECLARE @RadiologyLineId2 BIGINT;
DECLARE @PhysiotherapyLineId2 BIGINT;
DECLARE @DentalLineId2 BIGINT;
DECLARE @LaboratoryLineId2 BIGINT;
DECLARE @DermatologyLineId2 BIGINT;
DECLARE @LAPLineId2 BIGINT;

-- Get main category IDs again (needed for subcategory lookup and product linking)
SELECT @RadiologyLineId2 = Id FROM ProductCategories WHERE Name = 'Radiology Line' AND ParentCategoryId IS NULL;
SELECT @PhysiotherapyLineId2 = Id FROM ProductCategories WHERE Name = 'Physiotherapy Line' AND ParentCategoryId IS NULL;
SELECT @DentalLineId2 = Id FROM ProductCategories WHERE Name = 'Dental Line' AND ParentCategoryId IS NULL;
SELECT @LaboratoryLineId2 = Id FROM ProductCategories WHERE Name = 'Laboratory Line' AND ParentCategoryId IS NULL;
SELECT @DermatologyLineId2 = Id FROM ProductCategories WHERE Name = 'Dermatology Line' AND ParentCategoryId IS NULL;
SELECT @LAPLineId2 = Id FROM ProductCategories WHERE Name = 'LAP Line' AND ParentCategoryId IS NULL;

-- Get subcategory IDs
SELECT @XRayId = Id FROM ProductCategories WHERE Name = 'X-RAY' AND ParentCategoryId = @RadiologyLineId2;
SELECT @MobileXRayId = Id FROM ProductCategories WHERE Name = 'MOBILE X-RAY' AND ParentCategoryId = @RadiologyLineId2;
SELECT @PortableXRayId = Id FROM ProductCategories WHERE Name = 'PORTABLE X-RAY' AND ParentCategoryId = @RadiologyLineId2;
SELECT @DigitalFlatPanelId = Id FROM ProductCategories WHERE Name = 'DIGITAL FLAT PANEL' AND ParentCategoryId = @RadiologyLineId2;
SELECT @MRIId = Id FROM ProductCategories WHERE Name = 'MRI' AND ParentCategoryId = @RadiologyLineId2;
SELECT @CTScannerId = Id FROM ProductCategories WHERE Name = 'CT Scanner' AND ParentCategoryId = @RadiologyLineId2;
SELECT @CArmId = Id FROM ProductCategories WHERE Name = 'C-ARM' AND ParentCategoryId = @RadiologyLineId2;
SELECT @UArmId = Id FROM ProductCategories WHERE Name = 'U-ARM' AND ParentCategoryId = @RadiologyLineId2;
SELECT @DEXAId = Id FROM ProductCategories WHERE Name = 'DEXA' AND ParentCategoryId = @RadiologyLineId2;
SELECT @RadiographicFluoroscopyId = Id FROM ProductCategories WHERE Name = 'RADIOGRAPHIC FLUOROSCOPY' AND ParentCategoryId = @RadiologyLineId2;
SELECT @MammographyId = Id FROM ProductCategories WHERE Name = 'MAMMOGRAPHY' AND ParentCategoryId = @RadiologyLineId2;
SELECT @UltrasoundId = Id FROM ProductCategories WHERE Name = 'ULTRASOUND' AND ParentCategoryId = @RadiologyLineId2;

-- ============================================================================
-- LINK PRODUCTS TO CATEGORIES (continuing in same batch)
-- ============================================================================

-- Map products based on Category (text) field to CategoryId (FK)
-- X-RAY products
UPDATE Products
SET CategoryId = @XRayId
WHERE Category = 'X-RAY' AND CategoryId IS NULL;
PRINT 'Linked X-RAY products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- MOBILE X-RAY products
UPDATE Products
SET CategoryId = @MobileXRayId
WHERE Category = 'MOBILE X-RAY' AND CategoryId IS NULL;
PRINT 'Linked MOBILE X-RAY products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- PORTABLE X-RAY products
UPDATE Products
SET CategoryId = @PortableXRayId
WHERE Category = 'PORTABLE X-RAY' AND CategoryId IS NULL;
PRINT 'Linked PORTABLE X-RAY products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- DIGITAL FLAT PANEL products
UPDATE Products
SET CategoryId = @DigitalFlatPanelId
WHERE Category = 'DIGITAL FLAT PANEL' AND CategoryId IS NULL;
PRINT 'Linked DIGITAL FLAT PANEL products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- MRI products
UPDATE Products
SET CategoryId = @MRIId
WHERE Category = 'MRI' AND CategoryId IS NULL;
PRINT 'Linked MRI products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- CT Scanner products
UPDATE Products
SET CategoryId = @CTScannerId
WHERE Category = 'CT Scanner' AND CategoryId IS NULL;
PRINT 'Linked CT Scanner products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- C-ARM products
UPDATE Products
SET CategoryId = @CArmId
WHERE Category = 'C-ARM' AND CategoryId IS NULL;
PRINT 'Linked C-ARM products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- U-ARM products
UPDATE Products
SET CategoryId = @UArmId
WHERE Category = 'U-ARM' AND CategoryId IS NULL;
PRINT 'Linked U-ARM products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- DEXA products
UPDATE Products
SET CategoryId = @DEXAId
WHERE Category = 'DEXA' AND CategoryId IS NULL;
PRINT 'Linked DEXA products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- RADIOGRAPHIC FLUOROSCOPY products
UPDATE Products
SET CategoryId = @RadiographicFluoroscopyId
WHERE Category = 'RADIOGRAPHIC FLUOROSCOPY' AND CategoryId IS NULL;
PRINT 'Linked RADIOGRAPHIC FLUOROSCOPY products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- MAMMOGRAPHY products
UPDATE Products
SET CategoryId = @MammographyId
WHERE Category = 'MAMMOGRAPHY' AND CategoryId IS NULL;
PRINT 'Linked MAMMOGRAPHY products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- ULTRASOUND products
UPDATE Products
SET CategoryId = @UltrasoundId
WHERE Category = 'ULTRASOUND' AND CategoryId IS NULL;
PRINT 'Linked ULTRASOUND products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- Physiotherapy Line products
UPDATE Products
SET CategoryId = @PhysiotherapyLineId2
WHERE (Category = 'Physiotherapy Line' OR Category = 'Physiotherapy') AND CategoryId IS NULL;
PRINT 'Linked Physiotherapy products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- Dental Line products
UPDATE Products
SET CategoryId = @DentalLineId2
WHERE Category = 'Dental Line' AND CategoryId IS NULL;
PRINT 'Linked Dental products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- Laboratory Line products
UPDATE Products
SET CategoryId = @LaboratoryLineId2
WHERE (Category = 'Laboratory Line' OR Category = 'Laboratory') AND CategoryId IS NULL;
PRINT 'Linked Laboratory products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- Dermatology Line products
UPDATE Products
SET CategoryId = @DermatologyLineId2
WHERE Category = 'Dermatology Line' AND CategoryId IS NULL;
PRINT 'Linked Dermatology products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- LAP Line / IVD Line products (merged)
UPDATE Products
SET CategoryId = @LAPLineId2
WHERE (Category = 'LAP Line' OR Category = 'IVD Line' OR Category = 'IVD') AND CategoryId IS NULL;
PRINT 'Linked LAP/IVD products: ' + CAST(@@ROWCOUNT AS VARCHAR);

-- Radiology products (general - map to Radiology Line main category if no specific subcategory)
UPDATE Products
SET CategoryId = @RadiologyLineId2
WHERE (Category = 'Radiology' OR Category = 'Radiology Line') 
  AND CategoryId IS NULL
  AND Category NOT IN ('X-RAY', 'MOBILE X-RAY', 'PORTABLE X-RAY', 'DIGITAL FLAT PANEL', 'MRI', 'CT Scanner', 'C-ARM', 'U-ARM', 'DEXA', 'RADIOGRAPHIC FLUOROSCOPY', 'MAMMOGRAPHY', 'ULTRASOUND');
PRINT 'Linked general Radiology products: ' + CAST(@@ROWCOUNT AS VARCHAR);

GO

-- ============================================================================
-- STEP 6: VERIFICATION
-- ============================================================================
PRINT '';
PRINT '=== STEP 4: VERIFICATION ===';
PRINT '';

-- Count main categories
SELECT 
    'Main Categories' AS Status,
    COUNT(*) AS Count
FROM ProductCategories
WHERE IsActive = 1 AND ParentCategoryId IS NULL;
GO

-- Count products with CategoryId
SELECT 
    'Products with CategoryId' AS Status,
    COUNT(*) AS Count
FROM Products
WHERE CategoryId IS NOT NULL;
GO

-- Count products without CategoryId
SELECT 
    'Products without CategoryId' AS Status,
    COUNT(*) AS Count
FROM Products
WHERE CategoryId IS NULL AND Category IS NOT NULL;
GO

-- Show category product counts
SELECT 
    pc.Name AS CategoryName,
    COUNT(p.Id) AS ProductCount
FROM ProductCategories pc
LEFT JOIN Products p ON pc.Id = p.CategoryId
WHERE pc.IsActive = 1 AND pc.ParentCategoryId IS NULL
GROUP BY pc.Name
ORDER BY ProductCount DESC, pc.Name;
GO

PRINT '';
PRINT '=== SEEDING COMPLETE ===';
PRINT '';
PRINT 'Review the results above. If everything looks good, commit the transaction.';
PRINT '';
PRINT 'To commit: COMMIT TRANSACTION;';
PRINT 'To rollback: ROLLBACK TRANSACTION;';
PRINT '';

-- Uncomment to commit automatically:
COMMIT TRANSACTION;

-- If you want to review first, comment the COMMIT above and uncomment ROLLBACK:
-- ROLLBACK TRANSACTION;

