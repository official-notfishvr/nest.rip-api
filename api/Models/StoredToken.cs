using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace url.Models
{
    public class StoredToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string? AccessToken { get; set; }
        public string? UserId { get; set; }
        public string? RefreshToken { get; set; }
        public string? TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string? Scope { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
