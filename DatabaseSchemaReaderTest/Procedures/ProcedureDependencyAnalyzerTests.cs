using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Procedures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Procedures
{
    /// <summary>
    /// Tests for ProcedureDependencyAnalyzer
    /// </summary>
    [TestClass]
    public class ProcedureDependencyAnalyzerTests
    {
        private ProcedureDependencyAnalyzer _analyzer;

        [TestInitialize]
        public void SetUp()
        {
            _analyzer = new ProcedureDependencyAnalyzer();
        }

        [TestMethod]
        public void TestExtractTableReferencesFromSelect()
        {
            // Arrange
            var sourceCode = @"
                SELECT * FROM customers c
                JOIN orders o ON c.id = o.customer_id
                WHERE c.status = 'ACTIVE'";

            // Act
            var tables = _analyzer.ExtractTableReferences(sourceCode);

            // Assert
            Assert.IsTrue(tables.Contains("customers"));
            Assert.IsTrue(tables.Contains("orders"));
        }

        [TestMethod]
        public void TestExtractTableReferencesFromInsert()
        {
            // Arrange
            var sourceCode = @"
                INSERT INTO audit_log (action, timestamp)
                VALUES ('LOGIN', SYSDATE)";

            // Act
            var tables = _analyzer.ExtractTableReferences(sourceCode);

            // Assert
            Assert.IsTrue(tables.Contains("audit_log"));
        }

        [TestMethod]
        public void TestExtractTableReferencesFromUpdate()
        {
            // Arrange
            var sourceCode = @"
                UPDATE employees
                SET salary = salary * 1.1
                WHERE department_id = 10";

            // Act
            var tables = _analyzer.ExtractTableReferences(sourceCode);

            // Assert
            Assert.IsTrue(tables.Contains("employees"));
        }

        [TestMethod]
        public void TestExtractTableReferencesFromDelete()
        {
            // Arrange
            var sourceCode = @"
                DELETE FROM temp_data
                WHERE created_date < SYSDATE - 30";

            // Act
            var tables = _analyzer.ExtractTableReferences(sourceCode);

            // Assert
            Assert.IsTrue(tables.Contains("temp_data"));
        }

        [TestMethod]
        public void TestExtractPackageCalls()
        {
            // Arrange
            var sourceCode = @"
                BEGIN
                    my_package.do_something(p_id);
                    other_pkg.process_data(p_value);
                END;";

            // Act
            var packages = _analyzer.ExtractPackageCalls(sourceCode);

            // Assert
            Assert.IsTrue(packages.Contains("my_package"));
            Assert.IsTrue(packages.Contains("other_pkg"));
        }

        [TestMethod]
        public void TestExtractPackageCallsExcludesBuiltIn()
        {
            // Arrange
            var sourceCode = @"
                BEGIN
                    DBMS_OUTPUT.PUT_LINE('Hello');
                    UTL_FILE.FCLOSE(v_file);
                END;";

            // Act
            var packages = _analyzer.ExtractPackageCalls(sourceCode);

            // Assert
            Assert.IsFalse(packages.Contains("DBMS_OUTPUT"));
            Assert.IsFalse(packages.Contains("UTL_FILE"));
        }

        [TestMethod]
        public void TestExtractProcedureCalls()
        {
            // Arrange
            var sourceCode = @"
                BEGIN
                    process_order(p_order_id);
                    validate_customer(p_customer_id);
                END;";

            // Act
            var procedures = _analyzer.ExtractProcedureCalls(sourceCode);

            // Assert
            Assert.IsTrue(procedures.Contains("process_order"));
            Assert.IsTrue(procedures.Contains("validate_customer"));
        }

        [TestMethod]
        public void TestExtractFunctionCalls()
        {
            // Arrange
            var sourceCode = @"
                v_result := calculate_total(p_items);
                v_name := get_customer_name(p_id);";

            // Act
            var functions = _analyzer.ExtractFunctionCalls(sourceCode);

            // Assert
            Assert.IsTrue(functions.Contains("calculate_total"));
            Assert.IsTrue(functions.Contains("get_customer_name"));
        }

        [TestMethod]
        public void TestExtractTableReferencesWithSchema()
        {
            // Arrange
            var sourceCode = @"
                SELECT * FROM hr.employees e
                JOIN sales.orders o ON e.id = o.emp_id";

            // Act
            var tables = _analyzer.ExtractTableReferences(sourceCode);

            // Assert
            Assert.IsTrue(tables.Contains("hr.employees"));
            Assert.IsTrue(tables.Contains("sales.orders"));
        }

        [TestMethod]
        public void TestEmptySourceCode()
        {
            // Act
            var tables = _analyzer.ExtractTableReferences(string.Empty);
            var procedures = _analyzer.ExtractProcedureCalls(null);

            // Assert
            Assert.AreEqual(0, tables.Count);
            Assert.AreEqual(0, procedures.Count);
        }

        [TestMethod]
        public void TestExtractTableReferencesFromMerge()
        {
            // Arrange
            var sourceCode = @"
                MERGE INTO target_table t
                USING source_table s
                ON (t.id = s.id)
                WHEN MATCHED THEN UPDATE SET t.value = s.value
                WHEN NOT MATCHED THEN INSERT (id, value) VALUES (s.id, s.value)";

            // Act
            var tables = _analyzer.ExtractTableReferences(sourceCode);

            // Assert
            Assert.IsTrue(tables.Any(t => t.Equals("target_table", System.StringComparison.OrdinalIgnoreCase)));
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
        public void TestContainsDynamicSql_ExecuteImmediate()
        {
            // Arrange
            var sourceCode = @"
                BEGIN
                    EXECUTE IMMEDIATE 'SELECT * FROM dynamic_table';
                END;";

            // Act
            var result = _analyzer.ContainsDynamicSql(sourceCode);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestContainsDynamicSql_DbmsSql()
        {
            // Arrange
            var sourceCode = @"
                BEGIN
                    l_cursor := DBMS_SQL.OPEN_CURSOR;
                END;";

            // Act
            var result = _analyzer.ContainsDynamicSql(sourceCode);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestContainsDynamicSql_NoDynamicSql()
        {
            // Arrange
            var sourceCode = @"
                BEGIN
                    SELECT * FROM static_table;
                END;";

            // Act
            var result = _analyzer.ContainsDynamicSql(sourceCode);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestAnalyzeSourceCode_AllTables()
        {
            // Arrange
            var sourceCode = @"
                BEGIN
                    SELECT * FROM table1;
                    INSERT INTO table2 VALUES (1);
                    UPDATE table3 SET col = 1;
                END;";

            // Act
            var result = _analyzer.AnalyzeSourceCode(sourceCode);

            // Assert
            Assert.IsTrue(result.AllTables.Count >= 3);
        }

        [TestMethod]
        public void TestAnalyzeSourceCode_WithComments()
        {
            // Arrange
            var sourceCode = @"
                BEGIN
                    -- This is a comment: SELECT * FROM commented_table;
                    SELECT * FROM real_table;
                    /* Multi-line comment
                       INSERT INTO another_commented_table VALUES (1);
                    */
                END;";

            // Act
            var result = _analyzer.AnalyzeSourceCode(sourceCode);

            // Assert
            Assert.IsTrue(result.ReadTables.Any(t => t.Equals("real_table", System.StringComparison.OrdinalIgnoreCase)));
            // Comments should be removed, so commented tables should not appear
        }

        [TestMethod]
        public void TestAnalyzeSourceCode_EmptySourceCode()
        {
            // Act
            var result = _analyzer.AnalyzeSourceCode(string.Empty);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.AllTables.Count);
            Assert.IsFalse(result.ContainsDynamicSql);
        }

        [TestMethod]
        public void TestContainsDynamicSql_EmptySourceCode()
        {
            // Act
            var result = _analyzer.ContainsDynamicSql(string.Empty);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
