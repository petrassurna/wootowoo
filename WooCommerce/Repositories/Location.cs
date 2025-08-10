namespace WooCommerce.Repositories
{
  public class Location
  {

    public static string DatabaseConnection() =>
      $"Filename={DatabaseFilename()}";

    public static string DatabaseFilename()
      => $"{AppContext.BaseDirectory}database\\wootowoo.db";

    public static string DatabaseFolder()
      => $"{AppContext.BaseDirectory}database";


    public static bool DatabaseExists() => File.Exists(DatabaseFilename());

    public static void Delete()
    {
      if (DatabaseExists())
      {
        File.Delete(DatabaseFilename());
      }

    }


  }
}
