using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Text;
using WooCommerce.Http.Media;
using WooCommerce.Http.SourceInstallation.Categories;
using WooCommerce.Repositories.Category;

namespace WooCommerce.Http.DestinationInstallation
{
  public class CategoryUploader
  {

    HttpClient _httpClient;
    WordPressInstallation _destination;
    MediaUploader _mediaUploader;
    CategoryRepository _categoryRepository;

    public CategoryUploader(HttpClient httpClient, WordPressInstallation destination)
    {
      _httpClient = httpClient;
      _destination = destination;
      _mediaUploader = new MediaUploader(httpClient, destination);
      _categoryRepository = new CategoryRepository();
    }

    private async Task<CategoryUploaded> AddCategory(CategorySource category)
      => await CategoryHttp(HttpMethod.Post, category,
        0, $"{_destination.Url}/wp-json/wc/v3/products/categories");

    private async Task<CategoryUploaded> CategoryHttp(HttpMethod method, CategorySource category,
      int parent, string apiUrl)
    {
      CategoryUploaded categoryUploaded = new CategoryUploaded();

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


    private async Task<CategoryUploaded> CategoryUpdateHttp(CategorySourceNoImage category,
      int parent, string apiUrl)
    {
      CategoryUploaded categoryUploaded = new CategoryUploaded();

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


    private object BuildCategoryPayload(CategorySourceNoImage category, int parent)
    {
      return new
      {
        name = category.name,
        slug = category.slug,
        description = category.description,
        parent = parent,
        display = category.display,
        menu_order = category.menu_order,
        image = category.imageId > 0
            ? new { id = category.imageId }
            : null
      };
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


    private bool MediaFilesEquivalent(MediaFile file1, MediaFile file2)
    {
      if (file1 == null && file2 == null)
        return true;
      else if (file1 == null && file2 != null)
        return false;
      else if (file1 != null && file2 == null)
        return false;
      else
        return MediaFilesEquivalent(file1.src, file2.src);


    }



    private bool MediaFilesEquivalent(string url1, string url2)
    {
      if (url1 == null && url2 == null)
        return true;

      if (!IsValidWebUri(url1) || !IsValidWebUri(url2))
      {
        return false;
      }

      Uri uri1 = new Uri(url1);
      Uri uri2 = new Uri(url2);

      string pathAndQuery1 = uri1.Segments.Last();
      string pathAndQuery2 = uri2.Segments.Last();

      return pathAndQuery1 == pathAndQuery2;
    }


    public static bool IsValidWebUri(string input)
    {
      return Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult) &&
             (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

    private async Task<CategoryUploaded> UpdateCategory(CategorySource categoryToUpload, CategorySource categoryUploaded, int parent)
      => await CategoryHttp(HttpMethod.Put, categoryToUpload, parent,
        $"{_destination.Url}/wp-json/wc/v3/products/categories/{categoryUploaded.id}");


    private bool UpdateRequired(CategorySource newCategory, CategorySource existingUploadedCategory)
    {
      if (newCategory.name != existingUploadedCategory.name)
      {
        return true;
      }
      else if (newCategory.slug != existingUploadedCategory.slug)
      {
        return true;
      }
      else if (newCategory.description != existingUploadedCategory.description)
      {
        return true;
      }
      else if (newCategory.display != existingUploadedCategory.display)
      {
        return true;
      }
      else if (newCategory.menu_order != existingUploadedCategory.menu_order)
      {
        return true;
      }
      else if (!MediaFilesEquivalent(newCategory.image, existingUploadedCategory.image))
      {
        return true;
      }

      return false;
    }


    public async Task Upload(List<CategorySource> categories)
    {
      await UploadWithoutParents(categories);
      await UploadWithParents();
    }


    public async Task UploadWithParents()
    {
      int count = 1;
      IEnumerable<RepoCategory> categories = _categoryRepository.GetCategories();
      int total = categories.Where(c => c.CategoryAtSource.parent != 0).Count();

      foreach (var category in categories.Where(c => c.CategoryAtSource.parent != 0))
      {
        var parentRow = categories.Single(c => c.SourceId == category.SourceParent);

        if (category.DestinationParent != parentRow.DestinationId)
        {
          await UpdateCategoryParent(category.DestinationId, parentRow.DestinationId);

          category.DestinationParent = parentRow.DestinationId;
          _categoryRepository.SaveCategory(category);
        }

        Console.WriteLine($"{count}/{total} Updated parent for category {category.CategoryAtSource.name} to {parentRow.CategoryAtSource.name}");
        count++;
      }
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


    public async Task UploadWithoutParents(List<CategorySource> categories)
    {
      int count = 1;
      int total = categories.Count();

      foreach (var category in categories)
      {
        CategoryUploaded uploaded = await UploadCategory(category, count, total);

        _categoryRepository.SaveUpdatedCategory(category, uploaded);

        count++;
      }
    }

    private async Task<CategoryUploaded> UploadCategory(CategorySource originCategory, int count, int total)
    {
      CategoryUploaded categoryUploaded = null;
      List<CategorySource> destinationCategory = await ExistingCategories(originCategory.slug);
      bool hasBeenUploaded = false;

      if (destinationCategory.Count() > 0)
      {
        hasBeenUploaded = true;
      }

      if (!hasBeenUploaded)
      {
        categoryUploaded = await UploadNew(originCategory, count, total);
      }
      else
      {
        categoryUploaded = await UploadExisting(originCategory, destinationCategory.First(), count, total);
      }

      return categoryUploaded;
    }


    private async Task<CategoryUploaded> UploadExisting(CategorySource originCategory, CategorySource destinationCategory, int count, int total)
    {
      CategoryUploaded categoryUploaded = null;

      if (NeedToBeUploaded(destinationCategory, originCategory))
      {
        //don't reupload the category image, it will create a duplicate
        if (MediaFilesEquivalent(originCategory.image, destinationCategory.image) && originCategory.image != null)
        {
          var c = originCategory.CategorySourceExistingImage(destinationCategory.image.id);
          c.id = destinationCategory.id;

          categoryUploaded = await CategoryUpdateHttp(c, destinationCategory.parent, _destination.Url);
        }
        else
        {
          categoryUploaded = await UpdateCategory(originCategory, destinationCategory, destinationCategory.parent);
        }

        Console.WriteLine($"{count}/{total} Category {originCategory.name} updated at {_destination.Uri}");
      }
      else
      {
        categoryUploaded = new CategoryUploaded()
        {
          description = destinationCategory.description,
          display = destinationCategory.display,
          id = destinationCategory.id,
          image = Image(destinationCategory.image),
          menu_order = destinationCategory.menu_order,
          name = destinationCategory.name,
          parent = destinationCategory.parent,
          slug = destinationCategory.slug
        };

        Console.WriteLine($"{count}/{total} Category {originCategory.name} does not need updating at {_destination.Uri}");
      }

      return categoryUploaded;
    }

    private Image Image(MediaFile? image)
    {
      if (image == null) return null;

      Image image1 = new Image()
      {
        alt = image.alt,
        date_created = image.date_created,
        date_created_gmt = image.date_created_gmt,
        date_modified = image.date_modified,
        date_modified_gmt = image.date_modified_gmt,
        id = image.id,
        name = image.name,
        src = image.src
      };

      return image1;
    }

    private async Task<CategoryUploaded> UploadNew(CategorySource category, int count, int total)
    {
      List<CategorySource> existingUploadedCategory = await ExistingCategories(category.slug);

      CategoryUploaded categoryUploaded = await AddCategory(category);

      _categoryRepository.SaveNewUploadedCategory(category, categoryUploaded);
      Console.WriteLine($"{count}/{total} Category {category.name} saved at {_destination.Uri}");


      return categoryUploaded;
    }


    private async Task<bool> HasBeenUploaded(string slug) => (await ExistingCategories(slug)).Any();


    private bool NeedToBeUploaded(CategorySource destinationCategory, CategorySource originCategory)
    {

      if (destinationCategory.name != originCategory.name)
      {
        return true;
      }
      else if (destinationCategory.slug != originCategory.slug)
      {
        return true;
      }
      else if (destinationCategory.description != originCategory.description)
      {
        return true;
      }
      else if (destinationCategory.display != originCategory.display)
      {
        return true;
      }
      else if (destinationCategory.menu_order != originCategory.menu_order)
      {
        return true;
      }
      else if (!MediaFilesEquivalent(destinationCategory.image?.src, originCategory.image?.src))
      {
        return true;
      }

      return false;
    }

  }

}
