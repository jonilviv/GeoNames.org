CREATE TABLE [gno].[CountryInfo]
(
  [ISO] CHAR(2) NOT NULL,
  [ISO3] CHAR(3) NOT NULL,
  [ISO-Numeric] SMALLINT NOT NULL,
  [Fips] VARCHAR(2),
  [Country] VARCHAR(100) NOT NULL,
  [Capital] VARCHAR(100),
  [Area] NUMERIC(10, 2) NOT NULL,
  [Population] BIGINT NOT NULL,
  [Continent] CHAR(2) NOT NULL,
  [tld] CHAR(3),
  [CurrencyCode] CHAR(3),
  [CurrencyName] VARCHAR(20),
  [Phone] VARCHAR(20),
  [PostalCodeFormat] VARCHAR(100),
  [PostalCodeRegex] VARCHAR(150),
  [Languages] VARCHAR(100),
  [GeonameId] INT NOT NULL,
  [Neighbours] VARCHAR(50),
  [EquivalentFipsCode] VARCHAR(2)
);
GO