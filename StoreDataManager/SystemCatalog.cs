using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreDataManager
{
    public static class SystemCatalog
    {
        private static List<Index> indices = new List<Index>();

        public static void AddIndex(string indexName, string tableName, string columnName, string indexType)
        {
            indices.Add(new Index
            {
                IndexName = indexName,
                TableName = tableName,
                ColumnName = columnName,
                IndexType = indexType
            });
        }

        public static bool ContainsIndex(string tableName, string columnName)
        {
            return indices.Any(index => index.TableName == tableName && index.ColumnName == columnName);
        }
    }
}
