using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Procedures
{
    /// <summary>
    /// Combines SQL catalog dependencies and source code analysis for comprehensive dependency detection
    /// </summary>
    public class CombinedDependencyAnalyzer
    {
        private readonly ProcedureDependencyResolver _resolver;
        private readonly ProcedureDependencyAnalyzer _sourceCodeAnalyzer;
        private readonly IDictionary<string, string> _procedureSources;

        /// <summary>
        /// Initializes a new instance of the <see cref="CombinedDependencyAnalyzer"/> class
        /// </summary>
        /// <param name="dependencies">Dependencies from ALL_DEPENDENCIES</param>
        /// <param name="procedureSources">Dictionary of procedure sources (key: schema.procedure_name, value: source code)</param>
        public CombinedDependencyAnalyzer(
            IList<EntityDependency> dependencies,
            IDictionary<string, string> procedureSources)
        {
            _resolver = new ProcedureDependencyResolver(dependencies ?? new List<EntityDependency>());
            _sourceCodeAnalyzer = new ProcedureDependencyAnalyzer();
            _procedureSources = procedureSources ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets comprehensive dependencies combining SQL catalog and source code analysis
        /// </summary>
        /// <param name="schemaOwner">Schema owner</param>
        /// <param name="procedureName">Procedure name</param>
        /// <returns>Comprehensive dependency result</returns>
        public ComprehensiveDependencyResult GetDependencies(string schemaOwner, string procedureName)
        {
            var result = new ComprehensiveDependencyResult
            {
                SchemaOwner = schemaOwner,
                ProcedureName = procedureName
            };

            // Get catalog dependencies
            var catalogResult = _resolver.GetTableDependencies(schemaOwner, procedureName);
            foreach (var table in catalogResult.DirectTableDependencies)
            {
                result.CatalogDependencies.Add(new DependencyInfo
                {
                    SchemaOwner = table.SchemaOwner,
                    ObjectName = table.TableName,
                    ObjectType = DatabaseEntityType.Table,
                    DependencyType = table.DependencyType,
                    Source = "Catalog"
                });
            }
            foreach (var view in catalogResult.ViewDependencies)
            {
                result.CatalogDependencies.Add(new DependencyInfo
                {
                    SchemaOwner = view.SchemaOwner,
                    ObjectName = view.ViewName,
                    ObjectType = DatabaseEntityType.View,
                    DependencyType = view.DependencyType,
                    Source = "Catalog"
                });
            }
            foreach (var table in catalogResult.IndirectTableDependencies)
            {
                result.CatalogDependencies.Add(new DependencyInfo
                {
                    SchemaOwner = table.SchemaOwner,
                    ObjectName = table.TableName,
                    ObjectType = DatabaseEntityType.Table,
                    DependencyType = table.DependencyType,
                    Source = "Catalog (via view)"
                });
            }

            // Get source code and analyze it
            var sourceKey = string.Format("{0}.{1}", schemaOwner, procedureName);
            string sourceCode;
            if (_procedureSources.TryGetValue(sourceKey, out sourceCode) && !string.IsNullOrEmpty(sourceCode))
            {
                result.SourceCodeAnalysis = _sourceCodeAnalyzer.AnalyzeSourceCode(sourceCode);
                result.ContainsDynamicSql = result.SourceCodeAnalysis.ContainsDynamicSql;

                if (result.ContainsDynamicSql)
                {
                    result.DynamicSqlWarning = "Warning: This procedure contains dynamic SQL (EXECUTE IMMEDIATE or DBMS_SQL). " +
                                               "Some dependencies may not be detectable through static analysis.";
                }

                // Add source code dependencies
                foreach (var table in result.SourceCodeAnalysis.AllTables)
                {
                    var parts = table.Split('.');
                    var tableOwner = parts.Length > 1 ? parts[0] : schemaOwner;
                    var tableName = parts.Length > 1 ? parts[1] : parts[0];

                    result.SourceCodeDependencies.Add(new DependencyInfo
                    {
                        SchemaOwner = tableOwner,
                        ObjectName = tableName,
                        ObjectType = DatabaseEntityType.Table,
                        DependencyType = "REF",
                        Source = "SourceCode"
                    });
                }

                // Add package calls from the already-analyzed source code
                foreach (var package in result.SourceCodeAnalysis.PackageCalls)
                {
                    result.SourceCodeDependencies.Add(new DependencyInfo
                    {
                        SchemaOwner = schemaOwner,
                        ObjectName = package,
                        ObjectType = DatabaseEntityType.Package,
                        DependencyType = "REF",
                        Source = "SourceCode"
                    });
                }

                // Find new dependencies (in source code but not in catalog)
                var catalogSet = new HashSet<string>(
                    result.CatalogDependencies.Select(d => d.FullName),
                    StringComparer.OrdinalIgnoreCase);

                foreach (var dep in result.SourceCodeDependencies)
                {
                    if (!catalogSet.Contains(dep.FullName))
                    {
                        result.NewDependenciesFromSourceCode.Add(new DependencyInfo
                        {
                            SchemaOwner = dep.SchemaOwner,
                            ObjectName = dep.ObjectName,
                            ObjectType = dep.ObjectType,
                            DependencyType = dep.DependencyType,
                            Source = "SourceCode (New)"
                        });
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Generates a comparison report between catalog and source code dependencies
        /// </summary>
        /// <param name="schemaOwner">Schema owner</param>
        /// <param name="procedureName">Procedure name</param>
        /// <returns>Comparison report as text</returns>
        public string GenerateComparisonReport(string schemaOwner, string procedureName)
        {
            var result = GetDependencies(schemaOwner, procedureName);
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("Dependency Comparison Report for {0}.{1}", schemaOwner, procedureName));
            sb.AppendLine(new string('=', 60));

            if (result.ContainsDynamicSql)
            {
                sb.AppendLine();
                sb.AppendLine("*** WARNING ***");
                sb.AppendLine(result.DynamicSqlWarning);
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine("CATALOG DEPENDENCIES (from ALL_DEPENDENCIES):");
            sb.AppendLine(new string('-', 40));
            if (result.CatalogDependencies.Any())
            {
                var groupedByType = result.CatalogDependencies.GroupBy(d => d.ObjectType);
                foreach (var group in groupedByType)
                {
                    sb.AppendLine(string.Format("  {0}s:", group.Key));
                    foreach (var dep in group)
                    {
                        sb.AppendLine(string.Format("    - {0} ({1})", dep.FullName, dep.Source));
                    }
                }
            }
            else
            {
                sb.AppendLine("  (none found)");
            }

            sb.AppendLine();
            sb.AppendLine("SOURCE CODE DEPENDENCIES:");
            sb.AppendLine(new string('-', 40));
            if (result.SourceCodeAnalysis != null)
            {
                if (result.SourceCodeAnalysis.ReadTables.Any())
                {
                    sb.AppendLine("  Read Tables (SELECT/FROM/JOIN):");
                    foreach (var table in result.SourceCodeAnalysis.ReadTables)
                    {
                        sb.AppendLine(string.Format("    - {0}", table));
                    }
                }

                if (result.SourceCodeAnalysis.InsertTables.Any())
                {
                    sb.AppendLine("  Insert Tables:");
                    foreach (var table in result.SourceCodeAnalysis.InsertTables)
                    {
                        sb.AppendLine(string.Format("    - {0}", table));
                    }
                }

                if (result.SourceCodeAnalysis.UpdateTables.Any())
                {
                    sb.AppendLine("  Update Tables:");
                    foreach (var table in result.SourceCodeAnalysis.UpdateTables)
                    {
                        sb.AppendLine(string.Format("    - {0}", table));
                    }
                }

                if (result.SourceCodeAnalysis.DeleteTables.Any())
                {
                    sb.AppendLine("  Delete Tables:");
                    foreach (var table in result.SourceCodeAnalysis.DeleteTables)
                    {
                        sb.AppendLine(string.Format("    - {0}", table));
                    }
                }

                if (result.SourceCodeAnalysis.MergeTables.Any())
                {
                    sb.AppendLine("  Merge Tables:");
                    foreach (var table in result.SourceCodeAnalysis.MergeTables)
                    {
                        sb.AppendLine(string.Format("    - {0}", table));
                    }
                }

                if (result.SourceCodeAnalysis.PackageCalls.Any())
                {
                    sb.AppendLine("  Package Calls:");
                    foreach (var package in result.SourceCodeAnalysis.PackageCalls)
                    {
                        sb.AppendLine(string.Format("    - {0}", package));
                    }
                }
            }
            else
            {
                sb.AppendLine("  (source code not available)");
            }

            sb.AppendLine();
            sb.AppendLine("NEW DEPENDENCIES (found only in source code):");
            sb.AppendLine(new string('-', 40));
            if (result.NewDependenciesFromSourceCode.Any())
            {
                foreach (var dep in result.NewDependenciesFromSourceCode)
                {
                    sb.AppendLine(string.Format("  - {0} ({1})", dep.FullName, dep.ObjectType));
                }
                sb.AppendLine();
                sb.AppendLine("Note: These dependencies were found in source code but not in ALL_DEPENDENCIES.");
                sb.AppendLine("This could indicate:");
                sb.AppendLine("  - Dynamic SQL usage");
                sb.AppendLine("  - Tables/views accessed via synonyms");
                sb.AppendLine("  - Objects in different schemas");
                sb.AppendLine("  - Invalid object references");
            }
            else
            {
                sb.AppendLine("  (none - catalog and source code match)");
            }

            sb.AppendLine();
            sb.AppendLine("SUMMARY:");
            sb.AppendLine(new string('-', 40));
            sb.AppendLine(string.Format("  Catalog dependencies: {0}", result.CatalogDependencies.Count));
            sb.AppendLine(string.Format("  Source code dependencies: {0}", result.SourceCodeDependencies.Count));
            sb.AppendLine(string.Format("  New from source code: {0}", result.NewDependenciesFromSourceCode.Count));
            sb.AppendLine(string.Format("  Contains dynamic SQL: {0}", result.ContainsDynamicSql ? "Yes" : "No"));

            return sb.ToString();
        }
    }
}
