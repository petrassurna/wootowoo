using System.Diagnostics;
using WooCommerce.Http.SourceInstallation.Categories;
using WooCommerce.Repositories.Summary;
using WooCommerce.Synchronising.Fetchers;
using WooCommerce.Configuration;
using Microsoft.Extensions.Logging;
using WooCommerce.Repositories.Category;
using WooCommerce.Synchronising.Pushing.Categories;
using WooCommerce.Synchronising.Pushing;
using WooCommerce.Repositories;

namespace WooCommerce.Synchronising
{
  public class SynchronisingManager
  {
    private readonly Config _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;

    public SynchronisingManager(Config config, HttpClient httpClient, ILogger logger)
    {
      _config = config;
      _httpClient = httpClient;
      _logger = logger;
    }

    private async Task Fetch()
    {
      foreach (var fetcher in Fetchers())
      {
        await fetcher.Fetch();
      }
    }

    private async Task Fetch(IEnumerable<string> categorySlugs, IEnumerable<string> productIds)
    {
      foreach (var fetcher in Fetchers())
      {
        await fetcher.Fetch(categorySlugs, productIds);
      }
    }

    private IEnumerable<IFetcher> Fetchers()
    {
      //yield return new ProductFetcher(_httpClient, _config.Source, _logger);
      yield return new CategoryFetcher(_httpClient, _config.Source, _logger);
    }


    private IEnumerable<IPusher> Pushers()
    {
      CategoryRepository categoryRepository = new CategoryRepository();

      yield return new CategoryPusher(_httpClient, _config.Destination, _logger, categoryRepository.GetAllCategorySource());
    }


    private async Task Push()
    {

      foreach (var pusher in Pushers())
      {
        await pusher.Push();
      }
    
      
      //CategoryRepository categoryRepository = new CategoryRepository();
      //IEnumerable<RepositoryCategory> categories = categoryRepository.GetCategories();

      //WooCommerce.Workers.ProductUploader productSetter = new WooCommerce.Workers.ProductUploader(httpClient, config.Destination);
      //await productSetter.Upload(products);
    }


    public async Task Synchronise()
    {
      //Location.Delete();

      ImportSummaryRepository importSummary = new ImportSummaryRepository();

      Console.WriteLine($"Starting migration from {_config.Source.Url}");
      importSummary.Create();

      var sw = Stopwatch.StartNew();

      await Fetch();
      await Push();

      sw.Stop();
      //Console.WriteLine($"Elapsed ms: {sw.ElapsedMilliseconds}");

      importSummary.Complete();
    }



    /// <summary>
    /// Limit Synchronise to certain categories and products
    /// </summary>
    /// <param name="categorySlugs"></param>
    /// <param name="productIds"></param>
    /// <returns></returns>
    public async Task Synchronise(IEnumerable<string> categorySlugs, IEnumerable<string> productIds)
    {
      Repository.Delete();

      ImportSummaryRepository importSummary = new ImportSummaryRepository();

      Console.WriteLine($"Starting migration from {_config.Source.Url}");
      importSummary.Create();

      var sw = Stopwatch.StartNew();

      await Fetch(categorySlugs, productIds);
      await Push();

      sw.Stop();
      //Console.WriteLine($"Elapsed ms: {sw.ElapsedMilliseconds}");

      importSummary.Complete();
    }




  }
}
