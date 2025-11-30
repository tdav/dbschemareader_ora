//using DatabaseSchemaReader.CodeGen;

//namespace DatabaseSchemaReaderTest.Codegen
//{


//    /// <summary>
//    ///Create a simple model and write it to filesystem
//    ///</summary>
//    [TestClass]
//    public class CodeWriterBuildTest
//    {

//        [TestMethod]
//        public void BuildGeneratedCodeTest()
//        {
//            //arrange
//            var dbReader = TestHelper.GetNorthwindReader();
//            var schema = dbReader.ReadAll();

//            var directory = TestHelper.CreateDirectory("NorthwindCodeGen");
//            const string @namespace = "Northwind.Domain";
//            var settings = new CodeWriterSettings
//                               {
//                                   Namespace = @namespace, 
//                                   CodeTarget = CodeTarget.PocoNHibernateHbm, 
//                                   Namer = new PluralizingNamer(),
//                                   WriteProjectFile = true
//                               };

//            var codeWriter = new CodeWriter(schema, settings);

//            //act
//            codeWriter.Execute(directory);

//            //assert
//            var csproj = Path.Combine(directory.FullName, "Northwind.Domain.csproj");
//            Assert.IsTrue(File.Exists(csproj));

//            //can we build it?
//            var projectIsBuilt = BuildProject(csproj);
//            Assert.IsTrue(projectIsBuilt); //yes we can
//        }

//        ///// <summary>
//        ///// Builds the project - based on http://msdn.microsoft.com/en-us/library/microsoft.build.buildengine.engine.aspx.
//        ///// </summary>
//        ///// <param name="projectPath">The project (csproj) path</param>
//        ///// <returns>True if builds okay</returns>
//        //private static bool BuildProject(string projectPath)
//        //{
//        //    try
//        //    {
//        //        var logPath = Path.Combine(Path.GetDirectoryName(projectPath), "build.log");
//        //        var engine = new ProjectCollection();
//        //        var logger = new FileLogger { Parameters = "logfile=" + logPath };
//        //        engine.RegisterLogger(logger);
//        //        bool success = engine.LoadProject(projectPath).Build();
//        //        engine.UnregisterAllLoggers();
//        //        return success;
//        //    }
//        //    catch
//        //    {
//        //        return false;
//        //    }
//        //}

//    }
//}
