using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using WooCommerce.Http.SourceInstallation.Structures;

namespace WooCommerce.Http.Products.Fetching
{
  public class ProductFetchingHttp
  {
    private readonly WordPressInstallation _installation;
    private readonly HttpClient _httpClient;
    private readonly ProductFetchingHttp _productHttp;


    public ProductFetchingHttp(WordPressInstallation installation, HttpClient httpClient)
    {
      _installation = installation;
      _httpClient = httpClient;

    }


    public async Task<IEnumerable<Product>> GetProducts(int page, int per_page)
    {
      string responseBody = "";
      var request = new HttpRequestMessage(HttpMethod.Get, $"{_installation.Url}/wp-json/wc/v3/products?per_page={per_page}&page={page}");

      var plainCredentials = $"{_installation.Key}:{_installation.Secret}";
      var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(plainCredentials));
      var authHeader = "Basic " + base64Credentials;
      request.Headers.Add("Authorization", authHeader);

      var content = new StringContent("", null, "application/json");
      request.Content = content;
      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      responseBody = await response.Content.ReadAsStringAsync();

      List<Product> productList = JsonConvert.DeserializeObject<List<Product>>(responseBody);

      return productList;
    }


    public async Task<int> GetTotalProductCountAsync()
    {
      using (var httpClient = new HttpClient())
      {
        var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_installation.Key}:{_installation.Secret}"));
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

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


    public async Task<IReadOnlyList<Product>> GetProducts(IEnumerable<int> productIds, CancellationToken ct = default)
    {
      var ids = productIds?.Distinct().ToArray() ?? Array.Empty<int>();
      if (ids.Length == 0) return Array.Empty<Product>();

      // WooCommerce REST: per_page max is 100; use batches + include=
      const int BatchSize = 100;

      // Basic auth: username = consumer_key, password = consumer_secret
      var plain = $"{_installation.Key}:{_installation.Secret}";
      var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(plain));

      var all = new List<Product>(ids.Length);

      for (int i = 0; i < ids.Length; i += BatchSize)
      {
        var batch = ids.Skip(i).Take(BatchSize).ToArray();
        var include = string.Join(",", batch);

        var url =
            $"{_installation.Url}/wp-json/wc/v3/products" +
            $"?include={include}&orderby=include&per_page={Math.Min(BatchSize, batch.Length)}";

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var resp = await _httpClient.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        var products = JsonConvert.DeserializeObject<List<Product>>(json);
        if (products != null) all.AddRange(products);
      }

      return all;
    }


    public async Task<IEnumerable<Variation>> GetVariations(int productId)
    {
      var requestUrl = $"{_installation.Url}/wp-json/wc/v3/products/{productId}/variations";

      var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_installation.Key}:{_installation.Secret}"));
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

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
