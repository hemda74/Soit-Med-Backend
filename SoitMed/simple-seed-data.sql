-- =====================================================
-- Simple Data Seeding Script
-- =====================================================
-- This script only inserts data, assuming tables already exist

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
-- Step 2: Check what tables exist
-- =====================================================
PRINT 'Checking existing tables...';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') AND type in (N'U'))
    PRINT '✓ Clients table exists'
ELSE
    PRINT '✗ Clients table does NOT exist';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WeeklyPlans]') AND type in (N'U'))
    PRINT '✓ WeeklyPlans table exists'
ELSE
    PRINT '✗ WeeklyPlans table does NOT exist';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WeeklyPlanItems]') AND type in (N'U'))
    PRINT '✓ WeeklyPlanItems table exists'
ELSE
    PRINT '✗ WeeklyPlanItems table does NOT exist';

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalesReports]') AND type in (N'U'))
    PRINT '✓ SalesReports table exists'
ELSE
    PRINT '✗ SalesReports table does NOT exist';

-- =====================================================
-- Step 3: Show table structures if they exist
-- =====================================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WeeklyPlans]') AND type in (N'U'))
BEGIN
    PRINT '';
    PRINT 'WeeklyPlans table structure:';
    SELECT 
        COLUMN_NAME,
        DATA_TYPE,
        CHARACTER_MAXIMUM_LENGTH,
        IS_NULLABLE
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'WeeklyPlans'
    ORDER BY ORDINAL_POSITION;
END

-- =====================================================
-- Step 4: Only insert data if tables exist
-- =====================================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') AND type in (N'U'))
BEGIN
    PRINT '';
    PRINT 'Inserting sample clients...';
    
    -- Check if clients already exist
    IF NOT EXISTS (SELECT 1 FROM Clients WHERE CreatedBy = @UserId)
    BEGIN
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
    END
    ELSE
    BEGIN
        PRINT 'Clients already exist for this user.';
    END
END
ELSE
BEGIN
    PRINT 'ERROR: Clients table does not exist. Cannot insert client data.';
END

-- =====================================================
-- Step 5: Insert Weekly Plan (only if table exists and has correct structure)
-- =====================================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WeeklyPlans]') AND type in (N'U'))
BEGIN
    -- Check if WeeklyPlans table has the required columns
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'WeeklyPlans' AND COLUMN_NAME = 'PlanTitle')
    BEGIN
        PRINT '';
        PRINT 'Inserting weekly plan...';
        
        -- Check if weekly plan already exists
        IF NOT EXISTS (SELECT 1 FROM WeeklyPlans WHERE EmployeeId = @UserId)
        BEGIN
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
        END
        ELSE
        BEGIN
            PRINT 'Weekly plan already exists for this user.';
        END
    END
    ELSE
    BEGIN
        PRINT 'ERROR: WeeklyPlans table exists but does not have the expected structure (missing PlanTitle column).';
        PRINT 'Please check the table structure and ensure it matches the expected schema.';
    END
END
ELSE
BEGIN
    PRINT 'ERROR: WeeklyPlans table does not exist. Cannot insert weekly plan data.';
END

-- =====================================================
-- Step 6: Summary
-- =====================================================
PRINT '';
PRINT '=====================================================';
PRINT 'SCRIPT EXECUTION COMPLETED';
PRINT '=====================================================';

-- Show current data counts
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Clients]') AND type in (N'U'))
BEGIN
    SELECT 'Clients' as TableName, COUNT(*) as RecordCount FROM Clients WHERE CreatedBy = @UserId;
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WeeklyPlans]') AND type in (N'U'))
BEGIN
    SELECT 'WeeklyPlans' as TableName, COUNT(*) as RecordCount FROM WeeklyPlans WHERE EmployeeId = @UserId;
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SalesReports]') AND type in (N'U'))
BEGIN
    SELECT 'SalesReports' as TableName, COUNT(*) as RecordCount FROM SalesReports WHERE EmployeeId = @UserId;
END
