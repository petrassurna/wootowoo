using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;
using WooCommerce.Http;
using WooCommerce.Http.SourceInstallation;
using WooCommerce.Http.SourceInstallation.Structures;
using WooCommerce.Repositories.Products;

namespace WooCommerce.Synchronising.Fetchers.Products.Obtainers
{
  public class VariationProductObtainer : IObtainer
  {
    private readonly HttpClient _httpClient;
    private readonly WordPressInstallation _installation;
    private readonly int _maxParallelRequests;
    private readonly ProductRepository _productRepository;
    private readonly ILogger _logger;

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

      // Use parallelism with throttling
      var throttler = new SemaphoreSlim(_maxParallelRequests);

      var tasks = unprocessedVariations.Select(async variation =>
      {
        await throttler.WaitAsync();
        try
        {
          var variationDetails = await GetVariations(variation.Product.id);
          variation.Product.variationDetails = variationDetails;
          variation.VariationAdded = true;

          //problem here with database in use????
          _productRepository.SaveProduct(variation);
          total++;
          _logger.LogInformation($"{total} variable products updated, {unprocessed - total} left");

        }
        catch (Exception ex)
        {
          Console.WriteLine($"Failed to fetch/save variation for product {variation.Product.id}: {ex.Message}");
          // Optionally log or store failure
        }
        finally
        {
          throttler.Release();
        }
      });

      await Task.WhenAll(tasks);
    }


    public Task Get(int startAt) => Get();

    private async Task<IEnumerable<Variation>> GetVariations(int productId)
    {
      var requestUrl = $"{_installation.Url}/wp-json/wc/v3/products/{productId}/variations";

      var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_installation.Key}:{_installation.Secret}"));
      _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

      var response = await _httpClient.GetAsync(requestUrl);

      if (!response.IsSuccessStatusCode)
      {
        var error = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error fetching variations for product {productId}: {response.StatusCode}, {error}");
      }

      var responseBody = await response.Content.ReadAsStringAsync();

      if (string.IsNullOrWhiteSpace(responseBody) || responseBody.Trim() == "null")
      {
        return Enumerable.Empty<Variation>();
      }

      return JsonConvert.DeserializeObject<List<Variation>>(responseBody) ?? new List<Variation>();
    }



  }
}
