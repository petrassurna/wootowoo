using WooCommerce.Synchronising.Fetchers.Categories.Structures;

namespace WooCommerce.Http.SourceInstallation.Categories
{
  public static class CategoryTree
  {

    public static List<CategoryNode> BuildCategoryTree(this List<CategorySource> flatCategories)
    {
      var categoryMap = flatCategories.ToDictionary(c => c.id, c => new CategoryNode { Category = c });

      var roots = new List<CategoryNode>();

      foreach (var node in categoryMap.Values)
      {
        if (node.Category.parent == 0)
        {
          roots.Add(node);
        }
        else if (categoryMap.TryGetValue(node.Category.parent, out var parentNode))
        {
          parentNode.Children.Add(node);
        }
        else
        {
          roots.Add(node);
        }
      }

      return roots;
    }


    public static List<CategorySource> GetParents(this List<CategorySource> categories)
    {
      List<CategorySource> parents = new List<CategorySource>();

      foreach(var category in categories)
      {
        if(category.id == 0)
        {
          parents.Add(category);
        }
      }

      return parents;
    }

  }


  public class CategoryNode
  {
    public CategorySource Category { get; set; }

    public List<CategoryNode> Children { get; set; } = new List<CategoryNode>();

  }

}
