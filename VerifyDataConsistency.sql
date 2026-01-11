-- Verify Data Consistency Between TBS (Legacy) and itiwebapi44 (New) Databases
-- Customer → Equipment → Visits Workflow Verification

-- =====================================================
-- 1. CUSTOMER DATA VERIFICATION
-- =====================================================

-- Count customers in both databases
SELECT 'TBS_Customers' as DatabaseName, COUNT(*) as RecordCount FROM TBS.dbo.Stk_Customers
UNION ALL
SELECT 'itiwebapi44_Customers' as DatabaseName, COUNT(*) as RecordCount FROM itiwebapi44.dbo.Clients
UNION ALL
SELECT 'itiwebapi44_ApplicationUsers' as DatabaseName, COUNT(*) as RecordCount FROM itiwebapi44.dbo.AspNetUsers;

-- Check customer data overlap (matching by phone or email)
SELECT 
    'Customer_Match_Check' as CheckType,
    COUNT(*) as TBS_Customers,
    (SELECT COUNT(*) FROM itiwebapi44.dbo.Clients) as New_Clients,
    (SELECT COUNT(*) FROM TBS.dbo.Stk_Customers tbs 
     INNER JOIN itiwebapi44.dbo.Clients new ON tbs.Cus_Mobile = new.Phone OR tbs.Cus_Email = new.Email) as Matched_Customers
FROM TBS.dbo.Stk_Customers;

-- =====================================================
-- 2. EQUIPMENT DATA VERIFICATION  
-- =====================================================

-- Count equipment/items in both databases
SELECT 'TBS_Items' as DatabaseName, COUNT(*) as RecordCount FROM TBS.dbo.Stk_Items
UNION ALL
SELECT 'TBS_OrderOutItems' as DatabaseName, COUNT(*) as RecordCount FROM TBS.dbo.Stk_Order_Out_Items
UNION ALL  
SELECT 'itiwebapi44_Equipment' as DatabaseName, COUNT(*) as RecordCount FROM itiwebapi44.dbo.Equipment;

-- Check equipment by serial numbers
SELECT 
    'Equipment_By_Serial' as CheckType,
    COUNT(DISTINCT tbs.OOI_ID) as TBS_Equipment_Count,
    COUNT(DISTINCT e.Id) as New_Equipment_Count,
    COUNT(DISTINCT CASE WHEN tbs.SerialNum IS NOT NULL AND tbs.SerialNum != '' THEN tbs.OOI_ID END) as TBS_WithSerial,
    COUNT(DISTINCT CASE WHEN e.SerialNumber IS NOT NULL AND e.SerialNumber != '' THEN e.Id END) as New_WithSerial
FROM TBS.dbo.Stk_Order_Out_Items tbs
CROSS JOIN itiwebapi44.dbo.Equipment e;

-- Sample equipment data comparison (top 10)
SELECT TOP 10
    'TBS_Equipment_Sample' as Source,
    tbs.OOI_ID as EquipmentId,
    tbs.ItemId as ModelId,
    tbs.SerialNum as SerialNumber,
    item.Item_Name_Ar as ModelName,
    tbs.DevicePlace as Location
FROM TBS.dbo.Stk_Order_Out_Items tbs
LEFT JOIN TBS.dbo.Stk_Items item ON tbs.item_ID = item.Item_ID
WHERE tbs.SerialNum IS NOT NULL AND tbs.SerialNum != ''
ORDER BY tbs.OOI_ID;

-- =====================================================
-- 3. VISIT DATA VERIFICATION
-- =====================================================

-- Count visits in both databases
SELECT 'TBS_Visits' as DatabaseName, COUNT(*) as RecordCount FROM TBS.dbo.MNT_Visiting
UNION ALL
SELECT 'TBS_VisitingReports' as DatabaseName, COUNT(*) as RecordCount FROM TBS.dbo.MNT_VisitingReport
UNION ALL
SELECT 'itiwebapi44_MaintenanceVisits' as DatabaseName, COUNT(*) as RecordCount FROM itiwebapi44.dbo.MaintenanceVisits
UNION ALL
SELECT 'itiwebapi44_MaintenanceRequests' as DatabaseName, COUNT(*) as RecordCount FROM itiwebapi44.dbo.MaintenanceRequests;

-- Visit status comparison
SELECT 
    'TBS_Visit_Status_Distribution' as CheckType,
    CASE 
        WHEN v.IS_Cancelled = 1 THEN 'Cancelled'
        WHEN vr.RepVisit_Status_ID IS NOT NULL THEN 
            CASE vr.RepVisit_Status_ID
                WHEN 1 THEN 'Completed'
                WHEN 2 THEN 'Pending' 
                WHEN 3 THEN 'InProgress'
                ELSE 'Other'
            END
        ELSE 'Unknown'
    END as StatusCategory,
    COUNT(*) as Count
FROM TBS.dbo.MNT_Visiting v
LEFT JOIN TBS.dbo.MNT_VisitingReport vr ON v.VisitingId = vr.VisitingId
GROUP BY 
    CASE 
        WHEN v.IS_Cancelled = 1 THEN 'Cancelled'
        WHEN vr.RepVisit_Status_ID IS NOT NULL THEN 
            CASE vr.RepVisit_Status_ID
                WHEN 1 THEN 'Completed'
                WHEN 2 THEN 'Pending' 
                WHEN 3 THEN 'InProgress'
                ELSE 'Other'
            END
        ELSE 'Unknown'
    END
UNION ALL
SELECT 
    'New_Visit_Status_Distribution' as CheckType,
    mv.Status as StatusCategory,
    COUNT(*) as Count
FROM itiwebapi44.dbo.MaintenanceVisits mv
GROUP BY mv.Status;

-- =====================================================
-- 4. WORKFLOW VERIFICATION TEST
-- =====================================================

-- Test the complete workflow: Customer → Equipment → Visits
-- This query shows if the relationships exist in TBS database
SELECT TOP 10
    'TBS_Workflow_Test' as Source,
    c.Cus_ID as CustomerId,
    c.Cus_Name as CustomerName,
    ooi.OOI_ID as EquipmentId,
    ooi.SerialNum as SerialNumber,
    item.Item_Name_Ar as EquipmentModel,
    v.VisitingId as VisitId,
    v.VisitingDate as VisitDate,
    vr.ReportDate as ReportDate,
    CASE v.IS_Cancelled WHEN 1 THEN 'Cancelled' ELSE 'Active' END as VisitStatus
FROM TBS.dbo.Stk_Customers c
INNER JOIN TBS.dbo.MNT_Visiting v ON c.Cus_ID = v.Cus_ID
LEFT JOIN TBS.dbo.MNT_VisitingReport vr ON v.VisitingId = vr.VisitingId
LEFT JOIN TBS.dbo.Stk_Order_Out_Items ooi ON vr.OOI_ID = ooi.OOI_ID
LEFT JOIN TBS.dbo.Stk_Items item ON ooi.item_ID = item.Item_ID
WHERE c.Cus_ID IS NOT NULL
ORDER BY c.Cus_ID, v.VisitingDate DESC;

-- Test the same workflow in new database
SELECT TOP 10
    'New_Workflow_Test' as Source,
    c.Id as CustomerId,
    c.Name as CustomerName,
    e.Id as EquipmentId,
    e.SerialNumber as SerialNumber,
    e.Model as EquipmentModel,
    mv.Id as VisitId,
    mv.VisitDate as VisitDate,
    mv.CompletedAt as ReportDate,
    mv.Status as VisitStatus
FROM itiwebapi44.dbo.Clients c
INNER JOIN itiwebapi44.dbo.MaintenanceRequests mr ON c.Id = mr.CustomerId
INNER JOIN itiwebapi44.dbo.MaintenanceVisits mv ON mr.Id = mv.MaintenanceRequestId
LEFT JOIN itiwebapi44.dbo.Equipment e ON mv.DeviceId = e.Id
WHERE c.Id IS NOT NULL
ORDER BY c.Id, mv.VisitDate DESC;

-- =====================================================
-- 5. DATA MISMATCHES IDENTIFICATION
-- =====================================================

-- Find customers in TBS that might not exist in new system
SELECT TOP 20
    'Missing_Customers_New_DB' as IssueType,
    c.Cus_ID as LegacyCustomerId,
    c.Cus_Name as CustomerName,
    c.Cus_Mobile as Phone,
    c.Cus_Email as Email
FROM TBS.dbo.Stk_Customers c
LEFT JOIN itiwebapi44.dbo.Clients new ON c.Cus_Mobile = new.Phone OR c.Cus_Email = new.Email
WHERE new.Id IS NULL
AND (c.Cus_Mobile IS NOT NULL AND c.Cus_Mobile != '' OR c.Cus_Email IS NOT NULL AND c.Cus_Email != '');

-- Find equipment with serial numbers in TBS that might not exist in new system  
SELECT TOP 20
    'Missing_Equipment_New_DB' as IssueType,
    ooi.OOI_ID as LegacyEquipmentId,
    ooi.SerialNum as SerialNumber,
    item.Item_Name_Ar as Model,
    c.Cus_Name as CustomerName
FROM TBS.dbo.Stk_Order_Out_Items ooi
LEFT JOIN TBS.dbo.Stk_Items item ON ooi.item_ID = item.Item_ID
LEFT JOIN TBS.dbo.MNT_VisitingReport vr ON ooi.OOI_ID = vr.OOI_ID
LEFT JOIN TBS.dbo.MNT_Visiting v ON vr.VisitingId = v.VisitingId
LEFT JOIN TBS.dbo.Stk_Customers c ON v.Cus_ID = c.Cus_ID
LEFT JOIN itiwebapi44.dbo.Equipment new ON ooi.SerialNum = new.SerialNumber
WHERE new.Id IS NULL 
AND ooi.SerialNum IS NOT NULL AND ooi.SerialNum != ''
AND vr.OOI_ID IS NOT NULL; -- Only equipment that has visits

PRINT 'Data Consistency Verification Complete';
PRINT 'Compare the results above to identify:';
PRINT '1. Customer data gaps between systems';
PRINT '2. Equipment/serial number mismatches';  
PRINT '3. Visit status mapping differences';
PRINT '4. Workflow relationship integrity';
