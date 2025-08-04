using Newtonsoft.Json;
using System.Text;
using WooCommerce.Http.SourceInstallation.Structures;
using WooCommerce.Repository.Products;
using WooCommerce.Repository.Summary;

namespace WooCommerce.Http.SourceInstallation.Obtainers
{
  public class InitialProductObtainer : IObtainer
  {

    HttpClient _httpClient;
    WordPressInstallation _installation;
    ProductRepository _productRepository;
    ImportSummaryRepository _importSummaryRepo;

    private int PRODUCTS_PER_PAGE = 100;

    public InitialProductObtainer(HttpClient httpClient, WordPressInstallation installation)
    {
      _installation = installation;
      _httpClient = httpClient;

      _productRepository = new ProductRepository();
      _importSummaryRepo = new ImportSummaryRepository();
    }


    public async Task Get() => await Get(1, PRODUCTS_PER_PAGE);

    public async Task Get(int startAt)
    {
      var summary = _importSummaryRepo.Get();
      int productCount = _productRepository.GetProductCount();

      if (summary.ProductsImported < summary.ProductCountAtSource)
      {
        Console.WriteLine($"{summary.ProductsImported} products imported from {summary.ProductCountAtSource} total");

        await Get((startAt / PRODUCTS_PER_PAGE) + 1, PRODUCTS_PER_PAGE);
      }
      else
      {
        Console.WriteLine($"Product import completed, {summary.ProductCountAtSource} saved locally");
      }
    }


    public async Task Get(int page, int per_page)
    {
      int productImportedThisMethod = 0;

      var allProducts = new List<Product>();

      _productRepository.ClearDatabase();
      var summary = _importSummaryRepo.Get();
      int totalProductsImported = summary.ProductsImported;

      while (true)
      {
        var products = await GetProducts(page, per_page);

        if (products == null || !products.Any())
          break;

        allProducts.AddRange(products);
        productImportedThisMethod += products.Count();
        totalProductsImported += products.Count();

        Console.WriteLine($"Getting page: {page}");
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

        summary = _importSummaryRepo.UpdateProductsImported(totalProductsImported);

        Console.WriteLine($"Saved page: {page}, saved so far: {totalProductsImported}");

        page++;
      }

      Console.WriteLine($"Finished saving {productImportedThisMethod} products");
    }


    public async Task<IEnumerable<Product>> GetProducts(int page, int per_page)
    {
      string responseBody = "";
      var request = new HttpRequestMessage(HttpMethod.Get, $"{_installation.Url}/wp-json/wc/v3/products?per_page={per_page}&page={page}");

      var plainCredentials = $"{_installation.Key}:{_installation.Secret}";
      var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(plainCredentials));
      var authHeader = "Basic " + base64Credentials;
      request.Headers.Add("Authorization", authHeader);

      var content = new StringContent("", null, "application/json");
      request.Content = content;
      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      responseBody = await response.Content.ReadAsStringAsync();

      List<Product> productList = JsonConvert.DeserializeObject<List<Product>>(responseBody);

      return productList;
    }

  }
}
