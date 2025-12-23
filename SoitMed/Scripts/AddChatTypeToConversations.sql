-- Migration: Add ChatType column to ChatConversations table
-- Date: 2025-01-09
-- Description: Adds ChatType enum field to support different chat categories (Support, Sales, Maintenance)

USE [SoitMedDB]
GO

-- Check if column already exists
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ChatConversations' 
    AND COLUMN_NAME = 'ChatType'
)
BEGIN
    -- Add ChatType column with default value of 0 (Support)
    ALTER TABLE ChatConversations
    ADD ChatType INT NOT NULL DEFAULT 0;
    
    PRINT 'ChatType column added successfully to ChatConversations table';
END
ELSE
BEGIN
    PRINT 'ChatType column already exists in ChatConversations table';
END
GO

-- Add index on ChatType for better query performance
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_ChatConversations_ChatType' 
    AND object_id = OBJECT_ID('ChatConversations')
)
BEGIN
    CREATE INDEX IX_ChatConversations_ChatType 
    ON ChatConversations(ChatType);
    
    PRINT 'Index IX_ChatConversations_ChatType created successfully';
END
ELSE
BEGIN
    PRINT 'Index IX_ChatConversations_ChatType already exists';
END
GO

-- Add composite index for customer and chat type queries
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_ChatConversations_CustomerId_ChatType' 
    AND object_id = OBJECT_ID('ChatConversations')
)
BEGIN
    CREATE INDEX IX_ChatConversations_CustomerId_ChatType 
    ON ChatConversations(CustomerId, ChatType);
    
    PRINT 'Index IX_ChatConversations_CustomerId_ChatType created successfully';
END
ELSE
BEGIN
    PRINT 'Index IX_ChatConversations_CustomerId_ChatType already exists';
END
GO

PRINT 'Migration completed successfully';
GO

