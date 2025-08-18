using System.Buffers.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WooCommerce.Http
{
  public class WordPressInstallation
  {
    public Uri Uri { get; set; }
    public string Key { get; set; }
    public string Secret { get; set; }

    public WordPressUser? WordPressAPIUser { get; set; }

    public string Url => Uri.ToString().Trim().TrimEnd('/');
  }


  /// <summary>
  /// Log into WordPress Admin as an Administrator
  /// Go to Users → Profile(or Your Profile in the top-right menu)
  /// Scroll down to Application Passwords
  /// 
  /// If not https
  /// define( 'WP_ENVIRONMENT_TYPE', 'local' );
  /// /* That's all, stop editing! Happy publishing. */
  ///
  /// </summary>
  public class WordPressUser

  {
    public string Username { get; set; }
    public string Password { get; set; }


  }

}
