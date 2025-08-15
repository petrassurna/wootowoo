using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using WooCommerce.Repositories.Category;
using WooCommerce.Synchronising.Fetchers;
using WooCommerce.Synchronising.Fetchers.Categories.Http;
using WooCommerce.Synchronising.Fetchers.Categories.Structures;

namespace WooCommerce.Http.SourceInstallation.Categories
{
  public class CategoryFetcher : IFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly WordPressInstallation _installation;
    private readonly SemaphoreSlim _semaphore;
    private readonly int _requestDelayMs;
    private readonly CategoryRepository _categoryRepository;
    private readonly ILogger _logger;
    private readonly CategoryHttp _categoryHttp;

    private readonly int PAGE_SIZE = 20;

    public CategoryFetcher(HttpClient httpClient, WordPressInstallation installation, ILogger logger, int maxConcurrency = 3, int requestDelayMs = 100)
    {
      _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
      _installation = installation ?? throw new ArgumentNullException(nameof(installation));

      _semaphore = new SemaphoreSlim(maxConcurrency);
      _requestDelayMs = requestDelayMs;
      _categoryRepository = new CategoryRepository();
      _logger = logger;
      _categoryHttp = new CategoryHttp(httpClient, _installation, logger);
    }


    public async Task Fetch()
    {
      //_categoryRepository.DeleteCategories();
      int page = 1;

      int countInRepo = _categoryRepository.GetCategoryCount();
      int countAtDestination = await _categoryHttp.GetCategoryCount();

      if (countInRepo >= countAtDestination)
      {
        _logger.LogInformation($"{countInRepo} categories saved - no more to fetch");
        return;
      }

      page = (int)Math.Floor(Convert.ToDecimal((countInRepo / PAGE_SIZE)));
      if (page == 0)
        page = 1;

      List<CategorySource> categorySource = await GetAllCategoriesDirect(page, PAGE_SIZE);

      _categoryRepository.SaveCategoriesIfNotPresent(categorySource.Select(c => new RepositoryCategory()
      {
        Slug = c.slug,
        CategoryAtSource = c,
        DateAdded = DateTime.UtcNow,
        DestinationId = 0,
        SourceId = c.id,
        SourceParent = c.parent,
        DestinationParent = 0
      }));

      _logger.LogInformation($"{categorySource.Count()} categories saved from {_installation.Url}");
    }


    Task IFetcher.Fetch()
    {
      return Fetch();
    }


    public Task Fetch(IEnumerable<string> slugs, IEnumerable<string> productIds)
    {
      throw new NotImplementedException();
    }


    private async Task<List<CategorySource>> GetAllCategoriesDirect(int page, int pageSize)
    {
      var allCategories = new List<CategorySource>();
      int totalItems = 0;

      List<CategorySource> currentPageCategories;

      do
      {
        currentPageCategories = await GetCategoriesPages(page, pageSize);
        allCategories.AddRange(currentPageCategories);
        page++;

        totalItems += currentPageCategories.Count();

        _logger.LogInformation($"{totalItems} categories read from {_installation.Url}");
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
        r = JsonConvert.DeserializeObject<List<CategorySource>>(responseBody) ?? new List<CategorySource>();

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
