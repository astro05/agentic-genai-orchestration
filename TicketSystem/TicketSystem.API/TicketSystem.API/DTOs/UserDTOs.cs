using TicketSystem.API.Models;

namespace TicketSystem.API.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    /// <summary>Enum names of ticket categories this agent handles; empty for generalists.</summary>
    public List<string> HandledCategories { get; set; } = new();
}

public class UpdateRoleRequest
{
    public string UserId { get; set; } = string.Empty;
    public UserRole NewRole { get; set; }
}
