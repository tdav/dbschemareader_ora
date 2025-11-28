using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a graph of database entity dependencies
    /// </summary>
    [Serializable]
    public class DependencyGraph
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyGraph"/> class
        /// </summary>
        public DependencyGraph()
        {
            Nodes = new List<DatabaseEntity>();
            Edges = new List<EntityDependency>();
        }

        /// <summary>
        /// Gets or sets the list of entity nodes in the graph
        /// </summary>
        public List<DatabaseEntity> Nodes { get; set; }

        /// <summary>
        /// Gets or sets the list of dependency edges in the graph
        /// </summary>
        public List<EntityDependency> Edges { get; set; }

        /// <summary>
        /// Gets the entities that the specified entity depends on
        /// </summary>
        /// <param name="entity">The entity to find dependencies for</param>
        /// <returns>Entities that this entity depends on</returns>
        public IEnumerable<DatabaseEntity> GetDependencies(DatabaseEntity entity)
        {
            if (entity == null)
                return Enumerable.Empty<DatabaseEntity>();

            var dependencyNames = Edges
                .Where(e => string.Equals(e.OwnerName, entity.SchemaOwner, StringComparison.OrdinalIgnoreCase) &&
                           string.Equals(e.ObjectName, entity.Name, StringComparison.OrdinalIgnoreCase))
                .Select(e => new { Owner = e.ReferencedOwner, Name = e.ReferencedName })
                .ToList();

            return Nodes.Where(n => dependencyNames.Any(d =>
                string.Equals(d.Owner, n.SchemaOwner, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(d.Name, n.Name, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Gets the entities that depend on the specified entity
        /// </summary>
        /// <param name="entity">The entity to find reverse dependencies for</param>
        /// <returns>Entities that depend on this entity</returns>
        public IEnumerable<DatabaseEntity> GetReferencedBy(DatabaseEntity entity)
        {
            if (entity == null)
                return Enumerable.Empty<DatabaseEntity>();

            var referencedByNames = Edges
                .Where(e => string.Equals(e.ReferencedOwner, entity.SchemaOwner, StringComparison.OrdinalIgnoreCase) &&
                           string.Equals(e.ReferencedName, entity.Name, StringComparison.OrdinalIgnoreCase))
                .Select(e => new { Owner = e.OwnerName, Name = e.ObjectName })
                .ToList();

            return Nodes.Where(n => referencedByNames.Any(r =>
                string.Equals(r.Owner, n.SchemaOwner, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(r.Name, n.Name, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Gets all entities of a specific type
        /// </summary>
        /// <param name="type">The entity type to filter by</param>
        /// <returns>Entities of the specified type</returns>
        public IEnumerable<DatabaseEntity> GetByType(DatabaseEntityType type)
        {
            return Nodes.Where(n => n.EntityType == type);
        }

        /// <summary>
        /// Finds all circular dependencies in the graph
        /// </summary>
        /// <returns>Lists of entities that form circular dependencies</returns>
        public IEnumerable<List<DatabaseEntity>> FindCircularDependencies()
        {
            var result = new List<List<DatabaseEntity>>();
            var visited = new HashSet<string>();
            var recursionStack = new HashSet<string>();
            var path = new List<DatabaseEntity>();

            foreach (var node in Nodes)
            {
                var nodeKey = GetNodeKey(node);
                if (!visited.Contains(nodeKey))
                {
                    FindCircularDependenciesRecursive(node, visited, recursionStack, path, result);
                }
            }

            return result;
        }

        private void FindCircularDependenciesRecursive(
            DatabaseEntity node,
            HashSet<string> visited,
            HashSet<string> recursionStack,
            List<DatabaseEntity> path,
            List<List<DatabaseEntity>> result)
        {
            var nodeKey = GetNodeKey(node);
            visited.Add(nodeKey);
            recursionStack.Add(nodeKey);
            path.Add(node);

            foreach (var dependency in GetDependencies(node))
            {
                var depKey = GetNodeKey(dependency);

                if (!visited.Contains(depKey))
                {
                    FindCircularDependenciesRecursive(dependency, visited, recursionStack, path, result);
                }
                else if (recursionStack.Contains(depKey))
                {
                    // Found a cycle
                    var cycleStartIndex = path.FindIndex(p => GetNodeKey(p) == depKey);
                    if (cycleStartIndex >= 0)
                    {
                        var cycle = path.Skip(cycleStartIndex).ToList();
                        cycle.Add(dependency); // Complete the cycle
                        result.Add(cycle);
                    }
                }
            }

            path.RemoveAt(path.Count - 1);
            recursionStack.Remove(nodeKey);
        }

        private static string GetNodeKey(DatabaseEntity entity)
        {
            return string.Format("{0}.{1}", entity.SchemaOwner ?? "", entity.Name ?? "").ToUpperInvariant();
        }

        /// <summary>
        /// Finds an entity by name and optional type
        /// </summary>
        /// <param name="name">The entity name</param>
        /// <param name="schemaOwner">The schema owner (optional)</param>
        /// <param name="type">The entity type (optional)</param>
        /// <returns>The matching entity or null</returns>
        public DatabaseEntity FindEntity(string name, string schemaOwner = null, DatabaseEntityType? type = null)
        {
            return Nodes.FirstOrDefault(n =>
                string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase) &&
                (schemaOwner == null || string.Equals(n.SchemaOwner, schemaOwner, StringComparison.OrdinalIgnoreCase)) &&
                (!type.HasValue || n.EntityType == type.Value));
        }
    }
}
