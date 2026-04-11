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

        public TicketService(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _tickets = database.GetCollection<Ticket>(settings.TicketsCollection);
            _users = database.GetCollection<User>(settings.UsersCollection);
        }

        // ── Customer: Create Ticket ──────────────────────────────
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

        // ── Update ticket category+priority after AI classification
        public async Task UpdateAIClassification(string ticketId, TicketCategory category, TicketPriority priority)
        {
            var update = Builders<Ticket>.Update
                .Set(t => t.Category, category)
                .Set(t => t.Priority, priority)
                .Set(t => t.UpdatedAt, DateTime.UtcNow);
            await _tickets.UpdateOneAsync(t => t.Id == ticketId, update);
        }

        // ── Customer: Get own tickets ────────────────────────────
        public async Task<List<TicketDto>> GetByCustomerAsync(string customerId)
        {
            var tickets = await _tickets.Find(t => t.CreatedById == customerId)
                .SortByDescending(t => t.CreatedAt).ToListAsync();
            return tickets.Select(MapToDto).ToList();
        }

        // ── Agent: Get assigned tickets ──────────────────────────
        public async Task<List<TicketDto>> GetByAgentAsync(string agentId)
        {
            var tickets = await _tickets.Find(t => t.AssignedToId == agentId)
                .SortByDescending(t => t.CreatedAt).ToListAsync();
            return tickets.Select(MapToDto).ToList();
        }

        // ── Agent: Update ticket status ──────────────────────────
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

        // ── Agent: Update internal notes ─────────────────────────
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

        // ── Admin: Get all tickets ───────────────────────────────
        public async Task<List<TicketDto>> GetAllAsync()
        {
            var tickets = await _tickets.Find(_ => true)
                .SortByDescending(t => t.CreatedAt).ToListAsync();
            return tickets.Select(MapToDto).ToList();
        }

        // ── Admin: Assign ticket to agent ────────────────────────
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

        private static TicketDto MapToDto(Ticket t) => new()
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
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        };
    }
}