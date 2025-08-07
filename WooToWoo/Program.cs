// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using System.Diagnostics;
using WooCommerce.Http;
using WooToWoo.Configuration;
using WooToWoo.Helpers;
using WooCommerce.Workers;
using WooCommerce.Http.SourceInstallation.Structures;
using WooCommerce.Repositories;
using WooCommerce.Repositories.Summary;
using WooCommerce.Http.SourceInstallation.Categories;
using WooCommerce.Synchronizers.Categories;
using WooCommerce.Synchronizers.Categories.Structures;
using WooCommerce.Synchronizers.Categories.Structures.Origin;

internal class Program
{
  private static async Task Main(string[] args)
  {

    await Start(args);

  }


  private static async Task Start(string[] args)
  {

    if (!await Internet.HasInternetAsync())
    {
      Console.WriteLine("No internet connection!");
    }
    else if (args.Length == 0)
    {
      await ImportFromScratch();
    }
    else if (args[0].ToLower() == "r")
    {
      await ResumeImport();
    }
  }



  /// <summary>
  /// r option
  /// </summary>
  /// <returns></returns>

  private static async Task ResumeImport()
  {
    Config config = Config();

    if(!Location.DatabaseExists())
    {
      Console.WriteLine($"Database does not exist, try a fresh import by running without the 'r' flag");
      return;
    }

    ImportSummaryRepository importSummaryRepo = new ImportSummaryRepository();
    ImportSummary importSummary = importSummaryRepo.Get();

    if(importSummary == null)
    {
      Console.WriteLine($"There is no import to resume, try a fresh import by running without the 'r' flag");
      return;
    }

    Console.WriteLine($"Resuming migration from {config.Source.Url}");

    HttpClient httpClient = new HttpClient();

    //ProductGetter productGetter = new ProductGetter(httpClient, config.Source);
    //var products = await productGetter.GetAllProducts(importSummary.ProductsImported);

    CategoryGetter categoryGetter = new CategoryGetter(httpClient, config.Source);
    List<CategorySource> categories = await categoryGetter.GetAllCategories();

    CategorySynchronizer categoryUploader = new CategorySynchronizer(httpClient, config.Destination);
    await categoryUploader.Synchronize(categories);

    //await Trash.EmptyWooCommerceTrashAsync(config.Destination);

    //CategoryUploader categoryUploader = new CategoryUploader(httpClient, config.Destination);
    //await categoryUploader.Upload(categories);

    //?WooCommerce.Workers.ProductUploader productSetter = new WooCommerce.Workers.ProductUploader(httpClient, config.Destination);
    //?await productSetter.Upload(products);

    //}
    //catch(Exception e)
    ///{
    //  Console.WriteLine(e.Message);
    //  Console.WriteLine("Press any key to continue...");
    //  Console.ReadKey();
    //}

    //LiteDB

    importSummaryRepo.Complete();
  }



  private static async Task ImportFromScratch()
  {
    Config config = Config();
    Location.Delete();

    ImportSummaryRepository importSummary = new ImportSummaryRepository();

    Console.WriteLine($"Starting migration from {config.Source.Url}");
    importSummary.Create();

    //try
    //{
    var sw = Stopwatch.StartNew();

    HttpClient httpClient = new HttpClient();

    //ProductGetter productGetter = new ProductGetter(httpClient, config.Source);
    //var products = await productGetter.GetAllProducts();

    CategoryGetter categoryGetter = new CategoryGetter(httpClient, config.Source);
    List<CategorySource> categories = await categoryGetter.GetAllCategories();

    //await Trash.EmptyWooCommerceTrashAsync(config.Destination);

    CategorySynchronizer categoryUploader = new CategorySynchronizer(httpClient, config.Destination);
    await categoryUploader.Synchronize(categories);

    //?WooCommerce.Workers.ProductUploader productSetter = new WooCommerce.Workers.ProductUploader(httpClient, config.Destination);
    //?await productSetter.Upload(products);

    sw.Stop();
    Console.WriteLine($"Elapsed ms: {sw.ElapsedMilliseconds}");
    //}
    //catch(Exception e)
    ///{
    //  Console.WriteLine(e.Message);
    //  Console.WriteLine("Press any key to continue...");
    //  Console.ReadKey();
    //}

    //LiteDB

    importSummary.Complete();
  }



  private static void test()
  {
    string file = @"C:\Yart\Clients and Jobs\WooToWoo\WooToWoo\json1.json";

    string contents = File.ReadAllText(file);

    var variation = JsonConvert.DeserializeObject<Variation>(contents);

  }


  private static Config Config() => new Config
  {

    Source = new WordPressInstallation()
    {
      Key = "ck_d2c47d77fe2d80b545f43c7ce0a9a6193c1108a3",
      Secret = "cs_081101f0d9adec92b7ab55b95e9e7ca106b2b98b",
      Uri = new Uri("https://renovatorsparadisewebsite.kinsta.cloud")
    },

    /*
     Source = new WordPressInstallation()
    {
      Key = "ck_175b001764ee2ed76c242dcf70e29be6016245cf",
      Secret = "cs_e23a92e96a37dedfe480009e3266b055edbce909",
      Uri = new Uri("https://renovatorsparadise.com.au")
    },
 
    Source = new WordPressInstallation()
    {
      Key = "ck_d2c47d77fe2d80b545f43c7ce0a9a6193c1108a3",
      Secret = "cs_081101f0d9adec92b7ab55b95e9e7ca106b2b98b",
      Uri = new Uri("https://renovatorsparadisewebsite.kinsta.cloud")
    },

    */

    Destination = new WordPressInstallation()
    {
      WordPressUser = new WordPressUser()
      {
        ApplicationPasswordName = "admin",
        ApplicationPassword = "DPQF xUfG Eyom Wz68 Zgxf gbSA"
      },

      Key = "ck_8c4aa2d3a2e583f8be1418bc2af40a72733bc5bf",
      Secret = "cs_8ab09d981b37583338cccc3b9507de6a44a1399f",
      Uri = new Uri("https://testwoo.kinsta.cloud")
    }

  };

}