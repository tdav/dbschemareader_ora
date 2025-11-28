using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a dependency relationship between two database entities
    /// </summary>
    [Serializable]
    public class EntityDependency
    {
        /// <summary>
        /// Gets or sets the owner (schema) of the dependent object
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// Gets or sets the name of the dependent object
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// Gets or sets the type of the dependent object
        /// </summary>
        public DatabaseEntityType ObjectType { get; set; }

        /// <summary>
        /// Gets or sets the owner (schema) of the referenced object
        /// </summary>
        public string ReferencedOwner { get; set; }

        /// <summary>
        /// Gets or sets the name of the referenced object
        /// </summary>
        public string ReferencedName { get; set; }

        /// <summary>
        /// Gets or sets the type of the referenced object
        /// </summary>
        public DatabaseEntityType ReferencedType { get; set; }

        /// <summary>
        /// Gets or sets the dependency type (HARD or REF)
        /// </summary>
        public string DependencyType { get; set; }

        /// <summary>
        /// Returns a string representation of this dependency
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}.{1} ({2}) -> {3}.{4} ({5})",
                OwnerName, ObjectName, ObjectType,
                ReferencedOwner, ReferencedName, ReferencedType);
        }
    }
}
