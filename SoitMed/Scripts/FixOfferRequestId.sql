-- Fix OfferRequests.Id column type from bigint to nvarchar(50)
-- This resolves the EF type mismatch where OfferRequest.Id should be string (from BaseEntity)

USE [ITIWebApi44]
GO

PRINT 'Starting OfferRequests.Id type fix...'

-- Check current state
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMNPROPERTY(OBJECT_ID(TABLE_NAME), COLUMN_NAME, 'IsIdentity') AS IsIdentity
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'OfferRequests' AND COLUMN_NAME = 'Id'
GO

-- Step 1: Drop all foreign key constraints referencing OfferRequests.Id
PRINT 'Step 1: Dropping foreign key constraints referencing OfferRequests...'

DECLARE @sql NVARCHAR(MAX) = ''
SELECT @sql = @sql + 'ALTER TABLE ' + OBJECT_NAME(parent_object_id) + ' DROP CONSTRAINT ' + name + CHAR(13) + CHAR(10)
FROM sys.foreign_keys 
WHERE referenced_object_id = OBJECT_ID('OfferRequests')

IF LEN(@sql) > 0
BEGIN
    EXEC sp_executesql @sql
    PRINT 'Dropped all foreign key constraints referencing OfferRequests'
END
GO

-- Step 2: Drop all indexes on OfferRequests.Id
PRINT 'Step 2: Dropping indexes on OfferRequests.Id...'

DECLARE @index_sql NVARCHAR(MAX) = ''
SELECT @index_sql = @index_sql + 'DROP INDEX ' + name + ' ON OfferRequests' + CHAR(13) + CHAR(10)
FROM sys.indexes 
WHERE object_id = OBJECT_ID('OfferRequests') 
    AND name IS NOT NULL 
    AND name NOT LIKE 'PK_%'

IF LEN(@index_sql) > 0
BEGIN
    EXEC sp_executesql @index_sql
    PRINT 'Dropped all non-PK indexes on OfferRequests'
END
GO

-- Step 3: Drop primary key constraint
PRINT 'Step 3: Dropping primary key constraint...'

IF EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('OfferRequests') AND type = 'PK')
BEGIN
    DECLARE @PkName NVARCHAR(128)
    SELECT @PkName = name FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID('OfferRequests') AND type = 'PK'
    EXEC ('ALTER TABLE OfferRequests DROP CONSTRAINT ' + @PkName)
    PRINT 'Dropped OfferRequests primary key: ' + @PkName
END
GO

-- Step 4: Recreate OfferRequests table with correct types
PRINT 'Step 4: Recreating OfferRequests table with correct types...'

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
    WHERE c.object_id = OBJECT_ID('OfferRequests')
    ORDER BY c.column_id
    
    -- Create temp table and copy data
    SELECT * INTO #TempOfferRequests FROM OfferRequests
    
    -- Drop original table
    DROP TABLE OfferRequests
    
    -- Recreate table with correct Id type
    DECLARE @create_sql NVARCHAR(MAX) = 'CREATE TABLE OfferRequests ('
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
    PRINT 'Recreated OfferRequests table structure'
    
    -- Copy data back (convert Id to string)
    DECLARE @insert_sql NVARCHAR(MAX) = 'INSERT INTO OfferRequests ('
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
    
    SET @insert_sql = @insert_sql + @columns_list + ') SELECT ' + @values_list + ' FROM #TempOfferRequests'
    
    EXEC sp_executesql @insert_sql
    PRINT 'Copied data back to OfferRequests table'
    
END TRY
BEGIN CATCH
    PRINT 'Error recreating OfferRequests: ' + ERROR_MESSAGE()
    
    -- Restore from backup if needed
    IF OBJECT_ID('OfferRequests') IS NULL AND OBJECT_ID('TempOfferRequests') IS NOT NULL
    BEGIN
        SELECT * INTO OfferRequests FROM #TempOfferRequests
        PRINT 'Restored OfferRequests from backup'
    END
END CATCH
GO

-- Step 5: Clean up temp table
PRINT 'Step 5: Cleaning up...'
DROP TABLE IF EXISTS #TempOfferRequests
GO

-- Step 6: Recreate foreign key constraint to SalesOffers
PRINT 'Step 6: Recreating foreign key constraint to SalesOffers...'

ALTER TABLE OfferRequests 
ADD CONSTRAINT FK_OfferRequests_SalesOffers_CreatedOfferId 
FOREIGN KEY (CreatedOfferId) REFERENCES SalesOffers(Id)
ON DELETE NO ACTION
PRINT 'Recreated FK_OfferRequests_SalesOffers_CreatedOfferId'
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
WHERE TABLE_NAME = 'OfferRequests' AND COLUMN_NAME = 'Id'
UNION ALL
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

PRINT 'OfferRequests.Id type fix completed successfully!'
