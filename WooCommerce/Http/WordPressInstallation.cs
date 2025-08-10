namespace WooCommerce.Http
{
  public class WordPressInstallation
  {
    public Uri Uri { get; set; }
    public string Key { get; set; }
    public string Secret { get; set; }

    public WordPressUser WordPressUser { get; set; } 

    public string Url => Uri.ToString();

  }


  public class WordPressUser

  {
    public string ApplicationPasswordName { get; set; }
    public string ApplicationPassword { get; set; }


  }

}
