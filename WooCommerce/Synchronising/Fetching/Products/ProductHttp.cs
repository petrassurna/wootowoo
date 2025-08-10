using Newtonsoft.Json;
using System.Text;
using WooCommerce.Http;
using WooCommerce.Http.SourceInstallation.Structures;

namespace WooCommerce.Synchronising.Fetchers.Products
{
  public class ProductHttp
  {
    private readonly WordPressInstallation _installation;
    private readonly HttpClient _httpClient;
    private readonly ProductHttp _productHttp;


    public ProductHttp(WordPressInstallation installation, HttpClient httpClient)
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


  }
}
