using Microsoft.Extensions.Logging;
using WooCommerce.Http;

namespace WooCommerce.Configuration
{
  public class Config
  {

    public WordPressInstallation Destination { get; set; }

    public WordPressInstallation Source { get; set; }

    public async Task<(bool ok, string message)> IsValid(HttpClient httpClient, ILogger logger)
    {
      string message2 = "";

      (bool ok, string message1) = await DestinationReadAndWriteValid(httpClient, logger);

      if (ok)
      {
        (ok, message2) = await SourceReadValid(httpClient, logger);
      }

      return (ok, $"{message1} {message2}");
    }


    private async Task<(bool ok, string message)> DestinationReadAndWriteValid(HttpClient httpClient, ILogger logger)
    { 
     (bool ok, string message) = await Http.IsValidWordPressReadWrite(Destination.Url, Destination.WordPressAPIUser.Username, Destination.WordPressAPIUser.Password, httpClient);

      if (!ok && logger is not null)
      {
        logger.LogInformation(message);
      }

      return (ok, message);
    }

    private async Task<(bool ok, string message)> SourceReadValid(HttpClient httpClient, ILogger logger)
    {
      (bool ok, string message) = await Http.IsValidWooCommerceRead(Source.Url, Source.Key, Source.Secret, httpClient);

      if (!ok && logger is not null)
      {
        logger.LogInformation(message);
      }

      return (ok, message);
    }


  }
}
