using LiteDB;

namespace WooCommerce.Repositories.Summary
{
  public class ImportSummaryRepository
  {
    private readonly string _connectionString;
    private string IMPORT_SUMMARY = "ImportSummary";
    private static readonly object _lock = new object();

    public ImportSummaryRepository()
    {
      _connectionString = Repository.DatabaseConnection();
    }

    public ImportSummaryRepository(string connectionString)
    {
      _connectionString = connectionString;
    }

    public ImportSummary Get()
    {
      lock (_lock)
      {
        using (var db = new LiteDatabase(_connectionString))
        {
          var collection = db.GetCollection<ImportSummary>(IMPORT_SUMMARY);
          return collection.FindById(1);
        }
      }

    }


    public ImportSummary Complete()
    {
      ImportSummary summary = Get();

      summary.DateComplete = DateTime.UtcNow;
      Save(summary);

      return summary;
    }


    public ImportSummary Create()
    {
      lock (_lock)
      {
        ImportSummary summary = new ImportSummary()
        {
          Id = 1,
          DateCommenced = DateTime.UtcNow,
          ProductCountAtSource = 0,
          ProductsImported = 0
        };

        EnsureFolder();

        using (var db = new LiteDatabase(_connectionString))
        {
          var collection = db.GetCollection<ImportSummary>(IMPORT_SUMMARY);
          collection.Upsert(summary);
        }

        return summary;
      }
    }


    private void EnsureFolder()
    {
      if (string.IsNullOrWhiteSpace(Repository.DatabaseFolder()))
        return; 

      Directory.CreateDirectory(Repository.DatabaseFolder()); 
    }


    public ImportSummary Delete()
    {
      lock (_lock)
      {
        using (var db = new LiteDatabase(_connectionString))
        {
          var collection = db.GetCollection<ImportSummary>(IMPORT_SUMMARY);
          var existing = collection.FindById(1);

          if (existing != null)
          {
            collection.Delete(1);
          }

          return existing;
        }
      }
    }


    public void Save(ImportSummary summary)
    {
      lock (_lock)
      {
        using (var db = new LiteDatabase(_connectionString))
        {
          var collection = db.GetCollection<ImportSummary>(IMPORT_SUMMARY);

          collection.Upsert(summary);
        }
      }
    }


    public ImportSummary UpdateProductsAtSource(int productCountAtSource)
    {
      lock (_lock)
      {
        ImportSummary summary = Get();

        summary.ProductCountAtSource = productCountAtSource;

        using (var db = new LiteDatabase(_connectionString))
        {
          var collection = db.GetCollection<ImportSummary>(IMPORT_SUMMARY);
          collection.Upsert(summary);
        }

        return summary;
      }
    }


  }
}
