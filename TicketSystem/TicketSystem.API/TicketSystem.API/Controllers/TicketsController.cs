using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.API.DTOs;
using TicketSystem.API.Services;

namespace TicketSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly TicketService _ticketService;

    public TicketsController(TicketService ticketService) => _ticketService = ticketService;

    [HttpPost]
    public async Task<IActionResult> Create(CreateTicketDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var ticket = await _ticketService.CreateAsync(dto, userId);
        return Ok(ticket);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> GetAll() =>
        Ok(await _ticketService.GetAllAsync());

    [HttpGet("my")]
    public async Task<IActionResult> GetMine()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _ticketService.GetByUserAsync(userId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var ticket = await _ticketService.GetByIdAsync(id);
        return ticket == null ? NotFound() : Ok(ticket);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] string status)
    {
        await _ticketService.UpdateStatusAsync(id, status);
        return NoContent();
    }

    [HttpPatch("{id}/assign")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Assign(string id, [FromBody] string agentId)
    {
        await _ticketService.AssignAsync(id, agentId);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        await _ticketService.DeleteAsync(id);
        return NoContent();
    }
}