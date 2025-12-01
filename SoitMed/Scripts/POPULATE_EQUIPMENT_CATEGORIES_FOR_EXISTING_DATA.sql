-- Script: Populate Equipment Categories for Existing Data
-- Date: 2025-01-XX
-- Description: Populates InterestedEquipmentCategories for existing Clients and EquipmentCategories for existing WeeklyPlanTasks
--              based on their related offers and equipment

-- Set required options
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- ============================================================================
-- PART 1: Populate InterestedEquipmentCategories for Clients
-- ============================================================================
-- Extract unique equipment categories from clients' offers (via OfferEquipment)
-- and store them as JSON array in InterestedEquipmentCategories

PRINT 'Starting to populate InterestedEquipmentCategories for existing Clients...';
GO

-- Update Clients with categories from their offers
-- Using FOR JSON PATH to properly format JSON array
DECLARE @UpdatedClients INT = 0;

WITH ClientCategoryList AS (
    SELECT 
        c.Id AS ClientId,
        (
            SELECT DISTINCT cat.Category
            FROM SalesOffers so
            INNER JOIN OfferEquipment cat ON so.Id = cat.OfferId
            WHERE so.ClientId = c.Id
                AND cat.Category IS NOT NULL
                AND cat.Category != ''
            FOR JSON PATH
        ) AS CategoriesJson
    FROM Clients c
    WHERE (c.InterestedEquipmentCategories IS NULL OR c.InterestedEquipmentCategories = '')
)
UPDATE c
SET InterestedEquipmentCategories = ccl.CategoriesJson
FROM Clients c
INNER JOIN ClientCategoryList ccl ON c.Id = ccl.ClientId
WHERE ccl.CategoriesJson IS NOT NULL;

SET @UpdatedClients = @@ROWCOUNT;
PRINT 'Updated ' + CAST(@UpdatedClients AS VARCHAR(10)) + ' clients with InterestedEquipmentCategories';
GO

-- ============================================================================
-- PART 2: Populate EquipmentCategories for WeeklyPlanTasks
-- ============================================================================
-- Extract categories from:
-- 1. Related Client's InterestedEquipmentCategories (if ClientId exists)
-- 2. TaskProgress -> OfferRequest -> SalesOffer -> OfferEquipment (if available)

PRINT 'Starting to populate EquipmentCategories for existing WeeklyPlanTasks...';
GO

-- Update WeeklyPlanTasks with categories from their related clients
UPDATE wpt
SET EquipmentCategories = c.InterestedEquipmentCategories
FROM WeeklyPlanTasks wpt
INNER JOIN Clients c ON wpt.ClientId = c.Id
WHERE c.InterestedEquipmentCategories IS NOT NULL
    AND c.InterestedEquipmentCategories != ''
    AND (wpt.EquipmentCategories IS NULL OR wpt.EquipmentCategories = '');

DECLARE @UpdatedTasksFromClients INT = @@ROWCOUNT;
PRINT 'Updated ' + CAST(@UpdatedTasksFromClients AS VARCHAR(10)) + ' tasks with categories from related clients';
GO

-- Update WeeklyPlanTasks with categories from TaskProgress -> OfferRequest -> SalesOffer -> OfferEquipment
DECLARE @UpdatedTasksFromProgress INT = 0;

WITH TaskCategoryList AS (
    SELECT DISTINCT
        wpt.Id AS TaskId,
        (
            SELECT DISTINCT cat.Category
            FROM TaskProgresses tp
            INNER JOIN OfferRequests orq ON tp.OfferRequestId = orq.Id
            INNER JOIN SalesOffers so ON orq.CreatedOfferId = so.Id
            INNER JOIN OfferEquipment cat ON so.Id = cat.OfferId
            WHERE tp.TaskId = wpt.Id
                AND cat.Category IS NOT NULL
                AND cat.Category != ''
            FOR JSON PATH
        ) AS CategoriesJson
    FROM WeeklyPlanTasks wpt
    WHERE (wpt.EquipmentCategories IS NULL OR wpt.EquipmentCategories = '')
        AND EXISTS (
            SELECT 1 
            FROM TaskProgresses tp
            INNER JOIN OfferRequests orq ON tp.OfferRequestId = orq.Id
            INNER JOIN SalesOffers so ON orq.CreatedOfferId = so.Id
            INNER JOIN OfferEquipment cat ON so.Id = cat.OfferId
            WHERE tp.TaskId = wpt.Id
                AND cat.Category IS NOT NULL
                AND cat.Category != ''
        )
)
UPDATE wpt
SET EquipmentCategories = tcl.CategoriesJson
FROM WeeklyPlanTasks wpt
INNER JOIN TaskCategoryList tcl ON wpt.Id = tcl.TaskId
WHERE tcl.CategoriesJson IS NOT NULL;

SET @UpdatedTasksFromProgress = @@ROWCOUNT;
PRINT 'Updated ' + CAST(@UpdatedTasksFromProgress AS VARCHAR(10)) + ' tasks with categories from task progress/offers';
GO

-- ============================================================================
-- PART 3: Summary and Verification
-- ============================================================================

PRINT '';
PRINT '=== POPULATION SUMMARY ===';
PRINT '';

-- Count clients with categories
SELECT 
    COUNT(*) AS TotalClients,
    SUM(CASE WHEN InterestedEquipmentCategories IS NOT NULL AND InterestedEquipmentCategories != '' THEN 1 ELSE 0 END) AS ClientsWithCategories,
    SUM(CASE WHEN InterestedEquipmentCategories IS NULL OR InterestedEquipmentCategories = '' THEN 1 ELSE 0 END) AS ClientsWithoutCategories
FROM Clients;

-- Count tasks with categories
SELECT 
    COUNT(*) AS TotalTasks,
    SUM(CASE WHEN EquipmentCategories IS NOT NULL AND EquipmentCategories != '' THEN 1 ELSE 0 END) AS TasksWithCategories,
    SUM(CASE WHEN EquipmentCategories IS NULL OR EquipmentCategories = '' THEN 1 ELSE 0 END) AS TasksWithoutCategories
FROM WeeklyPlanTasks;

-- Sample data verification
PRINT '';
PRINT '=== SAMPLE DATA VERIFICATION ===';
PRINT '';

-- Show sample clients with categories
SELECT TOP 5
    Id,
    Name,
    InterestedEquipmentCategories
FROM Clients
WHERE InterestedEquipmentCategories IS NOT NULL
    AND InterestedEquipmentCategories != ''
ORDER BY UpdatedAt DESC;

-- Show sample tasks with categories
SELECT TOP 5
    Id,
    Title,
    ClientId,
    EquipmentCategories
FROM WeeklyPlanTasks
WHERE EquipmentCategories IS NOT NULL
    AND EquipmentCategories != ''
ORDER BY UpdatedAt DESC;

PRINT '';
PRINT 'Data population completed successfully!';
PRINT 'Note: Some clients/tasks may not have categories if they have no related offers or equipment.';
PRINT 'These can be populated manually when creating/updating records.';
GO

