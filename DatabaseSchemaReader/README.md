# DatabaseSchemaReader

A simple, cross-database facade over .Net DbProviderFactories to read database metadata.

* Source: https://github.com/martinjw/dbschemareader 
* GUI: https://github.com/martinjw/dbschemareader/releases
* Documentation: http://dbschemareader.codeplex.com/documentation

---

## Overview

Any ADO provider can be read (SqlServer, SqlServer CE 4, MySQL, SQLite, System.Data.OracleClient, ODP, Devart, PostgreSql, DB2...) into a single standard model.

Supported databases include SqlServer, SqlServer Ce, Oracle (via Microsoft, ODP and Devart), MySQL, SQLite, Postgresql, DB2, Ingres, VistaDb and Sybase ASE/ASA/UltraLite. For .net Core, we support SqlServer, SqlServer CE 4, SQLite, PostgreSql, MySQL and Oracle.

---

## Class Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│                              DatabaseSchemaReader (Core Library)                         │
└─────────────────────────────────────────────────────────────────────────────────────────┘
                                            │
                          ┌─────────────────┼─────────────────┐
                          │                 │                 │
                          ▼                 ▼                 ▼
              ┌───────────────────┐ ┌───────────────┐ ┌───────────────────┐
              │   DatabaseReader  │ │   SqlWriter   │ │    CodeWriter     │
              │  (Schema Reader)  │ │ (SQL Gen)     │ │  (Code Gen)       │
              └───────────────────┘ └───────────────┘ └───────────────────┘
                          │                 │                 │
                          ▼                 │                 │
              ┌───────────────────┐         │                 │
              │  DatabaseSchema   │◄────────┴─────────────────┘
              │ (Central Model)   │
              └───────────────────┘
                          │
      ┌───────────────────┼───────────────────────────────────────────┐
      │         │         │         │         │         │             │
      ▼         ▼         ▼         ▼         ▼         ▼             ▼
┌─────────┐┌─────────┐┌─────────┐┌─────────┐┌─────────┐┌─────────┐┌─────────┐
│Database ││Database ││Database ││Database ││Database ││Database ││Database │
│ Table   ││  View   ││ Stored  ││Function ││ Sequence││ Package ││ User    │
│         ││         ││Procedure││         ││         ││         ││         │
└─────────┘└─────────┘└─────────┘└─────────┘└─────────┘└─────────┘└─────────┘
      │
      ├───────────────────────────────────┐
      ▼                                   ▼
┌───────────────┐                 ┌───────────────┐
│DatabaseColumn │                 │DatabaseIndex  │
└───────────────┘                 └───────────────┘
      │                                   │
      │                                   │
      ▼                                   ▼
┌───────────────┐                 ┌───────────────┐
│DatabaseConstr.│                 │DatabaseTrigger│
└───────────────┘                 └───────────────┘
```

---

## Core Classes Description

### Main Classes

| Class | Namespace | Description |
|-------|-----------|-------------|
| **DatabaseReader** | `DatabaseSchemaReader` | Main entry point for reading database schema. Reads tables, views, stored procedures, functions, and all related metadata. |
| **DatabaseSchema** | `DatabaseSchemaReader.DataSchema` | Central container that holds all schema objects: tables, views, stored procedures, functions, packages, sequences, users. |
| **SqlWriter** | `DatabaseSchemaReader` | Generates SQL statements (SELECT, INSERT, UPDATE, DELETE) for tables. Supports multiple database dialects. |
| **CodeWriter** | `DatabaseSchemaReader.CodeGen` | Generates .NET code (POCO classes, NHibernate mappings, EF Code First) from schema. |
| **CompareSchemas** | `DatabaseSchemaReader.Compare` | Compares two database schemas and generates migration scripts. |

### DataSchema Classes

| Class | Description |
|-------|-------------|
| **DatabaseTable** | Represents a database table with columns, constraints, indexes, and triggers. |
| **DatabaseView** | Represents a database view. Inherits from DatabaseTable. Contains SQL definition. |
| **DatabaseColumn** | Represents a column with data type, length, precision, scale, nullable flag, primary/foreign key indicators. |
| **DatabaseConstraint** | Represents constraints: Primary Key, Foreign Key, Unique Key, Check, Default. |
| **DatabaseIndex** | Represents an index with indexed columns and uniqueness flag. |
| **DatabaseTrigger** | Represents a trigger with event type (INSERT/UPDATE/DELETE) and body. |
| **DatabaseStoredProcedure** | Represents a stored procedure with arguments and result sets. |
| **DatabaseFunction** | Represents a function (stored procedure that returns a value). Inherits from DatabaseStoredProcedure. |
| **DatabasePackage** | Represents an Oracle package containing procedures and functions. |
| **DatabaseSequence** | Represents a sequence with min/max values and increment. |
| **DatabaseArgument** | Represents an argument (IN/OUT parameter) to stored procedure or function. |
| **DataType** | Maps between database datatypes and .NET datatypes. |
| **DatabaseUser** | Represents a database user. |
| **DatabaseDbSchema** | Represents a database schema (namespace for objects). |

### Dependency Analysis Classes

| Class | Description |
|-------|-------------|
| **DatabaseEntity** | Unified entity for dependency analysis. Contains type, status, creation date, dependencies. |
| **EntityDependency** | Represents a dependency relationship between two database entities. |
| **DependencyGraph** | Graph structure for entity dependencies with methods to find circular dependencies. |
| **TableCluster** | Group of related tables connected by foreign keys. |
| **TableRelationships** | Parent and child table references for a specific table. |
| **TableReference** | FK-based reference to another table with column information. |

### Code Generation Classes

| Class | Description |
|-------|-------------|
| **CodeWriter** | Main class for generating code files from schema. |
| **CodeWriterSettings** | Settings for code generation: namespace, code target (POCO/NHibernate/EF). |
| **ClassWriter** | Writes individual class files. |
| **DataTypeWriter** | Converts database types to .NET types for code generation. |

### SQL Generation Classes

| Class | Description |
|-------|-------------|
| **DdlGeneratorFactory** | Factory for creating DDL generators for specific database types. |
| **MigrationGenerator** | Generates migration DDL statements (CREATE TABLE, ALTER TABLE, etc.). |
| **ProcedureGenerator** | Generates stored procedure DDL. |
| **TableGenerator** | Generates table DDL. |

### Comparison Classes

| Class | Description |
|-------|-------------|
| **CompareSchemas** | Compares two DatabaseSchema objects and generates migration script. |
| **CompareTables** | Compares tables between schemas. |
| **CompareColumns** | Compares columns between tables. |
| **CompareConstraints** | Compares constraints between tables. |
| **CompareIndexes** | Compares indexes between tables. |

---

## Usage Examples

### .NET Core/6/7/8 (netStandard1.5)

```c#
// Create the connection with the connection string
using (var connection = new SqlConnection(@"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind"))
{
    var dbReader = new DatabaseSchemaReader.DatabaseReader(connection);
    // Load the schema (this will take a little time on moderate to large database structures)
    var schema = dbReader.ReadAll();
    
    // Access tables
    foreach (var table in schema.Tables)
    {
        Console.WriteLine($"Table: {table.Name}");
        foreach (var column in table.Columns)
        {
            Console.WriteLine($"  Column: {column.Name} ({column.DbDataType})");
        }
    }
}
```

### Full .NET Framework (v3.5, v4.0, v4.5)

```c#
const string providerName = "System.Data.SqlClient";
const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";

// Create the database reader object
var dbReader = new DatabaseReader(connectionString, providerName);
// For Oracle, specify the Owner (Schema)
// dbReader.Owner = "HR";

// Load the schema
var schema = dbReader.ReadAll();

// The DatabaseSchema object has a collection of tables, views, stored procedures, functions, packages and datatypes
Console.WriteLine($"Tables: {schema.Tables.Count}");
Console.WriteLine($"Views: {schema.Views.Count}");
Console.WriteLine($"Stored Procedures: {schema.StoredProcedures.Count}");
```

### Reading Specific Table

```c#
var dbReader = new DatabaseReader(connectionString, providerName);

// Read only one table with all details
var ordersTable = dbReader.Table("ORDERS");

Console.WriteLine($"Table: {ordersTable.Name}");
Console.WriteLine($"Columns: {ordersTable.Columns.Count}");
Console.WriteLine($"Primary Key: {ordersTable.PrimaryKey?.Name}");
Console.WriteLine($"Foreign Keys: {ordersTable.ForeignKeys.Count}");
Console.WriteLine($"Indexes: {ordersTable.Indexes.Count}");
```

### Code Generation

```c#
const string providerName = "System.Data.SqlClient";
const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";

var reader = new DatabaseReader(connectionString, providerName);
var schema = reader.ReadAll();

// Generate POCO classes
var directory = new DirectoryInfo(Environment.CurrentDirectory);
var settings = new CodeWriterSettings
{
    Namespace = "Northwind.Domain",
    CodeTarget = CodeTarget.Poco // or CodeTarget.PocoNHibernateHbm, CodeTarget.PocoEntityCodeFirst
};
var codeWriter = new CodeWriter(schema, settings);
codeWriter.Execute(directory);
```

### SQL Generation

```c#
// Generate SQL for specific table
var sqlWriter = new SqlWriter(schema.FindTableByName("ORDERS"), SqlType.PostgreSql);

// Various SQL generation methods
var selectSql = sqlWriter.SelectAllSql();
var selectPageSql = sqlWriter.SelectPageStartToEndRowSql();
var insertSql = sqlWriter.InsertSql();
var updateSql = sqlWriter.UpdateSql();
var deleteSql = sqlWriter.DeleteSql();

// Script data INSERTs (not available in .net Core)
var sw = new DatabaseSchemaReader.Data.ScriptWriter { IncludeIdentity = true };
var inserts = sw.ReadTable("ORDERS", connectionString, providerName);
```

### Schema Comparison

```c#
// Load schemas from two databases
var acceptanceDb = new DatabaseReader(connectionString, providerName).ReadAll();
var developmentDb = new DatabaseReader(connectionString2, providerName).ReadAll();

// Compare and generate migration script
var comparison = new CompareSchemas(acceptanceDb, developmentDb);
var script = comparison.Execute();
// script contains DDL to upgrade acceptanceDb into the same schema as developmentDb
```

### Migrations (Low Level)

```c#
// Create a schema model programmatically
var dbSchema = new DatabaseSchema(null, SqlType.Oracle);
var table = dbSchema.AddTable("LOOKUP");
table.AddColumn<int>("Id").AddPrimaryKey().AddColumn<string>("Name").AddLength(30);
var newColumn = table.AddColumn("Updated", DbType.DateTime).AddNullable();

// Create a migration generator
var factory = new DatabaseSchemaReader.SqlGen.DdlGeneratorFactory(SqlType.Oracle);
var migrations = factory.MigrationGenerator();

// Turn the model into scripts
var tableScript = migrations.AddTable(table);
var columnScript = migrations.AddColumn(table, newColumn);
```

### Dependency Analysis

```c#
var reader = new DatabaseReader(connectionString, providerName);
var schema = reader.ReadAll();

// Build dependency graph
var graph = reader.BuildDependencyGraph();

// Find dependencies for a specific entity
var entity = graph.FindEntity("ORDERS");
var dependencies = graph.GetDependencies(entity);
var referencedBy = graph.GetReferencedBy(entity);

// Find circular dependencies
var circularDeps = graph.FindCircularDependencies();
```

### Table Relationship Analysis

```c#
var reader = new DatabaseReader(connectionString, providerName);
var schema = reader.ReadAll();

var analyzer = new TableRelationshipAnalyzer(schema);

// Get relationships for specific table
var relationships = analyzer.GetTableRelationships(ordersTable);
Console.WriteLine($"Parent tables: {relationships.ParentTables.Count}");
Console.WriteLine($"Child tables: {relationships.ChildTables.Count}");

// Get clusters of related tables
var clusters = analyzer.FindTableClusters();
foreach (var cluster in clusters)
{
    Console.WriteLine($"Cluster {cluster.ClusterId}: {cluster.TableCount} tables");
}

// Get statistics
var stats = analyzer.GetStatistics();
Console.WriteLine($"Total tables: {stats.TotalTables}");
Console.WriteLine($"Isolated tables: {stats.IsolatedTables}");
```

---

## Supported Database Providers

| Database | Provider Name | .NET Framework | .NET Core |
|----------|---------------|----------------|-----------|
| SQL Server | System.Data.SqlClient | ✓ | ✓ |
| SQL Server CE 4 | System.Data.SqlServerCe.4.0 | ✓ | ✓ |
| Oracle | System.Data.OracleClient | ✓ | - |
| Oracle ODP.NET | Oracle.ManagedDataAccess.Client | ✓ | ✓ |
| MySQL | MySql.Data.MySqlClient | ✓ | ✓ |
| PostgreSQL | Npgsql | ✓ | ✓ |
| SQLite | System.Data.SQLite | ✓ | ✓ |
| DB2 | IBM.Data.DB2 | ✓ | - |
| Firebird | FirebirdSql.Data.FirebirdClient | ✓ | - |

---

## Project Structure

```
DatabaseSchemaReader/
├── DatabaseReader.cs           # Main entry point
├── IDatabaseReader.cs          # Interface for DatabaseReader
├── SqlWriter.cs                # SQL statement generation
├── ReaderEventArgs.cs          # Event arguments for progress reporting
│
├── DataSchema/                 # Data model classes
│   ├── DatabaseSchema.cs       # Central schema container
│   ├── DatabaseTable.cs        # Table model
│   ├── DatabaseView.cs         # View model
│   ├── DatabaseColumn.cs       # Column model
│   ├── DatabaseConstraint.cs   # Constraint model
│   ├── DatabaseIndex.cs        # Index model
│   ├── DatabaseTrigger.cs      # Trigger model
│   ├── DatabaseStoredProcedure.cs # Stored procedure model
│   ├── DatabaseFunction.cs     # Function model
│   ├── DatabasePackage.cs      # Package model (Oracle)
│   ├── DatabaseSequence.cs     # Sequence model
│   ├── DatabaseArgument.cs     # Procedure argument model
│   ├── DataType.cs             # Data type mapping
│   ├── DatabaseEntity.cs       # Unified entity for dependencies
│   ├── EntityDependency.cs     # Dependency relationship
│   ├── DependencyGraph.cs      # Dependency graph structure
│   └── TableRelationshipModels.cs # Table relationship models
│
├── CodeGen/                    # Code generation
│   ├── CodeWriter.cs           # Main code writer
│   ├── CodeWriterSettings.cs   # Code generation settings
│   ├── ClassWriter.cs          # Individual class writer
│   ├── DataTypeWriter.cs       # Type conversion for code
│   ├── NHibernate/             # NHibernate mapping generation
│   ├── CodeFirst/              # EF Code First generation
│   └── Procedures/             # Stored procedure wrappers
│
├── SqlGen/                     # SQL generation
│   ├── DdlGeneratorFactory.cs  # DDL generator factory
│   ├── MigrationGenerator.cs   # Migration DDL generator
│   ├── SqlServer/              # SQL Server specific
│   ├── Oracle/                 # Oracle specific
│   ├── MySql/                  # MySQL specific
│   ├── PostgreSql/             # PostgreSQL specific
│   └── SqLite/                 # SQLite specific
│
├── Compare/                    # Schema comparison
│   ├── CompareSchemas.cs       # Main comparison class
│   ├── CompareTables.cs        # Table comparison
│   ├── CompareColumns.cs       # Column comparison
│   └── ...                     # Other comparison classes
│
├── Procedures/                 # Procedure analysis
│   ├── DependencyGraphBuilder.cs    # Builds dependency graphs
│   ├── TableRelationshipAnalyzer.cs # Analyzes table relationships
│   ├── CombinedDependencyAnalyzer.cs # Combined analysis
│   └── ProcedureDependencyAnalyzer.cs # Procedure dependencies
│
├── ProviderSchemaReaders/      # Database-specific readers
│   ├── Adapters/               # Provider adapters
│   └── Builders/               # Schema builders
│
└── Filters/                    # Schema filtering
    └── Exclusions.cs           # Exclusion rules
```

---

    Source: https://github.com/martinjw/dbschemareader or http://dbschemareader.codeplex.com/
    GUI: https://github.com/martinjw/dbschemareader/releases or http://dbschemareader.codeplex.com/releases
    Documentation: http://dbschemareader.codeplex.com/documentation

    ===General===

    A simple, cross-database facade over .Net 2.0 DbProviderFactories to read database metadata.

    Any ADO provider can be read  (SqlServer, SqlServer CE 4, MySQL, SQLite, System.Data.OracleClient, ODP, Devart, PostgreSql, DB2...) into a single standard model.

    Supported databases include SqlServer, SqlServer Ce, Oracle (via Microsoft, ODP and Devart), MySQL, SQLite, Postgresql, DB2, Ingres, VistaDb and Sybase ASE/ASA/UltraLite.  For .net Core, we support SqlServer, SqlServer CE 4, SQLite, PostgreSql, MySQL and Oracle (even if the database clients  are not yet available in .net Core, we are ready for them).

    ===Use===

    == Full .net framework (v3.5, v4.0, v4.5) ==

    To use it simply specify the connection string and ADO provider (eg System.Data,SqlClient or System.Data.OracleClient)

    const string providername = "System.Data.SqlClient";
    const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";
    //Create the database reader object.
    var dbReader = new DatabaseReader(connectionString, providername);
    //for Oracle, specify the Owner (Schema) as the full schema of an Oracle database is huge and will be very slow to load.
    //var dbReader = new DatabaseReader("Data Source=XE;User Id=hr;Password=hr;", "System.Data.OracleClient", "HR");
    //load the schema (this will take a little time on moderate to large database structures)
    var schema = dbReader.ReadAll();

    The DatabaseSchema object has a collection of tables, views, stored procedures, functions, packages and datatypes. Tables and views have columns, with their datatypes.

    == .net Core (netStandard1.5) ==

    //In .net Core, create the connection with the connection string
    using (var connection = new SqlConnection("Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind"))
    {
        var dr = new DatabaseSchemaReader.DatabaseReader(connection);
        //Then load the schema (this will take a little time on moderate to large database structures)
        var schema = dbReader.ReadAll();
    }

    ===Code generation===

    //first the standard schema reader
    const string providername = "System.Data.SqlClient";
    const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";
    var reader = new DatabaseReader(connectionString, providername);
    //for Oracle, specify dbReader.Owner = "MyOwner";
    //for .net Core, var reader = new DatabaseReader(new SqlConnection(connectionString));
    var schema = reader.ReadAll();

    //now write the code
    var directory = new DirectoryInfo(Environment.CurrentDirectory);
    var settings = new CodeWriterSettings
				       {
					       Namespace = "Northwind.Domain",
					       //CodeTarget = CodeTarget.Poco //default is POCO, or use EF Code First/NHibernate
				       };
    var codeWriter = new CodeWriter(schema, settings);
    codeWriter.Execute(directory);

    ===SQL generation===
    //Simple SQL
    var sqlWriter = new SqlWriter(schema.FindTableByName("ORDERS"), SqlType.PostgreSql);
    var selectSql = sqlWriter.SelectPageStartToEndRowSql(); //and others...

    //Script data INSERTs (not available in .net Core)
    var sw = new DatabaseSchemaReader.Data.ScriptWriter {IncludeIdentity = true};
    var inserts = sw.ReadTable("ORDERS", connectionString, providername);

    ===Comparisons===

    You can compare the schemas of two databases to get a diff script. 
    //load your schemas - nb .net Core requires ADO connection object
    var acceptanceDb = new DatabaseReader(connectionString, providername).ReadAll();
    var developmentDb = new DatabaseReader(connectionString2, providername).ReadAll();

    //compare
    var comparison = new CompareSchemas(acceptanceDb, developmentDb);
    var script = comparison.Execute(); //script to upgrade acceptanceDb into the same schema as developmentDb.

    ===Migrations (low level)===
    //create a schema model
    var dbSchema = new DatabaseSchema(null, SqlType.Oracle);
    var table = dbSchema.AddTable("LOOKUP");
    table.AddColumn<int>("Id").AddPrimaryKey().AddColumn<string>("Name").AddLength(30);
    var newColumn = table.AddColumn("Updated", DbType.DateTime).AddNullable();
    //create a migration generator
    var factory = new DatabaseSchemaReader.SqlGen.DdlGeneratorFactory(SqlType.Oracle);
    var migrations = factory.MigrationGenerator();
    //turn the model into scripts
    var tableScript = migrations.AddTable(table);
    var columnScript = migrations.AddColumn(table, newColumn);


## License

See repository `license.txt` and NuGet page for licensing and package details.