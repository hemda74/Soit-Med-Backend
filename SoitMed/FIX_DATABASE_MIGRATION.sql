-- ========================================
-- FIX DATABASE MIGRATION ISSUE
-- This script fixes the PersonalMail column issue
-- ========================================

-- IMPORTANT: Run this script on your database to fix the error:
-- "Invalid column name 'PersonalMail'"

BEGIN TRANSACTION;

BEGIN TRY
    
    -- 1. Drop FK_Doctors_Hospitals_HospitalId if exists
    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Doctors_Hospitals_HospitalId')
    BEGIN
        ALTER TABLE [Doctors] DROP CONSTRAINT [FK_Doctors_Hospitals_HospitalId];
        PRINT 'Dropped FK_Doctors_Hospitals_HospitalId';
    END
    
    -- 2. Drop IX_UserImages_UserId index if exists (SKIP if not exists - this is the bug!)
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserImages_UserId' AND object_id = OBJECT_ID('UserImages'))
    BEGIN
        DROP INDEX [IX_UserImages_UserId] ON [UserImages];
        PRINT 'Dropped IX_UserImages_UserId';
    END
    ELSE
    BEGIN
        PRINT 'IX_UserImages_UserId does not exist - SKIPPED (this is normal)';
    END
    
    -- 3. Drop IX_Doctors_HospitalId if exists
    IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Doctors_HospitalId' AND object_id = OBJECT_ID('Doctors'))
    BEGIN
        DROP INDEX [IX_Doctors_HospitalId] ON [Doctors];
        PRINT 'Dropped IX_Doctors_HospitalId';
    END
    
    -- 4. Drop HospitalId column from Doctors if exists
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Doctors') AND name = 'HospitalId')
    BEGIN
        ALTER TABLE [Doctors] DROP COLUMN [HospitalId];
        PRINT 'Dropped HospitalId column from Doctors';
    END
    
    -- 5. Add PersonalMail to Engineers table if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Engineers') AND name = 'PersonalMail')
    BEGIN
        ALTER TABLE [Engineers] ADD [PersonalMail] nvarchar(200) NULL;
        PRINT 'Added PersonalMail column to Engineers';
    END
    ELSE
    BEGIN
        PRINT 'PersonalMail already exists in Engineers - SKIPPED';
    END
    
    -- 6. Add PersonalMail to AspNetUsers table if not exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'PersonalMail')
    BEGIN
        ALTER TABLE [AspNetUsers] ADD [PersonalMail] nvarchar(max) NULL;
        PRINT 'Added PersonalMail column to AspNetUsers';
    END
    ELSE
    BEGIN
        PRINT 'PersonalMail already exists in AspNetUsers - SKIPPED';
    END
    
    -- 7. Create DoctorHospitals table if not exists
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DoctorHospitals')
    BEGIN
        CREATE TABLE [DoctorHospitals] (
            [Id] int IDENTITY(1,1) NOT NULL,
            [DoctorId] int NOT NULL,
            [HospitalId] nvarchar(450) NOT NULL,
            [AssignedAt] datetime2 NOT NULL,
            [IsActive] bit NOT NULL,
            CONSTRAINT [PK_DoctorHospitals] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_DoctorHospitals_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) 
                REFERENCES [Doctors] ([DoctorId]) ON DELETE CASCADE,
            CONSTRAINT [FK_DoctorHospitals_Hospitals_HospitalId] FOREIGN KEY ([HospitalId]) 
                REFERENCES [Hospitals] ([HospitalId]) ON DELETE CASCADE
        );
        PRINT 'Created DoctorHospitals table';
    END
    ELSE
    BEGIN
        PRINT 'DoctorHospitals table already exists - SKIPPED';
    END
    
    -- 8. Create IX_UserImages_UserId_IsProfileImage index if not exists
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserImages_UserId_IsProfileImage' AND object_id = OBJECT_ID('UserImages'))
    BEGIN
        CREATE UNIQUE INDEX [IX_UserImages_UserId_IsProfileImage] 
        ON [UserImages] ([UserId], [IsProfileImage]) 
        WHERE [IsProfileImage] = 1;
        PRINT 'Created IX_UserImages_UserId_IsProfileImage index';
    END
    ELSE
    BEGIN
        PRINT 'IX_UserImages_UserId_IsProfileImage already exists - SKIPPED';
    END
    
    -- 9. Create indexes on DoctorHospitals if not exist
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'DoctorHospitals')
    BEGIN
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DoctorHospitals_DoctorId_HospitalId' AND object_id = OBJECT_ID('DoctorHospitals'))
        BEGIN
            CREATE UNIQUE INDEX [IX_DoctorHospitals_DoctorId_HospitalId] 
            ON [DoctorHospitals] ([DoctorId], [HospitalId]);
            PRINT 'Created IX_DoctorHospitals_DoctorId_HospitalId index';
        END
        
        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DoctorHospitals_HospitalId' AND object_id = OBJECT_ID('DoctorHospitals'))
        BEGIN
            CREATE INDEX [IX_DoctorHospitals_HospitalId] 
            ON [DoctorHospitals] ([HospitalId]);
            PRINT 'Created IX_DoctorHospitals_HospitalId index';
        END
    END
    
    -- 10. Mark the migration as applied (if not already marked)
    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251001093910_AddPersonalMailFields')
    BEGIN
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES ('20251001093910_AddPersonalMailFields', '8.0.10');
        PRINT 'Marked AddPersonalMailFields migration as applied';
    END
    ELSE
    BEGIN
        PRINT 'AddPersonalMailFields migration already marked as applied - SKIPPED';
    END
    
    -- 11. Now apply the Weekly Plan migration tables
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WeeklyPlans')
    BEGIN
        CREATE TABLE [WeeklyPlans] (
            [Id] int IDENTITY(1,1) NOT NULL,
            [Title] nvarchar(200) NOT NULL,
            [Description] nvarchar(1000) NULL,
            [WeekStartDate] date NOT NULL,
            [WeekEndDate] date NOT NULL,
            [EmployeeId] nvarchar(450) NOT NULL,
            [Rating] int NULL,
            [ManagerComment] nvarchar(1000) NULL,
            [ManagerReviewedAt] datetime2 NULL,
            [CreatedAt] datetime2 NOT NULL,
            [UpdatedAt] datetime2 NOT NULL,
            [IsActive] bit NOT NULL,
            CONSTRAINT [PK_WeeklyPlans] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_WeeklyPlans_AspNetUsers_EmployeeId] FOREIGN KEY ([EmployeeId]) 
                REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
        );
        
        CREATE UNIQUE INDEX [IX_WeeklyPlans_EmployeeId_WeekStartDate] 
        ON [WeeklyPlans] ([EmployeeId], [WeekStartDate]);
        
        PRINT 'Created WeeklyPlans table';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WeeklyPlanTasks')
    BEGIN
        CREATE TABLE [WeeklyPlanTasks] (
            [Id] int IDENTITY(1,1) NOT NULL,
            [WeeklyPlanId] int NOT NULL,
            [Title] nvarchar(300) NOT NULL,
            [Description] nvarchar(1000) NULL,
            [IsCompleted] bit NOT NULL,
            [DisplayOrder] int NOT NULL,
            [CreatedAt] datetime2 NOT NULL,
            [UpdatedAt] datetime2 NOT NULL,
            [IsActive] bit NOT NULL,
            CONSTRAINT [PK_WeeklyPlanTasks] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_WeeklyPlanTasks_WeeklyPlans_WeeklyPlanId] FOREIGN KEY ([WeeklyPlanId]) 
                REFERENCES [WeeklyPlans] ([Id]) ON DELETE CASCADE
        );
        
        CREATE INDEX [IX_WeeklyPlanTasks_WeeklyPlanId] 
        ON [WeeklyPlanTasks] ([WeeklyPlanId]);
        
        PRINT 'Created WeeklyPlanTasks table';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DailyProgresses')
    BEGIN
        CREATE TABLE [DailyProgresses] (
            [Id] int IDENTITY(1,1) NOT NULL,
            [WeeklyPlanId] int NOT NULL,
            [ProgressDate] date NOT NULL,
            [Notes] nvarchar(2000) NOT NULL,
            [TasksWorkedOn] nvarchar(500) NULL,
            [CreatedAt] datetime2 NOT NULL,
            [UpdatedAt] datetime2 NOT NULL,
            [IsActive] bit NOT NULL,
            CONSTRAINT [PK_DailyProgresses] PRIMARY KEY ([Id]),
            CONSTRAINT [FK_DailyProgresses_WeeklyPlans_WeeklyPlanId] FOREIGN KEY ([WeeklyPlanId]) 
                REFERENCES [WeeklyPlans] ([Id]) ON DELETE CASCADE
        );
        
        CREATE UNIQUE INDEX [IX_DailyProgresses_WeeklyPlanId_ProgressDate] 
        ON [DailyProgresses] ([WeeklyPlanId], [ProgressDate]);
        
        PRINT 'Created DailyProgresses table';
    END
    
    -- 12. Mark the Weekly Plan migration as applied (if not already marked)
    IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] LIKE '2025100414%AddWeeklyPlanSystem')
    BEGIN
        -- Get the exact migration ID
        DECLARE @WeeklyPlanMigrationId NVARCHAR(150);
        SET @WeeklyPlanMigrationId = (SELECT TOP 1 REPLACE([MigrationId], '.cs', '') 
                                       FROM (VALUES ('20251004143246_AddWeeklyPlanSystem')) AS V([MigrationId]));
        
        INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
        VALUES (@WeeklyPlanMigrationId, '8.0.10');
        PRINT 'Marked AddWeeklyPlanSystem migration as applied';
    END
    
    COMMIT TRANSACTION;
    PRINT '✅ SUCCESS! All migrations applied successfully!';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '❌ ERROR: ' + ERROR_MESSAGE();
    THROW;
END CATCH

GO

-- Verify the fix
SELECT 'PersonalMail column in AspNetUsers' AS [Check], 
       CASE WHEN EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('AspNetUsers') AND name = 'PersonalMail') 
            THEN '✅ EXISTS' 
            ELSE '❌ MISSING' 
       END AS [Status]
UNION ALL
SELECT 'PersonalMail column in Engineers' AS [Check], 
       CASE WHEN EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Engineers') AND name = 'PersonalMail') 
            THEN '✅ EXISTS' 
            ELSE '❌ MISSING' 
       END AS [Status]
UNION ALL
SELECT 'DoctorHospitals table' AS [Check], 
       CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'DoctorHospitals') 
            THEN '✅ EXISTS' 
            ELSE '❌ MISSING' 
       END AS [Status]
UNION ALL
SELECT 'WeeklyPlans table' AS [Check], 
       CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'WeeklyPlans') 
            THEN '✅ EXISTS' 
            ELSE '❌ MISSING' 
       END AS [Status]
UNION ALL
SELECT 'WeeklyPlanTasks table' AS [Check], 
       CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'WeeklyPlanTasks') 
            THEN '✅ EXISTS' 
            ELSE '❌ MISSING' 
       END AS [Status]
UNION ALL
SELECT 'DailyProgresses table' AS [Check], 
       CASE WHEN EXISTS (SELECT * FROM sys.tables WHERE name = 'DailyProgresses') 
            THEN '✅ EXISTS' 
            ELSE '❌ MISSING' 
       END AS [Status];

GO

PRINT '';
PRINT '====================================';
PRINT '✅ DATABASE FIX COMPLETE!';
PRINT '====================================';
PRINT 'You can now run your application without errors.';
PRINT 'All migrations have been applied successfully.';




