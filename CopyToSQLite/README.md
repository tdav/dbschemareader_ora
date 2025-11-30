# CopyToSQLite

A Windows Forms utility (net48) for copying database schema and data to SQLite or SQL Server CE 4 databases.

---

## Overview

This application reads all schema and data from a source database (using any ADO.NET provider) and creates a new SQLite or SQL Server CE 4 database with the same tables and data.

---

## Features

- Connect to any database via ADO.NET providers
- Read full database schema (tables, views, columns, keys, indexes)
- Create new SQLite database with same structure
- Create new SQL Server CE 4 database (if driver is installed)
- Copy all data from source to target database
- Progress reporting during copy operation

---

## Class Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        CopyToSQLite                          │
└─────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
     ┌─────────────────┐ ┌─────────────┐ ┌─────────────────────┐
     │    CopyForm     │ │   Runner    │ │      Program        │
     │ (Main UI Form)  │ │(Copy Logic) │ │ (Entry Point)       │
     └─────────────────┘ └─────────────┘ └─────────────────────┘
              │               │
              │               │
              ▼               ▼
     ┌────────────────────────────────────┐
     │         IDatabaseCreator           │
     │         (Interface)                │
     └────────────────────────────────────┘
              │                │
              ▼                ▼
┌─────────────────────┐ ┌────────────────────────────┐
│ DatabaseCreator     │ │ SqlServerCeDatabaseCreator │
│ (SQLite)            │ │ (SQL Server CE 4)          │
└─────────────────────┘ └────────────────────────────┘
              │                │
              ▼                ▼
┌─────────────────────┐ ┌─────────────────────┐
│ DatabaseInserter    │ │ SqlServerInserter   │
│ (SQLite Insert)     │ │ (SQL Server Insert) │
└─────────────────────┘ └─────────────────────┘
```

---

## Classes Description

| Class | Description |
|-------|-------------|
| **CopyForm** | Main Windows Forms UI. Allows user to select source database, connection string, and destination. Displays progress during copy operation. |
| **Runner** | Orchestrates the schema reading and copy process. Uses DatabaseSchemaReader to read source schema and coordinates creation of target database. |
| **IDatabaseCreator** | Interface for database creation. Defines methods for creating database structure from schema. |
| **DatabaseCreator** | Implements IDatabaseCreator for SQLite. Creates SQLite database file with tables matching source schema. |
| **SqlServerCeDatabaseCreator** | Implements IDatabaseCreator for SQL Server CE 4. Creates .sdf file with tables matching source schema. |
| **DatabaseInserter** | Copies data from source to SQLite database. Handles data type conversions. |
| **DatabaseInserterFactory** | Factory for creating appropriate inserter based on target database type. |
| **SqlServerInserter** | Copies data from source to SQL Server CE 4 database. |

---

## Usage

1. Launch the application
2. Select the source database provider (e.g., System.Data.SqlClient)
3. Enter the connection string to the source database
4. Click "Read Schema" to load the source database structure
5. Select destination database type (SQLite or SQL Server CE 4)
6. Click "Copy" to start the copy process
7. Wait for completion - progress is displayed in the status bar

---

## Example Code Flow

```c#
// Inside Runner class
public void CopyToSQLite(string sourceConnectionString, string sourceProvider, string destinationPath)
{
    // 1. Read source schema using DatabaseSchemaReader
    var reader = new DatabaseReader(sourceConnectionString, sourceProvider);
    var schema = reader.ReadAll();
    
    // 2. Create SQLite database
    var creator = new DatabaseCreator(destinationPath);
    creator.CreateDatabase(schema);
    
    // 3. Copy data
    var inserter = new DatabaseInserter(destinationPath);
    foreach (var table in schema.Tables)
    {
        inserter.InsertData(table, sourceConnectionString, sourceProvider);
    }
}
```

---

## Limitations

- **Data Type Limitations**: SQLite and SQL Server CE 4 do not have the full range of data types as other databases:
  - SQL Server CE 4 does not support VARCHAR(MAX)
  - SQLite has limited data types (TEXT, INTEGER, REAL, BLOB)
  - Some complex types may not convert properly

- **Foreign Key Constraints**: Copying data may violate foreign key constraints, especially for identity primary keys. Data copy order matters.

- **Binary/LOB Data**: Large binary objects may have size limitations in target databases.

- **Stored Procedures**: Stored procedures, functions, and triggers are not copied (SQLite has limited support, SQL Server CE has none).

---

## Requirements

- Windows with .NET Framework 4.8
- SQLite driver (System.Data.SQLite) - included
- SQL Server CE 4.0 (optional, for .sdf output) - must be installed separately

---

## Build

- Open `CopyToSQLite.csproj` in Visual Studio 2022
- Restore NuGet packages
- Build the solution (MSBuild or VS)

> Note: `dotnet build` does not support legacy TFMs in this solution; use Visual Studio/MSBuild.

---

## Dependencies

- **DatabaseSchemaReader** - Core library for reading database schema
- **System.Data.SQLite** - SQLite ADO.NET provider

---

## License

See repository `README.md` and `license.txt` for licensing details.
