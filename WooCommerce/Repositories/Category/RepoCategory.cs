using LiteDB;
using WooCommerce.Synchronizers.Categories.Structures;
using WooCommerce.Synchronizers.Categories.Structures.Origin;

namespace WooCommerce.Repositories.Category
{
  public class RepoCategory
  {
    [BsonId]
    public string Slug { get; set; }

    public int DestinationId { get; set; }

    public int DestinationParent { get; set; }

    public int SourceId { get; set; }

    public int SourceParent { get; set; }

    public DateTime DateAdded { get; set; }

    public CategorySource CategoryAtSource { get; set; }


  }
}
