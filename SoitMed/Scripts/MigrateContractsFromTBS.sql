-- ============================================================================
-- Contract Migration Script: TBS → ITIWebApi44
-- ============================================================================
-- This script migrates contracts from legacy TBS database to new ITIWebApi44 database
-- Features:
--   - Idempotency: Skips contracts already migrated (checks LegacyContractId)
--   - Media Path Transformation: Converts legacy file paths to API URLs
--   - Installment Creation: Creates InstallmentSchedules from payment plans
--   - System Notes: Creates "Migrated from Legacy TBS" note in ContractNegotiations
-- ============================================================================
-- Usage: Run this script in SQL Server Management Studio on the server
--        that hosts both TBS and ITIWebApi44 databases
-- ============================================================================

USE ITIWebApi44;
GO

-- ============================================================================
-- Step 1: Create Helper Functions (outside transaction)
-- ============================================================================

-- Drop functions if they exist
IF OBJECT_ID('dbo.fn_ExtractFileName', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_ExtractFileName;
GO

IF OBJECT_ID('dbo.fn_TransformMediaPath', 'FN') IS NOT NULL
    DROP FUNCTION dbo.fn_TransformMediaPath;
GO

-- Helper Function: Extract filename from legacy path
CREATE FUNCTION dbo.fn_ExtractFileName(@Path NVARCHAR(500))
RETURNS NVARCHAR(500)
AS
BEGIN
    DECLARE @FileName NVARCHAR(500) = @Path;
    
    -- Remove common legacy path prefixes
    SET @FileName = REPLACE(@FileName, 'D:\Soit-Med\legacy\SOIT\UploadFiles\Files\', '');
    SET @FileName = REPLACE(@FileName, 'D:\Soit-Med\legacy\SOIT\UploadFiles\Images\', '');
    SET @FileName = REPLACE(@FileName, 'D:\Soit-Med\legacy\SOIT\Ar\MNT\FileUploaders\Reports\', '');
    SET @FileName = REPLACE(@FileName, 'C:\Soit-Med\legacy\SOIT\UploadFiles\Files\', '');
    SET @FileName = REPLACE(@FileName, 'C:\Soit-Med\legacy\SOIT\UploadFiles\Images\', '');
    SET @FileName = REPLACE(@FileName, 'C:\Soit-Med\legacy\SOIT\Ar\MNT\FileUploaders\Reports\', '');
    
    -- Extract filename (last part after backslash)
    IF CHARINDEX('\', @FileName) > 0
    BEGIN
        SET @FileName = REVERSE(LEFT(REVERSE(@FileName), CHARINDEX('\', REVERSE(@FileName)) - 1));
    END
    
    -- Remove leading/trailing spaces
    SET @FileName = LTRIM(RTRIM(@FileName));
    
    RETURN @FileName;
END;
GO

-- Helper Function: Transform legacy path to API URL
CREATE FUNCTION dbo.fn_TransformMediaPath(@LegacyPath NVARCHAR(500), @BaseUrl NVARCHAR(500))
RETURNS NVARCHAR(500)
AS
BEGIN
    DECLARE @FileName NVARCHAR(500) = dbo.fn_ExtractFileName(@LegacyPath);
    DECLARE @ApiUrl NVARCHAR(500) = '';
    
    IF @FileName IS NULL OR @FileName = ''
        RETURN '';
    
    -- Build API URL
    IF @BaseUrl IS NOT NULL AND @BaseUrl != ''
    BEGIN
        SET @ApiUrl = @BaseUrl + '/api/Media/files/' + @FileName;
    END
    ELSE
    BEGIN
        SET @ApiUrl = '/api/LegacyMedia/files/' + @FileName;
    END
    
    RETURN @ApiUrl;
END;
GO

-- ============================================================================
-- Step 2: Main Migration Script (with transaction)
-- ============================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @AdminUserId NVARCHAR(450) = 'Hemdan_Test_Administration_002'; -- Update with actual admin user ID
DECLARE @LegacyMediaApiBaseUrl NVARCHAR(500) = 'http://10.10.9.104:5266'; -- Update if needed
DECLARE @MigratedCount INT = 0;
DECLARE @ErrorCount INT = 0;
DECLARE @SkippedCount INT = 0;
DECLARE @StartTime DATETIME2 = GETUTCDATE();

PRINT '============================================================================';
PRINT 'Contract Migration: TBS → ITIWebApi44';
PRINT '============================================================================';
PRINT 'Start Time: ' + CONVERT(VARCHAR, @StartTime, 120);
PRINT '';

BEGIN TRY
    -- Note: We use individual transactions per contract to avoid cascading failures
    -- The main transaction is only for the overall process, not individual contracts

    -- ============================================================================
    -- Main Migration Logic
    -- ============================================================================
    
    DECLARE @ContractId INT;
    DECLARE @CusId INT;
    DECLARE @SoId INT;
    DECLARE @ContractCode INT;
    DECLARE @ClasserNumber NVARCHAR(50);
    DECLARE @ContractTotalValue DECIMAL(18,3);
    DECLARE @StartDate DATETIME;
    DECLARE @EndDate DATETIME;
    DECLARE @ScFile NVARCHAR(255);
    DECLARE @NotesTech NVARCHAR(MAX);
    DECLARE @NotesFinance NVARCHAR(MAX);
    DECLARE @NotesAdmin NVARCHAR(MAX);
    DECLARE @InstallmentMonths INT;
    DECLARE @InstallmentAmount DECIMAL(18,3);
    
    DECLARE @NewContractId BIGINT;
    DECLARE @ClientId BIGINT;
    DECLARE @DealId BIGINT;
    DECLARE @ContractStatus INT; -- 0=Draft, 1=SentToCustomer, 2=UnderNegotiation, 3=Signed, 4=Cancelled, 5=Expired
    DECLARE @DocumentUrl NVARCHAR(500);
    DECLARE @ContractContent NVARCHAR(MAX);
    DECLARE @ContractNumber NVARCHAR(100);
    DECLARE @Title NVARCHAR(200);
    
    -- Cursor to iterate through legacy contracts
    DECLARE contract_cursor CURSOR FOR
    SELECT 
        ContractId,
        [Cus_ID],
        [SO_ID],
        ContractCode,
        ClasserNumber,
        ContractTotalValue,
        StartDate,
        EndDate,
        NULL AS ScFile, -- TBS doesn't have ScFile in MNT_MaintenanceContract, check Stk_Sales_Contract if needed
        [Notes_Tech],
        [Notes_Finance],
        [Notes_Admin],
        NULL AS InstallmentMonths, -- Add if exists in TBS
        NULL AS InstallmentAmount  -- Add if exists in TBS
    FROM TBS.dbo.MNT_MaintenanceContract
    WHERE ContractId IS NOT NULL
    ORDER BY ContractId;

    OPEN contract_cursor;
    FETCH NEXT FROM contract_cursor INTO 
        @ContractId, @CusId, @SoId, @ContractCode, @ClasserNumber, 
        @ContractTotalValue, @StartDate, @EndDate, @ScFile,
        @NotesTech, @NotesFinance, @NotesAdmin, @InstallmentMonths, @InstallmentAmount;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Start a new transaction for each contract to avoid cascading failures
        BEGIN TRANSACTION ContractMigration;
        
        BEGIN TRY
            -- ====================================================================
            -- 1. Idempotency Check: Skip if already migrated
            -- ====================================================================
            IF EXISTS (SELECT 1 FROM ITIWebApi44.dbo.Contracts WHERE LegacyContractId = @ContractId)
            BEGIN
                SET @SkippedCount = @SkippedCount + 1;
                PRINT 'SKIPPED: Contract ' + CAST(@ContractId AS VARCHAR) + ' already migrated';
                COMMIT TRANSACTION ContractMigration;
                FETCH NEXT FROM contract_cursor INTO 
                    @ContractId, @CusId, @SoId, @ContractCode, @ClasserNumber, 
                    @ContractTotalValue, @StartDate, @EndDate, @ScFile,
                    @NotesTech, @NotesFinance, @NotesAdmin, @InstallmentMonths, @InstallmentAmount;
                CONTINUE;
            END

            -- ====================================================================
            -- 2. Find Client by LegacyCustomerId
            -- ====================================================================
            SELECT @ClientId = Id 
            FROM ITIWebApi44.dbo.Clients 
            WHERE LegacyCustomerId = @CusId;

            IF @ClientId IS NULL
            BEGIN
                -- Try to find by name from TBS
                DECLARE @CustomerName NVARCHAR(200);
                SELECT @CustomerName = [Cus_Name] 
                FROM TBS.dbo.Stk_Customers 
                WHERE [Cus_ID] = @CusId;

                IF @CustomerName IS NOT NULL
                BEGIN
                    SELECT @ClientId = Id 
                    FROM ITIWebApi44.dbo.Clients 
                    WHERE Name = @CustomerName;
                END
            END

            -- ====================================================================
            -- 2b. Create Placeholder Client if not found (to allow migration to proceed)
            -- ====================================================================
            IF @ClientId IS NULL
            BEGIN
                -- Get customer details from TBS
                DECLARE @TbsCustomerName NVARCHAR(200) = NULL;
                DECLARE @TbsCustomerMobile NVARCHAR(20) = NULL;
                DECLARE @TbsCustomerPhone NVARCHAR(20) = NULL;
                DECLARE @TbsCustomerEmail NVARCHAR(100) = NULL;
                DECLARE @TbsCustomerAddress NVARCHAR(500) = NULL;
                
                SELECT 
                    @TbsCustomerName = [Cus_Name],
                    @TbsCustomerMobile = [Cus_Mobile],
                    @TbsCustomerPhone = [Cus_Tel],
                    @TbsCustomerEmail = [Cus_Email],
                    @TbsCustomerAddress = [Cus_address]
                FROM TBS.dbo.Stk_Customers 
                WHERE [Cus_ID] = @CusId;
                
                -- Create placeholder client
                INSERT INTO ITIWebApi44.dbo.Clients (
                    Name,
                    Phone,
                    Email,
                    Address,
                    LegacyCustomerId,
                    RelatedUserId,
                    Status,
                    Priority,
                    CreatedBy,
                    CreatedAt,
                    UpdatedAt,
                    IsActive
                )
                VALUES (
                    ISNULL(@TbsCustomerName, 'Legacy Client ' + CAST(@CusId AS VARCHAR)),
                    ISNULL(@TbsCustomerMobile, @TbsCustomerPhone),
                    @TbsCustomerEmail,
                    @TbsCustomerAddress,
                    @CusId,
                    NULL,
                    'Active', -- ClientStatus.Active
                    'Medium', -- ClientPriority.Medium
                    'ContractMigration',
                    GETUTCDATE(),
                    GETUTCDATE(),
                    1
                );
                
                SET @ClientId = SCOPE_IDENTITY();
                PRINT 'CREATED: Placeholder client for CusId ' + CAST(@CusId AS VARCHAR) + ' -> ClientId ' + CAST(@ClientId AS VARCHAR) + ' (Name: ' + ISNULL(@TbsCustomerName, 'Legacy Client ' + CAST(@CusId AS VARCHAR)) + ')';
            END

            -- ====================================================================
            -- 3. Find Deal (optional - can be NULL)
            -- ====================================================================
            SET @DealId = NULL;
            IF @SoId IS NOT NULL
            BEGIN
                SELECT @DealId = Id 
                FROM ITIWebApi44.dbo.SalesDeals 
                WHERE Id = @SoId;
            END

            -- ====================================================================
            -- 4. Determine Contract Status
            -- ====================================================================
            DECLARE @Now DATETIME2 = GETUTCDATE();
            IF @EndDate < @Now
                SET @ContractStatus = 5; -- Expired
            ELSE IF @StartDate <= @Now AND @EndDate >= @Now
                SET @ContractStatus = 3; -- Signed (active legacy contracts)
            ELSE
                SET @ContractStatus = 3; -- Default to Signed for legacy contracts

            -- ====================================================================
            -- 5. Transform Media Path (if ScFile exists)
            -- ====================================================================
            SET @DocumentUrl = NULL;
            IF @ScFile IS NOT NULL AND @ScFile != ''
            BEGIN
                SET @DocumentUrl = dbo.fn_TransformMediaPath(@ScFile, @LegacyMediaApiBaseUrl);
            END

            -- ====================================================================
            -- 6. Build Contract Content from Notes
            -- ====================================================================
            SET @ContractContent = '';
            IF @NotesTech IS NOT NULL AND @NotesTech != ''
                SET @ContractContent = @ContractContent + 'Technical Notes: ' + @NotesTech + CHAR(13) + CHAR(10);
            IF @NotesFinance IS NOT NULL AND @NotesFinance != ''
                SET @ContractContent = @ContractContent + 'Financial Notes: ' + @NotesFinance + CHAR(13) + CHAR(10);
            IF @NotesAdmin IS NOT NULL AND @NotesAdmin != ''
                SET @ContractContent = @ContractContent + 'Administrative Notes: ' + @NotesAdmin + CHAR(13) + CHAR(10);
            SET @ContractContent = @ContractContent + 'Contract Code: ' + ISNULL(CAST(@ContractCode AS VARCHAR), 'N/A') + CHAR(13) + CHAR(10);
            SET @ContractContent = @ContractContent + 'Classer Number: ' + ISNULL(@ClasserNumber, 'N/A') + CHAR(13) + CHAR(10);
            SET @ContractContent = @ContractContent + 'Total Value: ' + CAST(@ContractTotalValue AS VARCHAR);

            -- ====================================================================
            -- 7. Generate Contract Number and Title (ensure uniqueness)
            -- ====================================================================
            -- Generate unique contract number: Use ContractCode if available, otherwise use LEG-ContractId
            -- If ContractCode would cause duplicate, append ContractId to make it unique
            DECLARE @BaseContractNumber NVARCHAR(100) = ISNULL(CAST(@ContractCode AS VARCHAR), 'LEG-' + CAST(@ContractId AS VARCHAR));
            
            -- Check if this contract number already exists
            IF EXISTS (SELECT 1 FROM ITIWebApi44.dbo.Contracts WHERE ContractNumber = @BaseContractNumber)
            BEGIN
                -- Make it unique by appending ContractId
                SET @ContractNumber = @BaseContractNumber + '-' + CAST(@ContractId AS VARCHAR);
            END
            ELSE
            BEGIN
                SET @ContractNumber = @BaseContractNumber;
            END
            
            SET @Title = 'Maintenance Contract ' + ISNULL(CAST(@ContractCode AS VARCHAR), CAST(@ContractId AS VARCHAR));

            -- ====================================================================
            -- 8. Insert Contract
            -- ====================================================================
            INSERT INTO ITIWebApi44.dbo.Contracts (
                DealId,
                ContractNumber,
                Title,
                ContractContent,
                DocumentUrl,
                Status,
                DraftedAt,
                SignedAt,
                ClientId,
                DraftedBy,
                CashAmount,
                InstallmentAmount,
                InstallmentDurationMonths,
                LegacyContractId,
                CreatedAt,
                UpdatedAt
            )
            VALUES (
                @DealId,
                @ContractNumber,
                @Title,
                @ContractContent,
                @DocumentUrl,
                @ContractStatus,
                @StartDate,
                CASE WHEN @ContractStatus = 3 THEN @StartDate ELSE NULL END,
                @ClientId,
                @AdminUserId,
                CASE WHEN @InstallmentAmount IS NULL THEN @ContractTotalValue ELSE NULL END,
                @InstallmentAmount,
                @InstallmentMonths,
                @ContractId,
                @StartDate,
                GETUTCDATE()
            );

            SET @NewContractId = SCOPE_IDENTITY();

            -- ====================================================================
            -- 9. Create System Note in ContractNegotiations
            -- ====================================================================
            INSERT INTO ITIWebApi44.dbo.ContractNegotiations (
                ContractId,
                ActionType,
                Notes,
                SubmittedBy,
                SubmitterRole,
                SubmittedAt,
                CreatedAt,
                UpdatedAt
            )
            VALUES (
                @NewContractId,
                'System',
                'Migrated from Legacy TBS',
                @AdminUserId,
                'System',
                GETUTCDATE(),
                GETUTCDATE(),
                GETUTCDATE()
            );

            -- ====================================================================
            -- 10. Create Installment Schedules (if payment plan exists)
            -- ====================================================================
            IF @InstallmentMonths IS NOT NULL AND @InstallmentMonths > 0 
               AND @InstallmentAmount IS NOT NULL AND @InstallmentAmount > 0
            BEGIN
                DECLARE @InstallmentNumber INT = 1;
                DECLARE @PerInstallmentAmount DECIMAL(18,2) = @InstallmentAmount / @InstallmentMonths;
                DECLARE @DueDate DATETIME2;
                DECLARE @InstallmentStatus INT; -- 0=Pending, 1=Paid, 2=Overdue, 3=Cancelled

                WHILE @InstallmentNumber <= @InstallmentMonths
                BEGIN
                    SET @DueDate = DATEADD(MONTH, @InstallmentNumber, @StartDate);
                    
                    -- Determine status: Overdue if due date passed, else Pending
                    IF @DueDate < @Now
                        SET @InstallmentStatus = 2; -- Overdue
                    ELSE
                        SET @InstallmentStatus = 0; -- Pending

                    INSERT INTO ITIWebApi44.dbo.InstallmentSchedules (
                        ContractId,
                        InstallmentNumber,
                        Amount,
                        DueDate,
                        Status,
                        NotificationSent7Days,
                        NotificationSent2Days,
                        NotificationSent1Day,
                        OverdueNotificationSent,
                        CreatedAt,
                        UpdatedAt
                    )
                    VALUES (
                        @NewContractId,
                        @InstallmentNumber,
                        @PerInstallmentAmount,
                        @DueDate,
                        @InstallmentStatus,
                        0,
                        0,
                        0,
                        0,
                        GETUTCDATE(),
                        GETUTCDATE()
                    );

                    SET @InstallmentNumber = @InstallmentNumber + 1;
                END
            END

            SET @MigratedCount = @MigratedCount + 1;
            
            IF @MigratedCount % 10 = 0
            BEGIN
                PRINT 'Progress: ' + CAST(@MigratedCount AS VARCHAR) + ' contracts migrated...';
            END

            -- Commit transaction for this contract
            COMMIT TRANSACTION ContractMigration;

        END TRY
        BEGIN CATCH
            -- Rollback transaction for this contract
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION ContractMigration;
            
            SET @ErrorCount = @ErrorCount + 1;
            DECLARE @ErrorMsg NVARCHAR(MAX) = 'ERROR: Contract ' + CAST(@ContractId AS VARCHAR) + ' - ' + ERROR_MESSAGE();
            PRINT @ErrorMsg;
            -- Continue with next contract (transaction already rolled back)
        END CATCH

        FETCH NEXT FROM contract_cursor INTO 
            @ContractId, @CusId, @SoId, @ContractCode, @ClasserNumber, 
            @ContractTotalValue, @StartDate, @EndDate, @ScFile,
            @NotesTech, @NotesFinance, @NotesAdmin, @InstallmentMonths, @InstallmentAmount;
    END

    CLOSE contract_cursor;
    DEALLOCATE contract_cursor;

    -- All individual contract transactions have been committed/rolled back
    -- No need for a main transaction commit here

    -- ============================================================================
    -- Print Summary
    -- ============================================================================
    DECLARE @EndTime DATETIME2 = GETUTCDATE();
    DECLARE @DurationSeconds INT = DATEDIFF(SECOND, @StartTime, @EndTime);

    PRINT '';
    PRINT '============================================================================';
    PRINT 'MIGRATION COMPLETED';
    PRINT '============================================================================';
    PRINT 'Start Time: ' + CONVERT(VARCHAR, @StartTime, 120);
    PRINT 'End Time: ' + CONVERT(VARCHAR, @EndTime, 120);
    PRINT 'Duration: ' + CAST(@DurationSeconds AS VARCHAR) + ' seconds';
    PRINT '';
    PRINT 'Results:';
    PRINT '  ✓ Migrated: ' + CAST(@MigratedCount AS VARCHAR);
    PRINT '  ⊘ Skipped: ' + CAST(@SkippedCount AS VARCHAR) + ' (already migrated)';
    PRINT '  ✗ Errors: ' + CAST(@ErrorCount AS VARCHAR);
    PRINT '============================================================================';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT '';
    PRINT '============================================================================';
    PRINT 'MIGRATION FAILED';
    PRINT '============================================================================';
    PRINT 'Error: ' + ERROR_MESSAGE();
    PRINT 'Line: ' + CAST(ERROR_LINE() AS VARCHAR);
    PRINT '============================================================================';
    THROW;
END CATCH;

-- ============================================================================
-- Step 3: Cleanup Helper Functions (optional - can keep them for future use)
-- ============================================================================
-- Uncomment the following lines if you want to remove the helper functions:
-- DROP FUNCTION IF EXISTS dbo.fn_TransformMediaPath;
-- DROP FUNCTION IF EXISTS dbo.fn_ExtractFileName;
-- GO

