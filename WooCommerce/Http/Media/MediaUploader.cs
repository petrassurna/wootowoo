using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using WooCommerce.Synchronising.Fetchers.Categories.Structures;

namespace WooCommerce.Http.Media
{
  public class MediaUploader
  {
    HttpClient _httpClient;
    WordPressInstallation _installation;

    public MediaUploader(HttpClient httpClient, WordPressInstallation installation)
    {
      _httpClient = httpClient;
      _installation = installation;
    }


    public async Task<int?> GetMediaIdByFileName(string fileName)
    {
      // Strip extension for search (WordPress search matches title)
      string searchTerm = Path.GetFileName(fileName);

      var requestUri = $"{_installation.Url}/wp-json/wp/v2/media?search={searchTerm}&per_page=100";

      using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

      // Add Basic Auth header
      var credentials = $"{_installation.WordPressAPIUser.Username}:{_installation.WordPressAPIUser.Password}";
      var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
      request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

      var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      var responseBody = await response.Content.ReadAsStringAsync();

      // Deserialize response
      var mediaItems = JsonConvert.DeserializeObject<List<MediaItem>>(responseBody);

      // Try to find an exact filename match in source_url
      var match = mediaItems.FirstOrDefault(m =>
          m.source_url.EndsWith(searchTerm, StringComparison.OrdinalIgnoreCase));

      return match?.id;
    }

    public async Task<int> Upload(MediaFile file)
    {
      int? mediaId = await GetMediaIdByFileName(file.src);

      if (mediaId is not null)
        return (int)mediaId;

      var media = await UploadFile(file.src);

      if (!string.IsNullOrEmpty(file.name) || !string.IsNullOrEmpty(file.alt))
      {
        media = await UpdateMetadata(media.id, file.name, file.alt);
      }

      return media.id;
    } 


    private async Task<Media> UploadFile(string fileUrl)
    {
      var requestUri = $"{_installation.Url}/wp-json/wp/v2/media";

      using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

      // Basic Auth
      var credentials = $"{_installation.WordPressAPIUser.Username}:{_installation.WordPressAPIUser.Password}";
      var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
      request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

      // Download the file
      using var httpResponse = await _httpClient.GetAsync(fileUrl);
      httpResponse.EnsureSuccessStatusCode();
      var fileStream = await httpResponse.Content.ReadAsStreamAsync();

      var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);
      var streamContent = new StreamContent(fileStream);
      streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

      // Add to multipart content
      using var multipartContent = new MultipartFormDataContent();
      multipartContent.Add(streamContent, "file", fileName);
      request.Content = multipartContent;

      // Send the upload request
      using var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      var responseBody = await response.Content.ReadAsStringAsync();
      return JsonConvert.DeserializeObject<Media>(responseBody);
    }


    private async Task<Media> UpdateMetadata(int mediaId, string title = null, string altText = null, string caption = null)
    {
      var requestUri = $"{_installation.Url}/wp-json/wp/v2/media/{mediaId}";

      var metadata = new Dictionary<string, string>();
      if (!string.IsNullOrEmpty(title)) metadata["title"] = title;
      if (!string.IsNullOrEmpty(altText)) metadata["alt_text"] = altText;
      if (!string.IsNullOrEmpty(caption)) metadata["caption"] = caption;

      var metadataJson = JsonConvert.SerializeObject(metadata);

      using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
      {
        Content = new StringContent(metadataJson, Encoding.UTF8, "application/json")
      };

      var credentials = $"{_installation.WordPressAPIUser.Username}:{_installation.WordPressAPIUser.Password}";
      var base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
      request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);

      using var response = await _httpClient.SendAsync(request);
      response.EnsureSuccessStatusCode();

      var responseBody = await response.Content.ReadAsStringAsync();
      return JsonConvert.DeserializeObject<Media>(responseBody);
    }

  }

  public class MediaItem
  {
    public int id { get; set; }
    public string source_url { get; set; }
    public Title_ title { get; set; }
  }

  public class Title_
  {
    public string rendered { get; set; }
  }
}
