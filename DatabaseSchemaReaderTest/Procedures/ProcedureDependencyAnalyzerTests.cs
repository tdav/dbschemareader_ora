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
    }
}
