namespace WooCommerce.Http
{
  public static class HttpHelper
  {
    public static bool IsValidWebUri(this string uri)
    {
      return Uri.TryCreate(uri, UriKind.Absolute, out Uri uriResult) &&
             (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }

  }
}
