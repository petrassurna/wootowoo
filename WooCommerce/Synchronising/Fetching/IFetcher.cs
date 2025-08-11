using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce.Synchronising.Fetchers
{
  public interface IFetcher
  {
    public Task Fetch();

    public Task Fetch(IEnumerable<string> categorySlugs, IEnumerable<int> productIds);

  }
}
