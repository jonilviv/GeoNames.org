namespace Import.GeoNames.org
{
    internal class ImportEntity
    {
        public string TableName { get; }

        public bool IsZIPed { get; }

        public uint FirstRow { get; }

        public string FileName
        {
            get
            {
                string ext = IsZIPed ? "zip" : "txt";

                return $"{TableName}.{ext}";
            }
        }

        public ImportEntity(string tableName, bool isZIPed = false, uint firstRow = 1)
        {
            TableName = tableName;
            IsZIPed = isZIPed;
            FirstRow = firstRow;
        }
    }
}