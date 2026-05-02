using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TicketSystem.API.DTOs;
using TicketSystem.API.Models;
using TicketSystem.API.Repositories;

namespace TicketSystem.API.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _config;

        public AuthService(IUserRepository userRepository, IConfiguration config)
        {
            _userRepository = userRepository;
            _config = config;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest req)
        {
            var existing = await _userRepository.GetByEmailAsync(req.Email);
            if (existing != null) return null;

            var user = new User
            {
                FullName = req.FullName,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = req.Role == UserRole.Admin ? UserRole.Customer : req.Role  // public cannot register as admin
            };

            await _userRepository.InsertAsync(user);
            return BuildAuthResponse(user);
        }

        public async Task<(AuthResponse? response, string? error)> LoginAsync(LoginRequest req)
        {
            // First check if user exists at all (regardless of active status)
            var user = await _userRepository.GetByEmailAsync(req.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return (null, "invalid_credentials");

            if (!user.IsActive)
                return (null, "user_deactivated");

            return (BuildAuthResponse(user), null);
        }

        private AuthResponse BuildAuthResponse(User user)
        {
            var token = GenerateJwt(user);
            return new AuthResponse
            {
                Token = token,
                UserId = user.Id!,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            };
        }

        private string GenerateJwt(User user)
        {
            var jwtKey = _config["JwtSettings:Secret"] ?? throw new Exception("JWT secret not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id!),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}