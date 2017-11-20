CREATE TABLE [gno].[TimeZones]
(
  [CountryCode] CHAR(2) NOT NULL,
  [TimeZoneId] VARCHAR(40) NOT NULL,
  [GMTOffset] NUMERIC(3, 1) NOT NULL,
  [DSTOffset] NUMERIC(3, 1) NOT NULL,
  [RawOffset] NUMERIC(3, 1) NOT NULL
);
GO