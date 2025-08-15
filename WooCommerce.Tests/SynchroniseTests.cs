using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using WooCommerce.Configuration;
using WooCommerce.Http;
namespace WooCommerce.Tests
{
  public class SynchroniseTests
  {

    [Fact]
    public async Task SynchroniseKeysOK()
    {
      Config config = GetConfig();

      HttpClient client = new HttpClient();
      var (ok, msg) = await config.IsValid(client, NullLogger.Instance);
      ok.ShouldBe(true);
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

