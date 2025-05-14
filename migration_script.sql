BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250514160843_AddCitiesAndCountries', N'8.0.3');
GO

COMMIT;
GO

