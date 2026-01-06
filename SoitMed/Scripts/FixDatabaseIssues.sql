-- ============================================================================
-- Fix Database Issues - Complete SQL Script
-- ============================================================================
-- This script fixes all database issues:
-- 1. Adds missing Client columns (LegacyCustomerId, RelatedUserId)
-- 2. Fixes Contracts table foreign key cascade issues
-- 3. Creates Contracts table if it doesn't exist
-- ============================================================================

BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Starting database fixes...';
    
    -- ============================================================================
    -- 1. Add Missing Client Columns
    -- ============================================================================
    
    -- Add LegacyCustomerId column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'LegacyCustomerId')
    BEGIN
        ALTER TABLE Clients
        ADD LegacyCustomerId INT NULL;
        
        PRINT '✓ Added LegacyCustomerId column to Clients table';
    END
    ELSE
    BEGIN
        PRINT '✓ LegacyCustomerId column already exists in Clients table';
    END

    -- Add RelatedUserId column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'RelatedUserId')
    BEGIN
        ALTER TABLE Clients
        ADD RelatedUserId NVARCHAR(450) NULL;
        
        PRINT '✓ Added RelatedUserId column to Clients table';
    END
    ELSE
    BEGIN
        PRINT '✓ RelatedUserId column already exists in Clients table';
    END

    -- Create index on RelatedUserId if it doesn't exist
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'RelatedUserId')
        AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clients_RelatedUserId' AND object_id = OBJECT_ID('Clients'))
    BEGIN
        CREATE INDEX IX_Clients_RelatedUserId ON Clients(RelatedUserId);
        PRINT '✓ Created index IX_Clients_RelatedUserId';
    END

    -- Add foreign key constraint if it doesn't exist
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'RelatedUserId')
        AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Clients_AspNetUsers_RelatedUserId')
    BEGIN
        ALTER TABLE Clients
        ADD CONSTRAINT FK_Clients_AspNetUsers_RelatedUserId 
        FOREIGN KEY (RelatedUserId) 
        REFERENCES AspNetUsers(Id) 
        ON DELETE NO ACTION;
        
        PRINT '✓ Added foreign key constraint FK_Clients_AspNetUsers_RelatedUserId';
    END

    -- ============================================================================
    -- 2. Fix Contracts Table (if exists, drop and recreate with correct constraints)
    -- ============================================================================
    
    -- Drop dependent tables first (in reverse order of dependencies)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentProofs')
    BEGIN
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PaymentProofs_InstallmentSchedules_InstallmentScheduleId')
        BEGIN
            ALTER TABLE PaymentProofs DROP CONSTRAINT FK_PaymentProofs_InstallmentSchedules_InstallmentScheduleId;
        END
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PaymentProofs_AspNetUsers_UploadedBy')
        BEGIN
            ALTER TABLE PaymentProofs DROP CONSTRAINT FK_PaymentProofs_AspNetUsers_UploadedBy;
        END
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PaymentProofs_AspNetUsers_VerifiedBy')
        BEGIN
            ALTER TABLE PaymentProofs DROP CONSTRAINT FK_PaymentProofs_AspNetUsers_VerifiedBy;
        END
        DROP TABLE PaymentProofs;
        PRINT '✓ Dropped PaymentProofs table';
    END
    
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'InstallmentSchedules')
    BEGIN
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_InstallmentSchedules_Contracts_ContractId')
        BEGIN
            ALTER TABLE InstallmentSchedules DROP CONSTRAINT FK_InstallmentSchedules_Contracts_ContractId;
        END
        DROP TABLE InstallmentSchedules;
        PRINT '✓ Dropped InstallmentSchedules table';
    END
    
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ContractNegotiations')
    BEGIN
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ContractNegotiations_Contracts_ContractId')
        BEGIN
            ALTER TABLE ContractNegotiations DROP CONSTRAINT FK_ContractNegotiations_Contracts_ContractId;
        END
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ContractNegotiations_AspNetUsers_SubmittedBy')
        BEGIN
            ALTER TABLE ContractNegotiations DROP CONSTRAINT FK_ContractNegotiations_AspNetUsers_SubmittedBy;
        END
        DROP TABLE ContractNegotiations;
        PRINT '✓ Dropped ContractNegotiations table';
    END
    
    -- Drop Contracts table if it exists (to fix cascade path issues)
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Contracts')
    BEGIN
        -- Drop foreign keys first
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Contracts_AspNetUsers_CustomerSignedBy')
        BEGIN
            ALTER TABLE Contracts DROP CONSTRAINT FK_Contracts_AspNetUsers_CustomerSignedBy;
        END
        
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Contracts_AspNetUsers_DraftedBy')
        BEGIN
            ALTER TABLE Contracts DROP CONSTRAINT FK_Contracts_AspNetUsers_DraftedBy;
        END
        
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Contracts_AspNetUsers_FinancialConfiguredBy')
        BEGIN
            ALTER TABLE Contracts DROP CONSTRAINT FK_Contracts_AspNetUsers_FinancialConfiguredBy;
        END
        
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Contracts_Clients_ClientId')
        BEGIN
            ALTER TABLE Contracts DROP CONSTRAINT FK_Contracts_Clients_ClientId;
        END
        
        IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Contracts_SalesDeals_DealId')
        BEGIN
            ALTER TABLE Contracts DROP CONSTRAINT FK_Contracts_SalesDeals_DealId;
        END
        
        -- Drop indexes
        IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Contracts_DealId_Status' AND object_id = OBJECT_ID('Contracts'))
        BEGIN
            DROP INDEX IX_Contracts_DealId_Status ON Contracts;
        END
        IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Contracts_Status_CreatedAt' AND object_id = OBJECT_ID('Contracts'))
        BEGIN
            DROP INDEX IX_Contracts_Status_CreatedAt ON Contracts;
        END
        IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Contracts_ContractNumber' AND object_id = OBJECT_ID('Contracts'))
        BEGIN
            DROP INDEX IX_Contracts_ContractNumber ON Contracts;
        END
        IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Contracts_ClientId' AND object_id = OBJECT_ID('Contracts'))
        BEGIN
            DROP INDEX IX_Contracts_ClientId ON Contracts;
        END
        IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Contracts_CustomerSignedBy' AND object_id = OBJECT_ID('Contracts'))
        BEGIN
            DROP INDEX IX_Contracts_CustomerSignedBy ON Contracts;
        END
        IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Contracts_DraftedBy' AND object_id = OBJECT_ID('Contracts'))
        BEGIN
            DROP INDEX IX_Contracts_DraftedBy ON Contracts;
        END
        IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Contracts_FinancialConfiguredBy' AND object_id = OBJECT_ID('Contracts'))
        BEGIN
            DROP INDEX IX_Contracts_FinancialConfiguredBy ON Contracts;
        END
        
        DROP TABLE Contracts;
        PRINT '✓ Dropped existing Contracts table';
    END

    -- Create Contracts table with correct constraints (NO ACTION to avoid cascade paths)
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Contracts')
    BEGIN
        CREATE TABLE Contracts (
            Id BIGINT NOT NULL IDENTITY(1,1),
            DealId BIGINT NOT NULL,
            ContractNumber NVARCHAR(100) NOT NULL,
            Title NVARCHAR(200) NOT NULL,
            ContractContent NVARCHAR(MAX) NULL,
            DocumentUrl NVARCHAR(500) NULL,
            Status INT NOT NULL,
            DraftedAt DATETIME2 NULL,
            SentToCustomerAt DATETIME2 NULL,
            SignedAt DATETIME2 NULL,
            CancelledAt DATETIME2 NULL,
            CancellationReason NVARCHAR(2000) NULL,
            DraftedBy NVARCHAR(450) NOT NULL,
            LastReviewedBy NVARCHAR(MAX) NULL,
            LastReviewedAt DATETIME2 NULL,
            ClientId BIGINT NOT NULL,
            CustomerSignedBy NVARCHAR(450) NULL,
            CustomerSignedAt DATETIME2 NULL,
            CashAmount DECIMAL(18,2) NULL,
            InstallmentAmount DECIMAL(18,2) NULL,
            InterestRate DECIMAL(5,2) NULL,
            LatePenaltyRate DECIMAL(5,2) NULL,
            InstallmentDurationMonths INT NULL,
            FinancialConfigurationCompletedAt DATETIME2 NULL,
            FinancialConfiguredBy NVARCHAR(450) NULL,
            CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            CONSTRAINT PK_Contracts PRIMARY KEY (Id)
        );
        
        PRINT '✓ Created Contracts table';
        
        -- Add foreign keys with NO ACTION to avoid cascade path issues
        ALTER TABLE Contracts
        ADD CONSTRAINT FK_Contracts_AspNetUsers_CustomerSignedBy 
        FOREIGN KEY (CustomerSignedBy) 
        REFERENCES AspNetUsers(Id) 
        ON DELETE NO ACTION;
        
        ALTER TABLE Contracts
        ADD CONSTRAINT FK_Contracts_AspNetUsers_DraftedBy 
        FOREIGN KEY (DraftedBy) 
        REFERENCES AspNetUsers(Id) 
        ON DELETE NO ACTION;
        
        ALTER TABLE Contracts
        ADD CONSTRAINT FK_Contracts_AspNetUsers_FinancialConfiguredBy 
        FOREIGN KEY (FinancialConfiguredBy) 
        REFERENCES AspNetUsers(Id) 
        ON DELETE NO ACTION;
        
        ALTER TABLE Contracts
        ADD CONSTRAINT FK_Contracts_Clients_ClientId 
        FOREIGN KEY (ClientId) 
        REFERENCES Clients(Id) 
        ON DELETE NO ACTION;
        
        ALTER TABLE Contracts
        ADD CONSTRAINT FK_Contracts_SalesDeals_DealId 
        FOREIGN KEY (DealId) 
        REFERENCES SalesDeals(Id) 
        ON DELETE NO ACTION;
        
        -- Create indexes
        CREATE INDEX IX_Contracts_DealId ON Contracts(DealId);
        CREATE INDEX IX_Contracts_ClientId ON Contracts(ClientId);
        CREATE INDEX IX_Contracts_Status ON Contracts(Status);
        CREATE UNIQUE INDEX IX_Contracts_ContractNumber ON Contracts(ContractNumber);
        CREATE INDEX IX_Contracts_DealId_Status ON Contracts(DealId, Status);
        CREATE INDEX IX_Contracts_Status_CreatedAt ON Contracts(Status, CreatedAt);
        CREATE INDEX IX_Contracts_CustomerSignedBy ON Contracts(CustomerSignedBy);
        CREATE INDEX IX_Contracts_DraftedBy ON Contracts(DraftedBy);
        CREATE INDEX IX_Contracts_FinancialConfiguredBy ON Contracts(FinancialConfiguredBy);
        
        PRINT '✓ Added foreign keys and indexes to Contracts table';
    END

    -- ============================================================================
    -- 3. Create ContractNegotiations table if it doesn't exist
    -- ============================================================================
    
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ContractNegotiations')
    BEGIN
        CREATE TABLE ContractNegotiations (
            Id BIGINT NOT NULL IDENTITY(1,1),
            ContractId BIGINT NOT NULL,
            ActionType NVARCHAR(50) NOT NULL,
            Notes NVARCHAR(MAX) NOT NULL,
            SubmittedBy NVARCHAR(450) NOT NULL,
            SubmitterRole NVARCHAR(50) NULL,
            Attachments NVARCHAR(2000) NULL,
            SubmittedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            CONSTRAINT PK_ContractNegotiations PRIMARY KEY (Id),
            CONSTRAINT FK_ContractNegotiations_Contracts_ContractId 
            FOREIGN KEY (ContractId) 
            REFERENCES Contracts(Id) 
            ON DELETE CASCADE,
            CONSTRAINT FK_ContractNegotiations_AspNetUsers_SubmittedBy 
            FOREIGN KEY (SubmittedBy) 
            REFERENCES AspNetUsers(Id) 
            ON DELETE NO ACTION
        );
        
        CREATE INDEX IX_ContractNegotiations_ContractId_SubmittedAt ON ContractNegotiations(ContractId, SubmittedAt);
        CREATE INDEX IX_ContractNegotiations_SubmittedBy ON ContractNegotiations(SubmittedBy);
        
        PRINT '✓ Created ContractNegotiations table';
    END

    -- ============================================================================
    -- 4. Create InstallmentSchedules table if it doesn't exist
    -- ============================================================================
    
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'InstallmentSchedules')
    BEGIN
        CREATE TABLE InstallmentSchedules (
            Id BIGINT NOT NULL IDENTITY(1,1),
            ContractId BIGINT NOT NULL,
            InstallmentNumber INT NOT NULL,
            Amount DECIMAL(18,2) NOT NULL,
            DueDate DATETIME2 NOT NULL,
            InterestAmount DECIMAL(18,2) NULL,
            LatePenaltyAmount DECIMAL(18,2) NULL,
            Status INT NOT NULL,
            PaidAt DATETIME2 NULL,
            PaymentNotes NVARCHAR(2000) NULL,
            NotificationSent7Days BIT NOT NULL DEFAULT 0,
            NotificationSent2Days BIT NOT NULL DEFAULT 0,
            NotificationSent1Day BIT NOT NULL DEFAULT 0,
            OverdueNotificationSent BIT NOT NULL DEFAULT 0,
            LastReminderSentAt DATETIME2 NULL,
            CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            CONSTRAINT PK_InstallmentSchedules PRIMARY KEY (Id),
            CONSTRAINT FK_InstallmentSchedules_Contracts_ContractId 
            FOREIGN KEY (ContractId) 
            REFERENCES Contracts(Id) 
            ON DELETE CASCADE
        );
        
        CREATE UNIQUE INDEX IX_InstallmentSchedules_ContractId_InstallmentNumber ON InstallmentSchedules(ContractId, InstallmentNumber);
        CREATE INDEX IX_InstallmentSchedules_DueDate ON InstallmentSchedules(DueDate);
        CREATE INDEX IX_InstallmentSchedules_Status_DueDate ON InstallmentSchedules(Status, DueDate);
        
        PRINT '✓ Created InstallmentSchedules table';
    END

    -- ============================================================================
    -- 5. Create PaymentProofs table if it doesn't exist
    -- ============================================================================
    
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentProofs')
    BEGIN
        CREATE TABLE PaymentProofs (
            Id BIGINT NOT NULL IDENTITY(1,1),
            InstallmentScheduleId BIGINT NOT NULL,
            PaymentMethod INT NOT NULL,
            ProofImageUrl NVARCHAR(500) NOT NULL,
            TransactionReference NVARCHAR(200) NULL,
            Notes NVARCHAR(2000) NULL,
            Status INT NOT NULL,
            UploadedBy NVARCHAR(450) NOT NULL,
            VerifiedAt DATETIME2 NULL,
            VerifiedBy NVARCHAR(450) NULL,
            VerificationNotes NVARCHAR(2000) NULL,
            RejectionReason NVARCHAR(2000) NULL,
            CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            CONSTRAINT PK_PaymentProofs PRIMARY KEY (Id),
            CONSTRAINT FK_PaymentProofs_InstallmentSchedules_InstallmentScheduleId 
            FOREIGN KEY (InstallmentScheduleId) 
            REFERENCES InstallmentSchedules(Id) 
            ON DELETE CASCADE,
            CONSTRAINT FK_PaymentProofs_AspNetUsers_UploadedBy 
            FOREIGN KEY (UploadedBy) 
            REFERENCES AspNetUsers(Id) 
            ON DELETE NO ACTION,
            CONSTRAINT FK_PaymentProofs_AspNetUsers_VerifiedBy 
            FOREIGN KEY (VerifiedBy) 
            REFERENCES AspNetUsers(Id) 
            ON DELETE NO ACTION
        );
        
        CREATE INDEX IX_PaymentProofs_InstallmentScheduleId_Status ON PaymentProofs(InstallmentScheduleId, Status);
        CREATE INDEX IX_PaymentProofs_UploadedBy ON PaymentProofs(UploadedBy);
        CREATE INDEX IX_PaymentProofs_VerifiedBy ON PaymentProofs(VerifiedBy);
        
        PRINT '✓ Created PaymentProofs table';
    END

    -- ============================================================================
    -- 6. Mark migrations as applied (if needed)
    -- ============================================================================
    
    -- Mark AddLegacyAndQrSupport as applied if not already
    IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260101115621_AddLegacyAndQrSupport')
    BEGIN
        INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
        VALUES ('20260101115621_AddLegacyAndQrSupport', '10.0.0');
        PRINT '✓ Marked migration 20260101115621_AddLegacyAndQrSupport as applied';
    END

    -- Mark FixMissingClientColumns as applied if not already
    IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260104102945_FixMissingClientColumns')
    BEGIN
        INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
        VALUES ('20260104102945_FixMissingClientColumns', '10.0.0');
        PRINT '✓ Marked migration 20260104102945_FixMissingClientColumns as applied';
    END

    -- Mark AddContractLifecycleModule as applied if not already
    IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260104114004_AddContractLifecycleModule')
    BEGIN
        INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
        VALUES ('20260104114004_AddContractLifecycleModule', '10.0.0');
        PRINT '✓ Marked migration 20260104114004_AddContractLifecycleModule as applied';
    END

    COMMIT TRANSACTION;
    PRINT '';
    PRINT '========================================';
    PRINT '✓ All database fixes completed successfully!';
    PRINT '========================================';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '========================================';
    PRINT '✗ Error occurred:';
    PRINT ERROR_MESSAGE();
    PRINT '========================================';
    THROW;
END CATCH;

