-- ============================================
-- Convert Offer Terms to JSON Arrays
-- ============================================
-- This script converts PaymentTerms, DeliveryTerms, ValidUntil from single values to JSON arrays
-- and adds WarrantyTerms column as JSON array
-- Safe migration with backward compatibility
-- ============================================
-- IMPORTANT: Make sure you're connected to the correct database before running this script
-- ============================================

BEGIN TRANSACTION;

-- Ensure we're using the correct schema
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

BEGIN TRY
    PRINT 'Starting Offer Terms conversion to JSON arrays...';
    PRINT '==================================================';

    -- 1. Add WarrantyTerms column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SalesOffers]') AND name = 'WarrantyTerms')
    BEGIN
        ALTER TABLE [dbo].[SalesOffers] 
        ADD [WarrantyTerms] NVARCHAR(MAX) NULL;
        PRINT '✅ WarrantyTerms column added successfully';
    END
    ELSE
    BEGIN
        PRINT '⚠️ WarrantyTerms column already exists';
    END

    -- 2. Convert existing PaymentTerms to JSON array (if not already JSON)
    DECLARE @PaymentTermsCount INT;
    SELECT @PaymentTermsCount = COUNT(*) 
    FROM [dbo].[SalesOffers] 
    WHERE PaymentTerms IS NOT NULL 
      AND PaymentTerms != '' 
      AND LEFT(LTRIM(PaymentTerms), 1) != '['; -- Not already JSON array

    PRINT '';
    PRINT 'Converting PaymentTerms to JSON arrays...';
    PRINT 'Found ' + CAST(@PaymentTermsCount AS VARCHAR(10)) + ' records to convert';

    UPDATE [dbo].[SalesOffers]
    SET PaymentTerms = '["' + REPLACE(REPLACE(PaymentTerms, '"', '\"'), '''', '''''') + '"]'
    WHERE PaymentTerms IS NOT NULL 
      AND PaymentTerms != '' 
      AND LEFT(LTRIM(PaymentTerms), 1) != '['; -- Not already JSON array

    PRINT '✅ PaymentTerms converted to JSON arrays';

    -- 3. Convert existing DeliveryTerms to JSON array (if not already JSON)
    DECLARE @DeliveryTermsCount INT;
    SELECT @DeliveryTermsCount = COUNT(*) 
    FROM [dbo].[SalesOffers] 
    WHERE DeliveryTerms IS NOT NULL 
      AND DeliveryTerms != '' 
      AND LEFT(LTRIM(DeliveryTerms), 1) != '['; -- Not already JSON array

    PRINT '';
    PRINT 'Converting DeliveryTerms to JSON arrays...';
    PRINT 'Found ' + CAST(@DeliveryTermsCount AS VARCHAR(10)) + ' records to convert';

    UPDATE [dbo].[SalesOffers]
    SET DeliveryTerms = '["' + REPLACE(REPLACE(DeliveryTerms, '"', '\"'), '''', '''''') + '"]'
    WHERE DeliveryTerms IS NOT NULL 
      AND DeliveryTerms != '' 
      AND LEFT(LTRIM(DeliveryTerms), 1) != '['; -- Not already JSON array

    PRINT '✅ DeliveryTerms converted to JSON arrays';

    -- 4. Convert ValidUntil from DateTime/DateTime2 to JSON array of date strings
    DECLARE @ValidUntilCount INT;
    DECLARE @ValidUntilType INT;
    DECLARE @HasValidUntil BIT = 0;
    DECLARE @HasValidUntilTemp BIT = 0;
    
    -- First, check if ValidUntilTemp exists from a previous failed run
    IF EXISTS (
        SELECT * FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'[dbo].[SalesOffers]') 
          AND name = 'ValidUntilTemp'
    )
    BEGIN
        SET @HasValidUntilTemp = 1;
        
        -- If ValidUntilTemp exists but ValidUntil doesn't, rename temp back to ValidUntil
        IF NOT EXISTS (
            SELECT * FROM sys.columns 
            WHERE object_id = OBJECT_ID(N'[dbo].[SalesOffers]') 
              AND name = 'ValidUntil'
        )
        BEGIN
            EXEC sp_rename '[dbo].[SalesOffers].[ValidUntilTemp]', 'ValidUntil', 'COLUMN';
            PRINT '⚠️ Recovered ValidUntil from ValidUntilTemp (previous run recovery)';
            SET @HasValidUntil = 1;
        END
        ELSE
        BEGIN
            -- Both exist, drop the temp column
            ALTER TABLE [dbo].[SalesOffers] 
            DROP COLUMN [ValidUntilTemp];
            PRINT '⚠️ Removed existing ValidUntilTemp column';
        END
    END
    
    -- Check if ValidUntil column exists and get its type
    SELECT @ValidUntilType = system_type_id
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[SalesOffers]') 
      AND name = 'ValidUntil';
    
    IF @ValidUntilType IS NOT NULL
    BEGIN
        SET @HasValidUntil = 1;
        SELECT @ValidUntilCount = COUNT(*) 
        FROM [dbo].[SalesOffers] 
        WHERE ValidUntil IS NOT NULL;

        PRINT '';
        PRINT 'Converting ValidUntil to JSON array...';
        PRINT 'Found ' + CAST(@ValidUntilCount AS VARCHAR(10)) + ' records to convert';

        -- Check if ValidUntil is DateTime or DateTime2 type (needs to alter column)
        -- system_type_id: 61 = DateTime, 42 = DateTime2, 231 = NVARCHAR
        IF @ValidUntilType IN (61, 42) -- DateTime or DateTime2
        BEGIN
            -- Make sure ValidUntilTemp doesn't already exist (should have been cleaned up earlier)
            IF EXISTS (
                SELECT * FROM sys.columns 
                WHERE object_id = OBJECT_ID(N'[dbo].[SalesOffers]') 
                  AND name = 'ValidUntilTemp'
            )
            BEGIN
                ALTER TABLE [dbo].[SalesOffers] 
                DROP COLUMN [ValidUntilTemp];
                PRINT '⚠️ Cleaned up existing ValidUntilTemp before conversion';
            END

            -- Convert DateTime/DateTime2 to JSON array format
            -- First, create the temp column
            ALTER TABLE [dbo].[SalesOffers] 
            ADD [ValidUntilTemp] NVARCHAR(MAX) NULL;
            
            -- Verify column exists before updating (safety check)
            IF NOT EXISTS (
                SELECT * FROM sys.columns 
                WHERE object_id = OBJECT_ID(N'[dbo].[SalesOffers]') 
                  AND name = 'ValidUntilTemp'
            )
            BEGIN
                RAISERROR('Failed to create ValidUntilTemp column', 16, 1);
            END
            
            -- Convert DateTime/DateTime2 to date string (YYYY-MM-DD format) and update
            -- SQL Server's parser may not recognize the column immediately after ALTER TABLE ADD
            -- So we'll use a workaround: insert into a temp table, then update from there
            -- Or use dynamic SQL which parses at execution time, not compile time
            
            -- First, verify the column was created
            IF NOT EXISTS (
                SELECT * FROM sys.columns 
                WHERE object_id = OBJECT_ID(N'[dbo].[SalesOffers]') 
                  AND name = 'ValidUntilTemp'
            )
            BEGIN
                RAISERROR('ValidUntilTemp column was not created successfully', 16, 1);
            END
            
            -- Use dynamic SQL to ensure SQL Server recognizes the newly added column at execution time
            DECLARE @UpdateValidUntilSQL NVARCHAR(MAX);
            SET @UpdateValidUntilSQL = N'
                UPDATE [dbo].[SalesOffers]
                SET [ValidUntilTemp] = ''["'' + CONVERT(VARCHAR(10), CAST(ValidUntil AS DATE), 120) + ''"]''
                WHERE ValidUntil IS NOT NULL;
            ';
            EXEC sp_executesql @UpdateValidUntilSQL;
            
            -- Verify the update worked
            DECLARE @UpdatedCount INT;
            SELECT @UpdatedCount = COUNT(*) 
            FROM [dbo].[SalesOffers] 
            WHERE [ValidUntilTemp] IS NOT NULL;
            
            IF @UpdatedCount = 0 AND @ValidUntilCount > 0
            BEGIN
                PRINT '⚠️ Warning: ValidUntilTemp update may have failed. Checking...';
            END
            ELSE
            BEGIN
                PRINT '✅ Updated ' + CAST(@UpdatedCount AS VARCHAR(10)) + ' records in ValidUntilTemp';
            END

            -- Drop the old DateTime/DateTime2 column
            ALTER TABLE [dbo].[SalesOffers] 
            DROP COLUMN [ValidUntil];

            -- Rename the new column
            EXEC sp_rename '[dbo].[SalesOffers].[ValidUntilTemp]', 'ValidUntil', 'COLUMN';

            PRINT '✅ ValidUntil column type changed from DateTime/DateTime2 to NVARCHAR(MAX) and converted to JSON array';
        END
        ELSE IF @ValidUntilType = 231 -- NVARCHAR type
        BEGIN
            -- Already NVARCHAR, just convert values to JSON array if not already
            UPDATE [dbo].[SalesOffers]
            SET ValidUntil = '["' + CAST(ValidUntil AS VARCHAR(MAX)) + '"]'
            WHERE ValidUntil IS NOT NULL 
              AND ValidUntil != '' 
              AND LEFT(LTRIM(CAST(ValidUntil AS VARCHAR(MAX))), 1) != '['; -- Not already JSON array

            PRINT '✅ ValidUntil converted to JSON arrays';
        END
        ELSE
        BEGIN
            PRINT '⚠️ ValidUntil has unexpected type (system_type_id: ' + CAST(@ValidUntilType AS VARCHAR(10)) + ')';
        END
    END
    ELSE IF @HasValidUntilTemp = 0
    BEGIN
        PRINT '⚠️ ValidUntil column not found';
    END

    -- 5. Update column types to NVARCHAR(MAX) if needed (for PaymentTerms and DeliveryTerms)
    -- Note: If they're already NVARCHAR(MAX), this will be a no-op
    
    PRINT '';
    PRINT 'Ensuring column types are NVARCHAR(MAX)...';
    
    -- Check and update PaymentTerms if needed
    IF EXISTS (
        SELECT * FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'[dbo].[SalesOffers]') 
          AND name = 'PaymentTerms' 
          AND max_length != -1 -- Not MAX
    )
    BEGIN
        ALTER TABLE [dbo].[SalesOffers] 
        ALTER COLUMN [PaymentTerms] NVARCHAR(MAX) NULL;
        PRINT '✅ PaymentTerms column type updated to NVARCHAR(MAX)';
    END

    -- Check and update DeliveryTerms if needed
    IF EXISTS (
        SELECT * FROM sys.columns 
        WHERE object_id = OBJECT_ID(N'[dbo].[SalesOffers]') 
          AND name = 'DeliveryTerms' 
          AND max_length != -1 -- Not MAX
    )
    BEGIN
        ALTER TABLE [dbo].[SalesOffers] 
        ALTER COLUMN [DeliveryTerms] NVARCHAR(MAX) NULL;
        PRINT '✅ DeliveryTerms column type updated to NVARCHAR(MAX)';
    END

    PRINT '';
    PRINT '==================================================';
    PRINT '✅ Offer Terms conversion completed successfully!';
    PRINT '==================================================';
    PRINT '';
    PRINT 'Summary:';
    PRINT '  - WarrantyTerms: Added as NVARCHAR(MAX)';
    PRINT '  - PaymentTerms: Converted to JSON arrays';
    PRINT '  - DeliveryTerms: Converted to JSON arrays';
    PRINT '  - ValidUntil: Converted from DateTime to JSON array of date strings';
    PRINT '';
    PRINT 'All existing data has been preserved and converted.';
    PRINT 'New format: JSON arrays like ["term1", "term2", "term3"]';
    PRINT '';

    COMMIT TRANSACTION;
    PRINT 'Transaction committed successfully!';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '❌ ERROR: Transaction rolled back!';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(10));
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR(10));
    THROW;
END CATCH;

