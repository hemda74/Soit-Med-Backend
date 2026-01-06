-- ============================================================================
-- Verify Contract-Client Mapping - التحقق من ربط العقود بالعملاء
-- ============================================================================
-- This script helps identify if contracts are correctly linked to clients
-- Run this to check if all contracts are linked to the same client
-- ============================================================================

USE ITIWebApi44;
GO

PRINT '============================================================================';
PRINT 'Contract-Client Mapping Verification';
PRINT '============================================================================';
PRINT '';

-- 1. Check total contracts and unique ClientIds
PRINT '1. Contract-Client Distribution:';
SELECT 
    COUNT(*) AS TotalContracts,
    COUNT(DISTINCT ClientId) AS UniqueClientIds,
    MIN(ClientId) AS MinClientId,
    MAX(ClientId) AS MaxClientId
FROM Contracts;

PRINT '';

-- 2. Show contracts grouped by ClientId
PRINT '2. Contracts per ClientId:';
SELECT 
    ClientId,
    COUNT(*) AS ContractCount
FROM Contracts
GROUP BY ClientId
ORDER BY ContractCount DESC;

PRINT '';

-- 3. Show sample contracts with their ClientIds and Client Names
PRINT '3. Sample Contracts with Client Information (first 20):';
SELECT TOP 20
    c.Id AS ContractId,
    c.ContractNumber,
    c.LegacyContractId,
    c.ClientId,
    cl.Name AS ClientName,
    cl.LegacyCustomerId
FROM Contracts c
LEFT JOIN Clients cl ON c.ClientId = cl.Id
ORDER BY c.Id DESC;

PRINT '';

-- 4. Check if there are contracts with the same LegacyContractId but different ClientIds
PRINT '4. Contracts with same LegacyContractId but different ClientIds (if any):';
SELECT 
    LegacyContractId,
    COUNT(DISTINCT ClientId) AS DifferentClientIds,
    STRING_AGG(CAST(ClientId AS VARCHAR), ', ') AS ClientIds
FROM Contracts
WHERE LegacyContractId IS NOT NULL
GROUP BY LegacyContractId
HAVING COUNT(DISTINCT ClientId) > 1;

PRINT '';

-- 5. Check clients that have multiple contracts
PRINT '5. Clients with Multiple Contracts:';
SELECT 
    cl.Id AS ClientId,
    cl.Name AS ClientName,
    cl.LegacyCustomerId,
    COUNT(c.Id) AS ContractCount
FROM Clients cl
INNER JOIN Contracts c ON cl.Id = c.ClientId
GROUP BY cl.Id, cl.Name, cl.LegacyCustomerId
HAVING COUNT(c.Id) > 1
ORDER BY ContractCount DESC;

PRINT '';

-- 6. Verify: Check if all contracts are linked to the same client
PRINT '6. CRITICAL CHECK: Are all contracts linked to the same client?';
DECLARE @TotalContracts INT;
DECLARE @UniqueClientIds INT;

SELECT @TotalContracts = COUNT(*) FROM Contracts;
SELECT @UniqueClientIds = COUNT(DISTINCT ClientId) FROM Contracts;

IF @UniqueClientIds = 1
BEGIN
    DECLARE @SingleClientId BIGINT;
    DECLARE @SingleClientName NVARCHAR(200);
    
    SELECT TOP 1 @SingleClientId = ClientId FROM Contracts;
    SELECT @SingleClientName = Name FROM Clients WHERE Id = @SingleClientId;
    
    PRINT '⚠️  WARNING: All ' + CAST(@TotalContracts AS VARCHAR) + ' contracts are linked to the SAME client!';
    PRINT '   ClientId: ' + CAST(@SingleClientId AS VARCHAR);
    PRINT '   ClientName: ' + ISNULL(@SingleClientName, 'NULL');
    PRINT '';
    PRINT '   This indicates a problem in the migration script!';
    PRINT '   Check MigrateContractsFromTBS.sql - the client lookup logic may be incorrect.';
END
ELSE
BEGIN
    PRINT '✓ Contracts are linked to ' + CAST(@UniqueClientIds AS VARCHAR) + ' different clients.';
    PRINT '  Total contracts: ' + CAST(@TotalContracts AS VARCHAR);
END

PRINT '';
PRINT '============================================================================';
PRINT 'Verification Complete';
PRINT '============================================================================';

