using WooCommerce.Http.SourceInstallation.Structures;

namespace WooCommerce.Synchronising.Fetching.Products
{
  public interface IObtainer
  {
    public Task Get();

    public Task Get(IEnumerable<int> productIds);

  }
}
