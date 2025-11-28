using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Procedures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Procedures
{
    /// <summary>
    /// Tests for TableRelationshipAnalyzer
    /// </summary>
    [TestClass]
    public class TableRelationshipAnalyzerTest
    {
        private DatabaseSchema CreateTestSchema()
        {
            var schema = new DatabaseSchema(null, SqlType.SqlServer);

            // Create tables
            var customers = schema.AddTable("Customers");
            customers.AddColumn("CustomerId").AddPrimaryKey("PK_Customers");
            customers.AddColumn("Name");

            var orders = schema.AddTable("Orders");
            orders.AddColumn("OrderId").AddPrimaryKey("PK_Orders");
            orders.AddColumn("CustomerId").AddForeignKey("FK_Orders_Customers", "Customers");

            var orderItems = schema.AddTable("OrderItems");
            orderItems.AddColumn("OrderItemId").AddPrimaryKey("PK_OrderItems");
            orderItems.AddColumn("OrderId").AddForeignKey("FK_OrderItems_Orders", "Orders");
            orderItems.AddColumn("ProductId").AddForeignKey("FK_OrderItems_Products", "Products");

            var products = schema.AddTable("Products");
            products.AddColumn("ProductId").AddPrimaryKey("PK_Products");
            products.AddColumn("Name");

            // Isolated table (no FK relationships)
            var logs = schema.AddTable("Logs");
            logs.AddColumn("LogId").AddPrimaryKey("PK_Logs");
            logs.AddColumn("Message");

            // Another isolated table
            var settings = schema.AddTable("Settings");
            settings.AddColumn("SettingId").AddPrimaryKey("PK_Settings");
            settings.AddColumn("Value");

            // Fix up references
            DatabaseSchemaFixer.UpdateReferences(schema);

            return schema;
        }

        [TestMethod]
        public void TestGetLinkedTables()
        {
            var schema = CreateTestSchema();
            var analyzer = new TableRelationshipAnalyzer(schema);

            var linkedTables = analyzer.GetLinkedTables();

            Assert.IsNotNull(linkedTables);
            Assert.AreEqual(4, linkedTables.Count);
            Assert.IsTrue(linkedTables.Any(t => t.Name == "Customers"));
            Assert.IsTrue(linkedTables.Any(t => t.Name == "Orders"));
            Assert.IsTrue(linkedTables.Any(t => t.Name == "OrderItems"));
            Assert.IsTrue(linkedTables.Any(t => t.Name == "Products"));
        }

        [TestMethod]
        public void TestGetIsolatedTables()
        {
            var schema = CreateTestSchema();
            var analyzer = new TableRelationshipAnalyzer(schema);

            var isolatedTables = analyzer.GetIsolatedTables();

            Assert.IsNotNull(isolatedTables);
            Assert.AreEqual(2, isolatedTables.Count);
            Assert.IsTrue(isolatedTables.Any(t => t.Name == "Logs"));
            Assert.IsTrue(isolatedTables.Any(t => t.Name == "Settings"));
        }

        [TestMethod]
        public void TestGetTableClusters()
        {
            var schema = CreateTestSchema();
            var analyzer = new TableRelationshipAnalyzer(schema);

            var clusters = analyzer.GetTableClusters();

            Assert.IsNotNull(clusters);
            Assert.AreEqual(1, clusters.Count);
            var cluster = clusters.First();
            Assert.AreEqual(4, cluster.TableCount);
            Assert.IsNotNull(cluster.CentralTable);
        }

        [TestMethod]
        public void TestGetTableRelationships()
        {
            var schema = CreateTestSchema();
            var analyzer = new TableRelationshipAnalyzer(schema);

            var relationships = analyzer.GetTableRelationships("Orders");

            Assert.IsNotNull(relationships);
            Assert.AreEqual("Orders", relationships.Table.Name);
            Assert.IsFalse(relationships.IsIsolated);

            // Orders has FK to Customers (1 parent)
            Assert.AreEqual(1, relationships.ParentTables.Count);
            Assert.AreEqual("Customers", relationships.ParentTables.First().Table.Name);

            // OrderItems references Orders (1 child)
            Assert.AreEqual(1, relationships.ChildTables.Count);
            Assert.AreEqual("OrderItems", relationships.ChildTables.First().Table.Name);
        }

        [TestMethod]
        public void TestGetTableRelationshipsIsolated()
        {
            var schema = CreateTestSchema();
            var analyzer = new TableRelationshipAnalyzer(schema);

            var relationships = analyzer.GetTableRelationships("Logs");

            Assert.IsNotNull(relationships);
            Assert.AreEqual("Logs", relationships.Table.Name);
            Assert.IsTrue(relationships.IsIsolated);
            Assert.AreEqual(0, relationships.ParentTables.Count);
            Assert.AreEqual(0, relationships.ChildTables.Count);
        }

        [TestMethod]
        public void TestFindPathBetweenTables()
        {
            var schema = CreateTestSchema();
            var analyzer = new TableRelationshipAnalyzer(schema);

            var path = analyzer.FindPathBetweenTables("Customers", "Products");

            Assert.IsNotNull(path);
            Assert.IsTrue(path.Count >= 2);
            Assert.AreEqual("Customers", path.First().Name);
            Assert.AreEqual("Products", path.Last().Name);
        }

        [TestMethod]
        public void TestFindPathBetweenTables_NoPath()
        {
            var schema = CreateTestSchema();
            var analyzer = new TableRelationshipAnalyzer(schema);

            var path = analyzer.FindPathBetweenTables("Customers", "Logs");

            Assert.IsNull(path);
        }

        [TestMethod]
        public void TestFindPathBetweenTables_SameTable()
        {
            var schema = CreateTestSchema();
            var analyzer = new TableRelationshipAnalyzer(schema);

            var path = analyzer.FindPathBetweenTables("Customers", "Customers");

            Assert.IsNotNull(path);
            Assert.AreEqual(1, path.Count);
            Assert.AreEqual("Customers", path.First().Name);
        }

        [TestMethod]
        public void TestGetRelatedTablesWithinDepth()
        {
            var schema = CreateTestSchema();
            var analyzer = new TableRelationshipAnalyzer(schema);

            // Depth 0 - only the table itself
            var depth0 = analyzer.GetRelatedTablesWithinDepth("Customers", 0);
            Assert.AreEqual(1, depth0.Count);
            Assert.AreEqual("Customers", depth0.First().Name);

            // Depth 1 - Customers and directly related tables
            var depth1 = analyzer.GetRelatedTablesWithinDepth("Customers", 1);
            Assert.IsTrue(depth1.Count >= 2);
            Assert.IsTrue(depth1.Any(t => t.Name == "Customers"));
            Assert.IsTrue(depth1.Any(t => t.Name == "Orders"));

            // Depth 2 - should include OrderItems
            var depth2 = analyzer.GetRelatedTablesWithinDepth("Customers", 2);
            Assert.IsTrue(depth2.Count >= 3);
            Assert.IsTrue(depth2.Any(t => t.Name == "OrderItems"));
        }

        [TestMethod]
        public void TestGetRelationshipStatistics()
        {
            var schema = CreateTestSchema();
            var analyzer = new TableRelationshipAnalyzer(schema);

            var stats = analyzer.GetRelationshipStatistics();

            Assert.IsNotNull(stats);
            Assert.AreEqual(6, stats.TotalTables);
            Assert.AreEqual(4, stats.LinkedTables);
            Assert.AreEqual(2, stats.IsolatedTables);
            Assert.AreEqual(1, stats.ClusterCount);
            Assert.IsTrue(stats.TotalRelationships >= 3);
        }

        [TestMethod]
        public void TestExtensionMethods()
        {
            var schema = CreateTestSchema();

            // Test extension method GetTableRelationshipAnalyzer
            var analyzer = schema.GetTableRelationshipAnalyzer();
            Assert.IsNotNull(analyzer);

            // Test extension method GetRelationshipStatistics
            var stats = schema.GetRelationshipStatistics();
            Assert.IsNotNull(stats);
            Assert.AreEqual(6, stats.TotalTables);
        }
    }
}
