using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WooCommerce.Http;
using WooCommerce.Http.DestinationInstallation;
using WooCommerce.Http.SourceInstallation.Structures;

namespace WooCommerce.Workers
{
  public class ProductUploader
  {
    private readonly HttpClient _httpClient;
    private readonly WordPressInstallation _installation;

    public ProductUploader(HttpClient httpClient, WordPressInstallation installation)
    {
      _httpClient = httpClient;
      _installation = installation;
    }

    private StringContent ProductAsString(Product product)
    {
      var productData = new
      {
        attributes = product.attributes,
        backorders_allowed = product.backorders_allowed,
        catalog_visibility = product.catalog_visibility,
        cross_sell_ids = product.cross_sell_ids,
        date_created = product.date_created,
        date_created_gmt = product.date_created,    //if  date_created_gmt doesnt work
        date_on_sale_from = product.date_on_sale_from,
        date_on_sale_from_gmt = product.date_on_sale_from_gmt,
        description = product.description,
        dimensions = product.dimensions,
        featured = product.featured,
        images = product.images.Select(i => new { i.name, i.alt, i.src}),
        low_stock_amount = product.low_stock_amount,
        manage_stock = product.manage_stock,
        meta_data = product.meta_data,
        menu_order = product.menu_order,
        name = product.name,
        password = product.post_password,
        purchase_note = product.purchase_note,
        regular_price = product.regular_price,
        reviews_allowed = product.reviews_allowed,
        sale_price = product.sale_price,
        short_description = product.short_description,
        shipping_class = product.shipping_class,
        sku = product.sku,
        slug = product.slug,
        sold_individually = product.sold_individually,
        stock_quantity = product.stock_quantity,
        status = product.status,
        stock_status = product.stock_status,
        type = product.type,
        upsell_ids = product.upsell_ids,
        weight = product.weight
      };

      var json = System.Text.Json.JsonSerializer.Serialize(productData, new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
      });

      StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

      return content;
    }


    public async Task<int> ProductExistsBySku(string sku)
    {
      var apiUrl = $"{_installation.Url}/wp-json/wc/v3/products?sku={sku}&consumer_key={_installation.Key}&consumer_secret={_installation.Secret}";

      var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_installation.Key}:{_installation.Secret}"));
      _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

      var response = await _httpClient.GetAsync(apiUrl);

      if (response.IsSuccessStatusCode)
      {
        var resultJson = await response.Content.ReadAsStringAsync();
        var products = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(resultJson);

        if (products != null && products.Count > 0)
        {
          if (products[0].TryGetValue("id", out var idValue) && int.TryParse(idValue.ToString(), out int productId))
          {
            return productId;
          }
        }

        return 0;
      }
      else
      {
        throw new Exception($"Error checking product: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
      }
    }



    public async Task Upload(IEnumerable<Product> products)
    {

      foreach (var product in products)
      {
        if (product.HttpVariations.Any())
        {
          await UploadVariableProduct(product);
        }
        else
        {
          await UploadSingleProduct(product);
        }
      }

    }


    public async Task<IEnumerable<int>> UploadImages(IEnumerable<ProductImage> images)
    {
      var uploadedIds = new List<int>();
      var semaphore = new SemaphoreSlim(1); // limit to 5 concurrent uploads
      var tasks = new List<Task>();

      foreach (var image in images)
      {
        await semaphore.WaitAsync();

        var task = Task.Run(async () =>
        {
          try
          {
            using (var httpClient = new HttpClient())
            {
              var username = _installation.WordPressAPIUser.Username; // WordPress admin username
              var appPassword = _installation.WordPressAPIUser.password; // WordPress Application Password
              var byteArray = Encoding.ASCII.GetBytes($"{username}:{appPassword}");
              httpClient.DefaultRequestHeaders.Authorization =
                  new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

              var imageData = await httpClient.GetByteArrayAsync(image.src);

              using (var content = new MultipartFormDataContent())
              {
                var imageContent = new ByteArrayContent(imageData);
                imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                content.Add(imageContent, "file", image.name);

                if (!string.IsNullOrEmpty(image.alt))
                {
                  content.Add(new StringContent(image.alt), "alt_text");
                }

                var apiUrl = $"{_installation.Url}/wp-json/wp/v2/media";
                var response = await httpClient.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(jsonResponse);
                var mediaId = jsonDoc.RootElement.GetProperty("id").GetInt32();

                lock (uploadedIds)
                {
                  uploadedIds.Add(mediaId);
                }
              }
            }
          }
          catch (Exception ex)
          {
            Console.WriteLine($"Error uploading image {image.name}: {ex.Message}");
            throw;
          }
          finally
          {
            semaphore.Release();
          }
        });

        tasks.Add(task);
      }

      await Task.WhenAll(tasks);

      return uploadedIds;
    }


    private async Task UploadSingleProduct(Product product)
    {
      int id = await ProductExistsBySku(product.sku);

      if(product.type != "simple")
      {

      }

      if (id != 0)
      {
        await UploadSingleProductExisting(product, id);
      }
      else
      {
        await UploadSingleProductNew(product);
      }

    }

    private async Task UploadSingleProductExisting(Product product, int id)
    {
      var apiUrl = $"{_installation.Url}/wp-json/wc/v3/products/{id}?consumer_key={_installation.Key}&consumer_secret={_installation.Secret}";

      var content = ProductAsString(product);
      var response = await _httpClient.PutAsync(apiUrl, content);

      if (response.IsSuccessStatusCode)
      {
        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine("✅ Product updated successfully:");
        Console.WriteLine(result);
      }
      else
      {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"❌ Error creating product: {response.StatusCode}");
        Console.WriteLine(error);
      }
    }


    public async Task<bool> SetProductPassword(int productId, string newPassword)
    {
      try
      {
        // Create the request payload
        var payload = new
        {
          status = "publish",
          password = newPassword
        };

        var jsonPayload = JsonConvert.SerializeObject(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_installation.WordPressAPIUser.Username}:{_installation.WordPressAPIUser.password}"));
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);

        var response = await _httpClient.PostAsync($"{_installation.Url}/wp-json/wc/v3/products/{productId}", content);

        if (response.IsSuccessStatusCode)
        {
          return true;
        }
        else
        {
          var errorContent = await response.Content.ReadAsStringAsync();
          Console.WriteLine($"Failed to set product password. Status code: {response.StatusCode}, Error: {errorContent}");
          return false;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error setting product password: {ex.Message}");
        return false;
      }
    }

    /*
     
    add_filter('woocommerce_rest_prepare_product', function ($response, $post, $request) {
    $response->data['post_password'] = $post->post_password;
    return $response;
}, 10, 3);

add_action('woocommerce_rest_insert_product', function ($post, $request, $creating) {
    if (isset($request['post_password'])) {
        wp_update_post([
            'ID' => $post->get_id(),
            'post_password' => sanitize_text_field($request['post_password'])
        ]);
    }
}, 10, 3);

    */

    private async Task<ProductUploaded> UploadSingleProductNew(Product product)
    {
      ProductUploaded productUploaded = null;
      var apiUrl = $"{_installation.Url}/wp-json/wc/v3/products?consumer_key={_installation.Key}&consumer_secret={_installation.Secret}";

      var content = ProductAsString(product);
      string contentAsString = await content.ReadAsStringAsync();

      var response = await _httpClient.PostAsync(apiUrl, content);

      if (response.IsSuccessStatusCode)
      {
        var result = await response.Content.ReadAsStringAsync();
        productUploaded = ProductUploaded.Deserialize(result);

        if (!string.IsNullOrWhiteSpace(product.post_password))
        {
          await SetProductPassword(productUploaded.id, product.post_password);
        }

        Console.WriteLine($"{product.name} created successfully");
      }
      else
      {
        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Error creating product {product.name}");
      }

      return productUploaded;
    }


    public async Task UploadVariableProduct(Product product)
    {
      ProductUploaded uploaded = await UploadSingleProductNew(product);

      if(uploaded != null)
      {
        foreach (var variation in product.HttpVariations)
        {
          await UploadVariation(uploaded, variation);
        }
      }
    }

    private async Task UploadVariation(ProductUploaded uploaded, Variation variation)
    {
      var apiUrl = $"{_installation.Url}/wp-json/wc/v3/products/{uploaded.id}/variations?consumer_key={_installation.Key}&consumer_secret={_installation.Secret}";

      using (var httpClient = new HttpClient())
      {
        VariationUpload variationUpload = VariationUpload.Make(variation);
        var variationJson = variationUpload.Serialize();
        var content = new StringContent(variationJson, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(apiUrl, content);

        if (response.IsSuccessStatusCode)
        {
          var jsonResponse = await response.Content.ReadAsStringAsync();
          var jsonDoc = JsonDocument.Parse(jsonResponse);
          var variationId = jsonDoc.RootElement.GetProperty("id").GetInt32();

          Console.WriteLine($"Variation created successfully (ID: {variationId})");
        }
        else
        {
          var error = await response.Content.ReadAsStringAsync();
          Console.WriteLine($"Error creating variation: {error}");
          throw new Exception($"Failed to create variation: {error}");
        }
      }
    }
  }
}
