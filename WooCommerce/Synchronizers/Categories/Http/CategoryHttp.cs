using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WooCommerce.Synchronizers.Categories.Structures.Destination;
using WooCommerce.Synchronizers.Categories.Structures.Origin;
using WooCommerce.Http;

namespace WooCommerce.Synchronizers.Categories.Http
{
  public class CategoryHttp
  {
    HttpClient _httpClient;
    WordPressInstallation _destination;

    public CategoryHttp(HttpClient httpClient, WordPressInstallation destination)
    {
      _httpClient = httpClient;
      _destination = destination;
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
                  category.image.src,
                  category.image.alt,
                  category.name
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

      var credentials = $"{_destination.Key}:{_destination.Secret}";
      var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

      using var request = new HttpRequestMessage(HttpMethod.Put, $"{_destination.Url}/wp-json/wc/v3/products/categories/{category.id}");
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
          Console.WriteLine($"❌ Failed to create category: {response.StatusCode}");
          var error = await response.Content.ReadAsStringAsync();
          Console.WriteLine(error);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"🚨 Error: {ex.Message}");
      }

      return categoryUploaded;
    }


    public async Task<List<CategorySource>> ExistingCategories(string slug)
    {
      var requestUri = $"{_destination.Url}/wp-json/wc/v3/products/categories?slug={slug}";

      using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

      var credentials = $"{_destination.Key}:{_destination.Secret}";
      var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
      request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      var responseBody = await response.Content.ReadAsStringAsync();

      List<CategorySource> categories = JsonConvert.DeserializeObject<List<CategorySource>>(responseBody);

      return categories;
    }

    public async Task<CategoryClassesDestination> UploadCategory(HttpMethod method, CategorySource category,
      int parent, string apiUrl)
    {
      CategoryClassesDestination categoryUploaded = new CategoryClassesDestination();

      var credentials = $"{_destination.Key}:{_destination.Secret}";
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
          Console.WriteLine($"❌ Failed to create category: {response.StatusCode}");
          var error = await response.Content.ReadAsStringAsync();
          Console.WriteLine(error);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"🚨 Error: {ex.Message}");
      }

      return categoryUploaded;
    }

    public async Task<bool> UpdateCategoryParent(int categoryId, int newParentId)
    {
      var apiUrl = $"{_destination.Url}/wp-json/wc/v3/products/categories/{categoryId}";

      var credentials = $"{_destination.Key}:{_destination.Secret}";
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
