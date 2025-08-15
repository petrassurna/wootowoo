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

    public static async Task<(bool ok, string message)> IsValidWooCommerceWrite(
   string baseUrl,
   string key,
   string secret,
   HttpClient http,
   CancellationToken ct = default)
    {
      if (string.IsNullOrWhiteSpace(baseUrl))
        return (false, "Base URL is required.");
      if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret))
        return (false, "Consumer key/secret are required.");
      if (http == null)
        return (false, "HttpClient is required.");

      string root = baseUrl.TrimEnd('/');
      string productsUrl = $"{root}/wp-json/wc/v3/products";

      // Prepare auth header (WooCommerce accepts ck/cs via Basic)
      var token = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{key}:{secret}"));

      // Payload: minimal draft product
      var payload = new
      {
        name = $"WooToWoo Write Test {Guid.NewGuid():N}",
        status = "draft",
        type = "simple",
        // keep it invisible just in case
        catalog_visibility = "hidden",
        virtual_ = true
      };
      string json = JsonSerializer.Serialize(payload);

      int? createdId = null;

      try
      {
        // 1) Create draft product (POST)
        using (var req = new HttpRequestMessage(HttpMethod.Post, productsUrl))
        {
          req.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
          req.Headers.UserAgent.ParseAdd("WooToWoo/1.0");
          req.Content = new StringContent(json, Encoding.UTF8, "application/json");

          using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
          if (!resp.IsSuccessStatusCode)
          {
            var body = await SafeReadAsync(resp, ct);
            return (false, ExplainFailure("POST", productsUrl, resp.StatusCode, body));
          }

          // Expect JSON with "id"
          var bytes = await resp.Content.ReadAsByteArrayAsync(ct);
          using var doc = JsonDocument.Parse(bytes);
          if (doc.RootElement.TryGetProperty("id", out var idProp) && idProp.TryGetInt32(out var id))
          {
            createdId = id;
          }
          else
          {
            return (false, "Write check: product created but no ID returned.");
          }
        }

        // 2) Delete it (DELETE ?force=true)
        string deleteUrl = $"{productsUrl}/{createdId}?force=true";
        using (var del = new HttpRequestMessage(HttpMethod.Delete, deleteUrl))
        {
          del.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
          del.Headers.UserAgent.ParseAdd("WooToWoo/1.0");

          using var resp = await http.SendAsync(del, HttpCompletionOption.ResponseHeadersRead, ct);
          if (!resp.IsSuccessStatusCode)
          {
            var body = await SafeReadAsync(resp, ct);
            // try soft delete (move to trash) as a fallback
            string trashUrl = $"{productsUrl}/{createdId}";
            using var del2 = new HttpRequestMessage(HttpMethod.Delete, trashUrl);
            del2.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
            del2.Headers.UserAgent.ParseAdd("WooToWoo/1.0");

            using var resp2 = await http.SendAsync(del2, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!resp2.IsSuccessStatusCode)
            {
              var body2 = await SafeReadAsync(resp2, ct);
              return (true, $"Write check passed (created product #{createdId}) but cleanup failed. DELETE errors: force={resp.StatusCode} {Short(body)}; trash={resp2.StatusCode} {Short(body2)}");
            }
          }
        }

        return (true, $"Write access OK for {baseUrl} (created and deleted test product #{createdId}).");
      }
      catch (TaskCanceledException) when (ct.IsCancellationRequested)
      {
        return (false, "Write check canceled.");
      }
      catch (Exception ex)
      {
        return (false, $"Unexpected error during write check: {ex.Message}");
      }

    }


    private static async Task<string> SafeReadAsync(HttpResponseMessage resp, CancellationToken ct)
    {
      try { return await resp.Content.ReadAsStringAsync(ct); }
      catch { return string.Empty; }
    }

    private static string ExplainFailure(string verb, string url, HttpStatusCode code, string body)
    {
      var msg = new StringBuilder();
      msg.Append($"{verb} {url} failed with {(int)code} {code}.");
      // Common Woo errors include JSON with "message"
      try
      {
        using var doc = JsonDocument.Parse(body);
        if (doc.RootElement.TryGetProperty("message", out var m))
          msg.Append($" {m.GetString()}");
      }
      catch { /* body not JSON */ }

      // Hints
      if (code == HttpStatusCode.Unauthorized || code == HttpStatusCode.Forbidden)
        msg.Append(" Check that the REST key has Read/Write permissions and belongs to a user with manage_woocommerce caps, and that you are using HTTPS.");

      return msg.ToString();
    }

    private static string Short(string s) =>
        string.IsNullOrWhiteSpace(s) ? "" : (s.Length <= 220 ? s : s.Substring(0, 220) + "…");


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
