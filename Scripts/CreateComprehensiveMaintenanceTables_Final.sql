-- =============================================
-- Comprehensive Maintenance Module Database Schema (Final)
-- =============================================
-- This script creates additional tables for the comprehensive maintenance module
-- that work with the existing database schema
-- Execute this script on the itiwebapi44 database

-- =============================================
-- 1. Add missing columns to existing tables FIRST
-- =============================================

-- Add missing columns to Equipment table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Equipment') AND name = 'SerialNumber')
BEGIN
    ALTER TABLE Equipment ADD SerialNumber NVARCHAR(100) NULL;
    PRINT 'Added SerialNumber column to Equipment table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Equipment') AND name = 'InstallationDate')
BEGIN
    ALTER TABLE Equipment ADD InstallationDate DATETIME2 NULL;
    PRINT 'Added InstallationDate column to Equipment table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Equipment') AND name = 'Location')
BEGIN
    ALTER TABLE Equipment ADD Location NVARCHAR(200) NULL;
    PRINT 'Added Location column to Equipment table';
END

-- Add missing columns to MaintenanceVisits table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'VisitType')
BEGIN
    ALTER TABLE MaintenanceVisits ADD VisitType INT NULL;
    PRINT 'Added VisitType column to MaintenanceVisits table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'CompletionDate')
BEGIN
    ALTER TABLE MaintenanceVisits ADD CompletionDate DATETIME2 NULL;
    PRINT 'Added CompletionDate column to MaintenanceVisits table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE MaintenanceVisits ADD CreatedAt DATETIME2 NULL;
    PRINT 'Added CreatedAt column to MaintenanceVisits table';
END

-- Update the new columns with existing data
UPDATE Equipment SET SerialNumber = QRCode WHERE SerialNumber IS NULL AND QRCode IS NOT NULL;
UPDATE Equipment SET InstallationDate = PurchaseDate WHERE InstallationDate IS NULL AND PurchaseDate IS NOT NULL;
UPDATE MaintenanceVisits SET CompletionDate = CompletedAt WHERE CompletionDate IS NULL AND CompletedAt IS NOT NULL;
UPDATE MaintenanceVisits SET CreatedAt = StartedAt WHERE CreatedAt IS NULL AND StartedAt IS NOT NULL;

PRINT 'Updated new columns with existing data';

-- =============================================
-- 2. Maintenance Contracts Table (New)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MaintenanceContracts')
BEGIN
    CREATE TABLE MaintenanceContracts (
        Id NVARCHAR(450) NOT NULL PRIMARY KEY,
        ContractNumber NVARCHAR(50) NOT NULL,
        ClientId NVARCHAR(450) NOT NULL,
        StartDate DATETIME2 NOT NULL,
        EndDate DATETIME2 NOT NULL,
        ContractValue DECIMAL(18,2) NOT NULL,
        Status INT NOT NULL, -- ContractStatus enum
        ContractType NVARCHAR(100) NULL,
        PaymentTerms NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        
        CONSTRAINT FK_MaintenanceContracts_Clients FOREIGN KEY (ClientId) REFERENCES Clients(Id),
        CONSTRAINT UQ_MaintenanceContracts_ContractNumber UNIQUE (ContractNumber)
    );
    
    CREATE INDEX IX_MaintenanceContracts_ClientId_Status ON MaintenanceContracts(ClientId, Status);
    PRINT 'Created MaintenanceContracts table';
END

-- =============================================
-- 3. Contract Items Table (New)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ContractItems')
BEGIN
    CREATE TABLE ContractItems (
        Id NVARCHAR(450) NOT NULL PRIMARY KEY,
        ContractId NVARCHAR(450) NOT NULL,
        EquipmentId INT NOT NULL, -- References existing Equipment table (int Id)
        ServiceType NVARCHAR(100) NOT NULL,
        Frequency NVARCHAR(50) NULL,
        Price DECIMAL(18,2) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_ContractItems_MaintenanceContracts FOREIGN KEY (ContractId) REFERENCES MaintenanceContracts(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ContractItems_Equipment FOREIGN KEY (EquipmentId) REFERENCES Equipment(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_ContractItems_ContractId_EquipmentId UNIQUE (ContractId, EquipmentId)
    );
    PRINT 'Created ContractItems table';
END

-- =============================================
-- 4. Enhanced Visit Reports Table (New)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EnhancedVisitReports')
BEGIN
    CREATE TABLE EnhancedVisitReports (
        Id NVARCHAR(450) NOT NULL PRIMARY KEY,
        VisitId INT NOT NULL, -- References existing MaintenanceVisits table (int Id)
        Findings NVARCHAR(2000) NULL,
        Actions NVARCHAR(2000) NULL,
        Recommendations NVARCHAR(2000) NULL,
        CustomerFeedback NVARCHAR(1000) NULL,
        NextVisitNotes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        
        CONSTRAINT FK_EnhancedVisitReports_MaintenanceVisits FOREIGN KEY (VisitId) REFERENCES MaintenanceVisits(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_EnhancedVisitReports_VisitId ON EnhancedVisitReports(VisitId);
    PRINT 'Created EnhancedVisitReports table';
END

-- =============================================
-- 5. Media Files Table (New)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MediaFiles')
BEGIN
    CREATE TABLE MediaFiles (
        Id NVARCHAR(450) NOT NULL PRIMARY KEY,
        VisitId INT NOT NULL, -- References existing MaintenanceVisits table (int Id)
        FileName NVARCHAR(200) NOT NULL,
        FilePath NVARCHAR(500) NOT NULL,
        FileType NVARCHAR(50) NULL,
        FileSize BIGINT NOT NULL,
        Description NVARCHAR(200) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_MediaFiles_MaintenanceVisits FOREIGN KEY (VisitId) REFERENCES MaintenanceVisits(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_MediaFiles_VisitId_FileType ON MediaFiles(VisitId, FileType);
    PRINT 'Created MediaFiles table';
END

-- =============================================
-- 6. Spare Parts Table (New)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SpareParts')
BEGIN
    CREATE TABLE SpareParts (
        Id NVARCHAR(450) NOT NULL PRIMARY KEY,
        PartNumber NVARCHAR(100) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Manufacturer NVARCHAR(100) NULL,
        Category NVARCHAR(50) NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        StockQuantity INT NOT NULL DEFAULT 0,
        MinStockLevel INT NOT NULL DEFAULT 0,
        Description NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        
        CONSTRAINT UQ_SpareParts_PartNumber UNIQUE (PartNumber)
    );
    
    CREATE INDEX IX_SpareParts_Category ON SpareParts(Category);
    PRINT 'Created SpareParts table';
END

-- =============================================
-- 7. Used Spare Parts Table (New)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UsedSpareParts')
BEGIN
    CREATE TABLE UsedSpareParts (
        Id NVARCHAR(450) NOT NULL PRIMARY KEY,
        VisitId INT NOT NULL, -- References existing MaintenanceVisits table (int Id)
        SparePartId NVARCHAR(450) NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        TotalPrice DECIMAL(18,2) NOT NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_UsedSpareParts_MaintenanceVisits FOREIGN KEY (VisitId) REFERENCES MaintenanceVisits(Id) ON DELETE CASCADE,
        CONSTRAINT FK_UsedSpareParts_SpareParts FOREIGN KEY (SparePartId) REFERENCES SpareParts(Id)
    );
    PRINT 'Created UsedSpareParts table';
END

-- =============================================
-- 8. Invoices Table (New)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MaintenanceInvoices')
BEGIN
    CREATE TABLE MaintenanceInvoices (
        Id NVARCHAR(450) NOT NULL PRIMARY KEY,
        InvoiceNumber NVARCHAR(50) NOT NULL,
        ClientId NVARCHAR(450) NOT NULL,
        InvoiceDate DATETIME2 NOT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL,
        TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Status INT NOT NULL, -- InvoiceStatus enum
        Notes NVARCHAR(2000) NULL,
        DueDate DATETIME2 NULL,
        PaidDate DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        
        CONSTRAINT FK_MaintenanceInvoices_Clients FOREIGN KEY (ClientId) REFERENCES Clients(Id),
        CONSTRAINT UQ_MaintenanceInvoices_InvoiceNumber UNIQUE (InvoiceNumber)
    );
    
    CREATE INDEX IX_MaintenanceInvoices_ClientId_Status ON MaintenanceInvoices(ClientId, Status);
    PRINT 'Created MaintenanceInvoices table';
END

-- =============================================
-- 9. Invoice Items Table (New)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MaintenanceInvoiceItems')
BEGIN
    CREATE TABLE MaintenanceInvoiceItems (
        Id NVARCHAR(450) NOT NULL PRIMARY KEY,
        InvoiceId NVARCHAR(450) NOT NULL,
        Description NVARCHAR(200) NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        TotalPrice DECIMAL(18,2) NOT NULL,
        ItemType NVARCHAR(50) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_MaintenanceInvoiceItems_Invoices FOREIGN KEY (InvoiceId) REFERENCES MaintenanceInvoices(Id) ON DELETE CASCADE
    );
    PRINT 'Created MaintenanceInvoiceItems table';
END

-- =============================================
-- 10. Payments Table (New)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MaintenancePayments')
BEGIN
    CREATE TABLE MaintenancePayments (
        Id NVARCHAR(450) NOT NULL PRIMARY KEY,
        InvoiceId NVARCHAR(450) NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        PaymentDate DATETIME2 NOT NULL,
        PaymentMethod NVARCHAR(50) NOT NULL,
        ReferenceNumber NVARCHAR(200) NULL,
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT FK_MaintenancePayments_Invoices FOREIGN KEY (InvoiceId) REFERENCES MaintenanceInvoices(Id)
    );
    
    CREATE INDEX IX_MaintenancePayments_InvoiceId_PaymentDate ON MaintenancePayments(InvoiceId, PaymentDate);
    PRINT 'Created MaintenancePayments table';
END

-- =============================================
-- 11. Create stored procedures for common operations
-- =============================================

-- Get customer equipment and visits (Now works with updated schema)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCustomerEquipmentVisits]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[GetCustomerEquipmentVisits]
GO

CREATE PROCEDURE [dbo].[GetCustomerEquipmentVisits]
    @CustomerId NVARCHAR(450),
    @IncludeLegacy BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get customer info from existing Clients table
    SELECT Id, Name, Phone, Email, Address, CreatedAt
    FROM Clients
    WHERE Id = @CustomerId;
    
    -- Get equipment from existing Equipment table (now has Location, SerialNumber, InstallationDate)
    SELECT Id, Name, Model, Manufacturer, InstallationDate, WarrantyExpiry, 
           Status as EquipmentStatus, Location, SerialNumber, CreatedAt
    FROM Equipment
    WHERE CustomerId = @CustomerId;
    
    -- Get visits from existing MaintenanceVisits table (now has VisitType, CompletionDate, CreatedAt)
    SELECT v.Id, v.DeviceId as EquipmentId, v.ScheduledDate as VisitDate, v.VisitType, v.Status, 
           v.CreatedAt, v.CompletionDate, e.Name as EquipmentName, e.SerialNumber as EquipmentSerialNumber
    FROM MaintenanceVisits v
    INNER JOIN Equipment e ON v.DeviceId = e.Id
    WHERE v.CustomerId = @CustomerId;
END
GO
PRINT 'Created GetCustomerEquipmentVisits stored procedure';

-- Get maintenance dashboard statistics (Now works with updated schema)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMaintenanceDashboardStats]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[GetMaintenanceDashboardStats]
GO

CREATE PROCEDURE [dbo].[GetMaintenanceDashboardStats]
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Now DATETIME2 = GETUTCDATE();
    DECLARE @StartOfMonth DATETIME2 = DATEFROMPARTS(YEAR(@Now), MONTH(@Now), 1);
    DECLARE @EndOfMonth DATETIME2 = DATEADD(DAY, -1, DATEADD(MONTH, 1, @StartOfMonth));
    
    SELECT 
        (SELECT COUNT(*) FROM Clients) AS TotalCustomers,
        (SELECT COUNT(*) FROM Equipment) AS TotalEquipment,
        (SELECT COUNT(*) FROM MaintenanceVisits) AS TotalVisits,
        (SELECT COUNT(*) FROM MaintenanceVisits WHERE ScheduledDate >= @StartOfMonth AND ScheduledDate <= @EndOfMonth) AS MonthlyVisits,
        (SELECT COUNT(*) FROM MaintenanceVisits WHERE Status = 1) AS PendingVisits, -- PendingApproval
        (SELECT COUNT(*) FROM MaintenanceVisits WHERE Status = 5) AS CompletedVisits, -- Completed
        (SELECT COUNT(*) FROM MaintenanceContracts WHERE Status = 2 AND EndDate >= @Now) AS ActiveContracts,
        CASE 
            WHEN (SELECT COUNT(*) FROM MaintenanceVisits) > 0 
            THEN CAST((SELECT COUNT(*) FROM MaintenanceVisits WHERE Status = 5) * 100.0 / (SELECT COUNT(*) FROM MaintenanceVisits) AS DECIMAL(10,2))
            ELSE 0 
        END AS VisitCompletionRate;
END
GO
PRINT 'Created GetMaintenanceDashboardStats stored procedure';

-- =============================================
-- 12. Create sample data for testing (optional)
-- =============================================
-- Uncomment the following section to create sample data
/*
-- Sample Maintenance Contract
INSERT INTO MaintenanceContracts (Id, ContractNumber, ClientId, StartDate, EndDate, ContractValue, Status, ContractType)
VALUES (NEWID(), 'MC-2024-001', (SELECT TOP 1 Id FROM Clients), '2024-01-01', '2024-12-31', 12000.00, 2, 'Annual');

-- Sample Spare Parts
INSERT INTO SpareParts (Id, PartNumber, Name, Manufacturer, Category, UnitPrice, StockQuantity, MinStockLevel)
VALUES 
    (NEWID(), 'SP-001', 'Medical Grade Filter', 'MedTech', 'Filters', 150.00, 50, 10),
    (NEWID(), 'SP-002', 'X-Ray Tube', 'RadiologyCorp', 'Imaging', 2500.00, 5, 2),
    (NEWID(), 'SP-003', 'Patient Monitor Cable', 'HealthTech', 'Cables', 75.00, 25, 5);
*/

PRINT '===========================================';
PRINT 'Comprehensive Maintenance Module database schema created successfully!';
PRINT 'All missing columns have been added to existing tables.';
PRINT 'New tables have been created successfully.';
PRINT 'Stored procedures have been updated to work with the new schema.';
PRINT '===========================================';
