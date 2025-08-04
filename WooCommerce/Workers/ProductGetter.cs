using System.Diagnostics;
using System.Text;
using WooCommerce.Http;
using WooCommerce.Http.SourceInstallation;
using WooCommerce.Http.SourceInstallation.Obtainers;
using WooCommerce.Http.SourceInstallation.Structures;
using WooCommerce.Repositories.Summary;

namespace WooCommerce.Workers
{
  /// <summary>
  /// Get products in batches 
  /// Depending on type, het further data eg Variations
  /// </summary>
  public class ProductGetter
  {

    HttpClient _httpClient;
    WordPressInstallation _installation;
    ImportSummaryRepository _importSummary;

    public ProductGetter(HttpClient httpClient, WordPressInstallation installation)
    {
      _httpClient = httpClient;
      _installation = installation;
      _importSummary = new ImportSummaryRepository();
    }


    public async Task<IEnumerable<Product>> GetAllProducts(int startAt)
    {
      await Initialise();
      IEnumerable<Product> products = Enumerable.Empty<Product>();

      var sw = Stopwatch.StartNew();

      foreach (var obtainer in Obtainers())
      {
        await obtainer.Get(startAt);
      }

      sw.Stop();
      Console.WriteLine("Elapsed ms: " + sw.ElapsedMilliseconds);

      return products;
    }

    public async Task<IEnumerable<Product>> GetAllProducts()
    {
      await Initialise();
      IEnumerable<Product> products = Enumerable.Empty<Product>();

      var sw = Stopwatch.StartNew();

      foreach (var obtainer in Obtainers())
      {
        await obtainer.Get();
      }

      sw.Stop();
      Console.WriteLine("Elapsed ms: " + sw.ElapsedMilliseconds);

      return products;
    }

    private async Task Initialise()
    {
      int total = await GetTotalProductCountAsync();
      _importSummary.UpdateProductsAtSource(total);
    }


    public async Task<int> GetTotalProductCountAsync()
    {
      using (var httpClient = new HttpClient())
      {
        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_installation.Key}:{_installation.Secret}"));
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);

        string requestUrl = $"{_installation.Url}/wp-json/wc/v3/products?per_page=1&page=1";

        var response = await httpClient.GetAsync(requestUrl);
        response.EnsureSuccessStatusCode();

        // WooCommerce returns total count in the headers
        if (response.Headers.Contains("X-WP-Total"))
        {
          var totalHeader = response.Headers.GetValues("X-WP-Total").FirstOrDefault();
          if (int.TryParse(totalHeader, out int total))
          {
            return total;
          }
        }

        throw new Exception("Total count not found in response headers.");
      }
    }


    private IEnumerable<IObtainer> Obtainers()
    {
      IObtainer initial = new InitialProductObtainer(_httpClient, _installation);
      IObtainer variable = new VariationProductObtainer(_httpClient, _installation);

      return [initial, variable];
    }

  }
}
