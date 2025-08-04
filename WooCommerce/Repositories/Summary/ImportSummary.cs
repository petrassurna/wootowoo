using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce.Repositories.Summary
{
  public class ImportSummary
  {
    [BsonId]
    public int Id { get; set; } = 1; // Always the same ID

    public DateTime DateCommenced { get; set; }

    public DateTime DateComplete { get; set; }

    public int ProductCountAtSource { get; set; }

    public int ProductsImported  { get; set; }
  }
}
