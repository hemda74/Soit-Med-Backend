-- ============================================================================
-- DIAGNOSE SLOW QUERIES
-- ============================================================================
-- This script helps identify why queries are slow in SSMS
-- ============================================================================

USE [ITIWebApi44];
GO

PRINT '=== DIAGNOSTIC: Why Queries Are Slow ===';
PRINT '';

-- ============================================================================
-- STEP 1: Check Table Sizes
-- ============================================================================
PRINT '=== STEP 1: Table Sizes ===';
SELECT 
    t.NAME AS TableName,
    s.Name AS SchemaName,
    p.rows AS RowCount,
    CAST(ROUND(((SUM(a.total_pages) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS TotalSpaceMB,
    CAST(ROUND(((SUM(a.used_pages) * 8) / 1024.00), 2) AS NUMERIC(36, 2)) AS UsedSpaceMB,
    CAST(ROUND(((SUM(a.total_pages) - SUM(a.used_pages)) * 8) / 1024.00, 2) AS NUMERIC(36, 2)) AS UnusedSpaceMB
FROM 
    sys.tables t
INNER JOIN      
    sys.indexes i ON t.OBJECT_ID = i.object_id
INNER JOIN 
    sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
INNER JOIN 
    sys.allocation_units a ON p.partition_id = a.container_id
LEFT OUTER JOIN 
    sys.schemas s ON t.schema_id = s.schema_id
WHERE 
    t.NAME IN ('Products', 'ProductCategories')
    AND t.is_ms_shipped = 0
    AND i.OBJECT_ID > 255 
GROUP BY 
    t.Name, s.Name, p.Rows
ORDER BY 
    TotalSpaceMB DESC;
GO

-- ============================================================================
-- STEP 2: Check Missing Indexes
-- ============================================================================
PRINT '';
PRINT '=== STEP 2: Missing Indexes (Recommended by SQL Server) ===';
SELECT 
    migs.avg_total_user_cost * (migs.avg_user_impact / 100.0) * (migs.user_seeks + migs.user_scans) AS improvement_measure,
    'CREATE INDEX [missing_index_' + CONVERT (varchar, mig.index_group_handle) + '_' + CONVERT (varchar, mid.index_handle) 
        + '_' + LEFT (PARSENAME(mid.statement, 1), 32) + ']'
    + ' ON ' + mid.statement 
    + ' (' + ISNULL (mid.equality_columns,'') 
        + CASE WHEN mid.equality_columns IS NOT NULL AND mid.inequality_columns IS NOT NULL THEN ',' ELSE '' END
        + ISNULL (mid.inequality_columns, '')
    + ')' 
    + ISNULL (' INCLUDE (' + mid.included_columns + ')', '') AS create_index_statement,
    migs.*, 
    mid.database_id, 
    mid.[statement] AS table_name
FROM 
    sys.dm_db_missing_index_groups mig
INNER JOIN 
    sys.dm_db_missing_index_group_stats migs ON migs.group_handle = mig.index_group_handle
INNER JOIN 
    sys.dm_db_missing_index_details mid ON mig.index_handle = mid.index_handle
WHERE 
    mid.database_id = DB_ID()
    AND mid.[statement] LIKE '%Product%'
ORDER BY 
    migs.avg_total_user_cost * migs.avg_user_impact * (migs.user_seeks + migs.user_scans) DESC;
GO

-- ============================================================================
-- STEP 3: Check Existing Indexes
-- ============================================================================
PRINT '';
PRINT '=== STEP 3: Existing Indexes on Products and ProductCategories ===';
SELECT 
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    i.is_primary_key AS IsPrimaryKey,
    STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS IndexColumns,
    STRING_AGG(ic.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS IncludedColumns
FROM 
    sys.indexes i
INNER JOIN 
    sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN 
    sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE 
    OBJECT_NAME(i.object_id) IN ('Products', 'ProductCategories')
    AND i.type > 0  -- Exclude heap
GROUP BY 
    OBJECT_NAME(i.object_id), i.name, i.type_desc, i.is_unique, i.is_primary_key, i.object_id, i.index_id
ORDER BY 
    TableName, IndexName;
GO

-- ============================================================================
-- STEP 4: Check Table Statistics
-- ============================================================================
PRINT '';
PRINT '=== STEP 4: Table Statistics (Last Updated) ===';
SELECT 
    OBJECT_NAME(object_id) AS TableName,
    name AS StatisticsName,
    STATS_DATE(object_id, stats_id) AS LastUpdated,
    rows AS RowCount,
    rows_sampled AS RowsSampled,
    CASE 
        WHEN rows > 0 THEN CAST((rows_sampled * 100.0 / rows) AS DECIMAL(5,2))
        ELSE 0 
    END AS SamplePercent
FROM 
    sys.stats
WHERE 
    OBJECT_NAME(object_id) IN ('Products', 'ProductCategories')
ORDER BY 
    TableName, LastUpdated DESC;
GO

-- ============================================================================
-- STEP 5: Check for Table Locks/Blocking
-- ============================================================================
PRINT '';
PRINT '=== STEP 5: Current Locks and Blocking ===';
SELECT 
    t1.resource_type,
    t1.resource_database_id,
    t1.resource_associated_entity_id,
    t1.request_mode,
    t1.request_session_id,
    t2.blocking_session_id,
    OBJECT_NAME(p.object_id) AS TableName,
    t1.request_status,
    t1.request_type
FROM 
    sys.dm_tran_locks t1
LEFT JOIN 
    sys.dm_os_waiting_tasks t2 ON t1.lock_owner_address = t2.resource_address
LEFT JOIN 
    sys.partitions p ON t1.resource_associated_entity_id = p.hobt_id
WHERE 
    t1.resource_database_id = DB_ID()
    AND (OBJECT_NAME(p.object_id) IN ('Products', 'ProductCategories') OR p.object_id IS NULL)
ORDER BY 
    t1.request_session_id;
GO

-- ============================================================================
-- STEP 6: Check Query Execution Plans (Most Expensive Queries)
-- ============================================================================
PRINT '';
PRINT '=== STEP 6: Most Expensive Queries (Last 24 Hours) ===';
SELECT TOP 10
    qs.execution_count,
    qs.total_elapsed_time / 1000000.0 AS total_elapsed_time_seconds,
    qs.total_elapsed_time / qs.execution_count / 1000000.0 AS avg_elapsed_time_seconds,
    qs.total_logical_reads,
    qs.total_logical_reads / qs.execution_count AS avg_logical_reads,
    qs.total_physical_reads,
    qs.total_physical_reads / qs.execution_count AS avg_physical_reads,
    SUBSTRING(qt.text, (qs.statement_start_offset/2) + 1,
        ((CASE qs.statement_end_offset
            WHEN -1 THEN DATALENGTH(qt.text)
            ELSE qs.statement_end_offset
        END - qs.statement_start_offset)/2) + 1) AS statement_text,
    qt.text AS full_query_text,
    qp.query_plan
FROM 
    sys.dm_exec_query_stats qs
CROSS APPLY 
    sys.dm_exec_sql_text(qs.sql_handle) qt
CROSS APPLY 
    sys.dm_exec_query_plan(qs.plan_handle) qp
WHERE 
    qt.text LIKE '%Product%'
    AND qs.last_execution_time > DATEADD(hour, -24, GETDATE())
ORDER BY 
    qs.total_elapsed_time DESC;
GO

-- ============================================================================
-- STEP 7: Check Fragmentation
-- ============================================================================
PRINT '';
PRINT '=== STEP 7: Index Fragmentation ===';
SELECT 
    OBJECT_NAME(ips.object_id) AS TableName,
    i.name AS IndexName,
    ips.index_id,
    ips.index_type_desc,
    ips.avg_fragmentation_in_percent,
    ips.page_count,
    CASE 
        WHEN ips.avg_fragmentation_in_percent > 30 THEN 'REBUILD'
        WHEN ips.avg_fragmentation_in_percent > 10 THEN 'REORGANIZE'
        ELSE 'OK'
    END AS RecommendedAction
FROM 
    sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'DETAILED') ips
INNER JOIN 
    sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
WHERE 
    OBJECT_NAME(ips.object_id) IN ('Products', 'ProductCategories')
    AND ips.page_count > 100  -- Only show indexes with significant pages
ORDER BY 
    ips.avg_fragmentation_in_percent DESC;
GO

-- ============================================================================
-- STEP 8: Test Query Performance
-- ============================================================================
PRINT '';
PRINT '=== STEP 8: Test Query Performance ===';
PRINT 'Testing ProductCategories query (what the API uses)...';
SET STATISTICS TIME ON;
SET STATISTICS IO ON;

SELECT 
    [p].[Id], 
    [p].[CreatedAt], 
    [p].[Description], 
    [p].[DescriptionAr], 
    [p].[DisplayOrder], 
    [p].[IconPath], 
    [p].[IsActive], 
    [p].[Name], 
    [p].[NameAr], 
    [p].[ParentCategoryId], 
    [p].[UpdatedAt]
FROM [ProductCategories] AS [p]
WHERE [p].[IsActive] = 1 AND [p].[ParentCategoryId] IS NULL
ORDER BY [p].[DisplayOrder], [p].[Name];

SET STATISTICS TIME OFF;
SET STATISTICS IO OFF;
GO

PRINT '';
PRINT 'Testing Products query (what the API uses)...';
SET STATISTICS TIME ON;
SET STATISTICS IO ON;

SELECT TOP 100
    [p].[Id], 
    [p].[BasePrice], 
    [p].[CatalogPath], 
    [p].[Category], 
    [p].[CategoryId], 
    [p].[Country], 
    [p].[CreatedAt], 
    [p].[CreatedBy], 
    [p].[DataSheetPath], 
    [p].[Description], 
    [p].[ImagePath], 
    [p].[InStock], 
    [p].[InventoryQuantity], 
    [p].[IsActive], 
    [p].[Model], 
    [p].[Name], 
    [p].[Provider], 
    [p].[ProviderImagePath], 
    [p].[UpdatedAt], 
    [p].[Year]
FROM [Products] AS [p]
WHERE [p].[IsActive] = 1
ORDER BY [p].[Name];

SET STATISTICS TIME OFF;
SET STATISTICS IO OFF;
GO

PRINT '';
PRINT '=== DIAGNOSTIC COMPLETE ===';
PRINT '';
PRINT 'Review the results above to identify:';
PRINT '  1. Large table sizes (STEP 1)';
PRINT '  2. Missing indexes (STEP 2) - Run add_performance_indexes.sql';
PRINT '  3. Fragmented indexes (STEP 7) - May need REBUILD';
PRINT '  4. Table locks (STEP 5) - May need to kill blocking sessions';
PRINT '  5. Outdated statistics (STEP 4) - May need UPDATE STATISTICS';
PRINT '  6. Slow queries (STEP 6) - Check execution plans';
PRINT '  7. Query performance (STEP 8) - Check execution time and I/O';
GO

