CREATE TABLE [gno].[AllCountries]
(
  [GeoNameId] INT NOT NULL,
  [Name] NVARCHAR(200) NOT NULL,
  [ASCIIName] VARCHAR(200),
  [AlternateNames] NVARCHAR(MAX),
  [Latitude] NUMERIC(8, 5) NOT NULL,
  [Longitude] NUMERIC(8, 5) NOT NULL,
  [FeatureClass] CHAR(1),
  [FeatureCode] VARCHAR(10),
  [CountryCode] CHAR(2),
  [CC2] VARCHAR(200),
  [Admin1Code] VARCHAR(20),
  [Admin2Code] VARCHAR(80),
  [Admin3Code] VARCHAR(20),
  [Admin4Code] VARCHAR(20),
  [Population] BIGINT NOT NULL,
  [Elevation] INT,
  [DEM] INT NOT NULL,
  [Timezone] VARCHAR(40),
  [ModificationDate] date NOT NULL
);
GO