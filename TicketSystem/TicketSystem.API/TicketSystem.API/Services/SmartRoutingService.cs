using TicketSystem.API.Models;
using TicketSystem.API.Repositories;

namespace TicketSystem.API.Services;

/// <summary>
/// Picks the least-loaded eligible agent for a ticket category.
/// Agents with <see cref="User.HandledCategories"/> list handle only those categories;
/// agents with an empty list are generalists (eligible for any ticket).
/// </summary>
public class SmartRoutingService
{
    private readonly IUserRepository _userRepository;
    private readonly ITicketRepository _ticketRepository;

    public SmartRoutingService(IUserRepository userRepository, ITicketRepository ticketRepository)
    {
        _userRepository = userRepository;
        _ticketRepository = ticketRepository;
    }


    public async Task<string?> FindBestAgentIdForCategoryAsync(TicketCategory category)
    {
        var agents = await _userRepository.GetAgentsForRoutingAsync();

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
            var openCount = await _ticketRepository.CountOpenOrInProgressByAgentIdAsync(id);
            load[id] = (int)openCount;
        }

        var minLoad = load.Values.DefaultIfEmpty(int.MaxValue).Min();
        var candidates = pool.Where(a => load[a.Id!] == minLoad).OrderBy(a => a.FullName).ToList();
        return candidates.FirstOrDefault()?.Id;
    }

    private static bool IsGeneralist(User a) =>
        a.HandledCategories == null || a.HandledCategories.Count == 0;
}
