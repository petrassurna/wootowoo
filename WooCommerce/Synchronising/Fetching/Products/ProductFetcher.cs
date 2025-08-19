using Microsoft.Extensions.Logging;
using WooCommerce.Http;
using WooCommerce.Http.Products.Fetching;
using WooCommerce.Http.SourceInstallation;
using WooCommerce.Http.SourceInstallation.Structures;
using WooCommerce.Repositories.Summary;
using WooCommerce.Synchronising.Fetching.Products;
using WooCommerce.Synchronising.Fetching.Products.Obtainers.AllProducts;

namespace WooCommerce.Synchronising.Fetchers.Products
{
  /// <summary>
  /// Get products in batches 
  /// Depending on type, het further data eg Variations
  /// </summary>
  public class ProductFetcher : IFetcher
  {

    private readonly HttpClient _httpClient;
    private readonly WordPressInstallation _installation;
    private readonly ImportSummaryRepository _importSummary;
    private readonly ILogger _logger;
    private readonly ProductFetchingHttp _productHttp;

    public ProductFetcher(HttpClient httpClient, WordPressInstallation installation, ILogger logger)
    {
      _httpClient = httpClient;
      _installation = installation;
      _importSummary = new ImportSummaryRepository();
      _logger = logger;
      _productHttp = new ProductFetchingHttp(installation, httpClient);
    }


    public async Task Fetch()
    {
      await Initialise();
      Console.WriteLine($"Getting products from {_installation.Url}");
      IEnumerable<Product> products = Enumerable.Empty<Product>();

      foreach (var obtainer in Obtainers())
      {
        await obtainer.Get();
      }
    }

    public async Task Fetch(IEnumerable<int> productIds)
    {
      _importSummary.UpdateProductsAtSource(productIds.Count());
      Console.WriteLine($"Getting products {string.Join(",", productIds)} from {_installation.Url}");
      IEnumerable<Product> products = Enumerable.Empty<Product>();

      foreach (var obtainer in Obtainers())
      {
        await obtainer.Get(productIds);
      }
    }


    private async Task Initialise()
    {
      int total = await _productHttp.GetTotalProductCountAsync();
      _importSummary.UpdateProductsAtSource(total);
    }

    private IEnumerable<IObtainer> Obtainers()
    {
      IObtainer initial = new InitialProductObtainer(_httpClient, _installation, _logger);
      IObtainer variable = new VariationProductObtainer(_httpClient, _installation, _logger);

      return [initial, variable];
    }

  }
}
