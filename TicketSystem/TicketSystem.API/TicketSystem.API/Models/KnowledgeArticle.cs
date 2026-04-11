using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TicketSystem.API.Models;

/// <summary>Knowledge base article used for reply-assist and routing context.</summary>
public class KnowledgeArticle
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("summary")]
    public string Summary { get; set; } = string.Empty;

    [BsonElement("body")]
    public string Body { get; set; } = string.Empty;

    /// <summary>Primary category this article applies to.</summary>
    [BsonElement("category")]
    public TicketCategory Category { get; set; } = TicketCategory.GeneralInquiry;

    [BsonElement("keywords")]
    public List<string> Keywords { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
