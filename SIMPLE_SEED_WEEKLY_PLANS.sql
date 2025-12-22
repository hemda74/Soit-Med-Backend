-- ==========================================
-- SIMPLE SEED DATA FOR WEEKLY PLANS
-- Copy and paste this in SQL Server Management Studio
-- ==========================================

USE [SoitMed]; -- Change this to your database name
GO

PRINT '====================================';
PRINT 'SEEDING WEEKLY PLANS DATA';
PRINT '====================================';

-- Get the first available user (will be used as employee)
DECLARE @Employee1 NVARCHAR(450);
DECLARE @Employee2 NVARCHAR(450);
DECLARE @Employee3 NVARCHAR(450);

SELECT TOP 3 
    @Employee1 = CASE WHEN ROW_NUMBER() OVER (ORDER BY Id) = 1 THEN Id ELSE @Employee1 END,
    @Employee2 = CASE WHEN ROW_NUMBER() OVER (ORDER BY Id) = 2 THEN Id ELSE @Employee2 END,
    @Employee3 = CASE WHEN ROW_NUMBER() OVER (ORDER BY Id) = 3 THEN Id ELSE @Employee3 END
FROM AspNetUsers
WHERE Id IS NOT NULL;

-- If we don't have enough users, use the same user for all
SET @Employee2 = ISNULL(@Employee2, @Employee1);
SET @Employee3 = ISNULL(@Employee3, @Employee1);

PRINT 'Using Users:';
PRINT '  Employee 1: ' + @Employee1;
PRINT '  Employee 2: ' + @Employee2;
PRINT '  Employee 3: ' + @Employee3;
PRINT '';

BEGIN TRANSACTION;

BEGIN TRY

-- ==========================================
-- PLAN 1: Current Week (In Progress)
-- ==========================================
DECLARE @Plan1 INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'Weekly Plan Oct 1-7, 2024',
    N'Focus on major hospitals in Cairo',
    '2024-10-01', '2024-10-07',
    @Employee1,
    GETDATE(), GETDATE(), 1
);
SET @Plan1 = SCOPE_IDENTITY();

INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan1, N'Visit Hospital 57357', N'Present new medical equipment', 1, 1, GETDATE(), GETDATE(), 1),
    (@Plan1, N'Follow up with Dar El Fouad', N'Check on last week quote', 0, 2, GETDATE(), GETDATE(), 1),
    (@Plan1, N'Prepare monthly sales report', N'Compile September sales data', 0, 3, GETDATE(), GETDATE(), 1);

INSERT INTO DailyProgresses (WeeklyPlanId, ProgressDate, Notes, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan1, '2024-10-01', N'Successfully visited Hospital 57357. Great interest shown in new equipment.', GETDATE(), GETDATE(), 1);

PRINT 'Created Plan 1: Current Week (In Progress)';

-- ==========================================
-- PLAN 2: Last Week (Excellent - Reviewed)
-- ==========================================
DECLARE @Plan2 INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'Weekly Plan Sep 24-30, 2024',
    N'End of month deals closing',
    '2024-09-24', '2024-09-30',
    @Employee1,
    5, N'Excellent performance! All tasks completed on time.',
    DATEADD(DAY, -3, GETDATE()),
    DATEADD(DAY, -7, GETDATE()),
    DATEADD(DAY, -3, GETDATE()),
    1
);
SET @Plan2 = SCOPE_IDENTITY();

INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan2, N'Close Nile Badrawi Hospital deal', N'Sign final contract', 1, 1, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1),
    (@Plan2, N'Collect overdue payments', N'From 3 hospitals', 1, 2, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1);

PRINT 'Created Plan 2: Last Week (Excellent)';

-- ==========================================
-- PLAN 3: Employee 2 - Current Week
-- ==========================================
DECLARE @Plan3 INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'Week Plan Oct 1-7',
    N'Field visits to Alexandria hospitals',
    '2024-10-01', '2024-10-07',
    @Employee2,
    GETDATE(), GETDATE(), 1
);
SET @Plan3 = SCOPE_IDENTITY();

INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan3, N'Visit Alexandria International Hospital', N'Present new radiology equipment', 0, 1, GETDATE(), GETDATE(), 1),
    (@Plan3, N'Follow up Misr Aviation Hospital', N'Answer their questions', 1, 2, GETDATE(), GETDATE(), 1);

PRINT 'Created Plan 3: Employee 2 Current Week';

-- ==========================================
-- PLAN 4: Employee 2 - Last Week (Good)
-- ==========================================
DECLARE @Plan4 INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'Week Plan Sep 24-30',
    N'Focus on Alexandria area',
    '2024-09-24', '2024-09-30',
    @Employee2,
    4, N'Good performance! Most tasks completed.',
    DATEADD(DAY, -2, GETDATE()),
    DATEADD(DAY, -7, GETDATE()),
    DATEADD(DAY, -2, GETDATE()),
    1
);
SET @Plan4 = SCOPE_IDENTITY();

INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan4, N'Visit 5 hospitals in Alexandria', N'Comprehensive field tour', 1, 1, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1),
    (@Plan4, N'Prepare 3 quotations', N'For new hospitals', 1, 2, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1);

PRINT 'Created Plan 4: Employee 2 Last Week (Good)';

-- ==========================================
-- PLAN 5: Employee 3 - Current Week
-- ==========================================
DECLARE @Plan5 INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'First Week October Plan',
    N'Focus on Delta hospitals',
    '2024-10-01', '2024-10-07',
    @Employee3,
    GETDATE(), GETDATE(), 1
);
SET @Plan5 = SCOPE_IDENTITY();

INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan5, N'Visit Mansoura International Hospital', N'Present new surgery equipment', 1, 1, GETDATE(), GETDATE(), 1),
    (@Plan5, N'Visit Tanta University Hospital', N'Discuss comprehensive maintenance contract', 1, 2, GETDATE(), GETDATE(), 1),
    (@Plan5, N'Prepare competitors report', N'Analyze competitor prices in the area', 0, 3, GETDATE(), GETDATE(), 1);

INSERT INTO DailyProgresses (WeeklyPlanId, ProgressDate, Notes, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan5, '2024-10-01', N'Very successful visit to Mansoura International Hospital. They showed great interest.', GETDATE(), GETDATE(), 1),
    (@Plan5, '2024-10-02', N'Productive meeting with Tanta University Hospital management. Initial agreement on maintenance contract.', GETDATE(), GETDATE(), 1);

PRINT 'Created Plan 5: Employee 3 Current Week';

-- ==========================================
-- PLAN 6: Employee 3 - Last Week (Average)
-- ==========================================
DECLARE @Plan6 INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'Week Plan Sep 24-30',
    N'Delta field visits',
    '2024-09-24', '2024-09-30',
    @Employee3,
    3, N'Acceptable performance, but needs more detailed daily reports.',
    DATEADD(DAY, -1, GETDATE()),
    DATEADD(DAY, -7, GETDATE()),
    DATEADD(DAY, -1, GETDATE()),
    1
);
SET @Plan6 = SCOPE_IDENTITY();

INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan6, N'Visit 4 hospitals', N'Field tour', 1, 1, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1),
    (@Plan6, N'Prepare quotations', N'2 new quotes', 0, 2, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1);

PRINT 'Created Plan 6: Employee 3 Last Week (Average)';

-- ==========================================
-- PLAN 7: Employee 1 - Two Weeks Ago (Very Good)
-- ==========================================
DECLARE @Plan7 INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'Week Plan Sep 17-23',
    N'Expansion in New Cairo area',
    '2024-09-17', '2024-09-23',
    @Employee1,
    4, N'Very good performance! Daily reports were excellent.',
    DATEADD(DAY, -10, GETDATE()),
    DATEADD(DAY, -14, GETDATE()),
    DATEADD(DAY, -10, GETDATE()),
    1
);
SET @Plan7 = SCOPE_IDENTITY();

INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan7, N'Open 3 new clients', N'In Fifth Settlement area', 1, 1, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1),
    (@Plan7, N'Prepare market study', N'Analyze New Cairo market', 1, 2, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1);

PRINT 'Created Plan 7: Employee 1 Two Weeks Ago (Very Good)';

-- ==========================================
-- PLAN 8: Employee 2 - Two Weeks Ago (Needs Improvement)
-- ==========================================
DECLARE @Plan8 INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'Week Plan Sep 17-23',
    N'Follow up existing clients',
    '2024-09-17', '2024-09-23',
    @Employee2,
    2, N'Needs improvement. Some important tasks were not completed. Please stick to the plan.',
    DATEADD(DAY, -9, GETDATE()),
    DATEADD(DAY, -14, GETDATE()),
    DATEADD(DAY, -9, GETDATE()),
    1
);
SET @Plan8 = SCOPE_IDENTITY();

INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan8, N'Follow up 10 clients', N'Ensure their satisfaction', 1, 1, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1),
    (@Plan8, N'Solve pending complaints', N'5 old complaints', 0, 2, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1);

PRINT 'Created Plan 8: Employee 2 Two Weeks Ago (Needs Improvement)';

-- ==========================================
-- PLAN 9: Employee 3 - Two Weeks Ago (Excellent)
-- ==========================================
DECLARE @Plan9 INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'Mid September Plan',
    N'Preparation for month end',
    '2024-09-17', '2024-09-23',
    @Employee3,
    5, N'Excellent! Best week so far. Keep it up!',
    DATEADD(DAY, -8, GETDATE()),
    DATEADD(DAY, -14, GETDATE()),
    DATEADD(DAY, -8, GETDATE()),
    1
);
SET @Plan9 = SCOPE_IDENTITY();

INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan9, N'Close 2 major deals', N'Mansoura and Tanta hospitals', 1, 1, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1),
    (@Plan9, N'Prepare end of month plan', N'Get ready for next quarter', 1, 2, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1);

INSERT INTO DailyProgresses (WeeklyPlanId, ProgressDate, Notes, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan9, '2024-09-17', N'Closed Mansoura Hospital deal worth 400,000 EGP!', DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1),
    (@Plan9, '2024-09-18', N'Closed Tanta Hospital deal worth 350,000 EGP!', DATEADD(DAY, -13, GETDATE()), DATEADD(DAY, -13, GETDATE()), 1);

PRINT 'Created Plan 9: Employee 3 Two Weeks Ago (Excellent)';

-- ==========================================
-- PLAN 10: Employee 1 - Future Week (Planning)
-- ==========================================
DECLARE @Plan10 INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'Second Week October Plan',
    N'Expansion to new areas',
    '2024-10-08', '2024-10-14',
    @Employee1,
    GETDATE(), GETDATE(), 1
);
SET @Plan10 = SCOPE_IDENTITY();

INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan10, N'Explore market in Sharkia', N'Visit 5 new hospitals', 0, 1, GETDATE(), GETDATE(), 1),
    (@Plan10, N'Prepare special offers', N'Special offers for new clients', 0, 2, GETDATE(), GETDATE(), 1),
    (@Plan10, N'Follow up pending deals', N'Close 3 deals from last month', 0, 3, GETDATE(), GETDATE(), 1);

PRINT 'Created Plan 10: Employee 1 Next Week (Planning)';

COMMIT TRANSACTION;

PRINT '';
PRINT '====================================';
PRINT 'SUCCESS! 10 WEEKLY PLANS CREATED';
PRINT '====================================';
PRINT '';
PRINT 'Summary:';
PRINT '- Total Plans: 10';
PRINT '- Employee 1: 4 plans';
PRINT '- Employee 2: 3 plans';
PRINT '- Employee 3: 3 plans';
PRINT '';

-- Verify
SELECT 
    '📊 VERIFICATION' AS Info,
    (SELECT COUNT(*) FROM WeeklyPlans WHERE IsActive = 1) AS [Total Plans],
    (SELECT COUNT(*) FROM WeeklyPlanTasks WHERE IsActive = 1) AS [Total Tasks],
    (SELECT COUNT(*) FROM DailyProgresses WHERE IsActive = 1) AS [Daily Progress Notes];

PRINT '';
PRINT '✅ You can now refresh your API and see the data!';
PRINT '====================================';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '❌ ERROR: ' + ERROR_MESSAGE();
    PRINT '';
END CATCH

GO
