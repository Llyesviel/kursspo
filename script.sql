BEGIN TRANSACTION;
GO

ALTER TABLE [Users] ADD [FullName] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Users] ADD [RegistrationDate] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Flight]') AND [c].[name] = N'FlightNumber');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Flight] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Flight] ALTER COLUMN [FlightNumber] nvarchar(50) NOT NULL;
GO

ALTER TABLE [Flight] ADD [ArrivalCityId] int NULL;
GO

ALTER TABLE [Flight] ADD [DepartureCityId] int NULL;
GO

ALTER TABLE [Aircraft] ADD [ManufacturerId] int NOT NULL DEFAULT 0;
GO

ALTER TABLE [Aircraft] ADD [Model] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [Aircraft] ADD [TotalSeats] int NOT NULL DEFAULT 0;
GO

CREATE TABLE [Country] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Country] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Manufacturer] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Country] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Manufacturer] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [City] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [CountryId] int NOT NULL,
    CONSTRAINT [PK_City] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_City_Country_CountryId] FOREIGN KEY ([CountryId]) REFERENCES [Country] ([Id]) ON DELETE CASCADE
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Country]'))
    SET IDENTITY_INSERT [Country] ON;
INSERT INTO [Country] ([Id], [Name])
VALUES (1, N'Россия'),
(2, N'США'),
(3, N'Германия');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[Country]'))
    SET IDENTITY_INSERT [Country] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Country', N'Name') AND [object_id] = OBJECT_ID(N'[Manufacturer]'))
    SET IDENTITY_INSERT [Manufacturer] ON;
INSERT INTO [Manufacturer] ([Id], [Country], [Name])
VALUES (1, N'Unknown', N'Default Manufacturer');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Country', N'Name') AND [object_id] = OBJECT_ID(N'[Manufacturer]'))
    SET IDENTITY_INSERT [Manufacturer] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CountryId', N'Name') AND [object_id] = OBJECT_ID(N'[City]'))
    SET IDENTITY_INSERT [City] ON;
INSERT INTO [City] ([Id], [CountryId], [Name])
VALUES (1, 1, N'Москва'),
(2, 1, N'Санкт-Петербург'),
(3, 2, N'Нью-Йорк'),
(4, 2, N'Лос-Анджелес'),
(5, 3, N'Берлин'),
(6, 3, N'Мюнхен');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CountryId', N'Name') AND [object_id] = OBJECT_ID(N'[City]'))
    SET IDENTITY_INSERT [City] OFF;
GO

CREATE INDEX [IX_Flight_ArrivalCityId] ON [Flight] ([ArrivalCityId]);
GO

CREATE INDEX [IX_Flight_DepartureCityId] ON [Flight] ([DepartureCityId]);
GO

CREATE INDEX [IX_Aircraft_ManufacturerId] ON [Aircraft] ([ManufacturerId]);
GO

CREATE INDEX [IX_City_CountryId] ON [City] ([CountryId]);
GO

ALTER TABLE [Aircraft] ADD CONSTRAINT [FK_Aircraft_Manufacturer_ManufacturerId] FOREIGN KEY ([ManufacturerId]) REFERENCES [Manufacturer] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Flight] ADD CONSTRAINT [FK_Flight_City_ArrivalCityId] FOREIGN KEY ([ArrivalCityId]) REFERENCES [City] ([Id]) ON DELETE NO ACTION;
GO

ALTER TABLE [Flight] ADD CONSTRAINT [FK_Flight_City_DepartureCityId] FOREIGN KEY ([DepartureCityId]) REFERENCES [City] ([Id]) ON DELETE NO ACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250514155939_AddFlightCityRelations', N'8.0.3');
GO

COMMIT;
GO

