using TicketSystem.API.Models;

namespace TicketSystem.API.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string userId);
    Task<User?> GetByEmailAsync(string email);
    Task<List<User>> GetAllAsync();
    Task<List<User>> GetActiveAgentsAsync();
    Task<List<User>> GetAgentsForRoutingAsync();
    Task InsertAsync(User user);
    Task<bool> UpdateRoleAsync(string userId, UserRole newRole);
    Task<bool> SetActiveAsync(string userId, bool isActive);
    Task<bool> DeletePermanentAsync(string userId);
}
