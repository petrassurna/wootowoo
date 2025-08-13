using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Text.Json;

namespace WooCommerce.Configuration
{
  public class Http
  {

    public static async Task<(bool ok, string message)> IsValid(
      string baseUrl,
      string username,
      string applicationPassword,
      CancellationToken ct = default)
    {
      if (string.IsNullOrWhiteSpace(baseUrl)) return (false, "Base URL is required.");
      baseUrl = baseUrl.TrimEnd('/');

      using var http = new HttpClient
      {
        Timeout = TimeSpan.FromSeconds(30)
      };

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
          return (true, "Write OK: draft created and deleted.");
        else
          return (true, "Write OK: draft created. Delete failed (likely server rule), but write is confirmed.");
      }
      catch
      {
        return (true, "Write OK: draft created. Delete not attempted/failed, but write is confirmed.");
      }
    }
  }
}
