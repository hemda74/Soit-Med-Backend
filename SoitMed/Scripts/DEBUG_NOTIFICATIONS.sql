-- Debug script to check notifications and offers

-- 1. Check if Notifications table exists
IF OBJECT_ID('Notifications', 'U') IS NOT NULL
BEGIN
    PRINT '✅ Notifications table EXISTS'
    
    -- Show table structure
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Notifications'
    ORDER BY ORDINAL_POSITION;
    
    -- Count total notifications
    DECLARE @TotalCount INT;
    SELECT @TotalCount = COUNT(*) FROM Notifications;
    PRINT 'Total Notifications: ' + CAST(@TotalCount AS VARCHAR);
    
    -- Show recent notifications
    PRINT '';
    PRINT '=== RECENT NOTIFICATIONS (Last 10) ===';
    SELECT TOP 10
        Id,
        UserId,
        Title,
        Message,
        Type,
        IsRead,
        CreatedAt,
        IsMobilePush
    FROM Notifications
    ORDER BY CreatedAt DESC;
    
    -- Show notifications by type
    PRINT '';
    PRINT '=== NOTIFICATIONS BY TYPE ===';
    SELECT 
        Type,
        COUNT(*) as Count,
        SUM(CASE WHEN IsRead = 1 THEN 1 ELSE 0 END) as ReadCount,
        SUM(CASE WHEN IsRead = 0 THEN 1 ELSE 0 END) as UnreadCount
    FROM Notifications
    GROUP BY Type
    ORDER BY Count DESC;
    
END
ELSE
BEGIN
    PRINT '❌ Notifications table DOES NOT EXIST!'
    PRINT 'You need to create the Notifications table!'
END

PRINT '';
PRINT '=== RECENT OFFERS ===';
-- Check recent offers
SELECT TOP 10
    Id,
    ClientId,
    Status,
    AssignedTo,
    TotalAmount,
    SentToClientAt,
    CreatedAt,
    UpdatedAt
FROM SalesOffers
ORDER BY CreatedAt DESC;

PRINT '';
PRINT '=== OFFERS BY STATUS ===';
SELECT 
    Status,
    COUNT(*) as Count
FROM SalesOffers
GROUP BY Status
ORDER BY Count DESC;

-- Check if there are any users
PRINT '';
PRINT '=== USER COUNT ===';
SELECT 
    COUNT(*) as TotalUsers
FROM AspNetUsers;

-- Check user roles
PRINT '';
PRINT '=== USERS BY ROLE ===';
SELECT 
    r.Name as RoleName,
    COUNT(ur.UserId) as UserCount
FROM AspNetRoles r
LEFT JOIN AspNetUserRoles ur ON r.Id = ur.RoleId
GROUP BY r.Name
ORDER BY UserCount DESC;



