# Data Migration Strategy: TBS to ITI Web API

## Overview
Complete migration of maintenance data from legacy TBS database to new itiwebapi44 database while preserving data integrity and business logic.

## Migration Phases

### Phase 1: Data Structure Mapping
```sql
-- Legacy TBS Tables → New itiwebapi44 Tables

-- Customers
Stk_Customers → Clients
- CusId → Id
- CusName → Name  
- CusTel → Phone
- CusMobile → Phone (secondary)
- CusAddress → Address

-- Equipment/Machines
Stk_Order_Out_Items → Equipment
- OoiId → Id
- SerialNum → SerialNumber
- ItemId → ModelId (link to Items table)
- DevicePlace → Location
- ItemDateExpire → WarrantyExpiry

-- Items/Models
Stk_Items → EquipmentModels
- ItemId → Id
- ItemNameAr → ModelName
- ItemNameEn → ModelNameEn
- ItemCode → ModelCode

-- Maintenance Visits
MNT_Visiting → MaintenanceVisits
- VisitingId → Id
- CusId → ClientId
- VisitingDate → ScheduledDate
- EmpCode → EngineerId
- VisitingTypeId → VisitType
- DefectDescription → IssueDescription

-- Visit Reports
MNT_VisitingReport → MaintenanceVisitReports
- VisitingReportId → Id
- VisitingId → MaintenanceVisitId
- ReportDecription → Report
- VisitingDateEffective → CompletedDate
- IsApproved → IsApproved
- Files → AttachmentPaths

-- Maintenance Contracts
MNT_MaintenanceContract → MaintenanceContracts
- ContractId → Id
- CusId → ClientId
- ContractCode → ContractNumber
- StartDate → StartDate
- EndDate → EndDate
- VisitingNumber → PlannedVisits

-- Contract Items
MNT_MaintenanceContract_Items → ContractEquipment
- ContractItemsId → Id
- ContractId → MaintenanceContractId
- OOI_ID → EquipmentId
```

### Phase 2: Data Migration Scripts

#### 2.1 Customers Migration
```sql
-- Migrate Customers from TBS to itiwebapi44
INSERT INTO itiwebapi44.dbo.Clients (Name, Phone, Email, Address, CreatedAt)
SELECT 
    CusName,
    COALESCE(CusTel, CusMobile) as Phone,
    NULL as Email, -- Legacy doesn't have email
    CusAddress,
    GETDATE() as CreatedAt
FROM TBS.dbo.Stk_Customers
WHERE CusName IS NOT NULL AND CusName != ''
AND NOT EXISTS (
    SELECT 1 FROM itiwebapi44.dbo.Clients c 
    WHERE c.Phone = COALESCE(TBS.dbo.Stk_Customers.CusTel, TBS.dbo.Stk_Customers.CusMobile)
);
```

#### 2.2 Equipment Models Migration
```sql
-- Migrate Equipment Models
INSERT INTO itiwebapi44.dbo.EquipmentModels (ModelName, ModelNameEn, ModelCode, Category, CreatedAt)
SELECT 
    ItemNameAr,
    ItemNameEn,
    ItemCode,
    'Medical Equipment' as Category,
    GETDATE() as CreatedAt
FROM TBS.dbo.Stk_Items
WHERE ItemNameAr IS NOT NULL AND ItemNameAr != ''
AND NOT EXISTS (
    SELECT 1 FROM itiwebapi44.dbo.EquipmentModels m 
    WHERE m.ModelCode = TBS.dbo.Stk_Items.ItemCode
);
```

#### 2.3 Equipment Migration
```sql
-- Migrate Equipment/Machines
INSERT INTO itiwebapi44.dbo.Equipment (ClientId, ModelId, SerialNumber, Location, WarrantyExpiry, Status, CreatedAt)
SELECT 
    c.Id as ClientId,
    m.Id as ModelId,
    ooi.SerialNum,
    ooi.DevicePlace,
    ooi.ItemDateExpire,
    'Active' as Status,
    GETDATE() as CreatedAt
FROM TBS.dbo.Stk_Order_Out_Items ooi
INNER JOIN TBS.dbo.Stk_Items i ON ooi.ItemId = i.ItemId
INNER JOIN itiwebapi44.dbo.EquipmentModels m ON m.ModelCode = i.ItemCode
LEFT JOIN TBS.dbo.Stk_Customers cust ON cust.CusId = ooi.OoId -- Assuming relationship
LEFT JOIN itiwebapi44.dbo.Clients c ON c.Phone = COALESCE(cust.CusTel, cust.CusMobile)
WHERE ooi.SerialNum IS NOT NULL AND ooi.SerialNum != ''
AND m.Id IS NOT NULL AND c.Id IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM itiwebapi44.dbo.Equipment e 
    WHERE e.SerialNumber = ooi.SerialNum
);
```

#### 2.4 Maintenance Visits Migration
```sql
-- Migrate Maintenance Visits
INSERT INTO itiwebapi44.dbo.MaintenanceVisits (ClientId, EquipmentId, EngineerId, ScheduledDate, VisitType, IssueDescription, Status, CreatedAt)
SELECT 
    c.Id as ClientId,
    e.Id as EquipmentId,
    emp.Id as EngineerId,
    v.VisitingDate,
    CASE v.VisitingTypeId
        WHEN 1 THEN 'Preventive'
        WHEN 2 THEN 'Corrective'
        WHEN 3 THEN 'Installation'
        WHEN 4 THEN 'Inspection'
        ELSE 'Other'
    END as VisitType,
    v.DefectDescription,
    CASE 
        WHEN v.IsCancelled = 1 THEN 'Cancelled'
        WHEN EXISTS (SELECT 1 FROM TBS.dbo.MNT_VisitingReport vr WHERE vr.VisitingId = v.VisitingId) THEN 'Completed'
        ELSE 'Scheduled'
    END as Status,
    GETDATE() as CreatedAt
FROM TBS.dbo.MNT_Visiting v
LEFT JOIN TBS.dbo.Stk_Customers cust ON cust.CusId = v.CusId
LEFT JOIN itiwebapi44.dbo.Clients c ON c.Phone = COALESCE(cust.CusTel, cust.CusMobile)
LEFT JOIN TBS.dbo.MNT_VisitingReport vr ON vr.VisitingId = v.VisitingId
LEFT JOIN TBS.dbo.Stk_Order_Out_Items ooi ON ooi.OoiId = vr.OoiId
LEFT JOIN itiwebapi44.dbo.Equipment e ON e.SerialNumber = ooi.SerialNum
LEFT JOIN TBS.dbo.Emp_Mas emp ON emp.EmpCode = v.EmpCode
WHERE c.Id IS NOT NULL
AND NOT EXISTS (
    SELECT 1 FROM itiwebapi44.dbo.MaintenanceVisits mv 
    WHERE mv.ScheduledDate = v.VisitingDate 
    AND mv.ClientId = c.Id
    AND mv.EquipmentId = e.Id
);
```

### Phase 3: Data Validation & Consistency

#### 3.1 Row Count Validation
```sql
-- Validate migration counts
SELECT 
    'TBS_Customers' as TableName, COUNT(*) as TBS_Count
FROM TBS.dbo.Stk_Customers
UNION ALL
SELECT 
    'New_Clients' as TableName, COUNT(*) as New_Count  
FROM itiwebapi44.dbo.Clients
UNION ALL
SELECT 
    'TBS_Visits' as TableName, COUNT(*) as TBS_Count
FROM TBS.dbo.MNT_Visiting
UNION ALL
SELECT 
    'New_Visits' as TableName, COUNT(*) as New_Count
FROM itiwebapi44.dbo.MaintenanceVisits;
```

#### 3.2 Data Consistency Checks
```sql
-- Check for orphaned records
SELECT 'Orphaned Equipment' as Issue, COUNT(*) as Count
FROM itiwebapi44.dbo.Equipment e
LEFT JOIN itiwebapi44.dbo.Clients c ON e.ClientId = c.Id
WHERE c.Id IS NULL

UNION ALL

SELECT 'Orphaned Visits' as Issue, COUNT(*) as Count  
FROM itiwebapi44.dbo.MaintenanceVisits mv
LEFT JOIN itiwebapi44.dbo.Clients c ON mv.ClientId = c.Id
WHERE c.Id IS NULL;
```

### Phase 4: Synchronization Strategy

#### 4.1 Incremental Sync Procedure
```sql
CREATE PROCEDURE sp_SyncNewMaintenanceData
AS
BEGIN
    -- Sync new customers added since last sync
    INSERT INTO itiwebapi44.dbo.Clients (Name, Phone, Email, Address, CreatedAt)
    SELECT CusName, COALESCE(CusTel, CusMobile), NULL, CusAddress, GETDATE()
    FROM TBS.dbo.Stk_Customers
    WHERE CusId > (SELECT ISNULL(MAX(CAST(Phone AS INT)), 0) FROM itiwebapi44.dbo.Clients)
    AND NOT EXISTS (SELECT 1 FROM itiwebapi44.dbo.Clients c WHERE c.Phone = COALESCE(TBS.dbo.Stk_Customers.CusTel, TBS.dbo.Stk_Customers.CusMobile));
    
    -- Similar logic for equipment, visits, etc.
END;
```

### Phase 5: Rollback Strategy

#### 5.1 Backup Before Migration
```sql
-- Create backup tables
SELECT * INTO itiwebapi44.dbo.Clients_Backup FROM itiwebapi44.dbo.Clients;
SELECT * INTO itiwebapi44.dbo.Equipment_Backup FROM itiwebapi44.dbo.Equipment;
SELECT * INTO itiwebapi44.dbo.MaintenanceVisits_Backup FROM itiwebapi44.dbo.MaintenanceVisits;
```

#### 5.2 Rollback Procedure
```sql
CREATE PROCEDURE sp_RollbackMaintenanceMigration
AS
BEGIN
    -- Truncate current tables
    TRUNCATE TABLE itiwebapi44.dbo.MaintenanceVisits;
    TRUNCATE TABLE itiwebapi44.dbo.Equipment;
    TRUNCATE TABLE itiwebapi44.dbo.Clients;
    
    -- Restore from backup
    INSERT INTO itiwebapi44.dbo.Clients SELECT * FROM itiwebapi44.dbo.Clients_Backup;
    INSERT INTO itiwebapi44.dbo.Equipment SELECT * FROM itiwebapi44.dbo.Equipment_Backup;
    INSERT INTO itiwebapi44.dbo.MaintenanceVisits SELECT * FROM itiwebapi44.dbo.MaintenanceVisits_Backup;
END;
```

## Migration Execution Plan

### Pre-Migration Checklist
- [ ] Full backup of both databases
- [ ] Test environment setup
- [ ] Migration scripts validated
- [ ] Rollback procedures tested
- [ ] Performance impact analysis

### Migration Day
- [ ] Execute migration in batches
- [ ] Validate each batch
- [ ] Monitor system performance
- [ ] Test application functionality
- [ ] Get stakeholder sign-off

### Post-Migration
- [ ] Monitor for data inconsistencies
- [ ] Set up incremental sync jobs
- [ ] Decommission legacy connections
- [ ] Update documentation
- [ ] Train users on new system

## Risk Mitigation

### High-Risk Areas
1. **Data Loss**: Full backups + rollback procedures
2. **Performance Issues**: Batch processing + off-hours migration
3. **Data Integrity**: Validation scripts + consistency checks
4. **Business Continuity**: Phased rollout + parallel running

### Success Criteria
- 100% data migration without loss
- <5 minute downtime during cutover
- All application functions working
- Performance within 10% of baseline
- User acceptance testing passed
