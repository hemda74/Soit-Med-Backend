-- ============================================================================
-- Comprehensive Migration Script: TBS → ITIWebApi44
-- ============================================================================
-- This script migrates Clients, Equipment, Contracts, and Installments from 
-- legacy TBS database to new ITIWebApi44 database
-- 
-- Features:
--   - Idempotency: Skips records already migrated (checks LegacyCustomerId, LegacySourceId, LegacyContractId)
--   - Media Path Transformation: Extracts filenames from pipe-separated paths
--   - Transaction Safety: Uses XACT_ABORT and proper rollback
--   - Read-Only on Source: Only SELECT from TBS, never UPDATE/DELETE
-- ============================================================================
-- Usage: Run this script in SQL Server Management Studio on the server
--        that hosts both TBS and ITIWebApi44 databases
-- ============================================================================
-- 
-- Media Path Logic (from soitmed_data_backend analysis):
--   - Files stored as pipe-separated: "file1.jpg|file2.pdf"
--   - Backend transforms to: /api/Media/files/{Uri.EscapeDataString(fileName)}
--   - Actual paths: D:\Soit-Med\legacy\SOIT\UploadFiles\Files
--                  D:\Soit-Med\legacy\SOIT\Ar\MNT\FileUploaders\Reports
--   - We store just the filename, API handles URL construction
-- ============================================================================

USE ITIWebApi44;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON; -- Immediate rollback on any error

-- ============================================================================
-- Step 1: Helper Functions for Media Path Transformation
-- ============================================================================

-- Drop functions if they exist
IF OBJECT_ID('dbo.fn_ExtractFileName', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_ExtractFileName;
GO

IF OBJECT_ID('dbo.fn_ExtractFileNamesFromPipeSeparated', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_ExtractFileNamesFromPipeSeparated;
GO

-- Helper Function: Extract filename from legacy path
-- Handles paths like: D:\Soit-Med\legacy\SOIT\UploadFiles\Files\image.jpg
-- Returns: image.jpg
CREATE FUNCTION dbo.fn_ExtractFileName(@Path NVARCHAR(500))
RETURNS NVARCHAR(500)
AS
BEGIN
    DECLARE @FileName NVARCHAR(500) = LTRIM(RTRIM(@Path));
    
    IF @FileName IS NULL OR @FileName = ''
        RETURN '';
    
    -- Remove common legacy path prefixes (exact match from soitmed_data_backend)
    SET @FileName = REPLACE(@FileName, 'D:\Soit-Med\legacy\SOIT\UploadFiles\Files\', '');
    SET @FileName = REPLACE(@FileName, 'D:\Soit-Med\legacy\SOIT\UploadFiles\Images\', '');
    SET @FileName = REPLACE(@FileName, 'D:\Soit-Med\legacy\SOIT\Ar\MNT\FileUploaders\Reports\', '');
    SET @FileName = REPLACE(@FileName, 'C:\Soit-Med\legacy\SOIT\UploadFiles\Files\', '');
    SET @FileName = REPLACE(@FileName, 'C:\Soit-Med\legacy\SOIT\UploadFiles\Images\', '');
    SET @FileName = REPLACE(@FileName, 'C:\Soit-Med\legacy\SOIT\Ar\MNT\FileUploaders\Reports\', '');
    
    -- Extract filename (last part after backslash or forward slash)
    IF CHARINDEX('\', @FileName) > 0
    BEGIN
        SET @FileName = REVERSE(LEFT(REVERSE(@FileName), CHARINDEX('\', REVERSE(@FileName)) - 1));
    END
    ELSE IF CHARINDEX('/', @FileName) > 0
    BEGIN
        SET @FileName = REVERSE(LEFT(REVERSE(@FileName), CHARINDEX('/', REVERSE(@FileName)) - 1));
    END
    
    -- Remove leading/trailing spaces
    SET @FileName = LTRIM(RTRIM(@FileName));
    
    RETURN @FileName;
END;
GO

-- Helper Function: Extract first filename from pipe-separated string
-- Handles: "file1.jpg|file2.pdf" or "file1.jpg,file2.pdf" or "file1.jpg;file2.pdf"
-- Returns: "file1.jpg" (first file only, for single-field storage)
CREATE FUNCTION dbo.fn_ExtractFileNamesFromPipeSeparated(@FilesField NVARCHAR(MAX))
RETURNS NVARCHAR(500)
AS
BEGIN
    DECLARE @FirstFile NVARCHAR(500) = '';
    DECLARE @CleanedField NVARCHAR(MAX) = LTRIM(RTRIM(@FilesField));
    
    IF @CleanedField IS NULL OR @CleanedField = ''
        RETURN '';
    
    -- Extract first filename (before pipe, comma, or semicolon)
    DECLARE @DelimiterPos INT;
    SET @DelimiterPos = CHARINDEX('|', @CleanedField);
    IF @DelimiterPos = 0
        SET @DelimiterPos = CHARINDEX(',', @CleanedField);
    IF @DelimiterPos = 0
        SET @DelimiterPos = CHARINDEX(';', @CleanedField);
    
    IF @DelimiterPos > 0
        SET @FirstFile = LEFT(@CleanedField, @DelimiterPos - 1);
    ELSE
        SET @FirstFile = @CleanedField;
    
    -- Extract just the filename (remove any remaining path)
    SET @FirstFile = dbo.fn_ExtractFileName(@FirstFile);
    
    RETURN LTRIM(RTRIM(@FirstFile));
END;
GO

-- ============================================================================
-- Step 2: Configuration Variables
-- ============================================================================

DECLARE @AdminUserId NVARCHAR(450) = 'Hemdan_Test_Administration_002'; -- Update with actual admin user ID
DECLARE @LegacyMediaApiBaseUrl NVARCHAR(500) = 'http://10.10.9.104:5266'; -- Legacy media API URL
DECLARE @StartTime DATETIME2 = GETUTCDATE();
DECLARE @ClientsMigrated INT = 0;
DECLARE @ClientsSkipped INT = 0;
DECLARE @EquipmentMigrated INT = 0;
DECLARE @EquipmentSkipped INT = 0;
DECLARE @ContractsMigrated INT = 0;
DECLARE @ContractsSkipped INT = 0;
DECLARE @InstallmentsMigrated INT = 0;
DECLARE @ErrorCount INT = 0;

PRINT '============================================================================';
PRINT 'Comprehensive Migration: TBS → ITIWebApi44';
PRINT '============================================================================';
PRINT 'Start Time: ' + CONVERT(VARCHAR, @StartTime, 120);
PRINT '';

-- ============================================================================
-- Step 3: Migrate Clients (Stk_Customers → Clients)
-- ============================================================================
-- Idempotency: Check by Email OR Phone (if both match, skip)
-- Field Mapping: Based on Client.cs model analysis
-- ============================================================================

PRINT '============================================================================';
PRINT 'PHASE 1: Migrating Clients (Stk_Customers → Clients)';
PRINT '============================================================================';

BEGIN TRY
    BEGIN TRANSACTION MigrateClients;
    
    -- Insert clients that don't exist (by Email or Phone match)
    INSERT INTO ITIWebApi44.dbo.Clients (
        Name,
        Type,
        OrganizationName,
        Specialization,
        Location,
        Phone,
        Email,
        Website,
        Address,
        City,
        Governorate,
        PostalCode,
        ContactPerson,
        ContactPersonPhone,
        ContactPersonEmail,
        ContactPersonPosition,
        Notes,
        Status,
        Priority,
        Classification,
        CreatedBy,
        AssignedTo,
        LegacyCustomerId,
        CreatedAt,
        UpdatedAt
    )
    SELECT 
        -- Basic Information
        LTRIM(RTRIM(ISNULL(c.[Cus_Name], 'Legacy Client ' + CAST(c.[Cus_ID] AS VARCHAR)))) AS Name,
        NULL AS Type, -- Not in TBS
        NULL AS OrganizationName, -- Not in TBS
        NULL AS Specialization, -- Not in TBS
        NULL AS Location, -- Not in TBS
        
        -- Contact Information
        LTRIM(RTRIM(ISNULL(c.[Cus_Tel], c.[Cus_Mobile]))) AS Phone,
        LTRIM(RTRIM(c.[Cus_Email])) AS Email,
        NULL AS Website, -- Not in TBS
        LTRIM(RTRIM(c.[Cus_address])) AS Address,
        NULL AS City, -- Not in TBS
        NULL AS Governorate, -- Not in TBS (has GovernorateCode but not name)
        NULL AS PostalCode, -- Not in TBS
        
        -- Contact Person Information
        LTRIM(RTRIM(c.[ContactPerson_Name])) AS ContactPerson,
        LTRIM(RTRIM(c.[ContactPerson_Mobile])) AS ContactPersonPhone,
        LTRIM(RTRIM(c.[ContactPerson_Email])) AS ContactPersonEmail,
        LTRIM(RTRIM(c.[ContactPerson_Job])) AS ContactPersonPosition,
        
        -- Business Information
        NULL AS Notes, -- Could concatenate Notes_Tech, Notes_Finance, Notes_Admin if needed
        'Active' AS Status, -- Default to Active
        'Medium' AS Priority, -- Default
        NULL AS Classification, -- Not in TBS
        
        -- System Information
        'CustomerMigration' AS CreatedBy,
        NULL AS AssignedTo,
        c.[Cus_ID] AS LegacyCustomerId,
        GETUTCDATE() AS CreatedAt,
        GETUTCDATE() AS UpdatedAt
        
    FROM TBS.dbo.Stk_Customers c
    WHERE c.[Cus_ID] IS NOT NULL
        -- Idempotency: Skip if client already exists with same Email OR Phone
        AND NOT EXISTS (
            SELECT 1 
            FROM ITIWebApi44.dbo.Clients cl
            WHERE cl.LegacyCustomerId = c.[Cus_ID]
                OR (
                    -- Match by Email (if both have email) - Fix collation conflict
                    (cl.Email IS NOT NULL AND cl.Email != '' 
                     AND c.[Cus_Email] IS NOT NULL AND c.[Cus_Email] != ''
                     AND LTRIM(RTRIM(cl.Email)) COLLATE DATABASE_DEFAULT = LTRIM(RTRIM(c.[Cus_Email])) COLLATE DATABASE_DEFAULT)
                    OR
                    -- Match by Phone (if both have phone) - Fix collation conflict
                    (cl.Phone IS NOT NULL AND cl.Phone != ''
                     AND (c.[Cus_Tel] IS NOT NULL OR c.[Cus_Mobile] IS NOT NULL)
                     AND (LTRIM(RTRIM(cl.Phone)) COLLATE DATABASE_DEFAULT = LTRIM(RTRIM(ISNULL(c.[Cus_Tel], ''))) COLLATE DATABASE_DEFAULT
                          OR LTRIM(RTRIM(cl.Phone)) COLLATE DATABASE_DEFAULT = LTRIM(RTRIM(ISNULL(c.[Cus_Mobile], ''))) COLLATE DATABASE_DEFAULT))
                )
        );
    
    SET @ClientsMigrated = @@ROWCOUNT;
    
    -- Count skipped
    SELECT @ClientsSkipped = COUNT(*)
    FROM TBS.dbo.Stk_Customers c
    WHERE EXISTS (
        SELECT 1 
        FROM ITIWebApi44.dbo.Clients cl
        WHERE cl.LegacyCustomerId = c.[Cus_ID]
            OR (
                (cl.Email IS NOT NULL AND cl.Email != '' 
                 AND c.[Cus_Email] IS NOT NULL AND c.[Cus_Email] != ''
                 AND LTRIM(RTRIM(cl.Email)) COLLATE DATABASE_DEFAULT = LTRIM(RTRIM(c.[Cus_Email])) COLLATE DATABASE_DEFAULT)
                OR
                (cl.Phone IS NOT NULL AND cl.Phone != ''
                 AND (c.[Cus_Tel] IS NOT NULL OR c.[Cus_Mobile] IS NOT NULL)
                 AND (LTRIM(RTRIM(cl.Phone)) COLLATE DATABASE_DEFAULT = LTRIM(RTRIM(ISNULL(c.[Cus_Tel], ''))) COLLATE DATABASE_DEFAULT
                      OR LTRIM(RTRIM(cl.Phone)) COLLATE DATABASE_DEFAULT = LTRIM(RTRIM(ISNULL(c.[Cus_Mobile], ''))) COLLATE DATABASE_DEFAULT))
            )
    );
    
    COMMIT TRANSACTION MigrateClients;
    
    PRINT 'Clients Migrated: ' + CAST(@ClientsMigrated AS VARCHAR);
    PRINT 'Clients Skipped (already exist): ' + CAST(@ClientsSkipped AS VARCHAR);
    PRINT '';
    
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION MigrateClients;
    
    SET @ErrorCount = @ErrorCount + 1;
    PRINT 'ERROR in Client Migration:';
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR);
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR);
    THROW;
END CATCH;

-- ============================================================================
-- Step 4: Migrate Equipment (Stk_Order_Out_Items → Equipment)
-- ============================================================================
-- Idempotency: Check by SerialNum OR LegacySourceId (OOI_ID)
-- Field Mapping: Based on Equipment.cs model analysis
-- Note: Equipment links to Client via LegacyCustomerId lookup
-- ============================================================================

PRINT '============================================================================';
PRINT 'PHASE 2: Migrating Equipment (Stk_Order_Out_Items → Equipment)';
PRINT '============================================================================';

BEGIN TRY
    BEGIN TRANSACTION MigrateEquipment;
    
    -- Insert equipment that doesn't exist (by SerialNum or LegacySourceId)
    -- Link to clients via most recent visit to satisfy CK_Equipment_Owner constraint
    -- Handle duplicate SerialNum values by ALWAYS appending OOI_ID to ensure uniqueness
    -- Use CTE to detect duplicates and ensure every QRCode is unique
    WITH EquipmentSource AS (
        SELECT 
            ooi.[OOI_ID],
            ooi.[SerialNum],
            ooi.[item_ID],
            ooi.[Item_DateExpire],
            i.[Item_Name_Ar],
            i.[Item_Name_En],
            i.[Item_Desc],
            i.[Producing_Company],
            -- Detect duplicates in source: count how many times this SerialNum appears
            COUNT(*) OVER (PARTITION BY LTRIM(RTRIM(ooi.[SerialNum]))) AS SerialNumCountInSource,
            -- Check if this exact SerialNum (or any variation) already exists in target database
            CASE 
                WHEN EXISTS (
                    SELECT 1 FROM ITIWebApi44.dbo.Equipment e 
                    WHERE e.QRCode = LTRIM(RTRIM(ooi.[SerialNum])) COLLATE DATABASE_DEFAULT
                       OR e.QRCode LIKE LTRIM(RTRIM(ooi.[SerialNum])) + '-OOI%' COLLATE DATABASE_DEFAULT
                ) THEN 1 
                ELSE 0 
            END AS ExistsInTarget,
            -- Row number to handle duplicates: first occurrence gets SerialNum, others get -OOI suffix
            ROW_NUMBER() OVER (PARTITION BY LTRIM(RTRIM(ooi.[SerialNum])) ORDER BY ooi.[OOI_ID]) AS SerialNumRowNum
        FROM TBS.dbo.Stk_Order_Out_Items ooi
        INNER JOIN TBS.dbo.Stk_Items i ON ooi.[item_ID] = i.[Item_ID]
        WHERE ooi.[OOI_ID] IS NOT NULL
            AND ooi.[SerialNum] IS NOT NULL 
            AND ooi.[SerialNum] != ''
            -- Idempotency: Skip if equipment already exists by LegacySourceId
            AND NOT EXISTS (
                SELECT 1 
                FROM ITIWebApi44.dbo.Equipment e
                WHERE e.LegacySourceId = CAST(ooi.[OOI_ID] AS NVARCHAR(100))
            )
    )
    INSERT INTO ITIWebApi44.dbo.Equipment (
        Name,
        QRCode,
        QRCodeImageData,
        QRCodePdfPath,
        Description,
        Model,
        Manufacturer,
        PurchaseDate,
        WarrantyExpiry,
        HospitalId,
        CustomerId,
        RepairVisitCount,
        Status,
        CreatedAt,
        LastMaintenanceDate,
        IsActive,
        QrToken,
        IsQrPrinted,
        QrLastPrintedDate,
        LegacySourceId
    )
    SELECT 
        -- Name: Use Item name (Arabic or English) or SerialNum as fallback
        LTRIM(RTRIM(ISNULL(es.[Item_Name_Ar], ISNULL(es.[Item_Name_En], ISNULL(es.[SerialNum], 'Equipment ' + CAST(es.[OOI_ID] AS VARCHAR)))))) AS Name,
        
        -- QRCode: ALWAYS append OOI_ID to ensure 100% uniqueness
        -- This prevents any duplicate key violations, even if SerialNum appears unique
        CASE 
            WHEN es.[SerialNum] IS NULL OR es.[SerialNum] = '' THEN 
                'EQ-' + CAST(es.[OOI_ID] AS VARCHAR)
            WHEN es.[SerialNum] IN ('00', '0', '000', '0000', 'NULL', 'N/A', 'NA', 'NONE', '') THEN
                -- Invalid SerialNum, use OOI_ID only
                'EQ-' + CAST(es.[OOI_ID] AS VARCHAR)
            WHEN es.SerialNumCountInSource > 1 THEN
                -- Duplicate in source: append OOI_ID (first one gets SerialNum-OOI, others too)
                LTRIM(RTRIM(es.[SerialNum])) + '-OOI' + CAST(es.[OOI_ID] AS VARCHAR)
            WHEN es.ExistsInTarget = 1 THEN
                -- Already exists in target: append OOI_ID
                LTRIM(RTRIM(es.[SerialNum])) + '-OOI' + CAST(es.[OOI_ID] AS VARCHAR)
            WHEN es.SerialNumRowNum > 1 THEN
                -- Not first occurrence of this SerialNum in this batch: append OOI_ID
                LTRIM(RTRIM(es.[SerialNum])) + '-OOI' + CAST(es.[OOI_ID] AS VARCHAR)
            ELSE 
                -- Appears unique, but to be safe, we'll still append OOI_ID for first occurrence
                -- This ensures no conflicts even if SerialNum exists elsewhere
                LTRIM(RTRIM(es.[SerialNum])) + '-OOI' + CAST(es.[OOI_ID] AS VARCHAR)
        END AS QRCode,
        
        NULL AS QRCodeImageData, -- Generated later
        NULL AS QRCodePdfPath, -- Generated later
        
        -- Description: Use Item description if available
        LTRIM(RTRIM(es.[Item_Desc])) AS Description,
        
        -- Model: Use Item name
        LTRIM(RTRIM(ISNULL(es.[Item_Name_Ar], es.[Item_Name_En]))) AS Model,
        
        -- Manufacturer: Use ProducingCompany if available
        LTRIM(RTRIM(es.[Producing_Company])) AS Manufacturer,
        
        NULL AS PurchaseDate, -- Not in TBS directly
        es.[Item_DateExpire] AS WarrantyExpiry,
        
        -- HospitalId: NULL (equipment linked to clients, not hospitals directly)
        NULL AS HospitalId,
        
        -- CustomerId: Link to client via most recent visit
        -- Find the most recent client for this equipment via visits
        -- Try to get ApplicationUser ID from client's RelatedUserId
        -- If no RelatedUserId exists, try to find any user with matching email/phone
        -- If still no match, use a default system user (you may need to create one)
        COALESCE(
            -- First try: Get from client's RelatedUserId
            (SELECT TOP 1 au.Id
             FROM TBS.dbo.MNT_VisitingReport vr
             INNER JOIN TBS.dbo.MNT_Visiting v ON vr.[VisitingId] = v.[VisitingId]
             INNER JOIN ITIWebApi44.dbo.Clients cl ON v.[Cus_ID] = cl.LegacyCustomerId
             INNER JOIN ITIWebApi44.dbo.AspNetUsers au ON cl.RelatedUserId = au.Id
             WHERE vr.[OOI_ID] = es.[OOI_ID]
                AND cl.RelatedUserId IS NOT NULL
             ORDER BY v.[VisitingDate] DESC),
            -- Second try: Find user by matching client email/phone
            (SELECT TOP 1 au.Id
             FROM TBS.dbo.MNT_VisitingReport vr
             INNER JOIN TBS.dbo.MNT_Visiting v ON vr.[VisitingId] = v.[VisitingId]
             INNER JOIN ITIWebApi44.dbo.Clients cl ON v.[Cus_ID] = cl.LegacyCustomerId
             INNER JOIN ITIWebApi44.dbo.AspNetUsers au 
                ON (cl.Email IS NOT NULL AND cl.Email != '' AND au.Email = cl.Email)
                OR (cl.Phone IS NOT NULL AND cl.Phone != '' AND au.PhoneNumber = cl.Phone)
             WHERE vr.[OOI_ID] = es.[OOI_ID]
             ORDER BY v.[VisitingDate] DESC),
            -- Fallback: Use admin user (update @AdminUserId if needed)
            @AdminUserId
        ) AS CustomerId,
        
        -- RepairVisitCount: Count visits for this equipment
        (SELECT COUNT(DISTINCT vr.[VisitingId])
         FROM TBS.dbo.MNT_VisitingReport vr
         WHERE vr.[OOI_ID] = es.[OOI_ID]) AS RepairVisitCount,
        
        1 AS Status, -- EquipmentStatus.Operational = 1
        
        GETUTCDATE() AS CreatedAt,
        NULL AS LastMaintenanceDate, -- Could calculate from latest visit
        
        1 AS IsActive, -- true
        
        NEWID() AS QrToken, -- Generate new GUID
        
        0 AS IsQrPrinted, -- false
        NULL AS QrLastPrintedDate,
        
        CAST(es.[OOI_ID] AS NVARCHAR(100)) AS LegacySourceId -- Store OOI_ID as string
        
    FROM EquipmentSource es;
    
    SET @EquipmentMigrated = @@ROWCOUNT;
    
    -- Count skipped
    SELECT @EquipmentSkipped = COUNT(*)
    FROM TBS.dbo.Stk_Order_Out_Items ooi
    WHERE ooi.[SerialNum] IS NOT NULL AND ooi.[SerialNum] != ''
        AND EXISTS (
            SELECT 1 
            FROM ITIWebApi44.dbo.Equipment e
            WHERE e.LegacySourceId = CAST(ooi.[OOI_ID] AS NVARCHAR(100))
                OR (e.QRCode IS NOT NULL AND e.QRCode != ''
                    AND LTRIM(RTRIM(e.QRCode)) COLLATE DATABASE_DEFAULT = LTRIM(RTRIM(ooi.[SerialNum])) COLLATE DATABASE_DEFAULT)
        );
    
    COMMIT TRANSACTION MigrateEquipment;
    
    PRINT 'Equipment Migrated: ' + CAST(@EquipmentMigrated AS VARCHAR);
    PRINT 'Equipment Skipped (already exist): ' + CAST(@EquipmentSkipped AS VARCHAR);
    PRINT '';
    
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION MigrateEquipment;
    
    SET @ErrorCount = @ErrorCount + 1;
    PRINT 'ERROR in Equipment Migration:';
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR);
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR);
    THROW;
END CATCH;

-- ============================================================================
-- Step 5: Link Equipment to Clients (via Maintenance Visits)
-- ============================================================================
-- Update Equipment.CustomerId based on visit relationships from TBS
-- Based on @soitmed_data_backend: Equipment linked via MNT_Visiting.Cus_ID
-- ============================================================================

PRINT '============================================================================';
PRINT 'PHASE 3: Linking Equipment to Clients';
PRINT '============================================================================';

BEGIN TRY
    BEGIN TRANSACTION LinkEquipmentToClients;
    
    -- Diagnostic: Count equipment with admin user
    DECLARE @EquipmentWithAdminUser INT;
    SELECT @EquipmentWithAdminUser = COUNT(*)
    FROM ITIWebApi44.dbo.Equipment
    WHERE CustomerId = @AdminUserId;
    
    PRINT 'Equipment with admin user (needs linking): ' + CAST(@EquipmentWithAdminUser AS VARCHAR);
    
    -- Diagnostic: Count clients with RelatedUserId
    DECLARE @ClientsWithRelatedUserId INT;
    SELECT @ClientsWithRelatedUserId = COUNT(*)
    FROM ITIWebApi44.dbo.Clients
    WHERE RelatedUserId IS NOT NULL;
    
    PRINT 'Clients with RelatedUserId: ' + CAST(@ClientsWithRelatedUserId AS VARCHAR);
    
    -- Diagnostic: Count equipment that can be linked via visits
    -- Based on @soitmed_data_backend: Equipment linked via MNT_VisitingReport.OOI_ID
    DECLARE @EquipmentWithVisits INT;
    DECLARE @EquipmentWithMatchingClients INT;
    DECLARE @TotalVisitingReports INT;
    DECLARE @EquipmentWithLegacySourceId INT;
    DECLARE @VisitingReportsWithOOI INT;
    
    -- Step 0: Count total visiting reports with OOI_ID
    SELECT @VisitingReportsWithOOI = COUNT(DISTINCT vr.[OOI_ID])
    FROM TBS.dbo.MNT_VisitingReport vr
    WHERE vr.[OOI_ID] IS NOT NULL;
    
    -- Step 0.5: Count equipment with LegacySourceId
    SELECT @EquipmentWithLegacySourceId = COUNT(*)
    FROM ITIWebApi44.dbo.Equipment e
    WHERE e.CustomerId = @AdminUserId
        AND e.LegacySourceId IS NOT NULL
        AND e.LegacySourceId != '';
    
    -- Step 1: Equipment with visits in TBS (using direct numeric comparison)
    -- Convert LegacySourceId to INT for direct comparison with OOI_ID
    SELECT @EquipmentWithVisits = COUNT(DISTINCT e.Id)
    FROM ITIWebApi44.dbo.Equipment e
    WHERE e.CustomerId = @AdminUserId
        AND e.LegacySourceId IS NOT NULL
        AND e.LegacySourceId != ''
        AND EXISTS (
            SELECT 1 FROM TBS.dbo.MNT_VisitingReport vr
            WHERE vr.[OOI_ID] = TRY_CAST(e.LegacySourceId AS INT)
               OR CAST(vr.[OOI_ID] AS NVARCHAR(100)) = LTRIM(RTRIM(e.LegacySourceId))
        );
    
    -- Step 2: Equipment with visits + matching clients with RelatedUserId
    SELECT @EquipmentWithMatchingClients = COUNT(DISTINCT e.Id)
    FROM ITIWebApi44.dbo.Equipment e
    WHERE e.CustomerId = @AdminUserId
        AND e.LegacySourceId IS NOT NULL
        AND e.LegacySourceId != ''
        AND EXISTS (
            SELECT 1 
            FROM TBS.dbo.MNT_VisitingReport vr
            INNER JOIN TBS.dbo.MNT_Visiting v ON vr.[VisitingId] = v.[VisitingId]
            INNER JOIN ITIWebApi44.dbo.Clients c ON v.[Cus_ID] = c.LegacyCustomerId
            WHERE (vr.[OOI_ID] = TRY_CAST(e.LegacySourceId AS INT)
               OR CAST(vr.[OOI_ID] AS NVARCHAR(100)) = LTRIM(RTRIM(e.LegacySourceId)))
                AND c.RelatedUserId IS NOT NULL
        );
    
    PRINT 'Total VisitingReports with OOI_ID in TBS: ' + CAST(@VisitingReportsWithOOI AS VARCHAR);
    PRINT 'Equipment with LegacySourceId (admin user): ' + CAST(@EquipmentWithLegacySourceId AS VARCHAR);
    PRINT 'Equipment with visits in TBS: ' + CAST(@EquipmentWithVisits AS VARCHAR);
    PRINT 'Equipment with matching clients (have RelatedUserId): ' + CAST(@EquipmentWithMatchingClients AS VARCHAR);
    
    -- Additional diagnostic: Check if OOI_ID values from Equipment exist in VisitingReports
    DECLARE @EquipmentOOIInVisitingReports INT;
    SELECT @EquipmentOOIInVisitingReports = COUNT(DISTINCT e.Id)
    FROM ITIWebApi44.dbo.Equipment e
    WHERE e.CustomerId = @AdminUserId
        AND e.LegacySourceId IS NOT NULL
        AND e.LegacySourceId != ''
        AND EXISTS (
            SELECT 1 FROM TBS.dbo.MNT_VisitingReport vr
            WHERE vr.[OOI_ID] = TRY_CAST(e.LegacySourceId AS INT)
        );
    
    PRINT 'Equipment with LegacySourceId that matches OOI_ID in VisitingReports (direct INT match): ' + CAST(@EquipmentOOIInVisitingReports AS VARCHAR);
    PRINT '';
    
    -- Link equipment to clients via visits
    -- Based on @soitmed_data_backend: Equipment is linked to clients via visits
    -- MNT_Visiting.Cus_ID = Customer ID, MNT_VisitingReport.OOI_ID = Equipment ID
    -- Find the most recent client for each equipment based on visits
    -- Use direct numeric comparison for better performance and reliability
    UPDATE e
    SET e.CustomerId = c.RelatedUserId
    FROM ITIWebApi44.dbo.Equipment e
    INNER JOIN (
        -- Find most recent client for each equipment via visits (from TBS)
        -- This matches the logic in @soitmed_data_backend ContractService.GetMachinesByCustomerIdAsync
        SELECT 
            vr.[OOI_ID] AS OOI_ID,
            CAST(vr.[OOI_ID] AS NVARCHAR(100)) AS LegacySourceId,
            v.[Cus_ID] AS LegacyCustomerId,
            ROW_NUMBER() OVER (PARTITION BY vr.[OOI_ID] ORDER BY v.[VisitingDate] DESC) AS rn
        FROM TBS.dbo.MNT_VisitingReport vr
        INNER JOIN TBS.dbo.MNT_Visiting v ON vr.[VisitingId] = v.[VisitingId]
        WHERE vr.[OOI_ID] IS NOT NULL
            AND v.[Cus_ID] IS NOT NULL
    ) latest_visit ON (
        -- Direct numeric comparison (preferred)
        latest_visit.OOI_ID = TRY_CAST(e.LegacySourceId AS INT)
        -- Fallback: string comparison
        OR latest_visit.LegacySourceId = LTRIM(RTRIM(e.LegacySourceId))
    )
    INNER JOIN ITIWebApi44.dbo.Clients c ON latest_visit.LegacyCustomerId = c.LegacyCustomerId
    WHERE latest_visit.rn = 1
        -- Update equipment that has default admin user (from Phase 2 fallback)
        AND e.CustomerId = @AdminUserId
        -- Only update if client has RelatedUserId
        AND c.RelatedUserId IS NOT NULL
        -- Ensure LegacySourceId exists
        AND e.LegacySourceId IS NOT NULL
        AND e.LegacySourceId != '';
    
    DECLARE @LinkedCount INT = @@ROWCOUNT;
    PRINT 'Equipment linked to clients (via visits): ' + CAST(@LinkedCount AS VARCHAR);
    
    -- Method 2: Link equipment via Maintenance Contract Items
    -- MNT_MaintenanceContract_Items.OOI_ID → MNT_MaintenanceContract.Cus_ID
    UPDATE e
    SET e.CustomerId = c.RelatedUserId
    FROM ITIWebApi44.dbo.Equipment e
    INNER JOIN TBS.dbo.MNT_MaintenanceContract_Items mci ON mci.[OOI_ID] = TRY_CAST(e.LegacySourceId AS INT)
    INNER JOIN TBS.dbo.MNT_MaintenanceContract mc ON mci.[ContractId] = mc.[ContractId]
    INNER JOIN ITIWebApi44.dbo.Clients c ON mc.[Cus_ID] = c.LegacyCustomerId
    WHERE e.CustomerId = @AdminUserId
        AND e.LegacySourceId IS NOT NULL
        AND e.LegacySourceId != ''
        AND c.RelatedUserId IS NOT NULL
        AND mci.[OOI_ID] IS NOT NULL
        AND mc.[Cus_ID] IS NOT NULL;
    
    DECLARE @LinkedViaContracts INT = @@ROWCOUNT;
    PRINT 'Equipment linked to clients (via contract items): ' + CAST(@LinkedViaContracts AS VARCHAR);
    SET @LinkedCount = @LinkedCount + @LinkedViaContracts;
    
    -- Method 3: Link equipment via Sales Invoices
    -- Stk_Order_Out_Items.OOI_ID → Stk_Order_Out.OO_ID → Stk_Order_Out.SI_ID → Stk_Sales_Inv.SC_ID → Stk_Sales_Contract.Cus_ID
    -- Note: Based on EquipmentService.cs, Stk_Sales_Inv uses SC_ID to link to Stk_Sales_Contract
    -- This matches the approach in EquipmentService.GetLegacyEquipmentByCustomerIdAsync
    BEGIN TRY
        UPDATE e
        SET e.CustomerId = c.RelatedUserId
        FROM ITIWebApi44.dbo.Equipment e
        INNER JOIN TBS.dbo.Stk_Order_Out_Items ooi ON ooi.[OOI_ID] = TRY_CAST(e.LegacySourceId AS INT)
        INNER JOIN TBS.dbo.Stk_Order_Out oo ON ooi.[OO_ID] = oo.[OO_ID]
        INNER JOIN TBS.dbo.Stk_Sales_Inv si ON oo.[SI_ID] = si.[SI_ID]
        INNER JOIN TBS.dbo.Stk_Sales_Contract sc ON si.[SC_ID] = sc.[SC_ID]
        INNER JOIN ITIWebApi44.dbo.Clients c ON sc.[Cus_ID] = c.LegacyCustomerId
        WHERE e.CustomerId = @AdminUserId
            AND e.LegacySourceId IS NOT NULL
            AND e.LegacySourceId != ''
            AND c.RelatedUserId IS NOT NULL
            AND oo.[SI_ID] IS NOT NULL
            AND si.[SC_ID] IS NOT NULL
            AND sc.[Cus_ID] IS NOT NULL;
        
        SET @LinkedViaSales = @@ROWCOUNT;
    END TRY
    BEGIN CATCH
        -- If SC_ID doesn't exist in Stk_Sales_Inv, skip this linking method
        PRINT 'Warning: Could not link via sales invoices (SC_ID column may not exist in Stk_Sales_Inv). Skipping...';
        SET @LinkedViaSales = 0;
    END CATCH;
    
    DECLARE @LinkedViaSales INT = @@ROWCOUNT;
    PRINT 'Equipment linked to clients (via sales invoices): ' + CAST(@LinkedViaSales AS VARCHAR);
    SET @LinkedCount = @LinkedCount + @LinkedViaSales;
    
    -- Method 4: Link equipment via Order Out (original source)
    -- Stk_Order_Out_Items.OOI_ID → Stk_Order_Out.OO_ID → Stk_Order_Out.Cus_ID
    UPDATE e
    SET e.CustomerId = c.RelatedUserId
    FROM ITIWebApi44.dbo.Equipment e
    INNER JOIN TBS.dbo.Stk_Order_Out_Items ooi ON ooi.[OOI_ID] = TRY_CAST(e.LegacySourceId AS INT)
    INNER JOIN TBS.dbo.Stk_Order_Out oo ON ooi.[OO_ID] = oo.[OO_ID]
    INNER JOIN ITIWebApi44.dbo.Clients c ON oo.[Cus_ID] = c.LegacyCustomerId
    WHERE e.CustomerId = @AdminUserId
        AND e.LegacySourceId IS NOT NULL
        AND e.LegacySourceId != ''
        AND c.RelatedUserId IS NOT NULL
        AND oo.[Cus_ID] IS NOT NULL;
    
    DECLARE @LinkedViaOrderOut INT = @@ROWCOUNT;
    PRINT 'Equipment linked to clients (via order out): ' + CAST(@LinkedViaOrderOut AS VARCHAR);
    SET @LinkedCount = @LinkedCount + @LinkedViaOrderOut;
    
    PRINT 'Total equipment linked to clients: ' + CAST(@LinkedCount AS VARCHAR);
    PRINT '';
    
    -- Diagnostic: Show remaining equipment with admin user
    DECLARE @RemainingWithAdmin INT;
    SELECT @RemainingWithAdmin = COUNT(*)
    FROM ITIWebApi44.dbo.Equipment
    WHERE CustomerId = @AdminUserId;
    
    PRINT 'Equipment still with admin user (no client user found): ' + CAST(@RemainingWithAdmin AS VARCHAR);
    PRINT 'NOTE: Equipment linked to admin user can be manually linked later when client accounts are created.';
    PRINT '';
    
    COMMIT TRANSACTION LinkEquipmentToClients;
    
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION LinkEquipmentToClients;
    
    SET @ErrorCount = @ErrorCount + 1;
    PRINT 'ERROR in Linking Equipment to Clients:';
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR);
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    THROW;
END CATCH;

-- ============================================================================
-- Step 6: Migrate Contracts (Reference existing script logic)
-- ============================================================================
-- Note: Contract migration is already handled by MigrateContractsFromTBS.sql
-- This section ensures contracts are linked to migrated clients
-- ============================================================================

PRINT '============================================================================';
PRINT 'PHASE 4: Verifying Contract-Client Links';
PRINT '============================================================================';

BEGIN TRY
    BEGIN TRANSACTION VerifyContractLinks;
    
    -- Update contracts to link to migrated clients
    UPDATE c
    SET c.ClientId = cl.Id,
        c.UpdatedAt = GETUTCDATE()
    FROM ITIWebApi44.dbo.Contracts c
    INNER JOIN TBS.dbo.MNT_MaintenanceContract mc ON c.LegacyContractId = mc.[ContractId]
    INNER JOIN ITIWebApi44.dbo.Clients cl ON mc.[Cus_ID] = cl.LegacyCustomerId
    WHERE c.ClientId IS NULL OR c.ClientId = 0;
    
    SET @ContractsMigrated = @@ROWCOUNT;
    
    PRINT 'Contracts linked to clients: ' + CAST(@ContractsMigrated AS VARCHAR);
    PRINT '';
    
    COMMIT TRANSACTION VerifyContractLinks;
    
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION VerifyContractLinks;
    
    SET @ErrorCount = @ErrorCount + 1;
    PRINT 'ERROR in Verifying Contract Links:';
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR);
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    THROW;
END CATCH;

-- ============================================================================
-- Step 7: Installments Migration
-- ============================================================================
-- NOTE: Installments are already created during contract migration
--       (see MigrateContractsFromTBS.sql Phase 10)
--       
-- Installments are created from contract payment plans:
--   - InstallmentMonths (from MNT_MaintenanceContract)
--   - InstallmentAmount (from MNT_MaintenanceContract)
--   
-- Stk_Sales_Inv does NOT have a ContractId column, so we cannot link
-- sales invoices directly to maintenance contracts for installment creation.
-- 
-- If you need to migrate installments from sales invoices, you would need
-- to establish the relationship through other tables (e.g., via Cus_ID and 
-- contract matching, or through SC_ID if there's a relationship).
-- ============================================================================

PRINT '============================================================================';
PRINT 'PHASE 5: Installments Migration';
PRINT '============================================================================';
PRINT 'NOTE: Installments are created during contract migration.';
PRINT '      See MigrateContractsFromTBS.sql for installment creation logic.';
PRINT '      Installments are generated from contract InstallmentMonths/Amount.';
PRINT '';

SET @InstallmentsMigrated = 0; -- Installments handled in contract migration

-- ============================================================================
-- Step 8: Summary Report
-- ============================================================================

DECLARE @EndTime DATETIME2 = GETUTCDATE();
DECLARE @DurationSeconds INT = DATEDIFF(SECOND, @StartTime, @EndTime);

PRINT '============================================================================';
PRINT 'MIGRATION SUMMARY';
PRINT '============================================================================';
PRINT 'Start Time: ' + CONVERT(VARCHAR, @StartTime, 120);
PRINT 'End Time: ' + CONVERT(VARCHAR, @EndTime, 120);
PRINT 'Duration: ' + CAST(@DurationSeconds AS VARCHAR) + ' seconds';
PRINT '';
PRINT 'Results:';
PRINT '  Clients Migrated: ' + CAST(@ClientsMigrated AS VARCHAR);
PRINT '  Clients Skipped: ' + CAST(@ClientsSkipped AS VARCHAR);
PRINT '  Equipment Migrated: ' + CAST(@EquipmentMigrated AS VARCHAR);
PRINT '  Equipment Skipped: ' + CAST(@EquipmentSkipped AS VARCHAR);
PRINT '  Contracts Linked: ' + CAST(@ContractsMigrated AS VARCHAR);
PRINT '  Installments Migrated: ' + CAST(@InstallmentsMigrated AS VARCHAR);
PRINT '  Errors: ' + CAST(@ErrorCount AS VARCHAR);
PRINT '============================================================================';

-- Cleanup helper functions (optional - comment out if you want to keep them)
-- DROP FUNCTION dbo.fn_ExtractFileName;
-- DROP FUNCTION dbo.fn_ExtractFileNamesFromPipeSeparated;
-- GO

PRINT '';
PRINT 'Migration completed successfully!';
PRINT 'Note: Media file paths are stored as filenames only.';
PRINT '      The API will construct URLs: /api/Media/files/{filename}';
PRINT '';

