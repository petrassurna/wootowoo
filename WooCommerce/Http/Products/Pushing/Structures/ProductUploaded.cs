using System.Text.Json;

namespace WooCommerce.Http.Products.Pushing.Structures
{

  public class ProductUploadedAttribute
  {
    public int id { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
    public int position { get; set; }
    public bool visible { get; set; }
    public bool variation { get; set; }
    public List<string> options { get; set; }
  }

  public class ProductUploadedCategory
  {
    public int id { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
  }

  public class ProductUploadedCollection
  {
    public string href { get; set; }
  }

  public class ProductUploadedDimensions
  {
    public string length { get; set; }
    public string width { get; set; }
    public string height { get; set; }
  }

  public class ProductUploadedLinks
  {
    public List<ProductUploadedSelf> self { get; set; }
    public List<ProductUploadedCollection> collection { get; set; }
  }

  public class ProductUploadedMetaData
  {
    public int id { get; set; }
    public string key { get; set; }
    public string value { get; set; }
  }

  public class ProductUploaded
  {
    public int id { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
    public string permalink { get; set; }
    public DateTime date_created { get; set; }
    public DateTime date_created_gmt { get; set; }
    public DateTime date_modified { get; set; }
    public DateTime date_modified_gmt { get; set; }
    public string type { get; set; }
    public string status { get; set; }
    public string password { get; set; }
    public bool featured { get; set; }
    public string catalog_visibility { get; set; }
    public string description { get; set; }
    public string short_description { get; set; }
    public string sku { get; set; }
    public string price { get; set; }
    public string regular_price { get; set; }
    public string sale_price { get; set; }
    public object date_on_sale_from { get; set; }
    public object date_on_sale_from_gmt { get; set; }
    public object date_on_sale_to { get; set; }
    public object date_on_sale_to_gmt { get; set; }
    public bool on_sale { get; set; }
    public bool purchasable { get; set; }
    public int total_sales { get; set; }
    public bool @virtual { get; set; }
    public bool downloadable { get; set; }
    public List<object> downloads { get; set; }
    public int download_limit { get; set; }
    public int download_expiry { get; set; }
    public string external_url { get; set; }
    public string button_text { get; set; }
    public string tax_status { get; set; }
    public string tax_class { get; set; }
    public bool manage_stock { get; set; }
    public int stock_quantity { get; set; }
    public string backorders { get; set; }
    public bool backorders_allowed { get; set; }
    public bool backordered { get; set; }
    public object low_stock_amount { get; set; }
    public bool sold_individually { get; set; }
    public string weight { get; set; }
    public ProductUploadedDimensions dimensions { get; set; }
    public bool shipping_required { get; set; }
    public bool shipping_taxable { get; set; }
    public string shipping_class { get; set; }
    public int shipping_class_id { get; set; }
    public bool reviews_allowed { get; set; }
    public string average_rating { get; set; }
    public int rating_count { get; set; }
    public List<object> upsell_ids { get; set; }
    public List<object> cross_sell_ids { get; set; }
    public int parent_id { get; set; }
    public string purchase_note { get; set; }
    public List<ProductUploadedCategory> categories { get; set; }
    public List<object> brands { get; set; }
    public List<object> tags { get; set; }
    public List<object> images { get; set; }
    public List<ProductUploadedAttribute> attributes { get; set; }
    public List<object> default_attributes { get; set; }
    public List<object> variations { get; set; }
    public List<object> grouped_products { get; set; }
    public int menu_order { get; set; }
    public string price_html { get; set; }
    public List<object> related_ids { get; set; }
    public List<ProductUploadedMetaData> meta_data { get; set; }
    public string stock_status { get; set; }
    public bool has_options { get; set; }
    public string post_password { get; set; }
    public string global_unique_id { get; set; }
    public string permalink_template { get; set; }
    public string generated_slug { get; set; }
    public ProductUploadedLinks _links { get; set; }

    public static ProductUploaded Deserialize(string json) => 
      JsonSerializer.Deserialize<ProductUploaded>(json);
    
  }

  public class ProductUploadedSelf
  {
    public string href { get; set; }
    public ProductUploadedTargetHints targetHints { get; set; }
  }

  public class ProductUploadedTargetHints
  {
    public List<string> allow { get; set; }
  }

}
