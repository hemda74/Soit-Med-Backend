-- Recreate SalesOffers table with correct schema and NVARCHAR(50) Id

USE [ITIWebApi44]
GO

PRINT 'Recreating SalesOffers table from scratch...'

-- Drop the broken table if it exists
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SalesOffers')
BEGIN
    DROP TABLE SalesOffers
    PRINT 'Dropped broken SalesOffers table'
END
GO

-- Create the table with correct schema
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
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
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
GO

PRINT 'Created SalesOffers table with NVARCHAR(50) Id'

-- Create indexes
CREATE INDEX IX_SalesOffers_ClientId ON SalesOffers(ClientId)
CREATE INDEX IX_SalesOffers_CreatedBy ON SalesOffers(CreatedBy)
CREATE INDEX IX_SalesOffers_AssignedTo ON SalesOffers(AssignedTo)
CREATE INDEX IX_SalesOffers_Status ON SalesOffers(Status)
CREATE INDEX IX_SalesOffers_OfferRequestId ON SalesOffers(OfferRequestId)
GO

PRINT 'Created indexes on SalesOffers'

-- Recreate foreign key constraint
ALTER TABLE OfferRequests 
ADD CONSTRAINT FK_OfferRequests_SalesOffers_CreatedOfferId 
FOREIGN KEY (CreatedOfferId) REFERENCES SalesOffers(Id)
ON DELETE NO ACTION
GO

PRINT 'Recreated FK_OfferRequests_SalesOffers_CreatedOfferId'

-- Verification
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

PRINT 'SalesOffers table recreation completed successfully!'
