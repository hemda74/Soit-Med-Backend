-- ============================================================================
-- Mark Migrations as Applied
-- ============================================================================
-- This script marks the problematic migrations as already applied
-- Use this if the database schema is already partially updated
-- ============================================================================

-- Mark AddLegalFieldsToSalesDeals as applied (if not already)
IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20251228153629_AddLegalFieldsToSalesDeals')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20251228153629_AddLegalFieldsToSalesDeals', '8.0.0');
    PRINT 'Marked 20251228153629_AddLegalFieldsToSalesDeals as applied';
END
ELSE
BEGIN
    PRINT '20251228153629_AddLegalFieldsToSalesDeals already marked as applied';
END

-- Mark AddLegacyAndQrSupport as applied (if not already)
IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260101115621_AddLegacyAndQrSupport')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
    VALUES ('20260101115621_AddLegacyAndQrSupport', '8.0.0');
    PRINT 'Marked 20260101115621_AddLegacyAndQrSupport as applied';
END
ELSE
BEGIN
    PRINT '20260101115621_AddLegacyAndQrSupport already marked as applied';
END

PRINT 'Migration marking completed!';

