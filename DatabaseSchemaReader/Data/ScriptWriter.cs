using System;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using System.Data;

namespace DatabaseSchemaReader.Data
{
    /// <summary>
    /// Reads a database table schema and data, and writes data to INSERT statements (for copies or backup)
    /// </summary>
    /// <remarks>
    /// This wraps Reader and InsertWriter to provide a higher level API.
    /// </remarks>
    public class ScriptWriter
    {
        private int _pageSize = 1000;

        /// <summary>
        /// Escape table and column names (default true)
        /// </summary>
        public bool EscapeNames { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of records returned. Default is 1000.
        /// </summary>
        /// <value>The size of the page.</value>
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                if (value <= 0) throw new InvalidOperationException("Must be a positive number");
                if (value > 10000) throw new InvalidOperationException("Value is too large - consider another method");
                _pageSize = value;
            }
        }

        /// <summary>
        /// Include identity values in INSERTs
        /// </summary>
        /// <value>
        ///   <c>true</c> if include identity; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeIdentity { get; set; }


        /// <summary>
        /// Include BLOB in INSERTS. This is only practical for small blobs for certain databases (eg it works in SqlServer Northwind).
        /// </summary>
        /// <value><c>true</c> if include blobs; otherwise, <c>false</c>.</value>
        public bool IncludeBlobs { get; set; }

        private static SqlType FindSqlType(string providerName)
        {
            var sqlType = ProviderToSqlType.Convert(providerName);
            return !sqlType.HasValue ? SqlType.SqlServer : sqlType.Value;
        }

//#if !NETSTANDARD2_0
        /// <summary>
        /// Reads the table schema and data and returns the INSERT statements
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="connection">The connection (must have a connection string.</param>
        /// <returns></returns>
        public string ReadTable(string tableName, DbConnection connection)
        {
            using (var dr = new DatabaseReader(connection))
            {
                var databaseTable = dr.Table(tableName);
                if (databaseTable == null) return null;
                return ReadTable(databaseTable, connection);
            }
        }

        /// <summary>
        /// Reads the table data and returns the INSERT statements
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        public string ReadTable(DatabaseTable databaseTable, DbConnection connection)
        {
 
            var r = new Reader(databaseTable);
 
            r.PageSize = PageSize;
            var dt = r.Read(connection);
            var w = new InsertWriter(databaseTable, dt);
            w.IncludeIdentity = IncludeIdentity;
            w.IncludeBlobs = IncludeBlobs;
            w.EscapeNames = EscapeNames;
            var providerName = connection.GetType().Namespace;
            return w.Write(FindSqlType(providerName));
        }
        
        /// <summary>
        /// Reads the table data and invokes the function for each INSERT statement. The databaseTable must have dataTypes (call DataReader.DataTypes()).
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="processRecord">The process record.</param>
        public void ReadTable(DatabaseTable databaseTable, DbConnection connection,
                              Func<string, bool> processRecord)
        {
            var providerName = connection.GetType().Namespace;
 
            var r = new Reader(databaseTable);
 
            var w = new InsertWriter(databaseTable, FindSqlType(providerName));
            r.Read(connection, record =>
                       {
                           var s = w.WriteInsert(record);
                           return processRecord(s);
                       });

        } 
 
        public string ReadTable(string tableName, string connectionString, string providerName)
        {
            var factory = DbProviderFactories.GetFactory(providerName);
            using var connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;
            return ReadTable(tableName, connection);
        }
        public string ReadTable(DatabaseTable databaseTable, string connectionString, string providerName)
        {
            var factory = DbProviderFactories.GetFactory(providerName);
            using var connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;
            return ReadTable(databaseTable, connection);
        }
        public void ReadTable(DatabaseTable databaseTable, string connectionString, string providerName, Func<string,bool> processRecord)
        {
            var factory = DbProviderFactories.GetFactory(providerName);
            using var connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;
            ReadTable(databaseTable, connection, processRecord);
        }
    }
}
