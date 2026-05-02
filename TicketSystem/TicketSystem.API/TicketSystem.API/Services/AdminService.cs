using TicketSystem.API.DTOs;
using TicketSystem.API.Models;
using TicketSystem.API.Repositories;

namespace TicketSystem.API.Services
{
    public class AdminService
    {
        private readonly IUserRepository _userRepository;

        public AdminService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto).ToList();
        }

        public async Task<List<UserDto>> GetAgentsAsync()
        {
            var agents = await _userRepository.GetActiveAgentsAsync();
            return agents.Select(MapToDto).ToList();
        }

        public async Task<bool> UpdateRoleAsync(string userId, UserRole newRole)
        {
            return await _userRepository.UpdateRoleAsync(userId, newRole);
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            return await _userRepository.SetActiveAsync(userId, false);
        }

        public async Task<bool> ActivateUserAsync(string userId)
        {
            return await _userRepository.SetActiveAsync(userId, true);
        }

        /// <summary>
        /// Permanently removes the user document. Cannot delete your own account.
        /// </summary>
        public async Task<(bool success, string? error)> PermanentlyDeleteUserAsync(string userId, string? actingAdminId)
        {
            if (actingAdminId != null && userId == actingAdminId)
                return (false, "cannot_delete_self");

            var deleted = await _userRepository.DeletePermanentAsync(userId);
            return deleted ? (true, null) : (false, "not_found");
        }

        public async Task<bool> CreateUserAsync(RegisterRequest req)
        {
            var existing = await _userRepository.GetByEmailAsync(req.Email);
            if (existing != null) return false;

            var user = new User
            {
                FullName = req.FullName,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = req.Role
            };
            await _userRepository.InsertAsync(user);
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