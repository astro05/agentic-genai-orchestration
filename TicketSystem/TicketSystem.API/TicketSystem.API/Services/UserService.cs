using MongoDB.Driver;
using TicketSystem.API.Models;

namespace TicketSystem.API.Services;

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(IConfiguration config)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
        _users = db.GetCollection<User>("users");
    }

    public async Task<List<User>> GetAllAsync() =>
        await _users.Find(_ => true).ToListAsync();

    public async Task<User?> GetByIdAsync(string id) =>
        await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task UpdateRoleAsync(string id, string role)
    {
        var update = Builders<User>.Update.Set(u => u.Role, role);
        await _users.UpdateOneAsync(u => u.Id == id, update);
    }

    public async Task DeleteAsync(string id) =>
        await _users.DeleteOneAsync(u => u.Id == id);
}