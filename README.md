# DatabaseSchemaReader

A simple, cross-database facade over .Net DbProviderFactories to read database metadata.

Any ADO provider can be read (SqlServer, SqlServer CE 4, MySQL, SQLite, System.Data.OracleClient, ODP, Devart, PostgreSql, DB2...) into a single standard model. For .net Core, we support SqlServer, SqlServer CE 4, SQLite, PostgreSql, MySQL and Oracle.

* Source: https://github.com/martinjw/dbschemareader
* Documentation: https://dbschemareader.codeplex.com/documentation
* Nuget: Install-Package DatabaseSchemaReader [![Nuget](https://img.shields.io/nuget/v/DatabaseSchemaReader.svg)](https://www.nuget.org/packages/DatabaseSchemaReader/)
* [![Appveyor Build Status](https://ci.appveyor.com/api/projects/status/github/martinjw/dbschemareader?svg=true)](https://ci.appveyor.com/project/martinjw/dbschemareader)

---

## Features

* Database schema read from most ADO providers
* Simple .net code generation:
  * Generate POCO classes for tables, and NHibernate or EF Code First mapping files
  * Generate simple ADO classes to use stored procedures
* Simple sql generation:
  * Generate table DDL (and translate to another SQL syntax, eg SqlServer to Oracle or SQLite)
  * Generate CRUD stored procedures (for SqlServer, Oracle, MySQL, DB2)
* Copy a database schema and data from any provider (SqlServer, Oracle etc) to a new SQLite database (and, with limitations, to SqlServer CE 4)
* Compare two schemas to generate a migration script
* Simple cross-database migrations generator

---

## Solution Architecture

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                           DatabaseSchemaReader Solution                              │
└─────────────────────────────────────────────────────────────────────────────────────┘
                                        │
    ┌───────────────┬───────────────────┼───────────────────┬───────────────┐
    │               │                   │                   │               │
    ▼               ▼                   ▼                   ▼               ▼
┌─────────┐ ┌─────────────────┐ ┌─────────────────┐ ┌─────────────┐ ┌─────────────┐
│Database │ │DatabaseSchema   │ │  CopyToSQLite   │ │ CodeGen     │ │   CoreTest  │
│Schema   │ │    Viewer       │ │     (UI)        │ │   Tester    │ │   (Tests)   │
│Reader   │ │     (UI)        │ │                 │ │             │ │             │
│ (Core)  │ │                 │ │                 │ │             │ │             │
└─────────┘ └─────────────────┘ └─────────────────┘ └─────────────┘ └─────────────┘
    │               │                   │                   │               │
    │               │                   │                   │               │
    └───────────────┴───────────────────┴───────────────────┴───────────────┘
                                        │
                        ┌───────────────┼───────────────┐
                        │               │               │
                        ▼               ▼               ▼
                ┌─────────────┐ ┌─────────────┐ ┌─────────────────┐
                │  DataSchema │ │   CodeGen   │ │     SqlGen      │
                │   (Model)   │ │  (Generate) │ │   (SQL DDL)     │
                └─────────────┘ └─────────────┘ └─────────────────┘
```

---

## Projects Overview

| Project | Description | Target Framework | Documentation |
|---------|-------------|------------------|---------------|
| **DatabaseSchemaReader** | Core library for reading database schemas | Multi-target (.NET 3.5 - .NET 7) | [README](DatabaseSchemaReader/README.md) |
| **DatabaseSchemaViewer** | Windows Forms UI for viewing and analyzing schemas | .NET Framework 4.8 | [README](DatabaseSchemaViewer/README.md) |
| **CopyToSQLite** | Windows Forms UI for copying databases to SQLite | .NET Framework 4.8 | [README](CopyToSQLite/README.md) |
| **CodeGenTester** | Test harness for code generation | .NET Framework 4.8 | [README](CodeGenTester/README.md) |
| **CoreTest** | .NET Core integration tests | .NET Core 3.1+ | [README](CoreTest/README.md) |
| **DatabaseSchemaReaderTest** | Main test suite | .NET Framework 4.8 | [README](DatabaseSchemaReaderTest/README.md) |

---

## DataSchema Model (Comprehensive Diagram)

```
                           ┌─────────────────────────────────┐
                           │         DatabaseSchema          │
                           │  (Central Container for all     │
                           │   database objects)             │
                           └─────────────────────────────────┘
                                           │
     ┌─────────────┬─────────────┬─────────┼─────────┬─────────────┬─────────────┐
     │             │             │         │         │             │             │
     ▼             ▼             ▼         ▼         ▼             ▼             ▼
┌─────────┐ ┌─────────────┐ ┌─────────┐ ┌─────┐ ┌─────────┐ ┌─────────────┐ ┌─────────┐
│ Tables  │ │    Views    │ │ Stored  │ │Func-│ │Packages │ │  Sequences  │ │ Users   │
│ List    │ │    List     │ │Procedures││tions│ │  List   │ │    List     │ │  List   │
└────┬────┘ └──────┬──────┘ └────┬────┘ └──┬──┘ └────┬────┘ └─────────────┘ └─────────┘
     │             │             │         │         │
     ▼             ▼             │         │         │
┌─────────────────────────┐      │         │         │
│     DatabaseTable       │      │         │         │
├─────────────────────────┤      │         │         │
│ - Name                  │      │         │         │
│ - SchemaOwner           │      │         │         │
│ - Description           │      │         │         │
│ - NetName               │      │         │         │
└─────────────────────────┘      │         │         │
     │                           │         │         │
     │ ┌───────────────┐         │         │         │
     ├─┤   Columns     ├─────────┼─────────┘         │
     │ └───────┬───────┘         │                   │
     │         │                 │                   │
     │         ▼                 │                   │
     │ ┌─────────────────────────────────┐           │
     │ │      DatabaseColumn             │           │
     │ ├─────────────────────────────────┤           │
     │ │ - Name             - Nullable   │           │
     │ │ - DbDataType       - DefaultValue           │
     │ │ - Length           - Description│           │
     │ │ - Precision        - IsPrimaryKey           │
     │ │ - Scale            - IsForeignKey           │
     │ │ - Ordinal          - IsAutoNumber           │
     │ │ - IsIndexed        - IsUniqueKey│           │
     │ └─────────────────────────────────┘           │
     │                                               │
     │ ┌───────────────┐                             │
     ├─┤  PrimaryKey   │                             │
     │ └───────┬───────┘                             │
     │         │                                     │
     │ ┌───────────────┐                             │
     ├─┤  ForeignKeys  ├─────────────────────────────┤
     │ └───────┬───────┘                             │
     │         │                                     │
     │ ┌───────────────┐                             │
     ├─┤  UniqueKeys   │                             │
     │ └───────┬───────┘                             │
     │         │                                     │
     │         ▼                                     │
     │ ┌─────────────────────────────────┐           │
     │ │    DatabaseConstraint           │           │
     │ ├─────────────────────────────────┤           │
     │ │ - Name              - Columns   │           │
     │ │ - ConstraintType    - Expression│           │
     │ │ - TableName         - DeleteRule│           │
     │ │ - RefersToTable     - UpdateRule│           │
     │ │ - RefersToConstraint            │           │
     │ └─────────────────────────────────┘           │
     │                                               │
     │ ┌───────────────┐                             │
     ├─┤   Indexes     │                             │
     │ └───────┬───────┘                             │
     │         │                                     │
     │         ▼                                     │
     │ ┌─────────────────────────────────┐           │
     │ │      DatabaseIndex              │           │
     │ ├─────────────────────────────────┤           │
     │ │ - Name             - Columns    │           │
     │ │ - TableName        - IsUnique   │           │
     │ │ - IndexType        - Filter     │           │
     │ └─────────────────────────────────┘           │
     │                                               │
     │ ┌───────────────┐                             │
     └─┤   Triggers    │                             │
       └───────┬───────┘                             │
               │                                     │
               ▼                                     │
       ┌─────────────────────────────────┐           │
       │      DatabaseTrigger            │           │
       ├─────────────────────────────────┤           │
       │ - Name             - TriggerBody│           │
       │ - TableName        - TriggerType│           │
       │ - TriggerEvent                  │           │
       └─────────────────────────────────┘           │
                                                     │
┌────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────┐     ┌─────────────────────────────────┐
│     DatabaseStoredProcedure             │     │      DatabasePackage            │
├─────────────────────────────────────────┤     ├─────────────────────────────────┤
│ - Name             - Arguments          │     │ - Name            - Definition  │
│ - SchemaOwner      - ResultSets         │     │ - SchemaOwner     - Body        │
│ - Package          - Sql (source)       │     │ - StoredProcedures              │
│ - Language         - NetName            │     │ - Functions                     │
└─────────────────────────────────────────┘     └─────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────┐
│       DatabaseArgument                  │
├─────────────────────────────────────────┤
│ - Name             - In/Out/InOut       │
│ - DatabaseDataType - Ordinal            │
│ - Length           - Precision          │
│ - Scale            - ProcedureName      │
│ - PackageName                           │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│      DatabaseFunction                   │
│ (extends DatabaseStoredProcedure)       │
├─────────────────────────────────────────┤
│ - ReturnType                            │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│      DatabaseSequence                   │
├─────────────────────────────────────────┤
│ - Name             - MinimumValue       │
│ - SchemaOwner      - MaximumValue       │
│ - IncrementBy                           │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│          DataType                       │
├─────────────────────────────────────────┤
│ - TypeName          - NetDataType       │
│ - ProviderDbType    - CreateFormat      │
│ - LiteralPrefix     - LiteralSuffix     │
│ - IsString/IsInt/IsFloat/IsNumeric      │
└─────────────────────────────────────────┘
```

---

## Constraint Types

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           ConstraintType (Enum)                              │
├─────────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────┐│
│  │ PrimaryKey  │ │ ForeignKey  │ │  UniqueKey  │ │    Check    │ │ Default ││
│  │             │ │             │ │             │ │             │ │         ││
│  │ Ensures     │ │ References  │ │ Ensures     │ │ Validates   │ │ Sets    ││
│  │ uniqueness  │ │ another     │ │ uniqueness  │ │ data with   │ │ default ││
│  │ and non-null│ │ table's     │ │ (allows     │ │ expression  │ │ value   ││
│  │             │ │ primary key │ │ nulls)      │ │             │ │         ││
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘ └─────────┘│
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Dependency Analysis Model

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Dependency Analysis                                 │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
        ┌───────────────────────────┼───────────────────────────┐
        ▼                           ▼                           ▼
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│ DatabaseEntity  │         │ EntityDependency│         │ DependencyGraph │
├─────────────────┤         ├─────────────────┤         ├─────────────────┤
│ - Name          │         │ - OwnerName     │         │ - Nodes         │
│ - SchemaOwner   │         │ - ObjectName    │         │ - Edges         │
│ - EntityType    │         │ - ObjectType    │         │                 │
│ - Status        │◄────────│ - ReferencedOwner         │ Methods:        │
│ - Created       │         │ - ReferencedName│◄────────│ - GetDependencies│
│ - LastDdlTime   │         │ - ReferencedType│         │ - GetReferencedBy│
│ - Dependencies  │         │ - DependencyType│         │ - FindCircular  │
│ - ReferencedBy  │         └─────────────────┘         │   Dependencies  │
└─────────────────┘                                     │ - FindEntity    │
        │                                               └─────────────────┘
        │
        ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                        DatabaseEntityType (Enum)                             │
├─────────────────────────────────────────────────────────────────────────────┤
│ Table │ View │ Procedure │ Function │ Package │ Trigger │ Sequence │ Index  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Table Relationship Analysis

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      Table Relationship Analysis                             │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────┐     ┌─────────────────────┐     ┌─────────────────────┐
│   TableCluster      │     │ TableRelationships  │     │   TableReference    │
├─────────────────────┤     ├─────────────────────┤     ├─────────────────────┤
│ - ClusterId         │     │ - Table             │     │ - Table             │
│ - Name              │     │ - ParentTables      │────▶│ - ForeignKeyName    │
│ - Tables            │     │ - ChildTables       │     │ - ForeignKeyColumns │
│ - TableCount        │     │ - TotalRelationships│     │ - ReferencedColumns │
│ - RelationshipCount │     │ - IsIsolated        │     │ - ConstraintName    │
│ - CentralTable      │     └─────────────────────┘     └─────────────────────┘
└─────────────────────┘

Example:
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Orders Cluster                                     │
│  ┌──────────────┐      ┌──────────────┐      ┌──────────────┐               │
│  │  Customers   │◀────▶│    Orders    │◀────▶│  Products    │               │
│  │              │      │  (Central)   │      │              │               │
│  └──────────────┘      └──────────────┘      └──────────────┘               │
│         ▲                     │                     ▲                        │
│         │                     ▼                     │                        │
│         │              ┌──────────────┐             │                        │
│         └──────────────│ OrderDetails │─────────────┘                        │
│                        │              │                                      │
│                        └──────────────┘                                      │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Use

### Full .NET Framework (v3.5, v4.0, v4.5-v4.8)

```C#
// To use it simply specify the connection string and ADO provider
const string providername = "System.Data.SqlClient";
const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";

// Create the database reader object
var dbReader = new DatabaseReader(connectionString, providername);
// For Oracle, you should always specify the Owner (Schema)
// dbReader.Owner = "HR";

// Then load the schema (this will take a little time on moderate to large database structures)
var schema = dbReader.ReadAll();

// There are no datatables, and the structure is identical for all providers
foreach (var table in schema.Tables)
{
    Console.WriteLine($"Table: {table.Name}");
    
    // Access columns
    foreach (var column in table.Columns)
    {
        Console.WriteLine($"  Column: {column.Name} ({column.DbDataType})");
        Console.WriteLine($"    - Nullable: {column.Nullable}");
        Console.WriteLine($"    - Primary Key: {column.IsPrimaryKey}");
        Console.WriteLine($"    - Foreign Key: {column.IsForeignKey}");
    }
    
    // Access constraints
    if (table.PrimaryKey != null)
    {
        Console.WriteLine($"  Primary Key: {table.PrimaryKey.Name}");
    }
    
    foreach (var fk in table.ForeignKeys)
    {
        Console.WriteLine($"  Foreign Key: {fk.Name} -> {fk.RefersToTable}");
    }
    
    // Access indexes
    foreach (var index in table.Indexes)
    {
        Console.WriteLine($"  Index: {index.Name} (Unique: {index.IsUnique})");
    }
}
```

### .NET Core/6/7/8 (netStandard1.5, netStandard 2.0)

```C#
// In .net Core, create the connection with the connection string
using (var connection = new SqlConnection("Data Source=.\\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind"))
{
    var dbReader = new DatabaseSchemaReader.DatabaseReader(connection);
    // Then load the schema
    var schema = dbReader.ReadAll();

    // The structure is identical for all providers (and the full framework)
    foreach (var table in schema.Tables)
    {
        // do something with your model
    }
}
```

---

## UIs

There are two simple UIs (.net framework 4.8 only for now).

### DatabaseSchemaViewer

Reads all the schema and displays it in a treeview. It also includes options for:
- Code generation, table DDL and stored procedure generation
- Comparing the schema to another database
- Viewing table relationships and dependencies

See [DatabaseSchemaViewer/README.md](DatabaseSchemaViewer/README.md) for details.

### CopyToSQLite

Reads all the schema and creates a new SQLite database file with the same tables and data. If SQL Server CE 4.0 is detected, it can do the same for that database.

**Limitations:**
- These databases do not have the full range of data types as other databases, so creating tables may fail (e.g. SqlServer CE 4 does not have VARCHAR(MAX))
- Copying data may violate foreign key constraints (especially for identity primary keys) and will fail

See [CopyToSQLite/README.md](CopyToSQLite/README.md) for details.

---

## Building the Source

* Use Visual Studio **2022** to open `DatabaseSchemaReader.sln` (includes .net Core)
  * You can also use the command line `build.bat` (msbuild)
  * You **cannot** use the command line `dotnet build` because Core tooling cannot build v3.5 (see https://github.com/Microsoft/msbuild/issues/1333)

---

## License

See [license.txt](license.txt) for licensing details.
