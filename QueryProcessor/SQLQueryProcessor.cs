using Entities;
using QueryProcessor.Exceptions;
using QueryProcessor.Operations;
using StoreDataManager;

namespace QueryProcessor
{
    public class SQLQueryProcessor
    {
        public static OperationStatus Execute(string sentence)
        {
            /// Parser para identificar y procesar las sentencias SQL

            if (sentence.StartsWith("CREATE DATABASE"))
            {
                // Extraer el nombre de la base de datos de la sentencia SQL
                var parts = sentence.Split(' ');
                if (parts.Length != 3)
                {
                    throw new UnknownSQLSentenceException();
                }

                var databaseName = parts[2];  // Obtener el nombre de la base de datos
                return Store.GetInstance().CreateDatabase(databaseName);
            }

            if (sentence.StartsWith("CREATE TABLE"))
            {
                return new CreateTable().Execute();
            }

            if (sentence.StartsWith("SELECT"))
            {
                return new Select().Execute();
            }

            else
            {
                throw new UnknownSQLSentenceException();
            }
        }
    }
}

