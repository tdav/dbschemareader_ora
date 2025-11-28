using System;
using System.Collections.Generic;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a unified database entity for dependency analysis
    /// </summary>
    [Serializable]
    public class DatabaseEntity : NamedSchemaObject<DatabaseEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseEntity"/> class
        /// </summary>
        public DatabaseEntity()
        {
            Dependencies = new List<EntityDependency>();
            ReferencedBy = new List<EntityDependency>();
        }

        /// <summary>
        /// Gets or sets the entity type
        /// </summary>
        public DatabaseEntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the source code (for procedures, functions, triggers, etc.)
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// Gets or sets the status (VALID, INVALID)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the creation date
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// Gets or sets the last DDL modification time
        /// </summary>
        public DateTime? LastDdlTime { get; set; }

        /// <summary>
        /// Gets or sets the list of entities that this entity depends on
        /// </summary>
        public List<EntityDependency> Dependencies { get; set; }

        /// <summary>
        /// Gets or sets the list of entities that depend on this entity
        /// </summary>
        public List<EntityDependency> ReferencedBy { get; set; }

        /// <summary>
        /// Returns a string representation of this entity
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}.{1} ({2})", SchemaOwner, Name, EntityType);
        }
    }
}
