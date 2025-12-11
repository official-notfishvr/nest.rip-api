using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using url.Models;
using url.Services;

namespace url.Controllers
{
    [ApiController]
    [Route("oauth")]
    public class OAuthController : ControllerBase
    {
        private readonly NestRipClient _client;
        private readonly IConfiguration _configuration;
        private readonly IMongoCollection<StoredToken> _tokens;

        public OAuthController(NestRipClient client, IConfiguration configuration, IMongoDatabase database)
        {
            _client = client;
            _configuration = configuration;
            _tokens = database.GetCollection<StoredToken>("tokens");
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback(string code)
        {
            var clientId = _configuration["NestRip:ClientId"];
            var clientSecret = _configuration["NestRip:ClientSecret"];
            var redirectUri = _configuration["NestRip:RedirectUri"];
            var dashboardUrl = _configuration["Server:DashboardUrl"] ?? "";
            var cookieDomain = _configuration["Server:CookieDomain"] ?? "";

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
            {
                return BadRequest("Missing OAuth configuration.");
            }

            try
            {
                var token = await _client.GetTokenAsync(code, redirectUri, clientId, clientSecret);

                if (token != null && !string.IsNullOrEmpty(token.AccessToken))
                {
                    string userId = GetUserIdFromToken(token.AccessToken);

                    var storedToken = new StoredToken
                    {
                        UserId = userId,
                        AccessToken = token.AccessToken,
                        RefreshToken = token.RefreshToken,
                        TokenType = token.TokenType,
                        ExpiresIn = token.ExpiresIn,
                        Scope = token.Scope,
                        CreatedAt = DateTime.UtcNow,
                    };

                    var filter = Builders<StoredToken>.Filter.Eq(t => t.UserId, userId);
                    await _tokens.ReplaceOneAsync(filter, storedToken, new ReplaceOptions { IsUpsert = true });

                    Response.Cookies.Append(
                        "NestRipUserId",
                        userId,
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                            Domain = cookieDomain,
                            Expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn),
                        }
                    );

                    return Redirect(dashboardUrl);
                }

                return BadRequest("Failed to retrieve access token.");
            }
            catch (Exception ex)
            {
                return BadRequest($"OAuth failed: {ex.Message}");
            }
        }

        private string GetUserIdFromToken(string accessToken)
        {
            try
            {
                var parts = accessToken.Split('.');
                if (parts.Length < 2)
                    return "unknown";

                var payload = parts[1];
                switch (payload.Length % 4)
                {
                    case 2:
                        payload += "==";
                        break;
                    case 3:
                        payload += "=";
                        break;
                }
                var jsonBytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
                var json = Encoding.UTF8.GetString(jsonBytes);

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("sub", out var sub))
                {
                    return sub.GetString() ?? "unknown";
                }
                return "unknown";
            }
            catch
            {
                return "unknown";
            }
        }
    }
}
