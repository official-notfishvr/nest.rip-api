using System.Net.Http.Json;
using System.Text.Json;
using url.Models;

namespace url.Services
{
    public class NestRipClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NestRipClient> _logger;
        private readonly bool _enableResponseLogging;
        private string? _accessToken;

        public NestRipClient(HttpClient httpClient, IConfiguration configuration, ILogger<NestRipClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _enableResponseLogging = configuration.GetValue<bool>("NestRip:EnableResponseLogging", false);

            if (_httpClient.BaseAddress == null)
            {
                var baseUrl = configuration["NestRip:BaseUrl"];
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    _httpClient.BaseAddress = new Uri(baseUrl);
                }
            }
        }

        public void SetAccessToken(string token)
        {
            _accessToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
        }

        private void LogResponse(string method, int statusCode, string content)
        {
            if (_enableResponseLogging)
            {
                _logger.LogInformation("{Method} response: {StatusCode} - {Content}", method, statusCode, content);
            }
        }

        private async Task<T?> GetWithLoggingAsync<T>(string endpoint)
            where T : class
        {
            var response = await _httpClient.GetAsync(endpoint);
            var content = await response.Content.ReadAsStringAsync();
            LogResponse($"GET {endpoint}", (int)response.StatusCode, content);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<MotdResponse?> GetMotdAsync()
        {
            return await GetWithLoggingAsync<MotdResponse>("motd");
        }

        public async Task<WelcomeResponse?> GetWelcomeAsync()
        {
            return await GetWithLoggingAsync<WelcomeResponse>("");
        }

        public async Task<VersionResponse?> GetVersionAsync()
        {
            return await GetWithLoggingAsync<VersionResponse>("version");
        }

        public async Task<OAuthTokenResponse?> GetTokenAsync(string code, string redirectUri, string clientId, string clientSecret)
        {
            var formData = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "client_id", clientId },
                { "client_secret", clientSecret },
            };
            var content = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync("oauth/token", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            LogResponse("POST oauth/token", (int)response.StatusCode, responseContent);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<OAuthTokenResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<User?> GetUserInfoAsync()
        {
            return await GetWithLoggingAsync<User>("oauth/userinfo");
        }

        public async Task<FileListResponse?> GetFilesAsync(int page = 1, int limit = 50, string sortColumn = "created_at", string sortDirection = "desc", string? search = null, bool showPastes = false)
        {
            var query = $"files?page={page}&limit={limit}&sortColumn={sortColumn}&sortDirection={sortDirection}&showPastes={showPastes.ToString().ToLower()}";
            if (!string.IsNullOrEmpty(search))
            {
                query += $"&search={Uri.EscapeDataString(search)}";
            }

            return await GetWithLoggingAsync<FileListResponse>(query);
        }

        public async Task<FileStatsResponse?> GetFileStatsAsync()
        {
            return await GetWithLoggingAsync<FileStatsResponse>("files/info");
        }

        public async Task<FileItem?> GetFileInfoAsync(string fileId)
        {
            return await GetWithLoggingAsync<FileItem>($"files/info/{fileId}");
        }

        public async Task<FileUploadResponse?> UploadFileAsync(Stream fileStream, string fileName, string? folderId = null)
        {
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "file", fileName);

            if (!string.IsNullOrEmpty(folderId))
            {
                content.Add(new StringContent(folderId), "folder");
            }

            var response = await _httpClient.PostAsync("files/upload", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            LogResponse("POST files/upload", (int)response.StatusCode, responseContent);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<FileUploadResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task DeleteFileAsync(string fileId)
        {
            var response = await _httpClient.DeleteAsync($"files/{fileId}");
            var content = await response.Content.ReadAsStringAsync();
            LogResponse($"DELETE files/{fileId}", (int)response.StatusCode, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<Folder?> CreateFolderAsync(string name, string? color = null)
        {
            var request = new CreateFolderRequest { Name = name, Color = color };
            var response = await _httpClient.PutAsJsonAsync("files/folders", request);
            var content = await response.Content.ReadAsStringAsync();
            LogResponse("PUT files/folders", (int)response.StatusCode, content);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<Folder>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<FolderListResponse?> GetFoldersAsync()
        {
            return await GetWithLoggingAsync<FolderListResponse>("files/folders");
        }

        public async Task<Folder?> UpdateFolderAsync(string folderId, string? name, string? color)
        {
            var request = new UpdateFolderRequest { Name = name, Color = color };
            var response = await _httpClient.PatchAsJsonAsync($"files/folders/{folderId}", request);
            var content = await response.Content.ReadAsStringAsync();
            LogResponse($"PATCH files/folders/{folderId}", (int)response.StatusCode, content);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<Folder>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task DeleteFolderAsync(string folderId)
        {
            var response = await _httpClient.DeleteAsync($"files/folders/{folderId}");
            var content = await response.Content.ReadAsStringAsync();
            LogResponse($"DELETE files/folders/{folderId}", (int)response.StatusCode, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<FolderOperationResponse?> AddFilesToFolderAsync(string folderId, List<string> fileIds)
        {
            var request = new AddFilesToFolderRequest { Files = fileIds };
            var response = await _httpClient.PostAsJsonAsync($"files/folders/{folderId}/add", request);
            var content = await response.Content.ReadAsStringAsync();
            LogResponse($"POST files/folders/{folderId}/add", (int)response.StatusCode, content);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<FolderOperationResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<FolderOperationResponse?> RemoveFilesFromFolderAsync(string folderId, List<string> fileIds)
        {
            var request = new RemoveFilesFromFolderRequest { Files = fileIds };
            var response = await _httpClient.PostAsJsonAsync($"files/folders/{folderId}/remove", request);
            var content = await response.Content.ReadAsStringAsync();
            LogResponse($"POST files/folders/{folderId}/remove", (int)response.StatusCode, content);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<FolderOperationResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<Short?> CreateShortAsync(string url, string? customCode = null)
        {
            var request = new CreateShortRequest { Url = url, Custom = customCode };
            var response = await _httpClient.PutAsJsonAsync("shorts", request);
            var content = await response.Content.ReadAsStringAsync();
            LogResponse("PUT shorts", (int)response.StatusCode, content);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<Short>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<List<Short>?> GetShortsAsync()
        {
            return await GetWithLoggingAsync<List<Short>>("shorts/");
        }

        public async Task<ShortInfoResponse?> GetShortInfoAsync()
        {
            return await GetWithLoggingAsync<ShortInfoResponse>("shorts/info");
        }

        public async Task<Short?> GetShortDetailsAsync(string shortId)
        {
            return await GetWithLoggingAsync<Short>($"shorts/{shortId}");
        }

        public async Task DeleteShortAsync(string shortId)
        {
            var response = await _httpClient.DeleteAsync($"shorts/{shortId}");
            var content = await response.Content.ReadAsStringAsync();
            LogResponse($"DELETE shorts/{shortId}", (int)response.StatusCode, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<Domain>?> GetDomainsAsync()
        {
            return await GetWithLoggingAsync<List<Domain>>("domains");
        }

        public async Task<UserProfile?> GetUserProfileAsync(string userId)
        {
            return await GetWithLoggingAsync<UserProfile>($"user/profile/{userId}");
        }

        public async Task<EmailInfoResponse?> GetEmailInfoAsync()
        {
            return await GetWithLoggingAsync<EmailInfoResponse>("email");
        }

        public async Task<EmailAlias?> CreateEmailAliasAsync(string alias, string forwardTo)
        {
            var request = new CreateAliasRequest { Alias = alias, ForwardTo = forwardTo };
            var response = await _httpClient.PutAsJsonAsync("email/aliases", request);
            var content = await response.Content.ReadAsStringAsync();
            LogResponse("PUT email/aliases", (int)response.StatusCode, content);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<EmailAlias>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<EmailAlias?> UpdateEmailAliasAsync(string aliasId, string? alias, string? forwardTo)
        {
            var request = new UpdateAliasRequest { Alias = alias, ForwardTo = forwardTo };
            var response = await _httpClient.PatchAsJsonAsync($"email/aliases/{aliasId}", request);
            var content = await response.Content.ReadAsStringAsync();
            LogResponse($"PATCH email/aliases/{aliasId}", (int)response.StatusCode, content);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<EmailAlias>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task DeleteEmailAliasAsync(string aliasId)
        {
            var response = await _httpClient.DeleteAsync($"email/aliases/{aliasId}");
            var content = await response.Content.ReadAsStringAsync();
            LogResponse($"DELETE email/aliases/{aliasId}", (int)response.StatusCode, content);
            response.EnsureSuccessStatusCode();
        }
    }
}