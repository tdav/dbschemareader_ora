using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Procedures
{
    /// <summary>
    /// Builds dependency graphs from database schema and dependency information
    /// </summary>
    public class DependencyGraphBuilder
    {
        /// <summary>
        /// Builds a dependency graph from a database schema
        /// </summary>
        /// <param name="schema">The database schema</param>
        /// <returns>The dependency graph</returns>
        public DependencyGraph BuildFromSchema(DatabaseSchema schema)
        {
            if (schema == null)
                throw new ArgumentNullException("schema");

            var graph = new DependencyGraph();

            // Add tables as nodes
            foreach (var table in schema.Tables)
            {
                var entity = new DatabaseEntity
                {
                    SchemaOwner = table.SchemaOwner,
                    Name = table.Name,
                    EntityType = DatabaseEntityType.Table
                };
                graph.Nodes.Add(entity);
            }

            // Add views as nodes
            foreach (var view in schema.Views)
            {
                var entity = new DatabaseEntity
                {
                    SchemaOwner = view.SchemaOwner,
                    Name = view.Name,
                    EntityType = DatabaseEntityType.View,
                    SourceCode = view.Sql
                };
                graph.Nodes.Add(entity);
            }

            // Add stored procedures as nodes
            foreach (var proc in schema.StoredProcedures)
            {
                var entity = new DatabaseEntity
                {
                    SchemaOwner = proc.SchemaOwner,
                    Name = proc.Name,
                    EntityType = DatabaseEntityType.Procedure,
                    SourceCode = proc.Sql
                };
                graph.Nodes.Add(entity);
            }

            // Add functions as nodes
            foreach (var func in schema.Functions)
            {
                var entity = new DatabaseEntity
                {
                    SchemaOwner = func.SchemaOwner,
                    Name = func.Name,
                    EntityType = DatabaseEntityType.Function,
                    SourceCode = func.Sql
                };
                graph.Nodes.Add(entity);
            }

            // Add packages as nodes
            foreach (var pack in schema.Packages)
            {
                var entity = new DatabaseEntity
                {
                    SchemaOwner = pack.SchemaOwner,
                    Name = pack.Name,
                    EntityType = DatabaseEntityType.Package,
                    SourceCode = pack.Definition
                };
                graph.Nodes.Add(entity);

                // Also add package body if available
                if (!string.IsNullOrEmpty(pack.Body))
                {
                    var bodyEntity = new DatabaseEntity
                    {
                        SchemaOwner = pack.SchemaOwner,
                        Name = pack.Name,
                        EntityType = DatabaseEntityType.PackageBody,
                        SourceCode = pack.Body
                    };
                    graph.Nodes.Add(bodyEntity);

                    // Add dependency from package body to package header
                    graph.Edges.Add(new EntityDependency
                    {
                        OwnerName = pack.SchemaOwner,
                        ObjectName = pack.Name,
                        ObjectType = DatabaseEntityType.PackageBody,
                        ReferencedOwner = pack.SchemaOwner,
                        ReferencedName = pack.Name,
                        ReferencedType = DatabaseEntityType.Package,
                        DependencyType = "HARD"
                    });
                }
            }

            // Add sequences as nodes
            foreach (var seq in schema.Sequences)
            {
                var entity = new DatabaseEntity
                {
                    SchemaOwner = seq.SchemaOwner,
                    Name = seq.Name,
                    EntityType = DatabaseEntityType.Sequence
                };
                graph.Nodes.Add(entity);
            }

            // Build edges from foreign keys (table dependencies)
            foreach (var table in schema.Tables)
            {
                foreach (var fk in table.ForeignKeys)
                {
                    if (!string.IsNullOrEmpty(fk.RefersToTable))
                    {
                        graph.Edges.Add(new EntityDependency
                        {
                            OwnerName = table.SchemaOwner,
                            ObjectName = table.Name,
                            ObjectType = DatabaseEntityType.Table,
                            ReferencedOwner = fk.RefersToSchema ?? table.SchemaOwner,
                            ReferencedName = fk.RefersToTable,
                            ReferencedType = DatabaseEntityType.Table,
                            DependencyType = "HARD"
                        });
                    }
                }

                // Add trigger dependencies
                foreach (var trigger in table.Triggers)
                {
                    graph.Edges.Add(new EntityDependency
                    {
                        OwnerName = trigger.SchemaOwner ?? table.SchemaOwner,
                        ObjectName = trigger.Name,
                        ObjectType = DatabaseEntityType.Trigger,
                        ReferencedOwner = table.SchemaOwner,
                        ReferencedName = table.Name,
                        ReferencedType = DatabaseEntityType.Table,
                        DependencyType = "HARD"
                    });
                }
            }

            // Analyze procedure source code for dependencies
            var analyzer = new ProcedureDependencyAnalyzer();
            foreach (var proc in schema.StoredProcedures)
            {
                if (!string.IsNullOrEmpty(proc.Sql))
                {
                    AddSourceCodeDependencies(graph, proc.SchemaOwner, proc.Name, 
                        DatabaseEntityType.Procedure, proc.Sql, analyzer);
                }
            }

            foreach (var func in schema.Functions)
            {
                if (!string.IsNullOrEmpty(func.Sql))
                {
                    AddSourceCodeDependencies(graph, func.SchemaOwner, func.Name, 
                        DatabaseEntityType.Function, func.Sql, analyzer);
                }
            }

            // Update entity dependencies lists
            UpdateEntityDependencyLists(graph);

            return graph;
        }

        /// <summary>
        /// Builds a dependency graph from a list of dependencies
        /// </summary>
        /// <param name="dependencies">The dependencies to build from</param>
        /// <returns>The dependency graph</returns>
        public DependencyGraph BuildFromDependencies(IEnumerable<EntityDependency> dependencies)
        {
            if (dependencies == null)
                throw new ArgumentNullException("dependencies");

            var graph = new DependencyGraph();
            var depList = dependencies.ToList();
            var nodeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dep in depList)
            {
                graph.Edges.Add(dep);

                // Add source node
                var sourceKey = GetNodeKey(dep.OwnerName, dep.ObjectName);
                if (!nodeKeys.Contains(sourceKey))
                {
                    nodeKeys.Add(sourceKey);
                    graph.Nodes.Add(new DatabaseEntity
                    {
                        SchemaOwner = dep.OwnerName,
                        Name = dep.ObjectName,
                        EntityType = dep.ObjectType
                    });
                }

                // Add target node
                var targetKey = GetNodeKey(dep.ReferencedOwner, dep.ReferencedName);
                if (!nodeKeys.Contains(targetKey))
                {
                    nodeKeys.Add(targetKey);
                    graph.Nodes.Add(new DatabaseEntity
                    {
                        SchemaOwner = dep.ReferencedOwner,
                        Name = dep.ReferencedName,
                        EntityType = dep.ReferencedType
                    });
                }
            }

            UpdateEntityDependencyLists(graph);
            return graph;
        }

        /// <summary>
        /// Merges source graph into target graph
        /// </summary>
        /// <param name="target">The target graph to merge into</param>
        /// <param name="source">The source graph to merge from</param>
        public void MergeGraphs(DependencyGraph target, DependencyGraph source)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (source == null)
                throw new ArgumentNullException("source");

            var existingNodes = new HashSet<string>(
                target.Nodes.Select(n => GetNodeKey(n.SchemaOwner, n.Name)),
                StringComparer.OrdinalIgnoreCase);

            // Add new nodes
            foreach (var node in source.Nodes)
            {
                var key = GetNodeKey(node.SchemaOwner, node.Name);
                if (!existingNodes.Contains(key))
                {
                    target.Nodes.Add(node);
                    existingNodes.Add(key);
                }
            }

            // Add all edges (may result in duplicates, but edges are lightweight)
            target.Edges.AddRange(source.Edges);

            // Update dependency lists
            UpdateEntityDependencyLists(target);
        }

        private void AddSourceCodeDependencies(
            DependencyGraph graph,
            string ownerName,
            string objectName,
            DatabaseEntityType objectType,
            string sourceCode,
            ProcedureDependencyAnalyzer analyzer)
        {
            // Extract table references
            var tables = analyzer.ExtractTableReferences(sourceCode);
            foreach (var table in tables)
            {
                var parts = table.Split('.');
                var refName = parts.Length > 1 ? parts[1] : parts[0];
                var refOwner = parts.Length > 1 ? parts[0] : ownerName;

                graph.Edges.Add(new EntityDependency
                {
                    OwnerName = ownerName,
                    ObjectName = objectName,
                    ObjectType = objectType,
                    ReferencedOwner = refOwner,
                    ReferencedName = refName,
                    ReferencedType = DatabaseEntityType.Table,
                    DependencyType = "REF"
                });
            }

            // Extract package calls
            var packages = analyzer.ExtractPackageCalls(sourceCode);
            foreach (var package in packages)
            {
                graph.Edges.Add(new EntityDependency
                {
                    OwnerName = ownerName,
                    ObjectName = objectName,
                    ObjectType = objectType,
                    ReferencedOwner = ownerName,
                    ReferencedName = package,
                    ReferencedType = DatabaseEntityType.Package,
                    DependencyType = "REF"
                });
            }
        }

        private void UpdateEntityDependencyLists(DependencyGraph graph)
        {
            // Clear existing lists
            foreach (var node in graph.Nodes)
            {
                node.Dependencies.Clear();
                node.ReferencedBy.Clear();
            }

            // Build lookup
            var nodeLookup = graph.Nodes.ToLookup(
                n => GetNodeKey(n.SchemaOwner, n.Name),
                StringComparer.OrdinalIgnoreCase);

            // Populate dependency lists
            foreach (var edge in graph.Edges)
            {
                var sourceKey = GetNodeKey(edge.OwnerName, edge.ObjectName);
                var sourceNodes = nodeLookup[sourceKey];

                foreach (var sourceNode in sourceNodes)
                {
                    sourceNode.Dependencies.Add(edge);
                }

                var targetKey = GetNodeKey(edge.ReferencedOwner, edge.ReferencedName);
                var targetNodes = nodeLookup[targetKey];

                foreach (var targetNode in targetNodes)
                {
                    targetNode.ReferencedBy.Add(edge);
                }
            }
        }

        private static string GetNodeKey(string owner, string name)
        {
            return string.Format("{0}.{1}", owner ?? "", name ?? "");
        }
    }
}
