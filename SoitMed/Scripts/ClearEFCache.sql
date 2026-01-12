-- Clear Entity Framework cache
-- This removes EF's internal metadata tables so it will rebuild with the correct schema

USE [ITIWebApi44]
GO

PRINT 'Clearing Entity Framework cache...'

-- Drop EF migration history table if it exists
IF EXISTS (SELECT 1 FROM sys.tables WHERE name = '__EFMigrationsHistory')
BEGIN
    DROP TABLE __EFMigrationsHistory
    PRINT 'Dropped __EFMigrationsHistory table'
END
GO

-- Drop any EF shadow tables or views
DECLARE @sql NVARCHAR(MAX) = ''
SELECT @sql = @sql + 'DROP TABLE IF EXISTS ' + name + CHAR(13) + CHAR(10)
FROM sys.tables 
WHERE name LIKE '__EF%' OR name LIKE '_EFMigrations%'

IF LEN(@sql) > 0
BEGIN
    EXEC sp_executesql @sql
    PRINT 'Dropped EF shadow tables'
END
GO

-- Drop any EF views
DECLARE @view_sql NVARCHAR(MAX) = ''
SELECT @view_sql = @view_sql + 'DROP VIEW IF EXISTS ' + name + CHAR(13) + CHAR(10)
FROM sys.views 
WHERE name LIKE '__EF%' OR name LIKE '_EFMigrations%'

IF LEN(@view_sql) > 0
BEGIN
    EXEC sp_executesql @view_sql
    PRINT 'Dropped EF views'
END
GO

PRINT 'EF cache cleared successfully!'
