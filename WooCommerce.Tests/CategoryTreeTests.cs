using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;
using Shouldly;
using System.Reflection;
using WooCommerce.Http.SourceInstallation.Categories;
using WooCommerce.Http.SourceInstallation.Structures;

namespace WooCommerce.Tests
{
  public class CategoryTreeTests
  {
    [Fact]
    public void CategoryTreeTest()
    {

      string content = File.ReadAllText(@"C:\Yart\Clients and Jobs\WooToWoo\WooToWoo\json1.json");

      List<Product> productList = JsonConvert.DeserializeObject<List<Product>>(content);

    }


    public static string ReadEmbeddedJson(string resourceName)
    {
      var assembly = Assembly.GetExecutingAssembly();

      var fullName = Array.Find(
          assembly.GetManifestResourceNames(),
          name => name.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase)
      );

      if (fullName == null)
        throw new Exception($"Resource '{resourceName}' not found.");

      using var stream = assembly.GetManifestResourceStream(fullName);
      using var reader = new StreamReader(stream);
      return reader.ReadToEnd();
    }
  }

}


