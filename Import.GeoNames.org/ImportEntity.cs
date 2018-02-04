using System;
using System.IO;

namespace Import.GeoNames.org
{
    internal class ImportEntity
    {
        public string FileURL { get; }

        public uint FirstRow { get; }

        public string TableName { get; }

        public string TempFolderName { get; }

        public bool IsZIPed { get; }

        public string DataFilePath { get; }

        public string ArchiveFilePath { get; }

        public ImportEntity(string tempFolderName, string fileUrl, uint firstRow = 1, string tableName = null)
        {
            FileURL = fileUrl;
            FirstRow = firstRow;
            TableName = tableName ?? Path.GetFileNameWithoutExtension(FileURL);

            if (string.IsNullOrWhiteSpace(tempFolderName))
            {
                return;
            }

            TempFolderName = Path.Combine(tempFolderName, Guid.NewGuid().ToString());

            string extention = Path.GetExtension(FileURL)?.ToLower();
            IsZIPed = extention == ".zip";

            string originalDataFileName = Path.GetFileNameWithoutExtension(fileUrl);

            DataFilePath = Path.Combine(TempFolderName, originalDataFileName + ".txt");

            if (IsZIPed)
            {
                ArchiveFilePath = Path.Combine(TempFolderName, TableName + ".zip");
            }
            else
            {
                ArchiveFilePath = DataFilePath;
            }
        }
    }
}