using Entities;
using StoreDataManager;

namespace QueryProcessor.Operations
{
    internal class CreateTable
    {
        internal OperationStatus Execute(string tableName, Dictionary<string, string> columns)
        {
            return Store.GetInstance().CreateTable(tableName, columns);
        }
    }
}
