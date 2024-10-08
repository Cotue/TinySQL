﻿using Entities;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace StoreDataManager
{
    public sealed class Store
    {
        private static Store? instance = null;
        private static readonly object _lock = new object();
        private string? currentDatabase = null; // Variable para almacenar la base de datos seleccionada

        public static Store GetInstance()
        {
            lock (_lock)
            {
                if (instance == null)
                {
                    instance = new Store();
                }
                return instance;
            }
        }

        private const string DatabaseBasePath = @"C:\TinySql\";
        private const string DataPath = $@"{DatabaseBasePath}\Data";
        private const string SystemCatalogPath = $@"{DataPath}\SystemCatalog";
        private const string SystemDatabasesFile = $@"{SystemCatalogPath}\SystemDatabases.table";
        private const string SystemTablesFile = $@"{SystemCatalogPath}\SystemTables.table";

        public string GetDataPath()
        {
            return DataPath;
        }
        public Store()
        {
            this.InitializeSystemCatalog();
            
        }
        public OperationStatus SetDatabase(string databaseName)
        {
            var databasePath = $@"{DataPath}\{databaseName}";

            // Verificar si la base de datos existe
            if (!Directory.Exists(databasePath))
            {
                Console.WriteLine($"La base de datos {databaseName} no existe.");
                return OperationStatus.Error;
            }

            // Establecer la base de datos actual
            currentDatabase = databaseName;
            Console.WriteLine($"Contexto cambiado a la base de datos: {databaseName}");
            return OperationStatus.Success;
        }

        private void InitializeSystemCatalog()
        {
            // Always make sure that the system catalog and above folder
            // exist when initializing
            Directory.CreateDirectory(SystemCatalogPath);
        }

        public OperationStatus CreateTable(string tableName, Dictionary<string, string> columns)
        {
            var currentDatabase = GetCurrentDatabase();
            if (string.IsNullOrEmpty(currentDatabase))
            {
                Console.WriteLine("No se ha seleccionado ninguna base de datos.");
                return OperationStatus.Error;
            }

            var tablePath = $@"{GetDataPath()}\{currentDatabase}\{tableName}.Table";

            // Verificar si la tabla ya existe
            if (File.Exists(tablePath))
            {
                Console.WriteLine($"La tabla {tableName} ya existe.");
                return OperationStatus.Error;
            }

            // Crear el archivo de la tabla
            using (FileStream stream = File.Create(tablePath))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // Escribir el esquema de la tabla en el archivo
                writer.Write(columns.Count); // Escribir el número de columnas

                foreach (var column in columns)
                {
                    writer.Write(column.Key.PadRight(30));   // Nombre de la columna (ej. "ID")
                    writer.Write(column.Value.PadRight(20)); // Tipo de la columna (ej. "INTEGER")
                }
            }

            Console.WriteLine($"Tabla {tableName} creada en la base de datos {currentDatabase}.");
            return OperationStatus.Success;
        }

        public OperationStatus DropTable(string tableName)
        {
            var dataPath = GetDataPath();
            var currentDatabase = GetCurrentDatabase();
            var tablePath = $@"{dataPath}\{currentDatabase}\{tableName}.Table";

            // Verificar si la tabla existe
            if (!File.Exists(tablePath))
            {
                Console.WriteLine($"La tabla {tableName} no existe.");
                return OperationStatus.Error;
            }

            // Verificar si la tabla está vacía
            FileInfo fileInfo = new FileInfo(tablePath);
            if (fileInfo.Length > 0)
            {
                Console.WriteLine($"No se puede eliminar la tabla {tableName} porque no está vacía.");
                return OperationStatus.Error;
            }

            // Eliminar la tabla
            File.Delete(tablePath);
            Console.WriteLine($"La tabla {tableName} ha sido eliminada.");
            return OperationStatus.Success;
        }

        public string? GetCurrentDatabase()
        {
            return currentDatabase;
        }

        public OperationStatus CreateDatabase(string databaseName)
        {
            // Ruta para la base de datos
            var databasePath = $@"{DataPath}\{databaseName}";

            // Verificar si la base de datos ya existe
            if (Directory.Exists(databasePath))
            {
                Console.WriteLine($"La base de datos {databaseName} ya existe.");
                return OperationStatus.Error;  // Evitar duplicados
            }

            // Crear la carpeta de la base de datos
            Directory.CreateDirectory(databasePath);
            Console.WriteLine($"Base de datos {databaseName} creada en {databasePath}.");

            // Actualizar el archivo SystemDatabases con el nombre de la nueva base de datos
            var systemDatabasesPath = SystemDatabasesFile;
            using (FileStream stream = File.Open(systemDatabasesPath, FileMode.Append))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // Escribir el nombre de la nueva base de datos
                writer.Write(databaseName);
            }

            return OperationStatus.Success;
        }



        public OperationStatus Select()
        {
            // Creates a default Table called ESTUDIANTES
            var tablePath = $@"{DataPath}\TESTDB\ESTUDIANTES.Table";
            using (FileStream stream = File.Open(tablePath, FileMode.OpenOrCreate))
            using (BinaryReader reader = new (stream))
            {
                // Print the values as a I know exactly the types, but this needs to be done right
                Console.WriteLine(reader.ReadInt32());
                Console.WriteLine(reader.ReadString());
                Console.WriteLine(reader.ReadString());
                return OperationStatus.Success;
            }
        }
    }
}
