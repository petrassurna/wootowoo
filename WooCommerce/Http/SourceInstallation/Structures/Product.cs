using Newtonsoft.Json;

namespace WooCommerce.Http.SourceInstallation.Structures
{
  public class ProductAttribute
  {
    public int id { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
    public int position { get; set; }
    public bool visible { get; set; }
    public bool variation { get; set; }
    public List<string> options { get; set; }
  }


  public class ProductBreadcrumb
  {
    [JsonProperty("@id")]
    public string id { get; set; }
  }

  public class ProductCategory
  {
    public int id { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
  }

  public class ProductCollection
  {
    public string href { get; set; }
  }

  public class ProductDimensions
  {
    [JsonConverter(typeof(StringOrArrayConverter))]
    public string? length { get; set; }

    [JsonConverter(typeof(StringOrArrayConverter))]

    public string? width { get; set; }

    [JsonConverter(typeof(StringOrArrayConverter))]
    public string? height { get; set; }
  }


  public class ProductGraph
  {
    [JsonProperty("@type")]
    public string type { get; set; }

    [JsonProperty("@id")]
    public string id { get; set; }
    public string url { get; set; }
    public string name { get; set; }
    public ProductIsPartOf isPartOf { get; set; }
    public object datePublished { get; set; }
    public DateTime? dateModified { get; set; }
    public ProductBreadcrumb breadcrumb { get; set; }
    public string inLanguage { get; set; }
    public List<ProductPotentialAction> potentialAction { get; set; }
    public List<ProductItemListElement> itemListElement { get; set; }
    public string description { get; set; }
    public ProductPrimaryImageOfPage primaryImageOfPage { get; set; }
    public ProductImage image { get; set; }
    public string thumbnailUrl { get; set; }
    public string contentUrl { get; set; }
    public double? width { get; set; }
    public double? height { get; set; }
    public string caption { get; set; }
  }

  public class ProductImage
  {
    public int id { get; set; }
    public DateTime? date_created { get; set; }
    public DateTime? date_created_gmt { get; set; }
    public DateTime? date_modified { get; set; }
    public DateTime? date_modified_gmt { get; set; }
    public string src { get; set; }
    public string name { get; set; }
    public string alt { get; set; }
  }

  public class ProductImage2
  {
    [JsonProperty("@id")]
    public string id { get; set; }
  }

  public class ProductIsPartOf
  {
    [JsonProperty("@id")]
    public string id { get; set; }
  }

  public class ProductItemListElement
  {
    [JsonProperty("@type")]
    public string type { get; set; }
    public int position { get; set; }
    public string name { get; set; }
    public string item { get; set; }
  }

  public class ProductLinks
  {
    public List<Self> self { get; set; }
    public List<ProductCollection> collection { get; set; }
  }

  public class ProductMetaData
  {
    public int id { get; set; }
    public string key { get; set; }
    public object value { get; set; }
  }

  public class ProductOgImage
  {
    public int width { get; set; }
    public int height { get; set; }
    public string url { get; set; }
    public string type { get; set; }
  }

  public class ProductPotentialAction
  {
    [JsonProperty("@type")]
    public string type { get; set; }
    public object target { get; set; }

    [JsonProperty("query-input")]
    public ProductQueryInput queryinput { get; set; }
  }

  public class ProductPrimaryImageOfPage
  {
    [JsonProperty("@id")]
    public string id { get; set; }
  }

  public class ProductQueryInput
  {
    [JsonProperty("@type")]
    public string type { get; set; }
    public bool valueRequired { get; set; }
    public string valueName { get; set; }
  }

  public class ProductRobots
  {
    public string index { get; set; }
    public string follow { get; set; }

    [JsonProperty("max-snippet")]
    public string maxsnippet { get; set; }

    [JsonProperty("max-image-preview")]
    public string maximagepreview { get; set; }

    [JsonProperty("max-video-preview")]
    public string maxvideopreview { get; set; }
  }

  public class Product
  {
    public ProductLinks _links { get; set; }
    public string average_rating { get; set; }
    public bool backordered { get; set; }
    public bool backorders_allowed { get; set; }
    public string backorders { get; set; }
    public List<object> brands { get; set; }
    public string button_text { get; set; }
    public string catalog_visibility { get; set; }
    public List<ProductCategory> categories { get; set; }
    public List<ProductAttribute> attributes { get; set; }
    public List<object> default_attributes { get; set; }
    public string description { get; set; }
    public ProductDimensions dimensions { get; set; }
    public bool downloadable { get; set; }
    public int download_expiry { get; set; }
    public int download_limit { get; set; }
    public List<object> downloads { get; set; }
    public DateTime? date_created { get; set; }
    public DateTime? date_created_gmt { get; set; }
    public DateTime? date_modified { get; set; }
    public DateTime? date_modified_gmt { get; set; }
    public object date_on_sale_from { get; set; }
    public object date_on_sale_from_gmt { get; set; }
    public object date_on_sale_to { get; set; }
    public object date_on_sale_to_gmt { get; set; }
    public string external_url { get; set; }
    public bool featured { get; set; }
    public string global_unique_id { get; set; }
    public List<object> grouped_products { get; set; }
    public bool has_options { get; set; }
    public List<Variation> HttpVariations { get; set; } = new List<Variation>();
    public int id { get; set; }
    public List<ProductImage> images { get; set; }
    public string name { get; set; }
    public bool manage_stock { get; set; }
    public int menu_order { get; set; }
    public List<ProductMetaData> meta_data { get; set; }
    public bool on_sale { get; set; }
    public int parent_id { get; set; }
    public string permalink { get; set; }
    public string post_password { get; set; }
    public string price { get; set; }
    public string price_html { get; set; }
    public string purchase_note { get; set; }
    public bool purchasable { get; set; }
    public int rating_count { get; set; }
    public List<int> related_ids { get; set; }
    public string regular_price { get; set; }
    public bool reviews_allowed { get; set; }
    public string sale_price { get; set; }
    public string shipping_class { get; set; }
    public int shipping_class_id { get; set; }
    public bool shipping_required { get; set; }
    public bool shipping_taxable { get; set; }
    public string short_description { get; set; }
    public string sku { get; set; }
    public string slug { get; set; }
    public object low_stock_amount { get; set; }
    public bool sold_individually { get; set; }
    public string status { get; set; }
    public string stock_status { get; set; }
    public double? stock_quantity { get; set; }
    public string tax_class { get; set; }
    public string tax_status { get; set; }
    public string type { get; set; }
    public List<int> upsell_ids { get; set; }
    public List<object> cross_sell_ids { get; set; }
    public int total_sales { get; set; }
    //public bool @virtual { get; set; }
    public string visibility { get; set; }
    public string weight { get; set; }
    //public YoastHeadJson yoast_head_json { get; set; }
    public string yoast_head { get; set; }
    public List<Tag> tags { get; set; }
    public List<int> variations { get; set; }
    public IEnumerable<Variation> variationDetails { get; set; }
  }

  public class Schema
  {
    [JsonProperty("@context")]
    public string context { get; set; }

    [JsonProperty("@graph")]
    public List<ProductGraph> graph { get; set; }
  }

  public class Self
  {
    public string href { get; set; }
    public TargetHints targetHints { get; set; }
  }

  public class Tag
  {
    public int id { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
  }

  public class TargetHints
  {
    public List<string> allow { get; set; }
  }

  public class TwitterMisc
  {
    [JsonProperty("Est. reading time")]
    public string Estreadingtime { get; set; }
  }

  public class YoastHeadJson
  {
    public string title { get; set; }
    public ProductRobots robots { get; set; }
    public string canonical { get; set; }
    public string og_locale { get; set; }
    public string og_type { get; set; }
    public string og_title { get; set; }
    public string og_description { get; set; }
    public string og_url { get; set; }
    public string og_site_name { get; set; }
    public DateTime? article_modified_time { get; set; }
    public string twitter_card { get; set; }
    public TwitterMisc twitter_misc { get; set; }
    public Schema schema { get; set; }
    public string description { get; set; }
    public List<ProductOgImage> og_image { get; set; }
  }

}
