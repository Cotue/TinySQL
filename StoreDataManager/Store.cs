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
        //Crea indice para los arboles
        public OperationStatus CreateIndex(string indexName, string tableName, string columnName, string indexType)
        {
            var tablePath = $@"{GetDataPath()}\{GetCurrentDatabase()}\{tableName}.Table";

            // Verificar si la tabla existe
            if (!File.Exists(tablePath))
            {
                Console.WriteLine($"La tabla {tableName} no existe.");
                return OperationStatus.Error;
            }

            // Verificar si ya existe un índice en esa columna
            if (SystemCatalog.ContainsIndex(tableName, columnName))
            {
                Console.WriteLine($"Ya existe un índice en la columna {columnName}.");
                return OperationStatus.Error;
            }

            // Crear el índice basado en el tipo
            if (indexType == "BTREE")
            {
                BTree index = new BTree(3);  // Crear un BTree de grado 3
                Console.WriteLine($"Creando índice BTREE en la columna {columnName} de la tabla {tableName}.");

                // Imprimir la cabecera de la tabla
                Console.WriteLine("ID".PadRight(10) + "|" + "Nombre".PadRight(30) + "|" + "Apellido".PadRight(50));
                Console.WriteLine(new string('-', 90));

                // Recorrer la columna y agregar al índice
                using (FileStream stream = File.Open(tablePath, FileMode.Open))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    while (stream.Position < stream.Length)
                    {
                        try
                        {
                            int id = reader.ReadInt32(); // Leer el ID (entero)
                            string nombre = reader.ReadString().Trim(); // Leer el nombre, quitar espacios en blanco al final
                            string apellido = reader.ReadString().Trim(); // Leer el apellido, quitar espacios en blanco al final

                            // Asegurarse de que los datos se alineen correctamente con los espacios definidos
                            Console.WriteLine("{0,-10} | {1,-30} | {2,-50}", id, nombre, apellido);

                            // Insertar en el índice si es la columna que se está indexando
                            if (columnName == "ID")
                            {
                                index.Insert(id);
                            }
                        }
                        catch (EndOfStreamException)
                        {
                            Console.WriteLine("Fin del archivo alcanzado.");
                            break;
                        }
                    }
                }
            }
            else if (indexType == "BST")
            {
                BST index = new BST();  // Crear un BST
                Console.WriteLine($"Creando índice BST en la columna {columnName} de la tabla {tableName}.");

                // Imprimir la cabecera de la tabla
                Console.WriteLine("ID".PadRight(10) + "|" + "Nombre".PadRight(30) + "|" + "Apellido".PadRight(50));
                Console.WriteLine(new string('-', 90));

                // Recorrer la columna y agregar al índice
                using (FileStream stream = File.Open(tablePath, FileMode.Open))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    while (stream.Position < stream.Length)
                    {
                        try
                        {
                            int id = reader.ReadInt32(); // Leer el ID (entero)
                            string nombre = reader.ReadString().Trim(); // Leer el nombre, quitar espacios en blanco al final
                            string apellido = reader.ReadString().Trim(); // Leer el apellido, quitar espacios en blanco al final

                            // Asegurarse de que los datos se alineen correctamente con los espacios definidos
                            Console.WriteLine("{0,-10} | {1,-30} | {2,-50}", id, nombre, apellido);

                            // Insertar en el índice si es la columna que se está indexando
                            if (columnName == "ID")
                            {
                                index.Insert(id);
                            }
                        }
                        catch (EndOfStreamException)
                        {
                            Console.WriteLine("Fin del archivo alcanzado.");
                            break;
                        }
                    }
                }
            }

            // Registrar el índice en el system catalog
            SystemCatalog.AddIndex(indexName, tableName, columnName, indexType);
            Console.WriteLine($"Índice {indexName} creado en la columna {columnName} de la tabla {tableName}.");

            return OperationStatus.Success;
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
                    writer.Write(column.Key.PadRight(30, ' '));   // Nombre de la columna
                    writer.Write(column.Value.PadRight(20, ' ')); // Tipo de la columna
                }
            }

            Console.WriteLine($"Tabla {tableName} creada en la base de datos {currentDatabase}.");
            Console.WriteLine($"Ruta de la tabla: {tablePath}");

            return OperationStatus.Success;
        }



        public OperationStatus DropTable(string tableName)
        {
            var dataPath = GetDataPath();
            var currentDatabase = GetCurrentDatabase();
            var tablePath = $@"{dataPath}\{currentDatabase}\{tableName}.Table";
            Console.WriteLine($"Ruta de la tabla: {tablePath}");

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



        public OperationStatus Select(string tableName)
        {
            var tablePath = $@"{GetDataPath()}\{GetCurrentDatabase()}\{tableName}.Table";

            // Verificar si la tabla existe
            if (!File.Exists(tablePath))
            {
                Console.WriteLine($"La tabla {tableName} no existe.");
                return OperationStatus.Error;
            }

            Console.WriteLine($"Mostrando datos de la tabla {tableName}:");


            // Recorrer los registros de la tabla
            using (FileStream stream = File.Open(tablePath, FileMode.Open))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Console.WriteLine("ID".PadRight(10) + "|" + "Nombre".PadRight(30) + "|" + "Apellido".PadRight(50));
                Console.WriteLine(new string('-', 90));

                while (stream.Position < stream.Length)
                {
                    try
                    {
                        int id = reader.ReadInt32(); // Leer el ID (entero)
                        string nombre = reader.ReadString().Trim(); // Leer el nombre, quitar espacios en blanco al final
                        string apellido = reader.ReadString().Trim(); // Leer el apellido, quitar espacios en blanco al final

                        // Asegurarse de que los datos se alineen correctamente con los espacios definidos
                        Console.WriteLine(id.ToString().PadRight(10) + "|" + nombre.PadRight(30) + "|" + apellido.PadRight(50));
                    }
                    catch (EndOfStreamException)
                    {
                        Console.WriteLine("Fin del archivo alcanzado.");
                        break;
                    }
                }
            }



            return OperationStatus.Success;
        }

    }
}
