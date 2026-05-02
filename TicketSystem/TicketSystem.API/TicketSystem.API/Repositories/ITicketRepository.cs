using TicketSystem.API.Models;

namespace TicketSystem.API.Repositories;

public interface ITicketRepository
{
    Task InsertAsync(Ticket ticket);
    Task InsertManyAsync(IEnumerable<Ticket> tickets);
    Task<Ticket?> GetByIdAsync(string ticketId);
    Task<List<Ticket>> GetByCustomerIdAsync(string customerId);
    Task<List<Ticket>> GetByAgentIdAsync(string agentId);
    Task<List<Ticket>> GetAllAsync();
    Task<long> CountOpenOrInProgressByAgentIdAsync(string agentId);
    Task<bool> UpdateClassificationAsync(string ticketId, TicketCategory category, TicketPriority priority);
    Task<bool> TryAssignAsync(string ticketId, string agentId, string agentName, bool onlyIfUnassigned);
    Task<bool> UpdateStatusAsync(string ticketId, TicketStatus status, bool setResolvedAt);
    Task<bool> UpdateNotesAsync(string ticketId, string notes);
    Task<bool> AddMessageAsync(string ticketId, TicketMessage message);
    Task<long> CountAllAsync();
}
