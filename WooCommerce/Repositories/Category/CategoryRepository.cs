using LiteDB;
using WooCommerce.Http.DestinationInstallation;
using WooCommerce.Http.SourceInstallation.Categories;
using WooCommerce.Repositories.Products;

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



    public int GetCategoryCount()
    {
      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);

        var collection = db.GetCollection<RepoProduct>(CATEGORIES);

        return collection.Count();
      }
    }


    public void SaveCategories(IEnumerable<RepoCategory> repoCategories)
    {
      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);

        var collection = db.GetCollection<RepoCategory>(CATEGORIES);
        collection.Upsert(repoCategories);
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


    public void SaveProducts(IEnumerable<RepoCategory> categoryList)
    {
      lock (_lock)
      {
        using var db = new LiteDatabase(_connectionString);
        var collection = db.GetCollection<RepoCategory>(CATEGORIES);

        collection.EnsureIndex(x => x.Slug);
        collection.Upsert(categoryList);
      }
    }

    public void SaveNewUploadedCategory(CategorySource category, CategoryUploaded uploaded)
    {
      RepoCategory repoCategory = new RepoCategory()
      {
        CategoryAtSource = category,
        DateAdded = DateTime.UtcNow,
        IdAtDestination = uploaded.id,
        IdAtSource = category.id,
        ParentAtSource = category.parent,
        Slug = category.slug
      };

      SaveCategory(repoCategory);
    }


    public void SaveUpdatedCategory(CategorySource category, CategoryUploaded uploaded)
    {
      RepoCategory repoCategory = new RepoCategory()
      {
        CategoryAtSource = category,
        IdAtDestination = uploaded.id,
        DateAdded = DateTime.UtcNow,
        IdAtSource = category.id,
        ParentAtDestination = 0,
        ParentAtSource = category.id,
        Slug = category.slug
      };

      SaveCategory(repoCategory);
    }

  }

}
