-- ========================================================================
-- CREATE TEST DATA FOR PERFORMANCE TESTING
-- ========================================================================
-- Purpose: Generate test data to validate caching and indexing improvements
-- Warning: This will add test data to your database
-- ========================================================================

USE ITIWebApi44;
GO

PRINT '========================================';
PRINT 'Creating Test Data';
PRINT '========================================';
PRINT '';

-- ========================================================================
-- SECTION 1: Test Clients (1a000 clients)
-- ========================================================================
PRINT 'Creating test clients...';

DECLARE @ClientCounter INT = 1;
DECLARE @MaxClients INT = 1000;

WHILE @ClientCounter <= @MaxClients
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Clients WHERE Name = 'Test Client ' + CAST(@ClientCounter AS NVARCHAR(10)))
    BEGIN
        INSERT INTO Clients (
            Name, Type, OrganizationName, Phone, Email, 
            Location, Status, Priority, CreatedAt, UpdatedAt
        )
        VALUES (
            'Test Client ' + CAST(@ClientCounter AS NVARCHAR(10)),
            CASE 
                WHEN @ClientCounter % 3 = 0 THEN 'Hospital'
                WHEN @ClientCounter % 3 = 1 THEN 'Clinic'
                ELSE 'Doctor'
            END,
            'Test Organization ' + CAST(@ClientCounter AS NVARCHAR(10)),
            '+20100000' + RIGHT('0000' + CAST(@ClientCounter AS NVARCHAR(10)), 4),
            'testclient' + CAST(@ClientCounter AS NVARCHAR(10)) + '@test.com',
            CASE 
                WHEN @ClientCounter % 5 = 0 THEN 'Cairo'
                WHEN @ClientCounter % 5 = 1 THEN 'Alexandria'
                WHEN @ClientCounter % 5 = 2 THEN 'Giza'
                WHEN @ClientCounter % 5 = 3 THEN 'Tanta'
                ELSE 'Mansoura'
            END,
            CASE 
                WHEN @ClientCounter % 4 = 0 THEN 'Active'
                WHEN @ClientCounter % 4 = 1 THEN 'Potential'
                WHEN @ClientCounter % 4 = 2 THEN 'Cold'
                ELSE 'Hot'
            END,
            CASE 
                WHEN @ClientCounter % 3 = 0 THEN 'High'
                WHEN @ClientCounter % 3 = 1 THEN 'Medium'
                ELSE 'Low'
            END,
            DATEADD(DAY, -(@MaxClients - @ClientCounter), GETUTCDATE()),
            GETUTCDATE()
        );
    END;
    
    SET @ClientCounter = @ClientCounter + 1;
    
    -- Progress indicator every 100 clients
    IF @ClientCounter % 100 = 0
        PRINT '  Created ' + CAST(@ClientCounter AS NVARCHAR(10)) + ' clients...';
END;

PRINT '✓ Created ' + CAST(@MaxClients AS NVARCHAR(10)) + ' test clients';
PRINT '';

-- ========================================================================
-- SECTION 2: Test Products (500 products)
-- ========================================================================
PRINT 'Creating test products...';

DECLARE @ProductCounter INT = 1;
DECLARE @MaxProducts INT = 500;

WHILE @ProductCounter <= @MaxProducts
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Products WHERE Name = 'Test Product ' + CAST(@ProductCounter AS NVARCHAR(10)))
    BEGIN
        INSERT INTO Products (
            Name, Model, Provider, Country, Category, BasePrice, 
            InStock, IsActive, CreatedAt, UpdatedAt
        )
        VALUES (
            'Test Product ' + CAST(@ProductCounter AS NVARCHAR(10)),
            'Model-' + CAST(@ProductCounter AS NVARCHAR(10)),
            CASE 
                WHEN @ProductCounter % 5 = 0 THEN 'Siemens'
                WHEN @ProductCounter % 5 = 1 THEN 'GE Healthcare'
                WHEN @ProductCounter % 5 = 2 THEN 'Philips'
                WHEN @ProductCounter % 5 = 3 THEN 'Mindray'
                ELSE 'Samsung'
            END,
            CASE 
                WHEN @ProductCounter % 4 = 0 THEN 'Germany'
                WHEN @ProductCounter % 4 = 1 THEN 'USA'
                WHEN @ProductCounter % 4 = 2 THEN 'China'
                ELSE 'Netherlands'
            END,
            CASE 
                WHEN @ProductCounter % 6 = 0 THEN 'X-Ray'
                WHEN @ProductCounter % 6 = 1 THEN 'Ultrasound'
                WHEN @ProductCounter % 6 = 2 THEN 'CT Scanner'
                WHEN @ProductCounter % 6 = 3 THEN 'MRI'
                WHEN @ProductCounter % 6 = 4 THEN 'ECG'
                ELSE 'Monitor'
            END,
            (10000 + (@ProductCounter * 100)) * (1 + (@ProductCounter % 10) * 0.1),
            CASE WHEN @ProductCounter % 10 != 0 THEN 1 ELSE 0 END,
            1,
            DATEADD(DAY, -(@MaxProducts - @ProductCounter), GETUTCDATE()),
            GETUTCDATE()
        );
    END;
    
    SET @ProductCounter = @ProductCounter + 1;
    
    IF @ProductCounter % 50 = 0
        PRINT '  Created ' + CAST(@ProductCounter AS NVARCHAR(10)) + ' products...';
END;

PRINT '✓ Created ' + CAST(@MaxProducts AS NVARCHAR(10)) + ' test products';
PRINT '';

-- ========================================================================
-- SECTION 3: Test Notifications (2000 notifications)
-- ========================================================================
PRINT 'Creating test notifications...';

DECLARE @NotificationCounter INT = 1;
DECLARE @MaxNotifications INT = 2000;
DECLARE @TestUserId NVARCHAR(450);

-- Get a random user ID for notifications
SELECT TOP 1 @TestUserId = Id FROM AspNetUsers WHERE Id IS NOT NULL;

IF @TestUserId IS NOT NULL
BEGIN
    WHILE @NotificationCounter <= @MaxNotifications
    BEGIN
        INSERT INTO Notifications (
            UserId, Title, Message, Type, IsRead, CreatedAt
        )
        VALUES (
            @TestUserId,
            'Test Notification ' + CAST(@NotificationCounter AS NVARCHAR(10)),
            'This is a test notification message for performance testing purposes.',
            CASE 
                WHEN @NotificationCounter % 5 = 0 THEN 'Request'
                WHEN @NotificationCounter % 5 = 1 THEN 'Assignment'
                WHEN @NotificationCounter % 5 = 2 THEN 'Update'
                WHEN @NotificationCounter % 5 = 3 THEN 'Reminder'
                ELSE 'Alert'
            END,
            CASE WHEN @NotificationCounter % 3 = 0 THEN 1 ELSE 0 END,
            DATEADD(MINUTE, -(@MaxNotifications - @NotificationCounter), GETUTCDATE())
        );
        
        SET @NotificationCounter = @NotificationCounter + 1;
        
        IF @NotificationCounter % 200 = 0
            PRINT '  Created ' + CAST(@NotificationCounter AS NVARCHAR(10)) + ' notifications...';
    END;
    
    PRINT '✓ Created ' + CAST(@MaxNotifications AS NVARCHAR(10)) + ' test notifications';
END
ELSE
    PRINT '⚠ No users found - skipping notifications';

PRINT '';

-- ========================================================================
-- COMPLETION SUMMARY
-- ========================================================================
DECLARE @ClientCount INT;
DECLARE @ProductCount INT;
DECLARE @NotificationCount INT;

SELECT @ClientCount = COUNT(*) FROM Clients WHERE Name LIKE 'Test Client%';
SELECT @ProductCount = COUNT(*) FROM Products WHERE Name LIKE 'Test Product%';
SELECT @NotificationCount = COUNT(*) FROM Notifications WHERE Title LIKE 'Test Notification%';

PRINT '========================================';
PRINT 'Test Data Creation Complete!';
PRINT '========================================';
PRINT '';
PRINT 'Summary:';
PRINT '- Clients: ' + CAST(@ClientCount AS NVARCHAR(10));
PRINT '- Products: ' + CAST(@ProductCount AS NVARCHAR(10));
PRINT '- Notifications: ' + CAST(@NotificationCount AS NVARCHAR(10));
PRINT '';
PRINT 'Database is ready for performance testing!';
PRINT '';

GO

