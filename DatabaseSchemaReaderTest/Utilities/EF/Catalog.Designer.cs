// Simplified POCO replacements for legacy EF ObjectContext generated code to allow compilation under .NET 10 without EF6.
using System.Collections.Generic;

namespace DatabaseSchemaReaderTest.Utilities.EF
{
    public class CatalogContainer
    {
        public List<Category> Categories { get; } = new();
        public List<Product> Products { get; } = new();
    }

    public class Category
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ICollection<Product> Products { get; } = new List<Product>();
    }

    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public Category? Category { get; set; }
    }
}
