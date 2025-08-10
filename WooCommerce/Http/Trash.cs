using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace WooCommerce.Http
{
  public class Trash
  {
    public static async Task EmptyWooCommerceTrashAsync(WordPressInstallation installation, ILogger logger)
    {
      var baseUrl = $"{installation.Url}/wp-json/wc/v3/products";
      var httpClient = new HttpClient();

      var byteArray = Encoding.ASCII.GetBytes($"{installation.Key}:{installation.Secret}");
      httpClient.DefaultRequestHeaders.Authorization =
          new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

      int page = 1;
      bool morePages = true;

      while (morePages)
      {

        var response = await httpClient.GetAsync($"{baseUrl}?status=trash&per_page=100&page={page}");
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<List<Product>>(jsonResponse);

        if (products.Count == 0)
        {
          morePages = false;
          break;
        }

        foreach (var product in products)
        {
          logger.LogInformation($"Deleting trashed product: {product.name} (ID: {product.id})");

          var deleteResponse = await httpClient.DeleteAsync($"{baseUrl}/{product.id}?force=true");
          if (deleteResponse.IsSuccessStatusCode)
          {
            logger.LogInformation($"Permanently deleted product ID {product.id}");
          }
          else
          {
            var error = await deleteResponse.Content.ReadAsStringAsync();
            logger.LogInformation($"Failed to delete product ID {product.id}: {error}");
          }
        }

        page++;
      }

      logger.LogInformation("All trashed products permanently deleted.");
    }

    public class Product
    {
      public int id { get; set; }
      public string name { get; set; }
    }


  }



}
