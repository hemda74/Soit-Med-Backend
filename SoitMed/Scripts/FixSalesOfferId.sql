-- Fix SalesOffers.Id column type from bigint to nvarchar(50)
-- This resolves the EF type mismatch where SalesOffer.Id should be string (from BaseEntity)

USE [ITIWebApi44]
GO

PRINT 'Starting SalesOffers.Id type fix...'

-- Check current state
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMNPROPERTY(OBJECT_ID('SalesOffers'), COLUMN_NAME, 'IsIdentity') AS IsIdentity
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SalesOffers' AND COLUMN_NAME = 'Id'
GO

-- Step 1: Drop all foreign key constraints referencing SalesOffers.Id
PRINT 'Step 1: Dropping foreign key constraints...'

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_OfferRequests_SalesOffers_CreatedOfferId')
BEGIN
    ALTER TABLE OfferRequests DROP CONSTRAINT FK_OfferRequests_SalesOffers_CreatedOfferId
    PRINT 'Dropped FK_OfferRequests_SalesOffers_CreatedOfferId'
END
GO

-- Step 2: Drop all indexes on SalesOffers.Id
PRINT 'Step 2: Dropping indexes...'

DECLARE @sql NVARCHAR(MAX) = ''
SELECT @sql = @sql + 'DROP INDEX ' + name + ' ON SalesOffers' + CHAR(13) + CHAR(10)
FROM sys.indexes 
WHERE object_id = OBJECT_ID('SalesOffers') 
    AND name IS NOT NULL 
    AND name <> 'PK_SalesOffers'

IF LEN(@sql) > 0
BEGIN
    EXEC sp_executesql @sql
    PRINT 'Dropped all non-PK indexes on SalesOffers'
END
GO

-- Step 3: Drop primary key constraint
PRINT 'Step 3: Dropping primary key constraint...'

IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('SalesOffers') AND type = 'PK')
BEGIN
    DECLARE @PkName NVARCHAR(128)
    SELECT @PkName = name FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('SalesOffers') AND type = 'PK'
    EXEC ('ALTER TABLE SalesOffers DROP CONSTRAINT ' + @PkName)
    PRINT 'Dropped SalesOffers primary key: ' + @PkName
END
GO

-- Step 4: Recreate table with correct column types (to handle identity)
PRINT 'Step 4: Recreating SalesOffers table with correct types...'

BEGIN TRY
    -- Create a temporary table to hold the data
    SELECT * INTO #TempSalesOffers FROM SalesOffers
    
    -- Drop the original table
    DROP TABLE SalesOffers
    
    -- Recreate the table with correct column types
    CREATE TABLE SalesOffers (
        Id NVARCHAR(50) NOT NULL PRIMARY KEY,
        OfferRequestId NVARCHAR(50) NULL,
        ClientId NVARCHAR(50) NOT NULL,
        CreatedBy NVARCHAR(50) NOT NULL,
        AssignedTo NVARCHAR(50) NOT NULL,
        OfferNumber NVARCHAR(50) NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(2000) NULL,
        FinalPrice DECIMAL(18,2) NOT NULL,
        OriginalPrice DECIMAL(18,2) NULL,
        DiscountPercentage DECIMAL(5,2) NULL,
        OfferDuration INT NULL,
        Status NVARCHAR(50) NOT NULL,
        ValidFrom DATETIME2 NOT NULL,
        ValidTo DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        UpdatedAt DATETIME2 NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        Notes NVARCHAR(2000) NULL,
        InternalNotes NVARCHAR(2000) NULL,
        Terms NVARCHAR(2000) NULL,
        PaymentTerms NVARCHAR(500) NULL,
        DeliveryTerms NVARCHAR(500) NULL,
        WarrantyTerms NVARCHAR(1000) NULL,
        SalesManagerId NVARCHAR(50) NULL,
        SalesManagerApprovalDate DATETIME2 NULL,
        SentToClientDate DATETIME2 NULL,
        ClientResponse NVARCHAR(50) NULL,
        ClientResponseDate DATETIME2 NULL,
        CompletionDate DATETIME2 NULL,
        CompletionNotes NVARCHAR(1000) NULL,
        NeedsModification BIT NOT NULL DEFAULT 0,
        ModificationReason NVARCHAR(1000) NULL,
        UnderReviewDate DATETIME2 NULL,
        ResumeDate DATETIME2 NULL
    )
    
    -- Copy data back
    INSERT INTO SalesOffers (
        Id, OfferRequestId, ClientId, CreatedBy, AssignedTo, OfferNumber, Title, Description,
        FinalPrice, OriginalPrice, DiscountPercentage, OfferDuration, Status, ValidFrom, ValidTo,
        CreatedAt, UpdatedAt, IsActive, Notes, InternalNotes, Terms, PaymentTerms, DeliveryTerms,
        WarrantyTerms, SalesManagerId, SalesManagerApprovalDate, SentToClientDate, ClientResponse,
        ClientResponseDate, CompletionDate, CompletionNotes, NeedsModification, ModificationReason,
        UnderReviewDate, ResumeDate
    )
    SELECT 
        Id, OfferRequestId, ClientId, CreatedBy, AssignedTo, OfferNumber, Title, Description,
        FinalPrice, OriginalPrice, DiscountPercentage, OfferDuration, Status, ValidFrom, ValidTo,
        CreatedAt, UpdatedAt, IsActive, Notes, InternalNotes, Terms, PaymentTerms, DeliveryTerms,
        WarrantyTerms, SalesManagerId, SalesManagerApprovalDate, SentToClientDate, ClientResponse,
        ClientResponseDate, CompletionDate, CompletionNotes, NeedsModification, ModificationReason,
        UnderReviewDate, ResumeDate
    FROM #TempSalesOffers
    
    PRINT 'Successfully recreated SalesOffers table with NVARCHAR(50) Id'
END TRY
BEGIN CATCH
    PRINT 'Error recreating SalesOffers: ' + ERROR_MESSAGE()
    
    -- Try to restore the original table if something went wrong
    IF OBJECT_ID('SalesOffers') IS NULL AND OBJECT_ID('TempSalesOffers') IS NOT NULL
    BEGIN
        SELECT * INTO SalesOffers FROM #TempSalesOffers
        PRINT 'Restored original SalesOffers table from backup'
    END
END CATCH
GO

-- Step 5: Clean up temp table
PRINT 'Step 5: Cleaning up...'
DROP TABLE IF EXISTS #TempSalesOffers
GO

-- Step 6: Recreate foreign key constraint
PRINT 'Step 6: Recreating foreign key constraint...'

ALTER TABLE OfferRequests 
ADD CONSTRAINT FK_OfferRequests_SalesOffers_CreatedOfferId 
FOREIGN KEY (CreatedOfferId) REFERENCES SalesOffers(Id)
ON DELETE NO ACTION
PRINT 'Recreated FK_OfferRequests_SalesOffers_CreatedOfferId'
GO

-- Step 7: Final verification
PRINT 'Step 7: Final verification...'
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SalesOffers' AND COLUMN_NAME = 'Id'
UNION ALL
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'OfferRequests' AND COLUMN_NAME = 'CreatedOfferId'
GO

PRINT 'SalesOffers.Id type fix completed successfully!'
