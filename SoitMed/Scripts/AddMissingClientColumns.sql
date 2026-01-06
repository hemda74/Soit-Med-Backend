-- ============================================================================
-- Add Missing Client Columns (LegacyCustomerId and RelatedUserId)
-- ============================================================================
-- This script adds the missing columns to the Clients table that are
-- required by the Entity Framework model but may not exist in the database.
-- ============================================================================

BEGIN TRANSACTION;

BEGIN TRY
    -- Add LegacyCustomerId column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'LegacyCustomerId')
    BEGIN
        ALTER TABLE Clients
        ADD LegacyCustomerId INT NULL;
        
        PRINT 'Added LegacyCustomerId column to Clients table';
    END
    ELSE
    BEGIN
        PRINT 'LegacyCustomerId column already exists in Clients table';
    END

    -- Add RelatedUserId column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'RelatedUserId')
    BEGIN
        ALTER TABLE Clients
        ADD RelatedUserId NVARCHAR(450) NULL;
        
        PRINT 'Added RelatedUserId column to Clients table';
    END
    ELSE
    BEGIN
        PRINT 'RelatedUserId column already exists in Clients table';
    END

    -- Create index on RelatedUserId if it doesn't exist
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'RelatedUserId')
        AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clients_RelatedUserId' AND object_id = OBJECT_ID('Clients'))
    BEGIN
        CREATE INDEX IX_Clients_RelatedUserId ON Clients(RelatedUserId);
        PRINT 'Created index IX_Clients_RelatedUserId';
    END

    -- Add foreign key constraint if it doesn't exist
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'RelatedUserId')
        AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Clients_AspNetUsers_RelatedUserId')
    BEGIN
        ALTER TABLE Clients
        ADD CONSTRAINT FK_Clients_AspNetUsers_RelatedUserId 
        FOREIGN KEY (RelatedUserId) 
        REFERENCES AspNetUsers(Id) 
        ON DELETE SET NULL;
        
        PRINT 'Added foreign key constraint FK_Clients_AspNetUsers_RelatedUserId';
    END

    COMMIT TRANSACTION;
    PRINT 'Migration completed successfully!';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error: ' + ERROR_MESSAGE();
    THROW;
END CATCH;

