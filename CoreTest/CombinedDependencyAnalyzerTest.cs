using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Procedures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTest
{
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
                }
            };

            var sources = new Dictionary<string, string>
            {
                {
                    "HR.MY_PROC",
                    @"CREATE OR REPLACE PROCEDURE my_proc AS
                    BEGIN
                        SELECT * FROM employees e
                        JOIN departments d ON e.dept_id = d.id;
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
            Assert.AreEqual(1, result.CatalogDependencies.Count);
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
            Assert.IsTrue(report.Contains("SUMMARY"));
        }
    }

    [TestClass]
    public class ProcedureDependencyResolverTest
    {
        [TestMethod]
        public void TestGetTableDependencies()
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

            var resolver = new ProcedureDependencyResolver(dependencies);

            // Act
            var result = resolver.GetTableDependencies("HR", "MY_PROC");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.DirectTableDependencies.Count);
            Assert.IsTrue(result.DirectTableDependencies.Any(t => t.TableName == "EMPLOYEES"));
            Assert.IsTrue(result.DirectTableDependencies.Any(t => t.TableName == "DEPARTMENTS"));
        }

        [TestMethod]
        public void TestGetTableDependencies_WithViews()
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
                    ReferencedName = "EMP_VIEW",
                    ReferencedType = DatabaseEntityType.View,
                    DependencyType = "HARD"
                },
                new EntityDependency
                {
                    OwnerName = "HR",
                    ObjectName = "EMP_VIEW",
                    ObjectType = DatabaseEntityType.View,
                    ReferencedOwner = "HR",
                    ReferencedName = "EMPLOYEES",
                    ReferencedType = DatabaseEntityType.Table,
                    DependencyType = "HARD"
                }
            };

            var resolver = new ProcedureDependencyResolver(dependencies);

            // Act
            var result = resolver.GetTableDependencies("HR", "MY_PROC");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.ViewDependencies.Count);
            Assert.AreEqual("EMP_VIEW", result.ViewDependencies[0].ViewName);
            Assert.AreEqual(1, result.IndirectTableDependencies.Count);
            Assert.AreEqual("EMPLOYEES", result.IndirectTableDependencies[0].TableName);
        }

        [TestMethod]
        public void TestFindProceduresByTable()
        {
            // Arrange
            var dependencies = new List<EntityDependency>
            {
                new EntityDependency
                {
                    OwnerName = "HR",
                    ObjectName = "PROC1",
                    ObjectType = DatabaseEntityType.Procedure,
                    ReferencedOwner = "HR",
                    ReferencedName = "EMPLOYEES",
                    ReferencedType = DatabaseEntityType.Table,
                    DependencyType = "HARD"
                },
                new EntityDependency
                {
                    OwnerName = "HR",
                    ObjectName = "PROC2",
                    ObjectType = DatabaseEntityType.Procedure,
                    ReferencedOwner = "HR",
                    ReferencedName = "EMPLOYEES",
                    ReferencedType = DatabaseEntityType.Table,
                    DependencyType = "HARD"
                }
            };

            var resolver = new ProcedureDependencyResolver(dependencies);

            // Act
            var result = resolver.FindProceduresByTable("HR", "EMPLOYEES");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public void TestGetFullDependencyTree()
        {
            // Arrange
            var dependencies = new List<EntityDependency>
            {
                new EntityDependency
                {
                    OwnerName = "HR",
                    ObjectName = "ROOT_PROC",
                    ObjectType = DatabaseEntityType.Procedure,
                    ReferencedOwner = "HR",
                    ReferencedName = "SUB_PROC",
                    ReferencedType = DatabaseEntityType.Procedure,
                    DependencyType = "HARD"
                },
                new EntityDependency
                {
                    OwnerName = "HR",
                    ObjectName = "SUB_PROC",
                    ObjectType = DatabaseEntityType.Procedure,
                    ReferencedOwner = "HR",
                    ReferencedName = "EMPLOYEES",
                    ReferencedType = DatabaseEntityType.Table,
                    DependencyType = "HARD"
                }
            };

            var resolver = new ProcedureDependencyResolver(dependencies);

            // Act
            var tree = resolver.GetFullDependencyTree("HR", "ROOT_PROC");

            // Assert
            Assert.IsNotNull(tree);
            Assert.AreEqual("ROOT_PROC", tree.ObjectName);
            Assert.AreEqual(1, tree.Children.Count);
            Assert.AreEqual("SUB_PROC", tree.Children[0].ObjectName);
            Assert.AreEqual(1, tree.Children[0].Children.Count);
            Assert.AreEqual("EMPLOYEES", tree.Children[0].Children[0].ObjectName);
        }

        [TestMethod]
        public void TestGetDependencySummary()
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
                }
            };

            var resolver = new ProcedureDependencyResolver(dependencies);

            // Act
            var summary = resolver.GetDependencySummary("HR", "MY_PROC");

            // Assert
            Assert.IsNotNull(summary);
            Assert.IsTrue(summary.Contains("Dependency Summary"));
            Assert.IsTrue(summary.Contains("EMPLOYEES"));
        }
    }

    [TestClass]
    public class ProcedureDependencyAnalyzerTest
    {
        private ProcedureDependencyAnalyzer _analyzer;

        [TestInitialize]
        public void SetUp()
        {
            _analyzer = new ProcedureDependencyAnalyzer();
        }

        [TestMethod]
        public void TestAnalyzeSourceCode_CategorizedDependencies()
        {
            // Arrange
            var sourceCode = @"
                CREATE OR REPLACE PROCEDURE test_proc AS
                BEGIN
                    SELECT * FROM read_table;
                    INSERT INTO insert_table (col) VALUES (1);
                    UPDATE update_table SET col = 2;
                    DELETE FROM delete_table WHERE id = 1;
                    MERGE INTO merge_table t USING source s ON (t.id = s.id);
                END;";

            // Act
            var result = _analyzer.AnalyzeSourceCode(sourceCode);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ReadTables.Any(t => t.Equals("read_table", System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(result.InsertTables.Any(t => t.Equals("insert_table", System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(result.UpdateTables.Any(t => t.Equals("update_table", System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(result.DeleteTables.Any(t => t.Equals("delete_table", System.StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(result.MergeTables.Any(t => t.Equals("merge_table", System.StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void TestContainsDynamicSql()
        {
            // Arrange
            var sourceCode1 = "EXECUTE IMMEDIATE 'SELECT * FROM table1'";
            var sourceCode2 = "DBMS_SQL.OPEN_CURSOR";
            var sourceCode3 = "SELECT * FROM static_table";

            // Act & Assert
            Assert.IsTrue(_analyzer.ContainsDynamicSql(sourceCode1));
            Assert.IsTrue(_analyzer.ContainsDynamicSql(sourceCode2));
            Assert.IsFalse(_analyzer.ContainsDynamicSql(sourceCode3));
        }

        [TestMethod]
        public void TestExtractTableReferences_FromMerge()
        {
            // Arrange
            var sourceCode = "MERGE INTO target_table t USING source_table s ON (t.id = s.id)";

            // Act
            var tables = _analyzer.ExtractTableReferences(sourceCode);

            // Assert
            Assert.IsTrue(tables.Any(t => t.Equals("target_table", System.StringComparison.OrdinalIgnoreCase)));
        }
    }
}
