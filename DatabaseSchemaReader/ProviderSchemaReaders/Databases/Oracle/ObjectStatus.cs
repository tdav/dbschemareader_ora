using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    /// <summary>
    /// Reads object status information from Oracle ALL_OBJECTS
    /// </summary>
    internal class ObjectStatus : OracleSqlExecuter<DatabaseEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectStatus"/> class
        /// </summary>
        /// <param name="commandTimeout">The command timeout</param>
        /// <param name="owner">The schema owner</param>
        public ObjectStatus(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
            Sql = @"SELECT 
    OWNER,
    OBJECT_NAME,
    OBJECT_TYPE,
    STATUS,
    CREATED,
    LAST_DDL_TIME
FROM ALL_OBJECTS
WHERE (OWNER = :OWNER OR :OWNER IS NULL)
AND OWNER NOT IN ('SYS', 'SYSTEM', 'SYSMAN', 'CTXSYS', 'MDSYS', 'OLAPSYS', 'ORDSYS', 'OUTLN', 'WKSYS', 'WMSYS', 'XDB', 'ORDPLUGINS')
AND OBJECT_TYPE IN ('TABLE', 'VIEW', 'FUNCTION', 'PROCEDURE', 'PACKAGE', 'PACKAGE BODY', 'TRIGGER', 'SEQUENCE', 'SYNONYM', 'INDEX', 'TYPE', 'MATERIALIZED VIEW')
ORDER BY OWNER, OBJECT_TYPE, OBJECT_NAME";
        }

        /// <summary>
        /// Executes the query and returns object status information
        /// </summary>
        /// <param name="connectionAdapter">The connection adapter</param>
        /// <returns>List of database entities with status</returns>
        public IList<DatabaseEntity> Execute(IConnectionAdapter connectionAdapter)
        {
            try
            {
                ExecuteDbReader(connectionAdapter);
            }
            catch (DbException ex)
            {
                System.Diagnostics.Trace.WriteLine("Error reading oracle object status " + ex.Message);
            }
            return Result;
        }

        /// <summary>
        /// Adds parameters to the command
        /// </summary>
        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "Owner", Owner);
        }

        /// <summary>
        /// Maps a data record to a DatabaseEntity
        /// </summary>
        protected override void Mapper(IDataRecord record)
        {
            var owner = record.GetString("OWNER");
            var name = record.GetString("OBJECT_NAME");
            var type = record.GetString("OBJECT_TYPE");
            var status = record.GetString("STATUS");
            
            var entity = new DatabaseEntity
            {
                SchemaOwner = owner,
                Name = name,
                EntityType = ConvertToEntityType(type),
                Status = status
            };

            // Read dates if available
            var createdOrdinal = record.GetOrdinal("CREATED");
            if (!record.IsDBNull(createdOrdinal))
            {
                entity.Created = record.GetDateTime(createdOrdinal);
            }

            var lastDdlOrdinal = record.GetOrdinal("LAST_DDL_TIME");
            if (!record.IsDBNull(lastDdlOrdinal))
            {
                entity.LastDdlTime = record.GetDateTime(lastDdlOrdinal);
            }

            Result.Add(entity);
        }

        /// <summary>
        /// Converts Oracle object type string to DatabaseEntityType
        /// </summary>
        private static DatabaseEntityType ConvertToEntityType(string oracleType)
        {
            if (string.IsNullOrEmpty(oracleType))
                return DatabaseEntityType.Table;

            switch (oracleType.ToUpperInvariant())
            {
                case "TABLE":
                    return DatabaseEntityType.Table;
                case "VIEW":
                    return DatabaseEntityType.View;
                case "FUNCTION":
                    return DatabaseEntityType.Function;
                case "PROCEDURE":
                    return DatabaseEntityType.Procedure;
                case "PACKAGE":
                    return DatabaseEntityType.Package;
                case "PACKAGE BODY":
                    return DatabaseEntityType.PackageBody;
                case "TRIGGER":
                    return DatabaseEntityType.Trigger;
                case "SEQUENCE":
                    return DatabaseEntityType.Sequence;
                case "SYNONYM":
                    return DatabaseEntityType.Synonym;
                case "INDEX":
                    return DatabaseEntityType.Index;
                case "TYPE":
                    return DatabaseEntityType.Type;
                case "MATERIALIZED VIEW":
                    return DatabaseEntityType.MaterializedView;
                default:
                    return DatabaseEntityType.Table;
            }
        }
    }
}
