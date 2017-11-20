USE [GeoNames.org];
GO

----------------------------------------------------------------------------------------------------
DELETE FROM [gno].[AllCountries];
GO

BULK INSERT [gno].[AllCountries]  
FROM 'C:\Users\iperehyn\OneDrive - SoftServe\Projects\SailorMan\geonames.org\allCountries.txt'  
WITH
(
    KEEPNULLS
   ,FIELDTERMINATOR = '\t'
   ,CODEPAGE = '65001'
   --,DATAFILETYPE = 'widechar'
   --,ROWTERMINATOR = '\n'
   ,ROWTERMINATOR = '
'
);
GO
----------------------------------------------------------------------------------------------------
SELECT * FROM [gno].[AllCountries] WHERE  [GeoNameId] = 706165;
GO

DELETE FROM [gno].[AlternateNames];
GO

BULK INSERT [gno].[AlternateNames]  
FROM 'C:\Users\iperehyn\OneDrive - SoftServe\Projects\SailorMan\geonames.org\alternateNames.txt'  
WITH
(
    KEEPNULLS
   ,FIELDTERMINATOR = '\t'
   ,CODEPAGE = '65001'
   ,ROWTERMINATOR = '
'
);
GO
----------------------------------------------------------------------------------------------------
DELETE FROM [gno].[Admin1CodesASCII];
GO

BULK INSERT [gno].[Admin1CodesASCII]  
FROM 'C:\Users\iperehyn\OneDrive - SoftServe\Projects\SailorMan\geonames.org\admin1CodesASCII.txt'  
WITH
(
    KEEPNULLS
   ,FIELDTERMINATOR = '\t'
   ,CODEPAGE = '65001'
   ,ROWTERMINATOR = '
'
);
GO
----------------------------------------------------------------------------------------------------
DELETE FROM [gno].[Admin2Codes];
GO

BULK INSERT [gno].[Admin2Codes]  
FROM 'C:\Users\iperehyn\OneDrive - SoftServe\Projects\SailorMan\geonames.org\admin2Codes.txt'  
WITH
(
    KEEPNULLS
   ,FIELDTERMINATOR = '\t'
   ,CODEPAGE = '65001'
   ,ROWTERMINATOR = '
'
);
GO
----------------------------------------------------------------------------------------------------
DELETE FROM [gno].[ISO-LanguageCodes];
GO

BULK INSERT [gno].[ISO-LanguageCodes]  
FROM 'C:\Users\iperehyn\OneDrive - SoftServe\Projects\SailorMan\geonames.org\iso-languagecodes.txt'  
WITH
(
    KEEPNULLS
--    ,ERRORFILE = 'c:\tmp\err.log'
   ,FIRSTROW = 2
   ,FIELDTERMINATOR = '\t'
   ,CODEPAGE = '65001'
   ,ROWTERMINATOR = '
'
);
GO
----------------------------------------------------------------------------------------------------
DELETE FROM [gno].[TimeZones];
GO

BULK INSERT [gno].[TimeZones]  
FROM 'C:\Users\iperehyn\OneDrive - SoftServe\Projects\SailorMan\geonames.org\timeZones.txt'  
WITH
(
    KEEPNULLS
--    ,ERRORFILE = 'c:\tmp\err.log'
   ,FIRSTROW = 2
   ,FIELDTERMINATOR = '\t'
   ,CODEPAGE = '65001'
   ,ROWTERMINATOR = '
'
);
GO
----------------------------------------------------------------------------------------------------
DELETE FROM [gno].[CountryInfo];
GO

BULK INSERT [gno].[CountryInfo]  
FROM 'C:\Users\iperehyn\OneDrive - SoftServe\Projects\SailorMan\geonames.org\countryInfo.txt'  
WITH
(
    KEEPNULLS
    --,ERRORFILE = 'c:\tmp\err.log'
   ,FIRSTROW = 52
   ,FIELDTERMINATOR = '\t'
   ,ROWTERMINATOR = '
'
);
GO
----------------------------------------------------------------------------------------------------
DELETE FROM [gno].[Hierarchy];
GO

BULK INSERT [gno].[Hierarchy]  
FROM 'C:\Users\iperehyn\OneDrive - SoftServe\Projects\SailorMan\geonames.org\hierarchy.txt'  
WITH
(
    KEEPNULLS
    ,ERRORFILE = 'c:\tmp\err.log'
   ,FIRSTROW = 52
   ,CODEPAGE = '65001'
   ,FIELDTERMINATOR = '\t'
   ,ROWTERMINATOR = '
'
);
GO