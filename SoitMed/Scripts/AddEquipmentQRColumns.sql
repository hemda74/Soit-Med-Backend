-- ============================================================================
-- Add Equipment QR Code Columns - SQL Script
-- ============================================================================
-- This script adds the missing QR code columns to the Equipment table:
-- - QrToken (uniqueidentifier, NOT NULL, default NEWID())
-- - IsQrPrinted (bit, default 0)
-- - QrLastPrintedDate (datetime2, nullable)
-- - LegacySourceId (nvarchar(100), nullable)
-- ============================================================================

BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Starting Equipment QR columns migration...';
    
    -- ============================================================================
    -- 1. Add QrToken column
    -- ============================================================================
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Equipment') AND name = 'QrToken')
    BEGIN
        -- Step 1: Add column as nullable first
        ALTER TABLE Equipment
        ADD QrToken UNIQUEIDENTIFIER NULL;
        
        PRINT '✓ Added QrToken column (nullable)';
        
        -- Step 2: Generate QrToken for existing records (using dynamic SQL to avoid parsing errors)
        EXEC('
            UPDATE Equipment
            SET QrToken = NEWID()
            WHERE QrToken IS NULL;
        ');
        
        PRINT '✓ Generated QrToken for existing records';
        
        -- Step 3: Make it NOT NULL
        ALTER TABLE Equipment
        ALTER COLUMN QrToken UNIQUEIDENTIFIER NOT NULL;
        
        PRINT '✓ Made QrToken NOT NULL';
    END
    ELSE
    BEGIN
        PRINT '✓ QrToken column already exists';
        
        -- Ensure existing records have QrToken (using dynamic SQL)
        EXEC('
            UPDATE Equipment
            SET QrToken = NEWID()
            WHERE QrToken IS NULL;
        ');
        
        PRINT '✓ Updated NULL QrToken values for existing records';
    END
    
    -- Create unique index on QrToken if it doesn't exist
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Equipment') AND name = 'QrToken')
        AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Equipment_QrToken' AND object_id = OBJECT_ID('Equipment'))
    BEGIN
        CREATE UNIQUE INDEX IX_Equipment_QrToken ON Equipment(QrToken);
        PRINT '✓ Created unique index IX_Equipment_QrToken';
    END
    
    -- ============================================================================
    -- 2. Add IsQrPrinted column
    -- ============================================================================
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Equipment') AND name = 'IsQrPrinted')
    BEGIN
        ALTER TABLE Equipment
        ADD IsQrPrinted BIT NOT NULL DEFAULT 0;
        
        PRINT '✓ Added IsQrPrinted column';
    END
    ELSE
    BEGIN
        PRINT '✓ IsQrPrinted column already exists';
    END
    
    -- ============================================================================
    -- 3. Add QrLastPrintedDate column
    -- ============================================================================
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Equipment') AND name = 'QrLastPrintedDate')
    BEGIN
        ALTER TABLE Equipment
        ADD QrLastPrintedDate DATETIME2 NULL;
        
        PRINT '✓ Added QrLastPrintedDate column';
    END
    ELSE
    BEGIN
        PRINT '✓ QrLastPrintedDate column already exists';
    END
    
    -- ============================================================================
    -- 4. Add LegacySourceId column
    -- ============================================================================
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Equipment') AND name = 'LegacySourceId')
    BEGIN
        ALTER TABLE Equipment
        ADD LegacySourceId NVARCHAR(100) NULL;
        
        PRINT '✓ Added LegacySourceId column';
    END
    ELSE
    BEGIN
        PRINT '✓ LegacySourceId column already exists';
    END
    
    -- Create index on LegacySourceId if it doesn't exist (for faster lookups)
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Equipment') AND name = 'LegacySourceId')
        AND NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Equipment_LegacySourceId' AND object_id = OBJECT_ID('Equipment'))
    BEGIN
        CREATE INDEX IX_Equipment_LegacySourceId ON Equipment(LegacySourceId);
        PRINT '✓ Created index IX_Equipment_LegacySourceId';
    END
    
    COMMIT TRANSACTION;
    PRINT '';
    PRINT '✅ Equipment QR columns migration completed successfully!';
    PRINT '';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '❌ Error occurred during migration:';
    PRINT ERROR_MESSAGE();
    PRINT '';
    THROW;
END CATCH;

