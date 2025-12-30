BEGIN TRANSACTION;
ALTER TABLE [SalesDeals] ADD [LegalReviewedAt] datetime2 NULL;

ALTER TABLE [SalesDeals] ADD [LegalReturnNotes] nvarchar(1000) NULL;

ALTER TABLE [SalesDeals] ADD [ReturnedToSalesmanAt] datetime2 NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251228153629_AddLegalFieldsToSalesDeals', N'10.0.0');

COMMIT;
GO

