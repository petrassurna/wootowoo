using Microsoft.Extensions.Logging;
using WooCommerce.Http;
using WooCommerce.Http.SourceInstallation.Structures;
using WooCommerce.Repositories.Products;
using WooCommerce.Repositories.Summary;
using WooCommerce.Synchronising.Fetchers.Products;

namespace WooCommerce.Synchronising.Fetching.Products.Obtainers.AllProducts
{
  public class InitialProductObtainer : IObtainer
  {

    private readonly HttpClient _httpClient;
    private readonly WordPressInstallation _installation;
    private readonly ProductRepository _productRepository;
    private readonly ImportSummaryRepository _importSummaryRepo;
    private readonly ProductHttp _productHttp;
    private readonly ILogger _logger;

    private int PRODUCTS_PER_PAGE = 100;

    public InitialProductObtainer(HttpClient httpClient, WordPressInstallation installation, ILogger logger)
    {
      _installation = installation;
      _httpClient = httpClient;
      _logger = logger;

      _productRepository = new ProductRepository();
      _importSummaryRepo = new ImportSummaryRepository();
      _productHttp = new ProductHttp(installation, httpClient);
    }


    public async Task Get()
    {
      var productsImported = _productRepository.GetProductsImportedCount();
      int productAtSource = await _productHttp.GetTotalProductCountAsync();

      if (productsImported >= productAtSource)
      {
        _logger.LogInformation($"{productsImported} products saved - no more to fetch");
        return;
      }

      int startAt = (int)Math.Floor(Convert.ToDecimal(productsImported / PRODUCTS_PER_PAGE)); 

      await Get(startAt + 1, PRODUCTS_PER_PAGE);
    }


    public async Task Get(IEnumerable<int> productIds)
    {
      var allProducts = new List<Product>();

      _productRepository.ClearDatabase();

      IEnumerable<Product> products = await _productHttp.GetProducts(productIds);

      _productRepository.SaveProducts(products.Select(p => new RepoProduct()
      {
        Id = p.id,
        DateAdded = DateTime.UtcNow,
        Name = p.name,
        ProductType = p.type,
        Slug = p.slug,
        Product = p,
        DateUploaded = null
      }));

      _logger.LogInformation($"Finished saving {productIds.Count()} products: {string.Join(",", productIds)}");
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="page">Starts at 1</param>
    /// <param name="per_page"></param>
    /// <returns></returns>
    public async Task Get(int page, int per_page)
    {
      int productImportedThisMethod = 0;

      var allProducts = new List<Product>();

      _productRepository.ClearDatabase();
      var summary = _importSummaryRepo.Get();

      while (true)
      {
        IEnumerable<Product> products = await _productHttp.GetProducts(page, per_page);

        if (products == null || !products.Any())
          break;

        allProducts.AddRange(products);
        productImportedThisMethod += products.Count();

        _logger.LogInformation($"Getting page: {page}");
        _productRepository.SaveProducts(products.Select(p => new RepoProduct()
        {
          Id = p.id,
          DateAdded = DateTime.UtcNow,
          Name = p.name,
          ProductType = p.type,
          Slug = p.slug,
          Product = p,
          DateUploaded = null
        }));

        _logger.LogInformation($"Saved page: {page}, saved so far: {_productRepository.GetProductsImportedCount()}");

        page++;
      }

      _logger.LogInformation($"Finished saving {productImportedThisMethod} products");
    }


  }

}
