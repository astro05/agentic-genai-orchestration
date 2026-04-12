using MongoDB.Driver;
using TicketSystem.API.Models;
using TicketSystem.API.Settings;

namespace TicketSystem.API.Services;

/// <summary>
/// Picks the least-loaded eligible agent for a ticket category.
/// Agents with <see cref="User.HandledCategories"/> list handle only those categories;
/// agents with an empty list are generalists (eligible for any ticket).
/// </summary>
public class SmartRoutingService
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<Ticket> _tickets;

    public SmartRoutingService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _users = database.GetCollection<User>(settings.UsersCollection);
        _tickets = database.GetCollection<Ticket>(settings.TicketsCollection);
    }


    public async Task<string?> FindBestAgentIdForCategoryAsync(TicketCategory category)
    {
        var agents = await _users
            .Find(u => u.Role == UserRole.Agent && u.IsActive)
            .ToListAsync();

        if (agents.Count == 0)
            return null;

        var categoryName = category.ToString();
        var specialists = agents
            .Where(a => a.HandledCategories is { Count: > 0 } &&
                        a.HandledCategories.Contains(categoryName))
            .ToList();

        var pool = specialists.Count > 0 ? specialists : agents.Where(IsGeneralist).ToList();
        if (pool.Count == 0)
            pool = agents;

        var load = new Dictionary<string, int>();
        foreach (var agent in pool)
        {
            var id = agent.Id!;
            var openCount = await _tickets.CountDocumentsAsync(t =>
                t.AssignedToId == id &&
                (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress));
            load[id] = (int)openCount;
        }

        var minLoad = load.Values.DefaultIfEmpty(int.MaxValue).Min();
        var candidates = pool.Where(a => load[a.Id!] == minLoad).OrderBy(a => a.FullName).ToList();
        return candidates.FirstOrDefault()?.Id;
    }

    private static bool IsGeneralist(User a) =>
        a.HandledCategories == null || a.HandledCategories.Count == 0;
}
