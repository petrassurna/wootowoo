using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using WooCommerce.Repositories.Category;

namespace WooCommerce.Http.SourceInstallation.Categories
{
  public class CategoryGetter
  {
    private readonly HttpClient _httpClient;
    private readonly WordPressInstallation _installation;
    private readonly SemaphoreSlim _semaphore;
    private readonly int _requestDelayMs;
    private readonly CategoryRepository _categoryRepository;

    public CategoryGetter(HttpClient httpClient, WordPressInstallation installation, int maxConcurrency = 3, int requestDelayMs = 100)
    {
      _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
      _installation = installation ?? throw new ArgumentNullException(nameof(installation));

      _semaphore = new SemaphoreSlim(maxConcurrency);
      _requestDelayMs = requestDelayMs;
      _categoryRepository = new CategoryRepository();
    }


    public async Task<List<CategorySource>> GetAllCategories()
    {
      _categoryRepository.DeleteCategories();

      List<CategorySource> categorySource = await GetAllCategoriesDirect();

      _categoryRepository.SaveCategoriesIfNotPresent(categorySource.Select(c => new RepoCategory()
      {
        Slug = c.slug,
        CategoryAtSource = c,
        DateAdded = DateTime.UtcNow,
        DestinationId = 0,
        SourceId = c.id,
        SourceParent = c.parent,
        DestinationParent = 0
      }));

      Console.WriteLine($"{categorySource.Count()} categories saved from {_installation.Url}");

      return categorySource;
    }


    private async Task<List<CategorySource>> GetAllCategoriesDirect()
    {
      var allCategories = new List<CategorySource>();
      int page = 1;
      const int pageSize = 20;
      int totalItems = 0;

      List<CategorySource> currentPageCategories;

      do
      {
        currentPageCategories = await GetCategoriesPages(page, pageSize);
        allCategories.AddRange(currentPageCategories);
        page++;

        totalItems += currentPageCategories.Count();

        Console.WriteLine($"{totalItems} categories read from {_installation.Url}");
      }
      while (currentPageCategories.Count == pageSize);

      return allCategories;
    }


    private async Task<List<CategorySource>> GetCategoriesPages(int page, int pageSize)
    {
      await _semaphore.WaitAsync();

      try
      {
        var requestUri = $"{_installation.Url}/wp-json/wc/v3/products/categories?per_page={pageSize}&page={page}";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        var credentials = $"{_installation.Key}:{_installation.Secret}";
        var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        List<CategorySource> r = new List<CategorySource>();

        try
        {
          r = JsonConvert.DeserializeObject<List<CategorySource>>(responseBody) ?? new List<CategorySource>();
        }
        catch (Exception e)
        {
        }

        return r;
      }
      finally
      {
        await Task.Delay(_requestDelayMs);
        _semaphore.Release();
      }

    }

  }
}
