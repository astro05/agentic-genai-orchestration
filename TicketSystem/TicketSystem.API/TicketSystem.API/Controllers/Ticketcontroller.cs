using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.API.DTOs;
using TicketSystem.API.Models;
using TicketSystem.API.Services;

namespace TicketSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly TicketService _ticketService;
        private readonly AIService _aiService;
        private readonly KnowledgeBaseService _knowledgeBase;

        public TicketController(
            TicketService ticketService,
            AIService aiService,
            KnowledgeBaseService knowledgeBase)
        {
            _ticketService = ticketService;
            _aiService = aiService;
            _knowledgeBase = knowledgeBase;
        }

        // ── Customer: Create ticket (AI classifies automatically) ──
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest req)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value!;

            var ticket = await _ticketService.CreateAsync(req, userId, userName);

            // AI classification + smart routing (category-based agent assignment)
            _ = Task.Run(async () =>
            {
                try
                {
                    var classification = await _aiService.ClassifyTicketAsync(req.Description);
                    var cat = TicketCategory.UncategorizedIssue;
                    var pri = TicketPriority.Medium;
                    if (Enum.TryParse<TicketCategory>(classification.Category, out var c))
                        cat = c;
                    if (Enum.TryParse<TicketPriority>(classification.Priority, out var p))
                        pri = p;

                    await _ticketService.UpdateAIClassification(ticket.Id!, cat, pri);
                    await _ticketService.TrySmartAssignAsync(ticket.Id!, cat);
                }
                catch
                {
                    /* AI / routing failure is non-critical */
                }
            });

            return Ok(ticket);
        }

        // ── Customer: Get own tickets ────────────────────────────
        [HttpGet("my")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyTickets()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var tickets = await _ticketService.GetByCustomerAsync(userId);
            return Ok(tickets);
        }

        // ── Agent: Get assigned tickets ──────────────────────────
        [HttpGet("assigned")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> GetAssignedTickets()
        {
            var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var tickets = await _ticketService.GetByAgentAsync(agentId);
            return Ok(tickets);
        }

        // ── Agent: Update ticket status ──────────────────────────
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] UpdateTicketStatusRequest req)
        {
            var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var success = await _ticketService.UpdateStatusAsync(id, agentId, req.Status);
            if (!success) return NotFound(new { message = "Ticket not found or not assigned to you." });
            return Ok(new { message = "Status updated." });
        }

        // ── Agent: Reply assist (draft + knowledge sources) ───────
        [HttpGet("{id}/reply-assist")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> GetReplyAssist(string id)
        {
            var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var (ok, err) = await _ticketService.ValidateReplyAssistAsync(id, agentId);
            if (!ok && err == "not_found")
                return NotFound(new { message = "Ticket not found." });
            if (!ok && err == "unassigned")
                return StatusCode(403, new { message = "Ticket is not assigned to an agent yet." });
            if (!ok)
                return StatusCode(403, new { message = "You can only use reply-assist on tickets assigned to you." });

            var ticket = await _ticketService.GetByIdAsync(id);
            if (ticket == null)
                return NotFound(new { message = "Ticket not found." });

            if (!Enum.TryParse<TicketCategory>(ticket.Category, out var category))
                category = TicketCategory.UncategorizedIssue;

            var articles = await _knowledgeBase.GetRelevantArticlesAsync(
                category,
                ticket.Title,
                ticket.Description,
                maxArticles: 5);

            var snippets = articles
                .Where(a => a.Id != null)
                .Select(a => (a.Id!, a.Title, a.Summary))
                .ToList();

            var draft = await _aiService.DraftReplyAssistAsync(
                ticket.Title,
                ticket.Description,
                ticket.Category,
                snippets);

            return Ok(draft);
        }

        // ── Agent: Save ticket notes ──────────────────────────────
        [HttpPut("{id}/notes")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> UpdateNotes(string id, [FromBody] UpdateTicketNotesRequest req)
        {
            var agentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var notes = req.Notes?.Trim() ?? string.Empty;
            if (notes.Length > 8000)
                return BadRequest(new { message = "Notes must be 8000 characters or less." });

            var success = await _ticketService.UpdateAgentNotesAsync(id, agentId, notes);
            if (!success) return NotFound(new { message = "Ticket not found or not assigned to you." });
            return Ok(new { message = "Notes saved." });
        }

        // ── Customer / Agent / Admin: Add conversation message ───
        [HttpPost("{id}/messages")]
        [Authorize(Roles = "Customer,Agent,Admin")]
        public async Task<IActionResult> AddMessage(string id, [FromBody] AddTicketMessageRequest req)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
            var roleStr = User.FindFirst(ClaimTypes.Role)?.Value ?? nameof(UserRole.Customer);
            if (!Enum.TryParse<UserRole>(roleStr, out var role))
                role = UserRole.Customer;

            var (message, error) = await _ticketService.AddMessageAsync(id, userId, userName, role, req);
            return error switch
            {
                "empty_body" => BadRequest(new { message = "Message cannot be empty." }),
                "body_too_long" => BadRequest(new { message = "Message must be 8000 characters or less." }),
                "not_found" => NotFound(new { message = "Ticket not found." }),
                "forbidden" => StatusCode(403, new { message = "You cannot post on this ticket." }),
                "invalid_reply" => BadRequest(new { message = "Reply target was not found on this ticket." }),
                _ => Ok(message)
            };
        }
    }
}