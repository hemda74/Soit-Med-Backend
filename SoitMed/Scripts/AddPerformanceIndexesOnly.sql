-- ============================================
-- Performance Optimization Indexes Only
-- Safe to run multiple times (Idempotent)
-- ============================================

USE ITIWebApi44;
GO

-- ============================================
-- Step 1: Add Computed Columns (if not exist)
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clients') AND name = 'NameLower')
BEGIN
    ALTER TABLE Clients 
    ADD NameLower AS LOWER(Name) PERSISTED,
        EmailLower AS LOWER(ISNULL(Email, '')) PERSISTED,
        SpecializationLower AS LOWER(ISNULL(Specialization, '')) PERSISTED;
    
    PRINT 'Computed columns added successfully.';
END
ELSE
BEGIN
    PRINT 'Computed columns already exist. Skipping...';
END
GO

-- ============================================
-- Step 2: Create Indexes on Computed Columns
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Clients_NameLower' AND object_id = OBJECT_ID('Clients'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Clients_NameLower 
    ON Clients(NameLower);
    PRINT 'Index IX_Clients_NameLower created.';
END
ELSE
BEGIN
    PRINT 'Index IX_Clients_NameLower already exists. Skipping...';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Clients_EmailLower' AND object_id = OBJECT_ID('Clients'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Clients_EmailLower 
    ON Clients(EmailLower) 
    INCLUDE (Name, Phone, Email);
    PRINT 'Index IX_Clients_EmailLower created.';
END
ELSE
BEGIN
    PRINT 'Index IX_Clients_EmailLower already exists. Skipping...';
END
GO

-- ============================================
-- Step 3: TaskProgresses Indexes
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TaskProgresses_EmployeeId_ProgressDate_VisitResult' AND object_id = OBJECT_ID('TaskProgresses'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TaskProgresses_EmployeeId_ProgressDate_VisitResult
    ON TaskProgresses(EmployeeId, ProgressDate, VisitResult)
    INCLUDE (ProgressType, NextStep)
    WHERE VisitResult IS NOT NULL;
    PRINT 'Index IX_TaskProgresses_EmployeeId_ProgressDate_VisitResult created.';
END
ELSE
BEGIN
    PRINT 'Index IX_TaskProgresses_EmployeeId_ProgressDate_VisitResult already exists. Skipping...';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TaskProgresses_ClientId_ProgressDate' AND object_id = OBJECT_ID('TaskProgresses'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TaskProgresses_ClientId_ProgressDate
    ON TaskProgresses(ClientId, ProgressDate)
    INCLUDE (ProgressType, VisitResult, NextStep);
    PRINT 'Index IX_TaskProgresses_ClientId_ProgressDate created.';
END
ELSE
BEGIN
    PRINT 'Index IX_TaskProgresses_ClientId_ProgressDate already exists. Skipping...';
END
GO

-- ============================================
-- Step 4: SalesOffers Index
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SalesOffers_AssignedTo_CreatedAt_Status' AND object_id = OBJECT_ID('SalesOffers'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SalesOffers_AssignedTo_CreatedAt_Status
    ON SalesOffers(AssignedTo, CreatedAt, Status)
    INCLUDE (TotalAmount, ClientId);
    PRINT 'Index IX_SalesOffers_AssignedTo_CreatedAt_Status created.';
END
ELSE
BEGIN
    PRINT 'Index IX_SalesOffers_AssignedTo_CreatedAt_Status already exists. Skipping...';
END
GO

-- ============================================
-- Step 5: SalesDeals Index
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SalesDeals_SalesmanId_CreatedAt_Status' AND object_id = OBJECT_ID('SalesDeals'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SalesDeals_SalesmanId_CreatedAt_Status
    ON SalesDeals(SalesmanId, CreatedAt, Status)
    INCLUDE (DealValue, ClientId);
    PRINT 'Index IX_SalesDeals_SalesmanId_CreatedAt_Status created.';
END
ELSE
BEGIN
    PRINT 'Index IX_SalesDeals_SalesmanId_CreatedAt_Status already exists. Skipping...';
END
GO

-- ============================================
-- Step 6: Clients Combined Index
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Clients_NameLower_SpecializationLower' AND object_id = OBJECT_ID('Clients'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Clients_NameLower_SpecializationLower
    ON Clients(NameLower, Specialization)
    INCLUDE (Phone, Email, Status)
    WHERE IsActive = 1;
    PRINT 'Index IX_Clients_NameLower_SpecializationLower created.';
END
ELSE
BEGIN
    PRINT 'Index IX_Clients_NameLower_SpecializationLower already exists. Skipping...';
END
GO

-- ============================================
-- Step 7: Clients Follow-up Index
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Clients_NextContactDate_Status_AssignedTo' AND object_id = OBJECT_ID('Clients'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Clients_NextContactDate_Status_AssignedTo
    ON Clients(NextContactDate, Status, AssignedTo)
    INCLUDE (Name, Phone, Email)
    WHERE NextContactDate IS NOT NULL AND Status = 'Active';
    PRINT 'Index IX_Clients_NextContactDate_Status_AssignedTo created.';
END
ELSE
BEGIN
    PRINT 'Index IX_Clients_NextContactDate_Status_AssignedTo already exists. Skipping...';
END
GO

-- ============================================
-- Step 8: WeeklyPlanTasks Index
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WeeklyPlanTasks_WeeklyPlanId_Status_PlannedDate' AND object_id = OBJECT_ID('WeeklyPlanTasks'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WeeklyPlanTasks_WeeklyPlanId_Status_PlannedDate
    ON WeeklyPlanTasks(WeeklyPlanId, Status, PlannedDate)
    INCLUDE (ClientId, TaskType, Priority);
    PRINT 'Index IX_WeeklyPlanTasks_WeeklyPlanId_Status_PlannedDate created.';
END
ELSE
BEGIN
    PRINT 'Index IX_WeeklyPlanTasks_WeeklyPlanId_Status_PlannedDate already exists. Skipping...';
END
GO

-- ============================================
-- Verification
-- ============================================

PRINT '';
PRINT '=== Verification ===';
PRINT '';

-- Check computed columns
DECLARE @ColCount INT;
SELECT @ColCount = COUNT(*) 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Clients' 
AND COLUMN_NAME IN ('NameLower', 'EmailLower', 'SpecializationLower');

IF @ColCount = 3
    PRINT '✓ All 3 computed columns exist';
ELSE
    PRINT '⚠ Only ' + CAST(@ColCount AS VARCHAR) + ' computed columns found (expected 3)';

-- Check indexes
DECLARE @IdxCount INT;
SELECT @IdxCount = COUNT(*) 
FROM sys.indexes 
WHERE name IN (
    'IX_TaskProgresses_EmployeeId_ProgressDate_VisitResult',
    'IX_TaskProgresses_ClientId_ProgressDate',
    'IX_SalesOffers_AssignedTo_CreatedAt_Status',
    'IX_SalesDeals_SalesmanId_CreatedAt_Status',
    'IX_Clients_NameLower_SpecializationLower',
    'IX_Clients_NextContactDate_Status_AssignedTo',
    'IX_WeeklyPlanTasks_WeeklyPlanId_Status_PlannedDate'
);

IF @IdxCount = 7
    PRINT '✓ All 7 performance indexes exist';
ELSE
    PRINT '⚠ Only ' + CAST(@IdxCount AS VARCHAR) + ' indexes found (expected 7)';

PRINT '';
PRINT '=== Script Complete ===';
PRINT 'Performance optimization indexes have been applied!';
GO

