-- ==========================================
-- SEED DATA FOR WEEKLY PLANS
-- 10 Weekly Plans for 3 Salesman Users
-- ==========================================

-- First, let's check if we have Salesman users
PRINT '====================================';
PRINT 'SEEDING WEEKLY PLANS DATA';
PRINT '====================================';
PRINT '';

-- Get or Create 3 Salesman Users
DECLARE @Salesman1 NVARCHAR(450);
DECLARE @Salesman2 NVARCHAR(450);
DECLARE @Salesman3 NVARCHAR(450);

-- Get existing Salesman users (first 3)
SELECT TOP 3 
    @Salesman1 = CASE WHEN ROW_NUMBER() OVER (ORDER BY Id) = 1 THEN Id ELSE @Salesman1 END,
    @Salesman2 = CASE WHEN ROW_NUMBER() OVER (ORDER BY Id) = 2 THEN Id ELSE @Salesman2 END,
    @Salesman3 = CASE WHEN ROW_NUMBER() OVER (ORDER BY Id) = 3 THEN Id ELSE @Salesman3 END
FROM AspNetUsers u
INNER JOIN AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN AspNetRoles r ON ur.RoleId = r.Id
WHERE r.Name = 'Salesman';

-- If we don't have enough Salesman users, use any users
IF @Salesman1 IS NULL OR @Salesman2 IS NULL OR @Salesman3 IS NULL
BEGIN
    SELECT TOP 3 
        @Salesman1 = CASE WHEN ROW_NUMBER() OVER (ORDER BY Id) = 1 THEN Id ELSE @Salesman1 END,
        @Salesman2 = CASE WHEN ROW_NUMBER() OVER (ORDER BY Id) = 2 THEN Id ELSE @Salesman2 END,
        @Salesman3 = CASE WHEN ROW_NUMBER() OVER (ORDER BY Id) = 3 THEN Id ELSE @Salesman3 END
    FROM AspNetUsers;
END

PRINT 'Using Salesman Users:';
PRINT 'Salesman 1: ' + ISNULL(@Salesman1, 'NOT FOUND');
PRINT 'Salesman 2: ' + ISNULL(@Salesman2, 'NOT FOUND');
PRINT 'Salesman 3: ' + ISNULL(@Salesman3, 'NOT FOUND');
PRINT '';

-- Check if we have users
IF @Salesman1 IS NULL
BEGIN
    PRINT '❌ ERROR: No users found in the database!';
    PRINT 'Please create users first.';
    RETURN;
END

BEGIN TRANSACTION;

BEGIN TRY

-- ==========================================
-- WEEKLY PLAN #1 - Salesman 1 - Current Week (In Progress)
-- ==========================================
DECLARE @Plan1Id INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'خطة الأسبوع الأول من أكتوبر',
    N'التركيز على مستشفيات القاهرة الكبرى',
    '2024-10-01',
    '2024-10-07',
    @Salesman1,
    NULL, -- Not reviewed yet
    NULL,
    NULL,
    GETDATE(),
    GETDATE(),
    1
);
SET @Plan1Id = SCOPE_IDENTITY();

-- Tasks for Plan 1
INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan1Id, N'زيارة مستشفى 57357', N'عرض المعدات الطبية الجديدة والمناقشة مع الإدارة', 1, 1, GETDATE(), GETDATE(), 1),
    (@Plan1Id, N'متابعة عرض مستشفى دار الفؤاد', N'متابعة العرض المقدم الأسبوع الماضي', 1, 2, GETDATE(), GETDATE(), 1),
    (@Plan1Id, N'إعداد تقرير المبيعات الشهري', N'تجميع بيانات مبيعات سبتمبر', 0, 3, GETDATE(), GETDATE(), 1),
    (@Plan1Id, N'الاتصال بمستشفى الجلاء', N'متابعة طلب الأسبوع الماضي', 0, 4, GETDATE(), GETDATE(), 1);

-- Daily Progress for Plan 1
INSERT INTO DailyProgresses (WeeklyPlanId, ProgressDate, Notes, TasksWorkedOn, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan1Id, '2024-10-01', N'اليوم قمت بزيارة مستشفى 57357 وتم عرض جميع المنتجات الجديدة. تم الاتفاق على موعد ثاني الأسبوع القادم لمناقشة العرض.', '1', GETDATE(), GETDATE(), 1),
    (@Plan1Id, '2024-10-02', N'اتصلت بمستشفى دار الفؤاد وتم تأكيد الموعد ليوم الأربعاء. بدأت في إعداد التقرير الشهري.', '2,3', GETDATE(), GETDATE(), 1);

PRINT '✅ Created Weekly Plan #1 (Salesman 1) - Current Week';

-- ==========================================
-- WEEKLY PLAN #2 - Salesman 1 - Last Week (Reviewed - Excellent)
-- ==========================================
DECLARE @Plan2Id INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'خطة الأسبوع الرابع من سبتمبر',
    N'إتمام صفقات نهاية الشهر',
    '2024-09-24',
    '2024-09-30',
    @Salesman1,
    5, -- Excellent
    N'أداء ممتاز! تم إنجاز جميع المهام في الوقت المحدد. استمر على هذا المستوى.',
    DATEADD(DAY, -3, GETDATE()),
    DATEADD(DAY, -7, GETDATE()),
    DATEADD(DAY, -3, GETDATE()),
    1
);
SET @Plan2Id = SCOPE_IDENTITY();

-- Tasks for Plan 2 (All completed)
INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan2Id, N'إغلاق صفقة مستشفى النيل بدراوي', N'توقيع العقد النهائي', 1, 1, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1),
    (@Plan2Id, N'متابعة التحصيلات المتأخرة', N'تحصيل المستحقات من 3 مستشفيات', 1, 2, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1),
    (@Plan2Id, N'تقرير نهاية الشهر', N'إعداد وتقديم التقرير للإدارة', 1, 3, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1);

-- Daily Progress for Plan 2
INSERT INTO DailyProgresses (WeeklyPlanId, ProgressDate, Notes, TasksWorkedOn, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan2Id, '2024-09-24', N'تم التوقيع على عقد مستشفى النيل بدراوي بقيمة 500 ألف جنيه.', '1', DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1),
    (@Plan2Id, '2024-09-25', N'تم تحصيل مستحقات من مستشفيين، المتبقي واحد فقط.', '2', DATEADD(DAY, -6, GETDATE()), DATEADD(DAY, -6, GETDATE()), 1),
    (@Plan2Id, '2024-09-26', N'تم تحصيل المستحقات المتبقية. بدأت في إعداد تقرير نهاية الشهر.', '2,3', DATEADD(DAY, -5, GETDATE()), DATEADD(DAY, -5, GETDATE()), 1);

PRINT '✅ Created Weekly Plan #2 (Salesman 1) - Last Week (Excellent)';

-- ==========================================
-- WEEKLY PLAN #3 - Salesman 2 - Current Week (Needs Improvement)
-- ==========================================
DECLARE @Plan3Id INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'خطة أسبوع 1-7 أكتوبر',
    N'زيارات ميدانية لمستشفيات الإسكندرية',
    '2024-10-01',
    '2024-10-07',
    @Salesman2,
    NULL,
    NULL,
    NULL,
    GETDATE(),
    GETDATE(),
    1
);
SET @Plan3Id = SCOPE_IDENTITY();

-- Tasks for Plan 3 (Mixed completion)
INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan3Id, N'زيارة مستشفى الإسكندرية الدولي', N'عرض منتجات الأشعة الجديدة', 0, 1, GETDATE(), GETDATE(), 1),
    (@Plan3Id, N'متابعة عرض مستشفى مصر للطيران', N'الرد على استفساراتهم', 1, 2, GETDATE(), GETDATE(), 1),
    (@Plan3Id, N'إعداد عروض أسعار جديدة', N'3 عروض لمستشفيات مختلفة', 0, 3, GETDATE(), GETDATE(), 1);

-- Daily Progress for Plan 3
INSERT INTO DailyProgresses (WeeklyPlanId, ProgressDate, Notes, TasksWorkedOn, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan3Id, '2024-10-01', N'تم التواصل مع مستشفى مصر للطيران والرد على جميع استفساراتهم بنجاح.', '2', GETDATE(), GETDATE(), 1);

PRINT '✅ Created Weekly Plan #3 (Salesman 2) - Current Week';

-- ==========================================
-- WEEKLY PLAN #4 - Salesman 2 - Previous Week (Good Performance)
-- ==========================================
DECLARE @Plan4Id INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'خطة الأسبوع الرابع من سبتمبر',
    N'التركيز على منطقة الإسكندرية',
    '2024-09-24',
    '2024-09-30',
    @Salesman2,
    4, -- Good
    N'أداء جيد! تم إنجاز معظم المهام. يمكن تحسين التواصل مع العملاء.',
    DATEADD(DAY, -2, GETDATE()),
    DATEADD(DAY, -7, GETDATE()),
    DATEADD(DAY, -2, GETDATE()),
    1
);
SET @Plan4Id = SCOPE_IDENTITY();

-- Tasks for Plan 4
INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan4Id, N'زيارة 5 مستشفيات في الإسكندرية', N'جولة ميدانية شاملة', 1, 1, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1),
    (@Plan4Id, N'إعداد 3 عروض أسعار', N'لمستشفيات جديدة', 1, 2, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1),
    (@Plan4Id, N'متابعة العملاء القدامى', N'التأكد من رضاهم', 1, 3, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1);

PRINT '✅ Created Weekly Plan #4 (Salesman 2) - Previous Week (Good)';

-- ==========================================
-- WEEKLY PLAN #5 - Salesman 3 - Current Week
-- ==========================================
DECLARE @Plan5Id INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'خطة الأسبوع الأول - أكتوبر',
    N'التركيز على مستشفيات الدلتا',
    '2024-10-01',
    '2024-10-07',
    @Salesman3,
    NULL,
    NULL,
    NULL,
    GETDATE(),
    GETDATE(),
    1
);
SET @Plan5Id = SCOPE_IDENTITY();

-- Tasks for Plan 5
INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan5Id, N'زيارة مستشفى المنصورة الدولي', N'عرض معدات العمليات الجديدة', 1, 1, GETDATE(), GETDATE(), 1),
    (@Plan5Id, N'زيارة مستشفى طنطا الجامعي', N'مناقشة عقد صيانة شامل', 1, 2, GETDATE(), GETDATE(), 1),
    (@Plan5Id, N'إعداد تقرير المنافسين', N'تحليل أسعار المنافسين في المنطقة', 0, 3, GETDATE(), GETDATE(), 1),
    (@Plan5Id, N'متابعة شكاوى العملاء', N'حل 3 شكاوى عالقة', 1, 4, GETDATE(), GETDATE(), 1);

-- Daily Progress for Plan 5
INSERT INTO DailyProgresses (WeeklyPlanId, ProgressDate, Notes, TasksWorkedOn, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan5Id, '2024-10-01', N'زيارة ناجحة جداً لمستشفى المنصورة الدولي. أبدوا اهتماماً كبيراً بالمعدات الجديدة.', '1', GETDATE(), GETDATE(), 1),
    (@Plan5Id, '2024-10-02', N'اجتماع مثمر مع إدارة مستشفى طنطا الجامعي. تم الاتفاق المبدئي على عقد الصيانة. تم حل شكوى أحد العملاء.', '2,4', GETDATE(), GETDATE(), 1);

PRINT '✅ Created Weekly Plan #5 (Salesman 3) - Current Week';

-- ==========================================
-- WEEKLY PLAN #6 - Salesman 3 - Previous Week (Average)
-- ==========================================
DECLARE @Plan6Id INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'خطة الأسبوع الرابع من سبتمبر',
    N'زيارات ميدانية للدلتا',
    '2024-09-24',
    '2024-09-30',
    @Salesman3,
    3, -- Average
    N'أداء مقبول، لكن يحتاج إلى مزيد من المتابعة اليومية والتقارير المفصلة.',
    DATEADD(DAY, -1, GETDATE()),
    DATEADD(DAY, -7, GETDATE()),
    DATEADD(DAY, -1, GETDATE()),
    1
);
SET @Plan6Id = SCOPE_IDENTITY();

-- Tasks for Plan 6
INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan6Id, N'زيارة 4 مستشفيات', N'جولة ميدانية', 1, 1, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1),
    (@Plan6Id, N'إعداد عروض الأسعار', N'2 عروض جديدة', 0, 2, DATEADD(DAY, -7, GETDATE()), DATEADD(DAY, -7, GETDATE()), 1);

PRINT '✅ Created Weekly Plan #6 (Salesman 3) - Previous Week (Average)';

-- ==========================================
-- WEEKLY PLAN #7 - Salesman 1 - Two Weeks Ago (Very Good)
-- ==========================================
DECLARE @Plan7Id INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'خطة الأسبوع الثالث من سبتمبر',
    N'التوسع في منطقة القاهرة الجديدة',
    '2024-09-17',
    '2024-09-23',
    @Salesman1,
    4,
    N'أداء جيد جداً! التقارير اليومية كانت ممتازة.',
    DATEADD(DAY, -10, GETDATE()),
    DATEADD(DAY, -14, GETDATE()),
    DATEADD(DAY, -10, GETDATE()),
    1
);
SET @Plan7Id = SCOPE_IDENTITY();

-- Tasks for Plan 7
INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan7Id, N'فتح 3 عملاء جدد', N'في منطقة التجمع الخامس', 1, 1, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1),
    (@Plan7Id, N'إعداد دراسة سوق', N'تحليل السوق في القاهرة الجديدة', 1, 2, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1),
    (@Plan7Id, N'تقديم 5 عروض أسعار', N'لمستشفيات جديدة', 1, 3, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1);

PRINT '✅ Created Weekly Plan #7 (Salesman 1) - Two Weeks Ago (Very Good)';

-- ==========================================
-- WEEKLY PLAN #8 - Salesman 2 - Two Weeks Ago (Needs Improvement)
-- ==========================================
DECLARE @Plan8Id INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'خطة أسبوع 17-23 سبتمبر',
    N'متابعة العملاء الحاليين',
    '2024-09-17',
    '2024-09-23',
    @Salesman2,
    2,
    N'يحتاج إلى تحسين. لم يتم إنجاز بعض المهام المهمة. يرجى الالتزام بالخطة.',
    DATEADD(DAY, -9, GETDATE()),
    DATEADD(DAY, -14, GETDATE()),
    DATEADD(DAY, -9, GETDATE()),
    1
);
SET @Plan8Id = SCOPE_IDENTITY();

-- Tasks for Plan 8
INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan8Id, N'متابعة 10 عملاء', N'التأكد من رضاهم', 1, 1, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1),
    (@Plan8Id, N'حل الشكاوى العالقة', N'5 شكاوى قديمة', 0, 2, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1),
    (@Plan8Id, N'إعداد تقرير المبيعات', N'تقرير أسبوعي مفصل', 0, 3, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1);

PRINT '✅ Created Weekly Plan #8 (Salesman 2) - Two Weeks Ago (Needs Improvement)';

-- ==========================================
-- WEEKLY PLAN #9 - Salesman 3 - Two Weeks Ago
-- ==========================================
DECLARE @Plan9Id INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'خطة منتصف سبتمبر',
    N'التحضير لنهاية الشهر',
    '2024-09-17',
    '2024-09-23',
    @Salesman3,
    5,
    N'ممتاز! أفضل أسبوع حتى الآن. استمر!',
    DATEADD(DAY, -8, GETDATE()),
    DATEADD(DAY, -14, GETDATE()),
    DATEADD(DAY, -8, GETDATE()),
    1
);
SET @Plan9Id = SCOPE_IDENTITY();

-- Tasks for Plan 9
INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan9Id, N'إغلاق 2 صفقات كبيرة', N'مستشفيات المنصورة وطنطا', 1, 1, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1),
    (@Plan9Id, N'إعداد خطة نهاية الشهر', N'الاستعداد للربع القادم', 1, 2, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1),
    (@Plan9Id, N'تدريب العملاء الجدد', N'على استخدام المعدات', 1, 3, DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1);

-- Daily Progress for Plan 9
INSERT INTO DailyProgresses (WeeklyPlanId, ProgressDate, Notes, TasksWorkedOn, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan9Id, '2024-09-17', N'تم إغلاق صفقة مستشفى المنصورة بقيمة 400 ألف جنيه!', '1', DATEADD(DAY, -14, GETDATE()), DATEADD(DAY, -14, GETDATE()), 1),
    (@Plan9Id, '2024-09-18', N'تم إغلاق صفقة مستشفى طنطا بقيمة 350 ألف جنيه!', '1', DATEADD(DAY, -13, GETDATE()), DATEADD(DAY, -13, GETDATE()), 1),
    (@Plan9Id, '2024-09-19', N'بدأت في إعداد خطة نهاية الشهر وتحديد الأهداف للربع القادم.', '2', DATEADD(DAY, -12, GETDATE()), DATEADD(DAY, -12, GETDATE()), 1);

PRINT '✅ Created Weekly Plan #9 (Salesman 3) - Two Weeks Ago (Excellent)';

-- ==========================================
-- WEEKLY PLAN #10 - Salesman 1 - Future Week (Planning)
-- ==========================================
DECLARE @Plan10Id INT;
INSERT INTO WeeklyPlans (Title, Description, WeekStartDate, WeekEndDate, EmployeeId, Rating, ManagerComment, ManagerReviewedAt, CreatedAt, UpdatedAt, IsActive)
VALUES (
    N'خطة الأسبوع الثاني من أكتوبر',
    N'التوسع في مناطق جديدة',
    '2024-10-08',
    '2024-10-14',
    @Salesman1,
    NULL,
    NULL,
    NULL,
    GETDATE(),
    GETDATE(),
    1
);
SET @Plan10Id = SCOPE_IDENTITY();

-- Tasks for Plan 10 (Future planning)
INSERT INTO WeeklyPlanTasks (WeeklyPlanId, Title, Description, IsCompleted, DisplayOrder, CreatedAt, UpdatedAt, IsActive)
VALUES 
    (@Plan10Id, N'استكشاف السوق في الشرقية', N'زيارة 5 مستشفيات جديدة', 0, 1, GETDATE(), GETDATE(), 1),
    (@Plan10Id, N'إعداد عروض مميزة', N'عروض خاصة للعملاء الجدد', 0, 2, GETDATE(), GETDATE(), 1),
    (@Plan10Id, N'متابعة الصفقات المعلقة', N'إغلاق 3 صفقات من الشهر الماضي', 0, 3, GETDATE(), GETDATE(), 1),
    (@Plan10Id, N'تحديث قاعدة البيانات', N'إضافة معلومات العملاء الجدد', 0, 4, GETDATE(), GETDATE(), 1),
    (@Plan10Id, N'إعداد تقرير الأسبوع', N'تقرير مفصل عن الزيارات', 0, 5, GETDATE(), GETDATE(), 1);

PRINT '✅ Created Weekly Plan #10 (Salesman 1) - Next Week (Planning)';

COMMIT TRANSACTION;

PRINT '';
PRINT '====================================';
PRINT '✅ SUCCESS! 10 WEEKLY PLANS CREATED';
PRINT '====================================';
PRINT '';
PRINT 'Summary:';
PRINT '- Salesman 1: 4 plans (Excellent performer)';
PRINT '- Salesman 2: 3 plans (Mixed performance)';
PRINT '- Salesman 3: 3 plans (Good performer)';
PRINT '';
PRINT 'Variations included:';
PRINT '✓ Current week plans (in progress)';
PRINT '✓ Previous week plans (reviewed)';
PRINT '✓ Future week plans (planning)';
PRINT '✓ Different ratings (1-5 stars)';
PRINT '✓ Manager comments';
PRINT '✓ Completed and incomplete tasks';
PRINT '✓ Daily progress notes';
PRINT '';
PRINT '🎉 You can now test all APIs!';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '❌ ERROR: ' + ERROR_MESSAGE();
    PRINT 'Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10));
    PRINT '';
    THROW;
END CATCH

GO

-- Verify the seeded data
SELECT 
    '📊 SEEDED DATA SUMMARY' AS [Info],
    (SELECT COUNT(*) FROM WeeklyPlans WHERE IsActive = 1) AS [Total Plans],
    (SELECT COUNT(*) FROM WeeklyPlanTasks WHERE IsActive = 1) AS [Total Tasks],
    (SELECT COUNT(*) FROM DailyProgresses WHERE IsActive = 1) AS [Total Progress Notes];

PRINT '';
PRINT 'Plans per Employee:';
SELECT 
    u.FirstName + ' ' + u.LastName AS [Employee Name],
    COUNT(wp.Id) AS [Plans Count],
    SUM(CASE WHEN wp.Rating IS NOT NULL THEN 1 ELSE 0 END) AS [Reviewed Plans],
    AVG(CAST(wp.Rating AS FLOAT)) AS [Average Rating]
FROM WeeklyPlans wp
INNER JOIN AspNetUsers u ON wp.EmployeeId = u.Id
WHERE wp.IsActive = 1
GROUP BY u.FirstName, u.LastName, u.Id;




