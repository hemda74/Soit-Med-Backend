-- Sales Support Data Seeding Script
-- This script creates a SalesSupport user and related data

-- 1. Ensure SalesSupport role exists
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SalesSupport')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES ('SalesSupport_Role_Id', 'SalesSupport', 'SALESSUPPORT', NEWID())
END

-- 2. Ensure Sales department exists
IF NOT EXISTS (SELECT 1 FROM Departments WHERE Name = 'Sales')
BEGIN
    INSERT INTO Departments (Name, Description, CreatedAt)
    VALUES ('Sales', 'Sales Department for managing sales operations and support', GETUTCDATE())
END

-- 3. Create SalesSupport user
DECLARE @SalesDeptId INT = (SELECT Id FROM Departments WHERE Name = 'Sales')
DECLARE @UserId NVARCHAR(450) = 'Ahmed_Hemdan_Engineering_001'
DECLARE @Email NVARCHAR(256) = 'salessupport@soitmed.com'
DECLARE @PasswordHash NVARCHAR(MAX) = 'AQAAAAIAAYagAAAAEABFB5cp4hnfoANUmLA4WYlVRpWyvP0dj2aRUtwG43+SiVkDHxHs2ocKzNNGVbho8A==' -- Password: Password123!

-- Check if user already exists
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = @UserId)
BEGIN
    INSERT INTO AspNetUsers (
        Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
        PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed,
        TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount,
        CreatedAt, DepartmentId, FirstName, IsActive, LastLoginAt, LastName, PersonalMail
    )
    VALUES (
        @UserId,
        @Email,
        UPPER(@Email),
        @Email,
        UPPER(@Email),
        1, -- EmailConfirmed
        @PasswordHash,
        NEWID(),
        NEWID(),
        '01234567890',
        0, -- PhoneNumberConfirmed
        0, -- TwoFactorEnabled
        NULL, -- LockoutEnd
        1, -- LockoutEnabled
        0, -- AccessFailedCount
        GETUTCDATE(),
        @SalesDeptId,
        'Ahmed',
        1, -- IsActive
        NULL, -- LastLoginAt
        'Hemdan',
        NULL -- PersonalMail
    )
END

-- 4. Assign SalesSupport role to user
DECLARE @RoleId NVARCHAR(450) = (SELECT Id FROM AspNetRoles WHERE Name = 'SalesSupport')

IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
BEGIN
    INSERT INTO AspNetUserRoles (UserId, RoleId)
    VALUES (@UserId, @RoleId)
END

-- 5. Create some sample ActivityLogs for testing
INSERT INTO ActivityLogs (PlanTaskId, UserId, InteractionType, ClientType, Result, Reason, Comment, CreatedAt, UpdatedAt)
SELECT 1, @UserId, 1, 1, 1, 1, 'Initial sales support activity log', GETUTCDATE(), GETUTCDATE()
WHERE NOT EXISTS (SELECT 1 FROM ActivityLogs WHERE UserId = @UserId)

-- 6. Create sample Offers linked to ActivityLogs
DECLARE @ActivityLogId BIGINT = (SELECT TOP 1 Id FROM ActivityLogs WHERE UserId = @UserId ORDER BY CreatedAt DESC)

IF @ActivityLogId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Offers WHERE ActivityLogId = @ActivityLogId)
BEGIN
    INSERT INTO Offers (ActivityLogId, UserId, OfferDetails, Status, CreatedAt, UpdatedAt)
    VALUES (@ActivityLogId, @UserId, 'Sample offer for medical equipment - MRI Scanner Model X1', 1, GETUTCDATE(), GETUTCDATE())
END

-- 7. Create sample Deals linked to ActivityLogs
IF @ActivityLogId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Deals WHERE ActivityLogId = @ActivityLogId)
BEGIN
    INSERT INTO Deals (ActivityLogId, UserId, DealValue, Status, ExpectedCloseDate, CreatedAt, UpdatedAt)
    VALUES (@ActivityLogId, @UserId, 150000.00, 1, DATEADD(month, 1, GETUTCDATE()), GETUTCDATE(), GETUTCDATE())
END

-- 8. Create sample Weekly Plan for SalesSupport
IF NOT EXISTS (SELECT 1 FROM WeeklyPlans WHERE EmployeeId = @UserId)
BEGIN
    INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, CreatedAt, UpdatedAt, IsActive)
    VALUES (
        'Sales Support Weekly Plan - Week 1',
        'Weekly plan for sales support activities including client follow-ups and offer processing',
        CAST(DATEADD(day, -(DATEPART(weekday, GETDATE()) - 2), GETDATE()) AS DATE), -- Start of current week
        CAST(DATEADD(day, 6, DATEADD(day, -(DATEPART(weekday, GETDATE()) - 2), GETDATE())) AS DATE), -- End of current week
        @UserId,
        GETUTCDATE(),
        GETUTCDATE(),
        1
    )
END

-- 9. Create sample Weekly Plan Tasks
DECLARE @WeeklyPlanId INT = (SELECT TOP 1 Id FROM WeeklyPlans WHERE EmployeeId = @UserId ORDER BY CreatedAt DESC)

IF @WeeklyPlanId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM WeeklyPlanTasks WHERE WeeklyPlanId = @WeeklyPlanId)
BEGIN
    INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
    VALUES 
        (@WeeklyPlanId, 'Review Pending Offers', 'Review and process all pending offers from salesmen', 0, 1, GETUTCDATE(), GETUTCDATE(), 1),
        (@WeeklyPlanId, 'Client Follow-up Calls', 'Make follow-up calls to clients with pending deals', 0, 2, GETUTCDATE(), GETUTCDATE(), 1),
        (@WeeklyPlanId, 'Prepare Delivery Terms', 'Prepare delivery terms for approved offers', 0, 3, GETUTCDATE(), GETUTCDATE(), 1),
        (@WeeklyPlanId, 'Update Deal Status', 'Update status of deals in progress', 0, 4, GETUTCDATE(), GETUTCDATE(), 1)
END

-- 10. Create sample Sales Report (if SalesSupport can create reports)
IF NOT EXISTS (SELECT 1 FROM SalesReports WHERE EmployeeId = @UserId)
BEGIN
    INSERT INTO SalesReports (Title, Body, Type, ReportDate, EmployeeId, CreatedAt, UpdatedAt, IsActive)
    VALUES (
        'Sales Support Weekly Report',
        'Weekly report covering sales support activities, processed offers, and client interactions. Successfully processed 5 offers and followed up with 12 clients.',
        'Weekly',
        CAST(GETDATE() AS DATE),
        @UserId,
        GETUTCDATE(),
        GETUTCDATE(),
        1
    )
END

-- Verification queries
SELECT 'User Created' as Status, Id, UserName, Email, FirstName, LastName, IsActive 
FROM AspNetUsers WHERE Id = @UserId

SELECT 'Role Assigned' as Status, u.UserName, r.Name as RoleName 
FROM AspNetUsers u
JOIN AspNetUserRoles ur ON u.Id = ur.UserId
JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE u.Id = @UserId

SELECT 'Activity Logs' as Status, COUNT(*) as Count FROM ActivityLogs WHERE UserId = @UserId
SELECT 'Offers' as Status, COUNT(*) as Count FROM Offers WHERE UserId = @UserId
SELECT 'Deals' as Status, COUNT(*) as Count FROM Deals WHERE UserId = @UserId
SELECT 'Weekly Plans' as Status, COUNT(*) as Count FROM WeeklyPlans WHERE EmployeeId = @UserId
SELECT 'Sales Reports' as Status, COUNT(*) as Count FROM SalesReports WHERE EmployeeId = @UserId

PRINT 'Sales Support data seeding completed successfully!'
PRINT 'User ID: ' + @UserId
PRINT 'Email: ' + @Email
PRINT 'Password: Password123!'
