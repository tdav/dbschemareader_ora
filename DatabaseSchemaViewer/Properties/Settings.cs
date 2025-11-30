namespace DatabaseSchemaViewer.Properties
{
    public sealed class Settings
    {
        public static Settings Default { get; } = new Settings();
        private Settings() { }
        public bool CodeGenUseForeignKeyIdProperties { get; set; } = false;
        public bool CodeGenUsePluralizingNamer { get; set; } = false;
        public bool CodeGenReadProcedures { get; set; } = false;
        public string CodeGenNamespace { get; set; } = "MyNamespace";
        public string CodeGenFilePath { get; set; } = ".";
        public bool CodeGenWriteUnitTest { get; set; } = false;
        public bool CodeGenWriteProjectFile { get; set; } = false;
        public bool CodeGenIncludeViews { get; set; } = false;
        public bool CodeGenWriteIndexAttribute { get; set; } = false;
        public string Provider { get; set; } = string.Empty;
        public string ScriptDirectory { get; set; } = ".";
        public int CodeGenProjectType { get; set; } = 0;
        public string SchemaOwner { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public string CompareConnectionString { get; set; } = string.Empty;
        public void Save() { }
    }
}
