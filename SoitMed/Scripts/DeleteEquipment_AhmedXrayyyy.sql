-- ============================================
-- SQL Script to Delete Equipment: "Ahmed X-rayyyy"
-- ============================================
-- This script safely removes the equipment record and all related data
-- 
-- WARNING: This will permanently delete:
-- - The equipment record
-- - All related repair requests (RepairRequests)
-- ============================================

BEGIN TRANSACTION;

DECLARE @EquipmentId INT;
DECLARE @EquipmentName NVARCHAR(200) = 'Ahmed X-rayyyy';

-- Step 1: Find the equipment by name (case-insensitive)
SELECT @EquipmentId = Id
FROM [dbo].[Equipment]
WHERE LOWER(LTRIM(RTRIM(Name))) = LOWER(LTRIM(RTRIM(@EquipmentName)));

-- Check if equipment exists
IF @EquipmentId IS NULL
BEGIN
    PRINT 'ERROR: Equipment "' + @EquipmentName + '" not found in database.';
    PRINT 'Please verify the exact equipment name.';
    ROLLBACK TRANSACTION;
    RETURN;
END

PRINT 'Found equipment: ID = ' + CAST(@EquipmentId AS NVARCHAR(20)) + ', Name = "' + @EquipmentName + '"';
PRINT '';

-- Step 2: Show related records that will be deleted
PRINT '=== Related Records to be Deleted ===';

DECLARE @RepairRequestCount INT;

SELECT @RepairRequestCount = COUNT(*) FROM [dbo].[RepairRequests] WHERE EquipmentId = @EquipmentId;

PRINT 'RepairRequests: ' + CAST(@RepairRequestCount AS NVARCHAR(10));
PRINT '';

-- Step 3: Delete related records (in order to avoid foreign key constraints)

-- Delete from RepairRequests first (references EquipmentId)
IF @RepairRequestCount > 0
BEGIN
    PRINT 'Deleting ' + CAST(@RepairRequestCount AS NVARCHAR(10)) + ' RepairRequest record(s)...';
    DELETE FROM [dbo].[RepairRequests] WHERE EquipmentId = @EquipmentId;
    PRINT 'RepairRequests deleted successfully.';
END

-- Step 4: Delete the equipment record
PRINT '';
PRINT 'Deleting equipment record...';
DELETE FROM [dbo].[Equipment] WHERE Id = @EquipmentId;

IF @@ROWCOUNT > 0
BEGIN
    PRINT 'Equipment "' + @EquipmentName + '" (ID: ' + CAST(@EquipmentId AS NVARCHAR(20)) + ') deleted successfully.';
    PRINT '';
    PRINT '=== Summary ===';
    PRINT 'Total records deleted:';
    PRINT '  - 1 Equipment';
    PRINT '  - ' + CAST(@RepairRequestCount AS NVARCHAR(10)) + ' RepairRequest(s)';
    PRINT '';
    PRINT 'Transaction completed successfully.';
    COMMIT TRANSACTION;
END
ELSE
BEGIN
    PRINT 'ERROR: Failed to delete equipment record.';
    ROLLBACK TRANSACTION;
END

-- ============================================
-- End of Script
-- ============================================

