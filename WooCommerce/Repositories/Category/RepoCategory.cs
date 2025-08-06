using LiteDB;
using WooCommerce.Http.SourceInstallation.Categories;

namespace WooCommerce.Repositories.Category
{
  public class RepoCategory
  {
    [BsonId]
    public string Slug { get; set; }

    public int IdAtSource { get; set; }

    public int IdAtDestination { get; set; }

    public int ParentAtSource { get; set; }

    public int ParentAtDestination { get; set; }

    public DateTime DateAdded { get; set; }

    public CategorySource CategoryAtSource { get; set; }


  }
}
