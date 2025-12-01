-- Add VoiceDescriptionUrl column to TaskProgresses table
-- This column stores the URL to voice recording files for task progress descriptions

USE ITIWebApi44;
GO

SET QUOTED_IDENTIFIER ON;
GO

PRINT 'Starting to add VoiceDescriptionUrl column to TaskProgresses table...';
GO

-- Check if column already exists
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[TaskProgresses]') 
    AND name = 'VoiceDescriptionUrl'
)
BEGIN
    ALTER TABLE [dbo].[TaskProgresses]
    ADD VoiceDescriptionUrl NVARCHAR(2000) NULL;
    
    PRINT 'Added VoiceDescriptionUrl column to TaskProgresses table';
END
ELSE
BEGIN
    PRINT 'VoiceDescriptionUrl column already exists in TaskProgresses table';
END
GO

PRINT '';
PRINT 'Verification:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TaskProgresses'
    AND COLUMN_NAME = 'VoiceDescriptionUrl';
GO

PRINT '';
PRINT 'Migration completed successfully!';
GO

