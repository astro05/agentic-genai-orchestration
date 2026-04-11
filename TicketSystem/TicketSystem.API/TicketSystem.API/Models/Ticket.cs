using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TicketSystem.API.Models;

public class Ticket
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Open"; // Open | In Progress | Resolved
    public string Priority { get; set; } = "Low"; // Low | Medium | High
    public string Category { get; set; } = string.Empty; // AI-generated
    public string CreatedBy { get; set; } = string.Empty; // User ID
    public string? AssignedTo { get; set; } // Agent User ID
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}