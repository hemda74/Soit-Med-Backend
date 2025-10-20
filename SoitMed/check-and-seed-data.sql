-- =====================================================
-- SoitMed Database Check and Data Seeding Script
-- =====================================================
-- This script checks if tables exist, creates them if needed, and seeds data

-- =====================================================
-- Step 1: Check if tables exist and create if needed
-- =====================================================
PRINT 'Checking database structure...';

-- Check if WeeklyPlans table exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WeeklyPlans]') AND type in (N'U'))
BEGIN
    PRINT 'WeeklyPlans table does not exist. Creating it...';
    
    -- Create WeeklyPlans table
    CREATE TABLE [WeeklyPlans] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [EmployeeId] nvarchar(450) NOT NULL,
        [WeekStartDate] datetime2 NOT NULL,
        [WeekEndDate] datetime2 NOT NULL,
        [PlanTitle] nvarchar(200) NOT NULL,
        [PlanDescription] nvarchar(1000) NULL,
        [Status] nvarchar(50) NOT NULL DEFAULT 'Draft',
        [ApprovalNotes] nvarchar(1000) NULL,
        [RejectionReason] nvarchar(1000) NULL,
        [SubmittedAt] datetime2 NULL,
        [ApprovedAt] datetime2 NULL,
        [RejectedAt] datetime2 NULL,
        [ApprovedBy] nvarchar(450) NULL,
        [RejectedBy] nvarchar(450) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_WeeklyPlans] PRIMARY KEY ([Id])
    );
    
    PRINT 'WeeklyPlans table created successfully.';
END
ELSE
BEGIN
    PRINT 'WeeklyPlans table already exists.';
END

-- Check if WeeklyPlanItems table exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WeeklyPlanItems]') AND type in (N'U'))
BEGIN
    PRINT 'WeeklyPlanItems table does not exist. Creating it...';
    
    -- Create WeeklyPlanItems table
    CREATE TABLE [WeeklyPlanItems] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [WeeklyPlanId] int NOT NULL,
        [ClientId] int NULL,
        [ClientName] nvarchar(200) NOT NULL,
        [ClientType] nvarchar(50) NULL,
        [ClientSpecialization] nvarchar(100) NULL,
        [ClientLocation] nvarchar(100) NULL,
        [ClientPhone] nvarchar(20) NULL,
        [ClientEmail] nvarchar(100) NULL,
        [PlannedVisitDate] datetime2 NOT NULL,
        [PlannedVisitTime] nvarchar(20) NULL,
        [VisitPurpose] nvarchar(500) NULL,
        [VisitNotes] nvarchar(1000) NULL,
        [Priority] nvarchar(50) NOT NULL DEFAULT 'Medium',
        [Status] nvarchar(50) NOT NULL DEFAULT 'Planned',
        [IsNewClient] bit NOT NULL DEFAULT 0,
        [ActualVisitDate] datetime2 NULL,
        [Results] nvarchar(2000) NULL,
        [Feedback] nvarchar(2000) NULL,
        [SatisfactionRating] int NULL,
        [NextVisitDate] datetime2 NULL,
        [FollowUpNotes] nvarchar(1000) NULL,
        [CancellationReason] nvarchar(1000) NULL,
        [PostponementReason] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_WeeklyPlanItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WeeklyPlanItems_WeeklyPlans_WeeklyPlanId] FOREIGN KEY ([WeeklyPlanId]) REFERENCES [WeeklyPlans] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_WeeklyPlanItems_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE SET NULL
    );
    
    PRINT 'WeeklyPlanItems table created successfully.';
END
ELSE
BEGIN
    PRINT 'WeeklyPlanItems table already exists.';
END

-- Check if Clients table exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') AND type in (N'U'))
BEGIN
    PRINT 'Clients table does not exist. Creating it...';
    
    -- Create Clients table
    CREATE TABLE [Clients] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        [Specialization] nvarchar(100) NULL,
        [Location] nvarchar(100) NULL,
        [Phone] nvarchar(20) NULL,
        [Email] nvarchar(100) NULL,
        [Website] nvarchar(200) NULL,
        [Address] nvarchar(500) NULL,
        [City] nvarchar(100) NULL,
        [Governorate] nvarchar(100) NULL,
        [PostalCode] nvarchar(20) NULL,
        [Notes] nvarchar(1000) NULL,
        [Status] nvarchar(50) NOT NULL DEFAULT 'Potential',
        [Priority] nvarchar(50) NOT NULL DEFAULT 'Medium',
        [PotentialValue] decimal(18,2) NULL,
        [ContactPerson] nvarchar(200) NULL,
        [ContactPersonPhone] nvarchar(20) NULL,
        [ContactPersonEmail] nvarchar(100) NULL,
        [ContactPersonPosition] nvarchar(100) NULL,
        [LastContactDate] datetime2 NULL,
        [NextContactDate] datetime2 NULL,
        [SatisfactionRating] int NULL,
        [CreatedBy] nvarchar(450) NOT NULL,
        [AssignedTo] nvarchar(450) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
    );
    
    PRINT 'Clients table created successfully.';
END
ELSE
BEGIN
    PRINT 'Clients table already exists.';
END

-- Check if SalesReports table exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalesReports]') AND type in (N'U'))
BEGIN
    PRINT 'SalesReports table does not exist. Creating it...';
    
    -- Create SalesReports table
    CREATE TABLE [SalesReports] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Title] nvarchar(100) NOT NULL,
        [Body] nvarchar(2000) NOT NULL,
        [Type] nvarchar(20) NOT NULL,
        [ReportDate] date NOT NULL,
        [EmployeeId] nvarchar(450) NOT NULL,
        [Rating] int NULL,
        [Comment] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
        [IsActive] bit NOT NULL DEFAULT 1,
        CONSTRAINT [PK_SalesReports] PRIMARY KEY ([Id])
    );
    
    PRINT 'SalesReports table created successfully.';
END
ELSE
BEGIN
    PRINT 'SalesReports table already exists.';
END

-- =====================================================
-- Step 2: Get User ID for ahmed@soitmed.com
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
-- Step 3: Insert Sample Clients
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
-- Step 4: Insert Weekly Plan
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
-- Step 5: Insert Weekly Plan Items
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
-- Step 6: Insert Sales Reports
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
-- Step 7: Summary
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
