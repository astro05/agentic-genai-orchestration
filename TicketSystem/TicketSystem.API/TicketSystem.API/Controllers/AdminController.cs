using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.API.Services;

namespace TicketSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly UserService _userService;

    public AdminController(UserService userService) => _userService = userService;

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers() =>
        Ok(await _userService.GetAllAsync());

    [HttpPatch("users/{id}/role")]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] string role)
    {
        await _userService.UpdateRoleAsync(id, role);
        return NoContent();
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
}