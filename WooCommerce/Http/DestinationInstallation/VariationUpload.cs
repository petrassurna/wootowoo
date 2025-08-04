using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;
using WooCommerce.Http.SourceInstallation.Structures;

namespace WooCommerce.Http.DestinationInstallation
{

  public class VariationUpload
  {
    public bool backorders_allowed { get; set; }

    public string backordered { get; set; }

    public string backorders { get; set; }

    public string description { get; set; }

    public bool downloadable { get; set; }

    public int download_limit { get; set; }

    public int download_expiry { get; set; }

    public IEnumerable<AttributeDownLoad> downloads { get; set; }

    public string global_unique_id { get; set; }

    public string height { get; set; }

    public bool in_stock { get; set; }

    public string length { get; set; }

    public int low_stock_amount { get; set; }

    public bool manage_stock { get; set; }

    public string regular_price { get; set; }

    public string sale_price { get; set; }

    public string shipping_class { get; set; }

    public string sku { get; set; }

    public int stock_quantity { get; set; }

    public bool @virtual { get; set; }

    public string width { get; set; }

    public string weight { get; set; }

    public List<AttributeUpload> attributes { get; set; } = new List<AttributeUpload>();

    public static VariationUpload Make(Variation variation)
    {
      VariationUpload variationUpload = new VariationUpload();

      variationUpload.backordered = variation.backordered;
      variationUpload.backorders = variation.backorders;
      variationUpload.backorders_allowed = variation.backorders_allowed;
      variationUpload.description = CleanDescription(variation.description);
      variationUpload.downloadable = variation.downloadable;
      variationUpload.download_limit = variation.download_limit;
      variationUpload.download_expiry = variation.download_expiry;

      variationUpload.downloads = variation.downloads.Select(d => new AttributeDownLoad()
      {
        name = d.name,
        file = d.file   //need to chnage file url
      });

      //check variation image

      variationUpload.global_unique_id = variation.global_unique_id;
      variationUpload.height = variation.dimensions.height;
      variationUpload.in_stock = variation.stock_status == "Yes" ? true : false;
      variationUpload.length = variation.dimensions.length;
      variationUpload.low_stock_amount = variation.low_stock_amount == null ? 0 : Convert.ToInt32(variation.low_stock_amount);
      variationUpload.manage_stock = variation.manage_stock == "true" ? true : false;
      variationUpload.regular_price = variation.regular_price;
      variationUpload.sale_price = variation.sale_price;
      variationUpload.shipping_class = variation.shipping_class;
      variationUpload.sku = variation.sku;
      variationUpload.stock_quantity = variation.stock_quantity;
      variationUpload.@virtual = variation.@virtual;
      variationUpload.weight = variation.weight;
      variationUpload.width = variation.dimensions.width;

      foreach (var att in variation.attributes)
      {
        variationUpload.attributes.Add(new AttributeUpload()
        {
          name = att.name,
          option = att.option
        });
      }

      return variationUpload;
    }


    public class AttributeDownLoad
    {
      public string file { get; set; }
      public string name { get; set; }
    }


    private static string CleanDescription(string input)
    {
      // Replace <br> and <p> with line breaks first
      string textWithBreaks = Regex.Replace(input, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
      textWithBreaks = Regex.Replace(textWithBreaks, "</p>", "\n", RegexOptions.IgnoreCase);
      textWithBreaks = Regex.Replace(textWithBreaks, "<p.*?>", string.Empty, RegexOptions.IgnoreCase);

      // Now strip remaining HTML tags
      string plainText = Regex.Replace(textWithBreaks, "<.*?>", string.Empty);

      // Decode HTML entities
      plainText = System.Net.WebUtility.HtmlDecode(plainText);

      return plainText.Trim();
    }

    public string Serialize()
    {
      var options = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
      };

      return JsonSerializer.Serialize(this, options);
    }
  }

  public class AttributeUpload
  {
    public string name { get; set; }
    public string option { get; set; }
  }


}
