using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Procedures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Procedures
{
    /// <summary>
    /// Tests for OraclePackageAnalyzer
    /// </summary>
    [TestClass]
    public class OraclePackageAnalyzerTests
    {
        private OraclePackageAnalyzer _analyzer;

        [TestInitialize]
        public void SetUp()
        {
            _analyzer = new OraclePackageAnalyzer();
        }

        [TestMethod]
        public void TestExtractProcedures()
        {
            // Arrange
            var package = new DatabasePackage
            {
                Name = "MY_PACKAGE",
                SchemaOwner = "HR",
                Definition = @"
                    CREATE OR REPLACE PACKAGE my_package AS
                        PROCEDURE process_order(p_order_id IN NUMBER);
                        PROCEDURE validate_customer(p_customer_id IN NUMBER, p_result OUT VARCHAR2);
                    END my_package;"
            };

            // Act
            var procedures = _analyzer.ExtractProcedures(package);

            // Assert
            Assert.AreEqual(2, procedures.Count);
            Assert.IsTrue(procedures.Any(p => p.Name == "process_order"));
            Assert.IsTrue(procedures.Any(p => p.Name == "validate_customer"));
        }

        [TestMethod]
        public void TestExtractFunctions()
        {
            // Arrange
            var package = new DatabasePackage
            {
                Name = "MY_PACKAGE",
                SchemaOwner = "HR",
                Definition = @"
                    CREATE OR REPLACE PACKAGE my_package AS
                        FUNCTION calculate_total(p_items IN NUMBER) RETURN NUMBER;
                        FUNCTION get_customer_name(p_id IN NUMBER) RETURN VARCHAR2;
                    END my_package;"
            };

            // Act
            var functions = _analyzer.ExtractFunctions(package);

            // Assert
            Assert.AreEqual(2, functions.Count);
            Assert.IsTrue(functions.Any(f => f.Name == "calculate_total"));
            Assert.IsTrue(functions.Any(f => f.Name == "get_customer_name"));
        }

        [TestMethod]
        public void TestExtractTypes()
        {
            // Arrange
            var package = new DatabasePackage
            {
                Name = "MY_PACKAGE",
                SchemaOwner = "HR",
                Definition = @"
                    CREATE OR REPLACE PACKAGE my_package AS
                        TYPE t_order_rec IS RECORD (id NUMBER, amount NUMBER);
                        TYPE t_order_tab IS TABLE OF t_order_rec;
                    END my_package;"
            };

            // Act
            var types = _analyzer.ExtractTypes(package);

            // Assert
            Assert.AreEqual(2, types.Count);
            Assert.IsTrue(types.Contains("t_order_rec"));
            Assert.IsTrue(types.Contains("t_order_tab"));
        }

        [TestMethod]
        public void TestExtractProceduresWithParameters()
        {
            // Arrange
            var package = new DatabasePackage
            {
                Name = "MY_PACKAGE",
                SchemaOwner = "HR",
                Definition = @"
                    CREATE OR REPLACE PACKAGE my_package AS
                        PROCEDURE do_something(p_id IN NUMBER, p_name IN VARCHAR2, p_result OUT NUMBER);
                    END my_package;"
            };

            // Act
            var procedures = _analyzer.ExtractProcedures(package);

            // Assert
            Assert.AreEqual(1, procedures.Count);
            var proc = procedures[0];
            Assert.AreEqual("do_something", proc.Name);
            Assert.AreEqual(3, proc.Arguments.Count);
        }

        [TestMethod]
        public void TestNullPackage()
        {
            // Act
            var procedures = _analyzer.ExtractProcedures(null);
            var functions = _analyzer.ExtractFunctions(null);
            var types = _analyzer.ExtractTypes(null);
            var variables = _analyzer.ExtractVariables(null);

            // Assert
            Assert.AreEqual(0, procedures.Count);
            Assert.AreEqual(0, functions.Count);
            Assert.AreEqual(0, types.Count);
            Assert.AreEqual(0, variables.Count);
        }

        [TestMethod]
        public void TestEmptyDefinition()
        {
            // Arrange
            var package = new DatabasePackage
            {
                Name = "EMPTY_PACKAGE",
                Definition = string.Empty
            };

            // Act
            var procedures = _analyzer.ExtractProcedures(package);
            var functions = _analyzer.ExtractFunctions(package);

            // Assert
            Assert.AreEqual(0, procedures.Count);
            Assert.AreEqual(0, functions.Count);
        }

        [TestMethod]
        public void TestMixedPackage()
        {
            // Arrange
            var package = new DatabasePackage
            {
                Name = "MIXED_PACKAGE",
                SchemaOwner = "HR",
                Definition = @"
                    CREATE OR REPLACE PACKAGE mixed_package AS
                        TYPE t_item IS RECORD (id NUMBER);
                        PROCEDURE add_item(p_item IN t_item);
                        FUNCTION get_count RETURN NUMBER;
                    END mixed_package;"
            };

            // Act
            var procedures = _analyzer.ExtractProcedures(package);
            var functions = _analyzer.ExtractFunctions(package);
            var types = _analyzer.ExtractTypes(package);

            // Assert
            Assert.AreEqual(1, procedures.Count);
            Assert.AreEqual(1, functions.Count);
            Assert.AreEqual(1, types.Count);
        }
    }
}
