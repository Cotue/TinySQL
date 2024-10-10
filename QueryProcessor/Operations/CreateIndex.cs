using StoreDataManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
namespace QueryProcessor.Operations
{
    internal class CreateIndex
    {
        internal OperationStatus Execute(string indexName, string tableName, string columnName, string indexType)
        {
            return Store.GetInstance().CreateIndex(indexName, tableName, columnName, indexType);
        }
    }
}
