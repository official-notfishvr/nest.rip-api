using Microsoft.AspNetCore.Mvc;

namespace url.Controllers
{
    [ApiController]
    [Route("api/proxy")]
    public class ImageProxyController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ImageProxyController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("image")]
        public async Task<IActionResult> ProxyImage([FromQuery] string? url = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest(new { message = "URL parameter is required" });
            }

            try
            {
                var decodedUrl = url;

                if (!decodedUrl.StartsWith("https://"))
                {
                    return BadRequest(new { message = "URL must be a valid HTTPS URL" });
                }

                if (!decodedUrl.StartsWith("https://nest.rip") && !decodedUrl.StartsWith("https://cdn.nest.rip"))
                {
                    return BadRequest(new { message = "Only nest.rip URLs are allowed" });
                }

                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

                using var client = new HttpClient(handler);
                using var request = new HttpRequestMessage(HttpMethod.Get, decodedUrl);

                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                request.Headers.Add("Referer", "https://nest.rip/");

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode);
                }

                var content = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

                return File(content, contentType);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("api")]
        public async Task<IActionResult> ProxyApi([FromQuery] string path, [FromQuery] string? token = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                return BadRequest(new { message = "path parameter is required" });
            }

            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    if (!Request.Cookies.TryGetValue("NestRipUserId", out var userId))
                    {
                        return Unauthorized(new { message = "No token provided and user not authenticated" });
                    }
                    return BadRequest(new { message = "Token parameter is required" });
                }

                var apiUrl = $"https://nest.rip/api/{path}";

                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
                handler.AutomaticDecompression = System.Net.DecompressionMethods.All;

                using var client = new HttpClient(handler);
                using var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:146.0) Gecko/20100101 Firefox/146.0");
                request.Headers.Add("Accept", "application/json, text/plain, */*");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
                request.Headers.Add("Authorization", $"Bearer {token}");
                request.Headers.Add("Referer", "https://nest.rip/dash/files");
                request.Headers.Add("Sec-Fetch-Dest", "empty");
                request.Headers.Add("Sec-Fetch-Mode", "cors");
                request.Headers.Add("Sec-Fetch-Site", "same-origin");

                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode);
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}