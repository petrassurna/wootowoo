using Microsoft.Extensions.Logging;
using WooCommerce.Repositories;
using WooCommerce.Repositories.Category;
using WooCommerce.Repositories.Products;
using WooCommerce.Synchronising.Fetchers;
using WooCommerce.Synchronising.Fetchers.Categories.Structures;
using WooCommerce.Synchronising.Fetching.Categories;

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
      _categoryHttp = new CategoryHttp(httpClient, _installation, logger, maxConcurrency, requestDelayMs);
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


    public async Task Fetch(IEnumerable<int> productIds) => await Fetch();


    private async Task<List<CategorySource>> GetAllCategoriesDirect(int page, int pageSize)
    {
      var allCategories = new List<CategorySource>();
      int totalItems = 0;

      List<CategorySource> currentPageCategories;

      do
      {
        currentPageCategories = await _categoryHttp.GetCategoriesPages(page, pageSize);
        allCategories.AddRange(currentPageCategories);
        page++;

        totalItems += currentPageCategories.Count();

        _logger.LogInformation($"{totalItems} categories read from {_installation.Url}");
      }
      while (currentPageCategories.Count == pageSize);

      return allCategories;
    }



  }
}
