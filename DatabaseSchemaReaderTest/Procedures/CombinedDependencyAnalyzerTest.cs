using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Procedures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Procedures
{
    /// <summary>
    /// Tests for CombinedDependencyAnalyzer
    /// </summary>
    [TestClass]
    public class CombinedDependencyAnalyzerTest
    {
        [TestMethod]
        public void TestGetDependencies_WithCatalogAndSourceCode()
        {
            // Arrange
            var dependencies = new List<EntityDependency>
            {
                new EntityDependency
                {
                    OwnerName = "HR",
                    ObjectName = "MY_PROC",
                    ObjectType = DatabaseEntityType.Procedure,
                    ReferencedOwner = "HR",
                    ReferencedName = "EMPLOYEES",
                    ReferencedType = DatabaseEntityType.Table,
                    DependencyType = "HARD"
                },
                new EntityDependency
                {
                    OwnerName = "HR",
                    ObjectName = "MY_PROC",
                    ObjectType = DatabaseEntityType.Procedure,
                    ReferencedOwner = "HR",
                    ReferencedName = "DEPARTMENTS",
                    ReferencedType = DatabaseEntityType.Table,
                    DependencyType = "HARD"
                }
            };

            var sources = new Dictionary<string, string>
            {
                {
                    "HR.MY_PROC",
                    @"CREATE OR REPLACE PROCEDURE my_proc AS
                    BEGIN
                        SELECT * FROM employees e
                        JOIN departments d ON e.dept_id = d.id
                        JOIN locations l ON d.location_id = l.id;
                    END;"
                }
            };

            var analyzer = new CombinedDependencyAnalyzer(dependencies, sources);

            // Act
            var result = analyzer.GetDependencies("HR", "MY_PROC");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("HR", result.SchemaOwner);
            Assert.AreEqual("MY_PROC", result.ProcedureName);
            Assert.AreEqual(2, result.CatalogDependencies.Count);
            Assert.IsTrue(result.SourceCodeDependencies.Count >= 3);
            // LOCATIONS should be in new dependencies (found in source but not in catalog)
            Assert.IsTrue(result.NewDependenciesFromSourceCode.Any(d => 
                d.ObjectName.Equals("locations", System.StringComparison.OrdinalIgnoreCase) ||
                d.ObjectName.Equals("l", System.StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void TestGetDependencies_WithDynamicSql()
        {
            // Arrange
            var dependencies = new List<EntityDependency>();
            var sources = new Dictionary<string, string>
            {
                {
                    "HR.DYNAMIC_PROC",
                    @"CREATE OR REPLACE PROCEDURE dynamic_proc AS
                    BEGIN
                        EXECUTE IMMEDIATE 'SELECT * FROM dynamic_table';
                    END;"
                }
            };

            var analyzer = new CombinedDependencyAnalyzer(dependencies, sources);

            // Act
            var result = analyzer.GetDependencies("HR", "DYNAMIC_PROC");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsDynamicSql);
            Assert.IsFalse(string.IsNullOrEmpty(result.DynamicSqlWarning));
        }

        [TestMethod]
        public void TestGetDependencies_WithDbmsSql()
        {
            // Arrange
            var dependencies = new List<EntityDependency>();
            var sources = new Dictionary<string, string>
            {
                {
                    "HR.DBMS_SQL_PROC",
                    @"CREATE OR REPLACE PROCEDURE dbms_sql_proc AS
                        l_cursor INTEGER;
                    BEGIN
                        l_cursor := DBMS_SQL.OPEN_CURSOR;
                        DBMS_SQL.PARSE(l_cursor, 'SELECT * FROM some_table', DBMS_SQL.NATIVE);
                    END;"
                }
            };

            var analyzer = new CombinedDependencyAnalyzer(dependencies, sources);

            // Act
            var result = analyzer.GetDependencies("HR", "DBMS_SQL_PROC");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsDynamicSql);
        }

        [TestMethod]
        public void TestGetDependencies_NoSourceCodeAvailable()
        {
            // Arrange
            var dependencies = new List<EntityDependency>
            {
                new EntityDependency
                {
                    OwnerName = "HR",
                    ObjectName = "NO_SOURCE_PROC",
                    ObjectType = DatabaseEntityType.Procedure,
                    ReferencedOwner = "HR",
                    ReferencedName = "EMPLOYEES",
                    ReferencedType = DatabaseEntityType.Table,
                    DependencyType = "HARD"
                }
            };
            var sources = new Dictionary<string, string>();

            var analyzer = new CombinedDependencyAnalyzer(dependencies, sources);

            // Act
            var result = analyzer.GetDependencies("HR", "NO_SOURCE_PROC");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.CatalogDependencies.Count);
            Assert.AreEqual(0, result.SourceCodeDependencies.Count);
            Assert.IsNull(result.SourceCodeAnalysis);
        }

        [TestMethod]
        public void TestGetDependencies_WithViewDependency()
        {
            // Arrange
            var dependencies = new List<EntityDependency>
            {
                new EntityDependency
                {
                    OwnerName = "HR",
                    ObjectName = "VIEW_PROC",
                    ObjectType = DatabaseEntityType.Procedure,
                    ReferencedOwner = "HR",
                    ReferencedName = "EMPLOYEE_VIEW",
                    ReferencedType = DatabaseEntityType.View,
                    DependencyType = "HARD"
                },
                new EntityDependency
                {
                    OwnerName = "HR",
                    ObjectName = "EMPLOYEE_VIEW",
                    ObjectType = DatabaseEntityType.View,
                    ReferencedOwner = "HR",
                    ReferencedName = "EMPLOYEES",
                    ReferencedType = DatabaseEntityType.Table,
                    DependencyType = "HARD"
                }
            };
            var sources = new Dictionary<string, string>();

            var analyzer = new CombinedDependencyAnalyzer(dependencies, sources);

            // Act
            var result = analyzer.GetDependencies("HR", "VIEW_PROC");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.CatalogDependencies.Any(d => 
                d.ObjectType == DatabaseEntityType.View && 
                d.ObjectName == "EMPLOYEE_VIEW"));
        }

        [TestMethod]
        public void TestGenerateComparisonReport()
        {
            // Arrange
            var dependencies = new List<EntityDependency>
            {
                new EntityDependency
                {
                    OwnerName = "HR",
                    ObjectName = "REPORT_PROC",
                    ObjectType = DatabaseEntityType.Procedure,
                    ReferencedOwner = "HR",
                    ReferencedName = "EMPLOYEES",
                    ReferencedType = DatabaseEntityType.Table,
                    DependencyType = "HARD"
                }
            };

            var sources = new Dictionary<string, string>
            {
                {
                    "HR.REPORT_PROC",
                    @"CREATE OR REPLACE PROCEDURE report_proc AS
                    BEGIN
                        SELECT * FROM employees;
                        INSERT INTO audit_log (action) VALUES ('REPORT');
                        UPDATE status_table SET last_run = SYSDATE;
                    END;"
                }
            };

            var analyzer = new CombinedDependencyAnalyzer(dependencies, sources);

            // Act
            var report = analyzer.GenerateComparisonReport("HR", "REPORT_PROC");

            // Assert
            Assert.IsNotNull(report);
            Assert.IsTrue(report.Contains("CATALOG DEPENDENCIES"));
            Assert.IsTrue(report.Contains("SOURCE CODE DEPENDENCIES"));
            Assert.IsTrue(report.Contains("NEW DEPENDENCIES"));
            Assert.IsTrue(report.Contains("SUMMARY"));
        }

        [TestMethod]
        public void TestGetDependencies_WithPackageCalls()
        {
            // Arrange
            var dependencies = new List<EntityDependency>();
            var sources = new Dictionary<string, string>
            {
                {
                    "HR.PACKAGE_CALLER",
                    @"CREATE OR REPLACE PROCEDURE package_caller AS
                    BEGIN
                        my_package.do_something(1);
                        other_pkg.process_data('test');
                    END;"
                }
            };

            var analyzer = new CombinedDependencyAnalyzer(dependencies, sources);

            // Act
            var result = analyzer.GetDependencies("HR", "PACKAGE_CALLER");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SourceCodeAnalysis);
            Assert.IsTrue(result.SourceCodeAnalysis.PackageCalls.Contains("my_package"));
            Assert.IsTrue(result.SourceCodeAnalysis.PackageCalls.Contains("other_pkg"));
        }

        [TestMethod]
        public void TestGetDependencies_NullInputs()
        {
            // Arrange
            var analyzer = new CombinedDependencyAnalyzer(null, null);

            // Act
            var result = analyzer.GetDependencies("HR", "TEST_PROC");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.CatalogDependencies.Count);
            Assert.AreEqual(0, result.SourceCodeDependencies.Count);
        }

        [TestMethod]
        public void TestGetDependencies_WithMergeStatement()
        {
            // Arrange
            var dependencies = new List<EntityDependency>();
            var sources = new Dictionary<string, string>
            {
                {
                    "HR.MERGE_PROC",
                    @"CREATE OR REPLACE PROCEDURE merge_proc AS
                    BEGIN
                        MERGE INTO target_table t
                        USING source_table s
                        ON (t.id = s.id)
                        WHEN MATCHED THEN UPDATE SET t.value = s.value
                        WHEN NOT MATCHED THEN INSERT (id, value) VALUES (s.id, s.value);
                    END;"
                }
            };

            var analyzer = new CombinedDependencyAnalyzer(dependencies, sources);

            // Act
            var result = analyzer.GetDependencies("HR", "MERGE_PROC");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.SourceCodeAnalysis);
            Assert.IsTrue(result.SourceCodeAnalysis.MergeTables.Any(t => 
                t.Equals("target_table", System.StringComparison.OrdinalIgnoreCase)));
        }
    }
}
