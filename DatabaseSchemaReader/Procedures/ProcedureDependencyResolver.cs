using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Procedures
{
    /// <summary>
    /// Resolves procedure dependencies from ALL_DEPENDENCIES data
    /// </summary>
    public class ProcedureDependencyResolver
    {
        private readonly IList<EntityDependency> _dependencies;
        private readonly Dictionary<string, List<EntityDependency>> _dependencyIndex;
        private readonly Dictionary<string, List<EntityDependency>> _reverseDependencyIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcedureDependencyResolver"/> class
        /// </summary>
        /// <param name="dependencies">List of dependencies from ALL_DEPENDENCIES</param>
        public ProcedureDependencyResolver(IList<EntityDependency> dependencies)
        {
            _dependencies = dependencies ?? new List<EntityDependency>();
            _dependencyIndex = new Dictionary<string, List<EntityDependency>>(StringComparer.OrdinalIgnoreCase);
            _reverseDependencyIndex = new Dictionary<string, List<EntityDependency>>(StringComparer.OrdinalIgnoreCase);
            BuildIndex();
        }

        private void BuildIndex()
        {
            foreach (var dep in _dependencies)
            {
                // Forward index (object -> what it depends on)
                var sourceKey = GetKey(dep.OwnerName, dep.ObjectName);
                if (!_dependencyIndex.ContainsKey(sourceKey))
                {
                    _dependencyIndex[sourceKey] = new List<EntityDependency>();
                }
                _dependencyIndex[sourceKey].Add(dep);

                // Reverse index (object -> what depends on it)
                var targetKey = GetKey(dep.ReferencedOwner, dep.ReferencedName);
                if (!_reverseDependencyIndex.ContainsKey(targetKey))
                {
                    _reverseDependencyIndex[targetKey] = new List<EntityDependency>();
                }
                _reverseDependencyIndex[targetKey].Add(dep);
            }
        }

        /// <summary>
        /// Gets all table dependencies for a procedure, including tables accessed through views
        /// </summary>
        /// <param name="schemaOwner">Schema owner</param>
        /// <param name="procedureName">Procedure name</param>
        /// <returns>Procedure dependency result</returns>
        public ProcedureDependencyResult GetTableDependencies(string schemaOwner, string procedureName)
        {
            var result = new ProcedureDependencyResult
            {
                SchemaOwner = schemaOwner,
                ProcedureName = procedureName
            };

            var key = GetKey(schemaOwner, procedureName);
            if (!_dependencyIndex.ContainsKey(key))
            {
                return result;
            }

            var directDeps = _dependencyIndex[key];
            var processedViews = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dep in directDeps)
            {
                switch (dep.ReferencedType)
                {
                    case DatabaseEntityType.Table:
                        result.DirectTableDependencies.Add(new TableDependencyInfo
                        {
                            SchemaOwner = dep.ReferencedOwner,
                            TableName = dep.ReferencedName,
                            DependencyType = dep.DependencyType,
                            IsDirect = true
                        });
                        break;

                    case DatabaseEntityType.View:
                        var viewInfo = new ViewDependencyInfo
                        {
                            SchemaOwner = dep.ReferencedOwner,
                            ViewName = dep.ReferencedName,
                            DependencyType = dep.DependencyType
                        };
                        result.ViewDependencies.Add(viewInfo);

                        // Get tables from view recursively
                        var viewKey = GetKey(dep.ReferencedOwner, dep.ReferencedName);
                        if (!processedViews.Contains(viewKey))
                        {
                            processedViews.Add(viewKey);
                            var viewTables = GetTablesFromView(dep.ReferencedOwner, dep.ReferencedName, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                            viewInfo.UnderlyingTables.AddRange(viewTables);

                            foreach (var tableName in viewTables)
                            {
                                var parts = tableName.Split('.');
                                result.IndirectTableDependencies.Add(new TableDependencyInfo
                                {
                                    SchemaOwner = parts.Length > 1 ? parts[0] : dep.ReferencedOwner,
                                    TableName = parts.Length > 1 ? parts[1] : parts[0],
                                    DependencyType = "REF",
                                    IsDirect = false,
                                    ThroughView = viewInfo.FullName
                                });
                            }
                        }
                        break;

                    case DatabaseEntityType.Procedure:
                        result.ProcedureDependencies.Add(GetKey(dep.ReferencedOwner, dep.ReferencedName));
                        break;

                    case DatabaseEntityType.Function:
                        result.FunctionDependencies.Add(GetKey(dep.ReferencedOwner, dep.ReferencedName));
                        break;

                    case DatabaseEntityType.Package:
                        result.PackageDependencies.Add(GetKey(dep.ReferencedOwner, dep.ReferencedName));
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the full dependency tree for a procedure
        /// </summary>
        /// <param name="schemaOwner">Schema owner</param>
        /// <param name="procedureName">Procedure name</param>
        /// <returns>Dependency tree</returns>
        public DependencyTree GetFullDependencyTree(string schemaOwner, string procedureName)
        {
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            return BuildDependencyTree(schemaOwner, procedureName, DatabaseEntityType.Procedure, 0, visited);
        }

        private DependencyTree BuildDependencyTree(string schemaOwner, string objectName, 
            DatabaseEntityType objectType, int level, HashSet<string> visited)
        {
            var tree = new DependencyTree
            {
                SchemaOwner = schemaOwner,
                ObjectName = objectName,
                ObjectType = objectType,
                Level = level
            };

            var key = GetKey(schemaOwner, objectName);
            if (visited.Contains(key))
            {
                return tree; // Prevent infinite recursion
            }
            visited.Add(key);

            if (!_dependencyIndex.ContainsKey(key))
            {
                return tree;
            }

            foreach (var dep in _dependencyIndex[key])
            {
                var childTree = BuildDependencyTree(
                    dep.ReferencedOwner, 
                    dep.ReferencedName, 
                    dep.ReferencedType, 
                    level + 1, 
                    visited);
                childTree.DependencyType = dep.DependencyType;
                tree.Children.Add(childTree);
            }

            return tree;
        }

        /// <summary>
        /// Finds all procedures that depend on a specific table
        /// </summary>
        /// <param name="schemaOwner">Schema owner</param>
        /// <param name="tableName">Table name</param>
        /// <returns>List of procedure names (schema.procedure)</returns>
        public List<string> FindProceduresByTable(string schemaOwner, string tableName)
        {
            var procedures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var key = GetKey(schemaOwner, tableName);

            if (_reverseDependencyIndex.ContainsKey(key))
            {
                foreach (var dep in _reverseDependencyIndex[key])
                {
                    if (dep.ObjectType == DatabaseEntityType.Procedure ||
                        dep.ObjectType == DatabaseEntityType.Function ||
                        dep.ObjectType == DatabaseEntityType.Package ||
                        dep.ObjectType == DatabaseEntityType.PackageBody)
                    {
                        procedures.Add(GetKey(dep.OwnerName, dep.ObjectName));
                    }
                    else if (dep.ObjectType == DatabaseEntityType.View)
                    {
                        // Also find procedures that use this view
                        var viewProcedures = FindProceduresByTable(dep.OwnerName, dep.ObjectName);
                        foreach (var proc in viewProcedures)
                        {
                            procedures.Add(proc);
                        }
                    }
                }
            }

            return procedures.ToList();
        }

        /// <summary>
        /// Gets a dependency summary as text
        /// </summary>
        /// <param name="schemaOwner">Schema owner</param>
        /// <param name="procedureName">Procedure name</param>
        /// <returns>Text summary</returns>
        public string GetDependencySummary(string schemaOwner, string procedureName)
        {
            var sb = new StringBuilder();
            var result = GetTableDependencies(schemaOwner, procedureName);

            sb.AppendLine(string.Format("Dependency Summary for {0}.{1}", schemaOwner, procedureName));
            sb.AppendLine(new string('=', 50));

            sb.AppendLine();
            sb.AppendLine("Direct Table Dependencies:");
            if (result.DirectTableDependencies.Any())
            {
                foreach (var table in result.DirectTableDependencies)
                {
                    sb.AppendLine(string.Format("  - {0} ({1})", table.FullName, table.DependencyType));
                }
            }
            else
            {
                sb.AppendLine("  (none)");
            }

            sb.AppendLine();
            sb.AppendLine("View Dependencies:");
            if (result.ViewDependencies.Any())
            {
                foreach (var view in result.ViewDependencies)
                {
                    sb.AppendLine(string.Format("  - {0} ({1})", view.FullName, view.DependencyType));
                    if (view.UnderlyingTables.Any())
                    {
                        sb.AppendLine("    Underlying tables:");
                        foreach (var table in view.UnderlyingTables)
                        {
                            sb.AppendLine(string.Format("      - {0}", table));
                        }
                    }
                }
            }
            else
            {
                sb.AppendLine("  (none)");
            }

            sb.AppendLine();
            sb.AppendLine("Indirect Table Dependencies (via views):");
            if (result.IndirectTableDependencies.Any())
            {
                foreach (var table in result.IndirectTableDependencies)
                {
                    sb.AppendLine(string.Format("  - {0} (via {1})", table.FullName, table.ThroughView));
                }
            }
            else
            {
                sb.AppendLine("  (none)");
            }

            sb.AppendLine();
            sb.AppendLine("Procedure Dependencies:");
            if (result.ProcedureDependencies.Any())
            {
                foreach (var proc in result.ProcedureDependencies)
                {
                    sb.AppendLine(string.Format("  - {0}", proc));
                }
            }
            else
            {
                sb.AppendLine("  (none)");
            }

            sb.AppendLine();
            sb.AppendLine("Package Dependencies:");
            if (result.PackageDependencies.Any())
            {
                foreach (var pack in result.PackageDependencies)
                {
                    sb.AppendLine(string.Format("  - {0}", pack));
                }
            }
            else
            {
                sb.AppendLine("  (none)");
            }

            return sb.ToString();
        }

        private List<string> GetTablesFromView(string schemaOwner, string viewName, HashSet<string> visited)
        {
            var tables = new List<string>();
            var key = GetKey(schemaOwner, viewName);

            if (visited.Contains(key))
            {
                return tables;
            }
            visited.Add(key);

            if (!_dependencyIndex.ContainsKey(key))
            {
                return tables;
            }

            foreach (var dep in _dependencyIndex[key])
            {
                if (dep.ReferencedType == DatabaseEntityType.Table)
                {
                    tables.Add(GetKey(dep.ReferencedOwner, dep.ReferencedName));
                }
                else if (dep.ReferencedType == DatabaseEntityType.View)
                {
                    // Recursively get tables from nested views
                    var nestedTables = GetTablesFromView(dep.ReferencedOwner, dep.ReferencedName, visited);
                    tables.AddRange(nestedTables);
                }
            }

            return tables;
        }

        private static string GetKey(string owner, string name)
        {
            return string.Format("{0}.{1}", owner ?? "", name ?? "");
        }
    }
}
