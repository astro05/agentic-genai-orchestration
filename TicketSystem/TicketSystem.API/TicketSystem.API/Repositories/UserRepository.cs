using MongoDB.Driver;
using TicketSystem.API.Data;
using TicketSystem.API.Models;

namespace TicketSystem.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(MongoDbContext dbContext)
    {
        _users = dbContext.Users;
    }

    public async Task<User?> GetByIdAsync(string userId) =>
        await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();

    public async Task<User?> GetByEmailAsync(string email) =>
        await _users.Find(u => u.Email == email).FirstOrDefaultAsync();

    public Task<List<User>> GetAllAsync() =>
        _users.Find(_ => true).SortByDescending(u => u.CreatedAt).ToListAsync();

    public Task<List<User>> GetActiveAgentsAsync() =>
        _users.Find(u => u.Role == UserRole.Agent && u.IsActive).ToListAsync();

    public Task<List<User>> GetAgentsForRoutingAsync() =>
        _users.Find(u => u.Role == UserRole.Agent && u.IsActive).ToListAsync();

    public Task InsertAsync(User user) =>
        _users.InsertOneAsync(user);

    public async Task<bool> UpdateRoleAsync(string userId, UserRole newRole)
    {
        var update = Builders<User>.Update.Set(u => u.Role, newRole);
        var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> SetActiveAsync(string userId, bool isActive)
    {
        var update = Builders<User>.Update.Set(u => u.IsActive, isActive);
        var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeletePermanentAsync(string userId)
    {
        var result = await _users.DeleteOneAsync(u => u.Id == userId);
        return result.DeletedCount > 0;
    }
}
