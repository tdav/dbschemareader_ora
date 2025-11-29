using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.Data;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.InsertWriterTests
{
    [TestClass]
    public class OracleDataExporterTest
    {
        private static DatabaseSchema CreateTestSchema()
        {
            var schema = new DatabaseSchema(null, "Oracle.ManagedDataAccess.Client");

            // Create Categories table (no FK)
            var categories = new DatabaseTable { Name = "CATEGORIES" };
            categories.Columns.Add(new DatabaseColumn { Name = "CATEGORY_ID", DbDataType = "NUMBER", IsPrimaryKey = true });
            categories.Columns.Add(new DatabaseColumn { Name = "CATEGORY_NAME", DbDataType = "VARCHAR2" });
            categories.PrimaryKey = new DatabaseConstraint
            {
                ConstraintType = ConstraintType.PrimaryKey,
                Name = "PK_CATEGORIES"
            };
            categories.PrimaryKey.Columns.Add("CATEGORY_ID");
            schema.Tables.Add(categories);

            // Create Products table (FK to Categories)
            var products = new DatabaseTable { Name = "PRODUCTS" };
            products.Columns.Add(new DatabaseColumn { Name = "PRODUCT_ID", DbDataType = "NUMBER", IsPrimaryKey = true });
            products.Columns.Add(new DatabaseColumn { Name = "PRODUCT_NAME", DbDataType = "VARCHAR2" });
            products.Columns.Add(new DatabaseColumn { Name = "CATEGORY_ID", DbDataType = "NUMBER", IsForeignKey = true, ForeignKeyTableName = "CATEGORIES" });
            products.PrimaryKey = new DatabaseConstraint
            {
                ConstraintType = ConstraintType.PrimaryKey,
                Name = "PK_PRODUCTS"
            };
            products.PrimaryKey.Columns.Add("PRODUCT_ID");
            var productsFk = new DatabaseConstraint
            {
                ConstraintType = ConstraintType.ForeignKey,
                Name = "FK_PRODUCTS_CATEGORIES",
                RefersToTable = "CATEGORIES"
            };
            productsFk.Columns.Add("CATEGORY_ID");
            products.AddConstraint(productsFk);
            schema.Tables.Add(products);

            // Create Orders table (FK to Products)
            var orders = new DatabaseTable { Name = "ORDERS" };
            orders.Columns.Add(new DatabaseColumn { Name = "ORDER_ID", DbDataType = "NUMBER", IsPrimaryKey = true });
            orders.Columns.Add(new DatabaseColumn { Name = "PRODUCT_ID", DbDataType = "NUMBER", IsForeignKey = true, ForeignKeyTableName = "PRODUCTS" });
            orders.Columns.Add(new DatabaseColumn { Name = "ORDER_DATE", DbDataType = "DATE" });
            orders.PrimaryKey = new DatabaseConstraint
            {
                ConstraintType = ConstraintType.PrimaryKey,
                Name = "PK_ORDERS"
            };
            orders.PrimaryKey.Columns.Add("ORDER_ID");
            var ordersFk = new DatabaseConstraint
            {
                ConstraintType = ConstraintType.ForeignKey,
                Name = "FK_ORDERS_PRODUCTS",
                RefersToTable = "PRODUCTS"
            };
            ordersFk.Columns.Add("PRODUCT_ID");
            orders.AddConstraint(ordersFk);
            schema.Tables.Add(orders);

            return schema;
        }

        [TestMethod]
        public void TestTablesSortedByForeignKeyDependencies()
        {
            // Arrange
            var schema = CreateTestSchema();

            // Use a mock provider factory (we won't actually connect)
            // For this test, we just verify the sort order
            var exporter = CreateMockExporter(schema);

            // Act
            var sortedTables = exporter.GetSortedTables().ToList();

            // Assert
            Assert.AreEqual(3, sortedTables.Count);
            
            // Categories should be first (no dependencies)
            Assert.AreEqual("CATEGORIES", sortedTables[0].Name);
            
            // Products should be second (depends on Categories)
            Assert.AreEqual("PRODUCTS", sortedTables[1].Name);
            
            // Orders should be last (depends on Products)
            Assert.AreEqual("ORDERS", sortedTables[2].Name);
        }

        [TestMethod]
        public void TestSelectLastRecordsSqlGeneration()
        {
            // Arrange
            var schema = CreateTestSchema();
            var exporter = CreateMockExporter(schema);
            var ordersTable = schema.Tables.First(t => t.Name == "ORDERS");

            // Act
            var sql = exporter.BuildSelectLastRecordsSql(ordersTable);

            // Assert
            Assert.IsTrue(sql.Contains("ORDER BY \"ORDER_ID\" DESC"), "Should have ORDER BY with DESC for last records: " + sql);
            Assert.IsTrue(sql.Contains("ROWNUM <= 1000"), "Should limit to 1000 records: " + sql);
            Assert.IsTrue(sql.Contains("ORDER BY \"ORDER_ID\" ASC"), "Should restore original order: " + sql);
            Assert.IsTrue(sql.Contains("\"ORDER_ID\""), "Should include ORDER_ID column: " + sql);
            Assert.IsTrue(sql.Contains("\"PRODUCT_ID\""), "Should include PRODUCT_ID column: " + sql);
            Assert.IsTrue(sql.Contains("\"ORDER_DATE\""), "Should include ORDER_DATE column: " + sql);
        }

        [TestMethod]
        public void TestSelectLastRecordsSqlWithCustomMaxRecords()
        {
            // Arrange
            var schema = CreateTestSchema();
            var exporter = CreateMockExporter(schema);
            exporter.MaxRecords = 500;
            var categoriesTable = schema.Tables.First(t => t.Name == "CATEGORIES");

            // Act
            var sql = exporter.BuildSelectLastRecordsSql(categoriesTable);

            // Assert
            Assert.IsTrue(sql.Contains("ROWNUM <= 500"), "Should limit to 500 records: " + sql);
        }

        [TestMethod]
        public void TestSelectLastRecordsSqlWithCompositeKey()
        {
            // Arrange
            var schema = new DatabaseSchema(null, "Oracle.ManagedDataAccess.Client");
            var orderDetails = new DatabaseTable { Name = "ORDER_DETAILS" };
            orderDetails.Columns.Add(new DatabaseColumn { Name = "ORDER_ID", DbDataType = "NUMBER", IsPrimaryKey = true });
            orderDetails.Columns.Add(new DatabaseColumn { Name = "PRODUCT_ID", DbDataType = "NUMBER", IsPrimaryKey = true });
            orderDetails.Columns.Add(new DatabaseColumn { Name = "QUANTITY", DbDataType = "NUMBER" });
            orderDetails.PrimaryKey = new DatabaseConstraint
            {
                ConstraintType = ConstraintType.PrimaryKey,
                Name = "PK_ORDER_DETAILS"
            };
            orderDetails.PrimaryKey.Columns.Add("ORDER_ID");
            orderDetails.PrimaryKey.Columns.Add("PRODUCT_ID");
            schema.Tables.Add(orderDetails);

            var exporter = CreateMockExporter(schema);

            // Act
            var sql = exporter.BuildSelectLastRecordsSql(orderDetails);

            // Assert
            Assert.IsTrue(sql.Contains("ORDER BY \"ORDER_ID\", \"PRODUCT_ID\" DESC"), 
                "Should ORDER BY all PK columns: " + sql);
        }

        [TestMethod]
        public void TestSelectLastRecordsSqlWithoutPrimaryKey()
        {
            // Arrange
            var schema = new DatabaseSchema(null, "Oracle.ManagedDataAccess.Client");
            var table = new DatabaseTable { Name = "LOG_ENTRIES" };
            table.Columns.Add(new DatabaseColumn { Name = "LOG_TIME", DbDataType = "TIMESTAMP" });
            table.Columns.Add(new DatabaseColumn { Name = "MESSAGE", DbDataType = "VARCHAR2" });
            schema.Tables.Add(table);

            var exporter = CreateMockExporter(schema);

            // Act
            var sql = exporter.BuildSelectLastRecordsSql(table);

            // Assert
            // Should use first column when no PK
            Assert.IsTrue(sql.Contains("ORDER BY \"LOG_TIME\" DESC"), 
                "Should ORDER BY first column when no PK: " + sql);
        }

        [TestMethod]
        public void TestEscapeNamesDisabled()
        {
            // Arrange
            var schema = CreateTestSchema();
            var exporter = CreateMockExporter(schema);
            exporter.EscapeNames = false;
            var ordersTable = schema.Tables.First(t => t.Name == "ORDERS");

            // Act
            var sql = exporter.BuildSelectLastRecordsSql(ordersTable);

            // Assert
            Assert.IsTrue(sql.Contains("ORDER BY ORDER_ID DESC"), "Should not escape column names");
            Assert.IsFalse(sql.Contains("\"ORDER_ID\""), "Should not have quoted column names");
        }

        [TestMethod]
        public void TestDefaultSettings()
        {
            // Arrange
            var schema = CreateTestSchema();
            var exporter = CreateMockExporter(schema);

            // Assert defaults
            Assert.AreEqual(1000, exporter.MaxRecords, "Default MaxRecords should be 1000");
            Assert.AreEqual(1, exporter.MinRecords, "Default MinRecords should be 1");
            Assert.IsTrue(exporter.IncludeIdentity, "Default IncludeIdentity should be true");
            Assert.IsFalse(exporter.IncludeBlobs, "Default IncludeBlobs should be false");
            Assert.IsTrue(exporter.EscapeNames, "Default EscapeNames should be true");
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void TestMaxRecordsCannotBeZero()
        {
            // Arrange
            var schema = CreateTestSchema();
            var exporter = CreateMockExporter(schema);

            // Act - should throw
            exporter.MaxRecords = 0;
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void TestMaxRecordsCannotBeNegative()
        {
            // Arrange
            var schema = CreateTestSchema();
            var exporter = CreateMockExporter(schema);

            // Act - should throw
            exporter.MaxRecords = -1;
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void TestMinRecordsCannotBeNegative()
        {
            // Arrange
            var schema = CreateTestSchema();
            var exporter = CreateMockExporter(schema);

            // Act - should throw
            exporter.MinRecords = -1;
        }

        private OracleDataExporter CreateMockExporter(DatabaseSchema schema)
        {
            // For unit testing, we use SQLite's factory just as a placeholder
            // since we're not actually connecting to a database
            var factory = System.Data.SQLite.SQLiteFactory.Instance;
            return new OracleDataExporter(schema, "Data Source=:memory:", factory);
        }
    }
}
