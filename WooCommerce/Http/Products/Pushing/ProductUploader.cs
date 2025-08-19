using WooCommerce.Http.SourceInstallation.Structures;

namespace WooCommerce.Http.Products.Pushing
{

  public class ProductUploader
  {
    string _key;
    string _secret;
    HttpClient _httpClient;
    string _url;

    public ProductUploader(string url, HttpClient httpClient, string key, string secret)
    {
      _key = key;
      _secret = secret;
      _httpClient = httpClient;
      _url = url;
    }


    public async Task<bool> Upload(IEnumerable<Product> products)
    {
      throw new NotImplementedException();
    }

  }
}
