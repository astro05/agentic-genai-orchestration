namespace TicketSystem.API.DTOs;

public class AIClassifyRequest
{
    public string Description { get; set; } = string.Empty;
}

public class AIClassifyResponse
{
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}

public class ReplyAssistSourceDto
{
    public string ArticleId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class ReplyAssistResponseDto
{
    public string Draft { get; set; } = string.Empty;
    public List<ReplyAssistSourceDto> Sources { get; set; } = new();
}