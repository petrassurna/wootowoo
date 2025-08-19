using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using WooCommerce.Http;
using WooCommerce.Synchronising.Fetchers.Categories.Structures;
using WooCommerce.Synchronising.Fetching.Categories.Structures;

namespace WooCommerce.Synchronising.Fetching.Categories
{
  public class CategoryPusherHttp
  {
    HttpClient _httpClient;
    WordPressInstallation _source;
    ILogger _logger;
    SemaphoreSlim _semaphore;
    int _requestDelayMs;

    public CategoryPusherHttp(HttpClient httpClient, WordPressInstallation destination, ILogger logger, int maxConcurrency = 3, int requestDelayMs = 100)
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
