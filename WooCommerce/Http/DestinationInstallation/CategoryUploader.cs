using Newtonsoft.Json;
using System;
using System.Net.Http.Headers;
using System.Text;
using WooCommerce.Http.Media;
using WooCommerce.Http.SourceInstallation.Categories;

namespace WooCommerce.Http.DestinationInstallation
{
  public class CategoryUploader
  {

    HttpClient _httpClient;
    WordPressInstallation _installation;
    MediaUploader _mediaUploader;

    public CategoryUploader(HttpClient httpClient, WordPressInstallation installation)
    {
      _httpClient = httpClient;
      _installation = installation;
      _mediaUploader = new MediaUploader(httpClient, installation);
    }

    private async Task<CategoryUploaded> AddCategory(CategorySource category, int parent)
      => await CategoryHttp(HttpMethod.Post, category, 
        parent, $"{_installation.Url}/wp-json/wc/v3/products/categories");

    private async Task<CategoryUploaded> CategoryHttp(HttpMethod method, CategorySource category, 
      int parent, string apiUrl)
    {
      CategoryUploaded categoryUploaded = new CategoryUploaded();

      var credentials = $"{_installation.Key}:{_installation.Secret}";
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
          categoryUploaded = JsonConvert.DeserializeObject<CategoryUploaded>(result);
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


    private object BuildCategoryPayload(CategorySource category, int parent)
    {
      return new
      {
        name = category.name,
        slug = category.slug,
        description = category.description,
        parent = parent,
        display = category.display,
        menu_order = category.menu_order,
        image = category.image != null
                ? new
                {
                  src = category.image.src,
                  alt = category.image.alt,
                  name = category.name
                }
                : null
      };
    }

    public async Task<List<CategorySource>> CategoryExists(string slug)
    {
      var requestUri = $"{_installation.Url}/wp-json/wc/v3/products/categories?slug={slug}";

      using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

      var credentials = $"{_installation.Key}:{_installation.Secret}";
      var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
      request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      var responseBody = await response.Content.ReadAsStringAsync();

      List<CategorySource> categories = JsonConvert.DeserializeObject<List<CategorySource>>(responseBody);

      return categories;
    }

    private int CategoryParent(List<CategoryMap> map, CategorySource source)
    {
      if (map.Any(m => m.CategorySource.id == source.parent))
      {
        return map.FirstOrDefault(m => m.CategorySource.id == source.parent).CategoryUploaded.id;
      }
      else
      {
        return 0;
      }
    }

    public async Task FirstPass(List<CategorySource> categories)
    {
      List<CategoryNode> nodes = categories.BuildCategoryTree();
      List<CategoryMap> map = new List<CategoryMap>();

      foreach (var node in nodes)
      {
        await UploadNode(node, map, 0);
      }
    }

    private bool MediaFilesEquivalent(string url1, string url2)
    {
      if (url1 == null && url2 == null)
        return true;

      Uri uri1 = new Uri(url1);
      Uri uri2 = new Uri(url2);

      string pathAndQuery1 = uri1.Segments.Last();
      string pathAndQuery2 = uri2.Segments.Last();

      return pathAndQuery1 == pathAndQuery2;
    }

    private async Task<CategoryUploaded> UpdateCategory(CategorySource categoryToUpload, CategorySource categoryUploaded, int parent)
      => await CategoryHttp(HttpMethod.Put, categoryToUpload, parent, 
        $"{_installation.Url}/wp-json/wc/v3/products/categories/{categoryUploaded.id}");

    public async Task Upload(List<CategorySource> categories)
    {
      await FirstPass(categories);
    }


    private async Task UploadNode(CategoryNode node, List<CategoryMap> map, int parent)
    {
      int next_parent = 0;
      List<CategorySource> existingUploadedCategory = await CategoryExists(node.Category.slug);

      if(node.Category.name.ToLower().Contains("export"))
      {

      }

      if (!existingUploadedCategory.Any())
      {
        CategoryUploaded uploaded = await AddCategory(node.Category, parent);

        map.Add(new CategoryMap
        {
          CategorySource = node.Category,
          CategoryUploaded = uploaded
        });

        next_parent = uploaded.id;
      }
      else
      {
        await UpdateCategory(existingUploadedCategory.First(), node.Category);
        next_parent = existingUploadedCategory.First().id;
      }

      foreach (var child in node.Children)
      {
        await UploadNode(child, map, next_parent);
      }
    }

    private async Task UpdateCategory(CategorySource uploadedCategory, CategorySource sourceCategory)
    {
      if (!UploadSuccessfull(uploadedCategory, sourceCategory))
      {
        await UpdateCategory(sourceCategory, uploadedCategory, 0);
      }
    }


    private bool UploadSuccessfull(CategorySource uploadedCategory, CategorySource sourceCategory)
    {

      if (uploadedCategory.name != sourceCategory.name)
      {
        return false;
      }
      else if (uploadedCategory.slug != sourceCategory.slug)
      {
        return false;
      }
      else if (uploadedCategory.description != sourceCategory.description)
      {
        return false;
      }
      else if (uploadedCategory.display != sourceCategory.display)
      {
        return false;
      }
      else if (uploadedCategory.menu_order != sourceCategory.menu_order)
      {
        return false;
      }
      else if (!MediaFilesEquivalent(uploadedCategory.image?.src, sourceCategory.image?.src))
      {
        return false;
      }

      return true;
    }

  }

}
