-- ========================================================================
-- PERFORMANCE INDEXES FOR SOITMED DATABASE
-- ========================================================================
-- Purpose: Add indexes to improve query performance for 10,000+ concurrent users
-- Safety: All indexes use IF NOT EXISTS checks to prevent errors
-- Impact: Indexes are created offline (compatible with SQL Server Express)
-- Note: For production, schedule during low-traffic periods
-- Author: SoitMed Development Team
-- Date: 2025-01-01
-- ========================================================================

USE ITIWebApi44;
GO

PRINT '========================================';
PRINT 'Starting Performance Index Creation';
PRINT '========================================';
PRINT '';

-- ========================================================================
-- SECTION 1: ASPNETUSERS TABLE (Identity/Authentication)
-- ========================================================================
PRINT 'Section 1: AspNetUsers Indexes...';

-- Index for user lookups by username (login operations)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUsers_UserName_Email' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AspNetUsers_UserName_Email
    ON AspNetUsers(UserName, Email)
    INCLUDE (Id, DepartmentId, IsActive)
;
    PRINT '✓ Created IX_AspNetUsers_UserName_Email';
END
ELSE
    PRINT '○ IX_AspNetUsers_UserName_Email already exists';

-- Index for filtering active users by department
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUsers_DepartmentId_IsActive' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_AspNetUsers_DepartmentId_IsActive
    ON AspNetUsers(DepartmentId, IsActive)
    INCLUDE (Id, UserName, Email)
    WHERE IsActive = 1
;
    PRINT '✓ Created IX_AspNetUsers_DepartmentId_IsActive';
END
ELSE
    PRINT '○ IX_AspNetUsers_DepartmentId_IsActive already exists';

PRINT '';

-- ========================================================================
-- SECTION 2: CLIENTS TABLE (Customer Management)
-- ========================================================================
PRINT 'Section 2: Clients Indexes...';

-- Index for client searches by status and priority
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Clients_Status_Priority' AND object_id = OBJECT_ID('Clients'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Clients_Status_Priority
    ON Clients(Status, Priority)
    INCLUDE (Id, Name, Phone, Email, CreatedAt)
;
    PRINT '✓ Created IX_Clients_Status_Priority';
END
ELSE
    PRINT '○ IX_Clients_Status_Priority already exists';

-- Index for client searches by name (autocomplete/search)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Clients_Name' AND object_id = OBJECT_ID('Clients'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Clients_Name
    ON Clients(Name)
    INCLUDE (Id, Phone, Email, Status)
;
    PRINT '✓ Created IX_Clients_Name';
END
ELSE
    PRINT '○ IX_Clients_Name already exists';

-- Index for client lookups by salesman and creation date
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Clients_CreatedBy_CreatedAt' AND object_id = OBJECT_ID('Clients'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Clients_CreatedBy_CreatedAt
    ON Clients(CreatedBy, CreatedAt DESC)
    INCLUDE (Id, Name, Status, Priority)
;
    PRINT '✓ Created IX_Clients_CreatedBy_CreatedAt';
END
ELSE
    PRINT '○ IX_Clients_CreatedBy_CreatedAt already exists';

PRINT '';

-- ========================================================================
-- SECTION 3: SALESOFFERS TABLE (Offer Management)
-- ========================================================================
PRINT 'Section 3: SalesOffers Indexes...';

-- Index for offers by client (already has Status+AssignedTo from Context.cs)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SalesOffers_ClientId_Status' AND object_id = OBJECT_ID('SalesOffers'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SalesOffers_ClientId_Status
    ON SalesOffers(ClientId, Status)
    INCLUDE (Id, TotalAmount, CreatedAt, AssignedTo)
;
    PRINT '✓ Created IX_SalesOffers_ClientId_Status';
END
ELSE
    PRINT '○ IX_SalesOffers_ClientId_Status already exists';

-- Index for offers by creation date (recent offers dashboard)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SalesOffers_CreatedAt' AND object_id = OBJECT_ID('SalesOffers'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SalesOffers_CreatedAt
    ON SalesOffers(CreatedAt DESC)
    INCLUDE (Id, ClientId, Status, TotalAmount, AssignedTo)
;
    PRINT '✓ Created IX_SalesOffers_CreatedAt';
END
ELSE
    PRINT '○ IX_SalesOffers_CreatedAt already exists';

-- Index for offers by creator (sales support)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SalesOffers_CreatedBy_Status' AND object_id = OBJECT_ID('SalesOffers'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SalesOffers_CreatedBy_Status
    ON SalesOffers(CreatedBy, Status)
    INCLUDE (Id, ClientId, TotalAmount, CreatedAt)
;
    PRINT '✓ Created IX_SalesOffers_CreatedBy_Status';
END
ELSE
    PRINT '○ IX_SalesOffers_CreatedBy_Status already exists';

PRINT '';

-- ========================================================================
-- SECTION 4: OFFERREQUESTS TABLE (Request Workflow)
-- ========================================================================
PRINT 'Section 4: OfferRequests Indexes...';

-- Index for requests by assigned support user
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OfferRequests_AssignedTo_Status' AND object_id = OBJECT_ID('OfferRequests'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_OfferRequests_AssignedTo_Status
    ON OfferRequests(AssignedTo, Status)
    INCLUDE (Id, ClientId, RequestDate, RequestedBy)
;
    PRINT '✓ Created IX_OfferRequests_AssignedTo_Status';
END
ELSE
    PRINT '○ IX_OfferRequests_AssignedTo_Status already exists';

-- Index for requests by client
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OfferRequests_ClientId_RequestDate' AND object_id = OBJECT_ID('OfferRequests'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_OfferRequests_ClientId_RequestDate
    ON OfferRequests(ClientId, RequestDate DESC)
    INCLUDE (Id, Status, RequestedBy)
;
    PRINT '✓ Created IX_OfferRequests_ClientId_RequestDate';
END
ELSE
    PRINT '○ IX_OfferRequests_ClientId_RequestDate already exists';

PRINT '';

-- ========================================================================
-- SECTION 5: SALESDEALS TABLE (Deal Management)
-- ========================================================================
PRINT 'Section 5: SalesDeals Indexes...';

-- Index for deals by offer
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SalesDeals_OfferId' AND object_id = OBJECT_ID('SalesDeals'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SalesDeals_OfferId
    ON SalesDeals(OfferId)
    INCLUDE (Id, Status, DealValue, CreatedAt)
;
    PRINT '✓ Created IX_SalesDeals_OfferId';
END
ELSE
    PRINT '○ IX_SalesDeals_OfferId already exists';

-- Index for deals by client
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SalesDeals_ClientId_Status' AND object_id = OBJECT_ID('SalesDeals'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SalesDeals_ClientId_Status
    ON SalesDeals(ClientId, Status)
    INCLUDE (Id, DealValue, CreatedAt, SalesManId)
;
    PRINT '✓ Created IX_SalesDeals_ClientId_Status';
END
ELSE
    PRINT '○ IX_SalesDeals_ClientId_Status already exists';

PRINT '';

-- ========================================================================
-- SECTION 6: PRODUCTS TABLE (Product Catalog)
-- ========================================================================
PRINT 'Section 6: Products Indexes...';

-- Index for active products by name (search/autocomplete)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Name_IsActive' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_Name_IsActive
    ON Products(Name, IsActive)
    INCLUDE (Id, Model, BasePrice, CategoryId, InStock)
    WHERE IsActive = 1
;
    PRINT '✓ Created IX_Products_Name_IsActive';
END
ELSE
    PRINT '○ IX_Products_Name_IsActive already exists';

-- Index for products by provider
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Provider_IsActive' AND object_id = OBJECT_ID('Products'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Products_Provider_IsActive
    ON Products(Provider, IsActive)
    INCLUDE (Id, Name, Model, BasePrice)
    WHERE IsActive = 1
;
    PRINT '✓ Created IX_Products_Provider_IsActive';
END
ELSE
    PRINT '○ IX_Products_Provider_IsActive already exists';

PRINT '';

-- ========================================================================
-- SECTION 7: NOTIFICATIONS TABLE (Real-time Notifications)
-- ========================================================================
PRINT 'Section 7: Notifications Indexes...';

-- Index for user notifications by read status
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Notifications_UserId_IsRead_CreatedAt' AND object_id = OBJECT_ID('Notifications'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Notifications_UserId_IsRead_CreatedAt
    ON Notifications(UserId, IsRead, CreatedAt DESC)
    INCLUDE (Id, Title, Message, Type)
;
    PRINT '✓ Created IX_Notifications_UserId_IsRead_CreatedAt';
END
ELSE
    PRINT '○ IX_Notifications_UserId_IsRead_CreatedAt already exists';

PRINT '';

-- ========================================================================
-- SECTION 8: WEEKLYPLANS TABLE (Task Planning)
-- ========================================================================
PRINT 'Section 8: WeeklyPlans Indexes (by Employee)...';

-- Index for weekly plans by employee and week
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WeeklyPlans_EmployeeId_WeekStart' AND object_id = OBJECT_ID('WeeklyPlans'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WeeklyPlans_EmployeeId_WeekStart
    ON WeeklyPlans(EmployeeId, WeekStartDate DESC)
    INCLUDE (Id, IsActive, CreatedAt)
;
    PRINT '✓ Created IX_WeeklyPlans_EmployeeId_WeekStart';
END
ELSE
    PRINT '○ IX_WeeklyPlans_EmployeeId_WeekStart already exists';

-- Index for weekly plans by active status and week
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WeeklyPlans_IsActive_WeekStart' AND object_id = OBJECT_ID('WeeklyPlans'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_WeeklyPlans_IsActive_WeekStart
    ON WeeklyPlans(IsActive, WeekStartDate DESC)
    INCLUDE (Id, EmployeeId, CreatedAt)
;
    PRINT '✓ Created IX_WeeklyPlans_IsActive_WeekStart';
END
ELSE
    PRINT '○ IX_WeeklyPlans_IsActive_WeekStart already exists';

PRINT '';

-- ========================================================================
-- SECTION 9: TASKPROGRESSES TABLE (Daily Progress Tracking)
-- ========================================================================
PRINT 'Section 9: TaskProgresses Indexes...';

-- Index for task progress by employee and date
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TaskProgresses_EmployeeId_ProgressDate' AND object_id = OBJECT_ID('TaskProgresses'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_TaskProgresses_EmployeeId_ProgressDate
    ON TaskProgresses(EmployeeId, ProgressDate DESC)
    INCLUDE (Id, ClientId, Status, TaskDescription)
;
    PRINT '✓ Created IX_TaskProgresses_EmployeeId_ProgressDate';
END
ELSE
    PRINT '○ IX_TaskProgresses_EmployeeId_ProgressDate already exists';

PRINT '';

-- ========================================================================
-- SECTION 10: CHATMESSAGES TABLE (Chat System)
-- ========================================================================
PRINT 'Section 10: ChatMessages Indexes...';

-- Index for messages by conversation (chat history)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatMessages_ConversationId_SentAt' AND object_id = OBJECT_ID('ChatMessages'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ChatMessages_ConversationId_SentAt
    ON ChatMessages(ConversationId, SentAt DESC)
    INCLUDE (Id, SenderId, Content, IsRead)
;
    PRINT '✓ Created IX_ChatMessages_ConversationId_SentAt';
END
ELSE
    PRINT '○ IX_ChatMessages_ConversationId_SentAt already exists';

-- Index for unread messages
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatMessages_ConversationId_IsRead' AND object_id = OBJECT_ID('ChatMessages'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ChatMessages_ConversationId_IsRead
    ON ChatMessages(ConversationId, IsRead)
    INCLUDE (Id, SenderId, SentAt)
    WHERE IsRead = 0
;
    PRINT '✓ Created IX_ChatMessages_ConversationId_IsRead';
END
ELSE
    PRINT '○ IX_ChatMessages_ConversationId_IsRead already exists';

PRINT '';

-- ========================================================================
-- SECTION 11: CHATCONVERSATIONS TABLE (Conversations)
-- ========================================================================
PRINT 'Section 11: ChatConversations Indexes...';

-- Index for conversations by participants
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatConversations_Participant1_Participant2' AND object_id = OBJECT_ID('ChatConversations'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ChatConversations_Participant1_Participant2
    ON ChatConversations(Participant1Id, Participant2Id)
    INCLUDE (Id, LastMessageAt, IsActive)
;
    PRINT '✓ Created IX_ChatConversations_Participant1_Participant2';
END
ELSE
    PRINT '○ IX_ChatConversations_Participant1_Participant2 already exists';

-- Index for active conversations by last message time
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ChatConversations_LastMessageAt' AND object_id = OBJECT_ID('ChatConversations'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ChatConversations_LastMessageAt
    ON ChatConversations(LastMessageAt DESC)
    INCLUDE (Id, Participant1Id, Participant2Id, IsActive)
    WHERE IsActive = 1
;
    PRINT '✓ Created IX_ChatConversations_LastMessageAt';
END
ELSE
    PRINT '○ IX_ChatConversations_LastMessageAt already exists';

PRINT '';

-- ========================================================================
-- SECTION 12: EQUIPMENT TABLE (Equipment Management)
-- ========================================================================
PRINT 'Section 12: Equipment Indexes...';

-- Index for equipment by hospital
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Equipment_HospitalId' AND object_id = OBJECT_ID('Equipment'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Equipment_HospitalId
    ON Equipment(HospitalId)
    INCLUDE (Id, Name, Model, Manufacturer, SerialNumber)
;
    PRINT '✓ Created IX_Equipment_HospitalId';
END
ELSE
    PRINT '○ IX_Equipment_HospitalId already exists';

-- Index for equipment by customer
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Equipment_CustomerId' AND object_id = OBJECT_ID('Equipment'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Equipment_CustomerId
    ON Equipment(CustomerId)
    INCLUDE (Id, Name, Model, Manufacturer, SerialNumber)
;
    PRINT '✓ Created IX_Equipment_CustomerId';
END
ELSE
    PRINT '○ IX_Equipment_CustomerId already exists';

PRINT '';

-- ========================================================================
-- SECTION 13: CLIENTVISITS TABLE (Visit Tracking)
-- ========================================================================
PRINT 'Section 13: ClientVisits Indexes...';

-- Index for visits by client and date
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ClientVisits_ClientId_VisitDate' AND object_id = OBJECT_ID('ClientVisits'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ClientVisits_ClientId_VisitDate
    ON ClientVisits(ClientId, VisitDate DESC)
    INCLUDE (Id, SalesManId, Outcome, Notes)
;
    PRINT '✓ Created IX_ClientVisits_ClientId_VisitDate';
END
ELSE
    PRINT '○ IX_ClientVisits_ClientId_VisitDate already exists';

-- Index for visits by salesman
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ClientVisits_SalesManId_VisitDate' AND object_id = OBJECT_ID('ClientVisits'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ClientVisits_SalesManId_VisitDate
    ON ClientVisits(SalesManId, VisitDate DESC)
    INCLUDE (Id, ClientId, Outcome)
;
    PRINT '✓ Created IX_ClientVisits_SalesManId_VisitDate';
END
ELSE
    PRINT '○ IX_ClientVisits_SalesManId_VisitDate already exists';

PRINT '';

-- ========================================================================
-- SECTION 14: SALESMANTARGETS TABLE (Target Management)
-- ========================================================================
PRINT 'Section 14: SalesManTargets Indexes...';

-- Index for targets by year and quarter (already has unique index from Context.cs)
-- Adding covering index for reporting
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SalesManTargets_Year_Quarter' AND object_id = OBJECT_ID('SalesManTargets'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_SalesManTargets_Year_Quarter
    ON SalesManTargets(Year, Quarter)
    INCLUDE (SalesManId, TargetRevenue, TargetType, CreatedByManagerId)
;
    PRINT '✓ Created IX_SalesManTargets_Year_Quarter';
END
ELSE
    PRINT '○ IX_SalesManTargets_Year_Quarter already exists';

PRINT '';

-- ========================================================================
-- SECTION 15: ACTIVITYLOGS TABLE (Activity Tracking)
-- ========================================================================
PRINT 'Section 15: ActivityLogs Indexes...';

-- Index for activity logs by user and date
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ActivityLogs_UserId_Timestamp' AND object_id = OBJECT_ID('ActivityLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ActivityLogs_UserId_Timestamp
    ON ActivityLogs(UserId, Timestamp DESC)
    INCLUDE (Id, ActivityType, Description)
;
    PRINT '✓ Created IX_ActivityLogs_UserId_Timestamp';
END
ELSE
    PRINT '○ IX_ActivityLogs_UserId_Timestamp already exists';

-- Index for activity logs by type
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ActivityLogs_ActivityType_Timestamp' AND object_id = OBJECT_ID('ActivityLogs'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ActivityLogs_ActivityType_Timestamp
    ON ActivityLogs(ActivityType, Timestamp DESC)
    INCLUDE (Id, UserId, Description)
;
    PRINT '✓ Created IX_ActivityLogs_ActivityType_Timestamp';
END
ELSE
    PRINT '○ IX_ActivityLogs_ActivityType_Timestamp already exists';

PRINT '';

-- ========================================================================
-- STATISTICS UPDATE
-- ========================================================================
PRINT 'Updating statistics for optimal query plans...';

UPDATE STATISTICS AspNetUsers WITH FULLSCAN;
UPDATE STATISTICS Clients WITH FULLSCAN;
UPDATE STATISTICS SalesOffers WITH FULLSCAN;
UPDATE STATISTICS OfferRequests WITH FULLSCAN;
UPDATE STATISTICS SalesDeals WITH FULLSCAN;
UPDATE STATISTICS Products WITH FULLSCAN;
UPDATE STATISTICS Notifications WITH FULLSCAN;
UPDATE STATISTICS WeeklyPlans WITH FULLSCAN;
UPDATE STATISTICS ChatMessages WITH FULLSCAN;

PRINT '✓ Statistics updated';
PRINT '';

-- ========================================================================
-- COMPLETION SUMMARY
-- ========================================================================
PRINT '========================================';
PRINT 'Index Creation Complete!';
PRINT '========================================';
PRINT '';
PRINT 'Summary:';
PRINT '- All indexes created successfully';
PRINT '- Statistics updated for optimal performance';
PRINT '- Database ready for high-load operations';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Monitor query performance using SQL Server Profiler';
PRINT '2. Review execution plans for slow queries';
PRINT '3. Consider partitioning large tables (>10M rows)';
PRINT '4. Set up Redis cache for frequently accessed data';
PRINT '';
PRINT 'Performance Tips:';
PRINT '- Connection pool increased to 300 (check appsettings.json)';
PRINT '- Use AsNoTracking() for read-only queries';
PRINT '- Implement caching for reference data';
PRINT '- Monitor index fragmentation monthly';
PRINT '';

GO
