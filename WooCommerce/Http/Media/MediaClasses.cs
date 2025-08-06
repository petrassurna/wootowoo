using Newtonsoft.Json;

namespace WooCommerce.Http.Media
{
  // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
  public class About
  {
    public string href { get; set; }
  }

  public class Author
  {
    public bool embeddable { get; set; }
    public string href { get; set; }
  }

  public class Caption
  {
    public string raw { get; set; }
    public string rendered { get; set; }
  }

  public class Collection
  {
    public string href { get; set; }
  }

  public class Cury
  {
    public string name { get; set; }
    public string href { get; set; }
    public bool templated { get; set; }
  }

  public class Description
  {
    public string raw { get; set; }
    public string rendered { get; set; }
  }

  public class Full
  {
    public string file { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public string mime_type { get; set; }
    public string source_url { get; set; }
  }

  public class Guid
  {
    public string rendered { get; set; }
    public string raw { get; set; }
  }

  public class ImageMeta
  {
    public string aperture { get; set; }
    public string credit { get; set; }
    public string camera { get; set; }
    public string caption { get; set; }
    public string created_timestamp { get; set; }
    public string copyright { get; set; }
    public string focal_length { get; set; }
    public string iso { get; set; }
    public string shutter_speed { get; set; }
    public string title { get; set; }
    public string orientation { get; set; }
    public List<object> keywords { get; set; }
  }

  public class Links
  {
    public List<Self> self { get; set; }
    public List<Collection> collection { get; set; }
    public List<About> about { get; set; }
    public List<Author> author { get; set; }
    public List<Reply> replies { get; set; }

    [JsonProperty("wp:action-unfiltered-html")]
    public List<WpActionUnfilteredHtml> wpactionunfilteredhtml { get; set; }

    [JsonProperty("wp:action-assign-author")]
    public List<WpActionAssignAuthor> wpactionassignauthor { get; set; }
    public List<Cury> curies { get; set; }
  }

  public class MediaDetails
  {
    public int width { get; set; }
    public int height { get; set; }
    public string file { get; set; }
    public int filesize { get; set; }
    public Sizes sizes { get; set; }
    public ImageMeta image_meta { get; set; }
  }

  public class Reply
  {
    public bool embeddable { get; set; }
    public string href { get; set; }
  }

  public class Media
  {
    public int id { get; set; }
    public DateTime date { get; set; }
    public DateTime date_gmt { get; set; }
    public Guid guid { get; set; }
    public DateTime modified { get; set; }
    public DateTime modified_gmt { get; set; }
    public string slug { get; set; }
    public string status { get; set; }
    public string type { get; set; }
    public string link { get; set; }
    public Title title { get; set; }
    public int author { get; set; }
    public int featured_media { get; set; }
    public string comment_status { get; set; }
    public string ping_status { get; set; }
    public string template { get; set; }
    public List<object> meta { get; set; }
    public string permalink_template { get; set; }
    public string generated_slug { get; set; }
    public List<string> class_list { get; set; }
    public Description description { get; set; }
    public Caption caption { get; set; }
    public string alt_text { get; set; }
    public string media_type { get; set; }
    public string mime_type { get; set; }
    public MediaDetails media_details { get; set; }
    public object post { get; set; }
    public string source_url { get; set; }
    public List<object> missing_image_sizes { get; set; }
    public Links _links { get; set; }
  }

  public class Self
  {
    public string href { get; set; }
    public TargetHints targetHints { get; set; }
  }

  public class Sizes
  {
    public Thumbnail thumbnail { get; set; }
    public WoocommerceGalleryThumbnail woocommerce_gallery_thumbnail { get; set; }
    public Full full { get; set; }
  }

  public class TargetHints
  {
    public List<string> allow { get; set; }
  }

  public class Thumbnail
  {
    public string file { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int filesize { get; set; }
    public string mime_type { get; set; }
    public string source_url { get; set; }
  }

  public class Title
  {
    public string raw { get; set; }
    public string rendered { get; set; }
  }

  public class WoocommerceGalleryThumbnail
  {
    public string file { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int filesize { get; set; }
    public string mime_type { get; set; }
    public string source_url { get; set; }
  }

  public class WpActionAssignAuthor
  {
    public string href { get; set; }
  }

  public class WpActionUnfilteredHtml
  {
    public string href { get; set; }
  }


}
