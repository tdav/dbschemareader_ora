# CodeGenTester

Test harness for DatabaseSchemaReader's code generation functionality.

---

## Overview

CodeGenTester is a console application that tests the code generation capabilities of the DatabaseSchemaReader library. It reads from a SQL Express Northwind database, generates projects for 3 configurations (NHibernate hbm, NHibernate fluent, and EF Code First), and builds the assemblies to verify they compile correctly.

---

## Project Structure

```
CodeGenTester/
├── CodeGenTester/              # Main console app
│   ├── CodeGenTester.csproj
│   └── Program.cs
├── CodeGen.TestRunner/         # Test runner for generated code
│   ├── CodeGen.TestRunner.csproj
│   └── Program.cs
├── build.bat                   # Build script
├── build.proj                  # MSBuild project file
└── ReadMe.txt                  # Original readme
```

---

## Class Diagram

```
┌────────────────────────────────────────────────────────────┐
│                      CodeGenTester                          │
└────────────────────────────────────────────────────────────┘
                              │
                              ▼
                  ┌───────────────────┐
                  │   Program.cs      │
                  │ (Console Entry)   │
                  └───────────────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
     ┌─────────────────┐ ┌─────────────┐ ┌─────────────────┐
     │ DatabaseReader  │ │ CodeWriter  │ │ ProjectWriter   │
     │ (Read Schema)   │ │ (Generate)  │ │ (Create .csproj)│
     └─────────────────┘ └─────────────┘ └─────────────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
     ┌─────────────────┐ ┌─────────────┐ ┌─────────────────┐
     │  POCO Classes   │ │  NHibernate │ │   EF Code First │
     │  (.cs files)    │ │  Mappings   │ │   Mappings      │
     └─────────────────┘ └─────────────┘ └─────────────────┘
```

---

## How It Works

1. **CodeGenTester** (console application):
   - Reads schema from SqlExpress Northwind database
   - Generates C# projects for 3 configurations:
     - NHibernate HBM mapping
     - NHibernate Fluent mapping
     - EF Code First
   - Builds the generated assemblies

2. **CodeGen.TestRunner** (test console application):
   - Uses the 3 generated projects
   - Executes code against the database (reads Categories table)
   - Verifies that generated code works correctly

---

## Setup

1. Ensure you have a SQL Express Northwind database:
   ```sql
   -- Connection string format
   Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind
   ```

2. Update the destination path in `CodeGenTester\app.config`:
   ```xml
   <appSettings>
     <add key="Destination" value="C:\path\to\output"/>
   </appSettings>
   ```

3. Run the build script:
   ```cmd
   build.bat
   ```

---

## Generated Code Structure

The tool generates the following output:

```
Output/
├── NorthwindNHibernate/           # NHibernate HBM project
│   ├── NorthwindNHibernate.csproj
│   ├── Categories.cs
│   ├── Orders.cs
│   ├── ...
│   └── Mapping/
│       ├── Categories.hbm.xml
│       └── ...
│
├── NorthwindFluent/               # NHibernate Fluent project
│   ├── NorthwindFluent.csproj
│   ├── Categories.cs
│   ├── Orders.cs
│   └── Mapping/
│       ├── CategoriesMap.cs
│       └── ...
│
└── NorthwindEF/                   # EF Code First project
    ├── NorthwindEF.csproj
    ├── Categories.cs
    ├── Orders.cs
    └── NorthwindContext.cs        # DbContext class
```

---

## Example Output

```
Reading schema from Northwind...
Tables: 8
Views: 11
Stored Procedures: 7

Generating NHibernate HBM project...
Generated: Categories.cs
Generated: Categories.hbm.xml
...

Generating NHibernate Fluent project...
Generated: Categories.cs
Generated: CategoriesMap.cs
...

Generating EF Code First project...
Generated: Categories.cs
Generated: NorthwindContext.cs
...

Building projects...
NorthwindNHibernate: Success
NorthwindFluent: Success
NorthwindEF: Success

Running tests...
All tests passed.
```

---

## Configuration Options

### CodeWriterSettings

```c#
var settings = new CodeWriterSettings
{
    Namespace = "Northwind.Domain",
    
    // Code target options:
    CodeTarget = CodeTarget.Poco,                    // Plain POCO classes
    // CodeTarget = CodeTarget.PocoNHibernateHbm,    // POCO + NHibernate HBM mappings
    // CodeTarget = CodeTarget.PocoNHibernateFluent, // POCO + NHibernate Fluent mappings
    // CodeTarget = CodeTarget.PocoEntityCodeFirst,  // POCO + EF DbContext
    
    IncludeViews = true,
    UsePluralizingNamer = true,
    WriteStoredProcedures = false
};
```

---

## Requirements

- Windows with .NET Framework 4.8
- Visual Studio 2022 (for build)
- SQL Server Express with Northwind database
- NuGet packages (restored during build):
  - NHibernate
  - FluentNHibernate
  - EntityFramework

---

## Build Commands

```cmd
# Using build.bat
build.bat

# Using MSBuild directly
msbuild build.proj /t:Test

# Using Visual Studio
# Open CodeGenTester.sln and build
```

---

## Troubleshooting

1. **Database Connection Failed**:
   - Verify SQL Express is running
   - Check connection string in app.config
   - Ensure Northwind database exists

2. **Build Failed**:
   - Restore NuGet packages
   - Check .NET Framework version
   - Verify Visual Studio is installed

3. **Generated Code Compilation Errors**:
   - Check for reserved keywords in table/column names
   - Review data type mappings
   - Check foreign key references

---

## License

See repository `README.md` and `license.txt` for licensing details.
