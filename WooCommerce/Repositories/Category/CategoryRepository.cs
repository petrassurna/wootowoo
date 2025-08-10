using LiteDB;
using WooCommerce.Repositories.Products;
using WooCommerce.Synchronising.Fetchers.Categories.Structures;
using WooCommerce.Synchronising.Fetching.Categories.Structures;

namespace WooCommerce.Repositories.Category
{
  public class CategoryRepository
  {
    private readonly string _connectionString;
    private const string CATEGORIES = "Categories";
    private static readonly object _lock = new object();

    public CategoryRepository()
    {
      _connectionString = Location.DatabaseConnection();
    }

    public CategoryRepository(string connectionString)
    {
      _connectionString = connectionString;
    }

    public void DeleteCategories()
    {
      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<RepoProduct>(CATEGORIES);

        collection.DeleteAll();

        // Recreate indexes (optional if schema hasn't changed)
        collection.EnsureIndex(x => x.Id);
        collection.EnsureIndex(x => x.Name);
      }
    }


    public IEnumerable<RepoCategory> GetCategories()
    {
      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);

        var collection = db.GetCollection<RepoCategory>(CATEGORIES);
        return collection.FindAll().ToList();
      }
    }


    public int GetCategoryCount()
    {
      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);

        var collection = db.GetCollection<RepoProduct>(CATEGORIES);

        return collection.Count();
      }
    }


    public void SaveCategoriesIfNotPresent(IEnumerable<RepoCategory> repoCategories)
    {
      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<RepoCategory>("Categories");

        foreach (var repo in repoCategories)
        {
          if (!collection.Exists(x => x.Slug == repo.Slug))
          {
            collection.Insert(repo);
          }
        }
      }
    }



    public void SaveCategory(RepoCategory repoCategory)
    {
      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);

        var collection = db.GetCollection<RepoCategory>(CATEGORIES);
        collection.Upsert(repoCategory);
      }
    }

    public void SaveNewUploadedCategory(CategorySource category, CategoryClassesDestination uploaded)
    {
      RepoCategory repoCategory = new RepoCategory()
      {
        CategoryAtSource = category,
        DateAdded = DateTime.UtcNow,
        DestinationId = uploaded.id,
        SourceId = category.id,
        SourceParent = category.parent,
        Slug = category.slug
      };

      SaveCategory(repoCategory);
    }


    public void SaveUpdatedCategory(CategorySource category, CategoryClassesDestination uploaded)
    {
      RepoCategory repoCategory = new RepoCategory()
      {
        CategoryAtSource = category,
        DestinationId = uploaded.id,
        DateAdded = DateTime.UtcNow,
        SourceId = category.id,
        DestinationParent = uploaded.parent,
        SourceParent = category.parent,
        Slug = category.slug
      };

      SaveCategory(repoCategory);
    }

  }

}
