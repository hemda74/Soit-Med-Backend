-- ============================================================================
-- URGENT FIX: ProductCategories Not Showing
-- ============================================================================
-- This script will:
-- 1. Check current state of ProductCategories
-- 2. Ensure main categories are active and have no parent
-- 3. Fix any issues preventing categories from appearing
-- ============================================================================

USE [ITIWebApi44];
GO

PRINT '=== STEP 1: DIAGNOSTIC - Current State ===';
PRINT '';

-- Check total categories
SELECT 'Total Categories' AS Status, COUNT(*) AS Count FROM ProductCategories;
GO

-- Check active categories
SELECT 'Active Categories' AS Status, COUNT(*) AS Count FROM ProductCategories WHERE IsActive = 1;
GO

-- Check main categories (no parent)
SELECT 'Main Categories (no parent)' AS Status, COUNT(*) AS Count FROM ProductCategories WHERE ParentCategoryId IS NULL;
GO

-- Check main active categories (what API needs)
SELECT 'Main Active Categories (IsActive=1 AND ParentCategoryId IS NULL)' AS Status, COUNT(*) AS Count 
FROM ProductCategories 
WHERE IsActive = 1 AND ParentCategoryId IS NULL;
GO

-- Show all categories
PRINT '';
PRINT '=== ALL CATEGORIES ===';
SELECT 
    Id,
    Name,
    IsActive,
    ParentCategoryId,
    DisplayOrder,
    CASE 
        WHEN IsActive = 1 AND ParentCategoryId IS NULL THEN '✅ Should appear in API'
        ELSE '❌ Will NOT appear in API'
    END AS WillAppearInAPI
FROM ProductCategories
ORDER BY IsActive DESC, ParentCategoryId, DisplayOrder, Name;
GO

PRINT '';
PRINT '=== STEP 2: FIX - Ensure Main Categories Are Active ===';
PRINT '⚠️  If ProductCategories table is empty, you need to create categories first!';
PRINT '';

BEGIN TRANSACTION;
GO

-- First, check if categories exist
DECLARE @CategoryCount INT;
SELECT @CategoryCount = COUNT(*) FROM ProductCategories;

IF @CategoryCount = 0
BEGIN
    PRINT '❌ ERROR: ProductCategories table is EMPTY!';
    PRINT 'You need to create categories first.';
    PRINT '';
    PRINT 'Creating main categories...';
    
    -- Create main categories if they don't exist
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Radiology Line')
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('Radiology Line', N'خط الأشعة', 'Medical imaging and radiology equipment', N'معدات التصوير الطبي والأشعة', NULL, 1, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: Radiology Line';
    END
    
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Physiotherapy Line')
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('Physiotherapy Line', N'خط العلاج الطبيعي', 'Physiotherapy and rehabilitation equipment', N'معدات العلاج الطبيعي وإعادة التأهيل', NULL, 2, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: Physiotherapy Line';
    END
    
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Dental Line')
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('Dental Line', N'خط طب الأسنان', 'Dental equipment and supplies', N'معدات ولوازم طب الأسنان', NULL, 3, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: Dental Line';
    END
    
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Laboratory Line')
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('Laboratory Line', N'خط المختبر', 'Laboratory equipment and diagnostics', N'معدات المختبر والتشخيص', NULL, 4, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: Laboratory Line';
    END
    
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'Dermatology Line')
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('Dermatology Line', N'خط الأمراض الجلدية', 'Dermatology equipment and devices', N'معدات وأجهزة الأمراض الجلدية', NULL, 7, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: Dermatology Line';
    END
    
    IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'LAP Line')
    BEGIN
        INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
        VALUES ('LAP Line', N'خط المختبر', 'In Vitro Diagnostics equipment', N'معدات التشخيص المخبري', NULL, 8, 1, GETUTCDATE(), GETUTCDATE());
        PRINT 'Created: LAP Line';
    END
    
    -- Get Radiology Line ID for subcategories
    DECLARE @RadiologyLineId BIGINT;
    SELECT @RadiologyLineId = Id FROM ProductCategories WHERE Name = 'Radiology Line';
    
    -- Create subcategories under Radiology Line
    IF @RadiologyLineId IS NOT NULL
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'X-RAY' AND ParentCategoryId = @RadiologyLineId)
        BEGIN
            INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
            VALUES ('X-RAY', N'الأشعة السينية', NULL, NULL, @RadiologyLineId, 1, 1, GETUTCDATE(), GETUTCDATE());
        END
        
        IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'MOBILE X-RAY' AND ParentCategoryId = @RadiologyLineId)
        BEGIN
            INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
            VALUES ('MOBILE X-RAY', N'الأشعة السينية المتنقلة', NULL, NULL, @RadiologyLineId, 2, 1, GETUTCDATE(), GETUTCDATE());
        END
        
        IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'PORTABLE X-RAY' AND ParentCategoryId = @RadiologyLineId)
        BEGIN
            INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
            VALUES ('PORTABLE X-RAY', N'الأشعة السينية المحمولة', NULL, NULL, @RadiologyLineId, 3, 1, GETUTCDATE(), GETUTCDATE());
        END
        
        IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'DIGITAL FLAT PANEL' AND ParentCategoryId = @RadiologyLineId)
        BEGIN
            INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
            VALUES ('DIGITAL FLAT PANEL', N'الشاشة المسطحة الرقمية', NULL, NULL, @RadiologyLineId, 4, 1, GETUTCDATE(), GETUTCDATE());
        END
        
        IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'MRI' AND ParentCategoryId = @RadiologyLineId)
        BEGIN
            INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
            VALUES ('MRI', N'الرنين المغناطيسي', NULL, NULL, @RadiologyLineId, 5, 1, GETUTCDATE(), GETUTCDATE());
        END
        
        IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'CT Scanner' AND ParentCategoryId = @RadiologyLineId)
        BEGIN
            INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
            VALUES ('CT Scanner', N'الماسح الضوئي', NULL, NULL, @RadiologyLineId, 6, 1, GETUTCDATE(), GETUTCDATE());
        END
        
        IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'C-ARM' AND ParentCategoryId = @RadiologyLineId)
        BEGIN
            INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
            VALUES ('C-ARM', N'جهاز C-ARM', NULL, NULL, @RadiologyLineId, 7, 1, GETUTCDATE(), GETUTCDATE());
        END
        
        IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'U-ARM' AND ParentCategoryId = @RadiologyLineId)
        BEGIN
            INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
            VALUES ('U-ARM', N'جهاز U-ARM', NULL, NULL, @RadiologyLineId, 8, 1, GETUTCDATE(), GETUTCDATE());
        END
        
        IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'DEXA' AND ParentCategoryId = @RadiologyLineId)
        BEGIN
            INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
            VALUES ('DEXA', N'قياس كثافة العظام', NULL, NULL, @RadiologyLineId, 9, 1, GETUTCDATE(), GETUTCDATE());
        END
        
        IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'RADIOGRAPHIC FLUOROSCOPY' AND ParentCategoryId = @RadiologyLineId)
        BEGIN
            INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
            VALUES ('RADIOGRAPHIC FLUOROSCOPY', N'التنظير الشعاعي', NULL, NULL, @RadiologyLineId, 10, 1, GETUTCDATE(), GETUTCDATE());
        END
        
        IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'MAMMOGRAPHY' AND ParentCategoryId = @RadiologyLineId)
        BEGIN
            INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
            VALUES ('MAMMOGRAPHY', N'تصوير الثدي', NULL, NULL, @RadiologyLineId, 11, 1, GETUTCDATE(), GETUTCDATE());
        END
        
        IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Name = 'ULTRASOUND' AND ParentCategoryId = @RadiologyLineId)
        BEGIN
            INSERT INTO ProductCategories (Name, NameAr, Description, DescriptionAr, ParentCategoryId, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
            VALUES ('ULTRASOUND', N'الموجات فوق الصوتية', NULL, NULL, @RadiologyLineId, 12, 1, GETUTCDATE(), GETUTCDATE());
        END
        
        PRINT 'Created Radiology subcategories';
    END
END
ELSE
BEGIN
    PRINT 'Categories exist. Ensuring they are active...';
    
    -- Fix: Ensure main categories are active and have no parent
    UPDATE ProductCategories
    SET IsActive = 1, ParentCategoryId = NULL
    WHERE Name IN ('Radiology Line', 'Physiotherapy Line', 'Dental Line', 'Laboratory Line', 'Dermatology Line', 'LAP Line')
      AND (IsActive = 0 OR ParentCategoryId IS NOT NULL);
    PRINT 'Fixed main categories';
    
    -- Ensure all subcategories under Radiology Line are active
    DECLARE @RadiologyId BIGINT;
    SELECT @RadiologyId = Id FROM ProductCategories WHERE Name = 'Radiology Line' AND ParentCategoryId IS NULL;
    
    IF @RadiologyId IS NOT NULL
    BEGIN
        UPDATE ProductCategories
        SET IsActive = 1
        WHERE ParentCategoryId = @RadiologyId AND IsActive = 0;
        PRINT 'Fixed Radiology subcategories';
    END
END

GO

PRINT '';
PRINT '=== STEP 3: VERIFICATION ===';
PRINT '';

SELECT 
    'Main Active Categories After Fix' AS Status,
    COUNT(*) AS Count
FROM ProductCategories
WHERE IsActive = 1 AND ParentCategoryId IS NULL;
GO

SELECT 
    Id,
    Name,
    IsActive,
    ParentCategoryId,
    '✅ Should appear in API' AS Status
FROM ProductCategories
WHERE IsActive = 1 AND ParentCategoryId IS NULL
ORDER BY DisplayOrder, Name;
GO

PRINT '';
PRINT '=== REVIEW RESULTS ABOVE ===';
PRINT 'If "Main Active Categories After Fix" count > 0, categories should now appear!';
PRINT '';
PRINT 'To commit changes: COMMIT TRANSACTION;';
PRINT 'To rollback: ROLLBACK TRANSACTION;';
PRINT '';

-- Uncomment to commit:
-- COMMIT TRANSACTION;

-- Uncomment to rollback:
-- ROLLBACK TRANSACTION;

