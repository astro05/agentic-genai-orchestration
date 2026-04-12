using MongoDB.Bson;
using MongoDB.Driver;
using TicketSystem.API.DTOs;
using TicketSystem.API.Models;
using TicketSystem.API.Settings;

namespace TicketSystem.API.Services
{
    public class TicketService
    {
        private readonly IMongoCollection<Ticket> _tickets;
        private readonly IMongoCollection<User> _users;
        private readonly SmartRoutingService _smartRouting;

        public TicketService(MongoDbSettings settings, SmartRoutingService smartRouting)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _tickets = database.GetCollection<Ticket>(settings.TicketsCollection);
            _users = database.GetCollection<User>(settings.UsersCollection);
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
            await _tickets.InsertOneAsync(ticket);
            return MapToDto(ticket);
        }

        
        public async Task UpdateAIClassification(string ticketId, TicketCategory category, TicketPriority priority)
        {
            var update = Builders<Ticket>.Update
                .Set(t => t.Category, category)
                .Set(t => t.Priority, priority)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);
            await _tickets.UpdateOneAsync(t => t.Id == ticketId, update);
        }

        /// <summary>
        /// Assigns the best-matching agent by category load (smart routing). No-op if already assigned.
        /// </summary>
        public async Task<bool> TrySmartAssignAsync(string ticketId, TicketCategory category)
        {
            var ticket = await _tickets.Find(t => t.Id == ticketId).FirstOrDefaultAsync();
            if (ticket == null || !string.IsNullOrEmpty(ticket.AssignedToId))
                return false;

            var agentId = await _smartRouting.FindBestAgentIdForCategoryAsync(category);
            if (string.IsNullOrEmpty(agentId))
                return false;

            var agent = await _users.Find(u => u.Id == agentId && u.Role == UserRole.Agent && u.IsActive)
                .FirstOrDefaultAsync();
            if (agent == null)
                return false;

            var update = Builders<Ticket>.Update
                .Set(t => t.AssignedToId, agentId)
                .Set(t => t.AssignedToName, agent.FullName)
                .Set(t => t.Status, TicketStatus.InProgress)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            var result = await _tickets.UpdateOneAsync(
                t => t.Id == ticketId && string.IsNullOrEmpty(t.AssignedToId),
                update);
            return result.ModifiedCount > 0;
        }

        public async Task<TicketDto?> GetByIdAsync(string ticketId)
        {
            var ticket = await _tickets.Find(t => t.Id == ticketId).FirstOrDefaultAsync();
            return ticket == null ? null : MapToDto(ticket);
        }

        /// <summary>
        /// Reply-assist is only for the agent currently assigned to the ticket.
        /// </summary>
        public async Task<(bool Ok, string Error)> ValidateReplyAssistAsync(string ticketId, string agentId)
        {
            var t = await _tickets.Find(x => x.Id == ticketId).FirstOrDefaultAsync();
            if (t == null) return (false, "not_found");
            if (string.IsNullOrEmpty(t.AssignedToId)) return (false, "unassigned");
            if (t.AssignedToId != agentId) return (false, "forbidden");
            return (true, string.Empty);
        }

        
        public async Task<List<TicketDto>> GetByCustomerAsync(string customerId)
        {
            var tickets = await _tickets.Find(t => t.CreatedById == customerId)
                .SortByDescending(t => t.CreatedAt).ToListAsync();
            return tickets.Select(MapToDto).ToList();
        }

       
        public async Task<List<TicketDto>> GetByAgentAsync(string agentId)
        {
            var tickets = await _tickets.Find(t => t.AssignedToId == agentId)
                .SortByDescending(t => t.CreatedAt).ToListAsync();
            return tickets.Select(MapToDto).ToList();
        }

      
        public async Task<bool> UpdateStatusAsync(string ticketId, string agentId, TicketStatus status)
        {
            var ticket = await _tickets.Find(t => t.Id == ticketId && t.AssignedToId == agentId).FirstOrDefaultAsync();
            if (ticket == null) return false;

            var updateDef = Builders<Ticket>.Update
                .Set(t => t.Status, status)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            if (status == TicketStatus.Resolved)
                updateDef = updateDef.Set(t => t.ResolvedAt, DateTime.UtcNow);

            await _tickets.UpdateOneAsync(t => t.Id == ticketId, updateDef);
            return true;
        }

       
        public async Task<bool> UpdateAgentNotesAsync(string ticketId, string agentId, string notes)
        {
            var ticket = await _tickets.Find(t => t.Id == ticketId && t.AssignedToId == agentId).FirstOrDefaultAsync();
            if (ticket == null) return false;

            var update = Builders<Ticket>.Update
                .Set(t => t.AgentNotes, notes)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);
            await _tickets.UpdateOneAsync(t => t.Id == ticketId, update);
            return true;
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

            var ticket = await _tickets.Find(t => t.Id == ticketId).FirstOrDefaultAsync();
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

            var update = Builders<Ticket>.Update
                .Push(t => t.Messages, msg)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            await _tickets.UpdateOneAsync(t => t.Id == ticketId, update);
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
            var tickets = await _tickets.Find(_ => true)
                .SortByDescending(t => t.CreatedAt).ToListAsync();
            return tickets.Select(MapToDto).ToList();
        }

     
        public async Task<bool> AssignAsync(string ticketId, string agentId)
        {
            var agent = await _users.Find(u => u.Id == agentId && u.Role == UserRole.Agent).FirstOrDefaultAsync();
            if (agent == null) return false;

            var update = Builders<Ticket>.Update
                .Set(t => t.AssignedToId, agentId)
                .Set(t => t.AssignedToName, agent.FullName)
                .Set(t => t.Status, TicketStatus.InProgress)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);

            var result = await _tickets.UpdateOneAsync(t => t.Id == ticketId, update);
            return result.MatchedCount > 0;
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