using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;

namespace DatabaseSchemaReader.Data
{
    /// <summary>
    /// Exports data from Oracle database tables, respecting foreign key constraints.
    /// Exports the last N records from each table, ensuring at least one record per table.
    /// </summary>
    public class OracleDataExporter
    {
        private readonly DatabaseSchema _databaseSchema;
        private readonly string _connectionString;
        private readonly DbProviderFactory _providerFactory;
        private int _maxRecords = 1000;
        private int _minRecords = 1;

        /// <summary>
        /// Raised when progress is made during export.
        /// </summary>
        public event EventHandler<DataExportProgressEventArgs> Progress;

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleDataExporter"/> class.
        /// </summary>
        /// <param name="databaseSchema">The database schema with tables and constraints.</param>
        /// <param name="connectionString">The Oracle connection string.</param>
        /// <param name="providerFactory">The DbProviderFactory for Oracle.</param>
        public OracleDataExporter(DatabaseSchema databaseSchema, string connectionString, DbProviderFactory providerFactory)
        {
            _databaseSchema = databaseSchema ?? throw new ArgumentNullException(nameof(databaseSchema));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
        }

        /// <summary>
        /// Gets or sets the maximum number of records to export per table. Default is 1000.
        /// </summary>
        public int MaxRecords
        {
            get => _maxRecords;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Must be a positive number");
                _maxRecords = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum number of records to export per table. Default is 1.
        /// </summary>
        public int MinRecords
        {
            get => _minRecords;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value), "Cannot be negative");
                _minRecords = value;
            }
        }

        /// <summary>
        /// Whether to include identity columns in INSERT statements. Default is true.
        /// </summary>
        public bool IncludeIdentity { get; set; } = true;

        /// <summary>
        /// Whether to include BLOB columns. Default is false.
        /// </summary>
        public bool IncludeBlobs { get; set; }

        /// <summary>
        /// Whether to escape table and column names. Default is true.
        /// </summary>
        public bool EscapeNames { get; set; } = true;

        /// <summary>
        /// Exports data from all tables in the schema, ordered by foreign key dependencies.
        /// Returns INSERT statements for each table.
        /// </summary>
        /// <returns>Dictionary with table name as key and INSERT statements as value.</returns>
        public IDictionary<string, string> ExportAll()
        {
            var result = new Dictionary<string, string>();
            var sortedTables = SchemaTablesSorter.TopologicalSort(_databaseSchema).ToList();
            var totalTables = sortedTables.Count;
            var currentTableIndex = 0;

            using (var connection = _providerFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                connection.Open();

                foreach (var table in sortedTables)
                {
                    currentTableIndex++;
                    OnProgress(new DataExportProgressEventArgs(
                        currentTableIndex, 
                        totalTables, 
                        table.Name, 
                        $"Exporting table {currentTableIndex} of {totalTables}: {table.Name}"));

                    var inserts = ExportTable(table, connection);
                    if (!string.IsNullOrEmpty(inserts))
                    {
                        result[table.Name] = inserts;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Exports data from all tables and returns a single combined script.
        /// </summary>
        /// <returns>Combined INSERT statements for all tables.</returns>
        public string ExportAllAsScript()
        {
            var exports = ExportAll();
            var sb = new StringBuilder();
            
            sb.AppendLine("-- Oracle Data Export");
            sb.AppendLine($"-- Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"-- Tables exported in foreign key dependency order");
            sb.AppendLine();

            foreach (var kvp in exports)
            {
                sb.AppendLine($"-- Table: {kvp.Key}");
                sb.AppendLine(kvp.Value);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Exports data from a single table.
        /// </summary>
        /// <param name="table">The table to export.</param>
        /// <param name="connection">Open database connection.</param>
        /// <returns>INSERT statements for the table.</returns>
        public string ExportTable(DatabaseTable table, DbConnection connection)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            var selectSql = BuildSelectLastRecordsSql(table);
            var insertWriter = new InsertWriter(table, SqlType.Oracle)
            {
                IncludeIdentity = IncludeIdentity,
                IncludeBlobs = IncludeBlobs,
                EscapeNames = EscapeNames
            };

            var sb = new StringBuilder();
            int recordCount = 0;

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = selectSql;

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var insert = insertWriter.WriteInsert(reader);
                        sb.AppendLine(insert);
                        recordCount++;
                    }
                }
            }

            // Check minimum records requirement
            if (recordCount < MinRecords && recordCount == 0)
            {
                // Table is empty, nothing to export
                return string.Empty;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Builds a SELECT statement that gets the last N records from a table.
        /// Uses Oracle-specific syntax with ROWNUM and ORDER BY.
        /// </summary>
        /// <param name="table">The table to query.</param>
        /// <returns>SQL SELECT statement.</returns>
        public string BuildSelectLastRecordsSql(DatabaseTable table)
        {
            var sqlWriter = new SqlWriter(table, SqlType.Oracle);
            var columns = table.Columns
                .Where(c => !c.IsComputed)
                .Select(c => EscapeNames ? sqlWriter.EscapedColumnName(c.Name) : c.Name)
                .ToArray();

            var tableName = EscapeNames ? sqlWriter.EscapedTableName : table.Name;
            var columnsString = string.Join(", ", columns);

            // Get primary key columns for ORDER BY
            var orderByColumns = GetOrderByColumns(table, sqlWriter);

            // Oracle syntax for getting last N records:
            // SELECT * FROM (
            //   SELECT columns FROM table ORDER BY pk_columns DESC
            // ) WHERE ROWNUM <= N
            //
            // We need to wrap again to maintain original order after limiting
            var sb = new StringBuilder();
            sb.AppendLine("SELECT " + columnsString + " FROM (");
            sb.AppendLine("  SELECT " + columnsString + " FROM (");
            sb.AppendLine("    SELECT " + columnsString);
            sb.AppendLine("    FROM " + tableName);
            sb.AppendLine("    ORDER BY " + orderByColumns + " DESC");
            sb.AppendLine("  )");
            sb.AppendLine("  WHERE ROWNUM <= " + _maxRecords);
            sb.AppendLine(")");
            sb.AppendLine("ORDER BY " + orderByColumns + " ASC");

            return sb.ToString();
        }

        /// <summary>
        /// Gets the columns to use for ORDER BY clause.
        /// Prefers primary key columns, falls back to first column.
        /// </summary>
        private string GetOrderByColumns(DatabaseTable table, SqlWriter sqlWriter)
        {
            var pkColumns = new List<string>();

            if (table.PrimaryKey != null && table.PrimaryKey.Columns.Count > 0)
            {
                foreach (var colName in table.PrimaryKey.Columns)
                {
                    var escapedName = EscapeNames ? sqlWriter.EscapedColumnName(colName) : colName;
                    pkColumns.Add(escapedName);
                }
            }

            // If no primary key, use first column
            if (pkColumns.Count == 0 && table.Columns.Count > 0)
            {
                var firstCol = table.Columns[0].Name;
                pkColumns.Add(EscapeNames ? sqlWriter.EscapedColumnName(firstCol) : firstCol);
            }

            return string.Join(", ", pkColumns.ToArray());
        }

        /// <summary>
        /// Gets tables sorted by foreign key dependencies (parent tables first).
        /// </summary>
        /// <returns>Tables in dependency order.</returns>
        public IEnumerable<DatabaseTable> GetSortedTables()
        {
            return SchemaTablesSorter.TopologicalSort(_databaseSchema);
        }

        /// <summary>
        /// Raises the Progress event.
        /// </summary>
        protected virtual void OnProgress(DataExportProgressEventArgs e)
        {
            Progress?.Invoke(this, e);
        }
    }

    /// <summary>
    /// Event arguments for data export progress.
    /// </summary>
    public class DataExportProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the current table number being processed.
        /// </summary>
        public int CurrentTable { get; }

        /// <summary>
        /// Gets the total number of tables to process.
        /// </summary>
        public int TotalTables { get; }

        /// <summary>
        /// Gets the name of the current table.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Gets the progress message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the progress percentage (0-100).
        /// </summary>
        public int ProgressPercentage => TotalTables > 0 ? (CurrentTable * 100) / TotalTables : 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataExportProgressEventArgs"/> class.
        /// </summary>
        public DataExportProgressEventArgs(int currentTable, int totalTables, string tableName, string message)
        {
            CurrentTable = currentTable;
            TotalTables = totalTables;
            TableName = tableName;
            Message = message;
        }
    }
}
