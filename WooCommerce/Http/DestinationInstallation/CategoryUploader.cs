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

    public async Task<List<CategorySource>> CategoryExists(string slug)
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

      if(!IsValidWebUri(url1) || !IsValidWebUri(url2))
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
      //await UploadWithParents();
    }


    public async Task UploadWithParents()
    {
      throw new NotImplementedException();
    }


    public async Task UploadWithoutParents(List<CategorySource> categories)
    {
      foreach (var category in categories)
      {
        await UploadCategory(category);
      }
    }

    //need to check what happens when categories uploaded
    //make one different,  is image reuploaded it shouldnt be
    private async Task UploadCategory(CategorySource category)
    {
      List<CategorySource> existingUploadedCategory = await CategoryExists(category.slug);

      if (!existingUploadedCategory.Any())
      {
        CategoryUploaded uploaded = await AddCategory(category);

        _categoryRepository.SaveNewUploadedCategory(category, uploaded);
        Console.WriteLine($"Category {category.name} saved");
      }
      else if (UpdateRequired(category, existingUploadedCategory.First()))
      {
        var existingUploadedCategory_ = existingUploadedCategory.First();

        //don't reupload the category image, it will create a duplicate
        if (MediaFilesEquivalent(category.image, existingUploadedCategory_.image) && category.image != null)
        {
          var c = category.CategorySourceExistingImage(existingUploadedCategory_.image.id);
          c.id = existingUploadedCategory_.id;

          await CategoryUpdateHttp(c, 0, _destination.Url);
        }
        else
        {
          CategoryUploaded categoryUploaded = await UpdateCategory(category, existingUploadedCategory_, existingUploadedCategory.First().parent);
          _categoryRepository.SaveUpdatedCategory(category, categoryUploaded);
        }

        Console.WriteLine($"Category {category.name} updated");
      }
      else
      {
        Console.WriteLine($"Category {category.name} is uploaded and up to date");
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
