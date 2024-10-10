using Entities;
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

        public OperationStatus CreateTable()
        {
            // Creates a default DB called TESTDB
            Directory.CreateDirectory($@"{DataPath}\TESTDB");

            // Creates a default Table called ESTUDIANTES
            var tablePath = $@"{DataPath}\TESTDB\ESTUDIANTES.Table";

            using (FileStream stream = File.Open(tablePath, FileMode.OpenOrCreate))
            using (BinaryWriter writer = new (stream))
            {
                // Create an object with a hardcoded.
                // First field is an int, second field is a string of size 30,
                // third is a string of 50
                int id = 1;
                string nombre = "Isaac".PadRight(30); // Pad to make the size of the string fixed
                string apellido = "Ramirez".PadRight(50);

                writer.Write(id);
                writer.Write(nombre);
                writer.Write(apellido);
            
            }
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
