using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using StoreDataManager;

namespace QueryProcessor.Operations
{
    internal class CreateDatabase
    {
        internal OperationStatus Execute()
        {
            return Store.GetInstance().CreateDatabase("Universidad");
        }
    }
}
