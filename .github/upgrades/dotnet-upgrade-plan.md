# .NET 10.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 10.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10.0 upgrade.
3. Upgrade DatabaseSchemaReader\DatabaseSchemaReader.csproj
4. Upgrade CoreTest\CoreTest.csproj
5. Upgrade DatabaseSchemaReaderTest\DatabaseSchemaReaderTest.csproj
6. Upgrade DatabaseSchemaViewer\DatabaseSchemaViewer.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                               | Current Version        | New Version | Description                                        |
|:-------------------------------------------|:----------------------:|:-----------:|:---------------------------------------------------|
| Azure.Identity                             | 1.15.0                 | 1.17.1      | Current version depends on deprecated MSAL version |
| Microsoft.Bcl.AsyncInterfaces              | 8.0.0                  | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.Bcl.Cryptography                 | 8.0.0                  | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.Bcl.TimeProvider                 | 8.0.1                  | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.Data.Sqlite                      | 9.0.8                  | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.Extensions.Caching.Abstractions  | 8.0.0                  | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.Extensions.Caching.Memory        | 8.0.1                  | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.Extensions.DependencyInjection.Abstractions | 8.0.2        | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.Extensions.Logging.Abstractions  | 8.0.3                  | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.Extensions.Options               | 8.0.2                  | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.Extensions.Primitives            | 8.0.0                  | 10.0.0      | Recommended for .NET 10.0                          |
| Microsoft.Identity.Client                  | 4.76.0                 | 4.79.2      | Critical bug fix release                           |
| Newtonsoft.Json                            | 13.0.3                 | 13.0.4      | Patch update                                       |
| System.Collections.Immutable               | 8.0.0                  | 10.0.0      | Recommended for .NET 10.0                          |
| System.Configuration.ConfigurationManager  | 8.0.1                  | 10.0.0      | Recommended for .NET 10.0                          |
| System.Diagnostics.DiagnosticSource        | 8.0.1                  | 10.0.0      | Recommended for .NET 10.0                          |
| System.Formats.Asn1                        | 8.0.2                  | 10.0.0      | Recommended for .NET 10.0                          |
| System.IO.Pipelines                        | 8.0.0                  | 10.0.0      | Recommended for .NET 10.0                          |
| System.Memory.Data                         | 8.0.1                  | 10.0.0      | Recommended for .NET 10.0                          |
| System.Security.Cryptography.Pkcs          | 8.0.1                  | 10.0.0      | Recommended for .NET 10.0                          |
| System.Security.Cryptography.ProtectedData | 8.0.0                  | 10.0.0      | Recommended for .NET 10.0                          |
| System.Security.Permissions                | 8.0.0                  | 10.0.0      | Recommended for .NET 10.0                          |
| System.Text.Encodings.Web                  | 8.0.0                  | 10.0.0      | Recommended for .NET 10.0                          |
| System.Text.Json                           | 8.0.x (8.0.5;8.0.6)    | 10.0.0      | Recommended for .NET 10.0                          |
| System.Threading.Channels                  | 8.0.0                  | 10.0.0      | Recommended for .NET 10.0                          |

### Project upgrade details
This section contains details about each project upgrade and modifications that need to be done in the project.

#### DatabaseSchemaReader\DatabaseSchemaReader.csproj modifications

Project properties changes:
  - Target frameworks should be changed from `netstandard2.0` to `net10.0` (consider multi-targeting if library consumers require `netstandard2.0`).

NuGet packages changes:
  - Microsoft.NETFramework.ReferenceAssemblies 1.0.3 is incompatible with .NET 10.0; remove.

Other changes:
  - Review conditional `Compile Remove` entries and constants for `netstandard1.5`, `net45`, `net40`, `net35`; clean obsolete conditions when migrating to .NET 10.0.

#### CoreTest\CoreTest.csproj modifications

Project properties changes:
  - Target framework should be changed from `net8.0` to `net10.0`.

NuGet packages changes:
  - Microsoft.Data.Sqlite should be updated from `9.0.8` to `10.0.0`.

#### DatabaseSchemaReaderTest\DatabaseSchemaReaderTest.csproj modifications

Project properties changes:
  - Target framework should be changed from `net48` to `net10.0-windows`.

NuGet packages changes:
  - Replace or remove deprecated and framework-included packages per analysis: Azure.Identity 1.15.0 -> 1.17.1; Microsoft.Identity.Client 4.76.0 -> 4.79.2; update various `System.*` and `Microsoft.Extensions.*` packages to 10.0.0; remove packages whose functionality is included in the framework (e.g., System.Buffers, System.Memory, System.Runtime, etc.).
  - Incompatible packages (no supported version found): Microsoft.Data.SqlClient.SNI; Oracle.ManagedDataAccess; Stub.System.Data.SQLite.Core.NetFramework — consider removing or replacing with modern equivalents compatible with .NET 10 or vendor guidance.

#### DatabaseSchemaViewer\DatabaseSchemaViewer.csproj modifications

Project properties changes:
  - Target framework should be changed from `net48` to `net10.0-windows` and convert project to SDK-style.

NuGet packages changes:
  - Update `System.*` libraries to 10.0.0 where applicable; remove packages included in framework; address Oracle.ManagedDataAccess (no supported version for .NET 10 identified).

Other changes:
  - Convert non-SDK style project to SDK-style format following recommended migration guidance.
