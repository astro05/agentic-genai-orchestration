using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.API.DTOs;
using TicketSystem.API.Services;

namespace TicketSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        private readonly TicketService _ticketService;

        public AdminController(AdminService adminService, TicketService ticketService)
        {
            _adminService = adminService;
            _ticketService = ticketService;
        }

        // ── Get all users ────────────────────────────────────────
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers() =>
            Ok(await _adminService.GetAllUsersAsync());

        // ── Get all agents ───────────────────────────────────────
        [HttpGet("agents")]
        public async Task<IActionResult> GetAgents() =>
            Ok(await _adminService.GetAgentsAsync());

        // ── Create user ──────────────────────────────────────────
        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterRequest req)
        {
            var success = await _adminService.CreateUserAsync(req);
            if (!success) return Conflict(new { message = "Email already exists." });
            return Ok(new { message = "User created." });
        }

        // ── Update user role ─────────────────────────────────────
        [HttpPut("users/role")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleRequest req)
        {
            var success = await _adminService.UpdateRoleAsync(req.UserId, req.NewRole);
            if (!success) return NotFound(new { message = "User not found." });
            return Ok(new { message = "Role updated." });
        }

        // ── Delete (deactivate) user ─────────────────────────────
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var success = await _adminService.DeleteUserAsync(id);
            if (!success) return NotFound(new { message = "User not found." });
            return Ok(new { message = "User deactivated." });
        }

        // ── Get all tickets ──────────────────────────────────────
        [HttpGet("tickets")]
        public async Task<IActionResult> GetAllTickets() =>
            Ok(await _ticketService.GetAllAsync());

        // ── Assign ticket to agent ───────────────────────────────
        [HttpPut("tickets/{ticketId}/assign")]
        public async Task<IActionResult> AssignTicket(string ticketId, [FromBody] AssignTicketRequest req)
        {
            var success = await _ticketService.AssignAsync(ticketId, req.AgentId);
            if (!success) return BadRequest(new { message = "Ticket or agent not found." });
            return Ok(new { message = "Ticket assigned." });
        }
    }
}