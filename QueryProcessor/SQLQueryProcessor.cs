﻿using Entities;
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

            else
            {
                throw new UnknownSQLSentenceException();
            }
        }
    }
}


