using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TicketSystem.API.Models
{
    public class Ticket
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("status")]
        public TicketStatus Status { get; set; } = TicketStatus.Open;

        [BsonElement("priority")]
        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        [BsonElement("category")]
        public TicketCategory Category { get; set; } = TicketCategory.UncategorizedIssue;

        [BsonElement("createdById")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedById { get; set; } = string.Empty;

        [BsonElement("createdByName")]
        public string CreatedByName { get; set; } = string.Empty;

        [BsonElement("assignedToId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? AssignedToId { get; set; }

        [BsonElement("assignedToName")]
        public string? AssignedToName { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("resolvedAt")]
        public DateTime? ResolvedAt { get; set; }

        [BsonElement("agentNotes")]
        public string AgentNotes { get; set; } = string.Empty;

        [BsonElement("messages")]
        public List<TicketMessage> Messages { get; set; } = new();
    }
}