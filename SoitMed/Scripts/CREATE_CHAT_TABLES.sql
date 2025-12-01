-- Chat System Database Tables
-- Run this script if you prefer manual SQL migration over EF Core migrations

-- Create ChatConversations table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChatConversations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChatConversations] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [CustomerId] NVARCHAR(450) NOT NULL,
        [AdminId] NVARCHAR(450) NULL,
        [LastMessageAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [LastMessagePreview] NVARCHAR(200) NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_ChatConversations] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ChatConversations_AspNetUsers_CustomerId] FOREIGN KEY ([CustomerId]) 
            REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChatConversations_AspNetUsers_AdminId] FOREIGN KEY ([AdminId]) 
            REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
    
    CREATE INDEX [IX_ChatConversations_CustomerId] ON [dbo].[ChatConversations]([CustomerId]);
    CREATE INDEX [IX_ChatConversations_AdminId] ON [dbo].[ChatConversations]([AdminId]);
    CREATE INDEX [IX_ChatConversations_LastMessageAt] ON [dbo].[ChatConversations]([LastMessageAt] DESC);
END
GO

-- Create ChatMessages table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ChatMessages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[ChatMessages] (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [ConversationId] BIGINT NOT NULL,
        [SenderId] NVARCHAR(450) NOT NULL,
        [MessageType] NVARCHAR(20) NOT NULL DEFAULT 'Text',
        [Content] NVARCHAR(2000) NULL,
        [VoiceFilePath] NVARCHAR(500) NULL,
        [VoiceDuration] INT NULL,
        [IsRead] BIT NOT NULL DEFAULT 0,
        [ReadAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ChatMessages_ChatConversations_ConversationId] FOREIGN KEY ([ConversationId]) 
            REFERENCES [dbo].[ChatConversations] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChatMessages_AspNetUsers_SenderId] FOREIGN KEY ([SenderId]) 
            REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
    
    CREATE INDEX [IX_ChatMessages_ConversationId] ON [dbo].[ChatMessages]([ConversationId]);
    CREATE INDEX [IX_ChatMessages_SenderId] ON [dbo].[ChatMessages]([SenderId]);
    CREATE INDEX [IX_ChatMessages_CreatedAt] ON [dbo].[ChatMessages]([CreatedAt] DESC);
    CREATE INDEX [IX_ChatMessages_IsRead] ON [dbo].[ChatMessages]([IsRead]);
END
GO

PRINT 'Chat tables created successfully!';
GO

