# CoreTest

Integration and unit tests for the DatabaseSchemaReader library targeting .NET Core.

---

## Overview

This project contains tests specifically for .NET Core compatibility of the DatabaseSchemaReader library. It includes tests for various database providers and the new dependency analysis features.

---

## Test Classes

| Test Class | Description |
|------------|-------------|
| **TestMicrosoftSqlClient** | Tests for Microsoft.Data.SqlClient provider (SQL Server) |
| **TestMySql** | Tests for MySQL database reading |
| **TestOracle** | Tests for Oracle database reading using managed ODP.NET |
| **TestPostgreSql** | Tests for PostgreSQL database reading via Npgsql |
| **TestSqLite** | Tests for SQLite database reading |
| **CombinedDependencyAnalyzerTest** | Tests for combined dependency analysis functionality |
| **TableRelationshipAnalyzerTest** | Tests for table relationship analysis |
| **OracleDataExporterTest** | Tests for Oracle data export functionality |

---

## Class Diagram

```
┌────────────────────────────────────────────────────────────┐
│                        CoreTest                             │
└────────────────────────────────────────────────────────────┘
                              │
      ┌───────────────────────┼───────────────────────┐
      │           │           │           │           │
      ▼           ▼           ▼           ▼           ▼
┌──────────┐┌──────────┐┌──────────┐┌──────────┐┌──────────┐
│  Test    ││  Test    ││  Test    ││  Test    ││  Test    │
│SqlServer ││  MySql   ││  Oracle  ││PostgreSql││  SQLite  │
└──────────┘└──────────┘└──────────┘└──────────┘└──────────┘
      │           │           │           │           │
      └───────────┴───────────┼───────────┴───────────┘
                              ▼
                  ┌───────────────────┐
                  │  DatabaseReader   │
                  │  (Core Library)   │
                  └───────────────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
     ┌─────────────────┐ ┌─────────────┐ ┌─────────────────┐
     │ DatabaseSchema  │ │ Dependency  │ │ Table           │
     │                 │ │ Analysis    │ │ Relationships   │
     └─────────────────┘ └─────────────┘ └─────────────────┘
```

---

## Running Tests

### Prerequisites

1. Install required database drivers:
   - Microsoft.Data.SqlClient for SQL Server
   - MySql.Data for MySQL
   - Oracle.ManagedDataAccess.Core for Oracle
   - Npgsql for PostgreSQL
   - Microsoft.Data.Sqlite for SQLite

2. Configure test databases (modify connection strings as needed)

### Execute Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~TestSqLite"

# Run with verbose output
dotnet test -v detailed
```

---

## Test Examples

### TestSqLite

```c#
[TestFixture]
public class TestSqLite
{
    [Test]
    public void ReadSchema()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        
        // Create test table
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Email TEXT
            )";
        cmd.ExecuteNonQuery();
        
        // Read schema
        var reader = new DatabaseReader(connection);
        var schema = reader.ReadAll();
        
        Assert.That(schema.Tables, Has.Count.EqualTo(1));
        Assert.That(schema.Tables[0].Name, Is.EqualTo("Users"));
        Assert.That(schema.Tables[0].Columns, Has.Count.EqualTo(3));
    }
}
```

### TableRelationshipAnalyzerTest

```c#
[TestFixture]
public class TableRelationshipAnalyzerTest
{
    [Test]
    public void AnalyzeRelationships()
    {
        var schema = CreateTestSchema();
        var analyzer = new TableRelationshipAnalyzer(schema);
        
        var relationships = analyzer.GetTableRelationships(schema.Tables[0]);
        
        Assert.That(relationships.ParentTables, Has.Count.GreaterThan(0));
        Assert.That(relationships.ChildTables, Has.Count.GreaterThan(0));
    }
    
    [Test]
    public void FindClusters()
    {
        var schema = CreateTestSchema();
        var analyzer = new TableRelationshipAnalyzer(schema);
        
        var clusters = analyzer.FindTableClusters();
        
        Assert.That(clusters, Has.Count.GreaterThan(0));
    }
}
```

### CombinedDependencyAnalyzerTest

```c#
[TestFixture]
public class CombinedDependencyAnalyzerTest
{
    [Test]
    public void AnalyzeDependencies()
    {
        var dependencies = new List<EntityDependency>
        {
            new EntityDependency
            {
                OwnerName = "HR",
                ObjectName = "GET_EMPLOYEE",
                ObjectType = DatabaseEntityType.Procedure,
                ReferencedOwner = "HR",
                ReferencedName = "EMPLOYEES",
                ReferencedType = DatabaseEntityType.Table
            }
        };
        
        var sources = new Dictionary<string, string>
        {
            { "HR.GET_EMPLOYEE", "SELECT * FROM EMPLOYEES WHERE ID = p_id" }
        };
        
        var analyzer = new CombinedDependencyAnalyzer(dependencies, sources);
        var result = analyzer.AnalyzeProcedure("HR", "GET_EMPLOYEE");
        
        Assert.That(result.ReferencedTables, Contains.Item("EMPLOYEES"));
    }
}
```

---

## Test Database Setup

### SQLite (In-Memory)

```c#
// No setup required - uses in-memory database
using var connection = new SqliteConnection("Data Source=:memory:");
```

### SQL Server

```sql
-- Create test database
CREATE DATABASE TestDb;
USE TestDb;

CREATE TABLE TestTable (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100)
);
```

### PostgreSQL

```sql
-- Create test database
CREATE DATABASE testdb;

CREATE TABLE test_table (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100)
);
```

### MySQL

```sql
-- Create test database
CREATE DATABASE testdb;

CREATE TABLE test_table (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100)
);
```

### Oracle

```sql
-- Create test schema
CREATE TABLE test_table (
    id NUMBER GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    name VARCHAR2(100)
);
```

---

## Dependencies

```xml
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="..." />
<PackageReference Include="NUnit" Version="..." />
<PackageReference Include="NUnit3TestAdapter" Version="..." />
<PackageReference Include="Microsoft.Data.SqlClient" Version="..." />
<PackageReference Include="MySql.Data" Version="..." />
<PackageReference Include="Oracle.ManagedDataAccess.Core" Version="..." />
<PackageReference Include="Npgsql" Version="..." />
<PackageReference Include="Microsoft.Data.Sqlite" Version="..." />
```

---

## Build

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run tests
dotnet test
```

---

## License

See repository `README.md` and `license.txt` for licensing details.
