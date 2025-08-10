using LiteDB;

namespace WooCommerce.Repositories.Products
{
  public class ProductRepository
  {
    private readonly string _connectionString;
    private const string PRODUCTS = "Products";
    private static readonly object _lock = new object();

    public ProductRepository()
    {
      _connectionString = Location.DatabaseConnection();
    }

    public ProductRepository(string connectionString)
    {
      _connectionString = connectionString;
    }

    public void ClearDatabase()
    {
      lock (_lock)
      {
        if (DatabaseExists())
        {
          using var db = new LiteDatabase(_connectionString);
          var collection = db.GetCollection<RepoProduct>(PRODUCTS);
          collection.DeleteAll();
          collection.EnsureIndex(x => x.Id);
          collection.EnsureIndex(x => x.Name);
          collection.EnsureIndex(x => x.ProductType);
        }
      }
    }


    private bool DatabaseExists() => File.Exists(_connectionString);


    public void DeleteProducts()
    {
      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<RepoProduct>(PRODUCTS);

        collection.DeleteAll();

        // Recreate indexes (optional if schema hasn't changed)
        collection.EnsureIndex(x => x.Id);
        collection.EnsureIndex(x => x.Name);
      }
    }


    public IEnumerable<RepoProduct> GetAllVariations()
    {
      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);

        var collection = db.GetCollection<RepoProduct>(PRODUCTS);
        var variations = collection.Find(p => p.ProductType == "variable").ToList();

        return variations;
      }
    }


    public IEnumerable<RepoProduct> GetAllUnprocessedVariations()
    {
      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);

        var collection = db.GetCollection<RepoProduct>(PRODUCTS);

        var variations = collection
          .Find(p => p.ProductType == "variable" && p.VariationAdded == false)
          .ToList();

        return variations;
      }
    }


      public int GetProductsImportedCount()
      {
        lock (_lock)
        {
          using var db = new LiteDatabase(_connectionString);

          var collection = db.GetCollection<RepoProduct>(PRODUCTS);

          return collection.Count();
        }
      }


      public int GetVariationCount()
      {
        lock (_lock)
        {
          using var db = new LiteDatabase(_connectionString);

          var collection = db.GetCollection<RepoProduct>(PRODUCTS);

          return collection.Count(p => p.ProductType == "variable");
        }
      }


      public void SaveProduct(RepoProduct variation)
      {
        lock (_lock)
        {
          using var db = new LiteDatabase(_connectionString);

          var collection = db.GetCollection<RepoProduct>(PRODUCTS);
          collection.Upsert(variation);
        }
      }


      public void SaveProducts(IEnumerable<RepoProduct> productList)
      {
        lock (_lock)
        {
          using var db = new LiteDatabase(_connectionString);
          var collection = db.GetCollection<RepoProduct>(PRODUCTS);

          collection.EnsureIndex(x => x.Id);
          collection.EnsureIndex(x => x.Name);

          collection.Upsert(productList);
        }
      }

  }
}
