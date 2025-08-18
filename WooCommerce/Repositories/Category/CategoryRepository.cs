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
      _connectionString = Repository.DatabaseConnection();
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


    public IEnumerable<CategorySource> GetAllCategorySource() 
      => GetCategories().Select(c => c.CategoryAtSource);



    public IEnumerable<RepositoryCategory> GetCategories()
    {
      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);

        var collection = db.GetCollection<RepositoryCategory>(CATEGORIES);
        return collection.FindAll().ToList();
      }
    }


    public IEnumerable<RepositoryCategory> GetCategories(IEnumerable<int> categoryIds)
    {
      var ids = categoryIds?.Distinct().ToArray() ?? Array.Empty<int>();
      if (ids.Length == 0) return Enumerable.Empty<RepositoryCategory>();

      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<RepositoryCategory>(CATEGORIES);

        return collection.FindAll().Where(c => categoryIds.Contains(c.SourceId)).ToList();
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


    public void SaveCategoriesIfNotPresent(IEnumerable<RepositoryCategory> repoCategories)
    {
      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<RepositoryCategory>("Categories");

        foreach (var repo in repoCategories)
        {
          if (!collection.Exists(x => x.Slug == repo.Slug))
          {
            collection.Insert(repo);
          }
        }
      }
    }



    public void SaveCategory(RepositoryCategory repoCategory)
    {
      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);

        var collection = db.GetCollection<RepositoryCategory>(CATEGORIES);
        collection.Upsert(repoCategory);
      }
    }

    public void SaveNewUploadedCategory(CategorySource category, CategoryClassesDestination uploaded)
    {
      RepositoryCategory repoCategory = new RepositoryCategory()
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
      RepositoryCategory repoCategory = new RepositoryCategory()
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
