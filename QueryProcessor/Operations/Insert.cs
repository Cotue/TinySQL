using StoreDataManager;
using System;
using System.IO;
using Entities;

namespace QueryProcessor.Operations
{
    internal class Insert
    {
        public OperationStatus Execute(string sentence)
        {
            // Parsear la sentencia SQL
            var parts = sentence.Split(' ');
            var tableName = parts[2];
            var valuesPart = sentence.Split("VALUES")[1].Trim('(', ')', ';');
            var values = valuesPart.Split(',');

            // Obtener el DataPath desde Store
            var dataPath = Store.GetInstance().GetDataPath();
            var tablePath = $@"{dataPath}\{Store.GetInstance().GetCurrentDatabase()}\{tableName}.Table";

            // Verificar si la tabla existe
            if (!File.Exists(tablePath))
            {
                Console.WriteLine($"La tabla {tableName} no existe.");
                return OperationStatus.Error;
            }

            // Insertar los valores en el archivo binario de la tabla
            using (FileStream stream = File.Open(tablePath, FileMode.Append))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                foreach (var value in values)
                {
                    writer.Write(value.Trim()); // Escribir los valores como cadenas (ajustar según el tipo de datos)
                }
            }

            Console.WriteLine($"Datos insertados en la tabla {tableName}");
            return OperationStatus.Success;
        }
    }
}
