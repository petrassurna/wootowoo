using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce.Repository.Products;

namespace WooCommerce.Repository
{
  public class Location
  {

    public static string DatabaseConnection() =>
      $"Filename={DatabaseFilename()}";

    public static string DatabaseFilename() 
      => $"{AppContext.BaseDirectory}database\\wootowoo.db";


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
