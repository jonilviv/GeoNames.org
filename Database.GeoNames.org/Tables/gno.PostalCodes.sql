CREATE TABLE [gno].[PostalCodes]
(
    [CountryCode] CHAR(2) NOT NULL,       -- iso country code, 2 characters
    [PostalCode]  VARCHAR(20) NOT NULL,
    [PlaceName]   NVARCHAR(180) NOT NULL,
    [AdminName1]  NVARCHAR(100),          -- 1. order subdivision (state)
    [AdminCode1]  VARCHAR(20),            -- 1. order subdivision (state)
    [AdminName2]  VARCHAR(100),           -- 2. order subdivision (county/province)
    [AdminCode2]  VARCHAR(20),            -- 2. order subdivision (county/province)
    [AdminName3]  VARCHAR(100),           -- 3. order subdivision (community)
    [AdminCode3]  VARCHAR(20),            -- 3. order subdivision (community)
    [Latitude]    NUMERIC(8, 5),          -- estimated latitude (wgs84)
    [Longitude]   NUMERIC(8, 5),          -- estimated longitude (wgs84)
    [Accuracy]    TINYINT                 -- accuracy of lat/lng from 1=estimated to 6=centroid
);
GO