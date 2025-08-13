using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Net.Http;

namespace WooCommerce.Configuration
{
  public class Http
  {

    public static async Task<(bool ok, string message)> IsValidWooCommerceRead(
       string baseUrl,
       string key,
       string secret,
              HttpClient http,
       CancellationToken ct = default)
    {
      var url = $"{baseUrl.TrimEnd('/')}/wp-json/wc/v3/products/categories?per_page=1";

      using var req = new HttpRequestMessage(HttpMethod.Get, url);

      // UA: some hosts reject requests without it
      req.Headers.UserAgent.ParseAdd("WooToWoo/1.0");

      // WooCommerce accepts consumer key/secret via Basic auth
      var token = Convert.ToBase64String(
          System.Text.Encoding.ASCII.GetBytes($"{key}:{secret}")
      );
      req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", token);

      try
      {
        using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        resp.EnsureSuccessStatusCode();

        if (resp.Headers.TryGetValues("X-WP-Total", out var values) &&
            int.TryParse(values.FirstOrDefault(), out var total))
        {
          return (true, $"Destination {baseUrl} API key is authorized for WooCommerce read.");
        }

        return (false, $"Destination {baseUrl} API key is not authorized for WooCommerce read.");
      }
      catch(Exception e)
      {
        return (false, $"Destination {baseUrl} API key is not authorized for WooCommerce read - {e.Message}");
      }
    }


    public static async Task<(bool ok, string message)> IsValidWordPressReadWrite(
      string baseUrl,
      string username,
      string applicationPassword,
       HttpClient http,
      CancellationToken ct = default)
    {
      if (string.IsNullOrWhiteSpace(baseUrl)) return (false, "Base URL is required.");
      baseUrl = baseUrl.TrimEnd('/');


      // Some hosts reject requests without a UA.
      http.DefaultRequestHeaders.UserAgent.ParseAdd("WP-Write-Test/1.0");

      // Basic auth: username:applicationPassword
      var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{applicationPassword}"));
      http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);

      // 1) Try to create a draft post (minimal payload)
      var postEndpoint = $"{baseUrl}/wp-json/wp/v2/posts";
      var payload = new
      {
        title = "API write test (safe to delete)",
        status = "draft"
      };

      using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
      HttpResponseMessage createResp;
      try
      {
        createResp = await http.PostAsync(postEndpoint, content, ct);
      }
      catch (Exception ex)
      {
        return (false, $"HTTP error while creating post: {ex.Message}");
      }

      if (createResp.StatusCode == HttpStatusCode.Unauthorized)
        return (false, "Unauthorized (401): username or application password is wrong, or HTTPS required.");
      if (createResp.StatusCode == HttpStatusCode.Forbidden)
        return (false, "Forbidden (403): user lacks permission to create posts.");

      if (createResp.StatusCode != HttpStatusCode.Created)
      {
        var body = await createResp.Content.ReadAsStringAsync(ct);
        return (false, $"Unexpected status {(int)createResp.StatusCode}: {body}");
      }

      // Parse created post ID
      int postId;
      try
      {
        using var stream = await createResp.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        postId = doc.RootElement.GetProperty("id").GetInt32();
      }
      catch
      {
        // If we can’t parse ID, we still proved write access.
        return (true, "Write OK: draft created (couldn’t parse ID to delete).");
      }

      // 2) Try to delete the draft (nice-to-have cleanup; not required to prove write)
      var deleteEndpoint = $"{baseUrl}/wp-json/wp/v2/posts/{postId}?force=true";
      try
      {
        var deleteResp = await http.DeleteAsync(deleteEndpoint, ct);
        // Some hosts block DELETE; creation alone proves write.
        if (deleteResp.IsSuccessStatusCode)
          return (true, $"Destination {baseUrl} API key is authorized for read and write");
        else
          return (false, $"Destination {baseUrl} API key write OK. Delete failed (likely server rule), but write access is confirmed");
      }
      catch
      {
        return (false, $"Destination {baseUrl} API key write OK. Draft created. Delete not attempted/failed, but write is confirmed.");
      }
    }


  }
}
