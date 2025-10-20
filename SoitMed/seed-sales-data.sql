-- =====================================================
-- SoitMed Sales Data Seeding Script
-- =====================================================
-- This script seeds sample sales data for testing
-- Run this script in your SQL Server database

-- =====================================================
-- Step 1: Get User ID for ahmed@soitmed.com
-- =====================================================
DECLARE @UserId NVARCHAR(450) = (SELECT Id FROM AspNetUsers WHERE Email = 'ahmed@soitmed.com');

-- Check if user exists
IF @UserId IS NULL
BEGIN
    PRINT 'ERROR: User ahmed@soitmed.com not found. Please ensure the user exists first.';
    RETURN;
END

PRINT 'Found User ID: ' + @UserId;

-- =====================================================
-- Step 2: Insert Sample Clients
-- =====================================================
PRINT 'Inserting sample clients...';

INSERT INTO Clients (
    Name, Type, Specialization, Location, Phone, Email, Website, 
    Address, City, Governorate, PostalCode, Notes, Status, Priority, 
    PotentialValue, ContactPerson, ContactPersonPhone, ContactPersonEmail, 
    ContactPersonPosition, LastContactDate, NextContactDate, SatisfactionRating, 
    CreatedBy, AssignedTo, CreatedAt, UpdatedAt
)
VALUES 
-- Client 1: Doctor
('د. أحمد محمد', 'Doctor', 'جراحة القلب', 'القاهرة', '01234567890', 
 'ahmed.mohamed@hospital.com', NULL, 'شارع التحرير', 'القاهرة', 'القاهرة', '11511', 
 'عميل مهم - يهتم بالأجهزة المتطورة', 'Active', 'High', 50000.00, 
 'د. أحمد محمد', '01234567890', 'ahmed.mohamed@hospital.com', 'رئيس قسم جراحة القلب', 
 '2024-01-10 10:00:00', '2024-01-17 10:00:00', 4, 
 @UserId, @UserId, GETUTCDATE(), GETUTCDATE()),

-- Client 2: Hospital
('مستشفى النيل', 'Hospital', 'مستشفى عام', 'الجيزة', '02345678901', 
 'info@nilehospital.com', 'www.nilehospital.com', 'شارع الهرم', 'الجيزة', 'الجيزة', '12511', 
 'مستشفى كبير - يحتاج أجهزة متعددة', 'Active', 'Medium', 100000.00, 
 'د. فاطمة علي', '02345678901', 'fatma.ali@nilehospital.com', 'مدير المشتريات', 
 '2024-01-12 14:00:00', '2024-01-16 14:00:00', 5, 
 @UserId, @UserId, GETUTCDATE(), GETUTCDATE()),

-- Client 3: Clinic
('عيادة الأمل', 'Clinic', 'عيادة أطفال', 'الإسكندرية', '03456789012', 
 'info@amalclinic.com', NULL, 'شارع سيدي بشر', 'الإسكندرية', 'الإسكندرية', '21511', 
 'عيادة صغيرة - تحتاج أجهزة أساسية', 'Potential', 'Low', 25000.00, 
 'د. سارة أحمد', '03456789012', 'sara.ahmed@amalclinic.com', 'مدير العيادة', 
 '2024-01-08 16:00:00', '2024-01-20 16:00:00', 3, 
 @UserId, @UserId, GETUTCDATE(), GETUTCDATE());

PRINT 'Clients inserted successfully.';

-- =====================================================
-- Step 3: Insert Weekly Plan
-- =====================================================
PRINT 'Inserting weekly plan...';

DECLARE @WeeklyPlanId INT;

INSERT INTO WeeklyPlans (
    EmployeeId, WeekStartDate, WeekEndDate, PlanTitle, PlanDescription, 
    Status, SubmittedAt, CreatedAt, UpdatedAt
)
VALUES (
    @UserId, 
    '2024-01-15 00:00:00', 
    '2024-01-21 23:59:59', 
    'خطة الأسبوع الحالي', 
    'زيارة العملاء الرئيسيين ومتابعة العروض المعلقة - أسبوع مكثف للتركيز على العملاء ذوي الأولوية العالية', 
    'Submitted', 
    GETUTCDATE(), 
    GETUTCDATE(), 
    GETUTCDATE()
);

SET @WeeklyPlanId = SCOPE_IDENTITY();
PRINT 'Weekly plan inserted with ID: ' + CAST(@WeeklyPlanId AS NVARCHAR(20));

-- =====================================================
-- Step 4: Insert Weekly Plan Items
-- =====================================================
PRINT 'Inserting weekly plan items...';

-- Get Client IDs
DECLARE @Client1Id INT = (SELECT Id FROM Clients WHERE Name = 'د. أحمد محمد');
DECLARE @Client2Id INT = (SELECT Id FROM Clients WHERE Name = 'مستشفى النيل');
DECLARE @Client3Id INT = (SELECT Id FROM Clients WHERE Name = 'عيادة الأمل');

INSERT INTO WeeklyPlanItems (
    WeeklyPlanId, ClientId, ClientName, ClientType, ClientSpecialization, 
    ClientLocation, ClientPhone, ClientEmail, PlannedVisitDate, PlannedVisitTime, 
    VisitPurpose, VisitNotes, Priority, Status, IsNewClient, ActualVisitDate, 
    Results, SatisfactionRating, CreatedAt, UpdatedAt
)
VALUES 
-- Plan Item 1: Completed Visit
(@WeeklyPlanId, @Client1Id, 'د. أحمد محمد', 'Doctor', 'جراحة القلب', 
 'القاهرة', '01234567890', 'ahmed.mohamed@hospital.com', 
 '2024-01-16 10:00:00', '10:00', 
 'زيارة د. أحمد محمد لمناقشة عرض الأجهزة الجديدة', NULL, 
 'High', 'Completed', 0, '2024-01-16 10:00:00', 
 'تمت الزيارة بنجاح - العميل مهتم بالعرض', 4, 
 GETUTCDATE(), GETUTCDATE()),

-- Plan Item 2: Planned Visit
(@WeeklyPlanId, @Client2Id, 'مستشفى النيل', 'Hospital', 'مستشفى عام', 
 'الجيزة', '02345678901', 'info@nilehospital.com', 
 '2024-01-18 14:00:00', '14:00', 
 'متابعة عرض مستشفى النيل', 'في انتظار رد من مدير المشتريات', 
 'Medium', 'Planned', 0, NULL, 
 NULL, NULL, 
 GETUTCDATE(), GETUTCDATE()),

-- Plan Item 3: Scheduled Call
(@WeeklyPlanId, @Client3Id, 'عيادة الأمل', 'Clinic', 'عيادة أطفال', 
 'الإسكندرية', '03456789012', 'info@amalclinic.com', 
 '2024-01-20 16:00:00', '16:00', 
 'مكالمة مع عيادة الأمل لتحديد موعد الزيارة', 'تأجيل المكالمة لليوم التالي', 
 'Low', 'Planned', 0, NULL, 
 NULL, NULL, 
 GETUTCDATE(), GETUTCDATE());

PRINT 'Weekly plan items inserted successfully.';

-- =====================================================
-- Step 5: Insert Sales Reports
-- =====================================================
PRINT 'Inserting sales reports...';

INSERT INTO SalesReports (
    Title, Body, Type, ReportDate, EmployeeId, Rating, Comment, 
    CreatedAt, UpdatedAt, IsActive
)
VALUES 
-- Monthly Report
('تقرير المبيعات الشهري', 
 'شهر جيد - زيادة في المبيعات بنسبة 20%. إجمالي المبيعات: 150,000 جنيه. عملاء جدد: 3. زيارات مكتملة: 15. صفقات مغلقة: 2', 
 'monthly', '2024-01-05', @UserId, 4, 'أداء ممتاز - استمر في نفس الوتيرة', 
 GETUTCDATE(), GETUTCDATE(), 1),

-- Weekly Report
('تقرير المبيعات الأسبوعي', 
 'أسبوع متوسط - التركيز على العملاء الكبار. إجمالي المبيعات: 45,000 جنيه. عملاء جدد: 1. زيارات مكتملة: 5. صفقات مغلقة: 1', 
 'weekly', '2024-01-12', @UserId, 3, 'جيد - ركز على المتابعة', 
 GETUTCDATE(), GETUTCDATE(), 1);

PRINT 'Sales reports inserted successfully.';

-- =====================================================
-- Step 6: Summary
-- =====================================================
PRINT '=====================================================';
PRINT 'DATA SEEDING COMPLETED SUCCESSFULLY!';
PRINT '=====================================================';
PRINT 'Summary of inserted data:';
PRINT '- 3 Clients (Doctor, Hospital, Clinic)';
PRINT '- 1 Weekly Plan with 3 plan items';
PRINT '- 2 Sales Reports (Monthly & Weekly)';
PRINT '=====================================================';

-- Verify data
SELECT 'Clients' as TableName, COUNT(*) as RecordCount FROM Clients WHERE CreatedBy = @UserId
UNION ALL
SELECT 'WeeklyPlans', COUNT(*) FROM WeeklyPlans WHERE EmployeeId = @UserId
UNION ALL
SELECT 'WeeklyPlanItems', COUNT(*) FROM WeeklyPlanItems wp 
    INNER JOIN WeeklyPlans w ON wp.WeeklyPlanId = w.Id 
    WHERE w.EmployeeId = @UserId
UNION ALL
SELECT 'SalesReports', COUNT(*) FROM SalesReports WHERE EmployeeId = @UserId;
