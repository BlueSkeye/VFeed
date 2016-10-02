using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SSUpdate
{
    public static class Program
    {
        private static void DisplayUsage(string programName)
        {
            Console.WriteLine("{0} [-f <database path>|-h]");
            Console.WriteLine();
            Console.WriteLine("-f : path to vFeed database.");
            Console.WriteLine("-h : display this help notice.");
            return;
        }

        private static void DisplayVersion(out string name)
        {
            Assembly mainAssembly = Assembly.GetEntryAssembly();
            AssemblyName mainAssemblyName = mainAssembly.GetName();

            name = mainAssemblyName.Name;
            Console.WriteLine("{0} v{1}.", name, mainAssemblyName.Version);
            return;
        }

        public static int Main(string[] args)
        {
            int result = 0;
            string programName;
            DisplayVersion(out programName);

            if (!ParseArgs(args)) {
                _displayUsage = true;
            }
            if (_displayUsage) {
                DisplayUsage(programName);
                return result;
            }
            return result;
        }

        private static bool ParseArgs(string[] args)
        {
            for(int argIndex = 0; args.Length > argIndex; argIndex++) {
                string scannedArg = args[argIndex];
                bool unrecognizedArgument = false;
                if ((2 > scannedArg.Length) || !scannedArg.StartsWith("-")) {
                    unrecognizedArgument = true;
                }
                else {
                    switch (scannedArg.Substring(1).ToUpper()) {
                        case "F":
                            string dbPath = TryGetAdditionalArgument(args, ref argIndex);
                            if (null == dbPath) { return false; }
                            _database = new FileInfo(dbPath);
                            if (!_database.Exists) {
                                Console.WriteLine("Database '{0}' not found.", _database.FullName);
                                return false;
                            }
                            using (FileStream data = TryOpenDatabase()) {
                                if (null == data) { return false; }
                            }
                            break;
                        default:
                            unrecognizedArgument = true;
                            break;
                    }
                }
                if (unrecognizedArgument) {
                    Console.WriteLine("Unrecognized argument '{0}'.",
                    scannedArg);
                    return false;
                }
            }
            _connectionString = string.Format("Data Source={0};Version=3;",
                _database.FullName);
            try {
                using (SQLiteConnection connection = new SQLiteConnection(_connectionString)) {
                    connection.Open();
                }
            }
            catch (Exception e) {
                Console.WriteLine("Failed to connect to SQLite database : {0}", e.Message);
                return false;
            }
            return true;
        }

        private static string TryGetAdditionalArgument(string[] from, ref int index)
        {
            string currentArgument = from[index];
            if (from.Length <= ++index) {
                Console.WriteLine("Additional argument missing after {0}.",
                    currentArgument);
                return null;
            }
            return from[index];
        }

        private static FileStream TryOpenDatabase()
        {
            try {
                return File.Open(_database.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception e) {
                Console.WriteLine("Error occurred while opening database '{0}' : {1}",
                    _database.FullName, e.Message);
                return null;
            }
        }

        private static string _connectionString;
        private static FileInfo _database;
        private static bool _displayUsage;
    }
}
