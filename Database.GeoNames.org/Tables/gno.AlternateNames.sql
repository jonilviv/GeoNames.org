CREATE TABLE [gno].[AlternateNames]
(
  [Id] INT NOT NULL,
  [GeoNameId] INT NOT NULL,
  [ISOLanguage] VARCHAR(7) NOT NULL,
  [Name] NVARCHAR(400),
  [IsPreferredName] BIT,
  [IsShortName] BIT,
  [IsColloquial] BIT,
  [IsHistoric] BIT,
);
GO