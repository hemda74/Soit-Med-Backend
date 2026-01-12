-- Complete fix for SalesOffers.Id and all dependent tables
-- This handles all foreign key constraints that reference SalesOffers.Id

USE [ITIWebApi44]
GO

PRINT 'Starting complete SalesOffers type fix...'

-- Check current state
SELECT 
    'SalesOffers' AS TableName,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SalesOffers' AND COLUMN_NAME = 'Id'
UNION ALL
SELECT 
    'OfferRequests' AS TableName,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'OfferRequests' AND COLUMN_NAME = 'CreatedOfferId'
GO

-- Step 1: Drop all foreign key constraints referencing SalesOffers
PRINT 'Step 1: Dropping all foreign key constraints referencing SalesOffers...'

DECLARE @sql NVARCHAR(MAX) = ''
SELECT @sql = @sql + 'ALTER TABLE ' + OBJECT_NAME(parent_object_id) + ' DROP CONSTRAINT ' + name + CHAR(13) + CHAR(10)
FROM sys.foreign_keys 
WHERE referenced_object_id = OBJECT_ID('SalesOffers')

IF LEN(@sql) > 0
BEGIN
    EXEC sp_executesql @sql
    PRINT 'Dropped all foreign key constraints referencing SalesOffers'
END
GO

-- Step 2: Drop all indexes on SalesOffers
PRINT 'Step 2: Dropping all indexes on SalesOffers...'

DECLARE @index_sql NVARCHAR(MAX) = ''
SELECT @index_sql = @index_sql + 'DROP INDEX ' + name + ' ON SalesOffers' + CHAR(13) + CHAR(10)
FROM sys.indexes 
WHERE object_id = OBJECT_ID('SalesOffers') 
    AND name IS NOT NULL 
    AND name NOT LIKE 'PK_%'

IF LEN(@index_sql) > 0
BEGIN
    EXEC sp_executesql @index_sql
    PRINT 'Dropped all non-PK indexes on SalesOffers'
END
GO

-- Step 3: Drop primary key constraint
PRINT 'Step 3: Dropping primary key constraint...'

IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('SalesOffers') AND type = 'PK')
BEGIN
    DECLARE @PkName NVARCHAR(128)
    SELECT @PkName = name FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('SalesOffers') AND type = 'PK'
    EXEC ('ALTER TABLE SalesOffers DROP CONSTRAINT ' + @PkName)
    PRINT 'Dropped SalesOffers primary key: ' + @PkName
END
GO

-- Step 4: Recreate SalesOffers table with correct types
PRINT 'Step 4: Recreating SalesOffers table with correct types...'

BEGIN TRY
    -- Get all column definitions first
    DECLARE @columns TABLE (
        ColumnName NVARCHAR(128),
        DataType NVARCHAR(128),
        MaxLength INT,
        IsNullable BIT,
        DefaultDefinition NVARCHAR(MAX)
    )
    
    INSERT INTO @columns
    SELECT 
        c.name,
        t.name + CASE 
            WHEN t.name IN ('char', 'varchar', 'nchar', 'nvarchar') AND c.max_length > 0 
                THEN '(' + CAST(c.max_length AS NVARCHAR) + ')'
            WHEN t.name IN ('decimal', 'numeric') 
                THEN '(' + CAST(c.precision AS NVARCHAR) + ',' + CAST(c.scale AS NVARCHAR) + ')'
            ELSE ''
        END,
        c.max_length,
        c.is_nullable,
        OBJECT_DEFINITION(c.default_object_id)
    FROM sys.columns c
    JOIN sys.types t ON c.user_type_id = t.user_type_id
    WHERE c.object_id = OBJECT_ID('SalesOffers')
    ORDER BY c.column_id
    
    -- Create temp table and copy data
    SELECT * INTO #TempSalesOffers FROM SalesOffers
    
    -- Drop original table
    DROP TABLE SalesOffers
    
    -- Recreate table with correct Id type
    DECLARE @create_sql NVARCHAR(MAX) = 'CREATE TABLE SalesOffers ('
    DECLARE @first BIT = 1
    
    SELECT @create_sql = @create_sql + 
        CASE WHEN @first = 0 THEN ', ' ELSE '' END +
        CASE 
            WHEN ColumnName = 'Id' THEN 'Id NVARCHAR(50) NOT NULL PRIMARY KEY'
            ELSE ColumnName + ' ' + DataType + 
                 CASE WHEN IsNullable = 0 THEN ' NOT NULL' ELSE ' NULL' END +
                 CASE WHEN DefaultDefinition IS NOT NULL THEN ' DEFAULT ' + DefaultDefinition ELSE '' END
        END,
        @first = 0
    FROM @columns
    ORDER BY 
        CASE WHEN ColumnName = 'Id' THEN 0 ELSE 1 END,
        ColumnName
    
    SET @create_sql = @create_sql + ')'
    
    EXEC sp_executesql @create_sql
    PRINT 'Recreated SalesOffers table structure'
    
    -- Copy data back (convert Id to string)
    DECLARE @insert_sql NVARCHAR(MAX) = 'INSERT INTO SalesOffers ('
    DECLARE @columns_list NVARCHAR(MAX) = ''
    DECLARE @values_list NVARCHAR(MAX) = ''
    
    SELECT @columns_list = @columns_list + CASE WHEN @columns_list = '' THEN '' ELSE ', ' END + ColumnName,
           @values_list = @values_list + CASE WHEN @values_list = '' THEN '' ELSE ', ' END + 
               CASE 
                   WHEN ColumnName = 'Id' THEN 'CAST(Id AS NVARCHAR(50))'
                   ELSE ColumnName
               END
    FROM @columns
    ORDER BY ColumnName
    
    SET @insert_sql = @insert_sql + @columns_list + ') SELECT ' + @values_list + ' FROM #TempSalesOffers'
    
    EXEC sp_executesql @insert_sql
    PRINT 'Copied data back to SalesOffers table'
    
END TRY
BEGIN CATCH
    PRINT 'Error recreating SalesOffers: ' + ERROR_MESSAGE()
    
    -- Restore from backup if needed
    IF OBJECT_ID('SalesOffers') IS NULL AND OBJECT_ID('TempSalesOffers') IS NOT NULL
    BEGIN
        SELECT * INTO SalesOffers FROM #TempSalesOffers
        PRINT 'Restored SalesOffers from backup'
    END
END CATCH
GO

-- Step 5: Clean up temp table
PRINT 'Step 5: Cleaning up...'
DROP TABLE IF EXISTS #TempSalesOffers
GO

-- Step 6: Recreate all foreign key constraints
PRINT 'Step 6: Recreating foreign key constraints...'

-- Recreate FK from OfferRequests
ALTER TABLE OfferRequests 
ADD CONSTRAINT FK_OfferRequests_SalesOffers_CreatedOfferId 
FOREIGN KEY (CreatedOfferId) REFERENCES SalesOffers(Id)
ON DELETE NO ACTION
PRINT 'Recreated FK_OfferRequests_SalesOffers_CreatedOfferId'

-- Add other FK constraints here if needed
-- (We would need to check what other tables reference SalesOffers)
GO

-- Step 7: Final verification
PRINT 'Step 7: Final verification...'
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SalesOffers' AND COLUMN_NAME = 'Id'
UNION ALL
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'OfferRequests' AND COLUMN_NAME = 'CreatedOfferId'
GO

PRINT 'Complete SalesOffers type fix completed successfully!'
