using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using WooCommerce.Configuration;
using WooCommerce.Http;
using WooCommerce.Synchronising;
namespace WooCommerce.Tests
{
  public class SynchroniseTests
  {
    [Fact]
    public async Task SynchroniseTest()
    {

      Config config = new Config()
      {
        Destination = new Http.WordPressInstallation()
        {
          Uri = new Uri("https://testwoo.kinsta.cloud"),
          WordPressAPIUser = new WordPressUser()
          {
            Username = "admin",
            Password = "DPQF xUfG Eyom Wz68 Zgxf gbSA"
          },
          Key = "ck_4ad4cb9c25ab7e4870830f4c78ae7a06317a2e47",
          Secret = "cs_bea7e093c4034e7ed81ff5a6b91790ce0121b5a6"
        },

        Source = new Http.WordPressInstallation()
        {
          Key = "ck_d2c47d77fe2d80b545f43c7ce0a9a6193c1108a3",
          Secret = "cs_081101f0d9adec92b7ab55b95e9e7ca106b2b98b",
          Uri = new Uri("https://renovatorsparadisewebsite.kinsta.cloud"),
          WordPressAPIUser = null
        }
      };

      HttpClient client = new HttpClient();
      var (ok, msg) = await config.IsValid(client, NullLogger.Instance);
      ok.ShouldBe(true);

      //SynchronisingManager manager = new SynchronisingManager(config, httpClient, logger);
      //await manager.Synchronise();
    }


    [Fact]
    public async Task SynchroniseKeysOK()
    {

      Config config = new Config()
      {
        Destination = new Http.WordPressInstallation()
        {
          Uri = new Uri("http://localhost:8080/woocommerce1"),
          WordPressAPIUser = new WordPressUser()
          {
            Username = "wordpressapi",
            Password = "oG2Q Bc5S FtJh zgBo jBNs fhJ3"
          },
          Key = "ck_4ad4cb9c25ab7e4870830f4c78ae7a06317a2e47",
          Secret = "cs_bea7e093c4034e7ed81ff5a6b91790ce0121b5a6"
        },

        Source = new Http.WordPressInstallation()
        {
          Key = "ck_b16e9e449edc5368554242d8d405a2b7341f46be",
          Secret = "cs_d7de615faf485d96565965c629b4081691c1f661",
          Uri = new Uri("http://localhost:8080/woocommerce2"),
          WordPressAPIUser = null
        }
      };

      HttpClient client = new HttpClient();
      var (ok, msg) = await config.IsValid(client, NullLogger.Instance);
      ok.ShouldBe(true);
    }


    [Fact]
    public async Task SynchroniseKeysNotOK1()
    {

      Config config = new Config()
      {
        Destination = new Http.WordPressInstallation()
        {
          Uri = new Uri("http://localhost:8080/woocommerce1"),
          WordPressAPIUser = new WordPressUser()
          {
            Username = "wordpressapi*",
            Password = "oG2Q Bc5S FtJh zgBo jBNs fhJ3"
          },
          Key = "ck_4ad4cb9c25ab7e4870830f4c78ae7a06317a2e47",
          Secret = "cs_bea7e093c4034e7ed81ff5a6b91790ce0121b5a6"
        },

        Source = new Http.WordPressInstallation()
        {
          Key = "ck_b16e9e449edc5368554242d8d405a2b7341f46be",
          Secret = "cs_d7de615faf485d96565965c629b4081691c1f661",
          Uri = new Uri("http://localhost:8080/woocommerce2"),
          WordPressAPIUser = null
        }
      };

      HttpClient client = new HttpClient();
      var (ok, msg) = await config.IsValid(client, NullLogger.Instance);
      ok.ShouldBe(true);
    }

  }

}


