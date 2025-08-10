namespace WooCommerce.Http.SourceInstallation.Structures
{
  public class VariationAttribute
  {
    public int id { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
    public string option { get; set; }
  }

  public class VariationCollection
  {
    public string href { get; set; }
  }

  public class VariationDimensions
  {
    public string length { get; set; }
    public string width { get; set; }
    public string height { get; set; }
  }

  public class VariationImage
  {
    public int id { get; set; }
    public DateTime date_created { get; set; }
    public DateTime date_created_gmt { get; set; }
    public DateTime date_modified { get; set; }
    public DateTime date_modified_gmt { get; set; }
    public string src { get; set; }
    public string name { get; set; }
    public string alt { get; set; }
  }

  public class VariationLinks
  {
    public List<VariationSelf> self { get; set; }
    public List<VariationCollection> collection { get; set; }
    public List<VariationUp> up { get; set; }
  }

  public class VariationMetaData
  {
    public int id { get; set; }
    public string key { get; set; }
    public string value { get; set; }
  }

  public class Variation
  {
    public int id { get; set; }
    public string type { get; set; }
    public DateTime date_created { get; set; }
    public DateTime date_created_gmt { get; set; }
    public DateTime date_modified { get; set; }
    public DateTime date_modified_gmt { get; set; }
    public string description { get; set; }
    public string permalink { get; set; }
    public string sku { get; set; }
    public string global_unique_id { get; set; }
    public string price { get; set; }
    public string regular_price { get; set; }
    public string sale_price { get; set; }
    public object date_on_sale_from { get; set; }
    public object date_on_sale_from_gmt { get; set; }
    public object date_on_sale_to { get; set; }
    public object date_on_sale_to_gmt { get; set; }
    public bool on_sale { get; set; }
    public string status { get; set; }
    public bool purchasable { get; set; }
    public bool @virtual { get; set; }
    public bool downloadable { get; set; }
    public List<VariationDownload> downloads { get; set; }
    public int download_limit { get; set; }
    public int download_expiry { get; set; }
    public string tax_status { get; set; }
    public string tax_class { get; set; }
    public string manage_stock { get; set; }
    public int stock_quantity { get; set; }
    public string stock_status { get; set; }
    public string backorders { get; set; }
    public bool backorders_allowed { get; set; }
    public string backordered { get; set; }

    public object low_stock_amount { get; set; }
    public string weight { get; set; }
    public VariationDimensions dimensions { get; set; }
    public string shipping_class { get; set; }
    public int shipping_class_id { get; set; }
    public VariationImage image { get; set; }

    public List<VariationAttribute> attributes { get; set; }
    public int menu_order { get; set; }
    public List<VariationMetaData> meta_data { get; set; }
    public string name { get; set; }
    public int parent_id { get; set; }
    public VariationLinks _links { get; set; }
  }


  public class VariationDownload
  {
    public string name { get; set; }
    public string file { get; set; }

  }


  public class VariationSelf
  {
    public string href { get; set; }
    public VariationTargetHints targetHints { get; set; }
  }

  public class VariationTargetHints
  {
    public List<string> allow { get; set; }
  }

  public class VariationUp
  {
    public string href { get; set; }
  }

}