using TicketSystem.API.Models;

namespace TicketSystem.API.DTOs
{
    // ─── Auth DTOs ───────────────────────────────────────────────
    public class RegisterRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Customer;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // ─── User DTOs ───────────────────────────────────────────────
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateRoleRequest
    {
        public string UserId { get; set; } = string.Empty;
        public UserRole NewRole { get; set; }
    }

    // ─── Ticket DTOs ─────────────────────────────────────────────
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

    // ─── AI DTOs ─────────────────────────────────────────────────
    public class AIClassifyRequest
    {
        public string Description { get; set; } = string.Empty;
    }

    public class AIClassifyResponse
    {
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
    }
}