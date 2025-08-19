using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using WooCommerce.Http;
using WooCommerce.Synchronising.Fetchers.Categories.Structures;
using WooCommerce.Synchronising.Fetching.Categories.Structures;

namespace WooCommerce.Synchronising.Fetching.Categories
{
  public class CategoryFetcherHttp
  {
    HttpClient _httpClient;
    WordPressInstallation _source;
    ILogger _logger;
    SemaphoreSlim _semaphore;
    int _requestDelayMs;

    public CategoryFetcherHttp(HttpClient httpClient, WordPressInstallation destination, ILogger logger, int maxConcurrency = 3, int requestDelayMs = 100)
    {
      _httpClient = httpClient;
      _source = destination;
      _logger = logger;
      _semaphore = new SemaphoreSlim(maxConcurrency);
      _requestDelayMs = requestDelayMs;
    }


    private object BuildCategoryPayload(CategorySource category, int parent)
    {
      return new
      {
        category.name,
        category.slug,
        category.description,
        parent,
        category.display,
        category.menu_order,
        image = category.image != null
                ? new
                {
                  category.image.src
                }
                : null
      };
    }


    private object BuildCategoryPayload(CategorySourceNoImage category, int parent)
    {
      return new
      {
        category.name,
        category.slug,
        category.description,
        parent,
        category.display,
        category.menu_order,
        image = category.imageId > 0
            ? new { id = category.imageId }
            : null
      };
    }


    public async Task<CategoryClassesDestination> CategoryUpdateHttp(CategorySourceNoImage category,
      int parent, string apiUrl)
    {
      CategoryClassesDestination categoryUploaded = new CategoryClassesDestination();

      var credentials = $"{_source.Key}:{_source.Secret}";
      var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

      using var request = new HttpRequestMessage(HttpMethod.Put, $"{_source.Url}/wp-json/wc/v3/products/categories/{category.id}");
      request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

      var newCategory = BuildCategoryPayload(category, parent);

      var json = JsonConvert.SerializeObject(newCategory);
      request.Content = new StringContent(json, Encoding.UTF8, "application/json");

      try
      {
        using var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
          var result = await response.Content.ReadAsStringAsync();
          categoryUploaded = JsonConvert.DeserializeObject<CategoryClassesDestination>(result);
        }
        else
        {
          _logger.LogInformation($"❌ Failed to create category: {response.StatusCode}");
          var error = await response.Content.ReadAsStringAsync();
          _logger.LogInformation(error);
        }
      }
      catch (Exception ex)
      {
        _logger.LogInformation($"🚨 Error: {ex.Message}");
      }

      return categoryUploaded;
    }



    public async Task<List<CategorySource>> ExistingCategories(string slug)
    {

      string baseUrl = _source.Url.Trim().TrimEnd('/');

      var requestUri = $"{baseUrl}/wp-json/wc/v3/products/categories?slug={slug}";

      using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

      var credentials = $"{_source.Key}:{_source.Secret}";
      var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
      request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      var responseBody = await response.Content.ReadAsStringAsync();

      List<CategorySource> categories = JsonConvert.DeserializeObject<List<CategorySource>>(responseBody);

      return categories;
    }


    public async Task<int> GetCategoryCount(CancellationToken ct = default)
    {
      var url = $"{_source.Url.TrimEnd('/')}/wp-json/wc/v3/products/categories?per_page=1";

      using var req = new HttpRequestMessage(HttpMethod.Get, url);

      // UA: some hosts reject requests without it
      req.Headers.UserAgent.ParseAdd("WooToWoo/1.0");

      // WooCommerce accepts consumer key/secret via Basic auth
      var token = Convert.ToBase64String(
          Encoding.ASCII.GetBytes($"{_source.Key}:{_source.Secret}")
      );
      req.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);

      using var resp = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
      resp.EnsureSuccessStatusCode();

      if (resp.Headers.TryGetValues("X-WP-Total", out var values) &&
          int.TryParse(values.FirstOrDefault(), out var total))
      {
        return total;
      }

      throw new InvalidOperationException("WooCommerce did not return X-WP-Total header.");
    }





    public async Task<CategoryClassesDestination> UploadCategory(HttpMethod method, CategorySource category,
      int parent, string apiUrl)
    {
      CategoryClassesDestination categoryUploaded = new CategoryClassesDestination();

      var credentials = $"{_source.Key}:{_source.Secret}";
      var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

      using var request = new HttpRequestMessage(method, apiUrl);
      request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

      var newCategory = BuildCategoryPayload(category, parent);

      var json = JsonConvert.SerializeObject(newCategory);
      request.Content = new StringContent(json, Encoding.UTF8, "application/json");

      try
      {
        using var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
          var result = await response.Content.ReadAsStringAsync();
          categoryUploaded = JsonConvert.DeserializeObject<CategoryClassesDestination>(result);
        }
        else
        {
          _logger.LogInformation($"❌ Failed to create category: {response.StatusCode}");
          var error = await response.Content.ReadAsStringAsync();
          _logger.LogInformation(error);
        }
      }
      catch (Exception ex)
      {
        _logger.LogInformation($"🚨 Error: {ex.Message}");
      }

      return categoryUploaded;
    }

    public async Task<bool> UpdateCategoryParent(int categoryId, int newParentId)
    {
      var apiUrl = $"{_source.Url}/wp-json/wc/v3/products/categories/{categoryId}";

      var credentials = $"{_source.Key}:{_source.Secret}";
      var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

      using var httpClient = new HttpClient();
      httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

      var payload = new
      {
        parent = newParentId
      };

      var json = JsonConvert.SerializeObject(payload);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      var response = await httpClient.PutAsync(apiUrl, content);

      if (response.IsSuccessStatusCode)
      {
        return true;
      }
      else
      {
        var error = await response.Content.ReadAsStringAsync();
        return false;
      }
    }

  }
}
