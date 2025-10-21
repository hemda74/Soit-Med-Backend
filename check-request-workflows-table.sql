-- Test if RequestWorkflows table exists and can be queried
SELECT COUNT(*) as TableExists FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RequestWorkflows';

-- If table exists, check its structure
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'RequestWorkflows'
ORDER BY ORDINAL_POSITION;

-- Check if there are any records
SELECT COUNT(*) as RecordCount FROM RequestWorkflows;


