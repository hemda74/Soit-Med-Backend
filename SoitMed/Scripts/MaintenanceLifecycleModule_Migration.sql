-- ============================================================================
-- Maintenance Lifecycle Module - Database Migration Script
-- ============================================================================
-- This script adds all necessary tables, columns, and indexes for the
-- Maintenance Lifecycle Module implementation.
--
-- IMPORTANT: Review and test this script in a development environment first!
-- ============================================================================

BEGIN TRANSACTION;

BEGIN TRY
    -- ============================================================================
    -- 1. Add new columns to MaintenanceVisits table
    -- ============================================================================

    -- Add TicketNumber (unique, auto-generated)
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'TicketNumber')
    BEGIN
        -- Step 1: Add column as nullable
        ALTER TABLE MaintenanceVisits
        ADD TicketNumber NVARCHAR(50) NULL;
    END
    
    -- Step 2: Generate ticket numbers for existing records (only if column exists)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'TicketNumber')
    BEGIN
        -- Use dynamic SQL to check and update in one go
        EXEC('
            IF EXISTS (SELECT TOP 1 1 FROM MaintenanceVisits WHERE TicketNumber IS NULL)
            BEGIN
                UPDATE MaintenanceVisits 
                SET TicketNumber = ''VISIT-'' + RIGHT(''000000'' + CAST(Id AS VARCHAR), 6) 
                WHERE TicketNumber IS NULL;
            END
        ');
    END
    
    -- Step 3: Make it NOT NULL if it's still nullable
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'TicketNumber' AND is_nullable = 1)
    BEGIN
        ALTER TABLE MaintenanceVisits
        ALTER COLUMN TicketNumber NVARCHAR(50) NOT NULL;
    END
    
    -- Step 4: Create unique index if it doesn't exist
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'TicketNumber')
        AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MaintenanceVisits_TicketNumber')
    BEGIN
        CREATE UNIQUE INDEX IX_MaintenanceVisits_TicketNumber ON MaintenanceVisits(TicketNumber);
    END

    -- Add CustomerId
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'CustomerId')
    BEGIN
        -- Step 1: Add column as nullable
        ALTER TABLE MaintenanceVisits
        ADD CustomerId NVARCHAR(450) NULL;
    END
    
    -- Step 2: Populate from MaintenanceRequest if possible (only if both tables and columns exist)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'CustomerId')
        AND EXISTS (SELECT * FROM sys.tables WHERE name = 'MaintenanceRequests')
        AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceRequests') AND name = 'CustomerId')
    BEGIN
        EXEC('
            IF EXISTS (SELECT TOP 1 1 FROM MaintenanceVisits mv INNER JOIN MaintenanceRequests mr ON mv.MaintenanceRequestId = mr.Id WHERE mv.CustomerId IS NULL)
            BEGIN
                UPDATE mv SET mv.CustomerId = mr.CustomerId 
                FROM MaintenanceVisits mv 
                INNER JOIN MaintenanceRequests mr ON mv.MaintenanceRequestId = mr.Id 
                WHERE mv.CustomerId IS NULL;
            END
        ');
    END
    
    -- Step 3: Add foreign key constraint (NO ACTION is default in SQL Server)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'CustomerId')
        AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MaintenanceVisits_CustomerId')
    BEGIN
        ALTER TABLE MaintenanceVisits
        ADD CONSTRAINT FK_MaintenanceVisits_CustomerId FOREIGN KEY (CustomerId)
        REFERENCES AspNetUsers(Id);
    END

    -- Add DeviceId
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'DeviceId')
    BEGIN
        -- Step 1: Add column as nullable
        ALTER TABLE MaintenanceVisits
        ADD DeviceId INT NULL;
    END
    
    -- Step 2: Populate from MaintenanceRequest if possible (only if both tables and columns exist)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'DeviceId')
        AND EXISTS (SELECT * FROM sys.tables WHERE name = 'MaintenanceRequests')
        AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceRequests') AND name = 'EquipmentId')
    BEGIN
        EXEC('
            IF EXISTS (SELECT TOP 1 1 FROM MaintenanceVisits mv INNER JOIN MaintenanceRequests mr ON mv.MaintenanceRequestId = mr.Id WHERE mv.DeviceId IS NULL)
            BEGIN
                UPDATE mv SET mv.DeviceId = mr.EquipmentId 
                FROM MaintenanceVisits mv 
                INNER JOIN MaintenanceRequests mr ON mv.MaintenanceRequestId = mr.Id 
                WHERE mv.DeviceId IS NULL;
            END
        ');
    END
    
    -- Step 3: Add foreign key constraint
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'DeviceId')
        AND EXISTS (SELECT * FROM sys.tables WHERE name = 'Equipment')
        AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MaintenanceVisits_DeviceId')
    BEGIN
        ALTER TABLE MaintenanceVisits
        ADD CONSTRAINT FK_MaintenanceVisits_DeviceId FOREIGN KEY (DeviceId)
        REFERENCES Equipment(Id);
    END

    -- Add ScheduledDate
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'ScheduledDate')
    BEGIN
        -- Step 1: Add column as nullable
        ALTER TABLE MaintenanceVisits
        ADD ScheduledDate DATETIME2 NULL;
    END
    
    -- Step 2: Populate from VisitDate for existing records (only if column exists)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'ScheduledDate')
        AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'VisitDate')
    BEGIN
        EXEC('
            IF EXISTS (SELECT TOP 1 1 FROM MaintenanceVisits WHERE ScheduledDate IS NULL)
            BEGIN
                UPDATE MaintenanceVisits SET ScheduledDate = VisitDate WHERE ScheduledDate IS NULL;
            END
        ');
    END
    
    -- Step 3: Make it NOT NULL if it's still nullable
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'ScheduledDate' AND is_nullable = 1)
    BEGIN
        ALTER TABLE MaintenanceVisits
        ALTER COLUMN ScheduledDate DATETIME2 NOT NULL;
    END

    -- Add Origin (VisitOrigin enum)
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'Origin')
    BEGIN
        ALTER TABLE MaintenanceVisits
        ADD Origin INT NOT NULL DEFAULT 2; -- Default to CallCenter for existing records
    END

    -- Add Status (VisitStatus enum) - keeping Outcome for backward compatibility
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'Status')
    BEGIN
        -- Step 1: Add column with default value
        ALTER TABLE MaintenanceVisits
        ADD Status INT NOT NULL DEFAULT 2; -- Default to Scheduled for existing records
    END
    
    -- Step 2: Map existing Outcome values to Status if possible (only if both columns exist)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'Status')
        AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'Outcome')
    BEGIN
        -- This is a simplified mapping - adjust based on your business logic
        EXEC('
            IF EXISTS (SELECT TOP 1 1 FROM MaintenanceVisits WHERE Status = 2)
            BEGIN
                UPDATE MaintenanceVisits 
                SET Status = CASE 
                    WHEN Outcome = 1 THEN 5 
                    WHEN Outcome = 2 THEN 6 
                    WHEN Outcome = 3 THEN 4 
                    WHEN Outcome = 4 THEN 7 
                    ELSE 2 
                END 
                WHERE Status = 2;
            END
        ');
    END

    -- Add ParentVisitId (self-reference)
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'ParentVisitId')
    BEGIN
        ALTER TABLE MaintenanceVisits
        ADD ParentVisitId INT NULL;
        
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_MaintenanceVisits_ParentVisitId')
        BEGIN
            ALTER TABLE MaintenanceVisits
            ADD CONSTRAINT FK_MaintenanceVisits_ParentVisitId FOREIGN KEY (ParentVisitId)
            REFERENCES MaintenanceVisits(Id);
        END
    END

    -- Add IsPaidVisit
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'IsPaidVisit')
    BEGIN
        ALTER TABLE MaintenanceVisits
        ADD IsPaidVisit BIT NOT NULL DEFAULT 0;
    END

    -- Add Cost
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'Cost')
    BEGIN
        ALTER TABLE MaintenanceVisits
        ADD Cost DECIMAL(18,2) NULL;
    END

    -- Add indexes for MaintenanceVisits (only if columns exist)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'Status')
        AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MaintenanceVisits_Status')
    BEGIN
        CREATE INDEX IX_MaintenanceVisits_Status ON MaintenanceVisits(Status);
    END

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'ScheduledDate')
        AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MaintenanceVisits_ScheduledDate')
    BEGIN
        CREATE INDEX IX_MaintenanceVisits_ScheduledDate ON MaintenanceVisits(ScheduledDate);
    END

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'CustomerId')
        AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'Status')
        AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MaintenanceVisits_CustomerId_Status')
    BEGIN
        CREATE INDEX IX_MaintenanceVisits_CustomerId_Status ON MaintenanceVisits(CustomerId, Status);
    END

    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'EngineerId')
        AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MaintenanceVisits') AND name = 'Status')
        AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MaintenanceVisits_EngineerId_Status')
    BEGIN
        CREATE INDEX IX_MaintenanceVisits_EngineerId_Status ON MaintenanceVisits(EngineerId, Status);
    END

    -- ============================================================================
    -- 2. Create VisitAssignees table (many-to-many)
    -- ============================================================================

    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'VisitAssignees')
    BEGIN
        CREATE TABLE VisitAssignees (
            VisitId INT NOT NULL,
            EngineerId NVARCHAR(450) NOT NULL,
            AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            AssignedById NVARCHAR(450) NULL,
            -- Use NONCLUSTERED primary key to avoid 900-byte limit (INT + NVARCHAR(450) = 904 bytes)
            CONSTRAINT PK_VisitAssignees PRIMARY KEY NONCLUSTERED (VisitId, EngineerId),
            CONSTRAINT FK_VisitAssignees_VisitId FOREIGN KEY (VisitId)
                REFERENCES MaintenanceVisits(Id) ON DELETE CASCADE,
            CONSTRAINT FK_VisitAssignees_EngineerId FOREIGN KEY (EngineerId)
                REFERENCES AspNetUsers(Id),
            CONSTRAINT FK_VisitAssignees_AssignedById FOREIGN KEY (AssignedById)
                REFERENCES AspNetUsers(Id)
        );
        
        -- Create clustered index on AssignedAt for better query performance
        CREATE CLUSTERED INDEX IX_VisitAssignees_AssignedAt ON VisitAssignees(AssignedAt);
        
        CREATE INDEX IX_VisitAssignees_VisitId ON VisitAssignees(VisitId);
        CREATE INDEX IX_VisitAssignees_EngineerId ON VisitAssignees(EngineerId);
        
        -- Migrate existing EngineerId assignments to VisitAssignees
        INSERT INTO VisitAssignees (VisitId, EngineerId, AssignedAt)
        SELECT Id, EngineerId, VisitDate
        FROM MaintenanceVisits
        WHERE EngineerId IS NOT NULL AND EngineerId != '';
    END
    ELSE
    BEGIN
        -- Table exists - check if primary key is clustered and needs to be changed
        -- If the PK is clustered and exceeds 900 bytes, we need to recreate it as non-clustered
        IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'PK_VisitAssignees' AND type_desc = 'CLUSTERED')
        BEGIN
            -- Drop the existing clustered PK
            ALTER TABLE VisitAssignees DROP CONSTRAINT PK_VisitAssignees;
            
            -- Recreate as non-clustered
            ALTER TABLE VisitAssignees
            ADD CONSTRAINT PK_VisitAssignees PRIMARY KEY NONCLUSTERED (VisitId, EngineerId);
            
            -- Create clustered index on AssignedAt if it doesn't exist
            IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_VisitAssignees_AssignedAt')
            BEGIN
                CREATE CLUSTERED INDEX IX_VisitAssignees_AssignedAt ON VisitAssignees(AssignedAt);
            END
        END
    END

    -- ============================================================================
    -- 3. Create VisitReports table (one-to-one)
    -- ============================================================================

    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'VisitReports')
    BEGIN
        CREATE TABLE VisitReports (
            Id INT IDENTITY(1,1) PRIMARY KEY,
            VisitId INT NOT NULL,
            ReportText NVARCHAR(MAX) NULL,
            MediaUrls NVARCHAR(MAX) NULL, -- JSON array
            CheckInTime DATETIME2 NULL,
            CheckOutTime DATETIME2 NULL,
            GPSCoordinates NVARCHAR(100) NULL,
            CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            UpdatedAt DATETIME2 NULL,
            CONSTRAINT FK_VisitReports_VisitId FOREIGN KEY (VisitId)
                REFERENCES MaintenanceVisits(Id) ON DELETE CASCADE
        );
        
        -- Create unique index on VisitId (one-to-one relationship)
        CREATE UNIQUE INDEX IX_VisitReports_VisitId ON VisitReports(VisitId);
        
        -- Migrate existing Report data to VisitReports
        INSERT INTO VisitReports (VisitId, ReportText, CreatedAt)
        SELECT Id, Report, VisitDate
        FROM MaintenanceVisits
        WHERE Report IS NOT NULL AND Report != '';
    END

    -- ============================================================================
    -- 4. Update SparePartRequests table
    -- ============================================================================

    -- Add VisitId (alternative to MaintenanceVisitId)
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SparePartRequests') AND name = 'VisitId')
    BEGIN
        ALTER TABLE SparePartRequests
        ADD VisitId INT NULL;
        
        -- Populate from MaintenanceVisitId if exists
        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SparePartRequests') AND name = 'MaintenanceVisitId')
        BEGIN
            EXEC('
                IF EXISTS (SELECT TOP 1 1 FROM SparePartRequests WHERE VisitId IS NULL AND MaintenanceVisitId IS NOT NULL)
                BEGIN
                    UPDATE SparePartRequests
                    SET VisitId = MaintenanceVisitId
                    WHERE VisitId IS NULL AND MaintenanceVisitId IS NOT NULL;
                END
            ');
        END
        
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_SparePartRequests_VisitId')
        BEGIN
            ALTER TABLE SparePartRequests
            ADD CONSTRAINT FK_SparePartRequests_VisitId FOREIGN KEY (VisitId)
            REFERENCES MaintenanceVisits(Id);
        END
    END

    -- Add PartId
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SparePartRequests') AND name = 'PartId')
    BEGIN
        ALTER TABLE SparePartRequests
        ADD PartId INT NULL;
        -- Note: Add FK constraint if you have a Parts/Products catalog table
    END

    -- ============================================================================
    -- 5. Update PaymentTransactions table
    -- ============================================================================

    -- Add VisitId
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentTransactions') AND name = 'VisitId')
    BEGIN
        ALTER TABLE PaymentTransactions
        ADD VisitId INT NULL;
        
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PaymentTransactions_VisitId')
        BEGIN
            ALTER TABLE PaymentTransactions
            ADD CONSTRAINT FK_PaymentTransactions_VisitId FOREIGN KEY (VisitId)
            REFERENCES MaintenanceVisits(Id);
        END
    END

    -- Add Method (PaymentMethod enum) - if column doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentTransactions') AND name = 'Method')
    BEGIN
        ALTER TABLE PaymentTransactions
        ADD Method INT NULL;
        
        -- Set default based on existing TransactionType if possible
        -- This is a simplified mapping - adjust based on your business logic
        EXEC('
            IF EXISTS (SELECT TOP 1 1 FROM PaymentTransactions WHERE Method IS NULL)
            BEGIN
                UPDATE PaymentTransactions
                SET Method = 1 -- Default to Cash
                WHERE Method IS NULL;
            END
        ');
        
        ALTER TABLE PaymentTransactions
        ALTER COLUMN Method INT NOT NULL;
    END

    -- Add CollectionDelegateId
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentTransactions') AND name = 'CollectionDelegateId')
    BEGIN
        ALTER TABLE PaymentTransactions
        ADD CollectionDelegateId NVARCHAR(450) NULL;
        
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PaymentTransactions_CollectionDelegateId')
        BEGIN
            ALTER TABLE PaymentTransactions
            ADD CONSTRAINT FK_PaymentTransactions_CollectionDelegateId FOREIGN KEY (CollectionDelegateId)
            REFERENCES AspNetUsers(Id);
        END
    END

    -- Add AccountsApproverId
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentTransactions') AND name = 'AccountsApproverId')
    BEGIN
        ALTER TABLE PaymentTransactions
        ADD AccountsApproverId NVARCHAR(450) NULL;
        
        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PaymentTransactions_AccountsApproverId')
        BEGIN
            ALTER TABLE PaymentTransactions
            ADD CONSTRAINT FK_PaymentTransactions_AccountsApproverId FOREIGN KEY (AccountsApproverId)
            REFERENCES AspNetUsers(Id);
        END
    END

    -- ============================================================================
    -- 6. Create EntityChangeLog table (audit log)
    -- ============================================================================

    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EntityChangeLogs')
    BEGIN
        CREATE TABLE EntityChangeLogs (
            Id INT IDENTITY(1,1) PRIMARY KEY,
            EntityName NVARCHAR(100) NOT NULL,
            EntityId INT NOT NULL,
            UserId NVARCHAR(450) NOT NULL,
            OldValue NVARCHAR(MAX) NULL, -- JSON
            NewValue NVARCHAR(MAX) NULL, -- JSON
            ChangeType NVARCHAR(50) NOT NULL, -- Created, Updated, Deleted
            ChangeDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
            ChangeDescription NVARCHAR(1000) NULL,
            CONSTRAINT FK_EntityChangeLogs_UserId FOREIGN KEY (UserId)
                REFERENCES AspNetUsers(Id)
        );
        
        CREATE INDEX IX_EntityChangeLogs_EntityName_EntityId ON EntityChangeLogs(EntityName, EntityId);
        CREATE INDEX IX_EntityChangeLogs_UserId ON EntityChangeLogs(UserId);
        CREATE INDEX IX_EntityChangeLogs_ChangeDate ON EntityChangeLogs(ChangeDate);
    END

    -- ============================================================================
    -- Migration Complete
    -- ============================================================================

    COMMIT TRANSACTION;
    
    PRINT 'Maintenance Lifecycle Module migration completed successfully!';
    PRINT 'Please review the changes and test thoroughly before deploying to production.';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorLine INT = ERROR_LINE();
    DECLARE @ErrorNumber INT = ERROR_NUMBER();
    
    PRINT 'Migration failed with error:';
    PRINT 'Error Number: ' + CAST(@ErrorNumber AS NVARCHAR(10));
    PRINT 'Error Line: ' + CAST(@ErrorLine AS NVARCHAR(10));
    PRINT 'Error Message: ' + @ErrorMessage;
    
    THROW;
END CATCH
