using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using WooCommerce.Configuration;
using WooCommerce.Synchronising;
namespace WooCommerce.Tests
{
  public class SimpleProductTest
  {

    [Fact]
    public async Task TransferOneProduct()
    {
      HttpClient httpClient = new HttpClient();

      SynchronisingManager manager = new SynchronisingManager(GetConfig(), httpClient, NullLogger.Instance);

      await manager.Synchronise(["furniture", "recycled-timber", "recycled-timbers", "speciality-items"], ["M151"]);
    }


    public Config GetConfig()
    {
      var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

      return configuration.Get<Config>();
    }

  }

}

