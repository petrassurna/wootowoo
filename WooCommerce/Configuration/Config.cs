using Microsoft.Extensions.Logging;
using WooCommerce.Http;

namespace WooCommerce.Configuration
{
  public class Config
  {

    public WordPressInstallation Destination { get; set; }

    public WordPressInstallation Source { get; set; }

    public async Task<bool> IsValid(HttpClient httpClient, ILogger logger)
    {
      (bool ok, string message) = await Http.IsValid(Destination.Url, Destination.WordPressAPIUser.Username, Destination.WordPressAPIUser.password);

      if(!ok)
      {
        logger.LogInformation(message);
      }

      return true;
    }
  }
}
