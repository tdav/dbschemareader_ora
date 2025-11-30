using System;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest
{
    
    /// <summary>
    /// 
    ///</summary>
    [TestClass]
    public class DatabaseReaderTest
    {

        [TestMethod] 
        public void NoConnectionStringTest()
        {
            new DatabaseReader((string)null, SqlType.SqlServer);

            Assert.Fail("Should not have succeeded");
        }

        [TestMethod] 
        public void NoProviderTest()
        {
            new DatabaseReader("Dummy", null);

            Assert.Fail("Should not have succeeded");
        }

        [TestMethod]
        public void SqlTypeTest()
        {
            var dr =  new DatabaseReader("Dummy", SqlType.SqlServer);
            Assert.AreEqual("System.Data.SqlClient", dr.DatabaseSchema.Provider);

            //the other types will fail if they aren't installed
        }
    }
}
