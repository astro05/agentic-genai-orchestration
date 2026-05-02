using TicketSystem.API.Models;

namespace TicketSystem.API.Repositories;

public interface IKnowledgeArticleRepository
{
    Task<List<KnowledgeArticle>> GetAllAsync();
    Task InsertManyAsync(IEnumerable<KnowledgeArticle> articles);
    Task<long> CountAllAsync();
}
