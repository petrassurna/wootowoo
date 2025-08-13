using WooCommerce.Configuration;
using WooCommerce.Synchronising;
namespace WooCommerce.Tests
{
  public class CategoryTreeTests
  {
    [Fact]
    public async Task CategoryTreeTest()
    {
      Config config = new Config()
      {

        Destination = new Http.WordPressInstallation()
        {
          Uri = new Uri("http://localhost:8080/woocommerce1/wp-admin/"),
          WordPressAPIUser = new Http.WordPressUser()
          {
            Username = "wordpressapi",
            password = "4t4w daB9 quBx JZEA En2x NECq"
          },
          Key = "ck_4ad4cb9c25ab7e4870830f4c78ae7a06317a2e47",
          Secret = "cs_bea7e093c4034e7ed81ff5a6b91790ce0121b5a6"
        },

        Source = new Http.WordPressInstallation()
        {
          Key = "ck_d2c47d77fe2d80b545f43c7ce0a9a6193c1108a3",
          Secret = "cs_081101f0d9adec92b7ab55b95e9e7ca106b2b98b",
          Uri = new Uri("https://renovatorsparadisewebsite.kinsta.cloud/"),
          WordPressAPIUser = null
        }

      };

      //SynchronisingManager manager = new SynchronisingManager(config, httpClient, logger);
      //await manager.Synchronise();

    }
  }

}


