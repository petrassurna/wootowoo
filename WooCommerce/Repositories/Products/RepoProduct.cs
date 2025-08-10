using LiteDB;
using WooCommerce.Http.SourceInstallation.Structures;

namespace WooCommerce.Repositories.Products
{
  public class RepoProduct
  {
    [BsonId]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public string ProductType { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime? DateUploaded { get; set; }
    public bool VariationAdded { get; set; }
    public Product Product { get; set; }
  }
}
