using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using System.Reflection;

namespace SSUpdate
{
    public static class Program
    {
        private static void AssertTableInfoColumnsOrder(SQLiteDataReader reader)
        {
            if (_tableInfoColumnOrderAlreadyAsserted) { return; }
            int fieldsCount = reader.FieldCount;
            if (6 < fieldsCount) {
                throw new ApplicationException("TABLE INFO schema mismatch.");
            }
            if ("name" != reader.GetName(TableInfoNameColumIndex)) {
                throw new ApplicationException("TABLE INFO name column schema mismatch.");
            }
            if ("type" != reader.GetName(TableInfoTypeColumIndex)) {
                throw new ApplicationException("TABLE INFO type column schema mismatch.");
            }
            if ("notnull" != reader.GetName(TableInfoNullableFlagColumIndex)) {
                throw new ApplicationException("TABLE INFO notnull column schema mismatch.");
            }
            if ("pk" != reader.GetName(TableInfoPrimaryKeyFlagColumIndex)) {
                throw new ApplicationException("TABLE INFO pk column schema mismatch.");
            }
            _tableInfoColumnOrderAlreadyAsserted = true;
            return;
        }

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

        private static bool LoadSQLiteSchema()
        {
            Console.WriteLine("Validating input schema.");
            using (SQLiteConnection connection = TryConnectToSQLite()) {
                if (null == connection) { return false; }
                using (SQLiteCommand retrieveTablesCommand =
                    new SQLiteCommand(GetTableListCommand, connection))
                {
                    using (SQLiteDataReader reader = retrieveTablesCommand.ExecuteReader()) {
                        while (reader.Read()) {
                            _tablesByName.Add(reader.GetString(0), null);
                        }
                    }
                }
                foreach(string inputTable in _tablesByName.Keys) {
                    try { VerifyTableDescription(connection, inputTable); }
                    catch (Exception e) {
                        Console.WriteLine("Failed to load schema for table '{0}' : {1}",
                            inputTable, e.Message);
                        return false;
                    }
                }
            }
            return true;
        }

        private static Dictionary<string, object> _tablesByName =
            new Dictionary<string, object>();

        public static int Main(string[] args)
        {
            int result = 0;
            string programName;
            DisplayVersion(out programName);

            if (!ParseArgs(args)) {
                _displayUsage = true;
                result = 1;
            }
            if (_displayUsage) {
                DisplayUsage(programName);
                return result;
            }
            if (!LoadSQLiteSchema()) {
                return 2;
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
            using (SQLiteConnection connection = TryConnectToSQLite()) {
                if (null == connection) { return false; }
            }
            return true;
        }

        private static SQLiteConnection TryConnectToSQLite()
        {
            try {
                SQLiteConnection connection = new SQLiteConnection(_connectionString);
                connection.Open();
                return connection;
            }
            catch (Exception e) {
                Console.WriteLine("Failed to connect to SQLite database : {0}", e.Message);
                return null;
            }
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

        private static object VerifyTableDescription(SQLiteConnection connection,
            string tableName)
        {
            using (SQLiteCommand command = new SQLiteCommand(
                string.Format(GetTableColumnsDescriptionCommand, tableName), connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    if (!_tableInfoColumnOrderAlreadyAsserted) {
                        AssertTableInfoColumnsOrder(reader);
                    }
                    throw new NotImplementedException();
                }
            }
            return null;
        }

        private const string GetTableColumnsDescriptionCommand =
            "PRAGMA table_info('{0}');";
        private const string GetTableListCommand =
            "SELECT NAME from sqlite_master";
        private const int TableInfoNameColumIndex = 1;
        private const int TableInfoTypeColumIndex = 2;
        private const int TableInfoNullableFlagColumIndex = 3;
        private const int TableInfoPrimaryKeyFlagColumIndex = 5;
        private static string _connectionString;
        private static FileInfo _database;
        private static bool _displayUsage;
        private static bool _tableInfoColumnOrderAlreadyAsserted;
    }
}
