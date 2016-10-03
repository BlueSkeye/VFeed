using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Reflection;
using System.Text;

namespace SSUpdate
{
    public static class Program
    {
        static Program()
        {
            _inInsertionOrderTableNames = new List<string>();
            _inInsertionOrderTableNames.Add("nvd_db");
            _inInsertionOrderTableNames.Add("cwe_db");
            _inInsertionOrderTableNames.Add("cve_cwe");
            _inInsertionOrderTableNames.Add("cwe_category");
            _inInsertionOrderTableNames.Add("cwe_capec");
            _inInsertionOrderTableNames.Add("cve_cpe");
            _inInsertionOrderTableNames.Add("cve_reference");
            _inInsertionOrderTableNames.Add("map_cve_aixapar");
            _inInsertionOrderTableNames.Add("map_cve_redhat");
            _inInsertionOrderTableNames.Add("map_redhat_bugzilla");
            _inInsertionOrderTableNames.Add("map_cve_suse");
            _inInsertionOrderTableNames.Add("map_cve_debian");
            _inInsertionOrderTableNames.Add("map_cve_mandriva");
            _inInsertionOrderTableNames.Add("map_cve_saint");
            _inInsertionOrderTableNames.Add("map_cve_milw0rm");
            _inInsertionOrderTableNames.Add("map_cve_osvdb");
            _inInsertionOrderTableNames.Add("map_cve_nessus");
            _inInsertionOrderTableNames.Add("map_cve_msf");
            _inInsertionOrderTableNames.Add("map_cve_openvas");
            _inInsertionOrderTableNames.Add("map_cve_scip");
            _inInsertionOrderTableNames.Add("map_cve_iavm");
            _inInsertionOrderTableNames.Add("map_cve_cisco");
            _inInsertionOrderTableNames.Add("map_cve_ubuntu");
            _inInsertionOrderTableNames.Add("map_cve_gentoo");
            _inInsertionOrderTableNames.Add("map_cve_fedora");
            _inInsertionOrderTableNames.Add("map_cve_certvn");
            _inInsertionOrderTableNames.Add("map_cve_ms");
            _inInsertionOrderTableNames.Add("map_cve_mskb");
            _inInsertionOrderTableNames.Add("map_cve_snort");
            _inInsertionOrderTableNames.Add("map_cve_suricata");
            _inInsertionOrderTableNames.Add("map_cve_vmware");
            _inInsertionOrderTableNames.Add("map_cve_bid");
            _inInsertionOrderTableNames.Add("map_cve_hp");
            _inInsertionOrderTableNames.Add("stat_new_cve");
            _inInsertionOrderTableNames.Add("map_cve_exploitdb");
            _inInsertionOrderTableNames.Add("map_cve_nmap");
            _inInsertionOrderTableNames.Add("map_cve_oval");
            _inInsertionOrderTableNames.Add("map_cve_d2");
            _inInsertionOrderTableNames.Add("stat_vfeed_kpi");
            _inInsertionOrderTableNames.Add("capec_db");
            _inInsertionOrderTableNames.Add("capec_mit");
            _inInsertionOrderTableNames.Add("cwe_wasc");
            return;
        }

        private static void AssertInputSchema(string tableName, SQLiteDataReader reader)
        {
            AssertTableInfoColumnsOrder(reader);
            List<ColumnInfo> columns = new List<ColumnInfo>();
            while (reader.Read()) {
                ColumnInfo thisColumn = new ColumnInfo() {
                    Name = reader.GetString(TableInfoNameColumIndex),
                    Type = reader.GetString(TableInfoTypeColumIndex),
                    Nullable = (0 == reader.GetInt32(TableInfoNullableFlagColumIndex)),
                    PrimaryKey = reader.GetInt32(TableInfoPrimaryKeyFlagColumIndex)
                };
                columns.Add(thisColumn);
            }
            _tablesByName[tableName] = columns;
            // TODO : Make sure the input schema is conformant with the current code line.
            // If not throw an exception.
            return;
        }

        private static void AssertTableInfoColumnsOrder(SQLiteDataReader reader)
        {
            if (_tableInfoColumnOrderAlreadyAsserted) { return; }
            int fieldsCount = reader.FieldCount;
            if (6 < fieldsCount) {
                throw new ApplicationException("TABLE INFO schema mismatch.");
            }
            string failedColumnName = null;
            try {
                if ((failedColumnName = "name") != reader.GetName(TableInfoNameColumIndex)) { return; }
                if ((failedColumnName = "type") != reader.GetName(TableInfoTypeColumIndex)) { return; }
                if ((failedColumnName = "notnull") != reader.GetName(TableInfoNullableFlagColumIndex)) { return; }
                if ((failedColumnName = "pk") != reader.GetName(TableInfoPrimaryKeyFlagColumIndex)) { return; }
                _tableInfoColumnOrderAlreadyAsserted = true;
                failedColumnName = null;
                return;
            }
            finally {
                if (null != failedColumnName) {
                    throw new ApplicationException(string.Format(
                        "TABLE INFO {0} column schema mismatch.", failedColumnName));
                }
            }
        }

        /// <summary>This is an helper method that partially reverse the Sqlite database
        /// into a Sql Server script.</summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private static string BuildSqlServerTableCreationInstruction(string tableName)
        {
            StringBuilder builder = new StringBuilder();
            List<ColumnInfo> columns;
            SortedList<int, ColumnInfo> primaryKeyColumns = new SortedList<int, ColumnInfo>();

            if (!_tablesByName.TryGetValue(tableName, out columns)) {
                throw new ArgumentException(string.Format("Unknown table '{0}'.", tableName));
            }
            builder.AppendFormat("CREATE TABLE [{0}] (\r\n", tableName);
            foreach(ColumnInfo column in columns) {
                if (0 != column.PrimaryKey) {
                    primaryKeyColumns.Add(column.PrimaryKey, column);
                }
                string sqlServerType;
                string sqliteType = column.Type.ToLower();
                int firstParenthesisIndex = sqliteType.IndexOf('(');
                string sizing = null;
                if (-1 != firstParenthesisIndex) {
                    sizing = sqliteType.Substring(firstParenthesisIndex);
                    sqliteType = sqliteType.Substring(0, firstParenthesisIndex);
                }
                switch (sqliteType) {
                    case "bigint":
                    case "date":
                    case "datetime":
                    case "int":
                    case "smallint":
                    case "text":
                    case "tinyint":
                        sqlServerType = sqliteType;
                        break;
                    case "character":
                    case "decimal":
                    case "nchar":
                    case "nvarchar":
                        sqlServerType = sqliteType + sizing;
                        break;
                    case "varying character":
                    case "native character":
                        sqlServerType = "nvarchar" + sizing;
                        break;
                    case "boolean":
                        sqlServerType = "bit";
                        break;
                    case "clob":
                        sqlServerType = "text";
                        break;
                    case "double":
                    case "double precision":
                        sqlServerType = "float";
                        break;
                    case "float":
                        sqlServerType = "real";
                        break;
                    case "int2":
                        sqlServerType = "smallint";
                        break;
                    case "int8":
                        sqlServerType = "bigint";
                        break;
                    case "integer":
                        sqlServerType = "int";
                        break;
                    case "mediumint":
                        sqlServerType = "int";
                        break;
                    case "name":
                        sqlServerType = "sysname";
                        break;
                    case "real":
                        sqlServerType = "float";
                        break;
                    case "unsigned big int":
                        sqlServerType = "bigint";
                        break;
                    default:
                        throw new NotImplementedException(
                            string.Format("Unrecognized sqlite type '{0}'.", column.Type));
                }
                builder.AppendFormat("\t[{0}] {1} {2},\r\n", column.Name,
                    sqlServerType, column.Nullable ? "NULL" : "NOT NULL");
            }
            if (0 < primaryKeyColumns.Count) {
                builder.Append("CONSTRAINT PRIMARY KEY CLUSTERED (");
                foreach(ColumnInfo primaryKeyColumn in primaryKeyColumns.Values) {
                    if (1 != primaryKeyColumn.PrimaryKey) { builder.Append(", "); }
                    builder.Append(primaryKeyColumn.Name);
                }
                builder.Append(")\r\n");
            }
            builder.Append(");\r\n");
            return builder.ToString();
        }

        private static SQLiteConnection ConnectToSQLite()
        {
            SQLiteConnection result = TryConnectToSQLite();
            if (null == result) { throw new ApplicationException(); }
            return result;
        }

        private static SqlConnection ConnectToSqlServer()
        {
            SqlConnection result = TryConnectToSqlServer();
            if (null == result) { throw new ApplicationException(); }
            return result;
        }

        private static void DisplayUsage(string programName)
        {
            Console.WriteLine("{0} [-f <database path> -s <target SQL Server> -d <Sql Server database>|-h]");
            Console.WriteLine();
            Console.WriteLine("-d : (optional) Sql Server vFeed database name (default vFeed).");
            Console.WriteLine("-f : path to vFeed database.");
            Console.WriteLine("-h : display this help notice.");
            Console.WriteLine(@"-s : target SQL server name ex : 'mymachine\SQLEXP12'.");
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

        private static string GetInsertCommand(string tableName, out int parametersCount)
        {
            StringBuilder builder = new StringBuilder();
            StringBuilder valuesBuilder = new StringBuilder();
            List<ColumnInfo> columns = _tablesByName[tableName];
            int parameterIndex = 0;
            foreach(ColumnInfo column in columns) {
                if (0 == builder.Length) {
                    builder.AppendFormat("INSERT [{0}] (", tableName);
                    valuesBuilder.AppendFormat(" VALUES (");
                }
                else {
                    builder.Append(", ");
                    valuesBuilder.Append(", ");
                }
                builder.Append("[" + column.Name + "]");
                valuesBuilder.AppendFormat("@P{0}", ++parameterIndex);
            }
            parametersCount = parameterIndex;
            builder.Append(")");
            valuesBuilder.Append(")");
            return builder.ToString() + valuesBuilder.ToString();
        }

        private static string GetReadCommand(string tableName)
        {
            StringBuilder builder = new StringBuilder();
            List<ColumnInfo> columns = _tablesByName[tableName];
            foreach(ColumnInfo column in columns) {
                builder.Append((0 == builder.Length) ? "SELECT " : ",");
                builder.Append("[" + column.Name + "]");
            }
            builder.AppendFormat(" FROM [{0}]", tableName);
            return builder.ToString();
        }

        private static bool ImportData()
        {
            bool atLeastOneIgnored = false;
            DateTime startTime = DateTime.Now;
            using (SQLiteConnection sqliteConnection = ConnectToSQLite()) {
                using (SqlConnection sqlServerConnection = ConnectToSqlServer()) {
                    foreach(string tableName in _inInsertionOrderTableNames) {
#if DEBUG
                        bool ignored = false;
                        foreach(string ignoredTableName in _ignoreTables) {
                            if (tableName == ignoredTableName) {
                                Console.WriteLine("Ignoring table '{0}'", ignoredTableName);
                                ignored = true;
                                break;
                            }
                        }
                        if (ignored) {
                            atLeastOneIgnored = true;
                            continue;
                        }
#endif
                        Console.WriteLine("Importing {0} table ", tableName);
                        int parametersCount;
                        string readCommand = GetReadCommand(tableName);
                        string insertCommand = GetInsertCommand(tableName, out parametersCount);
                        using (SQLiteCommand command = new SQLiteCommand(readCommand, sqliteConnection)) {
                            using(SQLiteDataReader reader = command.ExecuteReader()) {
                                Transfer(sqlServerConnection, reader, insertCommand, parametersCount);
                            }
                        }
                    }
                }
            }
            DateTime endTime = DateTime.Now;
            Console.WriteLine("Import completed in {0} seconds.",
                (int)((endTime - startTime).TotalSeconds));
            if (atLeastOneIgnored) {
                Console.WriteLine("WARNING : At least one table has been ignored.");
                return false;
            }
            return true;
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
                string[] tableNames = new string[_tablesByName.Keys.Count];
                _tablesByName.Keys.CopyTo(tableNames, 0);
                Console.WriteLine("Verifying input database schema.");
                foreach (string inputTable in tableNames) {
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
            if (!LoadSQLiteSchema()) { return 2; }
            if (!ImportData()) { return 3; }
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
                            using (FileStream data = TryOpenSqliteDatabase()) {
                                if (null == data) { return false; }
                            }
                            break;
                        case "H":
                            _displayUsage = true;
                            return true;
                        case "S":
                            _targetSqlServerName = TryGetAdditionalArgument(args, ref argIndex);
                            if (null == _targetSqlServerName) { return false; }
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
            if (null == _database) {
                Console.WriteLine("Mandatory -f parameter is missing.");
                return false;
            }
            _sqliteConnectionString = string.Format("Data Source={0};Version=3;",
                _database.FullName);
            using (SQLiteConnection connection = TryConnectToSQLite()) {
                if (null == connection) { return false; }
            }
            if (string.IsNullOrEmpty(_targetSqlServerName)) {
                Console.WriteLine("Mandatory -d parameter is missing.");
                return false;
            }
            _sqlServerConnectionString = string.Format(
                "Data Source={0};Initial Catalog={1};Integrated Security=True;MultipleActiveResultSets=True",
                _targetSqlServerName, _targetDatabaseName);
            using(SqlConnection connection = TryConnectToSqlServer()) {
                if (null == connection) { return false; }
            }
            return true;
        }

        private static void Transfer(SqlConnection sqlServerConnection, SQLiteDataReader reader,
            string insertCommandText, int parametersCount)
        {
            int linesCount = 0;
            using (SqlCommand insertCommand = new SqlCommand(insertCommandText, sqlServerConnection)) {
                SqlParameter[] parameters = new SqlParameter[parametersCount];
                for(int index = 0; index < parametersCount; index++) {
                    parameters[index] = insertCommand.Parameters.Add(new SqlParameter("@P" + (1 + index).ToString(), null));
                }
                while (reader.Read()) {
                    for(int index = 0; index < parametersCount; index++) {
                        parameters[index].Value = reader.GetValue(index);
                    }
                    insertCommand.ExecuteNonQuery();
                    if (0 == (++linesCount % 1000)) { Console.Write("."); }
                }
                Console.WriteLine("\r\n\r{0} lines imported.", linesCount);
                return;
            }
        }

        private static SQLiteConnection TryConnectToSQLite()
        {
            try {
                SQLiteConnection connection = new SQLiteConnection(_sqliteConnectionString);
                connection.Open();
                return connection;
            }
            catch (Exception e) {
                Console.WriteLine("Failed to connect to SQLite database : {0}", e.Message);
                return null;
            }
        }

        private static SqlConnection TryConnectToSqlServer()
        {
            try {
                SqlConnection connection = new SqlConnection(_sqlServerConnectionString);
                connection.Open();
                return connection;
            }
            catch (Exception e) {
                Console.WriteLine("Failed to connect to SqlServer database : {0}", e.Message);
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

        private static FileStream TryOpenSqliteDatabase()
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

        private static void VerifyTableDescription(SQLiteConnection connection,
            string tableName)
        {
            using (SQLiteCommand command = new SQLiteCommand(
                string.Format(GetTableColumnsDescriptionCommand, tableName), connection))
            {
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    if (!_tableInfoColumnOrderAlreadyAsserted) {
                        AssertTableInfoColumnsOrder(reader);
                    }
                    // Load schema.
                    if (!_schemaAlreadyAsserted) {
                        AssertInputSchema(tableName, reader);
                    }
                }
            }
            return;
        }

        private const string GetTableColumnsDescriptionCommand = "PRAGMA table_info('{0}');";
        private const string GetTableListCommand = "SELECT NAME from sqlite_master";
        private const int TableInfoNameColumIndex = 1;
        private const int TableInfoTypeColumIndex = 2;
        private const int TableInfoNullableFlagColumIndex = 3;
        private const int TableInfoPrimaryKeyFlagColumIndex = 5;
        private static FileInfo _database;
        private static bool _displayUsage;
#if DEBUG
        /// <summary>For debugging purpose only. This allow us to bypass
        /// already loaded tables and quicker fix a failing table.</summary>
        /// <remarks>This table should be empty during normal use.</remarks>
        private static readonly string[] _ignoreTables = new string[] {
            "nvd_db", "cwe_db", "cve_cwe", "cwe_category", "cwe_capec", "cve_cpe",
            "cve_reference", "map_cve_aixapar"
        };
#endif
        private static List<string> _inInsertionOrderTableNames;
        private static bool _schemaAlreadyAsserted;
        private static string _sqlServerConnectionString;
        private static string _sqliteConnectionString;
        private static Dictionary<string, List<ColumnInfo>> _tablesByName =
            new Dictionary<string, List<ColumnInfo>>();
        private static bool _tableInfoColumnOrderAlreadyAsserted;
        private static string _targetDatabaseName = "vFeed";
        private static string _targetSqlServerName;

        private struct ColumnInfo
        {
            internal string Name { get; set; }
            internal bool Nullable { get; set; }
            internal int PrimaryKey { get; set; }
            internal string Type { get; set; }
        }
    }
}
