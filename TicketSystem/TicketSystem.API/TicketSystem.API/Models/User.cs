using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TicketSystem.API.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("fullName")]
        public string FullName { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("role")]
        public UserRole Role { get; set; } = UserRole.Customer;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ticket category enum names this agent handles (e.g. AuthenticationIssue).
        /// Empty or null means the agent is a generalist and may receive any category when routing.
        /// </summary>
        [BsonElement("handledCategories")]
        public List<string> HandledCategories { get; set; } = new();
    }
}