-- Recreate OfferRequests table with correct schema

USE [ITIWebApi44]
GO

PRINT 'Recreating OfferRequests table with correct schema...'

-- Create the table with correct column types
CREATE TABLE OfferRequests (
    Id NVARCHAR(50) NOT NULL PRIMARY KEY,
    RequestedBy NVARCHAR(50) NOT NULL,
    ClientId NVARCHAR(50) NOT NULL,
    TaskProgressId BIGINT NULL,
    RequestedProducts NVARCHAR(2000) NOT NULL,
    SpecialNotes NVARCHAR(2000) NULL,
    RequestDate DATETIME2 NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Requested',
    AssignedTo NVARCHAR(50) NULL,
    CreatedOfferId NVARCHAR(50) NULL,
    CompletedAt DATETIME2 NULL,
    CompletionNotes NVARCHAR(1000) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
)
GO

PRINT 'Created OfferRequests table with NVARCHAR(50) Id'

-- Create indexes
CREATE INDEX IX_OfferRequests_ClientId ON OfferRequests(ClientId)
CREATE INDEX IX_OfferRequests_RequestedBy ON OfferRequests(RequestedBy)
CREATE INDEX IX_OfferRequests_Status ON OfferRequests(Status)
CREATE INDEX IX_OfferRequests_CreatedOfferId ON OfferRequests(CreatedOfferId)
CREATE INDEX IX_OfferRequests_TaskProgressId ON OfferRequests(TaskProgressId)
GO

PRINT 'Created indexes on OfferRequests'

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
WHERE TABLE_NAME = 'OfferRequests' AND COLUMN_NAME = 'Id'
UNION ALL
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

PRINT 'OfferRequests table recreation completed successfully!'
