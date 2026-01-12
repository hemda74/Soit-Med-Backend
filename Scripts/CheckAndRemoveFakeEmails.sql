-- =============================================
-- Check for and Remove Fake Emails from Database
-- =============================================
-- This script identifies and removes fake/test emails from customer records

PRINT 'Checking for fake emails in the database...';
PRINT '';

-- 1. First, let's examine the current email patterns in the Clients table
PRINT 'Current email patterns in Clients table:';
SELECT 
    Email,
    COUNT(*) as Count,
    CASE 
        WHEN Email LIKE '%test%' THEN 'Test Email'
        WHEN Email LIKE '%fake%' THEN 'Fake Email'
        WHEN Email LIKE '%example%' THEN 'Example Email'
        WHEN Email LIKE '%dummy%' THEN 'Dummy Email'
        WHEN Email LIKE '%demo%' THEN 'Demo Email'
        WHEN Email LIKE '%sample%' THEN 'Sample Email'
        WHEN Email LIKE '%@test.%' THEN 'Test Domain'
        WHEN Email LIKE '%@example.%' THEN 'Example Domain'
        WHEN Email LIKE '%@fake.%' THEN 'Fake Domain'
        WHEN Email LIKE '%temp%' THEN 'Temporary Email'
        WHEN Email LIKE '%admin%' THEN 'Admin Email'
        WHEN Email IS NULL OR Email = '' THEN 'Empty Email'
        ELSE 'Legitimate Email'
    END as EmailType
FROM Clients 
WHERE Email IS NOT NULL AND Email != ''
GROUP BY Email
ORDER BY Count DESC;

PRINT '';
PRINT 'Identifying fake/test emails...';

-- 2. Identify fake emails based on common patterns
SELECT 
    Id,
    Name,
    Email,
    Phone,
    CreatedAt,
    'FAKE_EMAIL' as IssueType
FROM Clients 
WHERE Email IS NOT NULL 
    AND (
        Email LIKE '%test%' OR
        Email LIKE '%fake%' OR
        Email LIKE '%example%' OR
        Email LIKE '%dummy%' OR
        Email LIKE '%demo%' OR
        Email LIKE '%sample%' OR
        Email LIKE '%@test.%' OR
        Email LIKE '%@example.%' OR
        Email LIKE '%@fake.%' OR
        Email LIKE '%temp%' OR
        Email LIKE '%admin%'
    );

PRINT '';
PRINT 'Count of fake emails found:';

SELECT COUNT(*) as FakeEmailCount
FROM Clients 
WHERE Email IS NOT NULL 
    AND (
        Email LIKE '%test%' OR
        Email LIKE '%fake%' OR
        Email LIKE '%example%' OR
        Email LIKE '%dummy%' OR
        Email LIKE '%demo%' OR
        Email LIKE '%sample%' OR
        Email LIKE '%@test.%' OR
        Email LIKE '%@example.%' OR
        Email LIKE '%@fake.%' OR
        Email LIKE '%temp%' OR
        Email LIKE '%admin%'
    );

PRINT '';
PRINT '===========================================';
PRINT 'REMOVAL SCRIPT - Execute with caution!';
PRINT '===========================================';
PRINT '';

-- 3. Script to remove fake emails (set them to NULL)
PRINT 'Script to remove fake emails (setting them to NULL):';
PRINT 'EXECUTE THE FOLLOWING ONLY IF YOU CONFIRM THE ABOVE RESULTS:';

-- Uncomment the following lines to actually remove fake emails:
/*
UPDATE Clients 
SET Email = NULL
WHERE Email IS NOT NULL 
    AND (
        Email LIKE '%test%' OR
        Email LIKE '%fake%' OR
        Email LIKE '%example%' OR
        Email LIKE '%dummy%' OR
        Email LIKE '%demo%' OR
        Email LIKE '%sample%' OR
        Email LIKE '%@test.%' OR
        Email LIKE '%@example.% OR
        Email LIKE '%@fake.%' OR
        Email LIKE '%temp%' OR
        Email LIKE '%admin%'
    );

PRINT 'Fake emails have been removed (set to NULL)';
*/

-- 4. Verification script
PRINT '';
PRINT 'Verification script to check results:';
PRINT 'Run this after removal to verify:';

SELECT 
    COUNT(*) as TotalCustomers,
    COUNT(CASE WHEN Email IS NULL OR Email = '' THEN 1 END) as EmptyEmails,
    COUNT(CASE WHEN Email IS NOT NULL AND Email != '' THEN 1 END) as ValidEmails
FROM Clients;

PRINT '';
PRINT '===========================================';
PRINT 'Fake Email Check Complete!';
PRINT '===========================================';
