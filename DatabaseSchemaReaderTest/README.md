# DatabaseSchemaReaderTest

Comprehensive unit and integration tests for the DatabaseSchemaReader library (.NET Framework).

---

## Overview

This project contains the main test suite for the DatabaseSchemaReader library, covering all major functionality including schema reading, code generation, SQL generation, and schema comparison.

---

## Test Structure

```
DatabaseSchemaReaderTest/
├── Codegen/                    # Code generation tests
├── Compare/                    # Schema comparison tests
├── Conversion/                 # Data conversion tests
├── DataSchema/                 # Data model tests
├── Filters/                    # Filter functionality tests
├── IntegrationTests/           # Database integration tests
├── Procedures/                 # Stored procedure tests
├── ProviderSchemaReaders/      # Provider-specific tests
├── SqlGen/                     # SQL generation tests
├── Utilities/                  # Utility class tests
├── ConnectionStrings.cs        # Test connection strings
├── InitSQLite.cs              # SQLite test initialization
├── TestHelper.cs              # Common test helpers
└── DatabaseReaderTest.cs      # Main reader tests
```

---

## Test Categories

| Category | Description | Files |
|----------|-------------|-------|
| **Unit Tests** | Tests that don't require database | Most tests in DataSchema/, Codegen/, SqlGen/ |
| **Integration Tests** | Tests requiring database connection | IntegrationTests/ |
| **SqlServer** | SQL Server specific tests | Tests tagged with [SqlServer] |
| **Oracle** | Oracle specific tests | Tests tagged with [Oracle] |
| **MySql** | MySQL specific tests | Tests tagged with [MySql] |
| **PostgreSql** | PostgreSQL specific tests | Tests tagged with [PostgreSql] |
| **SQLite** | SQLite specific tests | Tests tagged with [SQLite] |

---

## Class Diagram

```
┌────────────────────────────────────────────────────────────────────────┐
│                     DatabaseSchemaReaderTest                            │
└────────────────────────────────────────────────────────────────────────┘
                                    │
        ┌───────────────────────────┼───────────────────────────┐
        │               │           │           │               │
        ▼               ▼           ▼           ▼               ▼
  ┌──────────┐   ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐
  │ DataSch. │   │ Codegen  │ │  SqlGen  │ │ Compare  │ │ Integration  │
  │  Tests   │   │  Tests   │ │  Tests   │ │  Tests   │ │    Tests     │
  └──────────┘   └──────────┘ └──────────┘ └──────────┘ └──────────────┘
        │               │           │           │               │
        └───────────────┴───────────┼───────────┴───────────────┘
                                    ▼
                        ┌───────────────────┐
                        │    TestHelper     │
                        │ (Common Utilities)│
                        └───────────────────┘
```

---

## Key Test Classes

### DatabaseReaderTest

Tests for the main `DatabaseReader` class:

```c#
[TestFixture]
public class DatabaseReaderTest
{
    [Test]
    public void ReadAllTables()
    {
        var reader = new DatabaseReader(ConnectionString, ProviderName);
        var schema = reader.ReadAll();
        
        Assert.That(schema.Tables, Is.Not.Empty);
    }
    
    [Test]
    public void ReadSingleTable()
    {
        var reader = new DatabaseReader(ConnectionString, ProviderName);
        var table = reader.Table("Customers");
        
        Assert.That(table, Is.Not.Null);
        Assert.That(table.Columns, Is.Not.Empty);
    }
}
```

### CodeGen Tests

Tests for code generation functionality:

```c#
[TestFixture]
public class ClassWriterTest
{
    [Test]
    public void GeneratePocoClass()
    {
        var table = CreateTestTable();
        var settings = new CodeWriterSettings { Namespace = "Test" };
        var classWriter = new ClassWriter(table, settings);
        
        var code = classWriter.Write();
        
        Assert.That(code, Contains.Substring("public class"));
    }
}
```

### SqlGen Tests

Tests for SQL generation:

```c#
[TestFixture]
public class SqlWriterTest
{
    [Test]
    public void GenerateSelectSql()
    {
        var table = CreateTestTable();
        var writer = new SqlWriter(table, SqlType.SqlServer);
        
        var sql = writer.SelectAllSql();
        
        Assert.That(sql, Contains.Substring("SELECT"));
        Assert.That(sql, Contains.Substring("FROM"));
    }
    
    [Test]
    public void GenerateInsertSql()
    {
        var table = CreateTestTable();
        var writer = new SqlWriter(table, SqlType.SqlServer);
        
        var sql = writer.InsertSql();
        
        Assert.That(sql, Contains.Substring("INSERT INTO"));
        Assert.That(sql, Contains.Substring("VALUES"));
    }
}
```

### Compare Tests

Tests for schema comparison:

```c#
[TestFixture]
public class CompareSchemasTest
{
    [Test]
    public void DetectAddedTable()
    {
        var baseSchema = CreateSchema();
        var targetSchema = CreateSchema();
        targetSchema.Tables.Add(new DatabaseTable { Name = "NewTable" });
        
        var comparison = new CompareSchemas(baseSchema, targetSchema);
        var script = comparison.Execute();
        
        Assert.That(script, Contains.Substring("CREATE TABLE"));
    }
}
```

---

## Running Tests

### Prerequisites

1. SQL Server Express with Northwind database (for SQL Server tests)
2. Other databases as needed (Oracle, MySQL, PostgreSQL)
3. Connection strings configured in `ConnectionStrings.cs`

### Using Visual Studio

1. Open `DatabaseSchemaReader.sln` in Visual Studio 2022
2. Build solution
3. Open Test Explorer (Test → Test Explorer)
4. Run all tests or select specific test categories

### Using MSBuild

```cmd
# Build solution
msbuild DatabaseSchemaReader.sln /t:Build

# Run tests with MSTest
vstest.console DatabaseSchemaReaderTest\bin\Debug\DatabaseSchemaReaderTest.dll
```

### Run Specific Categories

```cmd
# Run only unit tests (no database required)
vstest.console ... /TestCaseFilter:"TestCategory!=Integration"

# Run only SQL Server tests
vstest.console ... /TestCaseFilter:"TestCategory=SqlServer"
```

---

## Test Configuration

### ConnectionStrings.cs

```c#
public static class ConnectionStrings
{
    public const string SqlServer = 
        @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";
    
    public const string Oracle = 
        "Data Source=XE;User Id=hr;Password=hr;";
    
    public const string MySql = 
        "Server=localhost;Database=test;Uid=root;Pwd=password;";
    
    public const string PostgreSql = 
        "Host=localhost;Database=test;Username=postgres;Password=password";
    
    public const string SQLite = 
        "Data Source=:memory:";
}
```

### TestHelper.cs

```c#
public static class TestHelper
{
    public static DatabaseTable CreateTestTable()
    {
        var table = new DatabaseTable { Name = "TestTable" };
        table.Columns.Add(new DatabaseColumn 
        { 
            Name = "Id", 
            DbDataType = "INT", 
            IsPrimaryKey = true 
        });
        table.Columns.Add(new DatabaseColumn 
        { 
            Name = "Name", 
            DbDataType = "VARCHAR", 
            Length = 100 
        });
        return table;
    }
    
    public static DatabaseSchema CreateTestSchema()
    {
        var schema = new DatabaseSchema(null, SqlType.SqlServer);
        schema.Tables.Add(CreateTestTable());
        return schema;
    }
}
```

---

## Test Attributes

```c#
// Mark test category
[TestCategory("Integration")]
[TestCategory("SqlServer")]
public void MyIntegrationTest() { }

// Skip test conditionally
[Ignore("Requires database connection")]
public void MySkippedTest() { }

// Parameterized test
[TestCase(SqlType.SqlServer)]
[TestCase(SqlType.Oracle)]
[TestCase(SqlType.MySql)]
public void TestMultipleDatabases(SqlType sqlType) { }
```

---

## Common Test Patterns

### Creating Test Schema

```c#
private DatabaseSchema CreateTestSchema()
{
    var schema = new DatabaseSchema(null, SqlType.SqlServer);
    
    // Add table with columns
    var table = schema.AddTable("Orders");
    table.AddColumn<int>("Id").AddPrimaryKey();
    table.AddColumn<string>("CustomerName").AddLength(100);
    table.AddColumn<DateTime>("OrderDate");
    
    // Add foreign key table
    var detailTable = schema.AddTable("OrderDetails");
    detailTable.AddColumn<int>("Id").AddPrimaryKey();
    detailTable.AddColumn<int>("OrderId");
    
    // Add foreign key
    var fk = new DatabaseConstraint
    {
        Name = "FK_OrderDetails_Orders",
        ConstraintType = ConstraintType.ForeignKey,
        RefersToTable = "Orders"
    };
    fk.Columns.Add("OrderId");
    detailTable.AddConstraint(fk);
    
    return schema;
}
```

### Mocking Database Connection

```c#
[Test]
public void TestWithMockedConnection()
{
    // Use in-memory SQLite for quick tests
    using var connection = new SQLiteConnection("Data Source=:memory:");
    connection.Open();
    
    // Create test schema
    InitSQLite.CreateTestDatabase(connection);
    
    // Test
    var reader = new DatabaseReader(connection);
    var schema = reader.ReadAll();
    
    Assert.That(schema.Tables, Has.Count.GreaterThan(0));
}
```

---

## Dependencies

```xml
<PackageReference Include="NUnit" Version="..." />
<PackageReference Include="NUnit.ConsoleRunner" Version="..." />
<PackageReference Include="System.Data.SQLite" Version="..." />
```

---

## Troubleshooting

1. **Test fails with connection error**:
   - Check connection strings in `ConnectionStrings.cs`
   - Ensure database server is running
   - Check database exists

2. **Test timeout**:
   - Large databases may take longer
   - Consider using smaller test databases

3. **Missing test category**:
   - Ensure `TestCategoryAttribute.cs` is properly defined

---

## License

See repository `README.md` and `license.txt` for licensing details.
