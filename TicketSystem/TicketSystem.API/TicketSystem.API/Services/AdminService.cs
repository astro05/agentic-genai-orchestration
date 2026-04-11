using MongoDB.Driver;
using TicketSystem.API.DTOs;
using TicketSystem.API.Models;
using TicketSystem.API.Settings;

namespace TicketSystem.API.Services
{
    public class AdminService
    {
        private readonly IMongoCollection<User> _users;

        public AdminService(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<User>(settings.UsersCollection);
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _users.Find(_ => true).SortByDescending(u => u.CreatedAt).ToListAsync();
            return users.Select(MapToDto).ToList();
        }

        public async Task<List<UserDto>> GetAgentsAsync()
        {
            var agents = await _users.Find(u => u.Role == UserRole.Agent && u.IsActive).ToListAsync();
            return agents.Select(MapToDto).ToList();
        }

        public async Task<bool> UpdateRoleAsync(string userId, UserRole newRole)
        {
            var update = Builders<User>.Update.Set(u => u.Role, newRole);
            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var update = Builders<User>.Update.Set(u => u.IsActive, false);
            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.MatchedCount > 0;
        }

        public async Task<bool> ActivateUserAsync(string userId)
        {
            var update = Builders<User>.Update.Set(u => u.IsActive, true);
            var result = await _users.UpdateOneAsync(u => u.Id == userId, update);
            return result.MatchedCount > 0;
        }

        /// <summary>Permanently removes the user document. Cannot delete your own account.</summary>
        public async Task<(bool success, string? error)> PermanentlyDeleteUserAsync(string userId, string? actingAdminId)
        {
            if (actingAdminId != null && userId == actingAdminId)
                return (false, "cannot_delete_self");

            var result = await _users.DeleteOneAsync(u => u.Id == userId);
            return result.DeletedCount > 0 ? (true, null) : (false, "not_found");
        }

        public async Task<bool> CreateUserAsync(RegisterRequest req)
        {
            var existing = await _users.Find(u => u.Email == req.Email).FirstOrDefaultAsync();
            if (existing != null) return false;

            var user = new User
            {
                FullName = req.FullName,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = req.Role
            };
            await _users.InsertOneAsync(user);
            return true;
        }

        private static UserDto MapToDto(User u) => new()
        {
            Id = u.Id!,
            FullName = u.FullName,
            Email = u.Email,
            Role = u.Role.ToString(),
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            HandledCategories = u.HandledCategories ?? new List<string>()
        };
    }
}