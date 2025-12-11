using System.Text.Json.Serialization;

namespace url.Models
{
    public class ErrorResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class WelcomeResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class VersionResponse
    {
        [JsonPropertyName("environment")]
        public string? Environment { get; set; }

        [JsonPropertyName("commit")]
        public string? Commit { get; set; }
    }

    public class MotdResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("message_parsed")]
        public string? MessageParsed { get; set; }

        [JsonPropertyName("creator")]
        public string? Creator { get; set; }

        [JsonPropertyName("creator_id")]
        public string? CreatorId { get; set; }

        [JsonPropertyName("creator_avatar")]
        public string? CreatorAvatar { get; set; }
    }

    public class OAuthTokenRequest
    {
        [JsonPropertyName("grant_type")]
        public string GrantType { get; set; } = "authorization_code";

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("redirect_uri")]
        public string? RedirectUri { get; set; }

        [JsonPropertyName("client_id")]
        public string? ClientId { get; set; }

        [JsonPropertyName("client_secret")]
        public string? ClientSecret { get; set; }
    }

    public class OAuthTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("scope")]
        public string? Scope { get; set; }
    }

    public class FileUploadResponse
    {
        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }

        [JsonPropertyName("cdnFileName")]
        public string? CdnFileName { get; set; }

        [JsonPropertyName("deletionURL")]
        public string? DeletionUrl { get; set; }

        [JsonPropertyName("fileURL")]
        public string? FileUrl { get; set; }

        [JsonPropertyName("accessibleURL")]
        public string? AccessibleUrl { get; set; }
    }

    public class FileListResponse
    {
        [JsonPropertyName("cdnPrefix")]
        public string? CdnPrefix { get; set; }

        [JsonPropertyName("hasApplicationUploads")]
        public bool HasApplicationUploads { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("totalUploads")]
        public int TotalUploads { get; set; }

        [JsonPropertyName("uploads")]
        public List<FileItem>? Uploads { get; set; }
    }

    public class FileItem
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("filename")]
        public string? Filename { get; set; }

        [JsonPropertyName("cdn_file_name")]
        public string? CdnFileName { get; set; }

        [JsonPropertyName("original_filename")]
        public string? OriginalFilename { get; set; }

        [JsonPropertyName("mime_type")]
        public string? MimeType { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("extra_size")]
        public long ExtraSize { get; set; }

        [JsonPropertyName("paste")]
        public bool Paste { get; set; }

        [JsonPropertyName("public")]
        public bool Public { get; set; }

        [JsonPropertyName("should_embed")]
        public bool ShouldEmbed { get; set; }

        [JsonPropertyName("hash")]
        public string? Hash { get; set; }

        [JsonPropertyName("user_agent")]
        public string? UserAgent { get; set; }

        [JsonPropertyName("exploding")]
        public bool Exploding { get; set; }

        [JsonPropertyName("archived")]
        public bool Archived { get; set; }

        [JsonPropertyName("views")]
        public int Views { get; set; }

        [JsonPropertyName("width")]
        public int? Width { get; set; }

        [JsonPropertyName("height")]
        public int? Height { get; set; }

        [JsonPropertyName("applicationUpload")]
        public bool ApplicationUpload { get; set; }

        [JsonPropertyName("applicationName")]
        public string? ApplicationName { get; set; }

        [JsonPropertyName("accessibleURL")]
        public string? AccessibleUrl { get; set; }
    }

    public class FileStatsResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("uploads")]
        public int Uploads { get; set; }

        [JsonPropertyName("storageUsed")]
        public long StorageUsed { get; set; }

        [JsonPropertyName("maxQuota")]
        public long MaxQuota { get; set; }
    }

    public class Folder
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("slug")]
        public string? Slug { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("public")]
        public bool Public { get; set; }

        [JsonPropertyName("file_count")]
        public int FileCount { get; set; }

        [JsonPropertyName("files")]
        public List<FolderFile>? Files { get; set; }
    }

    public class FolderFile
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("cdn_file_name")]
        public string? CdnFileName { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("mime_type")]
        public string? MimeType { get; set; }

        [JsonPropertyName("original_filename")]
        public string? OriginalFilename { get; set; }

        [JsonPropertyName("size")]
        public long Size { get; set; }

        [JsonPropertyName("preview_image")]
        public string? PreviewImage { get; set; }
    }

    public class FolderListResponse
    {
        [JsonPropertyName("folders")]
        public List<Folder>? Folders { get; set; }

        [JsonPropertyName("totalFolders")]
        public int TotalFolders { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }
    }

    public class CreateFolderRequest
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("color")]
        public string? Color { get; set; }
    }

    public class Short
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("short")]
        public string? ShortCode { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("views")]
        public int Views { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class CreateShortRequest
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("custom")]
        public string? Custom { get; set; }
    }

    public class User
    {
        [JsonPropertyName("sub")]
        public string? Sub { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }

        [JsonPropertyName("preferred_username")]
        public string? PreferredUsername { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }

        [JsonPropertyName("profile")]
        public string? Profile { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("discord")]
        public string? Discord { get; set; }
    }

    public class ShortInfoResponse
    {
        [JsonPropertyName("totalShorts")]
        public int TotalShorts { get; set; }

        [JsonPropertyName("totalViews")]
        public int TotalViews { get; set; }
    }

    public class Domain
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("domain")]
        public string? DomainName { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("usableBy")]
        public string? UsableBy { get; set; }

        [JsonPropertyName("expiration")]
        public DateTime? Expiration { get; set; }

        [JsonPropertyName("permanent")]
        public bool Permanent { get; set; }
    }

    public class UserProfile
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("uid")]
        public int Uid { get; set; }

        [JsonPropertyName("uploads")]
        public int Uploads { get; set; }

        [JsonPropertyName("storage_used")]
        public long StorageUsed { get; set; }

        [JsonPropertyName("shorts_count")]
        public int ShortsCount { get; set; }

        [JsonPropertyName("banned")]
        public bool Banned { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("profile_private")]
        public bool ProfilePrivate { get; set; }

        [JsonPropertyName("has_turbo")]
        public bool HasTurbo { get; set; }

        [JsonPropertyName("profile_bio")]
        public string? ProfileBio { get; set; }

        [JsonPropertyName("profile_color")]
        public string? ProfileColor { get; set; }

        [JsonPropertyName("rank")]
        public string? Rank { get; set; }

        [JsonPropertyName("premium_started_at")]
        public DateTime? PremiumStartedAt { get; set; }

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }

        [JsonPropertyName("invited_by")]
        public InvitedBy? InvitedBy { get; set; }

        [JsonPropertyName("profile_background")]
        public string? ProfileBackground { get; set; }

        [JsonPropertyName("profile_show_stats")]
        public bool ProfileShowStats { get; set; }

        [JsonPropertyName("profile_layout")]
        public string? ProfileLayout { get; set; }

        [JsonPropertyName("contributor")]
        public bool Contributor { get; set; }

        [JsonPropertyName("badges")]
        public List<string>? Badges { get; set; }

        [JsonPropertyName("invited_users")]
        public List<InvitedUser>? InvitedUsers { get; set; }

        [JsonPropertyName("discord_id")]
        public string? DiscordId { get; set; }

        [JsonPropertyName("discord_user")]
        public string? DiscordUser { get; set; }

        [JsonPropertyName("discord_avatar")]
        public string? DiscordAvatar { get; set; }
    }

    public class InvitedBy
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }
    }

    public class InvitedUser
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }
    }

    public class UpdateFolderRequest
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("color")]
        public string? Color { get; set; }
    }

    public class AddFilesToFolderRequest
    {
        [JsonPropertyName("files")]
        public List<string>? Files { get; set; }
    }

    public class RemoveFilesFromFolderRequest
    {
        [JsonPropertyName("files")]
        public List<string>? Files { get; set; }
    }

    public class FolderOperationResponse
    {
        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("addedCount")]
        public int? AddedCount { get; set; }

        [JsonPropertyName("removedCount")]
        public int? RemovedCount { get; set; }
    }

    public class EmailInfoResponse
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("aliases")]
        public List<EmailAlias>? Aliases { get; set; }
    }

    public class EmailAlias
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("alias")]
        public string? Alias { get; set; }

        [JsonPropertyName("forwardTo")]
        public string? ForwardTo { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAliasRequest
    {
        [JsonPropertyName("alias")]
        public string? Alias { get; set; }

        [JsonPropertyName("forwardTo")]
        public string? ForwardTo { get; set; }
    }

    public class UpdateAliasRequest
    {
        [JsonPropertyName("alias")]
        public string? Alias { get; set; }

        [JsonPropertyName("forwardTo")]
        public string? ForwardTo { get; set; }
    }
}
