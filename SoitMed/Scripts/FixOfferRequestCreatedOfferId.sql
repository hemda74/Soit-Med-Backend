-- Fix OfferRequest.CreatedOfferId column type from bigint to nvarchar(50)
-- This resolves the EF type mismatch between OfferRequest.CreatedOfferId (long?) and SalesOffer.Id (string)

USE [ITIWebApi44]
GO

PRINT 'Starting OfferRequest.CreatedOfferId type fix...'

-- Check current state
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'OfferRequests' AND COLUMN_NAME = 'CreatedOfferId'
GO

-- Step 1: Drop any foreign key constraints on CreatedOfferId
PRINT 'Step 1: Dropping foreign key constraints...'

IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_OfferRequests_SalesOffers_CreatedOfferId')
BEGIN
    ALTER TABLE OfferRequests DROP CONSTRAINT FK_OfferRequests_SalesOffers_CreatedOfferId
    PRINT 'Dropped FK_OfferRequests_SalesOffers_CreatedOfferId'
END
GO

-- Step 2: Drop any indexes on CreatedOfferId
PRINT 'Step 2: Dropping indexes...'

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OfferRequests_CreatedOfferId' AND object_id = OBJECT_ID('OfferRequests'))
BEGIN
    DROP INDEX IX_OfferRequests_CreatedOfferId ON OfferRequests
    PRINT 'Dropped IX_OfferRequests_CreatedOfferId'
END
GO

-- Step 3: Change column type
PRINT 'Step 3: Changing CreatedOfferId column type...'

ALTER TABLE OfferRequests
ALTER COLUMN CreatedOfferId NVARCHAR(50) NULL
PRINT 'Changed OfferRequests.CreatedOfferId to NVARCHAR(50) NULL'
GO

-- Step 4: Recreate foreign key constraint
PRINT 'Step 4: Recreating foreign key constraint...'

ALTER TABLE OfferRequests 
ADD CONSTRAINT FK_OfferRequests_SalesOffers_CreatedOfferId 
FOREIGN KEY (CreatedOfferId) REFERENCES SalesOffers(Id)
ON DELETE NO ACTION
PRINT 'Recreated FK_OfferRequests_SalesOffers_CreatedOfferId'
GO

-- Step 5: Recreate index
PRINT 'Step 5: Recreating index...'

CREATE INDEX IX_OfferRequests_CreatedOfferId ON OfferRequests(CreatedOfferId)
PRINT 'Recreated IX_OfferRequests_CreatedOfferId'
GO

-- Step 6: Final verification
PRINT 'Step 6: Final verification...'
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'OfferRequests' AND COLUMN_NAME = 'CreatedOfferId'
GO

PRINT 'OfferRequest.CreatedOfferId type fix completed successfully!'
