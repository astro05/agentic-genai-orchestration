using MongoDB.Driver;
using TicketSystem.API.Data;
using TicketSystem.API.Models;

namespace TicketSystem.API.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly IMongoCollection<Ticket> _tickets;

    public TicketRepository(MongoDbContext dbContext)
    {
        _tickets = dbContext.Tickets;
    }

    public Task InsertAsync(Ticket ticket) =>
        _tickets.InsertOneAsync(ticket);

    public Task InsertManyAsync(IEnumerable<Ticket> tickets) =>
        _tickets.InsertManyAsync(tickets);

    public async Task<Ticket?> GetByIdAsync(string ticketId) =>
        await _tickets.Find(t => t.Id == ticketId).FirstOrDefaultAsync();

    public Task<List<Ticket>> GetByCustomerIdAsync(string customerId) =>
        _tickets.Find(t => t.CreatedById == customerId).SortByDescending(t => t.CreatedAt).ToListAsync();

    public Task<List<Ticket>> GetByAgentIdAsync(string agentId) =>
        _tickets.Find(t => t.AssignedToId == agentId).SortByDescending(t => t.CreatedAt).ToListAsync();

    public Task<List<Ticket>> GetAllAsync() =>
        _tickets.Find(_ => true).SortByDescending(t => t.CreatedAt).ToListAsync();

    public Task<long> CountOpenOrInProgressByAgentIdAsync(string agentId) =>
        _tickets.CountDocumentsAsync(t =>
            t.AssignedToId == agentId &&
            (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress));

    public async Task<bool> UpdateClassificationAsync(string ticketId, TicketCategory category, TicketPriority priority)
    {
        var update = Builders<Ticket>.Update
            .Set(t => t.Category, category)
            .Set(t => t.Priority, priority)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);
        var result = await _tickets.UpdateOneAsync(t => t.Id == ticketId, update);
        return result.MatchedCount > 0;
    }

    public async Task<bool> TryAssignAsync(string ticketId, string agentId, string agentName, bool onlyIfUnassigned)
    {
        var filter = onlyIfUnassigned
            ? Builders<Ticket>.Filter.Where(t => t.Id == ticketId && string.IsNullOrEmpty(t.AssignedToId))
            : Builders<Ticket>.Filter.Where(t => t.Id == ticketId);

        var update = Builders<Ticket>.Update
            .Set(t => t.AssignedToId, agentId)
            .Set(t => t.AssignedToName, agentName)
            .Set(t => t.Status, TicketStatus.InProgress)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);

        var result = await _tickets.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0 || (!onlyIfUnassigned && result.MatchedCount > 0);
    }

    public async Task<bool> UpdateStatusAsync(string ticketId, TicketStatus status, bool setResolvedAt)
    {
        var update = Builders<Ticket>.Update
            .Set(t => t.Status, status)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);

        if (setResolvedAt)
            update = update.Set(t => t.ResolvedAt, DateTime.UtcNow);

        var result = await _tickets.UpdateOneAsync(t => t.Id == ticketId, update);
        return result.MatchedCount > 0;
    }

    public async Task<bool> UpdateNotesAsync(string ticketId, string notes)
    {
        var update = Builders<Ticket>.Update
            .Set(t => t.AgentNotes, notes)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);
        var result = await _tickets.UpdateOneAsync(t => t.Id == ticketId, update);
        return result.MatchedCount > 0;
    }

    public async Task<bool> AddMessageAsync(string ticketId, TicketMessage message)
    {
        var update = Builders<Ticket>.Update
            .Push(t => t.Messages, message)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);
        var result = await _tickets.UpdateOneAsync(t => t.Id == ticketId, update);
        return result.MatchedCount > 0;
    }

    public Task<long> CountAllAsync() =>
        _tickets.CountDocumentsAsync(_ => true);
}
