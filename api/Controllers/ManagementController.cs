using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using url.Models;
using url.Services;

namespace url.Controllers
{
    [ApiController]
    [Route("api")]
    public class ManagementController : ControllerBase
    {
        private readonly NestRipClient _client;
        private readonly IMongoCollection<StoredToken> _tokens;

        public ManagementController(NestRipClient client, IMongoDatabase database)
        {
            _client = client;
            _tokens = database.GetCollection<StoredToken>("tokens");
        }

        private async Task<StoredToken?> GetUserTokenAsync()
        {
            if (!Request.Cookies.TryGetValue("NestRipUserId", out var userId) || string.IsNullOrEmpty(userId))
            {
                return null;
            }

            var filter = Builders<StoredToken>.Filter.Eq(t => t.UserId, userId);
            return await _tokens.Find(filter).FirstOrDefaultAsync();
        }

        [HttpGet("motd")]
        public async Task<IActionResult> GetMotd()
        {
            try
            {
                var motd = await _client.GetMotdAsync();
                return Ok(motd);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("welcome")]
        public async Task<IActionResult> GetWelcome()
        {
            try
            {
                var welcome = await _client.GetWelcomeAsync();
                return Ok(welcome);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("version")]
        public async Task<IActionResult> GetVersion()
        {
            try
            {
                var version = await _client.GetVersionAsync();
                return Ok(version);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var user = await _client.GetUserInfoAsync();
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var stats = await _client.GetFileStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("files")]
        public async Task<IActionResult> GetFiles(int page = 1, int limit = 50, string sortColumn = "created_at", string sortDirection = "desc", string? search = null, bool showPastes = false)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var files = await _client.GetFilesAsync(page, limit, sortColumn, sortDirection, search, showPastes);
                return Ok(files);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("files/{id}")]
        public async Task<IActionResult> GetFileInfo(string id)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var file = await _client.GetFileInfoAsync(id);
                return Ok(file);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("files")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string? folder = null)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                using var stream = file.OpenReadStream();
                var response = await _client.UploadFileAsync(stream, file.FileName, folder);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("files/{id}")]
        public async Task<IActionResult> DeleteFile(string id)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                await _client.DeleteFileAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("folders")]
        public async Task<IActionResult> GetFolders()
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var folders = await _client.GetFoldersAsync();
                return Ok(folders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("folders")]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderRequest request)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var folder = await _client.CreateFolderAsync(request.Name, request.Color);
                return Ok(folder);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPatch("folders/{id}")]
        public async Task<IActionResult> UpdateFolder(string id, [FromBody] UpdateFolderRequest request)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var folder = await _client.UpdateFolderAsync(id, request.Name, request.Color);
                return Ok(folder);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("folders/{id}")]
        public async Task<IActionResult> DeleteFolder(string id)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                await _client.DeleteFolderAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("folders/{id}/add")]
        public async Task<IActionResult> AddFilesToFolder(string id, [FromBody] AddFilesToFolderRequest request)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var result = await _client.AddFilesToFolderAsync(id, request.Files!);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("folders/{id}/remove")]
        public async Task<IActionResult> RemoveFilesFromFolder(string id, [FromBody] RemoveFilesFromFolderRequest request)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var result = await _client.RemoveFilesFromFolderAsync(id, request.Files!);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("shorts")]
        public async Task<IActionResult> GetShorts()
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var shorts = await _client.GetShortsAsync();
                return Ok(shorts);
            }
            catch (Exception ex)
            {
                return Ok(new List<Short>());
            }
        }

        [HttpGet("shorts/info")]
        public async Task<IActionResult> GetShortsInfo()
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var info = await _client.GetShortInfoAsync();
                return Ok(info);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("shorts/{id}")]
        public async Task<IActionResult> GetShortDetails(string id)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var shortDetails = await _client.GetShortDetailsAsync(id);
                return Ok(shortDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("shorts")]
        public async Task<IActionResult> CreateShort([FromBody] CreateShortRequest request)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var result = await _client.CreateShortAsync(request.Url!, request.Custom);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("shorts/{id}")]
        public async Task<IActionResult> DeleteShort(string id)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                await _client.DeleteShortAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("domains")]
        public async Task<IActionResult> GetDomains()
        {
            try
            {
                var domains = await _client.GetDomainsAsync();
                return Ok(domains);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("email")]
        public async Task<IActionResult> GetEmailInfo()
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var info = await _client.GetEmailInfoAsync();
                return Ok(info);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("email/aliases")]
        public async Task<IActionResult> CreateEmailAlias([FromBody] CreateAliasRequest request)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var alias = await _client.CreateEmailAliasAsync(request.Alias!, request.ForwardTo!);
                return Ok(alias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPatch("email/aliases/{id}")]
        public async Task<IActionResult> UpdateEmailAlias(string id, [FromBody] UpdateAliasRequest request)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                var alias = await _client.UpdateEmailAliasAsync(id, request.Alias, request.ForwardTo);
                return Ok(alias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("email/aliases/{id}")]
        public async Task<IActionResult> DeleteEmailAlias(string id)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token == null)
                    return Unauthorized();

                _client.SetAccessToken(token.AccessToken!);
                await _client.DeleteEmailAliasAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("user/profile/{id}")]
        public async Task<IActionResult> GetUserProfile(string id)
        {
            try
            {
                var token = await GetUserTokenAsync();
                if (token != null)
                {
                    _client.SetAccessToken(token.AccessToken!);
                }
                var user = await _client.GetUserProfileAsync(id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
