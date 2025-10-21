-- Create RequestWorkflows table manually
-- This script creates the RequestWorkflows table with all necessary columns and relationships

-- Check if table already exists
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RequestWorkflows')
BEGIN
    -- Create RequestWorkflows table
    CREATE TABLE [RequestWorkflows] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [ActivityLogId] bigint NOT NULL,
        [OfferId] bigint NULL,
        [DealId] bigint NULL,
        [RequestType] nvarchar(50) NOT NULL,
        [FromRole] nvarchar(50) NOT NULL,
        [ToRole] nvarchar(50) NOT NULL,
        [FromUserId] nvarchar(450) NOT NULL,
        [ToUserId] nvarchar(450) NULL,
        [Status] int NOT NULL DEFAULT 0,
        [Comment] nvarchar(1000) NULL,
        [ClientName] nvarchar(200) NULL,
        [ClientAddress] nvarchar(500) NULL,
        [EquipmentDetails] nvarchar(1000) NULL,
        [DeliveryTermsId] bigint NULL,
        [PaymentTermsId] bigint NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [AssignedAt] datetime2 NULL,
        [CompletedAt] datetime2 NULL,
        CONSTRAINT [PK_RequestWorkflows] PRIMARY KEY ([Id])
    );

    -- Create foreign key constraints
    ALTER TABLE [RequestWorkflows] 
    ADD CONSTRAINT [FK_RequestWorkflows_ActivityLogs_ActivityLogId] 
    FOREIGN KEY ([ActivityLogId]) REFERENCES [ActivityLogs] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [RequestWorkflows] 
    ADD CONSTRAINT [FK_RequestWorkflows_AspNetUsers_FromUserId] 
    FOREIGN KEY ([FromUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [RequestWorkflows] 
    ADD CONSTRAINT [FK_RequestWorkflows_AspNetUsers_ToUserId] 
    FOREIGN KEY ([ToUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;

    -- Create indexes for better performance
    CREATE INDEX [IX_RequestWorkflows_FromUserId_Status] ON [RequestWorkflows] ([FromUserId], [Status]);
    CREATE INDEX [IX_RequestWorkflows_ToUserId_Status] ON [RequestWorkflows] ([ToUserId], [Status]);
    CREATE INDEX [IX_RequestWorkflows_CreatedAt] ON [RequestWorkflows] ([CreatedAt]);

    PRINT 'RequestWorkflows table created successfully!';
END
ELSE
BEGIN
    PRINT 'RequestWorkflows table already exists.';
END

-- Verify table creation
SELECT COUNT(*) as TableExists FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RequestWorkflows';
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'RequestWorkflows'
ORDER BY ORDINAL_POSITION;
