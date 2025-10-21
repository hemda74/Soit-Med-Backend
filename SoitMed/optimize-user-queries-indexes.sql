-- =====================================================
-- OPTIMIZE USER QUERIES - DATABASE INDEXES
-- =====================================================
-- Run these SQL commands to add indexes for maximum performance
-- These indexes will make the user queries much faster

-- =====================================================
-- 1. ASPNETUSERS TABLE INDEXES
-- =====================================================

-- Index for DepartmentId filtering (very common filter)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUsers_DepartmentId' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUsers_DepartmentId] 
    ON [AspNetUsers] ([DepartmentId])
    INCLUDE ([Id], [Email], [UserName], [IsActive], [CreatedAt])
END
GO

-- Index for IsActive filtering (very common filter)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUsers_IsActive' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUsers_IsActive] 
    ON [AspNetUsers] ([IsActive])
    INCLUDE ([Id], [Email], [UserName], [DepartmentId], [CreatedAt])
END
GO

-- Index for CreatedAt filtering and sorting (date range queries)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUsers_CreatedAt' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUsers_CreatedAt] 
    ON [AspNetUsers] ([CreatedAt] DESC)
    INCLUDE ([Id], [Email], [UserName], [IsActive], [DepartmentId])
END
GO

-- Index for search functionality (Email, UserName only - FirstName/LastName are MAX length)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUsers_Search' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUsers_Search] 
    ON [AspNetUsers] ([Email], [UserName])
    INCLUDE ([Id], [IsActive], [DepartmentId], [CreatedAt])
END
GO

-- Composite index for common filter combinations
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUsers_Active_Department' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUsers_Active_Department] 
    ON [AspNetUsers] ([IsActive], [DepartmentId], [CreatedAt] DESC)
    INCLUDE ([Id], [Email], [UserName])
END
GO

-- =====================================================
-- 2. ASPNETUSERROLES TABLE INDEXES
-- =====================================================

-- Index for UserId (already exists but let's make sure it's optimized)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUserRoles_UserId' AND object_id = OBJECT_ID('AspNetUserRoles'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_UserId] 
    ON [AspNetUserRoles] ([UserId])
    INCLUDE ([RoleId])
END
GO

-- Index for RoleId (for reverse lookups)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetUserRoles_RoleId' AND object_id = OBJECT_ID('AspNetUserRoles'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId] 
    ON [AspNetUserRoles] ([RoleId])
    INCLUDE ([UserId])
END
GO

-- =====================================================
-- 3. ASPNETROLES TABLE INDEXES
-- =====================================================

-- Index for Name lookup (role filtering)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AspNetRoles_Name' AND object_id = OBJECT_ID('AspNetRoles'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_AspNetRoles_Name] 
    ON [AspNetRoles] ([Name])
    INCLUDE ([Id])
END
GO

-- =====================================================
-- 4. USERIMAGES TABLE INDEXES
-- =====================================================

-- Index for UserId + IsActive + IsProfileImage (profile image queries)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserImages_User_Profile' AND object_id = OBJECT_ID('UserImages'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_UserImages_User_Profile] 
    ON [UserImages] ([UserId], [IsActive], [IsProfileImage], [UploadedAt] DESC)
    INCLUDE ([Id], [FileName], [FilePath], [ContentType], [FileSize], [AltText])
END
GO

-- Index for UserId only (general image queries)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserImages_UserId' AND object_id = OBJECT_ID('UserImages'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_UserImages_UserId] 
    ON [UserImages] ([UserId])
    INCLUDE ([Id], [FileName], [FilePath], [IsActive], [IsProfileImage], [UploadedAt])
END
GO

-- =====================================================
-- 5. DEPARTMENTS TABLE INDEXES
-- =====================================================

-- Index for Id (foreign key lookups)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Departments_Id' AND object_id = OBJECT_ID('Departments'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Departments_Id] 
    ON [Departments] ([Id])
    INCLUDE ([Name], [Description])
END
GO

-- =====================================================
-- 6. PERFORMANCE MONITORING QUERIES
-- =====================================================

-- Query to check index usage (run this after creating indexes)
/*
SELECT 
    i.name AS IndexName,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates,
    s.last_user_seek,
    s.last_user_scan,
    s.last_user_lookup
FROM sys.indexes i
LEFT JOIN sys.dm_db_index_usage_stats s ON i.object_id = s.object_id AND i.index_id = s.index_id
WHERE i.object_id = OBJECT_ID('AspNetUsers')
ORDER BY s.user_seeks + s.user_scans + s.user_lookups DESC
*/

-- =====================================================
-- 7. STATISTICS UPDATE
-- =====================================================

-- Update statistics for better query optimization
UPDATE STATISTICS [AspNetUsers]
UPDATE STATISTICS [AspNetUserRoles]
UPDATE STATISTICS [AspNetRoles]
UPDATE STATISTICS [UserImages]
UPDATE STATISTICS [Departments]

PRINT 'All indexes created successfully!'
PRINT 'Performance should be significantly improved.'
PRINT 'Run the performance monitoring query to check index usage.'
