using Entities;
using QueryProcessor.Exceptions;
using QueryProcessor.Operations;
using StoreDataManager;
using System.Text.RegularExpressions;

namespace QueryProcessor
{
    public class SQLQueryProcessor
    {
        public static OperationStatus Execute(string sentence)
        {
            /// Parser para identificar y procesar las sentencias SQL
            Console.WriteLine($"Sentencia recibida: {sentence}");
            if (sentence.StartsWith("CREATE DATABASE"))
            {
                var parts = sentence.Split(' ');
                if (parts.Length != 3)
                {
                    throw new UnknownSQLSentenceException();
                }

                var databaseName = parts[2];  // Obtener el nombre de la base de datos
                return Store.GetInstance().CreateDatabase(databaseName);
            }

            if (sentence.StartsWith("SET DATABASE"))
            {
                var parts = sentence.Split(' ');
                if (parts.Length != 3)
                {
                    throw new UnknownSQLSentenceException();
                }

                var databaseName = parts[2];
                return Store.GetInstance().SetDatabase(databaseName);
            }

            if (sentence.StartsWith("CREATE TABLE"))
            {
                var parts = sentence.Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                var tableName = parts[0].Split(' ')[2]; // Obtener el nombre de la tabla
                var columnsPart = parts[1].Trim();

                // Extraer columnas y sus tipos
                var columns = columnsPart.Split(',');
                var columnsDict = new Dictionary<string, string>();

                foreach (var column in columns)
                {
                    var columnParts = column.Trim().Split(' ');
                    var columnName = columnParts[0];
                    var columnType = columnParts[1];
                    columnsDict.Add(columnName, columnType);
                }

                return new CreateTable().Execute(tableName, columnsDict);  // Llamada con los parámetros correctos
            }

            if (sentence.StartsWith("INSERT INTO"))
            {
                return new Insert().Execute(sentence);
            }

            if (sentence.StartsWith("DROP TABLE"))
            {
                var parts = sentence.Split(' ');
                if (parts.Length != 3)
                {
                    throw new UnknownSQLSentenceException();
                }

                var tableName = parts[2];  // Obtener el nombre de la tabla
                return Store.GetInstance().DropTable(tableName);
            }

            if (sentence.StartsWith("SELECT"))
            {
                return new Select().Execute();
            }

            if (sentence.StartsWith("CREATE INDEX"))
            {
                // Dividir la sentencia en partes clave
                var parts = sentence.Split(new[] { ' ', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                
                //Esto es para ver como se realiza el split
                Console.WriteLine("Sentencia dividida en partes:");
                for (int i = 0; i < parts.Length; i++)
                {
                    Console.WriteLine($"parts[{i}]: {parts[i]}");
                }

                if (parts.Length < 8 || parts[6] != "OF" || parts[7] != "TYPE")
                {
                    throw new UnknownSQLSentenceException(); // Lanza excepción si la sintaxis es incorrecta
                }

                var indexName = parts[2];         // IDX_Casas_ID
                var tableName = parts[4];         // Casas
                var columnName = parts[5];        // ID
                var indexType = parts[8];         // BTREE o BST

                Console.WriteLine($"Nombre del índice: {indexName}, Tabla: {tableName}, Columna: {columnName}, Tipo: {indexType}");

                // Llamar a Store para crear el índice
                return Store.GetInstance().CreateIndex(indexName, tableName, columnName, indexType);
            }



            // Si no coincide con ninguna sentencia conocida
            else
            {
                throw new UnknownSQLSentenceException();
            }
        }
    }
}



