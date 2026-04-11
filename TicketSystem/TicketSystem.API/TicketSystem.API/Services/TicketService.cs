using MongoDB.Driver;
using TicketSystem.API.DTOs;
using TicketSystem.API.Models;

namespace TicketSystem.API.Services;

public class TicketService
{
    private readonly IMongoCollection<Ticket> _tickets;
    private readonly AiService _aiService;

    public TicketService(IConfiguration config, AiService aiService)
    {
        var client = new MongoClient(config["MongoDB:ConnectionString"]);
        var db = client.GetDatabase(config["MongoDB:DatabaseName"]);
        _tickets = db.GetCollection<Ticket>("tickets");
        _aiService = aiService;
    }

    public async Task<Ticket> CreateAsync(CreateTicketDto dto, string userId)
    {
        var (category, priority) = await _aiService.ClassifyTicketAsync(dto.Description);

        var ticket = new Ticket
        {
            Title = dto.Title,
            Description = dto.Description,
            CreatedBy = userId,
            Category = category,
            Priority = priority
        };

        await _tickets.InsertOneAsync(ticket);
        return ticket;
    }

    public async Task<List<Ticket>> GetAllAsync() =>
        await _tickets.Find(_ => true).ToListAsync();

    public async Task<List<Ticket>> GetByUserAsync(string userId) =>
        await _tickets.Find(t => t.CreatedBy == userId).ToListAsync();

    public async Task<Ticket?> GetByIdAsync(string id) =>
        await _tickets.Find(t => t.Id == id).FirstOrDefaultAsync();

    public async Task UpdateStatusAsync(string id, string status)
    {
        var update = Builders<Ticket>.Update
            .Set(t => t.Status, status)
            .Set(t => t.UpdatedAt, DateTime.UtcNow);
        await _tickets.UpdateOneAsync(t => t.Id == id, update);
    }

    public async Task AssignAsync(string id, string agentId)
    {
        var update = Builders<Ticket>.Update
            .Set(t => t.AssignedTo, agentId)
            .Set(t => t.Status, "In Progress")
            .Set(t => t.UpdatedAt, DateTime.UtcNow);
        await _tickets.UpdateOneAsync(t => t.Id == id, update);
    }

    public async Task DeleteAsync(string id) =>
        await _tickets.DeleteOneAsync(t => t.Id == id);
}