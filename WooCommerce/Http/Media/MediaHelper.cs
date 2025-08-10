using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce.Synchronising.Fetchers.Categories.Structures;

namespace WooCommerce.Http.Media
{
  public static class MediaHelper
  {
    public static bool MediaFilesEquivalent(MediaFile file1, MediaFile file2)
    {
      if (file1 == null || file2 == null)
        return file1 == file2;
      else
        return MediaFilesEquivalent(file1.src, file2.src);
    }


    public static bool MediaFilesEquivalent(string url1, string url2)
    {
      if (url1 == null && url2 == null)
        return true;

      if (!url1.IsValidWebUri() || !url2.IsValidWebUri())
      {
        return false;
      }

      Uri uri1 = new Uri(url1);
      Uri uri2 = new Uri(url2);

      string pathAndQuery1 = uri1.Segments.Last();
      string pathAndQuery2 = uri2.Segments.Last();

      return pathAndQuery1 == pathAndQuery2;
    }

  }
}
