using Microsoft.AspNetCore.Mvc;
using TicketSystem.API.DTOs;
using TicketSystem.API.Services;

namespace TicketSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { message = "Email and password are required." });

            var result = await _authService.RegisterAsync(req);
            if (result == null)
                return Conflict(new { message = "Email already registered." });

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var result = await _authService.LoginAsync(req);
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password." });

            return Ok(result);
        }
    }
}