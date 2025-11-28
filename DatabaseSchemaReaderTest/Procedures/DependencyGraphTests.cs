using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Procedures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Procedures
{
    /// <summary>
    /// Tests for dependency graph functionality
    /// </summary>
    [TestClass]
    public class DependencyGraphTests
    {
        [TestMethod]
        public void TestBuildGraphFromSchema()
        {
            // Arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            
            var table1 = new DatabaseTable { Name = "Table1", SchemaOwner = "dbo" };
            var table2 = new DatabaseTable { Name = "Table2", SchemaOwner = "dbo" };
            
            // Add a foreign key from Table2 to Table1
            table2.AddConstraint(new DatabaseConstraint
            {
                Name = "FK_Table2_Table1",
                ConstraintType = ConstraintType.ForeignKey,
                RefersToTable = "Table1",
                RefersToSchema = "dbo"
            });
            
            schema.Tables.Add(table1);
            schema.Tables.Add(table2);

            var builder = new DependencyGraphBuilder();

            // Act
            var graph = builder.BuildFromSchema(schema);

            // Assert
            Assert.IsNotNull(graph);
            Assert.AreEqual(2, graph.Nodes.Count);
            Assert.IsTrue(graph.Edges.Count >= 1);
        }

        [TestMethod]
        public void TestGetDependencies()
        {
            // Arrange
            var graph = new DependencyGraph();
            
            var entity1 = new DatabaseEntity { Name = "Entity1", SchemaOwner = "dbo", EntityType = DatabaseEntityType.Table };
            var entity2 = new DatabaseEntity { Name = "Entity2", SchemaOwner = "dbo", EntityType = DatabaseEntityType.Procedure };
            
            graph.Nodes.Add(entity1);
            graph.Nodes.Add(entity2);
            
            graph.Edges.Add(new EntityDependency
            {
                OwnerName = "dbo",
                ObjectName = "Entity2",
                ObjectType = DatabaseEntityType.Procedure,
                ReferencedOwner = "dbo",
                ReferencedName = "Entity1",
                ReferencedType = DatabaseEntityType.Table
            });

            // Act
            var dependencies = graph.GetDependencies(entity2).ToList();

            // Assert
            Assert.AreEqual(1, dependencies.Count);
            Assert.AreEqual("Entity1", dependencies[0].Name);
        }

        [TestMethod]
        public void TestGetReferencedBy()
        {
            // Arrange
            var graph = new DependencyGraph();
            
            var entity1 = new DatabaseEntity { Name = "Entity1", SchemaOwner = "dbo", EntityType = DatabaseEntityType.Table };
            var entity2 = new DatabaseEntity { Name = "Entity2", SchemaOwner = "dbo", EntityType = DatabaseEntityType.Procedure };
            
            graph.Nodes.Add(entity1);
            graph.Nodes.Add(entity2);
            
            graph.Edges.Add(new EntityDependency
            {
                OwnerName = "dbo",
                ObjectName = "Entity2",
                ObjectType = DatabaseEntityType.Procedure,
                ReferencedOwner = "dbo",
                ReferencedName = "Entity1",
                ReferencedType = DatabaseEntityType.Table
            });

            // Act
            var referencedBy = graph.GetReferencedBy(entity1).ToList();

            // Assert
            Assert.AreEqual(1, referencedBy.Count);
            Assert.AreEqual("Entity2", referencedBy[0].Name);
        }

        [TestMethod]
        public void TestGetByType()
        {
            // Arrange
            var graph = new DependencyGraph();
            
            graph.Nodes.Add(new DatabaseEntity { Name = "Table1", EntityType = DatabaseEntityType.Table });
            graph.Nodes.Add(new DatabaseEntity { Name = "Table2", EntityType = DatabaseEntityType.Table });
            graph.Nodes.Add(new DatabaseEntity { Name = "Proc1", EntityType = DatabaseEntityType.Procedure });
            graph.Nodes.Add(new DatabaseEntity { Name = "View1", EntityType = DatabaseEntityType.View });

            // Act
            var tables = graph.GetByType(DatabaseEntityType.Table).ToList();
            var procedures = graph.GetByType(DatabaseEntityType.Procedure).ToList();

            // Assert
            Assert.AreEqual(2, tables.Count);
            Assert.AreEqual(1, procedures.Count);
        }

        [TestMethod]
        public void TestFindCircularDependencies()
        {
            // Arrange
            var graph = new DependencyGraph();
            
            var entity1 = new DatabaseEntity { Name = "A", SchemaOwner = "dbo", EntityType = DatabaseEntityType.Procedure };
            var entity2 = new DatabaseEntity { Name = "B", SchemaOwner = "dbo", EntityType = DatabaseEntityType.Procedure };
            var entity3 = new DatabaseEntity { Name = "C", SchemaOwner = "dbo", EntityType = DatabaseEntityType.Procedure };
            
            graph.Nodes.Add(entity1);
            graph.Nodes.Add(entity2);
            graph.Nodes.Add(entity3);
            
            // Create a cycle: A -> B -> C -> A
            graph.Edges.Add(new EntityDependency
            {
                OwnerName = "dbo", ObjectName = "A", ObjectType = DatabaseEntityType.Procedure,
                ReferencedOwner = "dbo", ReferencedName = "B", ReferencedType = DatabaseEntityType.Procedure
            });
            graph.Edges.Add(new EntityDependency
            {
                OwnerName = "dbo", ObjectName = "B", ObjectType = DatabaseEntityType.Procedure,
                ReferencedOwner = "dbo", ReferencedName = "C", ReferencedType = DatabaseEntityType.Procedure
            });
            graph.Edges.Add(new EntityDependency
            {
                OwnerName = "dbo", ObjectName = "C", ObjectType = DatabaseEntityType.Procedure,
                ReferencedOwner = "dbo", ReferencedName = "A", ReferencedType = DatabaseEntityType.Procedure
            });

            // Act
            var cycles = graph.FindCircularDependencies().ToList();

            // Assert
            Assert.IsTrue(cycles.Count >= 1);
        }

        [TestMethod]
        public void TestNoCircularDependencies()
        {
            // Arrange
            var graph = new DependencyGraph();
            
            var entity1 = new DatabaseEntity { Name = "A", SchemaOwner = "dbo", EntityType = DatabaseEntityType.Procedure };
            var entity2 = new DatabaseEntity { Name = "B", SchemaOwner = "dbo", EntityType = DatabaseEntityType.Procedure };
            var entity3 = new DatabaseEntity { Name = "C", SchemaOwner = "dbo", EntityType = DatabaseEntityType.Procedure };
            
            graph.Nodes.Add(entity1);
            graph.Nodes.Add(entity2);
            graph.Nodes.Add(entity3);
            
            // No cycle: A -> B -> C
            graph.Edges.Add(new EntityDependency
            {
                OwnerName = "dbo", ObjectName = "A", ObjectType = DatabaseEntityType.Procedure,
                ReferencedOwner = "dbo", ReferencedName = "B", ReferencedType = DatabaseEntityType.Procedure
            });
            graph.Edges.Add(new EntityDependency
            {
                OwnerName = "dbo", ObjectName = "B", ObjectType = DatabaseEntityType.Procedure,
                ReferencedOwner = "dbo", ReferencedName = "C", ReferencedType = DatabaseEntityType.Procedure
            });

            // Act
            var cycles = graph.FindCircularDependencies().ToList();

            // Assert
            Assert.AreEqual(0, cycles.Count);
        }

        [TestMethod]
        public void TestFindEntity()
        {
            // Arrange
            var graph = new DependencyGraph();
            
            graph.Nodes.Add(new DatabaseEntity { Name = "Entity1", SchemaOwner = "dbo", EntityType = DatabaseEntityType.Table });
            graph.Nodes.Add(new DatabaseEntity { Name = "Entity2", SchemaOwner = "hr", EntityType = DatabaseEntityType.Table });
            graph.Nodes.Add(new DatabaseEntity { Name = "Entity1", SchemaOwner = "hr", EntityType = DatabaseEntityType.Procedure });

            // Act
            var found1 = graph.FindEntity("Entity1", "dbo");
            var found2 = graph.FindEntity("Entity1", "hr", DatabaseEntityType.Procedure);
            var notFound = graph.FindEntity("NotExists");

            // Assert
            Assert.IsNotNull(found1);
            Assert.AreEqual(DatabaseEntityType.Table, found1.EntityType);
            
            Assert.IsNotNull(found2);
            Assert.AreEqual(DatabaseEntityType.Procedure, found2.EntityType);
            
            Assert.IsNull(notFound);
        }

        [TestMethod]
        public void TestMergeGraphs()
        {
            // Arrange
            var graph1 = new DependencyGraph();
            var graph2 = new DependencyGraph();
            
            graph1.Nodes.Add(new DatabaseEntity { Name = "A", SchemaOwner = "dbo", EntityType = DatabaseEntityType.Table });
            graph2.Nodes.Add(new DatabaseEntity { Name = "B", SchemaOwner = "dbo", EntityType = DatabaseEntityType.Table });
            graph2.Edges.Add(new EntityDependency
            {
                OwnerName = "dbo", ObjectName = "B", ObjectType = DatabaseEntityType.Table,
                ReferencedOwner = "dbo", ReferencedName = "A", ReferencedType = DatabaseEntityType.Table
            });

            var builder = new DependencyGraphBuilder();

            // Act
            builder.MergeGraphs(graph1, graph2);

            // Assert
            Assert.AreEqual(2, graph1.Nodes.Count);
            Assert.AreEqual(1, graph1.Edges.Count);
        }
    }
}
