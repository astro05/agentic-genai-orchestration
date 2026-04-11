using MongoDB.Driver;
using TicketSystem.API.DTOs;
using TicketSystem.API.Helpers;
using TicketSystem.API.Models;

namespace TicketSystem.API.Services;

public class AuthService
{
    private readonly IMongoCollection<User> _users;
    private readonly JwtHelper _jwtHelper;

    public AuthService(IConfiguration config, JwtHelper jwtHelper)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
        _users = db.GetCollection<User>("users");
        _jwtHelper = jwtHelper;
    }

    public async Task<string?> RegisterAsync(RegisterDto dto)
    {
        var exists = await _users.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
        if (exists != null) return null;

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role
        };

        await _users.InsertOneAsync(user);
        return _jwtHelper.GenerateToken(user);
    }

    public async Task<string?> LoginAsync(LoginDto dto)
    {
        var user = await _users.Find(u => u.Email == dto.Email).FirstOrDefaultAsync();
        if (user == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) return null;
        return _jwtHelper.GenerateToken(user);
    }
}