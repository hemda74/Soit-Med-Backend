USE ITIWebApi44;
GO

PRINT 'Adding ReviewedBy column to WeeklyPlans table...'

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WeeklyPlans') AND name = 'ReviewedBy')
BEGIN
    ALTER TABLE WeeklyPlans ADD ReviewedBy NVARCHAR(450) NULL;
    PRINT 'ReviewedBy column added to WeeklyPlans table.'
END
ELSE
BEGIN
    PRINT 'ReviewedBy column already exists in WeeklyPlans table.'
END
GO


