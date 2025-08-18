using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using WooCommerce.Http;
using WooCommerce.Http.SourceInstallation.Structures;
using WooCommerce.Repositories.Products;
using WooCommerce.Synchronising.Fetchers.Products;

namespace WooCommerce.Synchronising.Fetching.Products.Obtainers.AllProducts
{
  public class VariationProductObtainer : IObtainer
  {
    private readonly HttpClient _httpClient;
    private readonly WordPressInstallation _installation;
    private readonly int _maxParallelRequests;
    private readonly ProductRepository _productRepository;
    private readonly ILogger _logger;
    private readonly ProductHttp _productHttp;

    private const string VARIABLE = "variable";

    public VariationProductObtainer(HttpClient httpClient, WordPressInstallation installation,
      ILogger logger,
      int maxParallelRequests = 10)
    {
      _httpClient = httpClient;
      _installation = installation;
      _maxParallelRequests = maxParallelRequests;
      _productRepository = new ProductRepository();
      _logger = logger;
      _productHttp = new ProductHttp(_installation, _httpClient);
    }

    public async Task Get()
    {
      int total = 0;

      var unprocessedVariations = _productRepository.GetAllUnprocessedVariations().ToList();

      int unprocessed = unprocessedVariations.Count();

      if(unprocessed == 0)
      {
        _logger.LogInformation($"{_productRepository.GetAllVariations().Count()} variations saved - no more to fetch");
        return;
      }

      _logger.LogInformation($"{unprocessed} variations to update");

      var throttler = new SemaphoreSlim(_maxParallelRequests);

      var tasks = unprocessedVariations.Select(async variation =>
      {
        await throttler.WaitAsync();
        try
        {
          var variationDetails = await _productHttp.GetVariations(variation.Product.id);
          variation.Product.variationDetails = variationDetails;
          variation.VariationAdded = true;

          _productRepository.SaveProduct(variation);
          total++;
          _logger.LogInformation($"{total} variable products updated, {unprocessed - total} left");

        }
        catch (Exception ex)
        {
          Console.WriteLine($"Failed to fetch/save variation for product {variation.Product.id}: {ex.Message}");
        }
        finally
        {
          throttler.Release();
        }
      });

      await Task.WhenAll(tasks);
    }


    public Task Get(IEnumerable<int> productIds) => Get();



    public Task Get(int startAt) => Get();

  


  }
}
