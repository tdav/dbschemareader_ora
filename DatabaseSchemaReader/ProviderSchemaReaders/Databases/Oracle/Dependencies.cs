using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    /// <summary>
    /// Reads object dependencies from Oracle ALL_DEPENDENCIES
    /// </summary>
    internal class Dependencies : OracleSqlExecuter<EntityDependency>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Dependencies"/> class
        /// </summary>
        /// <param name="commandTimeout">The command timeout</param>
        /// <param name="owner">The schema owner</param>
        public Dependencies(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
            Sql = @"SELECT 
    d.OWNER,
    d.NAME,
    d.TYPE,
    d.REFERENCED_OWNER,
    d.REFERENCED_NAME,
    d.REFERENCED_TYPE,
    d.DEPENDENCY_TYPE
FROM ALL_DEPENDENCIES d
WHERE (d.OWNER = :OWNER OR :OWNER IS NULL)
AND d.OWNER NOT IN ('SYS', 'SYSTEM', 'SYSMAN', 'CTXSYS', 'MDSYS', 'OLAPSYS', 'ORDSYS', 'OUTLN', 'WKSYS', 'WMSYS', 'XDB', 'ORDPLUGINS')
ORDER BY d.OWNER, d.NAME, d.TYPE";
        }

        /// <summary>
        /// Executes the query and returns dependencies
        /// </summary>
        /// <param name="connectionAdapter">The connection adapter</param>
        /// <returns>List of entity dependencies</returns>
        public IList<EntityDependency> Execute(IConnectionAdapter connectionAdapter)
        {
            try
            {
                ExecuteDbReader(connectionAdapter);
            }
            catch (DbException ex)
            {
                System.Diagnostics.Trace.WriteLine("Error reading oracle dependencies " + ex.Message);
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
        /// Maps a data record to an EntityDependency
        /// </summary>
        protected override void Mapper(IDataRecord record)
        {
            var owner = record.GetString("OWNER");
            var name = record.GetString("NAME");
            var type = record.GetString("TYPE");
            var refOwner = record.GetString("REFERENCED_OWNER");
            var refName = record.GetString("REFERENCED_NAME");
            var refType = record.GetString("REFERENCED_TYPE");
            var depType = record.GetString("DEPENDENCY_TYPE");

            var dependency = new EntityDependency
            {
                OwnerName = owner,
                ObjectName = name,
                ObjectType = ConvertToEntityType(type),
                ReferencedOwner = refOwner,
                ReferencedName = refName,
                ReferencedType = ConvertToEntityType(refType),
                DependencyType = depType
            };
            Result.Add(dependency);
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
