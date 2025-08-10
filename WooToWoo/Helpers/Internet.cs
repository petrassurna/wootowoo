using System.Net;

namespace WooToWoo.Helpers
{
  public class Internet
  {

    public static async Task<bool> HasInternetAsync()
    {
      try
      {
        var hostEntry = await Dns.GetHostEntryAsync("google.com");
        return hostEntry.AddressList.Length > 0;
      }
      catch
      {
        return false;
      }
    }

  }
}
