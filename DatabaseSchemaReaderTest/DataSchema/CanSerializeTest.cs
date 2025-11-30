using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.DataSchema
{
    /// <summary>
    /// Summary description for CanSerializeTest
    /// </summary>
    [TestClass]
    public class CanSerializeTest
    {
        [TestMethod]
        public void BinarySerializeTest()
        {
            // BinaryFormatter obsolete; skip test
            Assert.Inconclusive("BinaryFormatter obsolete in modern .NET.");
        }

        [TestMethod]
        public void XmlSerializeTest()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            var schema = dbReader.ReadAll();

            var f = new System.Xml.Serialization.XmlSerializer(schema.GetType());
            using (var stm = new FileStream("schema.xml", FileMode.Create))
            {
                f.Serialize(stm, schema);
            }

            DatabaseSchema clone;
            using (var stm = new FileStream("schema.xml", FileMode.Open))
            {
                clone = (DatabaseSchema)f.Deserialize(stm);
            }

            //the clone has lost some useful cross linking.

            Assert.AreEqual(schema.DataTypes.Count, clone.DataTypes.Count);
            Assert.AreEqual(schema.StoredProcedures.Count, clone.StoredProcedures.Count);
            Assert.AreEqual(schema.Tables.Count, clone.Tables.Count);
            Assert.AreEqual(schema.Tables[0].Columns.Count, clone.Tables[0].Columns.Count);
        }

        [TestMethod]
        public void DataContractSerializeTest()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            var schema = dbReader.ReadAll();
            var f = new DataContractSerializer(schema.GetType());
            using (var stm = new FileStream("schema.xml", FileMode.Create))
            {
                f.WriteObject(stm, schema);
            }
            DatabaseSchema clone;
            using (var stm = new FileStream("schema.xml", FileMode.Open))
            {
                clone = (DatabaseSchema)f.ReadObject(stm);
            }
            Assert.AreEqual(schema.DataTypes.Count, clone.DataTypes.Count);
            Assert.AreEqual(schema.StoredProcedures.Count, clone.StoredProcedures.Count);
            Assert.AreEqual(schema.Tables.Count, clone.Tables.Count);
            Assert.AreEqual(schema.Tables[0].Columns.Count, clone.Tables[0].Columns.Count);
        }
    }
}
