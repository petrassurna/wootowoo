using Microsoft.Extensions.Logging;
using WooCommerce.Http;
using WooCommerce.Http.Media;
using WooCommerce.Repositories.Category;
using WooCommerce.Synchronising.Fetchers.Categories.Http;
using WooCommerce.Synchronising.Fetchers.Categories.Structures;
using WooCommerce.Synchronising.Fetching.Categories.Structures;

namespace WooCommerce.Synchronising.Fetchers.Categories
{
  public class CategoryFetcher
  {

    HttpClient _httpClient;
    WordPressInstallation _destination;
    MediaUploader _mediaUploader;
    CategoryRepository _categoryRepository;
    CategoryHttp _categoryHttp;
    ILogger _logger;

    public CategoryFetcher(HttpClient httpClient, WordPressInstallation destination, ILogger logger)
    {
      _httpClient = httpClient;
      _destination = destination;
      _mediaUploader = new MediaUploader(httpClient, destination);
      _categoryRepository = new CategoryRepository();
      _categoryHttp = new CategoryHttp(_httpClient, _destination, logger);
      _logger = logger;

    }

    private async Task<CategoryClassesDestination> AddCategory(CategorySource category)
      => await _categoryHttp.UploadCategory(HttpMethod.Post, category,
        0, $"{_destination.Url}/wp-json/wc/v3/products/categories");

    public async Task Synchronize(List<CategorySource> categories)
    {
      await UploadWithoutParents(categories);
      await UploadWithParents();
    }

    private async Task<CategoryClassesDestination> UpdateCategory(CategorySource categoryToUpload, CategorySource categoryUploaded, int parent)
      => await _categoryHttp.UploadCategory(HttpMethod.Put, categoryToUpload, parent,
        $"{_destination.Url}/wp-json/wc/v3/products/categories/{categoryUploaded.id}");


    private async Task UploadWithParents()
    {
      int count = 1;
      IEnumerable<RepoCategory> categories = _categoryRepository.GetCategories();
      int total = categories.Where(c => c.CategoryAtSource.parent != 0).Count();

      foreach (var category in categories.Where(c => c.CategoryAtSource.parent != 0))
      {
        var parentRow = categories.Single(c => c.SourceId == category.SourceParent);

        if (category.DestinationParent != parentRow.DestinationId)
        {
          await _categoryHttp.UpdateCategoryParent(category.DestinationId, parentRow.DestinationId);

          category.DestinationParent = parentRow.DestinationId;
          _categoryRepository.SaveCategory(category);
        }

        _logger.LogInformation($"{count}/{total} Updated parent for category {category.CategoryAtSource.name} to {parentRow.CategoryAtSource.name}");
        count++;
      }
    }


    private async Task<CategoryClassesDestination> UploadCategory(CategorySource originCategory, int count, int total)
    {
      CategoryClassesDestination categoryUploaded = null;
      List<CategorySource> destinationCategory = await _categoryHttp.ExistingCategories(originCategory.slug);
      bool hasBeenUploaded = false;

      if (destinationCategory.Count() > 0)
      {
        hasBeenUploaded = true;
      }

      if (!hasBeenUploaded)
      {
        categoryUploaded = await UploadNewCategory(originCategory, count, total);
      }
      else
      {
        categoryUploaded = await UploadExistingCategory(originCategory, destinationCategory.First(), count, total);
      }

      return categoryUploaded;
    }

    private async Task<CategoryClassesDestination> UploadExistingCategory(CategorySource originCategory, CategorySource destinationCategory, int count, int total)
    {
      CategoryClassesDestination categoryUploaded = null;

      if (destinationCategory.IsDifferent(originCategory))
      {

        if (MediaHelper.MediaFilesEquivalent(originCategory.image, destinationCategory.image) && originCategory.image != null)
        {
          var c = originCategory.CategorySourceExistingImage(destinationCategory.image.id);
          c.id = destinationCategory.id;

          categoryUploaded = await _categoryHttp.CategoryUpdateHttp(c, destinationCategory.parent, _destination.Url);
        }
        else
        {
          //does the image already exist?
          int? mediaId = await _mediaUploader.GetMediaIdByFileName(Path.GetFileName(originCategory.image.src));

          if (mediaId is not null)
          {
            var c = originCategory.CategorySourceExistingImage((int)mediaId);
            c.id = destinationCategory.id;
            categoryUploaded = await _categoryHttp.CategoryUpdateHttp(c, destinationCategory.parent, _destination.Url);
          }
          else
          {
            categoryUploaded = await UpdateCategory(originCategory, destinationCategory, destinationCategory.parent);
          }

        }

        _logger.LogInformation($"{count}/{total} Category {originCategory.name} updated at {_destination.Uri}");
      }
      else
      {
        categoryUploaded = new CategoryClassesDestination()
        {
          description = destinationCategory.description,
          display = destinationCategory.display,
          id = destinationCategory.id,
          image = destinationCategory.image?.Image(),
          menu_order = destinationCategory.menu_order,
          name = destinationCategory.name,
          parent = destinationCategory.parent,
          slug = destinationCategory.slug
        };

        _logger.LogInformation($"{count}/{total} Category {originCategory.name} does not need updating at {_destination.Uri}");
      }

      return categoryUploaded;
    }

    private async Task<CategoryClassesDestination> UploadNewCategory(CategorySource category, int count, int total)
    {
      CategoryClassesDestination categoryUploaded = await AddCategory(category);

      _categoryRepository.SaveNewUploadedCategory(category, categoryUploaded);
      _logger.LogInformation($"{count}/{total} Category {category.name} saved at {_destination.Uri}");


      return categoryUploaded;
    }

    private async Task UploadWithoutParents(List<CategorySource> categories)
    {
      int count = 1;
      int total = categories.Count();

      foreach (var category in categories)
      {
        CategoryClassesDestination uploaded = await UploadCategory(category, count, total);

        _categoryRepository.SaveUpdatedCategory(category, uploaded);

        count++;
      }
    }



  }

}
