using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WooCommerce.Http.SourceInstallation.Categories
{
  public class CategoryGetter
  {
    private readonly HttpClient _httpClient;
    private readonly WordPressInstallation _installation;
    private readonly SemaphoreSlim _semaphore;
    private readonly int _requestDelayMs;

    public CategoryGetter(HttpClient httpClient, WordPressInstallation installation, int maxConcurrency = 3, int requestDelayMs = 100)
    {
      _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
      _installation = installation ?? throw new ArgumentNullException(nameof(installation));

      _semaphore = new SemaphoreSlim(maxConcurrency);
      _requestDelayMs = requestDelayMs; 
    }

    public async Task<List<CategorySource>> GetAllCategories()
    {
      var allCategories = new List<CategorySource>();
      int page = 1;
      const int pageSize = 20;

      List<CategorySource> currentPageCategories;

      do
      {
        currentPageCategories = await GetCategoriesPages(page, pageSize);
        allCategories.AddRange(currentPageCategories);
        page++;
      }
      while (currentPageCategories.Count == pageSize);

      return allCategories;
    }


    private async Task<List<CategorySource>> GetCategoriesPages(int page, int pageSize)
    {
      await _semaphore.WaitAsync();

      try
      {
        var requestUri = $"{_installation.Url}/wp-json/wc/v3/products/categories?per_page={pageSize}&page={page}";

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        var credentials = $"{_installation.Key}:{_installation.Secret}";
        var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        List<CategorySource> r = new List<CategorySource>();

        try
        {
          r = JsonConvert.DeserializeObject<List<CategorySource>>(responseBody) ?? new List<CategorySource>();
        }
        catch(Exception e)
        {
        }

        return r;     
      }
      finally
      {
        await Task.Delay(_requestDelayMs); 
        _semaphore.Release();          
      }

    }

  }
}
