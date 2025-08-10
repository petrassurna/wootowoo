using Newtonsoft.Json;
using WooCommerce.Http.Media;
using WooCommerce.Synchronising.Fetching.Categories.Structures;

namespace WooCommerce.Synchronising.Fetchers.Categories.Structures
{

  public class Breadcrumb
  {
    [JsonProperty("@id")]
    public string id { get; set; }
  }

  public class Collection
  {
    public string href { get; set; }
  }

  public class Graph
  {
    [JsonProperty("@type")]
    public string type { get; set; }

    [JsonProperty("@id")]
    public string id { get; set; }
    public string url { get; set; }
    public string name { get; set; }
    public IsPartOf isPartOf { get; set; }
    public Breadcrumb breadcrumb { get; set; }
    public string inLanguage { get; set; }
    public List<ItemListElement> itemListElement { get; set; }
    public string description { get; set; }
    public List<PotentialAction> potentialAction { get; set; }
  }

  public class MediaFile
  {
    public int id { get; set; }
    public DateTime? date_created { get; set; }
    public DateTime? date_created_gmt { get; set; }
    public DateTime? date_modified { get; set; }
    public DateTime? date_modified_gmt { get; set; }
    public string src { get; set; }
    public string name { get; set; }
    public string alt { get; set; }

    public Image Image() => new Image()
    {
      alt = alt,
      date_created = date_created,
      date_created_gmt = date_created_gmt,
      date_modified = date_modified,
      date_modified_gmt = date_modified_gmt,
      id = id,
      name = name,
      src = src
    };

  }

  public class IsPartOf
  {
    [JsonProperty("@id")]
    public string id { get; set; }
  }

  public class ItemListElement
  {
    [JsonProperty("@type")]
    public string type { get; set; }
    public int position { get; set; }
    public string name { get; set; }
    public string item { get; set; }
  }

  public class Links
  {
    public List<Self> self { get; set; }
    public List<Collection> collection { get; set; }
    public List<Up> up { get; set; }
  }

  public class PotentialAction
  {
    [JsonProperty("@type")]
    public string type { get; set; }
    public Target target { get; set; }

    [JsonProperty("query-input")]
    public QueryInput queryinput { get; set; }
  }

  public class QueryInput
  {
    [JsonProperty("@type")]
    public string type { get; set; }
    public bool valueRequired { get; set; }
    public string valueName { get; set; }
  }

  public class Robots
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

  public class CategorySource
  {
    public int id { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
    public int parent { get; set; }
    public string description { get; set; }
    public string display { get; set; }
    public MediaFile? image { get; set; }
    public int menu_order { get; set; }
    public int count { get; set; }
    public string yoast_head { get; set; }
    public YoastHeadJson yoast_head_json { get; set; }
    public Links _links { get; set; }

    public CategorySourceNoImage CategorySourceExistingImage(int mediaId) => new CategorySourceNoImage()
    {
      id = id,
      name = name,
      slug = slug,
      parent = parent,
      description = description,
      display = display,
      menu_order = menu_order,
      count = count,
      yoast_head = yoast_head,
      yoast_head_json = yoast_head_json,
      _links = _links,
      imageId = mediaId
    };


    public bool IsDifferent(CategorySource originCategory)
    {

      if (name != originCategory.name)
      {
        return true;
      }
      else if (slug != originCategory.slug)
      {
        return true;
      }
      else if (description != originCategory.description)
      {
        return true;
      }
      else if (display != originCategory.display)
      {
        return true;
      }
      else if (menu_order != originCategory.menu_order)
      {
        return true;
      }
      else if (!MediaHelper.MediaFilesEquivalent(image, originCategory.image))
      {
        return true;
      }

      return false;
    }


  }


  public class CategorySourceNoImage
  {
    public int id { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
    public int parent { get; set; }
    public string description { get; set; }
    public string display { get; set; }
    public int menu_order { get; set; }
    public int count { get; set; }
    public string yoast_head { get; set; }
    public YoastHeadJson yoast_head_json { get; set; }
    public Links _links { get; set; }
    public int imageId { get; set; }
  }

  public class Schema
  {
    [JsonProperty("@context")]
    public string context { get; set; }

    [JsonProperty("@graph")]
    public List<Graph> graph { get; set; }
  }

  public class Self
  {
    public string href { get; set; }
    public TargetHints targetHints { get; set; }
  }

  public class Target
  {
    [JsonProperty("@type")]
    public string type { get; set; }
    public string urlTemplate { get; set; }
  }

  public class TargetHints
  {
    public List<string> allow { get; set; }
  }

  public class Up
  {
    public string href { get; set; }
  }

  public class YoastHeadJson
  {
    public string title { get; set; }
    public Robots robots { get; set; }
    public string canonical { get; set; }
    public string og_locale { get; set; }
    public string og_type { get; set; }
    public string og_title { get; set; }
    public string og_description { get; set; }
    public string og_url { get; set; }
    public string og_site_name { get; set; }
    public string twitter_card { get; set; }

    /// <summary>
    /// What yoast_head_json.schema is
    /// It's auto-generated structured data (schema.org JSON-LD) from the Yoast SEO plugin.
    /// It’s meant for Google and other search engines to understand your page better.
    /// It includes:
    /// Site name
    /// Breadcrumbs
    /// Page type (e.g., WebSite, CollectionPage)
    /// Metadata useful for SEO only
    /// </summary>
    //public Schema schema { get; set; }
  }


}
