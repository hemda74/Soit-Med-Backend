-- =============================================
-- Add Email Status Tracking to Clients Table
-- =============================================

PRINT 'Adding email status columns to Clients table...';

-- Add HasEmail column for quick filtering
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'HasEmail')
BEGIN
    ALTER TABLE Clients ADD HasEmail BIT NOT NULL DEFAULT 0;
    PRINT 'Added HasEmail column to Clients table';
END
ELSE
BEGIN
    PRINT 'HasEmail column already exists in Clients table';
END

-- Add EmailStatus column for detailed tracking
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'EmailStatus')
BEGIN
    ALTER TABLE Clients ADD EmailStatus NVARCHAR(20) NOT NULL DEFAULT 'Missing';
    PRINT 'Added EmailStatus column to Clients table';
END
ELSE
BEGIN
    PRINT 'EmailStatus column already exists in Clients table';
END

-- Add EmailCreatedBy column to track who created the email
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'EmailCreatedBy')
BEGIN
    ALTER TABLE Clients ADD EmailCreatedBy NVARCHAR(450) NULL;
    PRINT 'Added EmailCreatedBy column to Clients table';
END
ELSE
BEGIN
    PRINT 'EmailCreatedBy column already exists in Clients table';
END

-- Add EmailCreatedAt column for audit trail
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'EmailCreatedAt')
BEGIN
    ALTER TABLE Clients ADD EmailCreatedAt DATETIME2 NULL;
    PRINT 'Added EmailCreatedAt column to Clients table';
END
ELSE
BEGIN
    PRINT 'EmailCreatedAt column already exists in Clients table';
END

-- Update existing clients' email status
PRINT 'Updating existing clients'' email status...';

UPDATE Clients 
SET HasEmail = CASE 
        WHEN Email IS NOT NULL AND Email != '' AND Email NOT LIKE '%@legacy.local' THEN 1 
        ELSE 0 
    END,
    EmailStatus = CASE 
        WHEN Email IS NULL OR Email = '' THEN 'Missing'
        WHEN Email LIKE '%@legacy.local' THEN 'Legacy'
        ELSE 'Valid'
    END,
    EmailCreatedAt = CASE 
        WHEN Email IS NOT NULL AND Email != '' AND Email NOT LIKE '%@legacy.local' THEN CreatedAt 
        ELSE NULL 
    END;

PRINT 'Email status columns added and updated successfully!';

-- Show current status
PRINT '';
PRINT 'Current clients email status:';
SELECT 
    Id,
    Name,
    Email,
    HasEmail,
    EmailStatus,
    EmailCreatedBy,
    EmailCreatedAt
FROM Clients 
ORDER BY Id;
