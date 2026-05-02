using MongoDB.Bson;
using TicketSystem.API.DTOs;
using TicketSystem.API.Models;
using TicketSystem.API.Repositories;

namespace TicketSystem.API.Services
{
    public class TicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserRepository _userRepository;
        private readonly SmartRoutingService _smartRouting;

        public TicketService(
            ITicketRepository ticketRepository,
            IUserRepository userRepository,
            SmartRoutingService smartRouting)
        {
            _ticketRepository = ticketRepository;
            _userRepository = userRepository;
            _smartRouting = smartRouting;
        }

     
        public async Task<TicketDto> CreateAsync(CreateTicketRequest req, string customerId, string customerName)
        {
            var ticket = new Ticket
            {
                Title = req.Title,
                Description = req.Description,
                CreatedById = customerId,
                CreatedByName = customerName
            };
            await _ticketRepository.InsertAsync(ticket);
            return MapToDto(ticket);
        }

        
        public async Task UpdateAIClassification(string ticketId, TicketCategory category, TicketPriority priority)
        {
            await _ticketRepository.UpdateClassificationAsync(ticketId, category, priority);
        }

        /// <summary>
        /// Assigns the best-matching agent by category load (smart routing). No-op if already assigned.
        /// </summary>
        public async Task<bool> TrySmartAssignAsync(string ticketId, TicketCategory category)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null || !string.IsNullOrEmpty(ticket.AssignedToId))
                return false;

            var agentId = await _smartRouting.FindBestAgentIdForCategoryAsync(category);
            if (string.IsNullOrEmpty(agentId))
                return false;

            var agent = await _userRepository.GetByIdAsync(agentId);
            if (agent is null || agent.Role != UserRole.Agent || !agent.IsActive)
                return false;
            return await _ticketRepository.TryAssignAsync(ticketId, agentId, agent.FullName, onlyIfUnassigned: true);
        }

        public async Task<TicketDto?> GetByIdAsync(string ticketId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            return ticket == null ? null : MapToDto(ticket);
        }

        /// <summary>
        /// Reply-assist is only for the agent currently assigned to the ticket.
        /// </summary>
        public async Task<(bool Ok, string Error)> ValidateReplyAssistAsync(string ticketId, string agentId)
        {
            var t = await _ticketRepository.GetByIdAsync(ticketId);
            if (t == null) return (false, "not_found");
            if (string.IsNullOrEmpty(t.AssignedToId)) return (false, "unassigned");
            if (t.AssignedToId != agentId) return (false, "forbidden");
            return (true, string.Empty);
        }

        
        public async Task<List<TicketDto>> GetByCustomerAsync(string customerId)
        {
            var tickets = await _ticketRepository.GetByCustomerIdAsync(customerId);
            return tickets.Select(MapToDto).ToList();
        }

       
        public async Task<List<TicketDto>> GetByAgentAsync(string agentId)
        {
            var tickets = await _ticketRepository.GetByAgentIdAsync(agentId);
            return tickets.Select(MapToDto).ToList();
        }

      
        public async Task<bool> UpdateStatusAsync(string ticketId, string agentId, TicketStatus status)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null || ticket.AssignedToId != agentId) return false;
            return await _ticketRepository.UpdateStatusAsync(
                ticketId,
                status,
                setResolvedAt: status == TicketStatus.Resolved);
        }

       
        public async Task<bool> UpdateAgentNotesAsync(string ticketId, string agentId, string notes)
        {
            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null || ticket.AssignedToId != agentId) return false;
            return await _ticketRepository.UpdateNotesAsync(ticketId, notes);
        }

        
        public async Task<(TicketMessageDto? message, string? error)> AddMessageAsync(
            string ticketId,
            string userId,
            string userName,
            UserRole role,
            AddTicketMessageRequest req)
        {
            var body = (req.Body ?? string.Empty).Trim();
            if (body.Length == 0)
                return (null, "empty_body");
            if (body.Length > 8000)
                return (null, "body_too_long");

            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
                return (null, "not_found");

            if (!CanPostMessage(ticket, userId, role))
                return (null, "forbidden");

            var messages = ticket.Messages ?? new List<TicketMessage>();
            if (!string.IsNullOrWhiteSpace(req.ReplyToMessageId))
            {
                var parentExists = messages.Any(m => m.Id == req.ReplyToMessageId);
                if (!parentExists)
                    return (null, "invalid_reply");
            }

            var msg = new TicketMessage
            {
                Id = ObjectId.GenerateNewId().ToString(),
                AuthorRole = role,
                AuthorUserId = userId,
                AuthorName = userName,
                Body = body,
                CreatedAt = DateTime.UtcNow,
                ReplyToMessageId = string.IsNullOrWhiteSpace(req.ReplyToMessageId)
                    ? null
                    : req.ReplyToMessageId
            };

            await _ticketRepository.AddMessageAsync(ticketId, msg);
            return (MapMessageToDto(msg), null);
        }

        private static bool CanPostMessage(Ticket ticket, string userId, UserRole role)
        {
            if (role == UserRole.Admin)
                return true;
            if (role == UserRole.Customer && ticket.CreatedById == userId)
                return true;
            if (role == UserRole.Agent && ticket.AssignedToId == userId)
                return true;
            return false;
        }

        
        public async Task<List<TicketDto>> GetAllAsync()
        {
            var tickets = await _ticketRepository.GetAllAsync();
            return tickets.Select(MapToDto).ToList();
        }

     
        public async Task<bool> AssignAsync(string ticketId, string agentId)
        {
            var agent = await _userRepository.GetByIdAsync(agentId);
            if (agent is null || agent.Role != UserRole.Agent || !agent.IsActive) return false;
            return await _ticketRepository.TryAssignAsync(ticketId, agentId, agent.FullName, onlyIfUnassigned: false);
        }

        private static TicketDto MapToDto(Ticket t)
        {
            var list = (t.Messages ?? new List<TicketMessage>())
                .OrderBy(m => m.CreatedAt)
                .Select(MapMessageToDto)
                .ToList();

            return new TicketDto
            {
                Id = t.Id!,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                Category = t.Category.ToString(),
                CreatedByName = t.CreatedByName,
                AssignedToName = t.AssignedToName,
                AgentNotes = t.AgentNotes ?? string.Empty,
                Messages = list,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            };
        }

        private static TicketMessageDto MapMessageToDto(TicketMessage m) => new()
        {
            Id = m.Id,
            AuthorRole = m.AuthorRole.ToString(),
            AuthorName = m.AuthorName,
            Body = m.Body,
            CreatedAt = m.CreatedAt,
            ReplyToMessageId = m.ReplyToMessageId
        };
    }
}