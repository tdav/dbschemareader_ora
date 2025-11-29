using System;
using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Procedures
{
    /// <summary>
    /// Result of source code dependency analysis
    /// </summary>
    [Serializable]
    public class SourceCodeDependencyResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceCodeDependencyResult"/> class
        /// </summary>
        public SourceCodeDependencyResult()
        {
            ReadTables = new List<string>();
            InsertTables = new List<string>();
            UpdateTables = new List<string>();
            DeleteTables = new List<string>();
            MergeTables = new List<string>();
            PackageCalls = new List<string>();
        }

        /// <summary>
        /// Tables read from (SELECT, FROM, JOIN)
        /// </summary>
        public List<string> ReadTables { get; set; }

        /// <summary>
        /// Tables inserted into (INSERT INTO)
        /// </summary>
        public List<string> InsertTables { get; set; }

        /// <summary>
        /// Tables updated (UPDATE)
        /// </summary>
        public List<string> UpdateTables { get; set; }

        /// <summary>
        /// Tables deleted from (DELETE FROM)
        /// </summary>
        public List<string> DeleteTables { get; set; }

        /// <summary>
        /// Tables merged into (MERGE INTO)
        /// </summary>
        public List<string> MergeTables { get; set; }

        /// <summary>
        /// Package calls found in source code
        /// </summary>
        public List<string> PackageCalls { get; set; }

        /// <summary>
        /// Whether the source code contains dynamic SQL
        /// </summary>
        public bool ContainsDynamicSql { get; set; }

        /// <summary>
        /// Gets all unique tables from all categories
        /// </summary>
        public List<string> AllTables
        {
            get
            {
                var allTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var t in ReadTables) allTables.Add(t);
                foreach (var t in InsertTables) allTables.Add(t);
                foreach (var t in UpdateTables) allTables.Add(t);
                foreach (var t in DeleteTables) allTables.Add(t);
                foreach (var t in MergeTables) allTables.Add(t);
                return new List<string>(allTables);
            }
        }
    }

    /// <summary>
    /// Result of procedure dependency resolution
    /// </summary>
    [Serializable]
    public class ProcedureDependencyResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcedureDependencyResult"/> class
        /// </summary>
        public ProcedureDependencyResult()
        {
            DirectTableDependencies = new List<TableDependencyInfo>();
            ViewDependencies = new List<ViewDependencyInfo>();
            IndirectTableDependencies = new List<TableDependencyInfo>();
            ProcedureDependencies = new List<string>();
            FunctionDependencies = new List<string>();
            PackageDependencies = new List<string>();
        }

        /// <summary>
        /// Schema owner of the procedure
        /// </summary>
        public string SchemaOwner { get; set; }

        /// <summary>
        /// Name of the procedure
        /// </summary>
        public string ProcedureName { get; set; }

        /// <summary>
        /// Direct table dependencies (procedure -> table)
        /// </summary>
        public List<TableDependencyInfo> DirectTableDependencies { get; set; }

        /// <summary>
        /// View dependencies (procedure -> view)
        /// </summary>
        public List<ViewDependencyInfo> ViewDependencies { get; set; }

        /// <summary>
        /// Indirect table dependencies (procedure -> view -> table)
        /// </summary>
        public List<TableDependencyInfo> IndirectTableDependencies { get; set; }

        /// <summary>
        /// Procedure dependencies
        /// </summary>
        public List<string> ProcedureDependencies { get; set; }

        /// <summary>
        /// Function dependencies
        /// </summary>
        public List<string> FunctionDependencies { get; set; }

        /// <summary>
        /// Package dependencies
        /// </summary>
        public List<string> PackageDependencies { get; set; }

        /// <summary>
        /// Gets all table dependencies (direct and indirect)
        /// </summary>
        public List<TableDependencyInfo> AllTableDependencies
        {
            get
            {
                var all = new List<TableDependencyInfo>();
                all.AddRange(DirectTableDependencies);
                all.AddRange(IndirectTableDependencies);
                return all;
            }
        }
    }

    /// <summary>
    /// Information about a table dependency
    /// </summary>
    [Serializable]
    public class TableDependencyInfo
    {
        /// <summary>
        /// Schema owner of the table
        /// </summary>
        public string SchemaOwner { get; set; }

        /// <summary>
        /// Table name
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Type of dependency (HARD, REF)
        /// </summary>
        public string DependencyType { get; set; }

        /// <summary>
        /// Whether the dependency is direct or through a view
        /// </summary>
        public bool IsDirect { get; set; }

        /// <summary>
        /// If indirect, the view through which this table is accessed
        /// </summary>
        public string ThroughView { get; set; }

        /// <summary>
        /// Full name of the table (schema.table)
        /// </summary>
        public string FullName
        {
            get
            {
                return string.IsNullOrEmpty(SchemaOwner) 
                    ? TableName 
                    : string.Format("{0}.{1}", SchemaOwner, TableName);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return IsDirect 
                ? FullName 
                : string.Format("{0} (via {1})", FullName, ThroughView);
        }
    }

    /// <summary>
    /// Information about a view dependency
    /// </summary>
    [Serializable]
    public class ViewDependencyInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewDependencyInfo"/> class
        /// </summary>
        public ViewDependencyInfo()
        {
            UnderlyingTables = new List<string>();
        }

        /// <summary>
        /// Schema owner of the view
        /// </summary>
        public string SchemaOwner { get; set; }

        /// <summary>
        /// View name
        /// </summary>
        public string ViewName { get; set; }

        /// <summary>
        /// Type of dependency (HARD, REF)
        /// </summary>
        public string DependencyType { get; set; }

        /// <summary>
        /// List of underlying tables that this view references
        /// </summary>
        public List<string> UnderlyingTables { get; set; }

        /// <summary>
        /// Full name of the view (schema.view)
        /// </summary>
        public string FullName
        {
            get
            {
                return string.IsNullOrEmpty(SchemaOwner) 
                    ? ViewName 
                    : string.Format("{0}.{1}", SchemaOwner, ViewName);
            }
        }
    }

    /// <summary>
    /// Represents a dependency tree for a database object
    /// </summary>
    [Serializable]
    public class DependencyTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyTree"/> class
        /// </summary>
        public DependencyTree()
        {
            Children = new List<DependencyTree>();
        }

        /// <summary>
        /// Schema owner
        /// </summary>
        public string SchemaOwner { get; set; }

        /// <summary>
        /// Object name
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// Object type
        /// </summary>
        public DatabaseEntityType ObjectType { get; set; }

        /// <summary>
        /// Dependency type
        /// </summary>
        public string DependencyType { get; set; }

        /// <summary>
        /// Child dependencies
        /// </summary>
        public List<DependencyTree> Children { get; set; }

        /// <summary>
        /// Depth level in the tree
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Full name of the object (schema.name)
        /// </summary>
        public string FullName
        {
            get
            {
                return string.IsNullOrEmpty(SchemaOwner) 
                    ? ObjectName 
                    : string.Format("{0}.{1}", SchemaOwner, ObjectName);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{0} ({1})", FullName, ObjectType);
        }
    }

    /// <summary>
    /// Comprehensive dependency result combining SQL catalog and source code analysis
    /// </summary>
    [Serializable]
    public class ComprehensiveDependencyResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComprehensiveDependencyResult"/> class
        /// </summary>
        public ComprehensiveDependencyResult()
        {
            CatalogDependencies = new List<DependencyInfo>();
            SourceCodeDependencies = new List<DependencyInfo>();
            NewDependenciesFromSourceCode = new List<DependencyInfo>();
        }

        /// <summary>
        /// Schema owner
        /// </summary>
        public string SchemaOwner { get; set; }

        /// <summary>
        /// Procedure name
        /// </summary>
        public string ProcedureName { get; set; }

        /// <summary>
        /// Dependencies found in SQL catalog (ALL_DEPENDENCIES)
        /// </summary>
        public List<DependencyInfo> CatalogDependencies { get; set; }

        /// <summary>
        /// Dependencies found in source code analysis
        /// </summary>
        public List<DependencyInfo> SourceCodeDependencies { get; set; }

        /// <summary>
        /// Dependencies found only in source code (not in catalog)
        /// </summary>
        public List<DependencyInfo> NewDependenciesFromSourceCode { get; set; }

        /// <summary>
        /// Whether the source code contains dynamic SQL
        /// </summary>
        public bool ContainsDynamicSql { get; set; }

        /// <summary>
        /// Warning message if dynamic SQL is present
        /// </summary>
        public string DynamicSqlWarning { get; set; }

        /// <summary>
        /// Source code dependency analysis result
        /// </summary>
        public SourceCodeDependencyResult SourceCodeAnalysis { get; set; }
    }

    /// <summary>
    /// Basic dependency information
    /// </summary>
    [Serializable]
    public class DependencyInfo
    {
        /// <summary>
        /// Schema owner
        /// </summary>
        public string SchemaOwner { get; set; }

        /// <summary>
        /// Object name
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// Object type
        /// </summary>
        public DatabaseEntityType ObjectType { get; set; }

        /// <summary>
        /// Dependency type (HARD, REF)
        /// </summary>
        public string DependencyType { get; set; }

        /// <summary>
        /// Source of this dependency (Catalog, SourceCode, Both)
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Full name of the object (schema.name)
        /// </summary>
        public string FullName
        {
            get
            {
                return string.IsNullOrEmpty(SchemaOwner) 
                    ? ObjectName 
                    : string.Format("{0}.{1}", SchemaOwner, ObjectName);
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{0} ({1}) - {2}", FullName, ObjectType, Source);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            var other = obj as DependencyInfo;
            if (other == null) return false;
            return string.Equals(SchemaOwner, other.SchemaOwner, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(ObjectName, other.ObjectName, StringComparison.OrdinalIgnoreCase) &&
                   ObjectType == other.ObjectType;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (SchemaOwner?.ToUpperInvariant().GetHashCode() ?? 0);
                hash = hash * 23 + (ObjectName?.ToUpperInvariant().GetHashCode() ?? 0);
                hash = hash * 23 + ObjectType.GetHashCode();
                return hash;
            }
        }
    }
}
