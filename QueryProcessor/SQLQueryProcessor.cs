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
                var parts = sentence.Split(' ');
                if (parts.Length != 3)
                {
                    throw new UnknownSQLSentenceException(); // Lanzar sin mensaje
                }

                var databaseName = parts[2];  // Obtener el nombre de la base de datos
                return Store.GetInstance().CreateDatabase(databaseName);
            }

            if (sentence.StartsWith("SET DATABASE"))
            {
                var parts = sentence.Split(' ');
                if (parts.Length != 3)
                {
                    throw new UnknownSQLSentenceException(); // Lanzar sin mensaje
                }

                var databaseName = parts[2];
                return Store.GetInstance().SetDatabase(databaseName);
            }

            if (sentence.StartsWith("INSERT INTO"))
            {
                
                return new Insert().Execute(sentence);
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


