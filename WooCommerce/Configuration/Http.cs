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
      if (string.IsNullOrWhiteSpace(baseUrl)) return (false, "Base URL is required.");
      if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret))
        return (false, "Consumer key/secret are required.");

      // Normalize base (use site root, not /wp-admin)
      baseUrl = baseUrl.Trim().TrimEnd('/');
      var i = baseUrl.IndexOf("/wp-admin", StringComparison.OrdinalIgnoreCase);
      if (i >= 0) baseUrl = baseUrl[..i];

      http.DefaultRequestHeaders.UserAgent.ParseAdd("WooToWoo/1.0");

      // Endpoints to try (pretty vs rest_route)
      var pretty = $"{baseUrl}/wp-json/wc/v3/products/categories?per_page=1";
      var restRoute = $"{baseUrl}/?rest_route=/wc/v3/products/categories&per_page=1";

      // Helper: add ck/cs as query params (HTTP-friendly)
      static string WithQueryAuth(string url, string ck, string cs)
      {
        var sep = url.Contains('?') ? "&" : "?";
        return $"{url}{sep}consumer_key={Uri.EscapeDataString(ck)}&consumer_secret={Uri.EscapeDataString(cs)}";
      }

      // Try in this order:
      var attempts = new[]
      {
        new { Url = pretty,    UseBasic = true,  Label = "pretty + basic" },
        new { Url = WithQueryAuth(pretty, key, secret),    UseBasic = false, Label = "pretty + query auth" },
        new { Url = restRoute, UseBasic = true,  Label = "rest_route + basic" },
        new { Url = WithQueryAuth(restRoute, key, secret), UseBasic = false, Label = "rest_route + query auth" },
    };

      HttpStatusCode? lastStatus = null;
      string lastBody = "";

      foreach (var a in attempts)
      {
        using var req = new HttpRequestMessage(HttpMethod.Get, a.Url);
        if (a.UseBasic)
        {
          var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{key}:{secret}"));
          req.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
        }

        HttpResponseMessage resp;
        try
        {
          resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        }
        catch (Exception ex)
        {
          lastStatus = null;
          lastBody = $"HTTP error: {ex.Message}";
          continue;
        }

        lastStatus = resp.StatusCode;
        lastBody = await resp.Content.ReadAsStringAsync(ct);

        if (resp.StatusCode == HttpStatusCode.Unauthorized)
          return (false, $"401 Unauthorized ({a.Label}). On HTTP, servers often drop Authorization; try query auth or add HTTP_AUTHORIZATION pass-through in .htaccess. Body: {lastBody}");

        if (resp.StatusCode == HttpStatusCode.Forbidden)
          return (false, $"403 Forbidden ({a.Label}). Key may lack read permission or a security plugin is blocking REST. Body: {lastBody}");

        if (resp.IsSuccessStatusCode)
        {
          // Success → read access confirmed
          // (Optional) try to read X-WP-Total for extra info
          int total = 0;
          if (resp.Headers.TryGetValues("X-WP-Total", out var vals) &&
              int.TryParse(vals.FirstOrDefault(), out var t)) total = t;

          var extra = total > 0 ? $" Total categories (server header): {total}." : "";
          return (true, $"WooCommerce read OK via {a.Label}.{extra}");
        }

        // else: try the next attempt (e.g., fall back to query auth or rest_route)
      }

      var statusText = lastStatus.HasValue ? $"{(int)lastStatus} {lastStatus}" : "no HTTP status";
      return (false, $"Failed to read WooCommerce categories after all attempts. Last response: {statusText}. Body: {lastBody}");
    }

    public static async Task<(bool ok, string message)> IsValidWordPressReadWrite(
        string baseUrl,
        string username,
        string applicationPassword,
        HttpClient http,
        CancellationToken ct = default)
    {
      if (string.IsNullOrWhiteSpace(baseUrl)) return (false, "Base URL is required.");
      baseUrl = baseUrl.Trim().TrimEnd('/');
      var i = baseUrl.IndexOf("/wp-admin", StringComparison.OrdinalIgnoreCase);
      if (i >= 0) baseUrl = baseUrl[..i];

      http.DefaultRequestHeaders.UserAgent.ParseAdd("WP-Write-Test/1.0");

      // Where is REST?
      var restPretty = $"{baseUrl}/wp-json/";
      var restFallback = $"{baseUrl}/?rest_route=/";
      var restBase = await TryOk(http, restPretty, ct) ? restPretty :
                     await TryOk(http, restFallback, ct) ? restFallback : null;
      if (restBase is null)
        return (false, "REST not reachable at /wp-json or ?rest_route=. Check permalinks/.htaccess.");

      // Build request (request-scoped Authorization)
      var pwd = (applicationPassword ?? "").Replace(" ", "");
      var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{pwd}"));
      var postEndpoint = $"{restBase}wp/v2/posts";

      var payload = new { title = "API write test (safe to delete)", status = "draft" };
      using var req = new HttpRequestMessage(HttpMethod.Post, postEndpoint)
      {
        Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
      };
      req.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);

      HttpResponseMessage createResp;
      try { createResp = await http.SendAsync(req, ct); }
      catch (Exception ex) { return (false, $"HTTP error while creating post: {ex.Message}"); }

      var body = await createResp.Content.ReadAsStringAsync(ct);

      if (createResp.StatusCode == HttpStatusCode.Unauthorized)
        return (false, $"401 Unauthorized. Check username (login name), app password, and that HTTP_AUTHORIZATION reaches PHP. Body: {body}");
      if (createResp.StatusCode == HttpStatusCode.Forbidden)
        return (false, $"403 Forbidden. User lacks capability to create posts. Body: {body}");

      if (createResp.StatusCode != HttpStatusCode.Created)
        return (false, $"Unexpected {(int)createResp.StatusCode} {createResp.ReasonPhrase}: {body}");

      // Parse created post ID
      int postId;
      try
      {
        using var stream = await createResp.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        postId = doc.RootElement.GetProperty("id").GetInt32();
      }
      catch { return (true, "Write OK: draft created (couldn’t parse ID to delete)."); }

      // Cleanup (best effort)
      var deleteEndpoint = $"{restBase}wp/v2/posts/{postId}?force=true";
      using var delReq = new HttpRequestMessage(HttpMethod.Delete, deleteEndpoint);
      delReq.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
      try
      {
        var del = await http.SendAsync(delReq, ct);
        return del.IsSuccessStatusCode
            ? (true, $"Write OK: draft created and deleted. ({baseUrl})")
            : (true, $"Write OK: draft created. Delete failed ({(int)del.StatusCode}).");
      }
      catch
      {
        return (true, "Write OK: draft created. Delete not attempted/failed.");
      }

      static async Task<bool> TryOk(HttpClient http, string url, CancellationToken ct)
      {
        try { using var r = await http.GetAsync(url, ct); return r.IsSuccessStatusCode; }
        catch { return false; }
      }
    }

  }
}
