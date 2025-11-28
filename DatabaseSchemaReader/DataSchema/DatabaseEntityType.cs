using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Enumeration of database entity types for unified dependency analysis
    /// </summary>
    [Serializable]
    public enum DatabaseEntityType
    {
        /// <summary>
        /// Database table
        /// </summary>
        Table,

        /// <summary>
        /// Database view
        /// </summary>
        View,

        /// <summary>
        /// Database function
        /// </summary>
        Function,

        /// <summary>
        /// Stored procedure
        /// </summary>
        Procedure,

        /// <summary>
        /// Package (Oracle)
        /// </summary>
        Package,

        /// <summary>
        /// Package body (Oracle)
        /// </summary>
        PackageBody,

        /// <summary>
        /// Database trigger
        /// </summary>
        Trigger,

        /// <summary>
        /// Sequence
        /// </summary>
        Sequence,

        /// <summary>
        /// Synonym
        /// </summary>
        Synonym,

        /// <summary>
        /// Index
        /// </summary>
        Index,

        /// <summary>
        /// Constraint
        /// </summary>
        Constraint,

        /// <summary>
        /// User-defined type
        /// </summary>
        Type,

        /// <summary>
        /// Materialized view
        /// </summary>
        MaterializedView
    }
}
