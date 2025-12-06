-- =====================================================
-- SAFE Product Seeding Script
-- Checks for existing products and only adds missing ones
-- No duplicates will be created
-- =====================================================

SET NOCOUNT ON;

PRINT '========================================';
PRINT 'SAFE PRODUCT SEEDING - NO DUPLICATES';
PRINT '========================================';
PRINT '';

-- =====================================================
-- STEP 0: Verify tables exist
-- =====================================================
PRINT 'Step 0: Verifying database tables...';
PRINT '----------------------------------------';

IF OBJECT_ID('dbo.Products', 'U') IS NULL
BEGIN
    PRINT '✗ ERROR: Products table does not exist!';
    PRINT '';
    PRINT 'You need to run migrations first:';
    PRINT '  cd Backend/SoitMed';
    PRINT '  dotnet ef database update';
    PRINT '';
    PRINT 'Aborting script...';
    SET NOCOUNT OFF;
    RETURN;
END

IF OBJECT_ID('dbo.ProductCategories', 'U') IS NULL
BEGIN
    PRINT '✗ ERROR: ProductCategories table does not exist!';
    PRINT '';
    PRINT 'You need to run migrations first:';
    PRINT '  cd Backend/SoitMed';
    PRINT '  dotnet ef database update';
    PRINT '';
    PRINT 'Aborting script...';
    SET NOCOUNT OFF;
    RETURN;
END

PRINT '✓ Products table exists';
PRINT '✓ ProductCategories table exists';
PRINT '';

-- =====================================================
-- STEP 1: Check existing data
-- =====================================================
PRINT 'Step 1: Checking existing data...';
PRINT '----------------------------------------';

DECLARE @ExistingProductCount INT;
SELECT @ExistingProductCount = COUNT(*) FROM dbo.Products WHERE IsActive = 1;
PRINT 'Existing active products: ' + CAST(@ExistingProductCount AS VARCHAR(10));

PRINT '';
PRINT 'Existing categories:';
SELECT 
    Id, 
    Name,
    (SELECT COUNT(*) FROM dbo.Products WHERE CategoryId = pc.Id AND IsActive = 1) AS ProductCount
FROM dbo.ProductCategories pc
WHERE IsActive = 1
ORDER BY DisplayOrder;

PRINT '';

-- =====================================================
-- STEP 2: Get Category IDs
-- =====================================================
DECLARE @RadiologyId INT, @PhysiotherapyId INT, @DermatologyId INT, @LaboratoryId INT;

SELECT @RadiologyId = Id FROM dbo.ProductCategories WHERE Name LIKE '%Radiology%' AND IsActive = 1;
SELECT @PhysiotherapyId = Id FROM dbo.ProductCategories WHERE Name LIKE '%Physiotherapy%' OR Name LIKE '%Physical%' AND IsActive = 1;
SELECT @DermatologyId = Id FROM dbo.ProductCategories WHERE Name LIKE '%Dermatology%' OR Name LIKE '%Derma%' AND IsActive = 1;
SELECT @LaboratoryId = Id FROM dbo.ProductCategories WHERE Name LIKE '%Laboratory%' OR Name LIKE '%Lab%' AND IsActive = 1;

-- If Dermatology doesn't exist, create it
IF @DermatologyId IS NULL
BEGIN
    INSERT INTO dbo.ProductCategories (Name, NameAr, Description, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
    VALUES ('Dermatology', 'طب الجلدية', 'Dermatology and Aesthetic Equipment', 7, 1, GETUTCDATE(), GETUTCDATE());
    SET @DermatologyId = SCOPE_IDENTITY();
    PRINT 'Created Dermatology category';
END

PRINT 'Category IDs:';
PRINT '  Radiology: ' + ISNULL(CAST(@RadiologyId AS VARCHAR(10)), 'NOT FOUND');
PRINT '  Physiotherapy: ' + ISNULL(CAST(@PhysiotherapyId AS VARCHAR(10)), 'NOT FOUND');
PRINT '  Dermatology: ' + ISNULL(CAST(@DermatologyId AS VARCHAR(10)), 'NOT FOUND');
PRINT '  Laboratory: ' + ISNULL(CAST(@LaboratoryId AS VARCHAR(10)), 'NOT FOUND');
PRINT '';

-- =====================================================
-- STEP 3: Create temporary table with all products from PDF
-- =====================================================
PRINT 'Step 2: Preparing products from PDF...';
PRINT '----------------------------------------';

CREATE TABLE #ProductsFromPDF (
    Name NVARCHAR(200),
    Model NVARCHAR(100),
    Provider NVARCHAR(100),
    Category NVARCHAR(100),
    CategoryId INT,
    Description NVARCHAR(2000)
);

-- Insert all products from PDF
INSERT INTO #ProductsFromPDF (Name, Model, Provider, Category, CategoryId, Description)
VALUES
-- RADIOLOGY LINE
('Calypso F X-Ray', 'Calypso F', 'SOITMED', 'Radiology', @RadiologyId, 'Floor/Ceiling Mounted X-Ray System'),
('Calypso Evolution X-Ray', 'Calypso Evolution', 'SOITMED', 'Radiology', @RadiologyId, 'Advanced X-Ray System'),
('Jumong General X-Ray', 'Jumong General', 'SOITMED', 'Radiology', @RadiologyId, 'Floor/Ceiling Mounted General X-Ray System'),
('MAC Mobile X-Ray', 'MAC', 'SOITMED', 'Radiology', @RadiologyId, 'Mobile X-Ray System'),
('TMS Mobile X-Ray', 'TMS', 'SOITMED', 'Radiology', @RadiologyId, 'Mobile X-Ray System'),
('PXP Series Portable X-Ray', 'PXP Series', 'SOITMED', 'Radiology', @RadiologyId, 'Portable X-Ray System'),
('VET-20BT Portable X-Ray', 'VET-20BT', 'SOITMED', 'Radiology', @RadiologyId, 'Portable Veterinary X-Ray System'),
('AiRTouch Portable X-Ray', 'AiRTouch', 'SOITMED', 'Radiology', @RadiologyId, 'Portable X-Ray System with Touch Interface'),
('PSM-PD Portable X-Ray', 'PSM-PD', 'SOITMED', 'Radiology', @RadiologyId, 'Portable X-Ray System'),
('PXM Series Digital Flat Panel', 'PXM Series', 'SOITMED', 'Radiology', @RadiologyId, 'Digital Flat Panel Detector - TOP SELLER'),
('VIVIX-S Wired Panel', 'VIVIX-S Wired', 'SOITMED', 'Radiology', @RadiologyId, 'Wired Digital Flat Panel Detector'),
('VIVIX-S Wireless Panel', 'VIVIX-S Wireless', 'SOITMED', 'Radiology', @RadiologyId, 'Wireless Digital Flat Panel Detector - MARKET LEADER'),
('VIVIX-S Long Panel', 'VIVIX-S Long', 'SOITMED', 'Radiology', @RadiologyId, 'Long Digital Flat Panel Detector'),
('VIVIX-M Mammography Panel', 'VIVIX-M', 'SOITMED', 'Radiology', @RadiologyId, 'Mammography Digital Flat Panel Detector'),
('VIVIX-V Equine Panel', 'VIVIX-V', 'SOITMED', 'Radiology', @RadiologyId, 'Equine/Veterinary Digital Flat Panel Detector'),
('i_Space 1.5T Closed MRI', 'i_Space 1.5T', 'SOITMED', 'Radiology', @RadiologyId, '1.5 Tesla Closed MRI System'),
('i_Field 1.5T Closed MRI', 'i_Field 1.5T', 'SOITMED', 'Radiology', @RadiologyId, '1.5 Tesla Closed MRI System'),
('i_Vision 1.5T Plus Closed MRI', 'i_Vision 1.5T Plus', 'SOITMED', 'Radiology', @RadiologyId, '1.5 Tesla Plus Closed MRI System'),
('i_Open 0.3T Open MRI', 'i_Open 0.3T', 'SOITMED', 'Radiology', @RadiologyId, '0.3 Tesla Open MRI System'),
('i_Open 0.36T Open MRI', 'i_Open 0.36T', 'SOITMED', 'Radiology', @RadiologyId, '0.36 Tesla Open MRI System'),
('i_Open 0.4T Open MRI', 'i_Open 0.4T', 'SOITMED', 'Radiology', @RadiologyId, '0.4 Tesla Open MRI System'),
('TURBOTOM Series CT Scanner', 'TURBOTOM Series', 'SOITMED', 'Radiology', @RadiologyId, 'CT Scanner - TOP SELLER'),
('Fully Automatic Ceiling Suspended Panorama DR', 'Panorama DR', 'SOITMED', 'Radiology', @RadiologyId, 'Fully Automatic Ceiling Suspended Panorama Digital Radiography System'),
('FPD Angiographic & Cardiac System', 'FPD Angio', 'SOITMED', 'Radiology', @RadiologyId, 'Flat Panel Detector Angiographic and Cardiac System'),
('Dynamic FPD RF System', 'Dynamic FPD RF', 'SOITMED', 'Radiology', @RadiologyId, 'Dynamic Flat Panel Detector Radiography/Fluoroscopy System'),
('TCA 6 C-ARM', 'TCA 6', 'SOITMED', 'Radiology', @RadiologyId, 'C-ARM Imaging System - TOP SELLER'),
('TCA 7 C-ARM', 'TCA 7', 'SOITMED', 'Radiology', @RadiologyId, 'Advanced C-ARM Imaging System'),
('Jumong U U-ARM', 'Jumong U', 'SOITMED', 'Radiology', @RadiologyId, 'U-ARM Imaging System'),
('Grand DEXA', 'Grand', 'SOITMED', 'Radiology', @RadiologyId, 'DEXA Bone Densitometry System'),
('Mini DEXA', 'Mini', 'SOITMED', 'Radiology', @RadiologyId, 'Compact DEXA Bone Densitometry System'),
('Grand Pro DEXA', 'Grand Pro', 'SOITMED', 'Radiology', @RadiologyId, 'Professional DEXA Bone Densitometry System'),
('Osteo Pro DEXA', 'Osteo Pro', 'SOITMED', 'Radiology', @RadiologyId, 'Professional Osteoporosis Assessment System'),
('Opera T Radiographic Fluoroscopy', 'Opera T', 'SOITMED', 'Radiology', @RadiologyId, 'Radiographic Fluoroscopy System'),
('Giotto Image Analog Mammography', 'Giotto Image Analog', 'SOITMED', 'Radiology', @RadiologyId, 'Analog Mammography System'),
('Giotto Image 3DL Digital Mammography', 'Giotto Image 3DL', 'SOITMED', 'Radiology', @RadiologyId, 'Digital Mammography System with 3D Capabilities'),
('Tomo Digital Mammography', 'Tomo Digital', 'SOITMED', 'Radiology', @RadiologyId, 'Digital Tomosynthesis Mammography System'),
('Piloter Human Ultrasound', 'Piloter Human', 'SOITMED', 'Radiology', @RadiologyId, 'Human Ultrasound System'),
('Navi Human Ultrasound', 'Navi Human', 'SOITMED', 'Radiology', @RadiologyId, 'Human Ultrasound System with Navigation'),
('Clover Human Ultrasound', 'Clover Human', 'SOITMED', 'Radiology', @RadiologyId, 'Human Ultrasound System'),
('Piloter Vet Ultrasound', 'Piloter Vet', 'SOITMED', 'Radiology', @RadiologyId, 'Veterinary Ultrasound System'),
('Navi Vet Ultrasound', 'Navi Vet', 'SOITMED', 'Radiology', @RadiologyId, 'Veterinary Ultrasound System with Navigation'),
('Clover Vet Ultrasound', 'Clover Vet', 'SOITMED', 'Radiology', @RadiologyId, 'Veterinary Ultrasound System'),

-- PHYSIOTHERAPY LINE
('STARTECAR Physiotherapy', 'STARTECAR', 'SOITMED', 'Physiotherapy', @PhysiotherapyId, 'Physiotherapy Equipment'),
('LYMPHACTIVE Physiotherapy', 'LYMPHACTIVE', 'SOITMED', 'Physiotherapy', @PhysiotherapyId, 'Lymphatic Drainage Therapy System'),
('TOWER Physiotherapy', 'TOWER', 'SOITMED', 'Physiotherapy', @PhysiotherapyId, 'Physiotherapy Tower System'),
('R2 OPERA DESIGN Physiotherapy', 'R2 OPERA DESIGN', 'SOITMED', 'Physiotherapy', @PhysiotherapyId, 'Opera Design Physiotherapy System'),
('MASTER Physiotherapy', 'MASTER', 'SOITMED', 'Physiotherapy', @PhysiotherapyId, 'Master Physiotherapy System'),
('HC TRC Physiotherapy', 'HC TRC', 'SOITMED', 'Physiotherapy', @PhysiotherapyId, 'HC TRC Physiotherapy System'),
('STARWAVE Physiotherapy', 'STARWAVE', 'SOITMED', 'Physiotherapy', @PhysiotherapyId, 'Shockwave Therapy System'),
('CRYOSTAR Physiotherapy', 'CRYOSTAR', 'SOITMED', 'Physiotherapy', @PhysiotherapyId, 'Cryotherapy System'),
('CRYPTSHOCK Physiotherapy', 'CRYPTSHOCK', 'SOITMED', 'Physiotherapy', @PhysiotherapyId, 'Cryotherapy and Shockwave Combined System'),

-- DERMATOLOGY LINE
('Q Switch Laser', 'Q Switch', 'SOITMED', 'Dermatology', @DermatologyId, 'Q-Switched Laser for Pigmentation and Tattoo Removal'),
('Diode Laser', 'Diode Laser', 'SOITMED', 'Dermatology', @DermatologyId, 'Diode Laser for Hair Removal'),
('CO2 Fractional Laser', 'CO2 Fractional', 'SOITMED', 'Dermatology', @DermatologyId, 'CO2 Fractional Laser for Skin Resurfacing'),
('Eride Smart Dual Diode Laser', 'Eride Smart Dual', 'SOITMED', 'Dermatology', @DermatologyId, 'Dual Wavelength Diode Laser System'),
('Eride Smart Dual+ Diode Laser', 'Eride Smart Dual+', 'SOITMED', 'Dermatology', @DermatologyId, 'Advanced Dual Wavelength Diode Laser System'),

-- LABORATORY LINE
('CH8500 Chemistry Analyzer', 'CH8500', 'SOITMED', 'Laboratory', @LaboratoryId, '5 Parameters + Open Input Chemistry Analyzer'),
('CH8310 Chemistry Analyzer', 'CH8310', 'SOITMED', 'Laboratory', @LaboratoryId, '3 Parameters + Open Input Chemistry Analyzer'),
('CH8300 Chemistry Analyzer', 'CH8300', 'SOITMED', 'Laboratory', @LaboratoryId, '3 Parameters + Open Input Chemistry Analyzer'),
('CH8600CRP Chemistry Analyzer', 'CH8600CRP', 'SOITMED', 'Laboratory', @LaboratoryId, '5 Parameters + CRP + Autoloader Chemistry Analyzer'),
('CH8600 Chemistry Analyzer', 'CH8600', 'SOITMED', 'Laboratory', @LaboratoryId, '5 Parameters + Autoloader Chemistry Analyzer'),
('CP880 Autoloader', 'CP880', 'SOITMED', 'Laboratory', @LaboratoryId, 'Autoloader System for Chemistry Analyzers'),
('CH8300CRP Chemistry Analyzer', 'CH8300CRP', 'SOITMED', 'Laboratory', @LaboratoryId, '3 Parameters + CRP + Open Input Chemistry Analyzer'),
('CP700 Open Input', 'CP700', 'SOITMED', 'Laboratory', @LaboratoryId, 'Open Input System for Chemistry Analyzers');

DECLARE @PDFProductCount INT;
SELECT @PDFProductCount = COUNT(*) FROM #ProductsFromPDF;
PRINT 'Products from PDF: ' + CAST(@PDFProductCount AS VARCHAR(10));
PRINT '';

-- =====================================================
-- STEP 4: Find missing products (in PDF but not in DB)
-- =====================================================
PRINT 'Step 3: Finding missing products...';
PRINT '----------------------------------------';

SELECT pdf.Name, pdf.Model, pdf.Category
INTO #MissingProducts
FROM #ProductsFromPDF pdf
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Products p 
    WHERE p.Name = pdf.Name 
    AND p.IsActive = 1
);

DECLARE @MissingCount INT;
SELECT @MissingCount = COUNT(*) FROM #MissingProducts;
PRINT 'Missing products to add: ' + CAST(@MissingCount AS VARCHAR(10));

IF @MissingCount > 0
BEGIN
    PRINT '';
    PRINT 'Missing products:';
    SELECT Name, Model, Category FROM #MissingProducts ORDER BY Category, Name;
END

PRINT '';

-- =====================================================
-- STEP 5: Insert only missing products
-- =====================================================
PRINT 'Step 4: Adding missing products...';
PRINT '----------------------------------------';

IF @MissingCount > 0
BEGIN
    INSERT INTO dbo.Products (Name, Model, Provider, Country, Category, CategoryId, BasePrice, Description, Year, InStock, IsActive, CreatedAt, UpdatedAt)
    SELECT 
        pdf.Name,
        pdf.Model,
        pdf.Provider,
        NULL, -- Country
        pdf.Category,
        pdf.CategoryId,
        0.00, -- BasePrice
        pdf.Description,
        NULL, -- Year
        1, -- InStock
        1, -- IsActive
        GETUTCDATE(),
        GETUTCDATE()
    FROM #ProductsFromPDF pdf
    INNER JOIN #MissingProducts mp ON pdf.Name = mp.Name;
    
    PRINT '✓ Successfully added ' + CAST(@MissingCount AS VARCHAR(10)) + ' new products!';
END
ELSE
BEGIN
    PRINT '✓ No new products to add - all products already exist!';
END

PRINT '';

-- =====================================================
-- STEP 6: Summary
-- =====================================================
PRINT 'Step 5: Final Summary';
PRINT '========================================';

DECLARE @FinalProductCount INT;
SELECT @FinalProductCount = COUNT(*) FROM dbo.Products WHERE IsActive = 1;

PRINT 'Products before: ' + CAST(@ExistingProductCount AS VARCHAR(10));
PRINT 'Products added: ' + CAST(@MissingCount AS VARCHAR(10));
PRINT 'Products after: ' + CAST(@FinalProductCount AS VARCHAR(10));
PRINT '';

PRINT 'Products by category:';
SELECT 
    pc.Name AS Category,
    COUNT(p.Id) AS ProductCount
FROM dbo.ProductCategories pc
LEFT JOIN dbo.Products p ON pc.Id = p.CategoryId AND p.IsActive = 1
WHERE pc.IsActive = 1
GROUP BY pc.Name, pc.DisplayOrder
ORDER BY pc.DisplayOrder;

PRINT '';
PRINT '========================================';
PRINT '✓ SAFE SEEDING COMPLETE - NO DUPLICATES!';
PRINT '========================================';

-- Cleanup
DROP TABLE #ProductsFromPDF;
DROP TABLE #MissingProducts;

SET NOCOUNT OFF;

