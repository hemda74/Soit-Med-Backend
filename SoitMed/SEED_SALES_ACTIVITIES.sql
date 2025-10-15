-- =============================================
-- Seed Sales Activities Data
-- =============================================

-- First, let's get the user IDs for the accounts
DECLARE @AhmedUserId NVARCHAR(450);
DECLARE @SalesSeederUserId NVARCHAR(450);
DECLARE @AhmedWeeklyPlanId INT;
DECLARE @SalesSeederWeeklyPlanId INT;

-- Get user IDs
SELECT @AhmedUserId = Id FROM AspNetUsers WHERE Email = 'ahmed@soitmed.com';
SELECT @SalesSeederUserId = Id FROM AspNetUsers WHERE UserName = 'salesseeder';

-- If users don't exist, create them
IF @AhmedUserId IS NULL
BEGIN
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FirstName, LastName, IsActive, CreatedAt, LastLoginAt)
    VALUES (NEWID(), 'ahmed@soitmed.com', 'AHMED@SOITMED.COM', 'ahmed@soitmed.com', 'AHMED@SOITMED.COM', 1, 'AQAAAAEAACcQAAAAEExampleHash', 'ExampleSecurityStamp', 'ExampleConcurrencyStamp', '+1234567890', 0, 0, NULL, 1, 0, 'Ahmed', 'Salesman', 1, GETUTCDATE(), GETUTCDATE());
    SELECT @AhmedUserId = Id FROM AspNetUsers WHERE Email = 'ahmed@soitmed.com';
END

IF @SalesSeederUserId IS NULL
BEGIN
    INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, FirstName, LastName, IsActive, CreatedAt, LastLoginAt)
    VALUES (NEWID(), 'salesseeder', 'SALESSEEDER', 'salesseeder@soitmed.com', 'SALESSEEDER@SOITMED.COM', 1, 'AQAAAAEAACcQAAAAEExampleHash', 'ExampleSecurityStamp', 'ExampleConcurrencyStamp', '+1234567891', 0, 0, NULL, 1, 0, 'Sales', 'Seeder', 1, GETUTCDATE(), GETUTCDATE());
    SELECT @SalesSeederUserId = Id FROM AspNetUsers WHERE UserName = 'salesseeder';
END

-- Create weekly plans for both users
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES 
('Ahmed Weekly Plan - Week 1', 'Sales activities for the first week', '2024-10-07', '2024-10-13', @AhmedUserId, NULL, NULL, NULL, GETUTCDATE(), GETUTCDATE(), 1),
('Sales Seeder Weekly Plan - Week 1', 'Sales activities for the first week', '2024-10-07', '2024-10-13', @SalesSeederUserId, NULL, NULL, NULL, GETUTCDATE(), GETUTCDATE(), 1);

-- Get the weekly plan IDs
SELECT @AhmedWeeklyPlanId = Id FROM WeeklyPlans WHERE EmployeeId = @AhmedUserId AND WeekStartDate = '2024-10-07';
SELECT @SalesSeederWeeklyPlanId = Id FROM WeeklyPlans WHERE EmployeeId = @SalesSeederUserId AND WeekStartDate = '2024-10-07';

-- Create tasks for Ahmed
INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
(@AhmedWeeklyPlanId, 'Visit 57357 Hospital', 'Initial visit to discuss MRI equipment needs', 0, 1, GETUTCDATE(), GETUTCDATE(), 1),
(@AhmedWeeklyPlanId, 'Follow up with Cairo Medical Center', 'Follow up on previous proposal for CT scanner', 0, 2, GETUTCDATE(), GETUTCDATE(), 1),
(@AhmedWeeklyPlanId, 'Present to Al-Azhar University Hospital', 'Present new ultrasound equipment proposal', 0, 3, GETUTCDATE(), GETUTCDATE(), 1);

-- Create tasks for Sales Seeder
INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
(@SalesSeederWeeklyPlanId, 'Visit National Cancer Institute', 'Discuss oncology equipment requirements', 0, 1, GETUTCDATE(), GETUTCDATE(), 1),
(@SalesSeederWeeklyPlanId, 'Follow up with Ain Shams Hospital', 'Follow up on X-ray machine proposal', 0, 2, GETUTCDATE(), GETUTCDATE(), 1),
(@SalesSeederWeeklyPlanId, 'Present to Dar Al-Fouad Hospital', 'Present new surgical equipment line', 0, 3, GETUTCDATE(), GETUTCDATE(), 1);

-- Get task IDs for Ahmed
DECLARE @AhmedTask1 INT, @AhmedTask2 INT, @AhmedTask3 INT;
SELECT @AhmedTask1 = Id FROM WeeklyPlanTasks WHERE WeeklyPlanId = @AhmedWeeklyPlanId AND Title = 'Visit 57357 Hospital';
SELECT @AhmedTask2 = Id FROM WeeklyPlanTasks WHERE WeeklyPlanId = @AhmedWeeklyPlanId AND Title = 'Follow up with Cairo Medical Center';
SELECT @AhmedTask3 = Id FROM WeeklyPlanTasks WHERE WeeklyPlanId = @AhmedWeeklyPlanId AND Title = 'Present to Al-Azhar University Hospital';

-- Get task IDs for Sales Seeder
DECLARE @SeederTask1 INT, @SeederTask2 INT, @SeederTask3 INT;
SELECT @SeederTask1 = Id FROM WeeklyPlanTasks WHERE WeeklyPlanId = @SalesSeederWeeklyPlanId AND Title = 'Visit National Cancer Institute';
SELECT @SeederTask2 = Id FROM WeeklyPlanTasks WHERE WeeklyPlanId = @SalesSeederWeeklyPlanId AND Title = 'Follow up with Ain Shams Hospital';
SELECT @SeederTask3 = Id FROM WeeklyPlanTasks WHERE WeeklyPlanId = @SalesSeederWeeklyPlanId AND Title = 'Present to Dar Al-Fouad Hospital';

-- Seed Activities for Ahmed (3 activities)
INSERT INTO ActivityLogs (PlanTaskId, UserId, InteractionType, ClientType, Result, Reason, Comment, CreatedAt, UpdatedAt)
VALUES 
-- Activity 1: Visit to 57357 Hospital - Interested with Deal
(@AhmedTask1, @AhmedUserId, 0, 0, 0, NULL, 'Successful visit to 57357 Hospital. Client showed strong interest in MRI machine. Discussed specifications and pricing.', DATEADD(day, -2, GETUTCDATE()), DATEADD(day, -2, GETUTCDATE())),

-- Activity 2: Follow up with Cairo Medical Center - Not Interested
(@AhmedTask2, @AhmedUserId, 1, 1, 1, 0, 'Follow-up call with Cairo Medical Center. Client mentioned budget constraints and decided not to proceed with CT scanner purchase.', DATEADD(day, -1, GETUTCDATE()), DATEADD(day, -1, GETUTCDATE())),

-- Activity 3: Present to Al-Azhar University Hospital - Interested with Offer
(@AhmedTask3, @AhmedUserId, 0, 2, 0, NULL, 'Presentation to Al-Azhar University Hospital was successful. They are interested in the new ultrasound equipment and requested detailed proposal.', DATEADD(hour, -3, GETUTCDATE()), DATEADD(hour, -3, GETUTCDATE()));

-- Seed Activities for Sales Seeder (3 activities)
INSERT INTO ActivityLogs (PlanTaskId, UserId, InteractionType, ClientType, Result, Reason, Comment, CreatedAt, UpdatedAt)
VALUES 
-- Activity 4: Visit National Cancer Institute - Interested with Deal
(@SeederTask1, @SalesSeederUserId, 0, 0, 0, NULL, 'Visit to National Cancer Institute was very productive. They need advanced oncology equipment and are ready to discuss terms.', DATEADD(day, -3, GETUTCDATE()), DATEADD(day, -3, GETUTCDATE())),

-- Activity 5: Follow up with Ain Shams Hospital - Interested with Offer
(@SeederTask2, @SalesSeederUserId, 1, 1, 0, NULL, 'Follow-up meeting with Ain Shams Hospital. They reviewed our X-ray machine proposal and requested some modifications.', DATEADD(day, -1, GETUTCDATE()), DATEADD(day, -1, GETUTCDATE())),

-- Activity 6: Present to Dar Al-Fouad Hospital - Not Interested
(@SeederTask3, @SalesSeederUserId, 0, 3, 1, 2, 'Presentation to Dar Al-Fouad Hospital. They mentioned they already have similar equipment and are not in need of new surgical equipment at this time.', DATEADD(hour, -1, GETUTCDATE()), DATEADD(hour, -1, GETUTCDATE()));

-- Get Activity IDs for creating Deals and Offers
DECLARE @AhmedActivity1 BIGINT, @AhmedActivity3 BIGINT, @SeederActivity1 BIGINT, @SeederActivity2 BIGINT;
SELECT @AhmedActivity1 = Id FROM ActivityLogs WHERE PlanTaskId = @AhmedTask1 AND UserId = @AhmedUserId;
SELECT @AhmedActivity3 = Id FROM ActivityLogs WHERE PlanTaskId = @AhmedTask3 AND UserId = @AhmedUserId;
SELECT @SeederActivity1 = Id FROM ActivityLogs WHERE PlanTaskId = @SeederTask1 AND UserId = @SalesSeederUserId;
SELECT @SeederActivity2 = Id FROM ActivityLogs WHERE PlanTaskId = @SeederTask2 AND UserId = @SalesSeederUserId;

-- Create Deals for interested activities
INSERT INTO Deals (ActivityLogId, UserId, DealValue, Status, ExpectedCloseDate, CreatedAt, UpdatedAt)
VALUES 
-- Deal for Ahmed's 57357 Hospital visit
(@AhmedActivity1, @AhmedUserId, 2500000.00, 0, DATEADD(month, 1, GETUTCDATE()), DATEADD(day, -2, GETUTCDATE()), DATEADD(day, -2, GETUTCDATE())),

-- Deal for Sales Seeder's National Cancer Institute visit
(@SeederActivity1, @SalesSeederUserId, 1800000.00, 0, DATEADD(month, 2, GETUTCDATE()), DATEADD(day, -3, GETUTCDATE()), DATEADD(day, -3, GETUTCDATE()));

-- Create Offers for interested activities
INSERT INTO Offers (ActivityLogId, UserId, OfferDetails, Status, DocumentUrl, CreatedAt, UpdatedAt)
VALUES 
-- Offer for Ahmed's Al-Azhar University Hospital presentation
(@AhmedActivity3, @AhmedUserId, 'Comprehensive ultrasound equipment proposal including installation, training, and 3-year maintenance contract. Total value: $850,000', 1, 'https://soitmed.com/proposals/ultrasound-al-azhar-2024.pdf', DATEADD(hour, -3, GETUTCDATE()), DATEADD(hour, -3, GETUTCDATE())),

-- Offer for Sales Seeder's Ain Shams Hospital follow-up
(@SeederActivity2, @SalesSeederUserId, 'Modified X-ray machine proposal with extended warranty and additional training sessions. Total value: $420,000', 1, 'https://soitmed.com/proposals/xray-ain-shams-2024.pdf', DATEADD(day, -1, GETUTCDATE()), DATEADD(day, -1, GETUTCDATE()));

PRINT 'Sales activities seeded successfully!';
PRINT 'Created 6 activities:';
PRINT '- 3 activities for ahmed@soitmed.com';
PRINT '- 3 activities for salesseeder';
PRINT '- 2 deals created for interested clients';
PRINT '- 2 offers created for interested clients';
PRINT 'Total seeded data: 6 activities, 2 deals, 2 offers';


