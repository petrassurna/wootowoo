using Microsoft.Extensions.Logging;
using System.Diagnostics;
using WooCommerce.Http;
using WooCommerce.Http.SourceInstallation;
using WooCommerce.Http.SourceInstallation.Structures;
using WooCommerce.Repositories.Summary;
using WooCommerce.Synchronising.Fetchers.Products.Obtainers;

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
    private readonly ProductHttp _productHttp;

    public ProductFetcher(HttpClient httpClient, WordPressInstallation installation, ILogger logger)
    {
      _httpClient = httpClient;
      _installation = installation;
      _importSummary = new ImportSummaryRepository();
      _logger = logger;
      _productHttp = new ProductHttp(installation, httpClient);
    }


    public async Task Fetch()
    {
      await Initialise();
      Console.WriteLine($"Getting products from {_installation.Url}");
      IEnumerable<Product> products = Enumerable.Empty<Product>();

      var sw = Stopwatch.StartNew();

      foreach (var obtainer in Obtainers())
      {
        await obtainer.Get();
      }

      sw.Stop();
      //Console.WriteLine("Elapsed ms: " + sw.ElapsedMilliseconds);

      //return products;
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
