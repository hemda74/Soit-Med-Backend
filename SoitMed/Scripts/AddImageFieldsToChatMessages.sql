-- =============================================
-- Add Image Fields to ChatMessages Table
-- Run this script in SSMS to add image attachment support
-- =============================================

USE [SoitMed];  -- Replace with your database name if different
GO

-- Add ImageFilePath column
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[ChatMessages]') 
    AND name = 'ImageFilePath'
)
BEGIN
    ALTER TABLE [dbo].[ChatMessages]
    ADD [ImageFilePath] NVARCHAR(500) NULL;
    PRINT 'ImageFilePath column added successfully.';
END
ELSE
BEGIN
    PRINT 'ImageFilePath column already exists.';
END
GO

-- Add ImageFileName column
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[ChatMessages]') 
    AND name = 'ImageFileName'
)
BEGIN
    ALTER TABLE [dbo].[ChatMessages]
    ADD [ImageFileName] NVARCHAR(255) NULL;
    PRINT 'ImageFileName column added successfully.';
END
ELSE
BEGIN
    PRINT 'ImageFileName column already exists.';
END
GO

-- Add ImageFileSize column
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[ChatMessages]') 
    AND name = 'ImageFileSize'
)
BEGIN
    ALTER TABLE [dbo].[ChatMessages]
    ADD [ImageFileSize] BIGINT NULL;
    PRINT 'ImageFileSize column added successfully.';
END
ELSE
BEGIN
    PRINT 'ImageFileSize column already exists.';
END
GO

PRINT 'Script completed. All image fields have been added to ChatMessages table.';
GO

