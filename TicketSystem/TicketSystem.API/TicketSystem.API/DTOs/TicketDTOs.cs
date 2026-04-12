using TicketSystem.API.Models;

namespace TicketSystem.API.DTOs;

public class CreateTicketRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateTicketStatusRequest
{
    public TicketStatus Status { get; set; }
}

public class AssignTicketRequest
{
    public string AgentId { get; set; } = string.Empty;
}

public class UpdateTicketNotesRequest
{
    public string Notes { get; set; } = string.Empty;
}

public class AddTicketMessageRequest
{
    public string Body { get; set; } = string.Empty;
    public string? ReplyToMessageId { get; set; }
}

public class TicketMessageDto
{
    public string Id { get; set; } = string.Empty;
    public string AuthorRole { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ReplyToMessageId { get; set; }
}

public class TicketDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public string? AssignedToName { get; set; }
    public string AgentNotes { get; set; } = string.Empty;
    public List<TicketMessageDto> Messages { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
