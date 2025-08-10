// See https://aka.ms/new-console-template for more information
using WooToWoo.Helpers;
using WooCommerce.Synchronising;
using WooCommerce.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

internal class Program
{
  private static async Task Main(string[] args)
  {

    var configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();

    var config = configuration.Get<Config>();

    var httpClient = new HttpClient();

    using var loggerFactory = LoggerFactory.Create(builder =>
    {
      builder
          .ClearProviders()
          .AddSimpleConsole(options =>
          {
            options.IncludeScopes = true;
            options.SingleLine = true;
            options.TimestampFormat = "hh:mm:ss ";
          })
          .SetMinimumLevel(LogLevel.Debug);
    });
    ILogger logger = loggerFactory.CreateLogger("WooToWoo");

    await Start(args, httpClient, logger, config);
  }


  private static async Task Start(string[] args, HttpClient httpClient, ILogger logger, Config config)
  {

    if (!await Internet.HasInternetAsync())
    {
      Console.WriteLine("No internet connection!");
    }
    else if (args.Length == 0)
    {
      await ImportFromScratch(httpClient, logger, config);
    }
    else if (args[0].ToLower() == "r")
    {
      //await ResumeImport(httpClient);
    }
  }

  private static async Task ImportFromScratch(HttpClient httpClient, ILogger logger, Config config)
  {
    //Location.Delete();//?

    SynchronisingManager manager = new SynchronisingManager(config, httpClient, logger);
    await manager.Synchronise();
  }

}