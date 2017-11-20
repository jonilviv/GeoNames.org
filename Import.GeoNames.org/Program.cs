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
using ImportGeoNames;

namespace Import.Geonames.org
{
    public static class Program
    {
        private const string __baseUrl = @"http://download.geonames.org/export/dump/";

        private static string _tempFolderName;
        private static string _connectionString;

        private static readonly ImportEntity[] __tablesNames =
            {
                new ImportEntity("admin1CodesASCII"),
                new ImportEntity("admin2Codes"),
                new ImportEntity("countryInfo", firstRow: 52),
                new ImportEntity("hierarchy", true),
                new ImportEntity("iso-languagecodes", firstRow: 2),
                new ImportEntity("timeZones", firstRow: 2),
                new ImportEntity("alternateNames", true),
                new ImportEntity("allCountries", true)
            };


        [STAThread]
        private static void Main(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                string argument = args[i];

                switch (argument.ToLower())
                {
                    case "-h":
                    case "-help":
                        ShowHelp();

                        return;
                    case "-connectionstring":
                    case "-cs":
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
                    case "-tf":
                        i++;

                        if (args.Length < i + 1)
                        {
                            WriteError(Resources.BAD_TEMPORARY_FOLDER);

                            return;
                        }

                        _tempFolderName = args[i];

                        if (string.IsNullOrWhiteSpace(_tempFolderName))
                        {
                            WriteError(Resources.BAD_TEMPORARY_FOLDER);

                            return;
                        }

                        break;
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

            Parallel.ForEach(__tablesNames, DoImportEntity);

            FinalClear();

            ReadKey();
        }

        private static void WriteError(string errorMessage)
        {
            lock (Console.Error)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Error.WriteLine(errorMessage);
                Console.ForegroundColor = originalColor;
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine(@"-c , -connectionString      Connection String for connecting to MS SQL Server.");
            Console.WriteLine(@"-tf, -tempFolder            Temporary folder, where files will be saved. If not set. will be used system TEMP folder.");
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
                var url = __baseUrl + importEntity.FileName;
                var fileName = Path.Combine(_tempFolderName, importEntity.FileName);

                DownloadFile(url, fileName);
                Console.WriteLine(importEntity.TableName + " - downloaded");

                ClearTable(importEntity.TableName);
                Console.WriteLine(importEntity.TableName + " - table cleared");

                if (importEntity.IsZIPed)
                {
                    ExtractFile(fileName);
                    Console.WriteLine(importEntity.TableName + " - file was uziped");
                }

                BulkImport(importEntity);
            }
            catch (Exception exception)
            {
                WriteError(importEntity.TableName + " - " + exception.Message);
            }
        }

        private static void DownloadFile(string url, string fileName)
        {
            using (var client = new ExtendedWebClient())
            {
                client.DownloadFile(new Uri(url), fileName);
            }
        }

        private static void ExtractFile(string fileName)
        {
            try
            {
                ZipFile.ExtractToDirectory(fileName, _tempFolderName);
            }
            catch
            {
                //Some idiots puts 2 files in 1 archive! So we should suppress exception.
            }

            File.Delete(fileName);
        }

        private static void ClearTable(string tableName)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                string clearTableSql = $"DELETE FROM [gno].[{tableName}];";

                using (SqlCommand clearTable = new SqlCommand(clearTableSql, connection))
                {
                    clearTable.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        private static void BulkImport(ImportEntity importEntity)
        {
            var fileName = Path.Combine(_tempFolderName, importEntity.TableName + ".txt");
            var logFile = Path.Combine(_tempFolderName, importEntity.TableName + ".log");

            string sqlCommand = $@"
BULK INSERT [gno].[{importEntity.TableName}]  
FROM '{fileName}'  
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

            File.Delete(fileName);
            Console.WriteLine(importEntity.TableName + " - was imported");
        }

        private static void FinalClear()
        {
            IEnumerable<string> logFiles = Directory.EnumerateFiles(_tempFolderName, "*.log");

            if (!logFiles.Any())
            {
                Directory.Delete(_tempFolderName, true);
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