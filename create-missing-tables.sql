-- Create missing tables for RequestWorkflow functionality
-- This script creates all required tables with proper constraints

-- Create DeliveryTerms table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DeliveryTerms')
BEGIN
    CREATE TABLE [DeliveryTerms] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [DeliveryMethod] nvarchar(100) NOT NULL,
        [DeliveryAddress] nvarchar(500) NOT NULL,
        [City] nvarchar(100) NULL,
        [State] nvarchar(100) NULL,
        [PostalCode] nvarchar(20) NULL,
        [Country] nvarchar(100) NULL,
        [EstimatedDeliveryDays] int NULL,
        [SpecialInstructions] nvarchar(1000) NULL,
        [IsUrgent] bit NOT NULL DEFAULT 0,
        [PreferredDeliveryDate] datetime2 NULL,
        [ContactPerson] nvarchar(200) NULL,
        [ContactPhone] nvarchar(50) NULL,
        [ContactEmail] nvarchar(200) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_DeliveryTerms] PRIMARY KEY ([Id])
    );
    PRINT 'DeliveryTerms table created successfully!';
END
ELSE
BEGIN
    PRINT 'DeliveryTerms table already exists.';
END

-- Create PaymentTerms table
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PaymentTerms')
BEGIN
    CREATE TABLE [PaymentTerms] (
        [Id] bigint IDENTITY(1,1) NOT NULL,
        [PaymentMethod] nvarchar(100) NOT NULL,
        [TotalAmount] decimal(18,2) NOT NULL,
        [DownPayment] decimal(18,2) NULL,
        [InstallmentCount] int NULL,
        [InstallmentAmount] decimal(18,2) NULL,
        [PaymentDueDays] int NULL,
        [BankName] nvarchar(200) NULL,
        [AccountNumber] nvarchar(100) NULL,
        [IBAN] nvarchar(50) NULL,
        [SwiftCode] nvarchar(20) NULL,
        [PaymentInstructions] nvarchar(1000) NULL,
        [RequiresAdvancePayment] bit NOT NULL DEFAULT 0,
        [AdvancePaymentPercentage] decimal(5,2) NULL,
        [Currency] nvarchar(10) NOT NULL DEFAULT 'USD',
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_PaymentTerms] PRIMARY KEY ([Id])
    );
    PRINT 'PaymentTerms table created successfully!';
END
ELSE
BEGIN
    PRINT 'PaymentTerms table already exists.';
END

-- Now create RequestWorkflows table with corrected constraints
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

    -- Create foreign key constraints (with corrected cascade behavior)
    ALTER TABLE [RequestWorkflows] 
    ADD CONSTRAINT [FK_RequestWorkflows_ActivityLogs_ActivityLogId] 
    FOREIGN KEY ([ActivityLogId]) REFERENCES [ActivityLogs] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [RequestWorkflows] 
    ADD CONSTRAINT [FK_RequestWorkflows_AspNetUsers_FromUserId] 
    FOREIGN KEY ([FromUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE;

    ALTER TABLE [RequestWorkflows] 
    ADD CONSTRAINT [FK_RequestWorkflows_AspNetUsers_ToUserId] 
    FOREIGN KEY ([ToUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;

    -- Add foreign keys for DeliveryTerms and PaymentTerms
    ALTER TABLE [RequestWorkflows] 
    ADD CONSTRAINT [FK_RequestWorkflows_DeliveryTerms_DeliveryTermsId] 
    FOREIGN KEY ([DeliveryTermsId]) REFERENCES [DeliveryTerms] ([Id]) ON DELETE SET NULL;

    ALTER TABLE [RequestWorkflows] 
    ADD CONSTRAINT [FK_RequestWorkflows_PaymentTerms_PaymentTermsId] 
    FOREIGN KEY ([PaymentTermsId]) REFERENCES [PaymentTerms] ([Id]) ON DELETE SET NULL;

    -- Create indexes for better performance
    CREATE INDEX [IX_RequestWorkflows_FromUserId_Status] ON [RequestWorkflows] ([FromUserId], [Status]);
    CREATE INDEX [IX_RequestWorkflows_ToUserId_Status] ON [RequestWorkflows] ([ToUserId], [Status]);
    CREATE INDEX [IX_RequestWorkflows_CreatedAt] ON [RequestWorkflows] ([CreatedAt]);
    CREATE INDEX [IX_RequestWorkflows_ActivityLogId] ON [RequestWorkflows] ([ActivityLogId]);

    PRINT 'RequestWorkflows table created successfully!';
END
ELSE
BEGIN
    PRINT 'RequestWorkflows table already exists.';
END

-- Verify all tables exist
SELECT 'Table Creation Summary' as Status;
SELECT TABLE_NAME as TableName, 
       CASE WHEN TABLE_NAME IN ('RequestWorkflows', 'DeliveryTerms', 'PaymentTerms') 
            THEN 'EXISTS' 
            ELSE 'MISSING' 
       END as Status
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('RequestWorkflows', 'DeliveryTerms', 'PaymentTerms')
ORDER BY TABLE_NAME;

