using MongoDB.Bson.Serialization.Attributes;

namespace TicketSystem.API.Models;

public class TicketMessage
{
    [BsonElement("id")]
    public string Id { get; set; } = string.Empty;

    [BsonElement("authorRole")]
    public UserRole AuthorRole { get; set; }

    [BsonElement("authorUserId")]
    public string AuthorUserId { get; set; } = string.Empty;

    [BsonElement("authorName")]
    public string AuthorName { get; set; } = string.Empty;

    [BsonElement("body")]
    public string Body { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //Optional id of another message on the same ticket this entry replies to.
    [BsonElement("replyToMessageId")]
    public string? ReplyToMessageId { get; set; }
}
