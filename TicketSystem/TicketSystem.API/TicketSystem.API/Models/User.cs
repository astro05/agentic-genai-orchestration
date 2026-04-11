using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TicketSystem.API.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Customer"; // Admin | Agent | Customer
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}