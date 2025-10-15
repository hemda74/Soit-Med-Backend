-- =============================================
-- Create Sales Funnel Tables for SoitMed
-- =============================================

-- Create ActivityLogs table
CREATE TABLE [ActivityLogs] (
    [Id] bigint NOT NULL IDENTITY(1,1),
    [PlanTaskId] int NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [InteractionType] int NOT NULL,
    [ClientType] int NOT NULL,
    [Result] int NOT NULL,
    [Reason] int NULL,
    [Comment] nvarchar(2000) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_ActivityLogs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ActivityLogs_WeeklyPlanTasks_PlanTaskId] FOREIGN KEY ([PlanTaskId]) REFERENCES [WeeklyPlanTasks] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ActivityLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

-- Create Deals table
CREATE TABLE [Deals] (
    [Id] bigint NOT NULL IDENTITY(1,1),
    [ActivityLogId] bigint NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [DealValue] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [ExpectedCloseDate] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Deals] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Deals_ActivityLogs_ActivityLogId] FOREIGN KEY ([ActivityLogId]) REFERENCES [ActivityLogs] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Deals_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

-- Create Offers table
CREATE TABLE [Offers] (
    [Id] bigint NOT NULL IDENTITY(1,1),
    [ActivityLogId] bigint NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [OfferDetails] nvarchar(2000) NOT NULL,
    [Status] int NOT NULL,
    [DocumentUrl] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_Offers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Offers_ActivityLogs_ActivityLogId] FOREIGN KEY ([ActivityLogId]) REFERENCES [ActivityLogs] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Offers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
);
GO

-- Create indexes for performance
CREATE INDEX [IX_ActivityLogs_PlanTaskId] ON [ActivityLogs] ([PlanTaskId]);
GO

CREATE INDEX [IX_ActivityLogs_UserId] ON [ActivityLogs] ([UserId]);
GO

CREATE INDEX [IX_ActivityLogs_CreatedAt] ON [ActivityLogs] ([CreatedAt]);
GO

CREATE INDEX [IX_Deals_UserId] ON [Deals] ([UserId]);
GO

CREATE INDEX [IX_Deals_Status] ON [Deals] ([Status]);
GO

CREATE INDEX [IX_Deals_CreatedAt] ON [Deals] ([CreatedAt]);
GO

CREATE INDEX [IX_Offers_UserId] ON [Offers] ([UserId]);
GO

CREATE INDEX [IX_Offers_Status] ON [Offers] ([Status]);
GO

CREATE INDEX [IX_Offers_CreatedAt] ON [Offers] ([CreatedAt]);
GO

-- Add unique constraints for one-to-one relationships
CREATE UNIQUE INDEX [IX_Deals_ActivityLogId] ON [Deals] ([ActivityLogId]);
GO

CREATE UNIQUE INDEX [IX_Offers_ActivityLogId] ON [Offers] ([ActivityLogId]);
GO

-- Insert migration record to track this change
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251014130000_AddSalesFunnelTablesOnly', N'8.0.3');
GO

PRINT 'Sales funnel tables created successfully!';
PRINT 'Tables created: ActivityLogs, Deals, Offers';
PRINT 'Indexes and foreign keys configured for optimal performance';
