using LiteDB;
using WooCommerce.Http.SourceInstallation.Structures;

namespace WooCommerce.Repository.Products
{
  public class RepoVariation
  {
    [BsonId]
    public int Id { get; set; }

    public int ProductId { get; set; }

    public DateTime DateAdded { get; set; }

    public Variation Variation { get; set; }
  }
}
