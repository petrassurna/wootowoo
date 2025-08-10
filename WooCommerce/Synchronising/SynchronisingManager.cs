using System.Diagnostics;
using WooCommerce.Http.SourceInstallation.Categories;
using WooCommerce.Repositories.Summary;
using WooCommerce.Synchronising.Fetchers.Products;
using WooCommerce.Synchronising.Fetchers;
using WooCommerce.Configuration;
using Microsoft.Extensions.Logging;

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

    private IEnumerable<IFetcher> Fetchers()
    {
      yield return new ProductFetcher(_httpClient, _config.Source, _logger);
      yield return new CategoryFetcher(_httpClient, _config.Source, _logger);
    }

    private void Push()
    {
      //CategorySynchronizer categoryUploader = new CategorySynchronizer(httpClient, config.Destination);
      //await categoryUploader.Synchronize(categories);

      //?WooCommerce.Workers.ProductUploader productSetter = new WooCommerce.Workers.ProductUploader(httpClient, config.Destination);
      //?await productSetter.Upload(products);
    }


    public async Task Synchronise()
    {
      //Location.Delete();

      ImportSummaryRepository importSummary = new ImportSummaryRepository();

      Console.WriteLine($"Starting migration from {_config.Source.Url}");
      importSummary.Create();

      var sw = Stopwatch.StartNew();

      await Fetch();
      Push();

      sw.Stop();
      //Console.WriteLine($"Elapsed ms: {sw.ElapsedMilliseconds}");

      importSummary.Complete();
    }




  }
}
