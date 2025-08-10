
namespace WooCommerce.Synchronising.Fetching.Categories.Structures
{

  public class Collection
  {
    public string href { get; set; }
  }

  public class Image
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

  public class Links
  {
    public List<Self> self { get; set; }
    public List<Collection> collection { get; set; }
  }

  public class CategoryClassesDestination
  {
    public int id { get; set; }
    public string name { get; set; }
    public string slug { get; set; }
    public int parent { get; set; }
    public string description { get; set; }
    public string display { get; set; }
    public Image image { get; set; }
    public int menu_order { get; set; }
  }

  public class Self
  {
    public string href { get; set; }
    public TargetHints targetHints { get; set; }
  }

  public class TargetHints
  {
    public List<string> allow { get; set; }
  }


}
