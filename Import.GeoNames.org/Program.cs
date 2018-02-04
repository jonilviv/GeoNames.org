using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
using Import.GeoNames.org;
using Import.GeoNames.org.Properties;

namespace Import.Geonames.org
{
    public static class Program
    {
        private const string __baseUrl = @"http://download.geonames.org/export/dump/";
        private const string __postalCodesBaseUrl = @"http://download.geonames.org/export/zip/";

        private static volatile bool _shrinkInProggress;

        private static string _tempFolderName;
        private static string _connectionString;
        private static ImportEntity[] _tablesNames;

        [STAThread]
        private static void Main(string[] args)
        {
            if (!args.Any())
            {
                ShowHelp();

                return;
            }

            for (var i = 0; i < args.Length; i++)
            {
                string argument = args[i];

                switch (argument.ToLower())
                {
                    case "/?":
                    case "-h":
                    case "/h":
                    case "-help":
                    case "/help":
                        ShowHelp();

                        return;
                    case "-connectionstring":
                    case "/connectionstring":
                    case "-cs":
                    case "/cs":
                        i++;

                        if (args.Length < i + 1)
                        {
                            WriteError(Resources.BAD_CONNECTION_STRING);

                            return;
                        }

                        _connectionString = args[i];

                        if (string.IsNullOrWhiteSpace(_connectionString))
                        {
                            WriteError(Resources.BAD_CONNECTION_STRING);

                            return;
                        }

                        break;
                    case "-tempfolder":
                    case "/tempfolder":
                    case "-tf":
                    case "/tf":
                        i++;

                        if (args.Length < i + 1)
                        {
                            WriteError(Resources.BAD_TEMPORARY_FOLDER);

                            return;
                        }

                        _tempFolderName = args[i];
                        bool correctTempFolder = !string.IsNullOrWhiteSpace(_tempFolderName) && Directory.Exists(_tempFolderName);

                        if (!correctTempFolder)
                        {
                            WriteError(Resources.BAD_TEMPORARY_FOLDER);

                            return;
                        }

                        break;
                    case "-cleardb":
                    case "/cleardb":
                        BuildTables();
                        ClearDB();

                        return;
                }
            }

            if (string.IsNullOrWhiteSpace(_connectionString) || !CheckConnection())
            {
                WriteError(Resources.BAD_CONNECTION_STRING);

                return;
            }

            if (_tempFolderName == null)
            {
                if (!CreateTempFolder())
                {
                    return;
                }
            }

            BuildTables();

            Parallel.ForEach(_tablesNames, DoImportEntity);

            ReadKey();
        }

        private static void BuildTables()
        {
            _tablesNames = new[]
            {
                new ImportEntity(_tempFolderName, __postalCodesBaseUrl + "allCountries.zip", tableName: "PostalCodes"),
                new ImportEntity(_tempFolderName, __baseUrl + "allCountries.zip"),
                new ImportEntity(_tempFolderName, __baseUrl + "admin1CodesASCII.txt"),
                new ImportEntity(_tempFolderName, __baseUrl + "admin2Codes.txt"),
                new ImportEntity(_tempFolderName, __baseUrl + "countryInfo.txt", 52),
                new ImportEntity(_tempFolderName, __baseUrl + "hierarchy.zip"),
                new ImportEntity(_tempFolderName, __baseUrl + "iso-languagecodes.txt", 2),
                new ImportEntity(_tempFolderName, __baseUrl + "timeZones.txt", 2),
                new ImportEntity(_tempFolderName, __baseUrl + "alternateNames.zip")
            };
        }

        private static void ClearDB()
        {
            ShrinkDB();

            IEnumerable<string> tables = _tablesNames.Select(x => x.TableName);

            Parallel.ForEach(tables, ClearTable);

            ShrinkDB();
        }

        private static void WriteError(string errorMessage)
        {
            lock (Console.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(errorMessage);
                Console.ResetColor();
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine(@"-c , -connectionString      Connection String for connecting to MS SQL Server.");
            Console.WriteLine(@"-tf, -tempFolder            Temporary folder, where files will be saved. If not set. will be used system TEMP folder.");
            Console.WriteLine(@"-cleardb                    Clear whall DataBase.");
        }

        private static bool CheckConnection()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    connection.Close();
                }

                return true;
            }
            catch (Exception exception)
            {
                WriteError(exception.Message);

                return false;
            }
        }

        private static bool CreateTempFolder()
        {
            try
            {
                //var tmp = Environment.GetEnvironmentVariable("TEMP");
                //var tmp = Environment.GetFolderPath(Environment.SpecialFolder.Templates);
                //var tmp = Environment.GetFolderPath(Environment.SpecialFolder.CommonTemplates);
                var tmp = Path.GetTempPath();

                _tempFolderName = Path.Combine(tmp, "GeoNames.org");

                if (Directory.Exists(_tempFolderName))
                {
                    Directory.Delete(_tempFolderName, true);
                }

                Directory.CreateDirectory(_tempFolderName);

                var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                if (isWindows)
                {
                    GrantAccess(_tempFolderName);
                }

                Console.WriteLine("Temp folder is: " + _tempFolderName);

                return true;
            }
            catch (Exception exception)
            {
                WriteError(exception.Message);

                return false;
            }
        }

        private static void GrantAccess(string fullPath)
        {
            var directoryInfo = new DirectoryInfo(fullPath);
            DirectorySecurity dSecurity = directoryInfo.GetAccessControl();
            var identity = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            const InheritanceFlags inheritanceFlags = InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit;
            var rule = new FileSystemAccessRule(identity, FileSystemRights.FullControl, inheritanceFlags, PropagationFlags.NoPropagateInherit, AccessControlType.Allow);
            dSecurity.AddAccessRule(rule);
            directoryInfo.SetAccessControl(dSecurity);
        }

        private static void DoImportEntity(ImportEntity importEntity)
        {
            try
            {
                Console.WriteLine(importEntity.TableName + " - download started");
                Directory.CreateDirectory(importEntity.TempFolderName);
                DownloadFile(importEntity.FileURL, importEntity.ArchiveFilePath);
                Console.WriteLine(importEntity.TableName + " - downloaded");

                if (importEntity.IsZIPed)
                {
                    ExtractFile(importEntity);
                    Console.WriteLine(importEntity.TableName + " - file was uziped");
                }

                ShrinkDB();
                ClearTable(importEntity.TableName);
                Console.WriteLine(importEntity.TableName + " - table cleared");

                ShrinkDB();
                BulkImport(importEntity);
                ClearAfterBulkInsert(importEntity);
                ShrinkDB();
            }
            catch (Exception exception)
            {
                WriteError(importEntity.FileURL + " - " + exception.Message);
            }
        }

        private static void ShrinkDB()
        {
            if (_shrinkInProggress)
            {
                return;
            }

            _shrinkInProggress = true;
            Console.WriteLine("DataBase shrink started.");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    string sqlCommand = $@"DBCC SHRINKDATABASE ([{connection.Database}], 0);";

                    connection.Open();

                    using (var command = new SqlCommand(sqlCommand, connection))
                    {
                        command.CommandTimeout = int.MaxValue;
                        command.ExecuteNonQuery();
                    }

                    connection.Close();

                    Console.WriteLine("DataBase shrinked.");
                }
            }
            catch (Exception exception)
            {
                WriteError("DataBase shrinked exception: " + exception.Message);
            }
            finally
            {
                _shrinkInProggress = false;
            }
        }

        private static void DownloadFile(string url, string fileName)
        {
            using (var client = new ExtendedWebClient())
            {
                client.DownloadFile(new Uri(url), fileName);
            }
        }

        private static void ExtractFile(ImportEntity importEntity)
        {
            try
            {
                ZipFile.ExtractToDirectory(importEntity.ArchiveFilePath, importEntity.TempFolderName);
            }
            catch (IOException exception)
            {
                //Some idiots puts 2 files in 1 archive! So we should suppress exception.
            }
            catch (Exception exception)
            {
                WriteError($"{importEntity.TableName} - extract file exception: {exception.Message}");
            }

            File.Delete(importEntity.ArchiveFilePath);
        }

        private static void ClearTable(string tableName)
        {
            Console.WriteLine($"{tableName} - clear started.");

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string clearTableSql = $"TRUNCATE TABLE [gno].[{tableName}];";

                    using (SqlCommand clearTable = new SqlCommand(clearTableSql, connection))
                    {
                        clearTable.CommandTimeout = int.MaxValue;
                        clearTable.ExecuteNonQuery();
                    }

                    connection.Close();

                    Console.WriteLine($"{tableName} - cleared.");
                }
            }
            catch (Exception exception)
            {
                WriteError($"{tableName} - during cleare was raised exception: {exception.Message}");
            }
        }

        private static void BulkImport(ImportEntity importEntity)
        {
            Console.WriteLine(importEntity.TableName + " - import started");

            string logFile = importEntity.DataFilePath + ".log";

            string sqlCommand = $@"
BULK INSERT [gno].[{importEntity.TableName}]  
FROM '{importEntity.DataFilePath}'  
WITH
(
    KEEPNULLS
   ,ERRORFILE = '{logFile}'
   ,FIRSTROW = {importEntity.FirstRow}
   ,FIELDTERMINATOR = '\t'
   ,CODEPAGE = '65001'
   ,ROWTERMINATOR = '0x0a'
);
";
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SqlCommand(sqlCommand, connection))
                {
                    command.CommandTimeout = int.MaxValue;
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }

            Console.WriteLine(importEntity.TableName + " - was imported");
        }

        private static void ClearAfterBulkInsert(ImportEntity importEntity)
        {
            IEnumerable<string> logFiles = Directory.EnumerateFiles(importEntity.TempFolderName, "*.log");

            if (!logFiles.Any())
            {
                Directory.Delete(importEntity.TempFolderName, true);
            }
        }

        [Conditional("DEBUG")]
        private static void ReadKey()
        {
            Console.WriteLine(@"Application ended successfully");
            Console.WriteLine(@"*** Preas any key to exit...");
            Console.ReadKey();
        }
    }
}